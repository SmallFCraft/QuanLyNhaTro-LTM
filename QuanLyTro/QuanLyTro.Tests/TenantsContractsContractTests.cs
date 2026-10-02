using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class TenantsContractsContractTests
{
    private static string Wwwroot =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "wwwroot");

    [TestMethod]
    public void TenantsJs_CoversFullCrudSurface()
    {
        var js = File.ReadAllText(Path.Combine(Wwwroot, "chutro", "js", "khach_thue.js"));
        foreach (var a in new[] { "KHACH_THUE_THEO_PHONG", "KHACH_THUE_THEM", "KHACH_THUE_CAP_NHAT",
                                  "KHACH_THUE_TRA_PHONG", "KHACH_THUE_XOA" })
        {
            Assert.IsTrue(js.Contains(a), $"khach_thue.js thiếu {a}");
        }
    }

    [TestMethod]
    public void ContractsJs_CoversFullCrudSurface()
    {
        var js = File.ReadAllText(Path.Combine(Wwwroot, "chutro", "js", "hop_dong.js"));
        foreach (var a in new[] { "HOP_DONG_LAY_TAT_CA", "HOP_DONG_TAO",
                                  "HOP_DONG_GIA_HAN", "HOP_DONG_CHAM_DUT" })
        {
            Assert.IsTrue(js.Contains(a), $"hop_dong.js thiếu {a}");
        }
    }
}
