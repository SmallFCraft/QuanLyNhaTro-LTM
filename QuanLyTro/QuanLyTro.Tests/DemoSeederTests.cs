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
    private static readonly string ConnectionString = TestDatabase.ConnectionString;

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

        foreach (var (table, expected) in new[]
                 {
                     ("phong", 4),
                     ("khach_thue", 3),
                     ("hop_dong", 3),
                     ("chi_so_dien_nuoc", 1),
                     ("hoa_don", 2),
                 })
        {
            var count = Convert.ToInt32(await ScalarAsync($"SELECT COUNT(*) FROM {table} WHERE id BETWEEN 7001 AND 7009"));
            Assert.AreEqual(expected, count, $"{table} bị nhân bản khi seed lại.");
        }
    }

    /// <summary>
    /// Phòng THẬT không được bị seeder đụng tới. Trước đây seeder dùng số phòng P201/P202 trùng
    /// với phòng thật, khiến chạy --seed-demo là lật trạng thái phòng đang có người. Nay dải demo
    /// là D201-D204 + id 7001-7004 nên phòng thật nằm ngoài tầm với.
    /// </summary>
    [TestMethod]
    public async Task Seed_KhongDungToiPhongThat()
    {
        // Một phòng thật giả lập, nằm ngoài dải id demo.
        await ExecuteAsync("""
            INSERT INTO phong (id, so_phong, gia_thue, so_nguoi_toi_da, trang_thai, mo_ta)
            VALUES (999901, 'REAL01', 1500000, 2, 'DaThue', 'Phòng thật giả lập')
            ON DUPLICATE KEY UPDATE trang_thai = 'DaThue'
            """);

        await DuLieuMau.SeedAsync(Db);

        Assert.AreEqual("DaThue", await ScalarAsync("SELECT trang_thai FROM phong WHERE id = 999901"),
            "Seeder không được đổi trạng thái phòng thật");

        await ExecuteAsync("DELETE FROM phong WHERE id = 999901");
    }

    /// <summary>Seed lại phải khôi phục đúng trạng thái thiết kế của 4 phòng demo.</summary>
    [TestMethod]
    public async Task Seed_PhucHoiTrangThaiPhongDemo()
    {
        await ExecuteAsync("UPDATE phong SET trang_thai = 'DaThue' WHERE id = 7002");

        await DuLieuMau.SeedAsync(Db);

        Assert.AreEqual("Trong", await ScalarAsync("SELECT trang_thai FROM phong WHERE id = 7002"),
            "P202 là phòng chờ nhận phòng, seed lại phải trả về 'Trong'");
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

        // Giờ LOCAL (không phải UTC): 00:00–07:00 sáng giờ VN thì UTC vẫn là tháng trước.
        var expected = $"{DateTime.Now:yyyy-MM}|2245000.00|ChuaThu|D201|HieuLuc";
        Assert.AreEqual(expected, columns, "Hóa đơn mẫu phải khớp phòng P201, hợp đồng HieuLuc, kỳ hiện tại.");
    }

    [TestMethod]
    public async Task Seed_CoversAllWorkflowStates()
    {
        // P201 đang thuê có hóa đơn chưa thu, P202 chờ nhận phòng, P203 đang thuê chưa lập hóa đơn, P204 trống.
        var trang_thai = await ScalarAsync("""
            SELECT GROUP_CONCAT(CONCAT(so_phong, ':', trang_thai) ORDER BY so_phong)
            FROM phong WHERE id BETWEEN 7001 AND 7009
            """);
        Assert.AreEqual("D201:DaThue,D202:Trong,D203:DaThue,D204:Trong", trang_thai,
            "Bộ dữ liệu mẫu phải phủ đủ 4 trạng thái phòng");

        // P203 có hợp đồng hiệu lực nhưng KHÔNG có hóa đơn kỳ này → demo cảnh báo thiếu hóa đơn.
        var p203 = await ScalarAsync("""
            SELECT COUNT(*) FROM hoa_don
            WHERE phong_id = 7003 AND ky_cuoc = DATE_FORMAT(NOW(), '%Y-%m')
            """);
        Assert.AreEqual("0", p203, "P203 phải cố ý thiếu hóa đơn kỳ này");
    }

    [TestMethod]
    public async Task Seed_ChoNhanPhongContract_HasQrAndPin()
    {
        var hop_dong = await ScalarAsync("""
            SELECT CONCAT_WS('|', trang_thai, ma_qr_token, ma_pin)
            FROM hop_dong WHERE id = 7002
            """);

        Assert.AreEqual($"ChoNhanPhong|{DuLieuMau.MaQrChoNhanPhong}|{DuLieuMau.MaPinChoNhanPhong}", hop_dong,
            "Hợp đồng chờ nhận phòng phải có QR/PIN cố định để demo lặp lại được");
    }

    [TestMethod]
    public async Task Seed_GhiDeCauHinhNhungKhongDungApiToken()
    {
        await ExecuteAsync("""
            INSERT INTO cau_hinh (id, ngan_hang, so_tai_khoan, chu_tai_khoan, template_qr, api_token)
            VALUES (1, 'SaiNganHang', '0000', 'SAI', 'qronly', 'TOKEN_KHONG_DUOC_XOA')
            ON DUPLICATE KEY UPDATE ngan_hang='SaiNganHang', so_tai_khoan='0000',
                chu_tai_khoan='SAI', template_qr='qronly', api_token='TOKEN_KHONG_DUOC_XOA'
            """);

        await DuLieuMau.SeedAsync(Db);

        var cau_hinh = await ScalarAsync("""
            SELECT CONCAT_WS('|', ngan_hang, so_tai_khoan, chu_tai_khoan, template_qr, api_token)
            FROM cau_hinh WHERE id = 1
            """);
        Assert.AreEqual(
            $"{DuLieuMau.NganHangDemo}|{DuLieuMau.SoTaiKhoanDemo}|{DuLieuMau.ChuTaiKhoanDemo}|compact|TOKEN_KHONG_DUOC_XOA",
            cau_hinh,
            "Seeder ghi đè cấu hình ngân hàng nhưng phải giữ nguyên API token");
    }

    [TestMethod]
    public async Task Seed_HopDongKhongHetHanTheoThoiGian()
    {
        // Dữ liệu mẫu phải dùng được lâu dài — hợp đồng không được hết hạn sau vài tháng.
        var con_han = await ScalarAsync("""
            SELECT COUNT(*) FROM hop_dong
            WHERE id BETWEEN 7001 AND 7009 AND ngay_ket_thuc > CURDATE()
            """);

        Assert.AreEqual("3", con_han, "Mọi hợp đồng mẫu phải còn hiệu lực tại thời điểm seed");
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
