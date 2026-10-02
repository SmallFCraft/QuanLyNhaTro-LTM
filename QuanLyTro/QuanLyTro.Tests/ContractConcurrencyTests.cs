using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using QuanLyTro.Server.Data;
using QuanLyTro.Server.Repositories;
using QuanLyTro.Server.Services;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class HopDongConcurrencyTests
{
    private const string ConnectionString =
        "Server=127.0.0.1;Port=3306;Database=quanly_phongtro_nhs;User ID=root;Password=;SslMode=None;";

    private const int TestRoomId = 9188;
    private const int TestTenantId = 9188;
    private const string TestRoomNumber = "TEST-RACE-9188";
    private const string TestCccd = "999999999188";

    private Database _database = null!;
    private HopDongService _service = null!;

    [TestInitialize]
    public async Task SetUpAsync()
    {
        _database = new Database(ConnectionString);
        var repo = new HopDongRepository(_database);
        _service = new HopDongService(repo);

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
                var contract = new HopDongDto(
                    0,
                    TestRoomId,
                    TestTenantId,
                    new DateOnly(2026, 1, 1),
                    new DateOnly(2026, 12, 31),
                    3_000_000m,
                    1_500_000m,
                    TrangThaiHopDong.HieuLuc,
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
        var ruleFailures = results.Count(r => !r.Success && r.Ex is LoiNghiepVu);

        Assert.AreEqual(1, successCount, $"Chỉ đúng 1 hợp đồng được phép thành công, thực tế: {successCount}");
        Assert.AreEqual(concurrency - 1, ruleFailures,
            $"Tất cả {concurrency - 1} request còn lại phải bị chặn bởi LoiNghiepVu");

        // DB authoritative count check
        await using var connection = await _database.OpenAsync();
        await using var command = new MySqlCommand(
            "SELECT COUNT(*) FROM hop_dong WHERE phong_id = @phongId AND trang_thai = 'HieuLuc'",
            connection);
        command.Parameters.AddWithValue("@phongId", TestRoomId);
        var activeInDb = Convert.ToInt64(await command.ExecuteScalarAsync());

        Assert.AreEqual(1L, activeInDb, "DB chỉ được có đúng 1 hợp đồng HieuLuc cho phòng này.");
    }

    private async Task SeedRoomAndTenantAsync()
    {
        await using var connection = await _database.OpenAsync();

        const string insertRoom = """
            INSERT INTO phong (id, so_phong, gia_thue, so_nguoi_toi_da, trang_thai, mo_ta)
            VALUES (@id, @soPhong, 3000000, 2, 'Trong', 'Test room');
            """;
        await using (var cmd = new MySqlCommand(insertRoom, connection))
        {
            cmd.Parameters.AddWithValue("@id", TestRoomId);
            cmd.Parameters.AddWithValue("@soPhong", TestRoomNumber);
            await cmd.ExecuteNonQueryAsync();
        }

        const string insertTenant = """
            INSERT INTO khach_thue (id, phong_id, ho_ten, ngay_sinh, cccd, mat_khau_hash, so_dien_thoai, que_quan, noi_lam_viec, da_dang_ky_tam_tru)
            VALUES (@id, @phongId, 'Test Representative', '1995-01-01', @cccd, NULL, '0987654321', 'Da Nang', NULL, 1);
            """;
        await using (var cmd = new MySqlCommand(insertTenant, connection))
        {
            cmd.Parameters.AddWithValue("@id", TestTenantId);
            cmd.Parameters.AddWithValue("@phongId", TestRoomId);
            cmd.Parameters.AddWithValue("@cccd", TestCccd);
            await cmd.ExecuteNonQueryAsync();
        }
    }

    private async Task CleanupAsync()
    {
        try
        {
            await using var connection = await _database.OpenAsync();

            await using (var cmd = new MySqlCommand("DELETE FROM hop_dong WHERE phong_id = @id", connection))
            {
                cmd.Parameters.AddWithValue("@id", TestRoomId);
                await cmd.ExecuteNonQueryAsync();
            }

            await using (var cmd = new MySqlCommand("DELETE FROM khach_thue WHERE id = @id", connection))
            {
                cmd.Parameters.AddWithValue("@id", TestTenantId);
                await cmd.ExecuteNonQueryAsync();
            }

            await using (var cmd = new MySqlCommand("DELETE FROM phong WHERE id = @id", connection))
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
