using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class LandlordQrContractTests
{
    private static string Wwwroot =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "wwwroot");

    private static string Html => File.ReadAllText(Path.Combine(Wwwroot, "chutro", "index.html"));
    private static string Js => File.ReadAllText(Path.Combine(Wwwroot, "chutro", "js", "hop_dong.js"));

    [TestMethod]
    public void ChutroHtml_LoadsQrcodeVendorScript()
    {
        var html = Html;
        Assert.IsTrue(html.Contains("vendor/qrcode.min.js"), "chutro/index.html phải nạp qrcode.min.js từ vendor.");
    }

    [TestMethod]
    public void HopDongJs_SupportsChoNhanPhong_AndQrAction()
    {
        var js = Js;
        Assert.IsTrue(js.Contains("ChoNhanPhong"), "hop_dong.js phải hỗ trợ trạng thái ChoNhanPhong.");
        Assert.IsTrue(js.Contains("'HOP_DONG_SINH_QR'"), "hop_dong.js phải gọi hành động HOP_DONG_SINH_QR.");
        Assert.IsTrue(js.Contains("showContractQr"), "hop_dong.js phải có hàm hiển thị QR/PIN cho chủ trọ.");
    }
}
