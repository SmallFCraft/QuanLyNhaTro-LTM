using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using QuanLyTro.Protocol;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Network;

/// <summary>
/// Cầu nối tiếp nhận thông điệp JSON từ JavaScript (WebView2) và định tuyến tới TcpClientService.
/// Định dạng JS gửi: { "requestId": "r1", "hanh_dong": "DANG_NHAP", "data": { ... } }
/// Định dạng C# trả về: { "requestId": "r1", "success": true/false, "data": ..., "error": "..." }
/// </summary>
public sealed class WebMessageBridge
{
    private readonly TcpClientService _client;

    public WebMessageBridge(TcpClientService client)
    {
        _client = client;
    }

    public sealed record ClientEnvelope(
        string? RequestId,
        [property: JsonPropertyName("action")] string? Action,
        [property: JsonPropertyName("hanh_dong")] string? HanhDong,
        JsonElement Data)
    {
        public string? EffectiveAction => !string.IsNullOrWhiteSpace(HanhDong) ? HanhDong : Action;
    }

    public async Task DispatchAsync(string rawJson, Func<string, Task> postBack)
    {
        string? reqId = null;
        try
        {
            var env = JsonSerializer.Deserialize<ClientEnvelope>(rawJson, JsonDefaults.Options);
            var action = env?.EffectiveAction;
            if (env is null || string.IsNullOrWhiteSpace(action))
            {
                await PostErrorAsync(postBack, null, "Gói tin JSON không hợp lệ hoặc thiếu Action.");
                return;
            }

            reqId = env.RequestId;

            // Action cục bộ: đăng xuất chỉ xóa token phiên trong C#, KHÔNG gửi lên TCP Server.
            if (action == "UI_LOGOUT")
            {
                _client.Token = null;
                var ok = JsonSerializer.Serialize(new { requestId = reqId, success = true, data = (object?)null, error = (string?)null }, JsonDefaults.Options);
                await postBack(ok);
                return;
            }

            // JS có thể không gửi trường `data`, gửi null, hoặc gửi giá trị không phải object.
            // JsonElement ở trạng thái Undefined/Null làm JsonSerializer.SerializeToElement ném
            // InvalidOperationException ("Operation is not valid due to the current state..."),
            // nên chuẩn hóa về object rỗng trước khi chuyển tiếp.
            var payload = NormalizeData(env.Data);

            var response = await _client.SendAsync<JsonElement, JsonElement>(action, payload);

            // Ràng buộc spec §3.2: token phiên sống trong C#, KHÔNG xuống JS.
            // Giữ token cho các request sau, trả JS chỉ phần login.js cần (hoTen, vai_tro).
            if (action == ActionNames.DangNhap)
            {
                response = CaptureTokenAndStrip(response);
            }

            // Khi Server trả về Data = null (vd DIEN_NUOC_LAY_KY_TRUOC cho phòng chưa có chỉ số kỳ trước):
            // TcpClientService.SendAsync trả về default(JsonElement) có ValueKind == Undefined.
            // JsonSerializer.Serialize gặp thuộc tính JsonElement(Undefined) sẽ ném:
            // "Operation is not valid due to the current state of the object."
            // Chuẩn hóa: nếu Undefined thì đưa về null để serialize ra "data": null an toàn cho JS.
            object? safeData = response.ValueKind == JsonValueKind.Undefined ? null : response;

            var okPayload = JsonSerializer.Serialize(new
            {
                requestId = reqId,
                success = true,
                data = safeData,
                error = (string?)null
            }, JsonDefaults.Options);

            await postBack(okPayload);
        }
        catch (ClientRequestException ex)
        {
            await PostErrorAsync(postBack, reqId, ex.Message);
        }
        catch (JsonException ex)
        {
            // JSON hỏng: báo riêng để JS hiển thị đúng, không gọi TcpClientService.
            await PostErrorAsync(postBack, reqId, "Gói tin JSON không hợp lệ: " + ex.Message);
        }
        catch (Exception ex)
        {
            await PostErrorAsync(postBack, reqId, "Lỗi kết nối hoặc hệ thống: " + ex.Message);
        }
    }

    private JsonElement CaptureTokenAndStrip(JsonElement response)
    {
        string? token = null;
        string? hoTen = null;
        JsonElement roleElement = default;

        if (response.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in response.EnumerateObject())
            {
                if (prop.NameEquals("token") || string.Equals(prop.Name, "Token", StringComparison.OrdinalIgnoreCase))
                {
                    token = prop.Value.GetString();
                }
                else if (prop.NameEquals("hoTen") || string.Equals(prop.Name, "HoTen", StringComparison.OrdinalIgnoreCase))
                {
                    hoTen = prop.Value.GetString();
                }
                else if (prop.NameEquals("vai_tro") || string.Equals(prop.Name, "VaiTro", StringComparison.OrdinalIgnoreCase))
                {
                    roleElement = prop.Value;
                }
            }
        }

        if (!string.IsNullOrEmpty(token) && _client is not null)
        {
            _client.Token = token;
        }

        object? roleVal = roleElement.ValueKind switch
        {
            JsonValueKind.String => roleElement.GetString(),
            JsonValueKind.Number => roleElement.GetInt32(),
            _ => null
        };

        return JsonSerializer.SerializeToElement(new
        {
            hoTen,
            vaiTro = roleVal
        }, JsonDefaults.Options);
    }

    private static JsonElement NormalizeData(JsonElement data)
    {
        // Undefined / Null làm SerializeToElement ném InvalidOperationException
        // ("Operation is not valid due to the current state of the object.").
        // Chuẩn hóa về object rỗng `{}` để luôn serialize an toàn.
        if (data.ValueKind == JsonValueKind.Undefined || data.ValueKind == JsonValueKind.Null)
        {
            return JsonSerializer.SerializeToElement(new { }, JsonDefaults.Options);
        }

        return data;
    }

    private static Task PostErrorAsync(Func<string, Task> postBack, string? requestId, string message)
    {
        var errPayload = JsonSerializer.Serialize(new
        {
            requestId,
            success = false,
            data = (object?)null,
            error = message
        }, JsonDefaults.Options);
        return postBack(errPayload);
    }
}
