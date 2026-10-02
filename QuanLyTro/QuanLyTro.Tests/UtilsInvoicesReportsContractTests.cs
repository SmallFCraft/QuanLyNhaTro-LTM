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
        var js = File.ReadAllText(Path.Combine(Wwwroot, "chutro", "js", "utils.js"));
        Assert.IsTrue(js.Contains("DIEN_NUOC_LAY_KY_TRUOC"), "utils.js chưa tự điền chỉ số cũ");
        Assert.IsTrue(js.Contains("DIEN_NUOC_GHI_SO"), "utils.js chưa lưu được chỉ số");
    }

    [TestMethod]
    public void InvoicesJs_CreatesAndPays()
    {
        var js = File.ReadAllText(Path.Combine(Wwwroot, "chutro", "js", "hoa_don.js"));
        Assert.IsTrue(js.Contains("HOA_DON_TAO"), "hoa_don.js chưa lập được hóa đơn");
        Assert.IsTrue(js.Contains("HOA_DON_THANH_TOAN"), "hoa_don.js chưa thu được tiền");
        // BR-11: hóa đơn đã thu không được thao tác
        Assert.IsTrue(js.Contains("'DaThu'"), "hoa_don.js thiếu chặn hóa đơn đã thu (BR-11)");
    }

    [TestMethod]
    public void ReportsJs_LoadsSummaryAndExports()
    {
        var js = File.ReadAllText(Path.Combine(Wwwroot, "chutro", "js", "reports.js"));
        Assert.IsTrue(js.Contains("BAO_CAO_TONG_QUAN"), "reports.js chưa tải số liệu");
        Assert.IsTrue(js.Contains("XUAT_HO_SO_TAM_TRU"), "reports.js chưa xuất được danh sách tạm trú");
    }
}
