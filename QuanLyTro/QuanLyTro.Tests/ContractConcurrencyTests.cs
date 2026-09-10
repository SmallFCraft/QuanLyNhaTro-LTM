using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using QuanLyTro.Server.Data;
using QuanLyTro.Server.Repositories;
using QuanLyTro.Server.Services;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class ContractConcurrencyTests
{
    private const string ConnectionString =
        "Server=127.0.0.1;Port=3306;Database=quanly_phongtro_nhs;User ID=root;Password=;SslMode=None;";

    private const int TestRoomId = 9188;
    private const int TestTenantId = 9188;
    private const string TestRoomNumber = "TEST-RACE-9188";
    private const string TestCccd = "999999999188";

    private Database _database = null!;
    private ContractService _service = null!;

    [TestInitialize]
    public async Task SetUpAsync()
    {
        _database = new Database(ConnectionString);
        var repo = new ContractRepository(_database);
        _service = new ContractService(repo);

        await CleanupAsync();
        await SeedRoomAndTenantAsync();
    }

    [TestCleanup]
    public async Task TearDownAsync()
    {
        await CleanupAsync();
    }

    [TestMethod]
    public async Task CreateAsync_ConcurrentRequests_OnlyOneActiveContractCreated()
    {
        const int concurrency = 10;
        var startSignal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var tasks = Enumerable.Range(0, concurrency).Select(async _ =>
        {
            await startSignal.Task;
            try
            {
                var contract = new ContractDto(
                    0,
                    TestRoomId,
                    TestTenantId,
                    new DateOnly(2026, 1, 1),
                    new DateOnly(2026, 12, 31),
                    3_000_000m,
                    1_500_000m,
                    ContractStatus.Active,
                    "Concurrent race test");

                var created = await _service.CreateAsync(contract);
                return (Success: true, Ex: (Exception?)null);
            }
            catch (Exception ex)
            {
                return (Success: false, Ex: ex);
            }
        }).ToList();

        // Thả đồng thời tất cả các task
        startSignal.SetResult();
        var results = await Task.WhenAll(tasks);

        var successCount = results.Count(r => r.Success);
        var ruleFailures = results.Count(r => !r.Success && r.Ex is BusinessRuleException);

        Assert.AreEqual(1, successCount, $"Chỉ đúng 1 hợp đồng được phép thành công, thực tế: {successCount}");
        Assert.AreEqual(concurrency - 1, ruleFailures,
            $"Tất cả {concurrency - 1} request còn lại phải bị chặn bởi BusinessRuleException");

        // DB authoritative count check
        await using var connection = await _database.OpenAsync();
        await using var command = new MySqlCommand(
            "SELECT COUNT(*) FROM contracts WHERE room_id = @roomId AND status = 'Active'",
            connection);
        command.Parameters.AddWithValue("@roomId", TestRoomId);
        var activeInDb = Convert.ToInt64(await command.ExecuteScalarAsync());

        Assert.AreEqual(1L, activeInDb, "DB chỉ được có đúng 1 hợp đồng Active cho phòng này.");
    }

    private async Task SeedRoomAndTenantAsync()
    {
        await using var connection = await _database.OpenAsync();

        const string insertRoom = """
            INSERT INTO rooms (id, room_number, price, max_occupants, status, description)
            VALUES (@id, @roomNumber, 3000000, 2, 'Available', 'Test room');
            """;
        await using (var cmd = new MySqlCommand(insertRoom, connection))
        {
            cmd.Parameters.AddWithValue("@id", TestRoomId);
            cmd.Parameters.AddWithValue("@roomNumber", TestRoomNumber);
            await cmd.ExecuteNonQueryAsync();
        }

        const string insertTenant = """
            INSERT INTO tenants (id, room_id, full_name, dob, id_card, password_hash, phone, hometown, workplace, is_temporary_registered)
            VALUES (@id, @roomId, 'Test Representative', '1995-01-01', @cccd, NULL, '0987654321', 'Da Nang', NULL, 1);
            """;
        await using (var cmd = new MySqlCommand(insertTenant, connection))
        {
            cmd.Parameters.AddWithValue("@id", TestTenantId);
            cmd.Parameters.AddWithValue("@roomId", TestRoomId);
            cmd.Parameters.AddWithValue("@cccd", TestCccd);
            await cmd.ExecuteNonQueryAsync();
        }
    }

    private async Task CleanupAsync()
    {
        try
        {
            await using var connection = await _database.OpenAsync();

            await using (var cmd = new MySqlCommand("DELETE FROM contracts WHERE room_id = @id", connection))
            {
                cmd.Parameters.AddWithValue("@id", TestRoomId);
                await cmd.ExecuteNonQueryAsync();
            }

            await using (var cmd = new MySqlCommand("DELETE FROM tenants WHERE id = @id", connection))
            {
                cmd.Parameters.AddWithValue("@id", TestTenantId);
                await cmd.ExecuteNonQueryAsync();
            }

            await using (var cmd = new MySqlCommand("DELETE FROM rooms WHERE id = @id", connection))
            {
                cmd.Parameters.AddWithValue("@id", TestRoomId);
                await cmd.ExecuteNonQueryAsync();
            }
        }
        catch
        {
            // Bỏ qua lỗi kết nối nếu DB chưa sẵn sàng ở giai đoạn cleanup đầu
        }
    }
}
