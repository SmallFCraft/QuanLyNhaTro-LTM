using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class TenantQrContractTests
{
    private static string Wwwroot =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "wwwroot");

    private static string Html => File.ReadAllText(Path.Combine(Wwwroot, "khachthue", "index.html"));
    private static string Js => File.ReadAllText(Path.Combine(Wwwroot, "khachthue", "js", "khach_thue.js"));

    [TestMethod]
    public void KhachThueHtml_LoadsJsqrVendorScript_AndHasCheckinPane()
    {
        var html = Html;
        Assert.IsTrue(html.Contains("vendor/jsqr.min.js"), "khachthue/index.html phải nạp jsqr.min.js từ vendor.");
        Assert.IsTrue(html.Contains("checkinQr()"), "khachthue/index.html cần nút kích hoạt quét QR nhận phòng.");
        Assert.IsTrue(html.Contains("tab-nhan-phong") || html.Contains("nhan-phong"),
            "khachthue/index.html cần màn hình chờ nhận phòng hiển thị khi khách chưa có phòng.");
    }

    [TestMethod]
    public void KhachThueJs_SupportsThreeCheckinModes_AndCallsAction()
    {
        var js = Js;
        Assert.IsTrue(js.Contains("KHACH_THUE_NHAN_PHONG_QR"), "khach_thue.js phải gọi hành động KHACH_THUE_NHAN_PHONG_QR.");
        Assert.IsTrue(js.Contains("jsQR"), "khach_thue.js phải giải mã ảnh bằng jsQR (tab tải ảnh / webcam).");
        Assert.IsTrue(js.Contains("getUserMedia") || js.Contains("BatCamera") || js.Contains("batCamera"),
            "khach_thue.js phải hỗ trợ quét bằng webcam.");
        Assert.IsTrue(js.Contains("Pin") || js.Contains("pin"), "khach_thue.js phải hỗ trợ nhập mã PIN 8 số thủ công.");
    }
}
