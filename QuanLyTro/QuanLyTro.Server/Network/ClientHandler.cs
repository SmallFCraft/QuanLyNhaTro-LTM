using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Server.Network;

/// <summary>
/// Xử lý một kết nối TCP từ Client qua một luồng bất đồng bộ (Task độc lập, KHÔNG dùng `new Thread`).
/// Giao thức: JSON UTF-8 một dòng kết thúc bằng '\n', giới hạn 1 MiB mỗi gói tin.
/// Bắt ngoại lệ ở tầng ngoài cùng để một client hỏng không bao giờ làm sập Server.
/// </summary>
public sealed class ClientHandler(TcpClient client, DieuPhoiYeuCau router)
{
    public const int MaxPacketBytes = 1024 * 1024; // 1 MiB

    /// <summary>UTF-8 KHÔNG BOM — BOM ở đầu gói tin làm `JsonSerializer` phía Client đọc hỏng.</summary>
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    public async Task RunAsync(CancellationToken ct = default)
    {
        using (client)
        await using (var stream = client.GetStream())
        using (var reader = new StreamReader(stream, Utf8NoBom, detectEncodingFromByteOrderMarks: false, bufferSize: 8192, leaveOpen: true))
        using (var writer = new StreamWriter(stream, Utf8NoBom, bufferSize: 8192, leaveOpen: true) { AutoFlush = true })
        {
            while (!ct.IsCancellationRequested && client.Connected)
            {
                string? line;
                try
                {
                    line = await reader.ReadLineAsync(ct);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception)
                {
                    // Client đóng đột ngột (RST) hoặc đứt mạng: kết thúc phiên nhẹ nhàng.
                    break;
                }

                if (line is null)
                {
                    // EOF: client chủ động ngắt kết nối.
                    break;
                }

                if (Encoding.UTF8.GetByteCount(line) > MaxPacketBytes)
                {
                    await WriteResponseAsync(writer, ResponsePacket.Fail("Gói tin vượt quá giới hạn 1 MiB."));
                    continue;
                }

                ResponsePacket response;
                try
                {
                    var request = JsonSerializer.Deserialize<RequestPacket>(line, JsonDefaults.Options);
                    if (request is null)
                    {
                        response = ResponsePacket.Fail("Gói tin không hợp lệ.");
                    }
                    else
                    {
                        response = await router.HandleAsync(request, ct);
                    }
                }
                catch (JsonException)
                {
                    response = ResponsePacket.Fail("Định dạng JSON không hợp lệ.");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[ClientHandler] Lỗi không mong muốn: {ex}");
                    response = ResponsePacket.Fail("Lỗi hệ thống, vui lòng thử lại.");
                }

                try
                {
                    await WriteResponseAsync(writer, response);
                }
                catch (Exception)
                {
                    // Lỗi ghi ra socket (client ngắt giữa chừng): thoát vòng lặp.
                    break;
                }
            }
        }
    }

    private static async Task WriteResponseAsync(StreamWriter writer, ResponsePacket response)
    {
        var json = JsonSerializer.Serialize(response, JsonDefaults.Options);
        await writer.WriteLineAsync(json);
    }
}
