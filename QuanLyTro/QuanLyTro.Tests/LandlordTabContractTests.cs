using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class LandlordTabContractTests
{
    private static string Wwwroot =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "wwwroot");

    [TestMethod]
    public void LandlordHtml_HasSevenTabContainers_AndJsReferencesEachTabKey()
    {
        var html = File.ReadAllText(Path.Combine(Wwwroot, "index.html"));
        var js = File.ReadAllText(Path.Combine(Wwwroot, "js", "landlord.js"));

        string[] tabKeys = { "dash", "rooms", "tenants", "contracts", "utils", "invoices", "reports" };
        foreach (var key in tabKeys)
        {
            Assert.IsTrue(html.Contains($"id=\"tab-{key}\""), $"index.html thiếu tab-{key}");
            Assert.IsTrue(js.Contains($"'{key}'"), $"landlord.js chưa xử lý tab {key}");
        }
    }
}
