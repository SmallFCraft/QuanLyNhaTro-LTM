using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class TenantShellContractTests
{
    private static string Wwwroot =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "wwwroot");

    [TestMethod]
    public void TenantShell_HasSingleTab_AndNoWriteButtons()
    {
        var html = File.ReadAllText(Path.Combine(Wwwroot, "khachthue", "index.html"));
        var start = html.IndexOf("id=\"khachthue\"", System.StringComparison.Ordinal);
        Assert.IsTrue(start >= 0, "khachthue/index.html thiếu cửa sổ #khachthue");

        var tenantBlock = html.Substring(start, html.Length - start);
        Assert.AreEqual(1, Regex.Matches(tenantBlock, "class=\"tab on\"").Count,
            "Shell khách thuê phải có đúng 1 tab");

        foreach (var verb in new[] { "Thêm", "Sửa", "Xóa" })
        {
            Assert.IsFalse(Regex.IsMatch(tenantBlock, $">{verb}<"), $"Shell khách thuê có nút ghi: {verb}");
        }
    }

    [TestMethod]
    public void TenantJs_RendersReceiptDetailAndHistoryFromServer()
    {
        var js = File.ReadAllText(Path.Combine(Wwwroot, "khachthue", "js", "khach_thue.js"));
        Assert.IsTrue(js.Contains("isPaid"), "tenant.js phải suy trạng thái đã thu");
        Assert.IsTrue(js.Contains("fmtDate"), "tenant.js phải định dạng ngày đóng tiền");
        // Chỉ đọc — không được có lệnh ghi nào
        Assert.IsFalse(js.Contains("HOA_DON_THANH_TOAN"), "shell khách thuê không được có thao tác ghi");
    }
}
