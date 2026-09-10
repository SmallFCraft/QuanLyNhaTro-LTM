using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Server.Data;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class SchemaInitializerTests
{
    [TestMethod]
    public void EmbeddedSchema_IsReadable()
    {
        var script = SchemaInitializer.ReadEmbeddedSchema();
        StringAssert.Contains(script, "CREATE TABLE IF NOT EXISTS users");
        StringAssert.Contains(script, "CREATE TABLE IF NOT EXISTS rooms");
        StringAssert.Contains(script, "CREATE TABLE IF NOT EXISTS tenants");
        StringAssert.Contains(script, "CREATE TABLE IF NOT EXISTS contracts");
        StringAssert.Contains(script, "CREATE TABLE IF NOT EXISTS utility_readings");
        StringAssert.Contains(script, "CREATE TABLE IF NOT EXISTS invoices");
    }

    [TestMethod]
    public void Schema_CarriesTenantPasswordHash()
    {
        // Delta 2 tác nhân — người thuê đăng nhập bằng CCCD.
        var script = SchemaInitializer.ReadEmbeddedSchema();
        StringAssert.Contains(script, "password_hash VARCHAR(255) NULL");
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

        var statements = SchemaInitializer.SplitStatements(script).ToList();

        Assert.AreEqual(3, statements.Count);
        Assert.AreEqual("CREATE DATABASE IF NOT EXISTS db;", statements[0]);
        Assert.AreEqual("USE db;", statements[1]);
        Assert.AreEqual("SELECT 1;", statements[2]);
    }

    [TestMethod]
    public void SplitStatements_SkipsBlankSegments()
    {
        var statements = SchemaInitializer.SplitStatements("-- statement\n-- statement\nSELECT 1;\n-- statement").ToList();
        Assert.AreEqual(1, statements.Count);
        Assert.AreEqual("SELECT 1;", statements[0]);
    }
}
