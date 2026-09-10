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
}
