using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Repositories;

/// <summary>Bản ghi tài khoản người dùng đọc từ bảng `tai_khoan` (Chủ trọ hoặc Công an).</summary>
public sealed record TaiKhoanRecord(int Id, string MatKhauHash, string HoTen, VaiTroNguoiDung VaiTro = VaiTroNguoiDung.ChuTro);

/// <summary>Bản ghi người thuê đọc từ bảng `khach_thue` (đăng nhập bằng CCCD).</summary>
public sealed record KhachThueAuthRecord(int Id, string Cccd, string MatKhauHash, string HoTen, int? PhongId);

/// <summary>Biên truy cập bảng `tai_khoan` — interface để XacThucService test được không cần DB.</summary>
public interface ITaiKhoanRepository
{
    Task<TaiKhoanRecord?> FindByUsernameAsync(string tenDangNhap, CancellationToken ct = default);
}

/// <summary>Biên truy cập bảng `khach_thue` — interface để XacThucService test được không cần DB.</summary>
public interface IKhachThueRepository
{
    Task<KhachThueAuthRecord?> FindByCccdAsync(string cccd, CancellationToken ct = default);
}
