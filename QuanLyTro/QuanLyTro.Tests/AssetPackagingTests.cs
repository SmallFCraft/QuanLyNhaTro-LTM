using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class AssetPackagingTests
{
    private static string Wwwroot =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "wwwroot");

    [TestMethod]
    public void Wwwroot_ContainsEntryPointAndBridge()
    {
        Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "index.html")), "Thiếu index.html");
        Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "js", "bridge.js")), "Thiếu bridge.js");
        Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "css", "style.css")), "Thiếu style.css");
    }

    [TestMethod]
    public void Wwwroot_ContainsSharedAssets()
    {
        Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "shared", "css", "tokens.css")), "Thiếu tokens.css");
        Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "shared", "css", "base.css")), "Thiếu base.css");
        Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "shared", "js", "bridge.js")), "Thiếu shared bridge.js");
        Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "shared", "js", "ui.js")), "Thiếu shared ui.js");
    }

    [TestMethod]
    public void Wwwroot_ContainsAuthModule()
    {
        Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "auth", "index.html")), "Thiếu auth/index.html");
        Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "auth", "auth.css")), "Thiếu auth/auth.css");
        Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "auth", "auth.js")), "Thiếu auth/auth.js");
    }

    [TestMethod]
    public void Wwwroot_ContainsLandlordModule()
    {
        Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "landlord", "index.html")), "Thiếu landlord/index.html");
        Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "landlord", "landlord.css")), "Thiếu landlord/landlord.css");
        Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "landlord", "js", "main.js")), "Thiếu landlord/js/main.js");
        foreach (var tab in new[] { "dash", "rooms", "tenants", "contracts", "utils", "invoices", "reports" })
        {
            Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "landlord", "js", tab + ".js")),
                $"Thiếu landlord/js/{tab}.js");
        }
    }

    [TestMethod]
    public void Wwwroot_ContainsPoliceModule()
    {
        Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "police", "index.html")), "Thiếu police/index.html");
        Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "police", "police.css")), "Thiếu police/police.css");
        Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "police", "js", "main.js")), "Thiếu police/js/main.js");
        foreach (var tab in new[] { "citizens", "residence", "history" })
        {
            Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "police", "js", tab + ".js")),
                $"Thiếu police/js/{tab}.js");
        }
    }
}
