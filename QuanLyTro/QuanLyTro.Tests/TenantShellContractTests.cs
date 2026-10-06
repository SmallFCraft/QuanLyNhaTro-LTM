using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class TenantShellContractTests
{
    // Shell khách thuê chỉ được gọi đúng 2 action: 1 đọc hóa đơn, 1 check-in QR (BR-17/18/19).
    private static readonly HashSet<string> AllowedActions = new(StringComparer.Ordinal)
    {
        "HOA_DON_CUA_TOI", "KHACH_THUE_NHAN_PHONG_QR",
    };

    /// <summary>Mọi literal tên action (CHU_HOA_CO_GACH) truyền vào bridge.call(...) trong file JS.</summary>
    private static HashSet<string> GetBridgeCallActions(string js)
    {
        var actions = new HashSet<string>(StringComparer.Ordinal);
        foreach (Match m in Regex.Matches(js, "bridge\\s*\\.\\s*call\\s*\\(\\s*['\"]([A-Z][A-Z0-9_]*)['\"]"))
        {
            actions.Add(m.Groups[1].Value);
        }
        return actions;
    }

    private static string Wwwroot =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "wwwroot");

    private static string TenantHtml =>
        File.ReadAllText(Path.Combine(Wwwroot, "khachthue", "index.html"));

    private static string TenantJs =>
        File.ReadAllText(Path.Combine(Wwwroot, "khachthue", "js", "khach_thue.js"));

    /// <summary>Trích tập data-tab của #tenantTabs theo cấu trúc, không cắt giữa chừng.</summary>
    private static HashSet<string> GetTenantTabs(string html)
    {
        var navStart = html.IndexOf("id=\"tenantTabs\"", StringComparison.Ordinal);
        Assert.IsTrue(navStart >= 0, "khachthue/index.html thiếu thanh điều hướng #tenantTabs");

        // Màn hình nhận phòng là view thay thế riêng, đứng sau nav — chặn đó là cuối khối nav.
        var navEnd = html.IndexOf("id=\"tab-nhan-phong\"", StringComparison.Ordinal);
        var navBlock = navEnd > navStart
            ? html.Substring(navStart, navEnd - navStart)
            : html.Substring(navStart);  // ponytail: hằng số cứng cắt nav; đổi sang parse DOM nếu nav lồng nhau

        var tabs = new HashSet<string>(StringComparer.Ordinal);
        foreach (Match m in Regex.Matches(navBlock, "data-tab=\"([^\"]+)\""))
        {
            tabs.Add(m.Groups[1].Value);
        }

        return tabs;
    }

    [TestMethod]
    public void TenantShell_TabsAndNoWriteButtons()
    {
        var html = TenantHtml;

        var expected = new HashSet<string>(StringComparer.Ordinal)
        {
            "overview", "current", "history", "utilities",
        };
        var actual = GetTenantTabs(html);
        Assert.IsTrue(expected.SetEquals(actual),
            "Tập data-tab của #tenantTabs phải đúng {overview, current, history, utilities}; nhận: {"
            + string.Join(", ", actual) + "}");

        Assert.IsFalse(Regex.IsMatch(html, "data-write\\s*=\\s*['\"]?true", RegexOptions.IgnoreCase),
            "Shell khách thuê không được có attribute data-write=\"true\"");

        // Allowlist: action thứ 13 hay tên gõ sai cũng bắt đỏ — chặn theo danh sách known-bad thì kẽ hở.
        var used = GetBridgeCallActions(TenantJs);
        Assert.IsTrue(used.IsSubsetOf(AllowedActions),
            "khach_thue.js gọi action ngoài allowlist {HOA_DON_CUA_TOI, KHACH_THUE_NHAN_PHONG_QR}; nhận: {"
            + string.Join(", ", used) + "}");
        Assert.IsTrue(used.Contains("KHACH_THUE_NHAN_PHONG_QR"),
            "khach_thue.js phải có action check-in QR KHACH_THUE_NHAN_PHONG_QR");
    }

    [TestMethod]
    public void TenantShell_EveryTabHasPane()
    {
        var html = TenantHtml;
        var tabs = GetTenantTabs(html);
        Assert.IsTrue(tabs.Count > 0, "Không trích được data-tab nào — test sẽ xanh vô nghĩa");
        foreach (var tab in tabs)
        {
            Assert.IsTrue(html.Contains($"<section id=\"tab-{tab}\""),
                $"Tab \"{tab}\" trong #tenantTabs thiếu <section id=\"tab-{tab}\">");
        }
    }

    /// <summary>Bàn phím cho cả 2 tab bar: Arrow chuyển tab, aria-selected khớp class="on".</summary>
    [TestMethod]
    public void TenantShell_TabsAreKeyboardReachable()
    {
        var js = TenantJs;
        Assert.IsTrue(js.Contains("ArrowRight"), "khach_thue.js phải xử lý ArrowRight cho tab bar");
        Assert.IsTrue(js.Contains("ArrowLeft"), "khach_thue.js phải xử lý ArrowLeft cho tab bar");
        Assert.IsTrue(js.Contains("aria-selected"), "khach_thue.js phải giữ aria-selected khi chuyển tab");
        Assert.IsTrue(js.Contains("#tenant-live") || js.Contains("tenant-live"),
            "khach_thue.js phải ghi thông báo nạp xong vào #tenant-live");
        var html = TenantHtml;
        Assert.IsTrue(html.Contains("id=\"tenant-live\""), "index.html phải có #tenant-live (aria-live)");
    }

    [TestMethod]
    public void TenantJs_RendersReceiptDetailAndHistoryFromServer()
    {
        var js = TenantJs;
        Assert.IsTrue(js.Contains("isPaid"), "tenant.js phải suy trạng thái đã thu");
        Assert.IsTrue(js.Contains("fmtDate"), "tenant.js phải định dạng ngày đóng tiền");
        // Action đọc duy nhất được phép trong shell khách thuê.
        Assert.IsTrue(js.Contains("HOA_DON_CUA_TOI"), "tenant.js phải đọc hóa đơn của chính khách");
    }
}
