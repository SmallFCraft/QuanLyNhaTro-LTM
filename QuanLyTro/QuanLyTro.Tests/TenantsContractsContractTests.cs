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
        var js = File.ReadAllText(Path.Combine(Wwwroot, "js", "landlord", "tenants.js"));
        foreach (var a in new[] { "TENANT_GET_BY_ROOM", "TENANT_ADD", "TENANT_UPDATE",
                                  "TENANT_CHECKOUT", "TENANT_DELETE" })
        {
            Assert.IsTrue(js.Contains(a), $"tenants.js thiếu {a}");
        }
    }

    [TestMethod]
    public void ContractsJs_CoversFullCrudSurface()
    {
        var js = File.ReadAllText(Path.Combine(Wwwroot, "js", "landlord", "contracts.js"));
        foreach (var a in new[] { "CONTRACT_GET_ALL", "CONTRACT_CREATE",
                                  "CONTRACT_RENEW", "CONTRACT_TERMINATE" })
        {
            Assert.IsTrue(js.Contains(a), $"contracts.js thiếu {a}");
        }
    }
}
