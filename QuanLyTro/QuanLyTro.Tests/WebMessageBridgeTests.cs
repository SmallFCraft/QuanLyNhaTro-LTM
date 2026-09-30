using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Network;
using QuanLyTro.Shared.Models;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class WebMessageBridgeTests
{
    [TestMethod]
    public async Task ParseAndDispatch_InvalidJson_ReturnsErrorEnvelope()
    {
        string? returnedJson = null;
        var bridge = new WebMessageBridge(client: null!);

        await bridge.DispatchAsync("invalid json", msg =>
        {
            returnedJson = msg;
            return Task.CompletedTask;
        });

        Assert.IsNotNull(returnedJson);
        using var doc = JsonDocument.Parse(returnedJson);
        Assert.IsFalse(doc.RootElement.GetProperty("success").GetBoolean());
        Assert.IsTrue(doc.RootElement.GetProperty("error").GetString()!.Contains("JSON"));
    }

    /// <summary>
    /// C1 + C2: AUTH_LOGIN thành công → bridge nạp `_client.Token` phía C#, đồng thời
    /// xoá trường `token` khỏi JSON trả về cho JS (chỉ còn fullName, role).
    /// </summary>
    [TestMethod]
    public async Task AuthLogin_CapturesTokenInCSharp_AndStripsFromJsPayload()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        _ = Task.Run(async () =>
        {
            try
            {
                using var client = await listener.AcceptTcpClientAsync(cts.Token);
                using var stream = client.GetStream();
                using var reader = new StreamReader(stream, Encoding.UTF8);
                var line = await reader.ReadLineAsync(cts.Token);
                if (line is not null)
                {
                    var okResponse = ResponsePacket.Ok(new LoginResult("secret-jwt-token-123", "Thiếu úy Nguyễn Văn A", UserRole.Police));
                    var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(okResponse, JsonDefaults.Options) + "\n");
                    await stream.WriteAsync(bytes, cts.Token);
                    await stream.FlushAsync(cts.Token);
                }
            }
            catch { }
        }, cts.Token);

        var tcpClient = new TcpClientService();
        await tcpClient.ConnectAsync("127.0.0.1", port, cts.Token);
        var bridge = new WebMessageBridge(tcpClient);

        string? jsReturnedJson = null;
        var rawJsRequest = JsonSerializer.Serialize(new
        {
            requestId = "req_1",
            action = ActionNames.AuthLogin,
            data = new { Username = "police1", Password = "secret-pass" }
        });

        await bridge.DispatchAsync(rawJsRequest, msg =>
        {
            jsReturnedJson = msg;
            return Task.CompletedTask;
        });

        // C1: token đã được gán vào C# client (đọc trước khi Disconnect() xoá Token)
        Assert.AreEqual("secret-jwt-token-123", tcpClient.Token, "C1: _client.Token phải được gán từ AUTH_LOGIN.");

        listener.Stop();
        tcpClient.Disconnect();

        // C2: JS nhận được fullName và role, nhưng KHÔNG có token
        Assert.IsNotNull(jsReturnedJson);
        using var doc = JsonDocument.Parse(jsReturnedJson);
        var root = doc.RootElement;
        Assert.IsTrue(root.GetProperty("success").GetBoolean());
        Assert.AreEqual("req_1", root.GetProperty("requestId").GetString());

        var data = root.GetProperty("data");
        Assert.IsFalse(data.TryGetProperty("token", out _), "C2: trường 'token' không được lộ xuống JS.");
        Assert.IsFalse(data.TryGetProperty("Token", out _), "C2: trường 'Token' không được lộ xuống JS.");
        Assert.AreEqual("Thiếu úy Nguyễn Văn A", data.GetProperty("fullName").GetString());
        Assert.AreEqual("Police", data.GetProperty("role").GetString());
    }
}
