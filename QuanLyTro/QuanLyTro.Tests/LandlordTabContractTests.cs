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
        var html = File.ReadAllText(Path.Combine(Wwwroot, "index.html"));
        // Tab 9 tách landlord.js thành thư mục landlord/: mỗi tab một file.
        var keysInLandlordJs = File.ReadAllText(Path.Combine(Wwwroot, "js", "landlord.js"));

        string[] tabKeys = { "dash", "rooms", "tenants", "contracts", "utils", "invoices", "reports" };
        foreach (var key in tabKeys)
        {
            Assert.IsTrue(html.Contains($"id=\"tab-{key}\""), $"index.html thiếu tab-{key}");
            var declared = keysInLandlordJs.Contains($"'{key}'") || keysInLandlordJs.Contains($"{key}:");
            Assert.IsTrue(declared,
                $"landlord.js chưa khai báo tab {key} trong LANDLORD_TITLES/LANDLORD_LOADERS");
        }
    }

    /// <summary>
    /// InvoiceDto không có trường IsPaid — trạng thái nằm ở `status` (enum InvoiceStatus,
    /// serialize thành chuỗi "Paid"/"Unpaid" bởi JsonStringEnumConverter). Đọc thẳng `i.isPaid`
    /// khiến mọi hóa đơn luôn hiện "Chưa thanh toán".
    /// </summary>
    [TestMethod]
    public void LandlordInvoiceRender_DerivesPaidStateFromStatusField()
    {
        // renderInvoices() chuyển sang landlord/invoices.js từ Task 9.
        var js = File.ReadAllText(Path.Combine(Wwwroot, "js", "landlord", "invoices.js"));
        var dto = File.ReadAllText(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..",
            "QuanLyTro.Shared", "Models", "InvoiceDto.cs"));

        Assert.IsTrue(dto.Contains("InvoiceStatus Status"),
            "InvoiceDto đổi hình dạng — cập nhật invoices.js và test này.");
        Assert.IsTrue(js.Contains("'Paid'"),
            "invoices.js phải suy trạng thái đã thu từ trường status, không phải isPaid.");
        Assert.IsFalse(js.Contains("i.isPaid"),
            "invoices.js không được đọc isPaid — InvoiceDto không có trường đó.");
    }
}
