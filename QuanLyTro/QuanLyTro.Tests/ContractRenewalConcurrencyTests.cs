using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using QuanLyTro.Server.Data;
using QuanLyTro.Server.Repositories;
using QuanLyTro.Server.Services;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Tests;

/// <summary>
/// US-10 / BR-06: gia hạn hợp đồng chỉ được đẩy end_date về TRƯỚC, không bao giờ lùi lại.
/// Regression cho race: client B đọc end_date cũ rồi ghi đè end_date mới hơn của client A.
/// </summary>
[TestClass]
public sealed class ContractRenewalConcurrencyTests
{
    private const string ConnectionString =
        "Server=127.0.0.1;Port=3306;Database=quanly_phongtro_nhs;User ID=root;Password=;SslMode=None;";

    private const int TestContractId = 9189;
    private const int TestRoomId = 9189;
    private const int TestTenantId = 9189;
    private const string TestRoomNumber = "TEST-RENEW-9189";
    private const string TestCccd = "999999999189";

    private static readonly DateOnly CurrentEnd = new(2026, 12, 31);
    private static readonly DateOnly ForwardEnd = new(2027, 12, 31);
    private static readonly DateOnly BackwardEnd = new(2027, 6, 30);

    private Database _database = null!;
    private ContractService _service = null!;

    [TestInitialize]
    public async Task SetUpAsync()
    {
        _database = new Database(ConnectionString);
        _service = new ContractService(new ContractRepository(_database));

        await CleanupAsync();
        await SeedContractAsync();
    }

    [TestCleanup]
    public async Task TearDownAsync()
    {
        await CleanupAsync();
    }

    [TestMethod]
    public async Task UpdateEndDateAsync_BackwardRenewal_ReturnsFalseAndKeepsForwardEndDate()
    {
        var repo = new ContractRepository(_database);

        // Client A gia hạn trước: end_date 2026-12-31 -> 2027-12-31.
        Assert.IsTrue(await repo.UpdateEndDateAsync(TestContractId, ForwardEnd));

        // Client B (dữ liệu cũ) gia hạn về 2027-06-30 — phải bị DB chặn.
        Assert.IsFalse(await repo.UpdateEndDateAsync(TestContractId, BackwardEnd));

        var inDb = await GetEndDateFromDbAsync();
        Assert.AreEqual(ForwardEnd, inDb, "end_date không được bị ghi lùi về sau.");
    }

    [TestMethod]
    public async Task RenewAsync_ExtendsForward_SucceedsAndPersists()
    {
        Assert.IsTrue(await _service.RenewAsync(TestContractId, ForwardEnd));
        Assert.AreEqual(ForwardEnd, await GetEndDateFromDbAsync());
    }

    [TestMethod]
    public async Task RenewAsync_StaleEndDateAfterRace_ThrowsBusinessRuleNotFalseSuccess()
    {
        // Mô phỏng race: client B vẫn thấy end_date cũ (2026-12-31, hợp lệ theo validate),
        // nhưng DB đã có end_date mới hơn nên UPDATE điều kiện trả về 0 dòng.
        var repo = new StaleRepo
        {
            Existing = new ContractDto(
                TestContractId, TestRoomId, TestTenantId,
                new DateOnly(2026, 1, 1), CurrentEnd,
                3_000_000m, 1_500_000m, ContractStatus.Active, null),
            UpdateSucceeds = false,
        };
        var service = new ContractService(repo);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => service.RenewAsync(TestContractId, BackwardEnd));

        StringAssert.Contains(ex.Message, "Ngày gia hạn phải sau ngày kết thúc hiện tại.");
    }

    private async Task SeedContractAsync()
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

        const string insertContract = """
            INSERT INTO contracts (id, room_id, representative_tenant_id, start_date, end_date,
                                   rental_price, deposit_amount, status, notes)
            VALUES (@id, @roomId, @tenantId, @startDate, @endDate, 3000000, 1500000, 'Active', 'Renewal race test');
            """;
        await using (var cmd = new MySqlCommand(insertContract, connection))
        {
            cmd.Parameters.AddWithValue("@id", TestContractId);
            cmd.Parameters.AddWithValue("@roomId", TestRoomId);
            cmd.Parameters.AddWithValue("@tenantId", TestTenantId);
            cmd.Parameters.AddWithValue("@startDate", new DateOnly(2026, 1, 1));
            cmd.Parameters.AddWithValue("@endDate", CurrentEnd);
            await cmd.ExecuteNonQueryAsync();
        }
    }

    private async Task<DateOnly> GetEndDateFromDbAsync()
    {
        await using var connection = await _database.OpenAsync();
        await using var command = new MySqlCommand(
            "SELECT end_date FROM contracts WHERE id = @id", connection);
        command.Parameters.AddWithValue("@id", TestContractId);
        var scalar = await command.ExecuteScalarAsync();
        return scalar switch
        {
            DateOnly d => d,
            DateTime dt => DateOnly.FromDateTime(dt),
            _ => default,
        };
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

    /// <summary>Stub: GetById trả về bản stale (end_date cũ), Update mô phỏng DB chặn (0 dòng).</summary>
    private sealed class StaleRepo : IContractRepository
    {
        public required ContractDto Existing { get; init; }
        public required bool UpdateSucceeds { get; init; }

        public Task<ContractDto?> GetByIdAsync(int contractId, CancellationToken ct = default) =>
            Task.FromResult<ContractDto?>(Existing);

        public Task<bool> HasActiveContractAsync(int roomId, CancellationToken ct = default) => Task.FromResult(false);
        public Task<bool> IsTenantInRoomAsync(int tenantId, int roomId, CancellationToken ct = default) => Task.FromResult(true);
        public Task<ContractDto> AddAsync(ContractDto contract, CancellationToken ct = default) => Task.FromResult(contract);
        public Task<bool> TerminateAsync(int contractId, string? notes, CancellationToken ct = default) => Task.FromResult(true);
        public Task<bool> UpdateEndDateAsync(int contractId, DateOnly newEndDate, CancellationToken ct = default) =>
            Task.FromResult(UpdateSucceeds);
        public Task<List<ContractListItem>> GetAllAsync(CancellationToken ct = default) => Task.FromResult(new List<ContractListItem>());
    }
}
