using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Server;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Tests;

[TestClass]
[DoNotParallelize]
public sealed class ServerLogTests
{
    private static string RepoRoot =>
        Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", ".."));

    [TestMethod]
    public void FormatRequest_OkLine_ChứaEndpointActionVàThờiGian()
    {
        var line = ServerLog.FormatRequest(
            "127.0.0.1:52735", ActionNames.KhachThueNhanPhongQr, success: true, message: null, elapsedMs: 12);

        StringAssert.Contains(line, "127.0.0.1:52735");
        StringAssert.Contains(line, ActionNames.KhachThueNhanPhongQr);
        StringAssert.Contains(line, "OK");
        StringAssert.Contains(line, "12 ms");
        Assert.IsFalse(line.Contains("FAIL"), "Dòng thành công không được mang nhãn FAIL.");
    }

    [TestMethod]
    public void FormatRequest_FailLine_IncludesMessageTiếngViệtChoDebugger()
    {
        var line = ServerLog.FormatRequest(
            "127.0.0.1:52735", ActionNames.DangNhap, success: false,
            message: "Không có quyền.", elapsedMs: 3);

        StringAssert.Contains(line, "FAIL");
        StringAssert.Contains(line, "Không có quyền.");
        StringAssert.Contains(line, "WARN", "Yêu cầu thất bại phải nổi bậc hơn yêu cầu thành công.");
    }

    [TestMethod]
    public void ServerLog_MặcĐịnhTắt_ĐểDotnetTestKhôngNgậpOutput()
    {
        var saved = Console.Out;
        var savedEnabled = ServerLog.Enabled;
        try
        {
            ServerLog.Enabled = false;
            var buffer = new StringWriter();
            Console.SetOut(buffer);
            ServerLog.Request("127.0.0.1:1", ActionNames.DangNhap, true, null, 1);
            Assert.AreEqual(string.Empty, buffer.ToString(), "Khi chưa bật, ServerLog phải im lặng tuyệt đối.");

            ServerLog.Enabled = true;
            ServerLog.Request("127.0.0.1:1", ActionNames.DangNhap, true, null, 1);
            StringAssert.Contains(buffer.ToString(), ActionNames.DangNhap);
        }
        finally
        {
            Console.SetOut(saved);
            ServerLog.Enabled = savedEnabled;
        }
    }

    [TestMethod]
    public void ServerLog_KhôngBaoGiờSerializePayload_BecauseChứaMatKhauVàMãQR()
    {
        var source = File.ReadAllText(Path.Combine(RepoRoot, "QuanLyTro", "QuanLyTro.Server", "ServerLog.cs"));
        Assert.IsFalse(source.Contains("JsonSerializer"),
            "ServerLog không được serialize gói tin — payload chứa MatKhau, ma_qr_token, ma_pin.");
    }

    [TestMethod]
    public void ClientHandlerVàRouter_GhiLogMỗiYêuCầuVàKhiTừChốiQuyền()
    {
        var handler = File.ReadAllText(Path.Combine(
            RepoRoot, "QuanLyTro", "QuanLyTro.Server", "Network", "ClientHandler.cs"));
        Assert.IsTrue(handler.Contains("ServerLog.Request("),
            "ClientHandler phải log mỗi yêu cầu (endpoint, action, kết quả, thời gian).");

        var router = File.ReadAllText(Path.Combine(
            RepoRoot, "QuanLyTro", "QuanLyTro.Server", "Network", "DieuPhoiYeuCau.cs"));
        Assert.IsTrue(router.Contains("ServerLog.Warn("),
            "Router phải log khi từ chối quyền — đây là lỗi hay gặp nhất khi soi bug.");
    }
}
