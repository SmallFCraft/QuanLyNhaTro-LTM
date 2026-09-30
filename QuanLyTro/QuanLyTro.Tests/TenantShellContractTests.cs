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
        var html = File.ReadAllText(Path.Combine(Wwwroot, "index.html"));
        var start = html.IndexOf("id=\"tenant\"", System.StringComparison.Ordinal);
        Assert.IsTrue(start >= 0, "index.html thiếu cửa sổ #tenant");

        var tenantBlock = html.Substring(start, html.Length - start);
        Assert.AreEqual(1, Regex.Matches(tenantBlock, "class=\"tab on\"").Count,
            "Shell khách thuê phải có đúng 1 tab");

        foreach (var verb in new[] { "Thêm", "Sửa", "Xóa" })
        {
            Assert.IsFalse(Regex.IsMatch(tenantBlock, $">{verb}<"), $"Shell khách thuê có nút ghi: {verb}");
        }
    }
}
