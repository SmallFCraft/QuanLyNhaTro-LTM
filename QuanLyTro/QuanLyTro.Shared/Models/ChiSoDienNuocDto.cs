namespace QuanLyTro.Shared.Models;

public sealed record ChiSoDienNuocDto(
    int Id,
    int PhongId,
    string KyCuoc, // yyyy-MM
    int DienCu,
    int DienMoi,
    decimal GiaDien,
    int NuocCu,
    int NuocMoi,
    decimal GiaNuoc
);
