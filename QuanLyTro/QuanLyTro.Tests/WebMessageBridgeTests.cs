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

    [TestMethod]
    public async Task DispatchAsync_WhenServerReturnsNullData_SerializesSafeDataNull()
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
                    // Mô phỏng server trả về Data = null (ví dụ: UTILITY_GET_PREVIOUS)
                    var nullDataResponse = ResponsePacket.Ok<object?>(null);
                    var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(nullDataResponse, JsonDefaults.Options) + "\n");
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
            requestId = "req_util_prev",
            action = ActionNames.UtilityGetPrevious,
            data = new { roomId = 1, billingMonth = "2026-09" }
        });

        await bridge.DispatchAsync(rawJsRequest, msg =>
        {
            jsReturnedJson = msg;
            return Task.CompletedTask;
        });

        listener.Stop();
        tcpClient.Disconnect();

        Assert.IsNotNull(jsReturnedJson);
        using var doc = JsonDocument.Parse(jsReturnedJson);
        var root = doc.RootElement;
        Assert.IsTrue(root.GetProperty("success").GetBoolean(), "Phải trả về success: true");
        Assert.AreEqual("req_util_prev", root.GetProperty("requestId").GetString());
        Assert.AreEqual(JsonValueKind.Null, root.GetProperty("data").ValueKind, "data phải là null trên JSON, không văng InvalidOperationException");
    }

    [TestMethod]
    public async Task DispatchAsync_UiLogout_ClearsClientToken()
    {
        var client = new TcpClientService { Token = "test_token" };
        var bridge = new WebMessageBridge(client);
        string? responseJson = null;

        await bridge.DispatchAsync(
            """{"requestId":"r_out","action":"UI_LOGOUT","data":{}}""",
            msg => { responseJson = msg; return Task.CompletedTask; });

        Assert.IsNull(client.Token, "UI_LOGOUT phải xóa token");
        StringAssert.Contains(responseJson, "\"success\":true");
    }

    /// <summary>
    /// Mô phỏng đầy đủ luồng thực tế của người dùng:
    /// 1. Đăng nhập admin qua AUTH_LOGIN (bridge lưu token nội bộ)
    /// 2. Chuyển sang tab "Điện Nước": gọi ROOM_GET_ALL
    /// 3. Gọi UTILITY_GET_PREVIOUS cho phòng chưa có chỉ số kỳ trước (trả Data = null)
    /// Toàn bộ không được ném InvalidOperationException hay trả success: false.
    /// </summary>
    [TestMethod]
    public async Task FullLandlordUtilityTabFlow_ThroughBridge_SucceedsWithoutException()
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

                while (!cts.Token.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync(cts.Token);
                    if (line is null) break;

                    var req = JsonSerializer.Deserialize<RequestPacket>(line, JsonDefaults.Options)!;
                    ResponsePacket resp = req.Action switch
                    {
                        ActionNames.AuthLogin => ResponsePacket.Ok(new LoginResult("adm-token-999", "Chủ trọ Admin", UserRole.Landlord)),
                        ActionNames.RoomGetAll => ResponsePacket.Ok(new[] { new RoomDto(101, "Phòng 101", 1500000m, 2, RoomStatus.Available, null) }),
                        ActionNames.UtilityGetPrevious => ResponsePacket.Ok<UtilityReadingDto?>(null),
                        _ => ResponsePacket.Fail("Unknown")
                    };

                    var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(resp, JsonDefaults.Options) + "\n");
                    await stream.WriteAsync(bytes, cts.Token);
                    await stream.FlushAsync(cts.Token);
                }
            }
            catch { }
        }, cts.Token);

        var tcpClient = new TcpClientService();
        await tcpClient.ConnectAsync("127.0.0.1", port, cts.Token);
        var bridge = new WebMessageBridge(tcpClient);

        // Bước 1: Login
        string? loginResp = null;
        await bridge.DispatchAsync(JsonSerializer.Serialize(new
        {
            requestId = "r_login",
            action = ActionNames.AuthLogin,
            data = new { Username = "admin", Password = "123" }
        }), msg => { loginResp = msg; return Task.CompletedTask; });

        Assert.IsNotNull(loginResp);
        using (var doc = JsonDocument.Parse(loginResp))
        {
            Assert.IsTrue(doc.RootElement.GetProperty("success").GetBoolean());
            Assert.AreEqual("Landlord", doc.RootElement.GetProperty("data").GetProperty("role").GetString());
        }

        // Bước 2: Chuyển sang tab Điện Nước -> gọi ROOM_GET_ALL
        string? roomsResp = null;
        await bridge.DispatchAsync(JsonSerializer.Serialize(new
        {
            requestId = "r_rooms",
            action = ActionNames.RoomGetAll,
            data = new { }
        }), msg => { roomsResp = msg; return Task.CompletedTask; });

        Assert.IsNotNull(roomsResp);
        using (var doc = JsonDocument.Parse(roomsResp))
        {
            Assert.IsTrue(doc.RootElement.GetProperty("success").GetBoolean());
            Assert.AreEqual(JsonValueKind.Array, doc.RootElement.GetProperty("data").ValueKind);
        }

        // Bước 3: LoadPreviousReading -> UTILITY_GET_PREVIOUS trả null
        string? utilResp = null;
        await bridge.DispatchAsync(JsonSerializer.Serialize(new
        {
            requestId = "r_util",
            action = ActionNames.UtilityGetPrevious,
            data = new { roomId = 101, billingMonth = "2026-09" }
        }), msg => { utilResp = msg; return Task.CompletedTask; });

        Assert.IsNotNull(utilResp);
        using (var doc = JsonDocument.Parse(utilResp))
        {
            Assert.IsTrue(doc.RootElement.GetProperty("success").GetBoolean(),
                $"UTILITY_GET_PREVIOUS phải thành công nhưng bị lỗi: {doc.RootElement.GetProperty("error").GetString()}");
            Assert.AreEqual(JsonValueKind.Null, doc.RootElement.GetProperty("data").ValueKind);
        }

        listener.Stop();
        tcpClient.Disconnect();
    }
}
