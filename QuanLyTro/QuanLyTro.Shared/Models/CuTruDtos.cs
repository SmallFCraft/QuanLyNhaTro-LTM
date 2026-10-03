namespace QuanLyTro.Shared.Models;

public sealed record LichSuCuTruDto(
    int Id,
    string HoTen,
    string Cccd,
    string SoPhong,
    string LoaiBienDong, // "Vào", "Ra", "Chuyển phòng"
    DateTime NgayBienDong,
    string? GhiChu);

public sealed record YeuCauXuatLichSu(
    DateTime? TuNgay,
    DateTime? DenNgay,
    string DinhDang, // "CSV", "Excel", "PDF"
    string? SoPhong,
    string? LoaiBienDong);

public sealed record KetQuaXuatFile(
    string DuongDanFile,
    int SoDong);
