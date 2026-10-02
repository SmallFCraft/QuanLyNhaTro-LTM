namespace QuanLyTro.Shared.Models;

public sealed record HoaDonDto(
    int Id,
    int PhongId,
    int HopDongId,
    string KyCuoc, // yyyy-MM
    decimal TienPhong,
    decimal TienDien,
    decimal TienNuoc,
    decimal PhiKhac,
    decimal TongTien,
    TrangThaiHoaDon TrangThai,
    DateTime? NgayDong
);

/// <summary>Kết quả phân trang danh sách hóa đơn của khách thuê (BR-14).</summary>
public sealed record TrangHoaDonCuaToiDto(
    List<HoaDonDto> DanhSach,
    int TongSo,
    int Trang,
    int SoLuongMoiTrang
);
