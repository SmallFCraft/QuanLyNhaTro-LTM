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

/// <summary>Kết quả phân trang danh sách hóa đơn của khách thuê (BR-14). PhongId null = chưa có phòng (BR-19).</summary>
public sealed record TrangHoaDonCuaToiDto(
    List<HoaDonDto> DanhSach,
    int TongSo,
    int Trang,
    int SoLuongMoiTrang,
    int? PhongId = null,
    string? SoPhong = null,
    HopDongCuaToiDto? HopDong = null,
    ChiSoKyNayDto? ChiSoKyNay = null
);

/// <summary>Hợp đồng đang hiệu lực của khách — shell khách thuê hiển thị hạn + giá thuê.</summary>
public sealed record HopDongCuaToiDto(
    int Id,
    string? SoPhong,
    DateOnly NgayBatDau,
    DateOnly NgayKetThuc,
    decimal GiaThue,
    decimal TienCoc
);

/// <summary>Chỉ số điện nước đã chốt của hóa đơn đang hiển thị — để khách tự kiểm tra cách tính.</summary>
public sealed record ChiSoKyNayDto(
    string KyCuoc,
    int DienCu,
    int DienMoi,
    decimal GiaDien,
    int NuocCu,
    int NuocMoi,
    decimal GiaNuoc,
    string HinhThucNuoc,
    int SoNguoiNuoc
);
