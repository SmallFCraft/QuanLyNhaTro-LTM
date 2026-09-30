using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Server.Data;

namespace QuanLyTro.Tests;

[TestClass]
public class SanityTests
{
    private static string Wwwroot =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "wwwroot");
    [TestMethod]
    public void Test_DefaultPort()
    {
        Assert.AreEqual(8888, Shared.SharedConstants.DefaultPort);
    }

    [TestMethod]
    public void DescribeFailure_DbDown_TellsUserToStartLaragon()
    {
        // MySqlException 1042 = Unable to connect to any MySQL hosts.
        var message = Database.DescribeFailure(
            1042, "Unable to connect to any of the specified MySQL hosts.");
        StringAssert.Contains(message, "MySQL");
        StringAssert.Contains(message, "Laragon");
    }

    [TestMethod]
    public void DescribeFailure_OtherDbError_HidesDetails()
    {
        var message = Database.DescribeFailure(1062, "Duplicate entry 'P101' for key 'room_number'");
        Assert.IsFalse(message.Contains("P101"), "Không lộ dữ liệu nội bộ ra Client.");
        Assert.IsFalse(message.Contains("Duplicate"));
    }

    [TestMethod]
    public void RoomsModal_StatusOptions_CoversAllThreeStates()
    {
        // Tránh tái diễn: form thêm phòng cũ từng crash ArgumentOutOfRangeException vì
        // combo trạng thái không được nạp item nào. UI giờ là WebView2 (Assets/wwwroot),
        // nên khóa contract của modal thêm/sửa phòng thay vì dựng WinForms control.
        var js = File.ReadAllText(Path.Combine(Wwwroot, "landlord", "js", "rooms.js"));

        Assert.IsTrue(js.Contains("'Available'"), "Modal thiếu trạng thái Trống");
        Assert.IsTrue(js.Contains("'Rented'"), "Modal thiếu trạng thái Đang thuê");
        Assert.IsTrue(js.Contains("'Maintenance'"), "Modal thiếu trạng thái Bảo trì");

        // Bộ lọc trên thanh công cụ phải có đủ 3 lựa chọn + mặc định "Tất cả".
        var html = File.ReadAllText(Path.Combine(Wwwroot, "landlord", "index.html"));
        var selectStart = html.IndexOf("id=\"room-status\"", System.StringComparison.Ordinal);
        Assert.IsTrue(selectStart >= 0, "landlord/index.html thiếu bộ lọc #room-status");
        var selectTag = html[selectStart..html.IndexOf("</select>", selectStart, System.StringComparison.Ordinal)];
        Assert.AreEqual(3, selectTag.Split("value=\"Available\"").Length - 1
                         + selectTag.Split("value=\"Rented\"").Length - 1
                         + selectTag.Split("value=\"Maintenance\"").Length - 1,
            "Bộ lọc #room-status phải có đủ Trống, Đang thuê, Bảo trì.");
        Assert.IsTrue(selectTag.Contains("value=\"\""), "Bộ lọc #room-status thiếu lựa chọn mặc định.");
    }
}
