using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class AuthShellContractTests
{
    private static string Wwwroot =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "wwwroot");

    private static string Html => File.ReadAllText(Path.Combine(Wwwroot, "auth", "index.html"));
    private static string Js => File.ReadAllText(Path.Combine(Wwwroot, "auth", "auth.js"));

    [TestMethod]
    public void AuthShell_HasRegisterTabAndForm()
    {
        var html = Html;
        Assert.IsTrue(html.Contains("switchAuthTab"), "auth shell thiếu điều khiển chuyển tab đăng nhập/đăng ký.");
        Assert.IsTrue(html.Contains("doRegister()"), "auth shell thiếu form gọi doRegister().");

        foreach (var field in new[] { "regHoTen", "regNgaySinh", "regCccd", "regSdt", "regQueQuan", "regNoiLamViec", "regMatKhau" })
        {
            Assert.IsTrue(html.Contains($"id=\"{field}\""), $"auth shell thiếu ô nhập {field}.");
        }
    }

    [TestMethod]
    public void AuthJs_SubmitsDangKyWithMatKhauAndFillsCccdOnSuccess()
    {
        var js = Js;
        Assert.IsTrue(js.Contains("'DANG_KY'"), "auth.js phải gọi hành động DANG_KY.");
        Assert.IsTrue(js.Contains("matKhau"), "auth.js phải gửi trường matKhau theo đặc tả.");
        Assert.IsTrue(js.Contains("Cccd"), "auth.js phải gửi CCCD của người đại diện.");
        Assert.IsTrue(js.Contains("lu"), "auth.js phải tự điền CCCD vào ô đăng nhập sau khi đăng ký thành công.");
    }

    /// <summary>Indicator kết nối Server: node có thật + auth.js probe định kỳ qua UI_SERVER_STATUS.</summary>
    [TestMethod]
    public void AuthShell_PollsServerConnectionIndicator()
    {
        var html = Html;
        Assert.IsTrue(html.Contains("id=\"srv-status\""), "auth shell thiếu indicator #srv-status.");

        var js = Js;
        Assert.IsTrue(js.Contains("'UI_SERVER_STATUS'"), "auth.js phải probe UI_SERVER_STATUS.");
        Assert.IsTrue(js.Contains("setInterval"), "auth.js phải poll trạng thái kết nối định kỳ.");
    }
}
