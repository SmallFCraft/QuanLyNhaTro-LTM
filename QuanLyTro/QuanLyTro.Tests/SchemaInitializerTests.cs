using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Server.Data;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class SchemaInitializerTests
{
    [TestMethod]
    public void EmbeddedSchema_IsReadable()
    {
        var script = KhoiTaoSchema.ReadEmbeddedSchema();
        StringAssert.Contains(script, "CREATE TABLE IF NOT EXISTS tai_khoan");
        StringAssert.Contains(script, "CREATE TABLE IF NOT EXISTS phong");
        StringAssert.Contains(script, "CREATE TABLE IF NOT EXISTS khach_thue");
        StringAssert.Contains(script, "CREATE TABLE IF NOT EXISTS hop_dong");
        StringAssert.Contains(script, "CREATE TABLE IF NOT EXISTS chi_so_dien_nuoc");
        StringAssert.Contains(script, "CREATE TABLE IF NOT EXISTS hoa_don");
    }

    [TestMethod]
    public void Schema_CarriesTenantPasswordHash()
    {
        // Delta 2 tác nhân — người thuê đăng nhập bằng CCCD.
        var script = KhoiTaoSchema.ReadEmbeddedSchema();
        StringAssert.Contains(script, "mat_khau_hash VARCHAR(255) NULL");
    }

    [TestMethod]
    public void SplitStatements_ReturnsOneCommandPerMarker()
    {
        const string script = """
        CREATE DATABASE IF NOT EXISTS db;
        -- statement
        USE db;
        -- statement
        SELECT 1;
        """;

        var statements = KhoiTaoSchema.SplitStatements(script).ToList();

        Assert.AreEqual(3, statements.Count);
        Assert.AreEqual("CREATE DATABASE IF NOT EXISTS db;", statements[0]);
        Assert.AreEqual("USE db;", statements[1]);
        Assert.AreEqual("SELECT 1;", statements[2]);
    }

    [TestMethod]
    public void SplitStatements_SkipsBlankSegments()
    {
        var statements = KhoiTaoSchema.SplitStatements("-- statement\n-- statement\nSELECT 1;\n-- statement").ToList();
        Assert.AreEqual(1, statements.Count);
        Assert.AreEqual("SELECT 1;", statements[0]);
    }
}
