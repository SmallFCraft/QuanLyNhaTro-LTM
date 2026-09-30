using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Shared.Models;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Tests;

[TestClass]
public class ProtocolTests
{
    [TestMethod]
    public void RequestPacket_RoundTripsTypedData()
    {
        var packet = RequestPacket.Create(ActionNames.RoomAdd, null,
            new RoomDto(0, "P101", 2_500_000m, 2, RoomStatus.Available, null, 0));
        var json = JsonSerializer.Serialize(packet, JsonDefaults.Options);
        var result = JsonSerializer.Deserialize<RequestPacket>(json, JsonDefaults.Options)!;
        Assert.AreEqual("P101", result.GetData<RoomDto>().RoomNumber);
        Assert.AreEqual(ActionNames.RoomAdd, result.Action);
    }

    [TestMethod]
    public void ResponsePacket_OkAndFailRoundTrip()
    {
        var ok = ResponsePacket.Ok(new[] { new RoomDto(1, "P1", 1_000_000m, 2, RoomStatus.Available, null, 0) });
        var okJson = JsonSerializer.Serialize(ok, JsonDefaults.Options);
        var okBack = JsonSerializer.Deserialize<ResponsePacket>(okJson, JsonDefaults.Options)!;
        Assert.IsTrue(okBack.Success);
        Assert.AreEqual(1, okBack.Data!.Value.Deserialize<List<RoomDto>>(JsonDefaults.Options)![0].Id);

        var fail = ResponsePacket.Fail("Số phòng đã tồn tại.");
        var failBack = JsonSerializer.Deserialize<ResponsePacket>(
            JsonSerializer.Serialize(fail, JsonDefaults.Options), JsonDefaults.Options)!;
        Assert.IsFalse(failBack.Success);
        Assert.IsNull(failBack.Data);
        Assert.AreEqual("Số phòng đã tồn tại.", failBack.Message);
    }

    [TestMethod]
    public void Packet_SerializesDateOnlyAndDecimal()
    {
        var contract = new ContractDto(0, 1, 2, new DateOnly(2026, 9, 1), new DateOnly(2027, 9, 1),
            2_000_000m, 4_000_000m, ContractStatus.Active, null);
        var packet = RequestPacket.Create(ActionNames.ContractCreate, "tok", contract);
        var back = JsonSerializer.Deserialize<RequestPacket>(
            JsonSerializer.Serialize(packet, JsonDefaults.Options), JsonDefaults.Options)!;
        var parsed = back.GetData<ContractDto>();
        Assert.AreEqual(new DateOnly(2027, 9, 1), parsed.EndDate);
        Assert.AreEqual(2_000_000m, parsed.RentalPrice);
    }

    [TestMethod]
    public void ActionNames_ContainsPoliceActions_AndUserRoleHasPolice()
    {
        Assert.IsTrue(ActionNames.All.Contains(ActionNames.ResidenceHistoryGet));
        Assert.IsTrue(ActionNames.All.Contains(ActionNames.ExportResidenceHistory));
        Assert.IsTrue(Enum.IsDefined(typeof(UserRole), UserRole.Police));
    }
}
