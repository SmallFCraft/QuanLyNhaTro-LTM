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
        var packet = RequestPacket.Create(ActionNames.PhongThem, null,
            new PhongDto(0, "P101", 2_500_000m, 2, TrangThaiPhong.Trong, null, 0));
        var json = JsonSerializer.Serialize(packet, JsonDefaults.Options);
        var result = JsonSerializer.Deserialize<RequestPacket>(json, JsonDefaults.Options)!;
        Assert.AreEqual("P101", result.GetData<PhongDto>().SoPhong);
        Assert.AreEqual(ActionNames.PhongThem, result.Action);
    }

    [TestMethod]
    public void ResponsePacket_OkAndFailRoundTrip()
    {
        var ok = ResponsePacket.Ok(new[] { new PhongDto(1, "P1", 1_000_000m, 2, TrangThaiPhong.Trong, null, 0) });
        var okJson = JsonSerializer.Serialize(ok, JsonDefaults.Options);
        var okBack = JsonSerializer.Deserialize<ResponsePacket>(okJson, JsonDefaults.Options)!;
        Assert.IsTrue(okBack.Success);
        Assert.AreEqual(1, okBack.Data!.Value.Deserialize<List<PhongDto>>(JsonDefaults.Options)![0].Id);

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
        var contract = new HopDongDto(0, 1, 2, new DateOnly(2026, 9, 1), new DateOnly(2027, 9, 1),
            2_000_000m, 4_000_000m, TrangThaiHopDong.HieuLuc, null);
        var packet = RequestPacket.Create(ActionNames.HopDongTao, "tok", contract);
        var back = JsonSerializer.Deserialize<RequestPacket>(
            JsonSerializer.Serialize(packet, JsonDefaults.Options), JsonDefaults.Options)!;
        var parsed = back.GetData<HopDongDto>();
        Assert.AreEqual(new DateOnly(2027, 9, 1), parsed.NgayKetThuc);
        Assert.AreEqual(2_000_000m, parsed.GiaThue);
    }

    [TestMethod]
    public void ActionNames_ContainsPoliceActions_AndUserRoleHasPolice()
    {
        Assert.IsTrue(ActionNames.All.Contains(ActionNames.LichSuCuTruLay));
        Assert.IsTrue(ActionNames.All.Contains(ActionNames.XuatLichSuCuTru));
        Assert.IsTrue(Enum.IsDefined(typeof(VaiTroNguoiDung), VaiTroNguoiDung.CongAn));
    }

    [TestMethod]
    public void ActionNames_ContainsNewPermissionActions()
    {
        CollectionAssert.Contains((System.Collections.ICollection)ActionNames.All, ActionNames.PhanQuyenLayMaTran);
        CollectionAssert.Contains((System.Collections.ICollection)ActionNames.All, ActionNames.PhanQuyenCapNhatVaiTro);
    }

    [TestMethod]
    public void UserRole_ContainsManager()
    {
        Assert.AreEqual(3, (int)VaiTroNguoiDung.QuanLy);
    }
}
