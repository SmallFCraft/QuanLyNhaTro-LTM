using System.Globalization;
using MySqlConnector;
using QuanLyTro.Server.Security;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Server.Data;

/// <summary>
/// Dữ liệu mẫu để demo: mọi tài khoản có MẬT KHẨU TRÙNG TÊN ĐĂNG NHẬP.
///
/// - `chutro`  / `chutro`  (bảng tai_khoan, vai ChuTro)
/// - `quanly`  / `quanly`  (bảng tai_khoan, vai QuanLy)
/// - `congan`  / `congan`  (bảng tai_khoan, vai CongAn)
/// - `100000000001` / `100000000001` (bảng khach_thue — người thuê đăng nhập bằng CCCD)
///
/// Chạy: dotnet run --project QuanLyTro/QuanLyTro.Server -- --seed-demo
/// Idempotent: chạy lại chỉ ghi đè đúng các dòng demo, không nhân bản.
/// Dải id 7001-7009, CCCD 1000000000xx — không đụng dải test (9188-9400, 990001+).
///
/// Bộ dữ liệu phủ đủ 4 trạng thái để mọi màn hình có dữ liệu thật:
///   D201 — đang thuê, có hóa đơn kỳ này chưa thu + 1 kỳ trước đã thu (biểu đồ doanh thu có cột).
///   D202 — hợp đồng CHỜ NHẬN PHÒNG kèm QR/PIN cố định (demo luồng nhận phòng).
///   D203 — đang thuê nhưng CHƯA lập hóa đơn kỳ này (demo cảnh báo "thiếu hóa đơn" ở Tổng quan).
///   D204 — trống hoàn toàn, không hợp đồng.
///
/// Tiền tố D (Demo) chứ không phải P: `so_phong` có UNIQUE nên số phòng demo trùng với phòng
/// thật (P201/P202/P204…) sẽ bị INSERT IGNORE âm thầm bỏ qua, seeder báo thành công mà không gieo gì.
/// </summary>
public static class DuLieuMau
{
    public const string ChuTroUsername = "chutro";
    public const string QuanLyUsername = "quanly";
    public const string CongAnUsername = "congan";
    public const string KhachThueIdCard = "100000000001";

    /// <summary>CCCD của khách đang chờ nhận phòng P202 — dùng để demo quét QR / nhập PIN.</summary>
    public const string KhachChoNhanPhongIdCard = "100000000002";

    /// <summary>Mã QR/PIN cố định của hợp đồng chờ nhận phòng — demo lặp lại được.</summary>
    public const string MaQrChoNhanPhong = "A1B2C3D4E5F60718";
    public const string MaPinChoNhanPhong = "12345678";

    /// <summary>
    /// Tài khoản nhận tiền gieo kèm bộ dữ liệu mẫu. Seeder GHI ĐÈ 4 cột này mỗi lần chạy,
    /// nhưng KHÔNG đụng `api_token` — token là bí mật, không được gieo hay xoá.
    /// </summary>
    public const string NganHangDemo = "MBBank";
    public const string SoTaiKhoanDemo = "0387368890";
    public const string ChuTaiKhoanDemo = "PHAM TRUONG HUY";

    public static async Task SeedAsync(Database database, CancellationToken ct = default)
    {
        // Luôn bảo đảm DB schema và enum mới nhất đã được áp dụng trước khi gieo dữ liệu.
        await new KhoiTaoSchema(database).InitializeAsync(ct);

        await using var connection = await database.OpenAsync(ct);

        // Giờ LOCAL, không phải UTC: 00:00–07:00 sáng giờ VN thì UTC vẫn là tháng trước,
        // khiến hóa đơn mẫu ghi lệch kỳ so với những gì giao diện hiển thị.
        var now = DateTime.Now;
        var month = now.ToString("yyyy-MM", CultureInfo.InvariantCulture);
        var prevMonth = now.AddMonths(-1).ToString("yyyy-MM", CultureInfo.InvariantCulture);
        var ngayKetThuc = DateOnly.FromDateTime(now).AddYears(3).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        // tai_khoan: ten_dang_nhap là khóa tự nhiên (UNIQUE), để id tự tăng.
        await UpsertTaiKhoanAsync(connection, ChuTroUsername, "Chủ Trọ Demo", "ChuTro", ct);
        await UpsertTaiKhoanAsync(connection, QuanLyUsername, "Quản Lý Demo", "QuanLy", ct);
        await UpsertTaiKhoanAsync(connection, CongAnUsername, "Công An Phường Demo", "CongAn", ct);

        // Gieo quyền mặc định vào quyen_vai_tro (QuanLy / CongAn / KhachThue).
        await SeedQuyenMacDinhAsync(connection, ct);

        await ExecuteAsync(connection, """
            INSERT INTO phong (id, so_phong, gia_thue, so_nguoi_toi_da, trang_thai, mo_ta)
            VALUES
                (7001, 'D201', 2000000, 2, 'DaThue', 'Phòng mẫu đang cho thuê'),
                (7002, 'D202', 2200000, 3, 'Trong',  'Phòng mẫu chờ khách nhận phòng'),
                (7003, 'D203', 1800000, 2, 'DaThue', 'Phòng mẫu đang thuê, chưa lập hóa đơn'),
                (7004, 'D204', 1900000, 2, 'Trong',  'Phòng mẫu còn trống')
            ON DUPLICATE KEY UPDATE
                so_phong = VALUES(so_phong), gia_thue = VALUES(gia_thue),
                so_nguoi_toi_da = VALUES(so_nguoi_toi_da),
                mo_ta = VALUES(mo_ta),
                -- Seeder GHI ĐÈ trạng thái của đúng 4 phòng demo (id 7001-7004). Phòng thật không
                -- nằm trong dải id này nên không bị ảnh hưởng.
                trang_thai = VALUES(trang_thai)
            """, ct);

        // Người thuê: mật khẩu = chính số CCCD (trùng tên đăng nhập).
        // 7002 chưa có phòng (phong_id NULL) — đúng trạng thái "chờ nhận phòng" của BR-19.
        await ExecuteAsync(connection, """
            INSERT INTO khach_thue (id, phong_id, ho_ten, ngay_sinh, cccd, mat_khau_hash, so_dien_thoai, que_quan, noi_lam_viec, da_dang_ky_tam_tru)
            VALUES
                (7001, 7001, 'Nguyễn Văn Demo',   '2001-05-10', @cccd1, @hash1, '0900000001', 'Đà Nẵng',   'Sinh viên', TRUE),
                (7002, NULL, 'Trần Thị Chờ',      '2002-08-22', @cccd2, @hash2, '0900000002', 'Quảng Nam', 'Nhân viên', FALSE),
                (7003, 7003, 'Lê Văn Chưa Đóng',  '2000-03-15', @cccd3, @hash3, '0900000003', 'Huế',       'Nhân viên', TRUE)
            ON DUPLICATE KEY UPDATE
                phong_id = VALUES(phong_id), ho_ten = VALUES(ho_ten), ngay_sinh = VALUES(ngay_sinh),
                mat_khau_hash = VALUES(mat_khau_hash), so_dien_thoai = VALUES(so_dien_thoai),
                que_quan = VALUES(que_quan), noi_lam_viec = VALUES(noi_lam_viec),
                da_dang_ky_tam_tru = VALUES(da_dang_ky_tam_tru)
            """, ct,
            ("@cccd1", KhachThueIdCard),
            ("@hash1", PasswordHasher.Hash(KhachThueIdCard)),
            ("@cccd2", KhachChoNhanPhongIdCard),
            ("@hash2", PasswordHasher.Hash(KhachChoNhanPhongIdCard)),
            ("@cccd3", "100000000003"),
            ("@hash3", PasswordHasher.Hash("100000000003")));

        // Hợp đồng: 7002 ở trạng thái Chờ nhận phòng kèm QR/PIN cố định để demo lặp lại được.
        // 7001 dùng ngày kết thúc +3 năm — bộ dữ liệu mẫu không được tự hết hạn theo thời gian.
        await ExecuteAsync(connection, """
            INSERT INTO hop_dong (id, phong_id, nguoi_dai_dien_id, ngay_bat_dau, ngay_ket_thuc, gia_thue, tien_coc, trang_thai, ghi_chu, ma_qr_token, ma_pin)
            VALUES
                (7001, 7001, 7001, '2026-01-01', @ngayKetThuc, 2000000, 1000000, 'HieuLuc',      'Hợp đồng mẫu',              NULL, NULL),
                (7002, 7002, 7002, @homNay,      @ngayKetThuc, 2200000, 1100000, 'ChoNhanPhong', 'Hợp đồng chờ nhận phòng',   @maQr, @maPin),
                (7003, 7003, 7003, '2026-02-01', @ngayKetThuc, 1800000,  900000, 'HieuLuc',      'Hợp đồng chưa lập hóa đơn', NULL, NULL)
            ON DUPLICATE KEY UPDATE
                phong_id = VALUES(phong_id), nguoi_dai_dien_id = VALUES(nguoi_dai_dien_id),
                ngay_bat_dau = VALUES(ngay_bat_dau), ngay_ket_thuc = VALUES(ngay_ket_thuc),
                gia_thue = VALUES(gia_thue), tien_coc = VALUES(tien_coc),
                trang_thai = VALUES(trang_thai), ghi_chu = VALUES(ghi_chu),
                ma_qr_token = VALUES(ma_qr_token), ma_pin = VALUES(ma_pin)
            """, ct,
            ("@ngayKetThuc", ngayKetThuc),
            ("@homNay", now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
            ("@maQr", MaQrChoNhanPhong),
            ("@maPin", MaPinChoNhanPhong));

        // Chỉ số điện nước kỳ hiện tại của P201: 1.230→1.280 kWh, 14→16 m³.
        await ExecuteAsync(connection, """
            INSERT INTO chi_so_dien_nuoc (id, phong_id, ky_cuoc, dien_cu, dien_moi, gia_dien, nuoc_cu, nuoc_moi, gia_nuoc)
            VALUES (7001, 7001, @month, 1230, 1280, 3500, 14, 16, 10000)
            ON DUPLICATE KEY UPDATE
                dien_cu = VALUES(dien_cu), dien_moi = VALUES(dien_moi),
                gia_dien = VALUES(gia_dien), nuoc_cu = VALUES(nuoc_cu),
                nuoc_moi = VALUES(nuoc_moi), gia_nuoc = VALUES(gia_nuoc)
            """, ct, ("@month", month));

        // P203 cố ý KHÔNG có chỉ số kỳ này → chưa lập được hóa đơn → Tổng quan hiện cảnh báo.

        // Hóa đơn: kỳ trước của P201 đã thu (để biểu đồ doanh thu có cột), kỳ này chưa thu.
        await ExecuteAsync(connection, """
            INSERT INTO hoa_don (id, phong_id, hop_dong_id, ky_cuoc, tien_phong, tien_dien, tien_nuoc, phi_khac, tong_tien, trang_thai, ngay_dong)
            VALUES
                (7001, 7001, 7001, @month,     2000000, 175000, 20000, 50000, 2245000, 'ChuaThu', NULL),
                (7002, 7001, 7001, @prevMonth, 2000000, 140000, 20000, 0,     2160000, 'DaThu',   NOW())
            ON DUPLICATE KEY UPDATE
                phong_id = VALUES(phong_id), hop_dong_id = VALUES(hop_dong_id),
                tien_phong = VALUES(tien_phong), tien_dien = VALUES(tien_dien),
                tien_nuoc = VALUES(tien_nuoc), phi_khac = VALUES(phi_khac),
                tong_tien = VALUES(tong_tien), trang_thai = VALUES(trang_thai),
                ngay_dong = VALUES(ngay_dong)
            """, ct, ("@month", month), ("@prevMonth", prevMonth));

        // Cấu hình nhận tiền: GHI ĐÈ 4 cột để QR dựng được ngay sau khi seed.
        // api_token KHÔNG nằm trong câu lệnh — token là bí mật, giữ nguyên giá trị đang có.
        await ExecuteAsync(connection, """
            INSERT INTO cau_hinh (id, ngan_hang, so_tai_khoan, chu_tai_khoan, template_qr)
            VALUES (1, @nganHang, @soTaiKhoan, @chuTaiKhoan, 'compact')
            ON DUPLICATE KEY UPDATE
                ngan_hang = VALUES(ngan_hang),
                so_tai_khoan = VALUES(so_tai_khoan),
                chu_tai_khoan = VALUES(chu_tai_khoan),
                template_qr = VALUES(template_qr)
            """, ct,
            ("@nganHang", NganHangDemo),
            ("@soTaiKhoan", SoTaiKhoanDemo),
            ("@chuTaiKhoan", ChuTaiKhoanDemo));
    }

    /// <summary>Mật khẩu = tên đăng nhập, đúng quy ước "trùng nhau" của bộ dữ liệu mẫu.</summary>
    private static Task UpsertTaiKhoanAsync(
        MySqlConnection connection, string tenDangNhap, string hoTen, string vaiTro, CancellationToken ct)
    {
        const string sql = """
            INSERT INTO tai_khoan (ten_dang_nhap, mat_khau_hash, ho_ten, vai_tro)
            VALUES (@tenDangNhap, @hash, @hoTen, @vaiTro)
            ON DUPLICATE KEY UPDATE
                mat_khau_hash = VALUES(mat_khau_hash), ho_ten = VALUES(ho_ten), vai_tro = VALUES(vai_tro)
            """;

        return ExecuteAsync(connection, sql, ct,
            ("@tenDangNhap", tenDangNhap),
            ("@hash", PasswordHasher.Hash(tenDangNhap)),
            ("@hoTen", hoTen),
            ("@vaiTro", vaiTro));
    }

    private static async Task SeedQuyenMacDinhAsync(MySqlConnection connection, CancellationToken ct)
    {
        // Gieo mọi quyền mặc định vào quyen_vai_tro và sổ cái quyen_mac_dinh_da_ap_dung.
        // Dùng INSERT IGNORE để nếu đã có rồi thì bỏ qua, nếu code thêm quyền mới thì tự động bù.
        const string insertQuyenSql = "INSERT IGNORE INTO quyen_vai_tro (vai_tro, hanh_dong) VALUES (@vaiTro, @hanhDong)";
        const string insertSoCaiSql = "INSERT IGNORE INTO quyen_mac_dinh_da_ap_dung (vai_tro, hanh_dong) VALUES (@vaiTro, @hanhDong)";

        foreach (var (vaiTro, danhSachHanhDong) in QuyenMacDinhTheoVaiTro.All)
        {
            foreach (var hanhDong in danhSachHanhDong)
            {
                await ExecuteAsync(connection, insertQuyenSql, ct, ("@vaiTro", vaiTro), ("@hanhDong", hanhDong));
                await ExecuteAsync(connection, insertSoCaiSql, ct, ("@vaiTro", vaiTro), ("@hanhDong", hanhDong));
            }
        }
    }

    private static async Task ExecuteAsync(
        MySqlConnection connection, string sql, CancellationToken ct, params (string Name, object Value)[] parameters)
    {
        await using var command = new MySqlCommand(sql, connection);
        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        await command.ExecuteNonQueryAsync(ct);
    }
}
