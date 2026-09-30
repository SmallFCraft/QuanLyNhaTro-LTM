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
}
