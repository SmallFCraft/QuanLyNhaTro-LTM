using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class DashRoomsContractTests
{
    private static string Wwwroot =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "wwwroot");

    [TestMethod]
    public void RoomsJs_CoversFullCrudSurface()
    {
        var js = File.ReadAllText(Path.Combine(Wwwroot, "landlord", "js", "rooms.js"));
        foreach (var action in new[] { "ROOM_GET_ALL", "ROOM_ADD", "ROOM_UPDATE", "ROOM_DELETE" })
        {
            Assert.IsTrue(js.Contains(action), $"rooms.js thiếu {action}");
        }
        // BR-12: nút xóa phải bị chặn khi phòng còn người
        Assert.IsTrue(js.Contains("currentOccupants"), "rooms.js thiếu kiểm tra sức chứa để chặn xóa");
    }

    [TestMethod]
    public void DashJs_LoadsSummaryAndLists()
    {
        var js = File.ReadAllText(Path.Combine(Wwwroot, "landlord", "js", "dash.js"));
        Assert.IsTrue(js.Contains("REPORT_SUMMARY"), "dash.js chưa tải số liệu tổng quan");
        Assert.IsTrue(js.Contains("INVOICE_GET_ALL"), "dash.js chưa tải danh sách còn nợ");
        Assert.IsTrue(js.Contains("CONTRACT_GET_ALL"), "dash.js chưa tải danh sách HĐ sắp hết hạn");
    }

    [TestMethod]
    public void IndexHtml_LoadsLandlordSubScripts()
    {
        var html = File.ReadAllText(Path.Combine(Wwwroot, "landlord", "index.html"));
        Assert.IsTrue(html.Contains("js/rooms.js"), "landlord/index.html chưa nạp js/rooms.js");
        Assert.IsTrue(html.Contains("js/dash.js"), "landlord/index.html chưa nạp js/dash.js");
    }
}
