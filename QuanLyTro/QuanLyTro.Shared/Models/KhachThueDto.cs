namespace QuanLyTro.Shared.Models;

public sealed record KhachThueDto(
    int Id,
    int? PhongId,
    string HoTen,
    DateOnly NgaySinh,
    string Cccd,
    string SoDienThoai,
    string QueQuan,
    string? NoiLamViec,
    bool DaDangKyTamTru,
    string? SoPhong = null
);
