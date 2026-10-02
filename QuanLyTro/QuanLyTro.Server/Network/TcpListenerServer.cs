using System.Net;
using System.Net.Sockets;

namespace QuanLyTro.Server.Network;

/// <summary>
/// TCP listener đa kết nối: mỗi <see cref="TcpClient"/> được giao cho một <see cref="ClientHandler"/>
/// chạy song song trên thread-pool. Bind `IPAddress.Any` nên Client khác máy trong LAN vẫn vào được.
/// </summary>
public sealed class TcpListenerServer(DieuPhoiYeuCau router)
{
    public const int DefaultPort = 8888;

    /// <summary>Cổng thực tế đang lắng nghe — truyền 0 để OS cấp cổng ngẫu nhiên (dùng cho test).</summary>
    public int Port { get; private set; }

    /// <summary>
    /// Lắng nghe tới khi <paramref name="ct"/> bị hủy. Bind xong trước await đầu tiên nên <see cref="Port"/>
    /// đọc được ngay sau khi gọi mà không cần await.
    /// </summary>
    public async Task StartAsync(int port = DefaultPort, CancellationToken ct = default)
    {
        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Port = ((IPEndPoint)listener.LocalEndpoint).Port;
        Console.WriteLine($"Server đang lắng nghe tại 0.0.0.0:{Port}");

        var clients = new List<Task>();
        try
        {
            while (!ct.IsCancellationRequested)
            {
                TcpClient client;
                try
                {
                    client = await listener.AcceptTcpClientAsync(ct);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (SocketException ex)
                {
                    Console.Error.WriteLine($"Listener dừng: {ex.Message}");
                    break;
                }

                Console.WriteLine($"Client kết nối: {client.Client.RemoteEndPoint}");
                clients.Add(new ClientHandler(client, router).RunAsync(ct));
                clients.RemoveAll(t => t.IsCompleted);
            }
        }
        finally
        {
            listener.Stop();
            // ClientHandler tự bắt mọi ngoại lệ nên chỉ cần chờ các phiên đóng lại.
            await Task.WhenAll(clients);
        }
    }
}
