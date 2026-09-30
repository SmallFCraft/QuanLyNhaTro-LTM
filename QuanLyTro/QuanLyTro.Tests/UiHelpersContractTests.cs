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
        var js = File.ReadAllText(Path.Combine(Wwwroot, "js", "ui.js"));
        foreach (var fn in new[] { "function esc", "function fmtMoney", "function fmtDate",
                                   "function fmtDateOnly", "function toast", "function openModal",
                                   "function confirmDialog" })
        {
            Assert.IsTrue(js.Contains(fn), $"ui.js thiếu {fn}");
        }
    }

    [TestMethod]
    public void IndexHtml_LoadsUiJs_AndHasModalRoot()
    {
        var html = File.ReadAllText(Path.Combine(Wwwroot, "index.html"));
        Assert.IsTrue(html.Contains("js/ui.js"), "index.html chưa nạp ui.js");
        Assert.IsTrue(html.Contains("id=\"modal-root\""), "index.html thiếu #modal-root");
    }

    /// <summary>
    /// Task 9–13 đọc các phần tử này qua getElementById; thiếu id thì task sau không chạy được.
    /// </summary>
    [TestMethod]
    public void IndexHtml_HasAllScreenElementIds()
    {
        var html = File.ReadAllText(Path.Combine(Wwwroot, "index.html"));
        string[] ids =
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
            // Công an
            "p-search", "p-from", "p-to",
            // Khách thuê
            "tenant-history-body", "rc-room", "rc-elec", "rc-water", "rc-other", "rc-total",
            // Dùng chung
            "modal-root",
        };
        foreach (var id in ids)
        {
            Assert.IsTrue(html.Contains($"id=\"{id}\""), $"index.html thiếu id=\"{id}\"");
        }
    }

    /// <summary>
    /// Thứ tự nạp bắt buộc: bridge trước ui, ui trước các file con.
    /// </summary>
    [TestMethod]
    public void IndexHtml_LoadsScriptsInOrder_WithoutDuplicates()
    {
        var html = File.ReadAllText(Path.Combine(Wwwroot, "index.html"));
        string[] order =
        {
            "js/bridge.js", "js/ui.js", "js/login.js", "js/landlord.js",
            "js/landlord/dash.js", "js/landlord/rooms.js", "js/landlord/tenants.js",
            "js/landlord/contracts.js", "js/landlord/utils.js", "js/landlord/invoices.js",
            "js/landlord/reports.js", "js/police.js", "js/tenant.js",
        };
        var last = -1;
        foreach (var src in order)
        {
            var tag = $"<script src=\"{src}\"></script>";
            Assert.AreEqual(1, html.Split(tag).Length - 1, $"index.html phải nạp đúng 1 lần: {src}");
            var at = html.IndexOf(tag, System.StringComparison.Ordinal);
            Assert.IsTrue(at > last, $"{src} nạp sai thứ tự");
            last = at;
        }
    }
}
