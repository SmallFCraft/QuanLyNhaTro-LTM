using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class LandlordTabContractTests
{
    private static string Wwwroot =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "wwwroot");

    [TestMethod]
    public void LandlordHtml_HasSevenTabContainers_AndJsReferencesEachTabKey()
    {
        var html = File.ReadAllText(Path.Combine(Wwwroot, "chutro", "index.html"));
        // landlord.js thành chutro/js/main.js: mỗi tab một file.
        var keysInLandlordJs = File.ReadAllText(Path.Combine(Wwwroot, "chutro", "js", "main.js"));

        string[] tabKeys = { "dash", "phong", "khach_thue", "hop_dong", "utils", "hoa_don", "reports" };
        foreach (var key in tabKeys)
        {
            Assert.IsTrue(html.Contains($"id=\"tab-{key}\""), $"index.html thiếu tab-{key}");
            var declared = keysInLandlordJs.Contains($"'{key}'") || keysInLandlordJs.Contains($"{key}:");
            Assert.IsTrue(declared,
                $"chutro/js/main.js chưa khai báo tab {key} trong LANDLORD_TITLES/LANDLORD_LOADERS");
        }
    }

    /// <summary>
    /// HoaDonDto không có trường IsPaid — trạng thái nằm ở `trang_thai` (enum TrangThaiHoaDon,
    /// serialize thành chuỗi "DaThu"/"ChuaThu" bởi JsonStringEnumConverter). Đọc thẳng `i.isPaid`
    /// khiến mọi hóa đơn luôn hiện "Chưa thanh toán".
    /// </summary>
    [TestMethod]
    public void LandlordInvoiceRender_DerivesPaidStateFromStatusField()
    {
        // renderInvoices() nằm ở chutro/js/hoa_don.js.
        var js = File.ReadAllText(Path.Combine(Wwwroot, "chutro", "js", "hoa_don.js"));
        var dto = File.ReadAllText(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..",
            "QuanLyTro.Shared", "Models", "HoaDonDto.cs"));

        Assert.IsTrue(dto.Contains("TrangThaiHoaDon TrangThai"),
            "HoaDonDto đổi hình dạng — cập nhật hoa_don.js và test này.");
        Assert.IsTrue(js.Contains("'DaThu'"),
            "hoa_don.js phải suy trạng thái đã thu từ trường trang_thai, không phải isPaid.");
        Assert.IsFalse(js.Contains("i.isPaid"),
            "hoa_don.js không được đọc isPaid — HoaDonDto không có trường đó.");
    }

    [TestMethod]
    public void Menustrip_HasWorkingNavigationHandlers()
    {
        var html = File.ReadAllText(Path.Combine(Wwwroot, "chutro", "index.html"));
        // Menu item "Báo cáo" phải gọi loadLandlordTab('reports')
        Assert.IsTrue(html.Contains("loadLandlordTab('reports')"),
            "Menu item Báo cáo chưa có onclick nhảy tới tab reports");
    }
}
