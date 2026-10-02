using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using QuanLyTro.Protocol;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Network;

/// <summary>
/// Kết nối TCP một kết nối lâu dài tới Server, tuần tự hóa request/response bằng semaphore.
/// Timeout 10 giây mỗi request qua linked cancellation token.
/// </summary>
public sealed class TcpClientService : IDisposable
{
    public const int MaxPacketBytes = 1024 * 1024; // 1 MiB
    public static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(10);

    private readonly SemaphoreSlim _gate = new(1, 1);
    private TcpClient? _client;
    private NetworkStream? _stream;
    private StreamReader? _reader;
    private int _disconnectedFired;

    /// <summary>Token phiên làm việc hiện tại, gửi kèm mỗi gói tin sau khi đăng nhập.</summary>
    public string? Token { get; set; }

    /// <summary>Sự kiện khi kết nối bị ngắt từ phía server (EOF hoặc lỗi stream).</summary>
    public event EventHandler? Disconnected;

    public bool IsConnected => _client?.Connected == true;

    /// <summary>Mở kết nối tới Server. Đóng kết nối cũ nếu có.</summary>
    public async Task ConnectAsync(string host, int port, CancellationToken ct = default)
    {
        Disconnect();
        _disconnectedFired = 0;

        var client = new TcpClient();
        try
        {
            await client.ConnectAsync(host, port, ct);
            _client = client;
            _stream = client.GetStream();
            _reader = new StreamReader(_stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 8192, leaveOpen: true);
        }
        catch
        {
            client.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Gửi một hanh_dong với payload, đợi phản hồi đúng hạn 10s.
    /// Thread-safe: Semaphore(1,1) bảo đảm không bao giờ xáo trộn cặp request/response.
    /// </summary>
    public async Task<TResp> SendAsync<TReq, TResp>(string hanh_dong, TReq data, CancellationToken ct = default)
    {
        if (_stream is null || _reader is null || _client is null || !_client.Connected)
        {
            throw new InvalidOperationException("Chưa kết nối tới server.");
        }

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        linkedCts.CancelAfter(RequestTimeout);
        var token = linkedCts.Token;

        await _gate.WaitAsync(token);
        try
        {
            var packet = RequestPacket.Create(hanh_dong, Token, data);
            var json = JsonSerializer.Serialize(packet, JsonDefaults.Options);
            var payload = Encoding.UTF8.GetBytes(json + "\n");
            if (payload.Length > MaxPacketBytes)
            {
                throw new InvalidOperationException($"Gói tin vượt quá giới hạn 1 MiB ({payload.Length} bytes).");
            }

            await _stream.WriteAsync(payload, token);
            await _stream.FlushAsync(token);

            // ponytail: ReadLineAsync đủ tốt cho packet 1 dòng \n < 1MiB; giới hạn MaxPacketBytes ở phía gửi và kiểm tra độ dài chuỗi trả về
            var line = await _reader.ReadLineAsync(token);
            if (line is null)
            {
                FireDisconnected();
                throw new IOException("Server đã đóng kết nối (EOF).");
            }

            if (Encoding.UTF8.GetByteCount(line) > MaxPacketBytes)
            {
                throw new InvalidOperationException("Gói tin nhận từ server vượt quá 1 MiB.");
            }

            var response = JsonSerializer.Deserialize<ResponsePacket>(line, JsonDefaults.Options)
                ?? throw new JsonException("Phản hồi từ server không đúng định dạng.");

            if (!response.Success)
            {
                // Thông điệp tiếng Việt từ LoiNghiepVu được server giữ nguyên
                throw new ClientRequestException(response.Message);
            }

            return response.GetData<TResp>()!;
        }
        catch (Exception ex) when (ex is IOException or SocketException)
        {
            FireDisconnected();
            throw;
        }
        finally
        {
            _gate.Release();
        }
    }

    private void FireDisconnected()
    {
        if (Interlocked.Exchange(ref _disconnectedFired, 1) == 0)
        {
            try
            {
                Disconnected?.Invoke(this, EventArgs.Empty);
            }
            catch { /* handlers shouldn't crash teardown */ }
        }
    }

    /// <summary>Chủ động đóng kết nối.</summary>
    public void Disconnect()
    {
        try { _reader?.Dispose(); } catch { }
        try { _stream?.Dispose(); } catch { }
        try { _client?.Dispose(); } catch { }
        _reader = null;
        _stream = null;
        _client = null;
        Token = null;
    }

    public void Dispose()
    {
        Disconnect();
        _gate.Dispose();
    }
}
