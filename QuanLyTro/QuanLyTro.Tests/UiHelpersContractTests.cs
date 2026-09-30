using System.IO;
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
                                   "function confirmDialog" })
        {
            Assert.IsTrue(js.Contains(fn), $"ui.js thiếu {fn}");
        }
    }

    /// <summary>
    /// Mỗi trang actor (landlord/police/tenant) nạp ui.js và có #modal-root cho modal dùng chung.
    /// </summary>
    [TestMethod]
    public void ActorPages_LoadUiJs_AndHaveModalRoot()
    {
        foreach (var actor in new[] { "landlord", "police", "tenant" })
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
        var landlord = File.ReadAllText(Path.Combine(Wwwroot, "landlord", "index.html"));
        string[] landlordIds =
        {
            // Tổng quan
            "kpi-total-rooms", "kpi-available", "kpi-tenants", "kpi-unpaid",
            "dash-overdue-rows", "dash-expiring-rows",
            // Phòng
            "room-search", "room-status",
            // Người thuê
            "tenant-room", "tenants-body",
            // Hợp đồng
            "contracts-body",
            // Điện nước
            "util-room", "util-month", "util-elec-old", "util-elec-new", "util-elec-rate",
            "util-water-old", "util-water-new", "util-water-rate",
            // Hóa đơn
            "inv-month", "inv-room", "invoices-body",
            // Thống kê
            "rep-month",
            // Dùng chung
            "modal-root",
        };
        foreach (var id in landlordIds)
        {
            Assert.IsTrue(landlord.Contains($"id=\"{id}\""), $"landlord/index.html thiếu id=\"{id}\"");
        }

        var police = File.ReadAllText(Path.Combine(Wwwroot, "police", "index.html"));
        foreach (var id in new[] { "p-search", "p-from", "p-to", "modal-root" })
        {
            Assert.IsTrue(police.Contains($"id=\"{id}\""), $"police/index.html thiếu id=\"{id}\"");
        }

        var tenant = File.ReadAllText(Path.Combine(Wwwroot, "tenant", "index.html"));
        foreach (var id in new[]
        {
            "tenant-history-body", "rc-room", "rc-elec", "rc-water", "rc-other", "rc-total", "modal-root"
        })
        {
            Assert.IsTrue(tenant.Contains($"id=\"{id}\""), $"tenant/index.html thiếu id=\"{id}\"");
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
            ("landlord", new[]
            {
                "../shared/js/bridge.js", "../shared/js/ui.js", "js/main.js", "js/dash.js",
                "js/rooms.js", "js/tenants.js", "js/contracts.js", "js/utils.js",
                "js/invoices.js", "js/reports.js",
            }),
            ("police", new[]
            {
                "../shared/js/bridge.js", "../shared/js/ui.js", "js/main.js",
                "js/citizens.js", "js/residence.js", "js/history.js",
            }),
            ("tenant", new[] { "../shared/js/bridge.js", "../shared/js/ui.js", "js/tenant.js" }),
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
