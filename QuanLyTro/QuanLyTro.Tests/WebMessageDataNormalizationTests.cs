using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Network;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Tests;

/// <summary>
/// Lỗi "Operation is not valid due to the current state of the object." xảy ra khi JS gửi gói tin
/// thiếu trường `data` (hoặc data:null). Khi đó JsonElement ở trạng thái Undefined/Null, và
/// JsonSerializer.SerializeToElement ném InvalidOperationException — người dùng thấy nó bọc thành
/// "Lỗi kết nối hoặc hệ thống: ...". Bộ test này khóa hành vi chuẩn hóa của WebMessageBridge.
/// </summary>
[TestClass]
public sealed class WebMessageDataNormalizationTests
{
    [TestMethod]
    public void SerializeToElement_OfUndefined_ThrowsThisExactMessage()
    {
        // Chứng minh đây đúng là nguồn của thông điệp người dùng gặp.
        JsonElement undef = default;
        var ex = Assert.ThrowsException<InvalidOperationException>(
            () => JsonSerializer.SerializeToElement(undef));
        Assert.AreEqual("Operation is not valid due to the current state of the object.", ex.Message);
    }

    [TestMethod]
    public void Envelope_MissingDataField_YieldsUndefined()
    {
        var env = JsonSerializer.Deserialize<WebMessageBridge.ClientEnvelope>(
            """{"requestId":"r1","action":"INVOICE_GET_ALL"}""", JsonDefaults.Options);

        Assert.IsNotNull(env);
        Assert.AreEqual(JsonValueKind.Undefined, env!.Data.ValueKind);
    }

    [TestMethod]
    public void Envelope_NullData_YieldsNullKind()
    {
        var env = JsonSerializer.Deserialize<WebMessageBridge.ClientEnvelope>(
            """{"requestId":"r1","action":"INVOICE_GET_ALL","data":null}""", JsonDefaults.Options);

        Assert.IsNotNull(env);
        Assert.AreEqual(JsonValueKind.Null, env!.Data.ValueKind);
    }

    [TestMethod]
    public async Task Dispatch_MissingDataField_DoesNotThrowInvalidOperation()
    {
        // Bridge với client chưa kết nối: lỗi kết nối là hợp lệ, nhưng KHÔNG được là
        // InvalidOperationException từ việc serialize JsonElement Undefined.
        string? reply = null;
        var bridge = new WebMessageBridge(new QuanLyTro.Network.TcpClientService());

        await bridge.DispatchAsync(
            """{"requestId":"r1","action":"INVOICE_GET_ALL"}""",
            msg => { reply = msg; return Task.CompletedTask; });

        Assert.IsNotNull(reply, "Bridge phải luôn trả lời, kể cả khi lỗi.");
        using var doc = JsonDocument.Parse(reply);
        Assert.IsFalse(doc.RootElement.GetProperty("success").GetBoolean());
        var error = doc.RootElement.GetProperty("error").GetString()!;
        Assert.IsFalse(error.Contains("Operation is not valid"),
            $"Không được lộ lỗi serialize JsonElement. Nhận: {error}");
    }

    [TestMethod]
    public async Task Dispatch_NullData_DoesNotThrowInvalidOperation()
    {
        string? reply = null;
        var bridge = new WebMessageBridge(new QuanLyTro.Network.TcpClientService());

        await bridge.DispatchAsync(
            """{"requestId":"r2","action":"INVOICE_GET_ALL","data":null}""",
            msg => { reply = msg; return Task.CompletedTask; });

        Assert.IsNotNull(reply);
        using var doc = JsonDocument.Parse(reply);
        var error = doc.RootElement.GetProperty("error").GetString()!;
        Assert.IsFalse(error.Contains("Operation is not valid"),
            $"Không được lộ lỗi serialize JsonElement. Nhận: {error}");
    }
}
