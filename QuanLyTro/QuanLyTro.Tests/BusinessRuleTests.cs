using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Server.Services;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class BusinessRuleTests
{
    [DataTestMethod]
    [DataRow("", 1000000, 2)]
    [DataRow("P1", 0, 2)]
    [DataRow("P1", -1, 2)]
    [DataRow("P1", 1000000, 0)]
    [DataRow("P1", 1000000, -1)]
    public void RoomService_RejectsInvalidRoom(string number, double price, int capacity)
    {
        Assert.ThrowsException<BusinessRuleException>(() =>
            RoomService.Validate(new RoomDto(0, number, (decimal)price, capacity, RoomStatus.Available, null, 0)));
    }

    [TestMethod]
    public void RoomService_AcceptsValidRoom()
    {
        var room = new RoomDto(0, "P101", 2_500_000m, 2, RoomStatus.Available, "Tầng 1", 0);
        RoomService.Validate(room);
    }

    private static TenantDto Tenant(
        int id = 0, int? roomId = 1, string fullName = "Nguyễn Văn A", string idCard = "012345678901",
        string phone = "0901234567", string hometown = "Đà Nẵng") =>
        new(id, roomId, fullName, new DateOnly(2000, 1, 1), idCard, phone, hometown, null, false);

    [DataTestMethod]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("01234567890")]    // 11 số
    [DataRow("0123456789012")]  // 13 số
    [DataRow("01234567890a")]   // có chữ
    public void TenantService_RejectsInvalidIdCard(string idCard)
    {
        Assert.ThrowsException<BusinessRuleException>(() =>
            TenantService.Validate(Tenant(idCard: idCard)));
    }

    [DataTestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void TenantService_RejectsEmptyFullName(string fullName)
    {
        Assert.ThrowsException<BusinessRuleException>(() =>
            TenantService.Validate(Tenant(fullName: fullName)));
    }

    [DataTestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void TenantService_RejectsEmptyPhone(string phone)
    {
        Assert.ThrowsException<BusinessRuleException>(() =>
            TenantService.Validate(Tenant(phone: phone)));
    }

    [DataTestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void TenantService_RejectsEmptyHometown(string hometown)
    {
        Assert.ThrowsException<BusinessRuleException>(() =>
            TenantService.Validate(Tenant(hometown: hometown)));
    }

    [TestMethod]
    public void TenantService_AcceptsValidTenant()
    {
        TenantService.Validate(Tenant());
    }

    [TestMethod]
    public void TenantService_DefaultPasswordIsLastSixDigitsOfIdCard()
    {
        Assert.AreEqual("678901", TenantService.DefaultPassword("012345678901"));
    }
}
