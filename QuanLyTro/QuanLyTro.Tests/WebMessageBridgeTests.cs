using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Network;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class WebMessageBridgeTests
{
    [TestMethod]
    public async Task ParseAndDispatch_InvalidJson_ReturnsErrorEnvelope()
    {
        string? returnedJson = null;
        var bridge = new WebMessageBridge(client: null!);

        await bridge.DispatchAsync("invalid json", msg =>
        {
            returnedJson = msg;
            return Task.CompletedTask;
        });

        Assert.IsNotNull(returnedJson);
        using var doc = JsonDocument.Parse(returnedJson);
        Assert.IsFalse(doc.RootElement.GetProperty("success").GetBoolean());
        Assert.IsTrue(doc.RootElement.GetProperty("error").GetString()!.Contains("JSON"));
    }
}
