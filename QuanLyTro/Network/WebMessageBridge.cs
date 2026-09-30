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
