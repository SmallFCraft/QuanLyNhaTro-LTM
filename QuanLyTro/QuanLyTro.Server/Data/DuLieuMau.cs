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
/// </summary>
public static class DuLieuMau
{
    public const string ChuTroUsername = "chutro";
    public const string QuanLyUsername = "quanly";
    public const string CongAnUsername = "congan";
    public const string KhachThueIdCard = "100000000001";

    private const int PhongId = 7001;

    public static async Task SeedAsync(Database database, CancellationToken ct = default)
    {
        // Luôn bảo đảm DB schema và enum mới nhất đã được áp dụng trước khi gieo dữ liệu.
        await new KhoiTaoSchema(database).InitializeAsync(ct);

        await using var connection = await database.OpenAsync(ct);
        var month = DateTime.UtcNow.ToString("yyyy-MM", System.Globalization.CultureInfo.InvariantCulture);

        // tai_khoan: ten_dang_nhap là khóa tự nhiên (UNIQUE), để id tự tăng.
        await UpsertTaiKhoanAsync(connection, ChuTroUsername, "Chủ Trọ Demo", "ChuTro", ct);
        await UpsertTaiKhoanAsync(connection, QuanLyUsername, "Quản Lý Demo", "QuanLy", ct);
        await UpsertTaiKhoanAsync(connection, CongAnUsername, "Công An Phường Demo", "CongAn", ct);

        // Gieo quyền mặc định vào quyen_vai_tro (QuanLy / CongAn / KhachThue).
        await SeedQuyenMacDinhAsync(connection, ct);

        // Phòng cho thuê có người ở (kèm 2 phòng trống để màn Quản lý phòng có dữ liệu).
        await ExecuteAsync(connection, """
            INSERT INTO phong (id, so_phong, gia_thue, so_nguoi_toi_da, trang_thai, mo_ta)
            VALUES
                (7001, 'P201', 2000000, 2, 'DaThue', 'Phòng mẫu đang cho thuê'),
                (7002, 'P202', 2200000, 3, 'Trong',  'Phòng mẫu còn trống'),
                (7003, 'P203', 1800000, 2, 'Trong',  'Phòng mẫu còn trống')
            ON DUPLICATE KEY UPDATE
                so_phong = VALUES(so_phong), gia_thue = VALUES(gia_thue),
                so_nguoi_toi_da = VALUES(so_nguoi_toi_da), trang_thai = VALUES(trang_thai),
                mo_ta = VALUES(mo_ta)
            """, ct);

        // Người thuê: mật khẩu = chính số CCCD (trùng tên đăng nhập).
        await ExecuteAsync(connection, """
            INSERT INTO khach_thue (id, phong_id, ho_ten, ngay_sinh, cccd, mat_khau_hash, so_dien_thoai, que_quan, noi_lam_viec, da_dang_ky_tam_tru)
            VALUES (7001, 7001, 'Nguyễn Văn Demo', '2001-05-10', @cccd, @hash, '0900000001', 'Đà Nẵng', 'Sinh viên', TRUE)
            ON DUPLICATE KEY UPDATE
                phong_id = VALUES(phong_id), ho_ten = VALUES(ho_ten), ngay_sinh = VALUES(ngay_sinh),
                mat_khau_hash = VALUES(mat_khau_hash), so_dien_thoai = VALUES(so_dien_thoai),
                que_quan = VALUES(que_quan), noi_lam_viec = VALUES(noi_lam_viec),
                da_dang_ky_tam_tru = VALUES(da_dang_ky_tam_tru)
            """, ct,
            ("@cccd", KhachThueIdCard),
            ("@hash", PasswordHasher.Hash(KhachThueIdCard)));

        await ExecuteAsync(connection, """
            INSERT INTO hop_dong (id, phong_id, nguoi_dai_dien_id, ngay_bat_dau, ngay_ket_thuc, gia_thue, tien_coc, trang_thai, ghi_chu)
            VALUES (7001, 7001, 7001, '2026-01-01', '2026-12-31', 2000000, 1000000, 'HieuLuc', 'Hợp đồng mẫu')
            ON DUPLICATE KEY UPDATE
                phong_id = VALUES(phong_id), nguoi_dai_dien_id = VALUES(nguoi_dai_dien_id),
                ngay_bat_dau = VALUES(ngay_bat_dau), ngay_ket_thuc = VALUES(ngay_ket_thuc),
                gia_thue = VALUES(gia_thue), tien_coc = VALUES(tien_coc),
                trang_thai = VALUES(trang_thai), ghi_chu = VALUES(ghi_chu)
            """, ct);

        // Chỉ số điện nước kỳ hiện tại: 1.230→1.280 kWh, 14→16 m³.
        await ExecuteAsync(connection, """
            INSERT INTO chi_so_dien_nuoc (id, phong_id, ky_cuoc, dien_cu, dien_moi, gia_dien, nuoc_cu, nuoc_moi, gia_nuoc)
            VALUES (7001, 7001, @month, 1230, 1280, 3500, 14, 16, 10000)
            ON DUPLICATE KEY UPDATE
                dien_cu = VALUES(dien_cu), dien_moi = VALUES(dien_moi),
                gia_dien = VALUES(gia_dien), nuoc_cu = VALUES(nuoc_cu),
                nuoc_moi = VALUES(nuoc_moi), gia_nuoc = VALUES(gia_nuoc)
            """, ct, ("@month", month));

        // Hóa đơn chưa thu: 2.000.000 + 175.000 + 20.000 + 50.000 = 2.245.000 đ.
        await ExecuteAsync(connection, """
            INSERT INTO hoa_don (id, phong_id, hop_dong_id, ky_cuoc, tien_phong, tien_dien, tien_nuoc, phi_khac, tong_tien, trang_thai)
            VALUES (7001, 7001, 7001, @month, 2000000, 175000, 20000, 50000, 2245000, 'ChuaThu')
            ON DUPLICATE KEY UPDATE
                phong_id = VALUES(phong_id), hop_dong_id = VALUES(hop_dong_id),
                tien_phong = VALUES(tien_phong), tien_dien = VALUES(tien_dien),
                tien_nuoc = VALUES(tien_nuoc), phi_khac = VALUES(phi_khac),
                tong_tien = VALUES(tong_tien), trang_thai = VALUES(trang_thai)
            """, ct, ("@month", month));
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
