using System;
using System.Text.Json;
using System.Threading.Tasks;
using QuanLyTro.Protocol;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Network;

/// <summary>
/// Cầu nối tiếp nhận thông điệp JSON từ JavaScript (WebView2) và định tuyến tới TcpClientService.
/// Định dạng JS gửi: { "requestId": "r1", "action": "AUTH_LOGIN", "data": { ... } }
/// Định dạng C# trả về: { "requestId": "r1", "success": true/false, "data": ..., "error": "..." }
/// </summary>
public sealed class WebMessageBridge
{
    private readonly TcpClientService _client;

    public WebMessageBridge(TcpClientService client)
    {
        _client = client;
    }

    public sealed record ClientEnvelope(string? RequestId, string? Action, JsonElement Data);

    public async Task DispatchAsync(string rawJson, Func<string, Task> postBack)
    {
        string? reqId = null;
        try
        {
            var env = JsonSerializer.Deserialize<ClientEnvelope>(rawJson, JsonDefaults.Options);
            if (env is null || string.IsNullOrWhiteSpace(env.Action))
            {
                await PostErrorAsync(postBack, null, "Gói tin JSON không hợp lệ hoặc thiếu Action.");
                return;
            }

            reqId = env.RequestId;
            var response = await _client.SendAsync<JsonElement, JsonElement>(env.Action, env.Data);

            // Ràng buộc spec §3.2: token phiên sống trong C#, KHÔNG xuống JS.
            // Giữ token cho các request sau, trả JS chỉ phần login.js cần (fullName, role).
            if (env.Action == ActionNames.AuthLogin)
            {
                response = CaptureTokenAndStrip(response);
            }

            var okPayload = JsonSerializer.Serialize(new
            {
                requestId = reqId,
                success = true,
                data = response,
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
        string? fullName = null;
        JsonElement roleElement = default;

        if (response.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in response.EnumerateObject())
            {
                if (prop.NameEquals("token") || string.Equals(prop.Name, "Token", StringComparison.OrdinalIgnoreCase))
                {
                    token = prop.Value.GetString();
                }
                else if (prop.NameEquals("fullName") || string.Equals(prop.Name, "FullName", StringComparison.OrdinalIgnoreCase))
                {
                    fullName = prop.Value.GetString();
                }
                else if (prop.NameEquals("role") || string.Equals(prop.Name, "Role", StringComparison.OrdinalIgnoreCase))
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
            fullName,
            role = roleVal
        }, JsonDefaults.Options);
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
