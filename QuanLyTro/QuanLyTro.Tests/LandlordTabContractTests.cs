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

    [TestMethod]
    public void LandlordTenantTab_IncludesUnassignedQuery_AndStatusColumn()
    {
        var html = File.ReadAllText(Path.Combine(Wwwroot, "chutro", "index.html"));
        var js = File.ReadAllText(Path.Combine(Wwwroot, "chutro", "js", "khach_thue.js"));

        // 1. Phải đổi tên tab thành "Người dùng"
        Assert.IsTrue(html.Contains("Người dùng"), "Tab khach_thue phải mang tên 'Người dùng'");
        Assert.IsTrue(html.Contains("id=\"cnt-khach_thue\""), "Tab khach_thue phải có badge đếm id='cnt-khach_thue'");

        // 2. Bảng người dùng (#tab-khach_thue) phải có cột Trạng thái — cô lập theo container
        // để tránh xanh giả do ăn theo tab hop_dong / hoa_don.
        var tenantTabHtml = ExtractContainer(html, "tab-khach_thue");
        Assert.IsTrue(tenantTabHtml.Contains("<th>Trạng thái</th>"),
            "Bảng người dùng (#tab-khach_thue) phải có cột header '<th>Trạng thái</th>'");

        // 3. JS phải gọi lấy khách chưa có phòng (phongId: 0)
        Assert.IsTrue(js.Contains("phongId: 0") || js.Contains("phongId:0"),
            "khach_thue.js phải gọi KHACH_THUE_THEO_PHONG với phongId: 0 để lấy khách chưa gán phòng");

        // 4. JS phải render trạng thái thuê
        Assert.IsTrue(js.Contains("Chờ nhận phòng") && js.Contains("Đang thuê"),
            "khach_thue.js phải render nhãn trạng thái 'Chờ nhận phòng' và 'Đang thuê'");
    }

    private static string ExtractContainer(string html, string containerId)
    {
        var marker = $"id=\"{containerId}\"";
        var idx = html.IndexOf(marker, StringComparison.Ordinal);
        if (idx < 0) return string.Empty;
        // Lấy đoạn HTML từ vị trí tab container đến container tab kế tiếp hoặc 1500 ký tự
        var nextTabIdx = html.IndexOf("id=\"tab-", idx + marker.Length, StringComparison.Ordinal);
        var end = nextTabIdx > idx ? nextTabIdx : Math.Min(html.Length, idx + 1500);
        return html[idx..end];
    }
}
