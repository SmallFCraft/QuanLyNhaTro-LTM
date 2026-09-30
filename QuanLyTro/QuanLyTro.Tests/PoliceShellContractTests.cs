using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class PoliceShellContractTests
{
    private static string Wwwroot =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "wwwroot");

    [TestMethod]
    public void PoliceShell_HasThreeTabs_AndNoWriteButtons()
    {
        var html = File.ReadAllText(Path.Combine(Wwwroot, "index.html"));
        var start = html.IndexOf("id=\"police\"", System.StringComparison.Ordinal);
        Assert.IsTrue(start >= 0, "index.html thiếu cửa sổ #police");

        var policeBlock = html.Substring(start, html.Length - start);
        string[] tabKeys = { "citizens", "residence", "history" };
        foreach (var key in tabKeys)
        {
            Assert.IsTrue(policeBlock.Contains($"id=\"ptab-{key}\""), $"Thiếu tab công an: ptab-{key}");
        }

        // BR-16: không được có nút ghi trong shell công an
        foreach (var verb in new[] { "Thêm", "Sửa", "Xóa", "Lập hợp đồng", "Thu tiền" })
        {
            Assert.IsFalse(Regex.IsMatch(policeBlock, $">{verb}<"), $"Shell công an có nút ghi: {verb}");
        }
    }
}
