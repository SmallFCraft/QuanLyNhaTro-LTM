using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class UtilsInvoicesReportsContractTests
{
    private static string Wwwroot =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "wwwroot");

    [TestMethod]
    public void UtilsJs_LoadsPreviousAndSavesReading()
    {
        var js = File.ReadAllText(Path.Combine(Wwwroot, "js", "landlord", "utils.js"));
        Assert.IsTrue(js.Contains("UTILITY_GET_PREVIOUS"), "utils.js chưa tự điền chỉ số cũ");
        Assert.IsTrue(js.Contains("UTILITY_RECORD"), "utils.js chưa lưu được chỉ số");
    }

    [TestMethod]
    public void InvoicesJs_CreatesAndPays()
    {
        var js = File.ReadAllText(Path.Combine(Wwwroot, "js", "landlord", "invoices.js"));
        Assert.IsTrue(js.Contains("INVOICE_CREATE"), "invoices.js chưa lập được hóa đơn");
        Assert.IsTrue(js.Contains("INVOICE_PAY"), "invoices.js chưa thu được tiền");
        // BR-11: hóa đơn đã thu không được thao tác
        Assert.IsTrue(js.Contains("'Paid'"), "invoices.js thiếu chặn hóa đơn đã thu (BR-11)");
    }

    [TestMethod]
    public void ReportsJs_LoadsSummaryAndExports()
    {
        var js = File.ReadAllText(Path.Combine(Wwwroot, "js", "landlord", "reports.js"));
        Assert.IsTrue(js.Contains("REPORT_SUMMARY"), "reports.js chưa tải số liệu");
        Assert.IsTrue(js.Contains("EXPORT_RESIDENCE"), "reports.js chưa xuất được danh sách tạm trú");
    }
}
