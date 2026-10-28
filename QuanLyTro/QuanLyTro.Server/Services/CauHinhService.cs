using System.Text;
using QuanLyTro.Server.Repositories;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Services;

/// <summary>
/// Cấu hình nhận tiền cho QR SePay. Ràng buộc lấy từ tài liệu https://developer.sepay.vn/vi/tien-ich-khac/tao-qr-code:
/// `acc` chỉ chữ/số tối đa 19 ký tự, `holder` ASCII in hoa không dấu tối đa 80 ký tự,
/// `template` thuộc {rỗng, compact, qronly, standee}.
/// </summary>
public sealed class CauHinhService(ICauHinhRepository cau_hinh)
{
    public const int SoTaiKhoanMaxLength = 19;
    public const int ChuTaiKhoanMaxLength = 80;
    public const int NganHangMaxLength = 50;
    public const int ApiTokenMaxLength = 255;

    public Task<CauHinhThanhToanDto> GetAsync(CancellationToken ct = default) => cau_hinh.GetAsync(ct);

    /// <summary>Chỉ Chủ trọ gọi (route đã chặn) — trả kèm api_token để form cài đặt hiển thị.</summary>
    public Task<CauHinhDayDuDto> GetFullAsync(CancellationToken ct = default) => cau_hinh.GetFullAsync(ct);

    /// <param name="api_token">null = giữ token cũ; "" = xoá token; chuỗi khác = đặt token mới.</param>
    public async Task<CauHinhThanhToanDto> UpdateAsync(
        CauHinhThanhToanDto request, string? api_token = null, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var ngan_hang = (request.NganHang ?? string.Empty).Trim();
        if (ngan_hang.Length > NganHangMaxLength)
        {
            throw new LoiNghiepVu($"Mã ngân hàng tối đa {NganHangMaxLength} ký tự.");
        }

        // STK để trống = tắt QR (chủ trọ chưa có tài khoản). Có nhập thì phải đúng định dạng SePay.
        var so_tai_khoan = (request.SoTaiKhoan ?? string.Empty).Trim();
        if (so_tai_khoan.Length > 0 && (!so_tai_khoan.All(char.IsAsciiLetterOrDigit) || so_tai_khoan.Length > SoTaiKhoanMaxLength))
        {
            throw new LoiNghiepVu($"Số tài khoản chỉ gồm chữ và số, tối đa {SoTaiKhoanMaxLength} ký tự.");
        }

        if (so_tai_khoan.Length > 0 && ngan_hang.Length == 0)
        {
            throw new LoiNghiepVu("Có số tài khoản thì phải chọn ngân hàng.");
        }

        var chu_tai_khoan = ChuanHoaChuTaiKhoan(request.ChuTaiKhoan);
        var template = (request.TemplateQr ?? "compact").Trim();
        if (!CauHinhThanhToanDto.TemplateHopLe.Contains(template, StringComparer.Ordinal))
        {
            throw new LoiNghiepVu("Mẫu QR không hợp lệ.");
        }

        // Token: null = giữ nguyên (form không gửi lại), "" = xoá, chuỗi khác = đặt mới.
        var token = api_token?.Trim();
        if (token is { Length: > ApiTokenMaxLength })
        {
            throw new LoiNghiepVu($"API token tối đa {ApiTokenMaxLength} ký tự.");
        }

        var da_luu = new CauHinhThanhToanDto(ngan_hang, so_tai_khoan, chu_tai_khoan, template);
        await cau_hinh.SaveAsync(da_luu, token, ct);
        return da_luu;
    }

    /// <summary>Bỏ dấu tiếng Việt, đ→D, in hoa, chỉ giữ chữ/số/khoảng trắng, cắt còn tối đa 80 ký tự.</summary>
    public static string ChuanHoaChuTaiKhoan(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

        var khong_dau = new StringBuilder(raw.Length);
        foreach (var ch in raw.Normalize(NormalizationForm.FormD))
        {
            // Loại dấu thanh (combining marks) sau khi tách NFD.
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch) == System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            khong_dau.Append(ch switch
            {
                'đ' => 'd',
                'Đ' => 'D',
                _ => ch
            });
        }

        var gon = new StringBuilder(khong_dau.Length);
        foreach (var ch in khong_dau.ToString().Normalize(NormalizationForm.FormC).ToUpperInvariant())
        {
            if (char.IsAsciiLetterOrDigit(ch) || ch == ' ')
            {
                gon.Append(ch);
            }
        }

        var ket_qua = string.Join(' ', gon.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        return ket_qua.Length > ChuTaiKhoanMaxLength ? ket_qua[..ChuTaiKhoanMaxLength].TrimEnd() : ket_qua;
    }
}
