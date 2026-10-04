using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class PoliceShellContractTests
{
    private static string Wwwroot =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "wwwroot");

    private static string PoliceHtml => Path.Combine(Wwwroot, "congan", "index.html");

    /// <summary>Mọi hanh_dong ghi mà Công an phường tuyệt đối không được gọi (BR-16).</summary>
    private static readonly string[] ForbiddenWriteActions =
    {
        "PHONG_THEM", "PHONG_CAP_NHAT", "PHONG_XOA",
        "KHACH_THUE_THEM", "KHACH_THUE_CAP_NHAT", "KHACH_THUE_TRA_PHONG", "KHACH_THUE_XOA",
        "HOP_DONG_TAO", "HOP_DONG_CHAM_DUT", "HOP_DONG_GIA_HAN",
        "DIEN_NUOC_GHI_SO",
        "HOA_DON_TAO", "HOA_DON_THANH_TOAN"
    };

    [TestMethod]
    public void PoliceShell_HasThreeTabs_AndNoWriteButtons()
    {
        var html = File.ReadAllText(PoliceHtml);
        var start = html.IndexOf("id=\"congan\"", System.StringComparison.Ordinal);
        Assert.IsTrue(start >= 0, "congan/index.html thiếu cửa sổ #congan");

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
    /// `token` từ đối tượng người dùng trả về DANG_NHAP.
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
        // Tìm kiếm ở tab Công dân, khoảng ngày ở tab Biến động.
        var citizens = File.ReadAllText(Path.Combine(Wwwroot, "congan", "js", "citizens.js"));
        var history = File.ReadAllText(Path.Combine(Wwwroot, "congan", "js", "history.js"));
        Assert.IsTrue(citizens.Contains("document.getElementById('p-search')"), "citizens.js phải đọc #p-search bằng getElementById");
        Assert.IsTrue(history.Contains("document.getElementById('p-from')"), "history.js phải đọc #p-from bằng getElementById");
        Assert.IsTrue(history.Contains("document.getElementById('p-to')"), "history.js phải đọc #p-to bằng getElementById");

        // BR-16: shell công an không gọi bất kỳ hanh_dong ghi nào — quét mọi script của module.
        var policeJs = Directory.GetFiles(Path.Combine(Wwwroot, "congan", "js"), "*.js");
        foreach (var file in policeJs)
        {
            var js = File.ReadAllText(file);
            foreach (var hanh_dong in ForbiddenWriteActions)
            {
                Assert.IsFalse(js.Contains(hanh_dong),
                    $"{Path.GetFileName(file)} vi phạm BR-16 — chứa hanh_dong ghi: {hanh_dong}");
            }
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
        var js = File.ReadAllText(Path.Combine(Wwwroot, "congan", "js", "citizens.js"));

        Assert.IsTrue(js.Contains("function exportCitizens"), "citizens.js thiếu exportCitizens()");
        Assert.IsTrue(html.Contains("onclick=\"exportCitizens()\""),
            "congan/index.html chưa nối nút Xuất danh sách sang exportCitizens()");

        // Không được để nút Xuất danh sách gọi nhầm export lịch sử: tab Công dân phải
        // đứng trước tab Biến động trong DOM, và nút export đầu tiên phải là exportCitizens.
        var citizens = html.IndexOf("id=\"ptab-citizens\"", System.StringComparison.Ordinal);
        var history = html.IndexOf("id=\"ptab-history\"", System.StringComparison.Ordinal);
        Assert.IsTrue(citizens >= 0 && history > citizens, "thứ tự tab công an đã đổi");
        var firstExport = html.IndexOf("onclick=\"export", citizens, System.StringComparison.Ordinal);
        Assert.IsTrue(firstExport > 0 && firstExport < history,
            "nút export đầu tiên trong vùng Công dân phải là exportCitizens()");
    }

    /// <summary>
    /// Đồng bộ dynamic police shell: 4 KPI không giữ số tĩnh, dropdown phòng dựng từ dữ liệu thật,
    /// footer Công dân đếm từ render, ngày Biến động mặc định từ tháng hiện tại.
    /// </summary>
    [TestMethod]
    public void PoliceShell_DerivesKpisAndFiltersFromLiveData()
    {
        var html = File.ReadAllText(PoliceHtml);
        var mainJs = File.ReadAllText(Path.Combine(Wwwroot, "congan", "js", "main.js"));
        var citizensJs = File.ReadAllText(Path.Combine(Wwwroot, "congan", "js", "citizens.js"));
        var historyJs = File.ReadAllText(Path.Combine(Wwwroot, "congan", "js", "history.js"));

        // 1. 4 KPI có id động để JS gán từ cache thật.
        foreach (var id in new[] { "kpi-rooms", "kpi-tenants", "kpi-registered", "kpi-unregistered" })
        {
            Assert.IsTrue(html.Contains($"id=\"{id}\""), $"congan/index.html thiếu KPI động id='{id}'");
        }
        Assert.IsTrue(mainJs.Contains("renderPoliceKpis"),
            "congan/js/main.js thiếu renderPoliceKpis() — KPI vẫn là số tĩnh");
        // Số tĩnh 24/41/36/5 không được nằm trong thẻ .val.
        Assert.IsFalse(System.Text.RegularExpressions.Regex.IsMatch(html, "class=\"val\">[0-9]"),
            "congan/index.html vẫn chứa số tĩnh trong thẻ .val — phải để JS gán từ server");

        // 2. Dropdown phòng động: không hard-code P101/P102/P103, có id để JS dựng option.
        foreach (var id in new[] { "p-room-filter", "p-res-room-filter", "p-hist-room" })
        {
            Assert.IsTrue(html.Contains($"id=\"{id}\""), $"congan/index.html thiếu dropdown động id='{id}'");
        }
        Assert.IsFalse(html.Contains("<option>P101</option>"),
            "dropdown phòng công an vẫn hard-code P101 — phải dựng từ PHONG_LAY_TAT_CA");
        Assert.IsTrue(mainJs.Contains("syncRoomDropdowns"),
            "congan/js/main.js thiếu syncRoomDropdowns()");

        // 3. Footer Công dân đếm từ render, không dùng số tĩnh.
        Assert.IsTrue(citizensJs.Contains("#ptab-citizens .tblfoot"),
            "citizens.js phải cập nhật footer #ptab-citizens theo dữ liệu hiển thị");

        // 4. Ngày mặc định Biến động tính từ tháng hiện tại, không fix 2026-09.
        Assert.IsFalse(html.Contains("value=\"2026-09-01\"") || html.Contains("value=\"2026-09-30\""),
            "congan/index.html vẫn fix ngày 2026-09 — historyRange() phải tự tính theo tháng hiện tại");
        Assert.IsTrue(historyJs.Contains("firstOfMonth"),
            "history.js phải tự tính ngày đầu tháng hiện tại trong historyRange()");
    }

    /// <summary>Tab Tạm trú phải có nút Xuất danh sách hoạt động, nối sang exportResidence.</summary>
    [TestMethod]
    public void ResidenceTab_ExportButton_ExportsPendingResidence()
    {
        var html = File.ReadAllText(PoliceHtml);
        var js = File.ReadAllText(Path.Combine(Wwwroot, "congan", "js", "residence.js"));

        Assert.IsTrue(js.Contains("function exportResidence"), "residence.js thiếu exportResidence()");
        Assert.IsTrue(html.Contains("onclick=\"exportResidence()\""),
            "congan/index.html chưa nối nút Xuất danh sách sang exportResidence()");
    }
}
