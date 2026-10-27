using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Server.Data;

namespace QuanLyTro.Tests;

/// <summary>
/// Dữ liệu mẫu trên MySQL thật. Quy ước: mật khẩu TRÙNG tên đăng nhập cho mọi tài khoản.
/// Idempotent — chạy nhiều lần vẫn đúng số dòng. Dải id 7001-7009, CCCD 1000000000xx.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class DemoSeederTests
{
    private const string ConnectionString =
        "Server=127.0.0.1;Port=3306;Database=quanly_phongtro_nhs;User Id=root;Password=;SslMode=None;";

    private static readonly Database Db = new(ConnectionString);

    [TestInitialize]
    public Task SeedAsync() => DuLieuMau.SeedAsync(Db);

    [TestMethod]
    public async Task Seed_CreatesManagerAccount_WithPasswordMatchingUsername()
    {
        var account = await new QuanLyTro.Server.Repositories.TaiKhoanRepository(Db)
            .FindByUsernameAsync(DuLieuMau.QuanLyUsername);

        Assert.IsNotNull(account, "Thiếu tài khoản demo manager");
        Assert.AreEqual("QuanLy", account.VaiTro.ToString(), "Sai vai cho manager");
        Assert.IsTrue(
            QuanLyTro.Server.Security.PasswordHasher.Verify(DuLieuMau.QuanLyUsername, account.MatKhauHash),
            "Mật khẩu của manager phải bằng chính tên đăng nhập.");
    }

    [TestMethod]
    public async Task Seed_CreatesLandlordAndPoliceAccounts_WithPasswordMatchingUsername()
    {
        foreach (var (tenDangNhap, vai_tro) in new[]
                 {
                     (DuLieuMau.ChuTroUsername, "ChuTro"),
                     (DuLieuMau.CongAnUsername, "CongAn"),
                 })
        {
            var account = await new QuanLyTro.Server.Repositories.TaiKhoanRepository(Db)
                .FindByUsernameAsync(tenDangNhap);

            Assert.IsNotNull(account, $"Thiếu tài khoản demo {tenDangNhap}");
            Assert.AreEqual(vai_tro, account.VaiTro.ToString(), $"Sai vai cho {tenDangNhap}");
            Assert.IsTrue(
                QuanLyTro.Server.Security.PasswordHasher.Verify(tenDangNhap, account.MatKhauHash),
                $"Mật khẩu của {tenDangNhap} phải bằng chính tên đăng nhập.");
        }
    }

    [TestMethod]
    public async Task Seed_CreatesTenantLoggingInByIdCard_WithPasswordMatchingIdCard()
    {
        var tenant = await new QuanLyTro.Server.Repositories.KhachThueAuthRepository(Db)
            .FindByCccdAsync(DuLieuMau.KhachThueIdCard);

        Assert.IsNotNull(tenant, "Phải có người thuê mẫu đăng nhập được bằng CCCD.");
        Assert.AreEqual(7001, tenant.PhongId, "Người thuê mẫu phải ở phòng 7001.");
        Assert.IsTrue(
            QuanLyTro.Server.Security.PasswordHasher.Verify(DuLieuMau.KhachThueIdCard, tenant.MatKhauHash),
            "Mật khẩu người thuê phải trùng số CCCD (tên đăng nhập).");
    }

    [TestMethod]
    public async Task Seed_IsIdempotent_SecondRunDoesNotDuplicate()
    {
        await DuLieuMau.SeedAsync(Db);
        await DuLieuMau.SeedAsync(Db);

        foreach (var table in new[] { "phong", "khach_thue", "hop_dong", "chi_so_dien_nuoc", "hoa_don" })
        {
            var count = Convert.ToInt32(await ScalarAsync($"SELECT COUNT(*) FROM {table} WHERE id BETWEEN 7001 AND 7009"));
            var expected = table switch
            {
                "phong" => 3,
                _ => 1,
            };

            Assert.AreEqual((int)expected, count, $"{table} bị nhân bản khi seed lại.");
        }
    }

    /// <summary>
    /// Seed lại KHÔNG được lật trạng thái phòng đang có người về 'Trong'. Trước đây cột trang_thai
    /// nằm trong ON DUPLICATE KEY UPDATE nên mỗi lần chạy test/--seed-demo là phòng có người + hợp đồng
    /// hiệu lực bị ghi thành "Phòng trống", khiến chủ trọ tưởng phòng trống rồi quét lại mã QR cũ.
    /// </summary>
    [TestMethod]
    public async Task Seed_DoesNotResetOccupiedRoomStatus()
    {
        await ExecuteAsync("UPDATE phong SET trang_thai = 'DaThue' WHERE id = 7002");

        await DuLieuMau.SeedAsync(Db);

        Assert.AreEqual("DaThue", await ScalarAsync("SELECT trang_thai FROM phong WHERE id = 7002"),
            "Seed lại không được đổi trạng thái phòng đang cho thuê về 'Trong'.");
    }

    [TestMethod]
    public async Task Seed_InvoiceMatchesRoomContractAndCurrentMonth()
    {
        var columns = await ScalarAsync("""
            SELECT CONCAT_WS('|', i.ky_cuoc, i.tong_tien, i.trang_thai, r.so_phong, c.trang_thai)
            FROM hoa_don i
            JOIN phong r ON r.id = i.phong_id
            JOIN hop_dong c ON c.id = i.hop_dong_id
            WHERE i.id = 7001
            """);

        var expected = $"{DateTime.UtcNow:yyyy-MM}|2245000.00|ChuaThu|P201|HieuLuc";
        Assert.AreEqual(expected, columns, "Hóa đơn mẫu phải khớp phòng P201, hợp đồng HieuLuc, kỳ hiện tại.");
    }

    private async Task ExecuteAsync(string sql)
    {
        await using var connection = await Db.OpenAsync();
        await using var command = new MySqlConnector.MySqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private async Task<string> ScalarAsync(string sql)
    {
        await using var connection = await Db.OpenAsync();
        await using var command = new MySqlConnector.MySqlCommand(sql, connection);
        return Convert.ToString(await command.ExecuteScalarAsync()) ?? string.Empty;
    }
}
