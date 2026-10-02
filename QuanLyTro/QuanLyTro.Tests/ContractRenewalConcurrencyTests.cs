using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using QuanLyTro.Server.Data;
using QuanLyTro.Server.Repositories;
using QuanLyTro.Server.Services;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Tests;

/// <summary>
/// US-10 / BR-06: gia hạn hợp đồng chỉ được đẩy ngay_ket_thuc về TRƯỚC, không bao giờ lùi lại.
/// Regression cho race: client B đọc ngay_ket_thuc cũ rồi ghi đè ngay_ket_thuc mới hơn của client A.
/// </summary>
[TestClass]
public sealed class HopDongRenewalConcurrencyTests
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
    private HopDongService _service = null!;

    [TestInitialize]
    public async Task SetUpAsync()
    {
        _database = new Database(ConnectionString);
        _service = new HopDongService(new HopDongRepository(_database));

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
        var repo = new HopDongRepository(_database);

        // Client A gia hạn trước: ngay_ket_thuc 2026-12-31 -> 2027-12-31.
        Assert.IsTrue(await repo.UpdateEndDateAsync(TestContractId, ForwardEnd));

        // Client B (dữ liệu cũ) gia hạn về 2027-06-30 — phải bị DB chặn.
        Assert.IsFalse(await repo.UpdateEndDateAsync(TestContractId, BackwardEnd));

        var inDb = await GetEndDateFromDbAsync();
        Assert.AreEqual(ForwardEnd, inDb, "ngay_ket_thuc không được bị ghi lùi về sau.");
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
        // Mô phỏng race: client B vẫn thấy ngay_ket_thuc cũ (2026-12-31, hợp lệ theo validate),
        // nhưng DB đã có ngay_ket_thuc mới hơn nên UPDATE điều kiện trả về 0 dòng.
        var repo = new StaleRepo
        {
            Existing = new HopDongDto(
                TestContractId, TestRoomId, TestTenantId,
                new DateOnly(2026, 1, 1), CurrentEnd,
                3_000_000m, 1_500_000m, TrangThaiHopDong.HieuLuc, null),
            UpdateSucceeds = false,
        };
        var service = new HopDongService(repo);

        var ex = await Assert.ThrowsExceptionAsync<LoiNghiepVu>(
            () => service.RenewAsync(TestContractId, BackwardEnd));

        StringAssert.Contains(ex.Message, "Ngày gia hạn phải sau ngày kết thúc hiện tại.");
    }

    private async Task SeedContractAsync()
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

        const string insertContract = """
            INSERT INTO hop_dong (id, phong_id, nguoi_dai_dien_id, ngay_bat_dau, ngay_ket_thuc,
                                   gia_thue, tien_coc, trang_thai, ghi_chu)
            VALUES (@id, @phongId, @khachThueId, @ngayBatDau, @ngayKetThuc, 3000000, 1500000, 'HieuLuc', 'Renewal race test');
            """;
        await using (var cmd = new MySqlCommand(insertContract, connection))
        {
            cmd.Parameters.AddWithValue("@id", TestContractId);
            cmd.Parameters.AddWithValue("@phongId", TestRoomId);
            cmd.Parameters.AddWithValue("@khachThueId", TestTenantId);
            cmd.Parameters.AddWithValue("@ngayBatDau", new DateOnly(2026, 1, 1));
            cmd.Parameters.AddWithValue("@ngayKetThuc", CurrentEnd);
            await cmd.ExecuteNonQueryAsync();
        }
    }

    private async Task<DateOnly> GetEndDateFromDbAsync()
    {
        await using var connection = await _database.OpenAsync();
        await using var command = new MySqlCommand(
            "SELECT ngay_ket_thuc FROM hop_dong WHERE id = @id", connection);
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

    /// <summary>Stub: GetById trả về bản stale (ngay_ket_thuc cũ), Update mô phỏng DB chặn (0 dòng).</summary>
    private sealed class StaleRepo : IHopDongRepository
    {
        public required HopDongDto Existing { get; init; }
        public required bool UpdateSucceeds { get; init; }

        public Task<HopDongDto?> GetByIdAsync(int hopDongId, CancellationToken ct = default) =>
            Task.FromResult<HopDongDto?>(Existing);

        public Task<bool> HasActiveContractAsync(int phongId, CancellationToken ct = default) => Task.FromResult(false);
        public Task<bool> IsTenantInRoomAsync(int khachThueId, int phongId, CancellationToken ct = default) => Task.FromResult(true);
        public Task<HopDongDto> AddAsync(HopDongDto contract, CancellationToken ct = default) => Task.FromResult(contract);
        public Task<bool> TerminateAsync(int hopDongId, string? ghi_chu, CancellationToken ct = default) => Task.FromResult(true);
        public Task<bool> UpdateEndDateAsync(int hopDongId, DateOnly newEndDate, CancellationToken ct = default) =>
            Task.FromResult(UpdateSucceeds);
        public Task<List<MucHopDongItem>> GetAllAsync(CancellationToken ct = default) => Task.FromResult(new List<MucHopDongItem>());
    }
}
