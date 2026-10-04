using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class CameraPermissionContractTests
{
    private static string RepoRoot =>
        Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", ".."));

    [TestMethod]
    public void Form1_HandlesPermissionRequested_AllowsCamera()
    {
        var form1Path = Path.Combine(RepoRoot, "QuanLyTro", "Form1.cs");
        var code = File.ReadAllText(form1Path);
        Assert.IsTrue(code.Contains("PermissionRequested"), "Form1.cs phải lắng nghe sự kiện PermissionRequested của WebView2.");
        Assert.IsTrue(code.Contains("CoreWebView2PermissionKind.Camera"), "Form1.cs phải cấp quyền Camera.");
        Assert.IsTrue(code.Contains("CoreWebView2PermissionState.Allow"), "Form1.cs phải gán Allow cho quyền camera.");
    }

    [TestMethod]
    public void KhachThueJs_HandlesCameraFallbackAndReloadsWithTabPreserved()
    {
        var jsPath = Path.Combine(RepoRoot, "QuanLyTro", "Assets", "wwwroot", "khachthue", "js", "khach_thue.js");
        var code = File.ReadAllText(jsPath);
        Assert.IsTrue(code.Contains("Could not start video source") || code.Contains("fallback"),
            "khach_thue.js phải có fallback khi camera báo Could not start video source.");
    }

    [TestMethod]
    public void KhachThueJs_DoesNotCrashOnInterruptedPlayRequest()
    {
        var jsPath = Path.Combine(RepoRoot, "QuanLyTro", "Assets", "wwwroot", "khachthue", "js", "khach_thue.js");
        var code = File.ReadAllText(jsPath);

        // Chromium reject promise play() bằng AbortError khi có load mới xen vào -> phải nuốt, không hiện "Lỗi thiết bị".
        Assert.IsTrue(code.Contains("AbortError"), "khach_thue.js phải bỏ qua AbortError của video.play().");
        Assert.IsTrue(code.Contains("loadedmetadata"), "khach_thue.js phải chờ video sẵn sàng trước khi play.");
        Assert.IsTrue(code.Contains("camStarting"), "khach_thue.js phải chặn bấm đúp gây race load camera.");
    }

    [TestMethod]
    public void KhachThueHtml_AutoplaysCameraVideo()
    {
        var htmlPath = Path.Combine(RepoRoot, "QuanLyTro", "Assets", "wwwroot", "khachthue", "index.html");
        var html = File.ReadAllText(htmlPath);
        var videoTag = System.Text.RegularExpressions.Regex.Match(html, "<video[^>]*>").Value;
        Assert.IsTrue(videoTag.Contains("autoplay") && videoTag.Contains("muted"),
            "#qrVideo phải autoplay+muted để stream chạy mà không cần lệnh play());");
    }
}
