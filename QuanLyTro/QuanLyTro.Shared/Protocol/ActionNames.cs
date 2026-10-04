namespace QuanLyTro.Shared.Protocol;

/// <summary>
/// Tên 26 hành động của giao thức TCP bằng tiếng Việt không dấu.
/// </summary>
public static class ActionNames
{
    public const string DangNhap = "DANG_NHAP";

    public const string PhongLayTatCa = "PHONG_LAY_TAT_CA";
    public const string PhongThem = "PHONG_THEM";
    public const string PhongCapNhat = "PHONG_CAP_NHAT";
    public const string PhongXoa = "PHONG_XOA";

    public const string KhachThueTheoPhong = "KHACH_THUE_THEO_PHONG";
    public const string KhachThueThem = "KHACH_THUE_THEM";
    public const string KhachThueCapNhat = "KHACH_THUE_CAP_NHAT";
    public const string KhachThueTraPhong = "KHACH_THUE_TRA_PHONG";
    public const string KhachThueXoa = "KHACH_THUE_XOA";

    public const string HopDongTao = "HOP_DONG_TAO";
    public const string HopDongGiaHan = "HOP_DONG_GIA_HAN";
    public const string HopDongChamDut = "HOP_DONG_CHAM_DUT";
    public const string HopDongLayTatCa = "HOP_DONG_LAY_TAT_CA";

    public const string DienNuocLayKyTruoc = "DIEN_NUOC_LAY_KY_TRUOC";
    public const string DienNuocGhiSo = "DIEN_NUOC_GHI_SO";

    public const string HoaDonTao = "HOA_DON_TAO";
    public const string HoaDonLayTatCa = "HOA_DON_LAY_TAT_CA";
    public const string HoaDonThanhToan = "HOA_DON_THANH_TOAN";
    public const string HoaDonCuaToi = "HOA_DON_CUA_TOI";

    public const string BaoCaoTongQuan = "BAO_CAO_TONG_QUAN";
    public const string XuatHoSoTamTru = "XUAT_HO_SO_TAM_TRU";

    public const string LichSuCuTruLay = "LICH_SU_CU_TRU_LAY";
    public const string XuatLichSuCuTru = "XUAT_LICH_SU_CU_TRU";

    // Quản lý phân quyền động — CHỈ Chủ trọ (ChuTro) được gọi.
    public const string PhanQuyenLayMaTran = "PHAN_QUYEN_LAY_MA_TRAN";
    public const string PhanQuyenCapNhatVaiTro = "PHAN_QUYEN_CAP_NHAT_VAI_TRO";

    // Tự đăng ký khách thuê & nhận phòng quét QR (BR-17..BR-19).
    // DANG_KY là hành động công khai (không cần token) — xem DieuPhoiYeuCau.
    public const string DangKy = "DANG_KY";
    public const string HopDongSinhQr = "HOP_DONG_SINH_QR";
    public const string KhachThueNhanPhongQr = "KHACH_THUE_NHAN_PHONG_QR";

    /// <summary>Toàn bộ tên hanh_dong hợp lệ — dùng cho test và kiểm tra router.</summary>
    public static readonly IReadOnlyList<string> All =
    [
        DangNhap,
        PhongLayTatCa, PhongThem, PhongCapNhat, PhongXoa,
        KhachThueTheoPhong, KhachThueThem, KhachThueCapNhat, KhachThueTraPhong, KhachThueXoa,
        HopDongTao, HopDongGiaHan, HopDongChamDut, HopDongLayTatCa,
        DienNuocLayKyTruoc, DienNuocGhiSo,
        HoaDonTao, HoaDonLayTatCa, HoaDonThanhToan, HoaDonCuaToi,
        BaoCaoTongQuan, XuatHoSoTamTru,
        LichSuCuTruLay, XuatLichSuCuTru,
        PhanQuyenLayMaTran, PhanQuyenCapNhatVaiTro,
        DangKy, HopDongSinhQr, KhachThueNhanPhongQr,
    ];
}
