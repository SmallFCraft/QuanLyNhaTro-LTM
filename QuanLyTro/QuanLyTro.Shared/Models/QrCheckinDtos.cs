namespace QuanLyTro.Shared.Models;

/// <summary>
/// DTO trả về cho action `HOP_DONG_SINH_QR`.
/// </summary>
public sealed record ThongTinSinhQrDto(
    int HopDongId,
    string MaQrToken,
    string MaPin,
    string QrDataString
);

/// <summary>
/// DTO trả về sau khi khách thuê quét QR / nhập PIN nhận phòng thành công (`KHACH_THUE_NHAN_PHONG_QR`).
/// </summary>
public sealed record KetQuaNhanPhongDto(
    int HopDongId,
    string SoPhong,
    decimal GiaThue,
    DateOnly NgayBatDau,
    DateOnly NgayKetThuc
);
