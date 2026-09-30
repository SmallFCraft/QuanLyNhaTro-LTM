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
        var js = File.ReadAllText(Path.Combine(Wwwroot, "js", "landlord.js"));

        string[] tabKeys = { "dash", "rooms", "tenants", "contracts", "utils", "invoices", "reports" };
        foreach (var key in tabKeys)
        {
            Assert.IsTrue(html.Contains($"id=\"tab-{key}\""), $"index.html thiếu tab-{key}");
            Assert.IsTrue(js.Contains($"'{key}'"), $"landlord.js chưa xử lý tab {key}");
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
        var js = File.ReadAllText(Path.Combine(Wwwroot, "js", "landlord.js"));
        var dto = File.ReadAllText(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..",
            "QuanLyTro.Shared", "Models", "InvoiceDto.cs"));

        Assert.IsTrue(dto.Contains("InvoiceStatus Status"),
            "InvoiceDto đổi hình dạng — cập nhật landlord.js và test này.");
        Assert.IsTrue(js.Contains("i.status === 'Paid'"),
            "landlord.js phải suy trạng thái đã thu từ trường status, không phải isPaid.");
    }
}
