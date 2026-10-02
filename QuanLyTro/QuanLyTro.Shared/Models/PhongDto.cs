namespace QuanLyTro.Shared.Models;

public sealed record PhongDto(
    int Id,
    string SoPhong,
    decimal GiaThue,
    int SoNguoiToiDa,
    TrangThaiPhong TrangThai,
    string? MoTa,
    int SoNguoiHienTai = 0
);
