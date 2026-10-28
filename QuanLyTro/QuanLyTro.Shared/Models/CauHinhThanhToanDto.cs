namespace QuanLyTro.Shared.Models;

/// <summary>
/// Cấu hình nhận tiền để dựng ảnh QR SePay (https://qr.sepay.vn/img — public, không cần token).
/// Server chuẩn hoá trước khi trả: STK chỉ chữ/số, chủ TK ASCII hoa bỏ dấu, template thuộc whitelist.
/// KHÔNG chứa API token — xem <see cref="CauHinhDayDuDto"/>.
/// </summary>
public sealed record CauHinhThanhToanDto(
    string NganHang,
    string SoTaiKhoan,
    string ChuTaiKhoan,
    string TemplateQr = "compact")
{
    /// <summary>True khi đã đủ ngân hàng + STK để dựng QR.</summary>
    public bool DaCauHinh => !string.IsNullOrWhiteSpace(NganHang) && !string.IsNullOrWhiteSpace(SoTaiKhoan);

    /// <summary>Giá trị hợp lệ của tham số `template` theo tài liệu SePay.</summary>
    public static readonly IReadOnlyList<string> TemplateHopLe = ["", "compact", "qronly", "standee"];
}

/// <summary>
/// Bản đầy đủ CHỈ trả cho Chủ trọ (CAU_HINH_DAY_DU): thêm API token và cờ báo đã có token.
/// Token không bao giờ nằm trong <see cref="CauHinhThanhToanDto"/> vì khách thuê cũng đọc DTO đó.
/// </summary>
public sealed record CauHinhDayDuDto(
    string NganHang,
    string SoTaiKhoan,
    string ChuTaiKhoan,
    string TemplateQr,
    string ApiToken,
    bool CoApiToken)
{
    public bool DaCauHinh => !string.IsNullOrWhiteSpace(NganHang) && !string.IsNullOrWhiteSpace(SoTaiKhoan);

    public static CauHinhDayDuDto TuCauHinh(CauHinhThanhToanDto cfg, string api_token) =>
        new(cfg.NganHang, cfg.SoTaiKhoan, cfg.ChuTaiKhoan, cfg.TemplateQr,
            api_token, !string.IsNullOrWhiteSpace(api_token));
}
