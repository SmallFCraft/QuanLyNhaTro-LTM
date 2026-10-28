using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuanLyTro.Server.Services;

/// <summary>Một giao dịch tiền vào đọc từ SePay (chỉ các trường đối soát cần).</summary>
public sealed record GiaoDichSepay(
    string Id,
    string NoiDung,
    decimal SoTien,
    DateTime ThoiDiem,
    string? MaThamChieu);

/// <summary>
/// Client đọc giao dịch từ SePay API v2 (https://developer.sepay.vn/vi/sepay-api/v2/giao-dich/danh-sach).
/// CHỈ đọc — không bao giờ ghi/hoàn tiền. Header xác thực: `Authorization: Bearer &lt;api_token&gt;`.
/// ponytail: không dùng thư viện SDK; endpoint là JSON thuần, 1 GET + 1 DTO là đủ.
/// </summary>
public interface ISePayClient
{
    Task<List<GiaoDichSepay>> LayGiaoDichVaoAsync(
        string apiToken, DateTime tu, DateTime den, CancellationToken ct = default);
}

/// <summary>Dùng khi router được dựng trong test — không có giao dịch nào.</summary>
public sealed class NullSePayClient : ISePayClient
{
    public Task<List<GiaoDichSepay>> LayGiaoDichVaoAsync(
        string apiToken, DateTime tu, DateTime den, CancellationToken ct = default) =>
        Task.FromResult(new List<GiaoDichSepay>());
}

public sealed class SePayClient(HttpClient http) : ISePayClient
{
    public const string BaseUrl = "https://userapi.sepay.vn/v2";

    public async Task<List<GiaoDichSepay>> LayGiaoDichVaoAsync(
        string apiToken, DateTime tu, DateTime den, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(apiToken))
        {
            throw new LoiNghiepVu("Chưa cấu hình API token SePay.");
        }

        var ket_qua = new List<GiaoDichSepay>();
        var page = 1;
        var tuStr = Uri.EscapeDataString(tu.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
        var denStr = Uri.EscapeDataString(den.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));

        while (true)
        {
            // transfer_type=in: chỉ tiền vào. per_page=100 là mức tối đa tài liệu cho phép.
            var url = $"{BaseUrl}/transactions"
                + $"?transfer_type=in&per_page=100&page={page}"
                + $"&transaction_date_from={tuStr}"
                + $"&transaction_date_to={denStr}"
                + "&transaction_date_sort=asc";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
            request.Headers.UserAgent.ParseAdd("QuanLyTro-Server/1.0");

            HttpResponseMessage response;
            try
            {
                response = await http.SendAsync(request, ct);
            }
            catch (HttpRequestException ex)
            {
                throw new LoiNghiepVu($"Không kết nối được SePay: {ex.Message}");
            }

            if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden)
            {
                throw new LoiNghiepVu("SePay từ chối API token. Kiểm tra lại token trong tab Cài đặt.");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                throw new LoiNghiepVu("SePay giới hạn 3 request/giây. Thử lại sau ít giây.");
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new LoiNghiepVu($"SePay trả lỗi {(int)response.StatusCode}.");
            }

            var body = await response.Content.ReadFromJsonAsync<SePayResponse>(JsonOptions, ct);
            var trang = body?.Data ?? [];
            if (trang.Count == 0)
            {
                break;
            }

            foreach (var t in trang)
            {
                if (!string.Equals(t.TransferType, "in", StringComparison.OrdinalIgnoreCase)) continue;
                if (string.IsNullOrWhiteSpace(t.Id)) continue;

                // Ngày sai định dạng: gán mốc `tu` (cận dưới của cửa sổ) thay vì `DateTime.Now`.
                // Gán Now sẽ làm MAX(thoi_diem) nhảy vọt lên hiện tại, bỏ qua toàn bộ giao dịch cũ hơn.
                var thoiDiem = DateTime.TryParse(t.TransactionDate, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var ngay) ? ngay : tu;

                ket_qua.Add(new GiaoDichSepay(
                    t.Id,
                    t.TransactionContent ?? string.Empty,
                    t.AmountIn,
                    thoiDiem,
                    t.ReferenceNumber));
            }

            if (trang.Count < 100)
            {
                break; // trang cuối
            }
            page++;
        }

        return ket_qua;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private sealed record SePayResponse(
        [property: JsonPropertyName("status")] string? Status,
        [property: JsonPropertyName("data")] List<SePayTransaction>? Data);

    private sealed record SePayTransaction(
        [property: JsonPropertyName("id")] string? Id,
        [property: JsonPropertyName("transaction_content")] string? TransactionContent,
        [property: JsonPropertyName("amount_in")] decimal AmountIn,
        [property: JsonPropertyName("transaction_date")] string? TransactionDate,
        [property: JsonPropertyName("reference_number")] string? ReferenceNumber,
        [property: JsonPropertyName("transfer_type")] string? TransferType);
}
