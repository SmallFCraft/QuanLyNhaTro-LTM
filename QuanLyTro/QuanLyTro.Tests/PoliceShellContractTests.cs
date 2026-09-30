using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class PoliceShellContractTests
{
    private static string Wwwroot =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "wwwroot");

    private static string PoliceHtml => Path.Combine(Wwwroot, "police", "index.html");

    /// <summary>Mọi action ghi mà Công an phường tuyệt đối không được gọi (BR-16).</summary>
    private static readonly string[] ForbiddenWriteActions =
    {
        "ROOM_ADD", "ROOM_UPDATE", "ROOM_DELETE",
        "TENANT_ADD", "TENANT_UPDATE", "TENANT_CHECKOUT", "TENANT_DELETE",
        "CONTRACT_CREATE", "CONTRACT_TERMINATE", "CONTRACT_RENEW",
        "UTILITY_RECORD",
        "INVOICE_CREATE", "INVOICE_PAY"
    };

    [TestMethod]
    public void PoliceShell_HasThreeTabs_AndNoWriteButtons()
    {
        var html = File.ReadAllText(PoliceHtml);
        var start = html.IndexOf("id=\"police\"", System.StringComparison.Ordinal);
        Assert.IsTrue(start >= 0, "police/index.html thiếu cửa sổ #police");

        var policeBlock = html.Substring(start, html.Length - start);
        string[] tabKeys = { "citizens", "residence", "history" };
        foreach (var key in tabKeys)
        {
            Assert.IsTrue(policeBlock.Contains($"id=\"ptab-{key}\""), $"Thiếu tab công an: ptab-{key}");
        }

        // BR-16: không được có nút ghi trong shell công an
        foreach (var verb in new[] { "Thêm", "Sửa", "Xóa", "Lập hợp đồng", "Thu tiền" })
        {
            Assert.IsFalse(Regex.IsMatch(policeBlock, $">{verb}<"), $"Shell công an có nút ghi: {verb}");
        }
    }

    /// <summary>
    /// C2 (spec §3.2): token phiên chỉ sống trong C#. Không file JS/HTML nào được đọc
    /// `token` từ đối tượng người dùng trả về AUTH_LOGIN.
    /// </summary>
    [TestMethod]
    public void FrontEnd_NeverReadsSessionToken()
    {
        // Quét đệ quy toàn bộ wwwroot: module mới thêm vào không được lọt lưới.
        var files = Directory.EnumerateFiles(Wwwroot, "*.*", SearchOption.AllDirectories)
            .Where(f => f.EndsWith(".js") || f.EndsWith(".html"));

        foreach (var file in files)
        {
            var text = File.ReadAllText(file);
            Assert.IsFalse(
                Regex.IsMatch(text, @"\b\w+\.(token|Token)\b"),
                $"{Path.GetFileName(file)} đọc trường token — token phải ở lại phía C#.");
            Assert.IsFalse(
                Regex.IsMatch(text, @"localStorage|sessionStorage"),
                $"{Path.GetFileName(file)} dùng storage trình duyệt — token không được lưu ở đó.");
        }
    }

    [TestMethod]
    public void PoliceJs_SearchInputAndDateRangeAreAddressableById()
    {
        var html = File.ReadAllText(PoliceHtml);
        Assert.IsTrue(html.Contains("id=\"p-search\""), "ô tìm kiếm công an cần id để JS đọc");
        Assert.IsTrue(html.Contains("id=\"p-from\""), "ô ngày bắt đầu cần id");
        Assert.IsTrue(html.Contains("id=\"p-to\""), "ô ngày kết thúc cần id");
    }

    [TestMethod]
    public void PoliceJs_UsesDocumentElementIds_AndNoWriteActions()
    {
        var js = File.ReadAllText(Path.Combine(Wwwroot, "js", "police.js"));
        Assert.IsTrue(js.Contains("document.getElementById('p-search')"), "police.js phải đọc #p-search bằng getElementById");
        Assert.IsTrue(js.Contains("document.getElementById('p-from')"), "police.js phải đọc #p-from bằng getElementById");
        Assert.IsTrue(js.Contains("document.getElementById('p-to')"), "police.js phải đọc #p-to bằng getElementById");

        // BR-16: shell công an không gọi bất kỳ action ghi nào
        foreach (var action in ForbiddenWriteActions)
        {
            Assert.IsFalse(js.Contains(action), $"police.js vi phạm BR-16 — chứa action ghi: {action}");
        }
    }

    /// <summary>
    /// Nút "Xuất danh sách" ở tab Công dân phải xuất danh sách công dân đang lọc,
    /// không phải lịch sử biến động (nút đó nằm ở tab Biến động).
    /// </summary>
    [TestMethod]
    public void CitizensTab_ExportButton_ExportsCitizensNotHistory()
    {
        var html = File.ReadAllText(PoliceHtml);
        var js = File.ReadAllText(Path.Combine(Wwwroot, "js", "police.js"));

        Assert.IsTrue(js.Contains("function exportCitizens"), "police.js thiếu exportCitizens()");
        Assert.IsTrue(html.Contains("onclick=\"exportCitizens()\""),
            "police/index.html chưa nối nút Xuất danh sách sang exportCitizens()");

        // Không được để nút Xuất danh sách gọi nhầm export lịch sử: tab Công dân phải
        // đứng trước tab Biến động trong DOM, và nút export đầu tiên phải là exportCitizens.
        var citizens = html.IndexOf("id=\"ptab-citizens\"", System.StringComparison.Ordinal);
        var history = html.IndexOf("id=\"ptab-history\"", System.StringComparison.Ordinal);
        Assert.IsTrue(citizens >= 0 && history > citizens, "thứ tự tab công an đã đổi");
        var firstExport = html.IndexOf("onclick=\"export", citizens, System.StringComparison.Ordinal);
        Assert.IsTrue(firstExport > 0 && firstExport < history,
            "nút export đầu tiên trong vùng Công dân phải là exportCitizens()");
    }
}
