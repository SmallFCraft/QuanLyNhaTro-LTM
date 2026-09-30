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
}
