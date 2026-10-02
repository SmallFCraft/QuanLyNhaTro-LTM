namespace QuanLyTro.Shared.Models;

public sealed record MucQuyenVaiTroDto(string HanhDong, string MoTa, string Nhom);

public sealed record MaTranQuyenVaiTroDto(
    Dictionary<string, List<string>> QuyenTheoVaiTro,
    List<MucQuyenVaiTroDto> DanhSachHanhDong
);

public sealed record YeuCauCapNhatQuyenVaiTro(string VaiTro, List<string> DanhSachHanhDong);

/// <summary>Payload đăng nhập: { TenDangNhap, MatKhau }</summary>
public sealed record YeuCauDangNhap(string TenDangNhap, string MatKhau);

/// <summary>Kết quả đăng nhập: { Token, HoTen, VaiTro }</summary>
public sealed record KetQuaDangNhap(string Token, string HoTen, VaiTroNguoiDung VaiTro);

/// <summary>Payload lấy chỉ số kỳ trước: { PhongId }</summary>
public sealed record TruyVanPhong(int PhongId);

/// <summary>Payload tạo hóa đơn</summary>
public sealed record YeuCauTaoHoaDon(int PhongId, string KyCuoc, decimal PhiKhac);

/// <summary>Thống kê tổng quan: US-04, US-18, US-19.</summary>
public sealed record BaoCaoTongQuanDto(
    int TongSoPhong,
    int PhongTrong,
    int PhongDaThue,
    int KhachHienTai,
    decimal SoTienDaThu,
    decimal SoTienChuaThu
);

/// <summary>Dòng xuất danh sách tạm trú (US-08): họ tên, ngày sinh, CCCD, quê quán, phòng.</summary>
public sealed record XuatHoSoTamTruDto(
    string HoTen,
    DateOnly NgaySinh,
    string Cccd,
    string QueQuan,
    string SoPhong
);
