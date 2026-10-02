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
        var message = Database.DescribeFailure(1062, "Duplicate entry 'P101' for key 'so_phong'");
        Assert.IsFalse(message.Contains("P101"), "Không lộ dữ liệu nội bộ ra Client.");
        Assert.IsFalse(message.Contains("Duplicate"));
    }

    [TestMethod]
    public void RoomsModal_StatusOptions_CoversAllThreeStates()
    {
        // Tránh tái diễn: form thêm phòng cũ từng crash ArgumentOutOfRangeException vì
        // combo trạng thái không được nạp item nào. UI giờ là WebView2 (Assets/wwwroot),
        // nên khóa contract của modal thêm/sửa phòng thay vì dựng WinForms control.
        var js = File.ReadAllText(Path.Combine(Wwwroot, "chutro", "js", "phong.js"));

        Assert.IsTrue(js.Contains("'Trong'"), "Modal thiếu trạng thái Trống");
        Assert.IsTrue(js.Contains("'DaThue'"), "Modal thiếu trạng thái Đang thuê");
        Assert.IsTrue(js.Contains("'BaoTri'"), "Modal thiếu trạng thái Bảo trì");

        // Bộ lọc trên thanh công cụ phải có đủ 3 lựa chọn + mặc định "Tất cả".
        var html = File.ReadAllText(Path.Combine(Wwwroot, "chutro", "index.html"));
        var selectStart = html.IndexOf("id=\"room-trang_thai\"", System.StringComparison.Ordinal);
        Assert.IsTrue(selectStart >= 0, "chutro/index.html thiếu bộ lọc #room-trang_thai");
        var selectTag = html[selectStart..html.IndexOf("</select>", selectStart, System.StringComparison.Ordinal)];
        Assert.AreEqual(3, selectTag.Split("value=\"Trong\"").Length - 1
                         + selectTag.Split("value=\"DaThue\"").Length - 1
                         + selectTag.Split("value=\"BaoTri\"").Length - 1,
            "Bộ lọc #room-trang_thai phải có đủ Trống, Đang thuê, Bảo trì.");
        Assert.IsTrue(selectTag.Contains("value=\"\""), "Bộ lọc #room-trang_thai thiếu lựa chọn mặc định.");
    }
}
