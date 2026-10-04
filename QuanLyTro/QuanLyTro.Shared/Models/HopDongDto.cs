namespace QuanLyTro.Shared.Models;

public sealed record HopDongDto(
    int Id,
    int PhongId,
    int NguoiDaiDienId,
    DateOnly NgayBatDau,
    DateOnly NgayKetThuc,
    decimal GiaThue,
    decimal TienCoc,
    TrangThaiHopDong TrangThai,
    string? GhiChu,
    string? TenNguoiDaiDien = null,
    string? SoPhong = null,
    string? MaQrToken = null,
    string? MaPin = null
);
