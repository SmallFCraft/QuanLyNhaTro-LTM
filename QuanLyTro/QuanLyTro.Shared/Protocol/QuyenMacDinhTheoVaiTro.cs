using QuanLyTro.Shared.Models;

namespace QuanLyTro.Shared.Protocol;

/// <summary>
/// Bộ quyền MẶC ĐỊNH cho từng vai trò cấp dưới (QuanLy/CongAn/KhachThue).
/// Chủ trọ (ChuTro) bypass cứng trong MaTranPhanQuyen.
/// </summary>
public static class QuyenMacDinhTheoVaiTro
{
    /// <summary>Quản lý: toàn bộ nghiệp vụ vận hành cơ sở. Không có quyền quản trị phân quyền.</summary>
    public static readonly IReadOnlyList<string> QuanLy =
    [
        ActionNames.PhongLayTatCa, ActionNames.PhongThem, ActionNames.PhongCapNhat, ActionNames.PhongXoa,
        ActionNames.KhachThueTheoPhong, ActionNames.KhachThueThem, ActionNames.KhachThueCapNhat,
        ActionNames.KhachThueTraPhong, ActionNames.KhachThueXoa,
        ActionNames.HopDongTao, ActionNames.HopDongChamDut, ActionNames.HopDongGiaHan,
        ActionNames.HopDongLayTatCa,
        ActionNames.DienNuocLayKyTruoc, ActionNames.DienNuocGhiSo,
        ActionNames.HoaDonTao, ActionNames.HoaDonLayTatCa, ActionNames.HoaDonThanhToan,
        ActionNames.BaoCaoTongQuan,
        ActionNames.XuatHoSoTamTru, ActionNames.LichSuCuTruLay, ActionNames.XuatLichSuCuTru,
    ];

    /// <summary>Công an phường: chỉ đọc phòng / người thuê / hồ sơ tạm trú (BR-16).</summary>
    public static readonly IReadOnlyList<string> CongAn =
    [
        ActionNames.PhongLayTatCa,
        ActionNames.KhachThueTheoPhong,
        ActionNames.XuatHoSoTamTru,
        ActionNames.LichSuCuTruLay,
        ActionNames.XuatLichSuCuTru,
    ];

    /// <summary>Người thuê: chỉ xem hóa đơn của chính mình.</summary>
    public static readonly IReadOnlyList<string> KhachThue =
    [
        ActionNames.HoaDonCuaToi,
    ];

    /// <summary>Role (đúng chính tả ENUM) → danh sách action mặc định.</summary>
    public static IReadOnlyDictionary<string, IReadOnlyList<string>> All { get; } =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [nameof(VaiTroNguoiDung.QuanLy)] = QuanLy,
            [nameof(VaiTroNguoiDung.CongAn)] = CongAn,
            [nameof(VaiTroNguoiDung.KhachThue)] = KhachThue,
        };

    /// <summary>Danh mục action có thể gán quyền, render màn Phân quyền.</summary>
    public static IReadOnlyList<MucQuyenVaiTroDto> DanhMuc { get; } =
    [
        new(ActionNames.PhongLayTatCa, "Xem danh sách phòng", "Phòng trọ"),
        new(ActionNames.PhongThem, "Thêm phòng", "Phòng trọ"),
        new(ActionNames.PhongCapNhat, "Sửa phòng", "Phòng trọ"),
        new(ActionNames.PhongXoa, "Xóa phòng", "Phòng trọ"),

        new(ActionNames.KhachThueTheoPhong, "Xem khách thuê theo phòng", "Khách thuê"),
        new(ActionNames.KhachThueThem, "Thêm khách thuê", "Khách thuê"),
        new(ActionNames.KhachThueCapNhat, "Sửa thông tin khách thuê", "Khách thuê"),
        new(ActionNames.KhachThueTraPhong, "Trả phòng", "Khách thuê"),
        new(ActionNames.KhachThueXoa, "Xóa khách thuê", "Khách thuê"),

        new(ActionNames.HopDongTao, "Tạo hợp đồng", "Hợp đồng"),
        new(ActionNames.HopDongGiaHan, "Gia hạn hợp đồng", "Hợp đồng"),
        new(ActionNames.HopDongChamDut, "Chấm dứt hợp đồng", "Hợp đồng"),
        new(ActionNames.HopDongLayTatCa, "Xem danh sách hợp đồng", "Hợp đồng"),

        new(ActionNames.DienNuocLayKyTruoc, "Xem chỉ số điện nước kỳ trước", "Điện nước"),
        new(ActionNames.DienNuocGhiSo, "Ghi chỉ số điện nước", "Điện nước"),

        new(ActionNames.HoaDonTao, "Tạo hóa đơn", "Hóa đơn"),
        new(ActionNames.HoaDonLayTatCa, "Xem danh sách hóa đơn", "Hóa đơn"),
        new(ActionNames.HoaDonThanhToan, "Thu tiền / thanh toán hóa đơn", "Hóa đơn"),
        new(ActionNames.HoaDonCuaToi, "Xem hóa đơn của chính mình", "Hóa đơn"),

        new(ActionNames.BaoCaoTongQuan, "Xem thống kê doanh thu", "Báo cáo & cư trú"),
        new(ActionNames.XuatHoSoTamTru, "Xuất hồ sơ tạm trú", "Báo cáo & cư trú"),
        new(ActionNames.LichSuCuTruLay, "Tra cứu lịch sử cư trú", "Báo cáo & cư trú"),
        new(ActionNames.XuatLichSuCuTru, "Xuất lịch sử cư trú", "Báo cáo & cư trú"),
    ];
}
