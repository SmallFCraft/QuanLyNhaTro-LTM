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
        Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "auth", "index.html")), "Thiếu auth/index.html");
        Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "chutro", "index.html")), "Thiếu chutro/index.html");
        Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "congan", "index.html")), "Thiếu congan/index.html");
        Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "khachthue", "index.html")), "Thiếu khachthue/index.html");
        Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "shared", "js", "bridge.js")), "Thiếu shared/js/bridge.js");
        Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "shared", "css", "base.css")), "Thiếu shared/css/base.css");
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
        Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "chutro", "index.html")), "Thiếu chutro/index.html");
        Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "chutro", "chutro.css")), "Thiếu chutro/chutro.css");
        Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "chutro", "js", "main.js")), "Thiếu chutro/js/main.js");
        foreach (var tab in new[] { "dash", "phong", "khach_thue", "hop_dong", "utils", "hoa_don", "reports", "perms" })
        {
            Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "chutro", "js", tab + ".js")),
                $"Thiếu chutro/js/{tab}.js");
        }
    }

    [TestMethod]
    public void Wwwroot_ContainsTenantModule()
    {
        Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "khachthue", "index.html")), "Thiếu khachthue/index.html");
        Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "khachthue", "khachthue.css")), "Thiếu khachthue/khachthue.css");
        Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "khachthue", "js", "khach_thue.js")), "Thiếu khachthue/js/khach_thue.js");
    }

    [TestMethod]
    public void Wwwroot_ContainsPoliceModule()
    {
        Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "congan", "index.html")), "Thiếu congan/index.html");
        Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "congan", "congan.css")), "Thiếu congan/congan.css");
        Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "congan", "js", "main.js")), "Thiếu congan/js/main.js");
        foreach (var tab in new[] { "citizens", "residence", "history" })
        {
            Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "congan", "js", tab + ".js")),
                $"Thiếu congan/js/{tab}.js");
        }
    }
}
