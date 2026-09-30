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

    /// <summary>
    /// C2 (spec §3.2): token phiên chỉ sống trong C#. Không file JS/HTML nào được đọc
    /// `token` từ đối tượng người dùng trả về AUTH_LOGIN.
    /// </summary>
    [TestMethod]
    public void FrontEnd_NeverReadsSessionToken()
    {
        var files = Directory.GetFiles(Path.Combine(Wwwroot, "js"), "*.js")
            .Append(Path.Combine(Wwwroot, "index.html"));

        foreach (var file in files)
        {
            var text = File.ReadAllText(file);
            Assert.IsFalse(
                Regex.IsMatch(text, @"\b\w+\.(token|Token)\b"),
                $"{Path.GetFileName(file)} đọc trường token — token phải ở lại phía C#.");
            Assert.IsFalse(
                Regex.IsMatch(text, @"localStorage|sessionStorage"),
                $"{Path.GetFileName(file)} dùng storage trình duyệt — token không được lưu ở đó.");
        }
    }

    [TestMethod]
    public void PoliceJs_SearchInputAndDateRangeAreAddressableById()
    {
        var html = File.ReadAllText(Path.Combine(Wwwroot, "index.html"));
        Assert.IsTrue(html.Contains("id=\"p-search\""), "ô tìm kiếm công an cần id để JS đọc");
        Assert.IsTrue(html.Contains("id=\"p-from\""), "ô ngày bắt đầu cần id");
        Assert.IsTrue(html.Contains("id=\"p-to\""), "ô ngày kết thúc cần id");
    }

    [TestMethod]
    public void PoliceJs_UsesDocumentElementIds_AndNoWriteActions()
    {
        var js = File.ReadAllText(Path.Combine(Wwwroot, "js", "police.js"));
        Assert.IsTrue(js.Contains("document.getElementById('p-search')"), "police.js phải đọc #p-search bằng getElementById");
        Assert.IsTrue(js.Contains("document.getElementById('p-from')"), "police.js phải đọc #p-from bằng getElementById");
        Assert.IsTrue(js.Contains("document.getElementById('p-to')"), "police.js phải đọc #p-to bằng getElementById");

        // BR-16: shell công an không gọi bất kỳ action ghi nào
        var forbiddenWriteActions = new[]
        {
            "ROOM_ADD", "ROOM_UPDATE", "ROOM_DELETE",
            "TENANT_ADD", "TENANT_UPDATE", "TENANT_CHECKOUT", "TENANT_DELETE",
            "CONTRACT_CREATE", "CONTRACT_TERMINATE", "CONTRACT_RENEW",
            "UTILITY_RECORD",
            "INVOICE_CREATE", "INVOICE_PAY"
        };
        foreach (var action in forbiddenWriteActions)
        {
            Assert.IsFalse(js.Contains(action), $"police.js vi phạm BR-16 — chứa action ghi: {action}");
        }
    }
}
