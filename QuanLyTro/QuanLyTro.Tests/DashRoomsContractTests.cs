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
        var js = File.ReadAllText(Path.Combine(Wwwroot, "chutro", "js", "phong.js"));
        foreach (var hanh_dong in new[] { "PHONG_LAY_TAT_CA", "PHONG_THEM", "PHONG_CAP_NHAT", "PHONG_XOA" })
        {
            Assert.IsTrue(js.Contains(hanh_dong), $"phong.js thiếu {hanh_dong}");
        }
        // BR-12: nút xóa phải bị chặn khi phòng còn người
        Assert.IsTrue(js.Contains("soNguoiHienTai"), "phong.js thiếu kiểm tra sức chứa để chặn xóa");
    }

    [TestMethod]
    public void DashJs_LoadsSummaryAndLists()
    {
        var js = File.ReadAllText(Path.Combine(Wwwroot, "chutro", "js", "dash.js"));
        Assert.IsTrue(js.Contains("BAO_CAO_TONG_QUAN"), "dash.js chưa tải số liệu tổng quan");
        Assert.IsTrue(js.Contains("HOA_DON_LAY_TAT_CA"), "dash.js chưa tải danh sách còn nợ");
        Assert.IsTrue(js.Contains("HOP_DONG_LAY_TAT_CA"), "dash.js chưa tải danh sách HĐ sắp hết hạn");
    }

    [TestMethod]
    public void DashCharts_HaveContainersAndRenderHooks()
    {
        var html = File.ReadAllText(Path.Combine(Wwwroot, "chutro", "index.html"));
        Assert.IsTrue(html.Contains("id=\"dash-rev-chart\""), "tổng quan thiếu khung biểu đồ doanh thu");
        Assert.IsTrue(html.Contains("id=\"dash-room-pie\""), "tổng quan thiếu biểu đồ tròn tình trạng phòng");
        Assert.IsTrue(html.Contains("id=\"dash-money-pie\""), "tổng quan thiếu biểu đồ tròn thu/còn nợ");

        var js = File.ReadAllText(Path.Combine(Wwwroot, "chutro", "js", "dash.js"));
        Assert.IsTrue(js.Contains("renderDashCharts"), "dash.js chưa vẽ biểu đồ tổng quan");
        Assert.IsTrue(js.Contains("renderPie('dash-room-pie'"), "dash.js chưa nạp biểu đồ tròn phòng");
        Assert.IsTrue(js.Contains("renderPie('dash-money-pie'"), "dash.js chưa nạp biểu đồ tròn thu/nợ");
        // Múi phải là phần tử riêng để hover/<title> được từng phần (conic-gradient không làm được).
        Assert.IsTrue(js.Contains("class=\"pie-seg\""), "múi biểu đồ tròn phải là <circle> riêng để hover");
    }

    [TestMethod]
    public void Shells_LockViewportAndScrollOnlyContent()
    {
        // Header + tabs + statusstrip không được trôi khi cuộn — .content là vùng cuộn duy nhất.
        foreach (var cssFile in new[] { "chutro/chutro.css", "congan/congan.css", "khachthue/khachthue.css" })
        {
            var css = File.ReadAllText(Path.Combine(Wwwroot, cssFile));
            Assert.IsTrue(css.Contains("overflow-y:auto"), $"{cssFile} thiếu vùng cuộn cho .content");
            Assert.IsTrue(css.Contains("body:has(> .win){height:100vh;overflow:hidden}"),
                $"{cssFile} chưa khóa body để giữ header/footer cố định");
        }
    }

    [TestMethod]
    public void IndexHtml_LoadsLandlordSubScripts()
    {
        var html = File.ReadAllText(Path.Combine(Wwwroot, "chutro", "index.html"));
        Assert.IsTrue(html.Contains("js/phong.js"), "chutro/index.html chưa nạp js/phong.js");
        Assert.IsTrue(html.Contains("js/dash.js"), "chutro/index.html chưa nạp js/dash.js");
    }
}
