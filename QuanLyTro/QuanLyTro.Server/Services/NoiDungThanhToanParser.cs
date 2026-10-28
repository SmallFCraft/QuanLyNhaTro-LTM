using System.Text;
using System.Text.RegularExpressions;

namespace QuanLyTro.Server.Services;

/// <summary>Kết quả đọc nội dung chuyển khoản: mã phòng + kỳ cước.</summary>
public sealed record NoiDungThanhToan(string SoPhong, string KyCuoc);

/// <summary>
/// Đọc nội dung chuyển khoản do khách ghi (ngân hàng có thể chèn tiền tố, đổi hoa/thường, thêm dấu)
/// để tìm phòng + kỳ cước. Phòng được khớp theo DANH SÁCH PHÒNG THẬT thay vì đoán mẫu chữ:
/// nội dung ngân hàng đầy mã giao dịch (MBVCB.123.456) nên đoán mẫu sẽ bắt nhầm.
/// </summary>
public static class NoiDungThanhToanParser
{
    // Kỳ cước: chấp nhận YYYY-MM, MM-YYYY, MMYYYY, YYYYMM (tháng 1-12, năm 20xx).
    private static readonly Regex[] MauKy =
    [
        new(@"(?<!\d)(?<nam>20\d{2})[-/. ]?(?<thang>0?[1-9]|1[0-2])(?!\d)", RegexOptions.Compiled),
        new(@"(?<!\d)(?<thang>0?[1-9]|1[0-2])[-/. ]?(?<nam>20\d{2})(?!\d)", RegexOptions.Compiled),
        new(@"(?<!\d)(?<thang>0?[1-9]|1[0-2])(?<nam>20\d{2})(?!\d)", RegexOptions.Compiled),
    ];

    /// <summary>
    /// null khi không đọc được phòng hoặc kỳ. <paramref name="soPhongHopLe"/> là số phòng đang có
    /// hóa đơn chưa thu — chỉ những phòng này mới được coi là khớp.
    /// </summary>
    public static NoiDungThanhToan? Doc(string? noiDung, IEnumerable<string> soPhongHopLe)
    {
        if (string.IsNullOrWhiteSpace(noiDung)) return null;

        var sach = ChuanHoa(noiDung);
        var khong_khoang_trang = sach.Replace(" ", string.Empty);

        // Phòng dài nhất khớp trước: "P102" phải thắng "P10" nếu cả hai đều tồn tại.
        var phong = soPhongHopLe
            .Select(p => (Goc: p, Gon: ChuanHoa(p).Replace(" ", string.Empty)))
            .Where(p => p.Gon.Length > 0
                && khong_khoang_trang.Contains(p.Gon, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(p => p.Gon.Length)
            .Select(p => p.Goc)
            .FirstOrDefault();

        if (phong is null) return null;

        var ky = TimKy(sach);
        return ky is null ? null : new NoiDungThanhToan(phong.ToUpperInvariant(), ky);
    }

    /// <summary>Chỉ đọc kỳ cước, không cần biết phòng — dùng khi đã có danh sách hóa đơn cần dò.</summary>
    public static string? TimKyTuNoiDung(string? noiDung) =>
        string.IsNullOrWhiteSpace(noiDung) ? null : TimKy(ChuanHoa(noiDung));

    /// <summary>Bỏ dấu tiếng Việt, gộp khoảng trắng — để so khớp không phụ thuộc dấu.</summary>
    public static string ChuanHoa(string raw)
    {
        var khong_dau = new StringBuilder(raw.Length);
        foreach (var ch in raw.Normalize(NormalizationForm.FormD))
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch)
                == System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                continue;
            }
            khong_dau.Append(ch switch { 'đ' => 'd', 'Đ' => 'D', _ => ch });
        }
        return Regex.Replace(khong_dau.ToString(), @"\s+", " ").Trim();
    }

    /// <summary>"09-2026" / "9/2026" / "092026" / "2026-09" / "202609" → "2026-09".</summary>
    public static string? ChuanHoaKy(string raw)
    {
        var so = Regex.Replace(raw, @"\D", string.Empty);

        // 4 chữ số đầu là năm hợp lệ → YYYYMM; ngược lại 4 chữ số cuối là năm → MMYYYY.
        if (so.Length is >= 5 and <= 6)
        {
            if (LaNam(so[..4])) return Dung(so[..4], so[4..]);
            if (LaNam(so[^4..])) return Dung(so[^4..], so[..^4]);
        }
        if (so.Length is 4)
        {
            // "0926" → tháng 09 năm 2026.
            return Dung("20" + so[2..], so[..2]);
        }
        return null;

        static bool LaNam(string s) => int.TryParse(s, out var nam) && nam is >= 2000 and <= 2999;

        static string? Dung(string nam, string thang) =>
            int.TryParse(nam, out var n) && int.TryParse(thang, out var t) && t is >= 1 and <= 12
                ? $"{n:D4}-{t:D2}"
                : null;
    }

    private static string? TimKy(string sach)
    {
        foreach (var mau in MauKy)
        {
            var m = mau.Match(sach);
            if (m.Success)
            {
                var ky = ChuanHoaKy($"{m.Groups["nam"].Value}{m.Groups["thang"].Value}");
                if (ky is not null) return ky;
            }
        }
        return null;
    }
}
