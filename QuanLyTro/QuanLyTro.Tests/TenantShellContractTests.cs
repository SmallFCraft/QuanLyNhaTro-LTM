using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class TenantShellContractTests
{
    // Toàn bộ action ghi — shell khách thuê chỉ đọc.
    private static readonly string[] WriteActions =
    {
        "HOA_DON_THANH_TOAN", "PHONG_THEM", "PHONG_XOA", "PHONG_CAP_NHAT",
        "KHACH_THUE_CAP_NHAT", "KHACH_THUE_XOA", "KHACH_THUE_THEM",
        "HOP_DONG_TAO", "HOP_DONG_CHAM_DUT", "HOP_DONG_GIA_HAN",
        "DIEN_NUOC_GHI_SO", "PHAN_QUYEN_CAP_NHAT_VAI_TRO",
    };

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
    public void TenantShell_HasSingleTab_AndNoWriteButtons()
    {
        var html = TenantHtml;

        var expected = new HashSet<string>(StringComparer.Ordinal)
        {
            "overview", "current", "history",
        };
        var actual = GetTenantTabs(html);
        Assert.IsTrue(expected.SetEquals(actual),
            "Tập data-tab của #tenantTabs phải đúng {overview, current, history}; nhận: {"
            + string.Join(", ", actual) + "}");

        Assert.IsFalse(Regex.IsMatch(html, "data-write\\s*=\\s*\"true\"", RegexOptions.IgnoreCase),
            "Shell khách thuê không được có attribute data-write=\"true\"");

        var js = TenantJs;
        foreach (var action in WriteActions)
        {
            Assert.IsFalse(js.Contains(action), $"khach_thue.js không được chứa action ghi: {action}");
        }
    }

    [TestMethod]
    public void TenantShell_EveryTabHasPane()
    {
        var html = TenantHtml;
        var tabs = GetTenantTabs(html);
        Assert.IsTrue(tabs.Count > 0, "Không trích được data-tab nào — test sẽ xanh vô nghĩa");
        foreach (var tab in tabs)
        {
            Assert.IsTrue(html.Contains($"id=\"tab-{tab}\""),
                $"Tab \"{tab}\" trong #tenantTabs thiếu section id=\"tab-{tab}\"");
        }
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
