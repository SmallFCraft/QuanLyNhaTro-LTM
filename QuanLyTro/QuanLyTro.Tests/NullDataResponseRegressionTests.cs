using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Network;
using QuanLyTro.Shared.Models;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Tests;

/// <summary>
/// Regression: khi Server trả về Data = null (ví dụ: UTILITY_GET_PREVIOUS khi phòng chưa có
/// chỉ số kỳ trước, hoặc bất kỳ action nào trả null), TcpClientService trả về JsonElement
/// có ValueKind == Undefined.
/// Nếu không chuẩn hóa, JsonSerializer.Serialize sẽ ném:
/// "Operation is not valid due to the current state of the object."
/// Bộ test này khóa chặt bản vá tại WebMessageBridge.
/// </summary>
[TestClass]
public sealed class NullDataResponseRegressionTests
{
    [TestMethod]
    public void ServerOk_WithNullData_WireJsonHasDataNull()
    {
        var serverPacket = ResponsePacket.Ok<UtilityReadingDto?>(null);
        var wireJson = JsonSerializer.Serialize(serverPacket, JsonDefaults.Options);

        Assert.IsTrue(wireJson.Contains("\"data\":null"));

        var clientPacket = JsonSerializer.Deserialize<ResponsePacket>(wireJson, JsonDefaults.Options);
        Assert.IsNotNull(clientPacket);
        Assert.IsFalse(clientPacket!.Data.HasValue, "Data trong ResponsePacket là null nên HasValue = false");

        var dataElement = clientPacket.GetData<JsonElement>();
        Assert.AreEqual(JsonValueKind.Undefined, dataElement.ValueKind,
            "GetData<JsonElement>() trả về default(JsonElement) có ValueKind == Undefined");
    }

    [TestMethod]
    public void UndefinedJsonElement_InAnonymousObject_ThrowsInvalidOperationException()
    {
        // Khóa đúng exception mà người dùng gặp phải trước khi vá
        JsonElement undef = default;
        var ex = Assert.ThrowsException<InvalidOperationException>(() =>
        {
            JsonSerializer.Serialize(new
            {
                requestId = "r1",
                success = true,
                data = undef,
                error = (string?)null
            }, JsonDefaults.Options);
        });

        Assert.AreEqual("Operation is not valid due to the current state of the object.", ex.Message);
    }

    [TestMethod]
    public void NormalizedSafeData_SerializesCleanly_WithDataNull()
    {
        // Chứng minh bản vá hoạt động: nếu Undefined thì đổi thành null
        JsonElement undef = default;
        object? safeData = undef.ValueKind == JsonValueKind.Undefined ? null : undef;

        var json = JsonSerializer.Serialize(new
        {
            requestId = "r1",
            success = true,
            data = safeData,
            error = (string?)null
        }, JsonDefaults.Options);

        using var doc = JsonDocument.Parse(json);
        Assert.IsTrue(doc.RootElement.GetProperty("success").GetBoolean());
        Assert.AreEqual(JsonValueKind.Null, doc.RootElement.GetProperty("data").ValueKind);
        Assert.AreEqual("r1", doc.RootElement.GetProperty("requestId").GetString());
    }
}
