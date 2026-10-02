using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Network;
using QuanLyTro.Protocol;
using QuanLyTro.Shared.Models;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Tests;

/// <summary>
/// Test <see cref="TcpClientService"/> chống lại một stub TcpListener — không cần server thật.
/// </summary>
[TestClass]
public sealed class TcpClientServiceTests
{
    /// <summary>Mở listener trên cổng trống, chạy handler cho từng kết nối tới.</summary>
    private static async Task<TcpListener> StartStubAsync(
        Func<string, Task<string>> onLine, CancellationToken ct)
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        _ = Task.Run(async () =>
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var client = await listener.AcceptTcpClientAsync(ct);
                    _ = Task.Run(async () =>
                    {
                        using var c = client;
                        using var stream = c.GetStream();
                        using var reader = new StreamReader(stream, Encoding.UTF8);
                        while (await reader.ReadLineAsync(ct) is { } line)
                        {
                            var reply = await onLine(line);
                            var bytes = Encoding.UTF8.GetBytes(reply + "\n");
                            await stream.WriteAsync(bytes, ct);
                            await stream.FlushAsync(ct);
                        }
                    }, ct);
                }
            }
            catch (Exception) { /* listener dừng — hết test */ }
        }, ct);
        return listener;
    }

    private static int PortOf(TcpListener listener) =>
        ((IPEndPoint)listener.LocalEndpoint).Port;

    [TestMethod]
    public async Task TcpClientService_DeserializesTypedResponse()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        string? receivedLine = null;
        var phong = new List<PhongDto>
        {
            new(1, "P.101", 1_500_000m, 3, TrangThaiPhong.DaThue, "Tầng 1", 2),
            new(2, "P.102", 1_700_000m, 2, TrangThaiPhong.Trong, null),
        };

        var listener = await StartStubAsync(line =>
        {
            receivedLine = line;
            return Task.FromResult(JsonSerializer.Serialize(
                ResponsePacket.Ok(phong), JsonDefaults.Options));
        }, cts.Token);

        var client = new TcpClientService();
        await client.ConnectAsync("127.0.0.1", PortOf(listener), cts.Token);

        var result = await client.SendAsync<YeuCauDangNhap, List<PhongDto>>(
            ActionNames.PhongLayTatCa, new YeuCauDangNhap("admin", "123456"), cts.Token);

        listener.Stop();
        client.Disconnect();

        Assert.IsNotNull(receivedLine);
        var request = JsonSerializer.Deserialize<RequestPacket>(receivedLine, JsonDefaults.Options)!;
        Assert.AreEqual(ActionNames.PhongLayTatCa, request.Action);
        Assert.AreEqual("admin", request.GetData<YeuCauDangNhap>().TenDangNhap);

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("P.101", result[0].SoPhong);
        Assert.AreEqual(TrangThaiPhong.DaThue, result[0].TrangThai);
        Assert.AreEqual(1_500_000m, result[0].GiaThue);
        Assert.IsNull(result[1].MoTa);
    }

    [TestMethod]
    public async Task TcpClientService_ThrowsClientRequestException_WithVerbatimServerMessage()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        const string serverMessage = "Số phòng đã tồn tại.";

        var listener = await StartStubAsync(_ => Task.FromResult(JsonSerializer.Serialize(
            ResponsePacket.Fail(serverMessage), JsonDefaults.Options)), cts.Token);

        var client = new TcpClientService();
        await client.ConnectAsync("127.0.0.1", PortOf(listener), cts.Token);

        var ex = await Assert.ThrowsExceptionAsync<ClientRequestException>(() =>
            client.SendAsync<YeuCauDangNhap, List<PhongDto>>(
                ActionNames.PhongThem, new YeuCauDangNhap("admin", "123456"), cts.Token));

        listener.Stop();
        client.Disconnect();

        Assert.AreEqual(serverMessage, ex.Message);
    }

    [TestMethod]
    public async Task TcpClientService_RaisesDisconnected_WhenServerClosesConnection()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        var disconnected = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        _ = Task.Run(async () =>
        {
            using var c = await listener.AcceptTcpClientAsync(cts.Token);
        }, cts.Token);

        var client = new TcpClientService();
        client.Disconnected += (_, _) => disconnected.TrySetResult(true);
        await client.ConnectAsync("127.0.0.1", PortOf(listener), cts.Token);

        // Server đóng socket ngay sau khi nhận kết nối; client phải phát hiện EOF.
        var ex = await Assert.ThrowsExceptionAsync<IOException>(() =>
            client.SendAsync<YeuCauDangNhap, KetQuaDangNhap>(
                ActionNames.DangNhap, new YeuCauDangNhap("admin", "123456"), cts.Token));

        Assert.IsNotNull(ex);
        Assert.IsTrue(await disconnected.Task.WaitAsync(TimeSpan.FromSeconds(10)),
            "Client phải phát event Disconnected khi gặp EOF.");

        listener.Stop();
        client.Disconnect();
    }

    [TestMethod]
    public async Task TcpClientService_KeepsToken_FromLoginResult()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

        var listener = await StartStubAsync(_ => Task.FromResult(JsonSerializer.Serialize(
            ResponsePacket.Ok(new KetQuaDangNhap("tok-abc", "Nguyễn Văn A", VaiTroNguoiDung.ChuTro)),
            JsonDefaults.Options)), cts.Token);

        var client = new TcpClientService();
        await client.ConnectAsync("127.0.0.1", PortOf(listener), cts.Token);

        var login = await client.SendAsync<YeuCauDangNhap, KetQuaDangNhap>(
            ActionNames.DangNhap, new YeuCauDangNhap("admin", "123456"), cts.Token);

        client.Token = login.Token;
        Assert.AreEqual("tok-abc", client.Token);
        Assert.AreEqual(VaiTroNguoiDung.ChuTro, login.VaiTro);

        listener.Stop();
        client.Disconnect();
    }
}
