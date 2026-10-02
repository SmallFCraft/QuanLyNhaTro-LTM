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
    /// C1 + C2: DANG_NHAP thành công → bridge nạp `_client.Token` phía C#, đồng thời
    /// xoá trường `token` khỏi JSON trả về cho JS (chỉ còn hoTen, vai_tro).
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
                    var okResponse = ResponsePacket.Ok(new KetQuaDangNhap("secret-jwt-token-123", "Thiếu úy Nguyễn Văn A", VaiTroNguoiDung.CongAn));
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
            hanh_dong = ActionNames.DangNhap,
            data = new { TenDangNhap = "police1", MatKhau = "secret-pass" }
        });

        await bridge.DispatchAsync(rawJsRequest, msg =>
        {
            jsReturnedJson = msg;
            return Task.CompletedTask;
        });

        // C1: token đã được gán vào C# client (đọc trước khi Disconnect() xoá Token)
        Assert.AreEqual("secret-jwt-token-123", tcpClient.Token, "C1: _client.Token phải được gán từ DANG_NHAP.");

        listener.Stop();
        tcpClient.Disconnect();

        // C2: JS nhận được hoTen và vai_tro, nhưng KHÔNG có token
        Assert.IsNotNull(jsReturnedJson);
        using var doc = JsonDocument.Parse(jsReturnedJson);
        var root = doc.RootElement;
        Assert.IsTrue(root.GetProperty("success").GetBoolean());
        Assert.AreEqual("req_1", root.GetProperty("requestId").GetString());

        var data = root.GetProperty("data");
        Assert.IsFalse(data.TryGetProperty("token", out _), "C2: trường 'token' không được lộ xuống JS.");
        Assert.IsFalse(data.TryGetProperty("Token", out _), "C2: trường 'Token' không được lộ xuống JS.");
        Assert.AreEqual("Thiếu úy Nguyễn Văn A", data.GetProperty("hoTen").GetString());
        Assert.AreEqual("CongAn", data.GetProperty("vai_tro").GetString());
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
                    // Mô phỏng server trả về Data = null (ví dụ: DIEN_NUOC_LAY_KY_TRUOC)
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
            hanh_dong = ActionNames.DienNuocLayKyTruoc,
            data = new { phongId = 1, kyCuoc = "2026-09" }
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
            """{"requestId":"r_out","hanh_dong":"UI_LOGOUT","data":{}}""",
            msg => { responseJson = msg; return Task.CompletedTask; });

        Assert.IsNull(client.Token, "UI_LOGOUT phải xóa token");
        StringAssert.Contains(responseJson, "\"success\":true");
    }

    /// <summary>
    /// Mô phỏng đầy đủ luồng thực tế của người dùng:
    /// 1. Đăng nhập admin qua DANG_NHAP (bridge lưu token nội bộ)
    /// 2. Chuyển sang tab "Điện Nước": gọi PHONG_LAY_TAT_CA
    /// 3. Gọi DIEN_NUOC_LAY_KY_TRUOC cho phòng chưa có chỉ số kỳ trước (trả Data = null)
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
                        ActionNames.DangNhap => ResponsePacket.Ok(new KetQuaDangNhap("adm-token-999", "Chủ trọ Admin", VaiTroNguoiDung.ChuTro)),
                        ActionNames.PhongLayTatCa => ResponsePacket.Ok(new[] { new PhongDto(101, "Phòng 101", 1500000m, 2, TrangThaiPhong.Trong, null) }),
                        ActionNames.DienNuocLayKyTruoc => ResponsePacket.Ok<ChiSoDienNuocDto?>(null),
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
            hanh_dong = ActionNames.DangNhap,
            data = new { TenDangNhap = "admin", MatKhau = "123" }
        }), msg => { loginResp = msg; return Task.CompletedTask; });

        Assert.IsNotNull(loginResp);
        using (var doc = JsonDocument.Parse(loginResp))
        {
            Assert.IsTrue(doc.RootElement.GetProperty("success").GetBoolean());
            Assert.AreEqual("ChuTro", doc.RootElement.GetProperty("data").GetProperty("vai_tro").GetString());
        }

        // Bước 2: Chuyển sang tab Điện Nước -> gọi PHONG_LAY_TAT_CA
        string? roomsResp = null;
        await bridge.DispatchAsync(JsonSerializer.Serialize(new
        {
            requestId = "r_rooms",
            hanh_dong = ActionNames.PhongLayTatCa,
            data = new { }
        }), msg => { roomsResp = msg; return Task.CompletedTask; });

        Assert.IsNotNull(roomsResp);
        using (var doc = JsonDocument.Parse(roomsResp))
        {
            Assert.IsTrue(doc.RootElement.GetProperty("success").GetBoolean());
            Assert.AreEqual(JsonValueKind.Array, doc.RootElement.GetProperty("data").ValueKind);
        }

        // Bước 3: LoadPreviousReading -> DIEN_NUOC_LAY_KY_TRUOC trả null
        string? utilResp = null;
        await bridge.DispatchAsync(JsonSerializer.Serialize(new
        {
            requestId = "r_util",
            hanh_dong = ActionNames.DienNuocLayKyTruoc,
            data = new { phongId = 101, kyCuoc = "2026-09" }
        }), msg => { utilResp = msg; return Task.CompletedTask; });

        Assert.IsNotNull(utilResp);
        using (var doc = JsonDocument.Parse(utilResp))
        {
            Assert.IsTrue(doc.RootElement.GetProperty("success").GetBoolean(),
                $"DIEN_NUOC_LAY_KY_TRUOC phải thành công nhưng bị lỗi: {doc.RootElement.GetProperty("error").GetString()}");
            Assert.AreEqual(JsonValueKind.Null, doc.RootElement.GetProperty("data").ValueKind);
        }

        listener.Stop();
        tcpClient.Disconnect();
    }
}
