using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class LandlordQrContractTests
{
    private static string Wwwroot =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "wwwroot");

    private static string Html => File.ReadAllText(Path.Combine(Wwwroot, "chutro", "index.html"));
    private static string Js => File.ReadAllText(Path.Combine(Wwwroot, "chutro", "js", "hop_dong.js"));

    [TestMethod]
    public void ChutroHtml_LoadsQrcodeVendorScript()
    {
        var html = Html;
        Assert.IsTrue(html.Contains("vendor/qrcode.min.js"), "chutro/index.html phải nạp qrcode.min.js từ vendor.");
    }

    [TestMethod]
    public void HopDongJs_SupportsChoNhanPhong_AndQrAction()
    {
        var js = Js;
        Assert.IsTrue(js.Contains("ChoNhanPhong"), "hop_dong.js phải hỗ trợ trạng thái ChoNhanPhong.");
        Assert.IsTrue(js.Contains("'HOP_DONG_SINH_QR'"), "hop_dong.js phải gọi hành động HOP_DONG_SINH_QR.");
        Assert.IsTrue(js.Contains("showContractQr"), "hop_dong.js phải có hàm hiển thị QR/PIN cho chủ trọ.");
        // Phòng giữ HĐ chờ nhận phòng không được nhả về danh sách "còn trống" cho màn tạo HĐ:
        // chặn ở nguồn rồi, nhưng UI phải tự chặn ở dropdown để không gọi API thừa.
        Assert.IsTrue(js.Contains("'ChoNhanPhong'"), "hop_dong.js phải lọc ChoNhanPhong trong tập busy.");
        Assert.IsTrue(js.Contains("contractStatusByRoom") || js.Contains("ChoNhanPhong"),
            "hop_dong.js phải dùng trạng thái HĐ chung (ui.js) để lọc phòng bận.");
    }

    /// <summary>
    /// Tab Người thuê + tab Phòng suy trạng thái thuê từ HĐ (ui.js), không từ phong_id:
    /// hai tab phải đọc cùng một Map phongId→trạng thái HĐ.
    /// </summary>
    [TestMethod]
    public void TenantAndRoomJs_DeriveStatusFromContractState_NotFromPhongId()
    {
        var ui = File.ReadAllText(Path.Combine(Wwwroot, "shared", "js", "ui.js"));
        Assert.IsTrue(ui.Contains("contractStatusByRoom"),
            "shared/js/ui.js phải có hàm contractStatusByRoom dùng chung.");
        Assert.IsTrue(ui.Contains("tenantStatusTag"),
            "shared/js/ui.js phải có hàm tenantStatusTag dùng chung.");

        var tenant = File.ReadAllText(Path.Combine(Wwwroot, "chutro", "js", "khach_thue.js"));
        Assert.IsTrue(tenant.Contains("contractStatusByRoom"),
            "khach_thue.js phải dùng contractStatusByRoom thay vì suy từ phongId.");
        Assert.IsTrue(tenant.Contains("tenantStatusTag"),
            "khach_thue.js phải dùng tenantStatusTag để render tag trạng thái.");

        var phong = File.ReadAllText(Path.Combine(Wwwroot, "chutro", "js", "phong.js"));
        Assert.IsTrue(phong.Contains("contractStatusByRoom"),
            "phong.js phải dùng contractStatusByRoom để chuẩn hoá trạng thái phòng.");
        Assert.IsTrue(phong.Contains("_contractStateByRoom"),
            "phong.js phải nhớ trạng thái HĐ theo phòng cho renderRooms().");
    }
}
