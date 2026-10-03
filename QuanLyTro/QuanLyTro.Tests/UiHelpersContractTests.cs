using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class UiHelpersContractTests
{
    private static string Wwwroot =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "wwwroot");

    [TestMethod]
    public void UiJs_ExposesSharedHelpers()
    {
        var js = File.ReadAllText(Path.Combine(Wwwroot, "shared", "js", "ui.js"));
        foreach (var fn in new[] { "function esc", "function fmtMoney", "function fmtDate",
                                   "function fmtDateOnly", "function toast", "function openModal",
                                   "function confirmDialog", "function alertDialog" })
        {
            Assert.IsTrue(js.Contains(fn), $"ui.js thiếu {fn}");
        }
    }

    /// <summary>
    /// Không còn hộp thoại mặc định của WebView2 (Chromium tự chèn tiêu đề
    /// "app.quanlytro.local says"). Mọi thông báo đi qua modal/toast nội bộ.
    /// </summary>
    [TestMethod]
    public void Frontend_DoesNotCallNativeAlertOrConfirm()
    {
        foreach (var file in Directory.GetFiles(Wwwroot, "*.js", SearchOption.AllDirectories))
        {
            var js = File.ReadAllText(file);
            var relative = Path.GetRelativePath(Wwwroot, file);
            Assert.IsFalse(Regex.IsMatch(js, @"(?<![\w.])alert\s*\("),
                $"{relative} còn gọi alert() gốc — dùng alertDialog()/toast().");
            Assert.IsFalse(Regex.IsMatch(js, @"(?<![\w.])confirm\s*\("),
                $"{relative} còn gọi confirm() gốc — dùng confirmDialog().");
        }
    }

    /// <summary>window.alert bị thay bằng toast nội bộ trong ui.js — không lộ tên miền.</summary>
    [TestMethod]
    public void UiJs_ReplacesNativeAlertWithToast()
    {
        var js = File.ReadAllText(Path.Combine(Wwwroot, "shared", "js", "ui.js"));
        Assert.IsTrue(js.Contains("window.alert ="), "ui.js chưa thay window.alert");
    }

    /// <summary>
    /// Mỗi trang actor (landlord/police/tenant) nạp ui.js và có #modal-root cho modal dùng chung.
    /// </summary>
    [TestMethod]
    public void ActorPages_LoadUiJs_AndHaveModalRoot()
    {
        foreach (var actor in new[] { "chutro", "congan", "khachthue" })
        {
            var html = File.ReadAllText(Path.Combine(Wwwroot, actor, "index.html"));
            Assert.IsTrue(html.Contains("shared/js/ui.js"), $"{actor}/index.html chưa nạp ui.js");
            Assert.IsTrue(html.Contains("id=\"modal-root\""), $"{actor}/index.html thiếu #modal-root");
        }
    }

    /// <summary>
    /// Task 9–13 đọc các phần tử này qua getElementById; thiếu id thì task sau không chạy được.
    /// Mỗi nhóm id phải nằm trong đúng trang của actor tương ứng.
    /// </summary>
    [TestMethod]
    public void ActorPages_HasAllScreenElementIds()
    {
        var landlord = File.ReadAllText(Path.Combine(Wwwroot, "chutro", "index.html"));
        string[] landlordIds =
        {
            // Tổng quan
            "kpi-total-phong", "kpi-available", "kpi-khach_thue", "kpi-unpaid",
            "dash-overdue-rows", "dash-expiring-rows",
            // Phòng
            "room-search", "room-trang_thai",
            // Người thuê
            "tenant-room", "khach_thue-body",
            // Hợp đồng
            "hop_dong-body",
            // Điện nước
            "util-room", "util-month", "util-elec-old", "util-elec-new", "util-elec-rate",
            "util-water-old", "util-water-new", "util-water-rate",
            // Hóa đơn
            "inv-month", "inv-room", "hoa_don-body",
            // Thống kê
            "rep-month",
            // Dùng chung
            "modal-root",
        };
        foreach (var id in landlordIds)
        {
            Assert.IsTrue(landlord.Contains($"id=\"{id}\""), $"chutro/index.html thiếu id=\"{id}\"");
        }

        var police = File.ReadAllText(Path.Combine(Wwwroot, "congan", "index.html"));
        foreach (var id in new[] { "p-search", "p-from", "p-to", "modal-root" })
        {
            Assert.IsTrue(police.Contains($"id=\"{id}\""), $"congan/index.html thiếu id=\"{id}\"");
        }

        var tenant = File.ReadAllText(Path.Combine(Wwwroot, "khachthue", "index.html"));
        foreach (var id in new[]
        {
            "tenant-history-body", "rc-room", "rc-elec", "rc-water", "rc-other", "rc-total", "modal-root"
        })
        {
            Assert.IsTrue(tenant.Contains($"id=\"{id}\""), $"khachthue/index.html thiếu id=\"{id}\"");
        }
    }

    /// <summary>
    /// Thứ tự nạp bắt buộc trên mỗi trang: shared bridge trước shared ui,
    /// ui trước các script riêng của trang.
    /// </summary>
    [TestMethod]
    public void ActorPages_LoadScriptsInOrder_WithoutDuplicates()
    {
        // Danh sách script đầy đủ của từng trang, theo đúng thứ tự nạp.
        var pages = new (string Actor, string[] Scripts)[]
        {
            ("auth", new[] { "../shared/js/bridge.js", "../shared/js/ui.js", "auth.js" }),
            ("chutro", new[]
            {
                "../shared/js/bridge.js", "../shared/js/ui.js", "js/main.js", "js/dash.js",
                "js/phong.js", "js/khach_thue.js", "js/hop_dong.js", "js/utils.js",
                "js/hoa_don.js", "js/reports.js", "js/perms.js",
            }),
            ("congan", new[]
            {
                "../shared/js/bridge.js", "../shared/js/ui.js", "js/main.js",
                "js/citizens.js", "js/residence.js", "js/history.js",
            }),
            ("khachthue", new[] { "../shared/js/bridge.js", "../shared/js/ui.js", "js/khach_thue.js" }),
        };
        foreach (var (actor, scripts) in pages)
        {
            var html = File.ReadAllText(Path.Combine(Wwwroot, actor, "index.html"));
            var last = -1;
            foreach (var src in scripts)
            {
                var tag = $"<script src=\"{src}\"></script>";
                Assert.AreEqual(1, html.Split(new[] { tag }, System.StringSplitOptions.None).Length - 1,
                    $"{actor}/index.html phải nạp đúng 1 lần: {tag}");
                var at = html.IndexOf(tag, System.StringComparison.Ordinal);
                Assert.IsTrue(at > last, $"{actor}/index.html nạp sai thứ tự: {tag}");
                last = at;
            }
        }
    }
}
