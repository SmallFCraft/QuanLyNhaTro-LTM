using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Server.Config;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class ServerOptionsTests
{
    [TestMethod]
    public void ServerOptions_RejectsInvalidPort()
    {
        Assert.ThrowsException<InvalidDataException>(() =>
            ServerOptions.Validate(new ServerOptions("server=localhost", 0)));
    }

    [TestMethod]
    public void ServerOptions_LoadsFromJson()
    {
        const string json = """
        { "ConnectionString": "Server=127.0.0.1;Port=3306;Database=db;", "Port": 8888 }
        """;
        var path = Path.Combine(Path.GetTempPath(), $"opts-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, json);
        try
        {
            var options = ServerOptions.Load(path);
            Assert.AreEqual(8888, options.Port);
            StringAssert.Contains(options.ConnectionString, "Database=db");
        }
        finally
        {
            File.Delete(path);
        }
    }
}
