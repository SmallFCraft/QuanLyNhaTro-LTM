using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Server.Repositories;
using QuanLyTro.Server.Services;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class ResidenceServiceTests
{
    [TestMethod]
    [ExpectedException(typeof(LoiNghiepVu))]
    public async Task ExportHistory_EndDateBeforeStartDate_ThrowsException()
    {
        var service = new CuTruService(repo: null!);
        var req = new YeuCauXuatLichSu(
            TuNgay: new DateTime(2026, 10, 1),
            DenNgay: new DateTime(2026, 9, 1),
            DinhDang: "CSV",
            SoPhong: null,
            LoaiBienDong: null);

        await service.ExportHistoryAsync(req, CancellationToken.None);
    }

    [TestMethod]
    [ExpectedException(typeof(LoiNghiepVu))]
    public async Task GetHistory_EndDateBeforeStartDate_ThrowsException()
    {
        var service = new CuTruService(repo: null!);
        await service.GetHistoryAsync(
            from: new DateTime(2026, 10, 1),
            to: new DateTime(2026, 9, 1),
            room: null,
            CancellationToken.None);
    }

    /// <summary>
    /// I5: không truyền ngày → service phải chốt khoảng mặc định 90 ngày (không quét cả bảng),
    /// và cả hai đầu khoảng đều nằm quanh "hôm nay", không phải DateTime.MinValue.
    /// </summary>
    [TestMethod]
    public async Task GetHistory_NoDates_UsesDefaultWindow_NotEntireTable()
    {
        var repo = new CapturingResidenceRepository();
        var service = new CuTruService(repo);
        var today = DateTime.Now.Date;

        var result = await service.GetHistoryAsync(from: null, to: null, room: null, CancellationToken.None);

        Assert.AreEqual(0, result.Count);
        Assert.IsNotNull(repo.LastFrom);
        Assert.IsNotNull(repo.LastTo);
        Assert.AreNotEqual(DateTime.MinValue, repo.LastFrom!.Value, "from mặc định không được là MinValue.");
        Assert.AreEqual(CuTruService.DefaultWindowDays, (repo.LastFrom!.Value.Date - repo.LastTo!.Value.Date).TotalDays * -1,
            $"Khoảng mặc định phải là {CuTruService.DefaultWindowDays} ngày.");
        Assert.AreEqual(today, repo.LastTo!.Value.Date, "Mốc kết thúc mặc định phải là hôm nay.");
    }

    /// <summary>I5: khoảng ngày quá rộng bị từ chối thay vì kéo cả bảng qua mạng.</summary>
    [TestMethod]
    [ExpectedException(typeof(LoiNghiepVu))]
    public async Task GetHistory_WindowTooWide_ThrowsException()
    {
        var service = new CuTruService(repo: null!);
        await service.GetHistoryAsync(
            from: new DateTime(2000, 1, 1),
            to: new DateTime(2026, 1, 1),
            room: null,
            CancellationToken.None);
    }

    /// <summary>I5 (spec §2.3): tab "Biến động" xem trước tối đa 10 dòng trước khi xuất.</summary>
    [TestMethod]
    public async Task GetHistory_PreviewCappedAtTenRows()
    {
        var repo = new CapturingResidenceRepository();
        var service = new CuTruService(repo);

        await service.GetHistoryAsync(
            from: new DateTime(2026, 9, 1),
            to: new DateTime(2026, 9, 30),
            room: null,
            CancellationToken.None);

        Assert.AreEqual(CuTruService.PreviewRowLimit, repo.LastLimit,
            "Đường xem trước phải chặn số dòng theo PreviewRowLimit.");
        Assert.AreEqual(10, CuTruService.PreviewRowLimit);
    }

    /// <summary>DenNgay cuối ngày: ngày 30 phải bao trùm cả 23:59 cùng ngày, không cắt mất bản ghi.</summary>
    [TestMethod]
    public async Task GetHistory_EndDate_IncludesWholeDay()
    {
        var repo = new CapturingResidenceRepository();
        var service = new CuTruService(repo);

        await service.GetHistoryAsync(
            from: new DateTime(2026, 9, 1),
            to: new DateTime(2026, 9, 30),
            room: null,
            CancellationToken.None);

        Assert.AreEqual(new DateTime(2026, 9, 30).Date, repo.LastTo!.Value.Date);
        Assert.AreEqual(23, repo.LastTo.Value.Hour);
        Assert.AreEqual(59, repo.LastTo.Value.Minute);
    }

    private sealed class CapturingResidenceRepository : CuTruRepository
    {
        public CapturingResidenceRepository() : base(database: null!) { }

        public DateTime? LastFrom { get; private set; }
        public DateTime? LastTo { get; private set; }
        public int? LastLimit { get; private set; }

        public override Task<List<LichSuCuTruDto>> GetHistoryAsync(
            DateTime from, DateTime to, string? soPhong, int? limit = null, CancellationToken ct = default)
        {
            LastFrom = from;
            LastTo = to;
            LastLimit = limit;
            return Task.FromResult(new List<LichSuCuTruDto>());
        }
    }
}
