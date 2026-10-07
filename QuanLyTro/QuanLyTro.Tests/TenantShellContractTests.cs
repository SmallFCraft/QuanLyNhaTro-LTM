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

        // Đúng 1 tab active — HashSet dedup sẽ nuốt tab trùng, nên đếm trực tiếp trên navBlock.
        var navStart2 = html.IndexOf("id=\"tenantTabs\"", StringComparison.Ordinal);
        var navEnd2 = html.IndexOf("id=\"tab-nhan-phong\"", StringComparison.Ordinal);
        var navBlock2 = navEnd2 > navStart2
            ? html.Substring(navStart2, navEnd2 - navStart2)
            : html.Substring(navStart2);
        Assert.AreEqual(1, Regex.Matches(navBlock2, "class=\"tab on\"").Count,
            "Thanh #tenantTabs phải có đúng 1 tab active");

        // Không nút ghi: quét label literal như PoliceShellContractTests — attribute
        // data-write="true" không tồn tại ở bất kỳ file wwwroot nào nên assert nó không bao giờ đỏ.
        var shellStart = html.IndexOf("id=\"khachthue\"", StringComparison.Ordinal);
        Assert.IsTrue(shellStart >= 0, "khachthue/index.html thiếu cửa sổ #khachthue");
        var shellBlock = html.Substring(shellStart);
        foreach (var verb in new[] { "Thêm", "Sửa", "Xóa" })
        {
            Assert.IsFalse(Regex.IsMatch(shellBlock, $">{verb}<"), $"Shell khách thuê có nút ghi: {verb}");
        }

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

    /// <summary>Biểu đồ Tiện ích sinh SVG bằng DOM API — không thư viện, không thẻ svg trong index.html.</summary>
    [TestMethod]
    public void TenantJs_BuildsChartWithoutLibrary()
    {
        var js = TenantJs;
        Assert.IsTrue(js.Contains("createElementNS"),
            "khach_thue.js phải dựng SVG bằng document.createElementNS (không thư viện biểu đồ)");
        Assert.IsFalse(TenantHtml.Contains("<svg"),
            "index.html không được chứa sẵn <svg> — biểu đồ phải sinh trong JS");
    }

    /// <summary>Ruling controller: chart vẽ SẢN LƯỢNG (kWh/khối), không phải tiền — tổng đ phải biến mất khỏi <title>.</summary>
    [TestMethod]
    public void TenantJs_ChartPlotsQuantitiesNotMoney()
    {
        var js = TenantJs;
        Assert.IsTrue(js.Contains("elecQty"), "buildTenantChart phải tính sản lượng điện (kWh), không dùng trực tiếp tienDien");
        // Chỉ soi thân hàm buildTenantChart: khẳng định trên cả file là vô nghĩa vì tiền vẫn
        // hợp lệ ở KPI/tóm tắt. Trong hàm chỉ được còn sản lượng — không được có tienDien/tongTien.
        var fn = Regex.Match(js, @"function buildTenantChart\(rows\) \{[\s\S]*?\n\}").Value;
        Assert.IsFalse(string.IsNullOrEmpty(fn), "không tìm thấy buildTenantChart trong khach_thue.js");
        Assert.IsFalse(fn.Contains("tongTien"),
            "buildTenantChart không được đọc tongTien — chart vẽ theo tiền là sai ruling");
        Assert.IsTrue(fn.Contains("kWh điện"), "chart <title> phải nêu đơn vị sản lượng");
    }

    /// <summary>Ruling A: mỗi KPI Tiện ích phải có sub ghi phạm vi — 4 KPI, không số nào trần.</summary>
    [TestMethod]
    public void TenantJs_UtilityKpisCarryScopeSub()
    {
        var js = TenantJs;
        foreach (var label in new[] { "Số kỳ đã phát sinh", "Trung bình mỗi kỳ", "Kỳ cao nhất", "Tổng đã nộp" })
        {
            Assert.IsTrue(js.Contains(label), $"renderTenantUtilities thiếu KPI \"{label}\"");
        }
        Assert.IsTrue(js.Contains("trên ${totalPages} trang"),
            "KPI 'Số kỳ đã phát sinh' phải ghi phạm vi phân trang trong sub");
        Assert.IsTrue(js.Contains("trang này"),
            "các KPI tính trên 1 trang phải ghi rõ 'trang này' trong sub");
    }

    /// <summary>Ruling B: không ship STK bịa — BANK_INFO rỗng thì copyTransfer không dựng nội dung có số tài khoản.</summary>
    [TestMethod]
    public void TenantJs_NoInventedBankAccount()
    {
        var js = TenantJs;
        Assert.IsTrue(js.Contains("const BANK_INFO = '';"),
            "BANK_INFO phải là hằng rỗng cho tới khi server có hồ sơ nhận tiền");
        Assert.IsFalse(Regex.IsMatch(js, @"\d{9,}\s*·"),
            "không được nhúng số tài khoản giả vào khach_thue.js");
        Assert.IsTrue(js.Contains("thông tin nhận tiền chưa cấu hình"),
            "copyTransfer phải nói rõ chưa cấu hình thông tin nhận tiền khi BANK_INFO rỗng");
    }

    /// <summary>Ruling C: canvas QR phải bỏ `hidden` trước khi vẽ; pane dùng class, không ghi style.display.</summary>
    [TestMethod]
    public void TenantJs_QrCanvasUnhiddenAndPaneUsesClass()
    {
        var js = TenantJs;
        Assert.IsTrue(js.Contains("canvas.hidden = false"),
            "decodeQrFile phải bỏ `hidden` của #qrCanvas trước khi drawImage");
        // Chỉ so thân hàm switchCheckinTab — showCheckinScreen dùng p.style.display hợp lệ
        // để ẩn các tab-pane khi hiện màn nhận phòng.
        var fn = Regex.Match(js, @"function switchCheckinTab\(tabName\) \{[\s\S]*?\n\}").Value;
        Assert.IsFalse(string.IsNullOrEmpty(fn), "không tìm thấy switchCheckinTab trong khach_thue.js");
        Assert.IsTrue(fn.Contains("classList.toggle"),
            "switchCheckinTab phải chuyển pane bằng classList.toggle");
        Assert.IsFalse(fn.Contains("p.style.display") || fn.Contains("p.style["),
            "switchCheckinTab không được ghi style.display đè .checkin-pane/.active");
    }

    /// <summary>Task 4: trạng thái rỗng nói thật, không giữ câu placeholder "đang được cập nhật".</summary>
    [TestMethod]
    public void TenantJs_EmptyStateHasNoPlaceholderCopy()
    {
        Assert.IsFalse(TenantHtml.Contains("đang được cập nhật"),
            "index.html không được giữ câu placeholder 'đang được cập nhật'");
        var js = TenantJs;
        Assert.IsTrue(js.Contains("Chưa phát sinh kỳ cước nào"), "trạng thái rỗng phải nói 'Chưa phát sinh kỳ cước nào'");
        Assert.IsTrue(js.Contains("renderTenantLoading"), "phải có skeleton lúc nạp");
        Assert.IsTrue(js.Contains("'skel'") || js.Contains("class=\"skel\""), "skeleton dùng div.skel");
        Assert.IsTrue(js.Contains("Thử lại"), "trạng thái lỗi phải có nút Thử lại");
    }
}
