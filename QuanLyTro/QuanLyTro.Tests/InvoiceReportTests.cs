using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Server.Repositories;
using QuanLyTro.Server.Services;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class InvoiceServiceTests
{
    private static readonly DateOnly Start = new(2026, 1, 1);
    private static readonly DateOnly End = new(2026, 12, 31);

    private static HopDongDto Contract(decimal giaThue = 2_500_000m) =>
        new(9, 101, 5, Start, End, giaThue, 1_000_000m, TrangThaiHopDong.HieuLuc, null);

    private static ChiSoDienNuocDto BanGhi(
        string month = "2026-09",
        int oldElec = 100,
        int newElec = 150,
        int nuocCu = 20,
        int nuocMoi = 25) =>
        new(4, 101, month, oldElec, newElec, 3500m, nuocCu, nuocMoi, 10_000m);

    private static HoaDonDto Invoice(int id = 3, TrangThaiHoaDon trang_thai = TrangThaiHoaDon.ChuaThu) =>
        new(id, 101, 9, "2026-09", 2_500_000m, 175_000m, 50_000m, 0m, 2_725_000m, trang_thai,
            trang_thai == TrangThaiHoaDon.DaThu ? new DateTime(2026, 9, 5) : null);

    // BR-09
    [TestMethod]
    public async Task InvoiceService_RejectsRoomWithoutActiveContract()
    {
        var repo = new StubInvoiceRepo { Contract = null };
        var service = new HoaDonService(repo);

        var ex = await Assert.ThrowsExceptionAsync<LoiNghiepVu>(
            () => service.CreateAsync(new YeuCauTaoHoaDon(101, "2026-09", 0m)));

        StringAssert.Contains(ex.Message, "hợp đồng");
    }

    // BR-09
    [TestMethod]
    public async Task InvoiceService_RejectsMonthWithoutUtilityReading()
    {
        var repo = new StubInvoiceRepo { BanGhi = null };
        var service = new HoaDonService(repo);

        var ex = await Assert.ThrowsExceptionAsync<LoiNghiepVu>(
            () => service.CreateAsync(new YeuCauTaoHoaDon(101, "2026-09", 0m)));

        StringAssert.Contains(ex.Message, "điện nước");
    }

    // BR-10
    [TestMethod]
    public async Task InvoiceService_ComputesTotalOnServer()
    {
        var repo = new StubInvoiceRepo();
        var service = new HoaDonService(repo);

        var created = await service.CreateAsync(new YeuCauTaoHoaDon(101, "2026-09", 120_000m));

        var saved = repo.Added!;
        Assert.AreEqual(2_500_000m, saved.TienPhong);
        Assert.AreEqual(175_000m, saved.TienDien);   // (150-100) * 3500
        Assert.AreEqual(50_000m, saved.TienNuoc);          // (25-20) * 10000
        Assert.AreEqual(120_000m, saved.PhiKhac);
        Assert.AreEqual(2_845_000m, saved.TongTien);
        Assert.AreEqual(9, saved.HopDongId);
        Assert.AreEqual(TrangThaiHoaDon.ChuaThu, saved.TrangThai);
        Assert.IsNull(saved.NgayDong);
        Assert.AreEqual(saved.TongTien, created.TongTien);
    }

    // BR-10
    [TestMethod]
    public async Task InvoiceService_RejectsNegativeOtherFees()
    {
        var service = new HoaDonService(new StubInvoiceRepo());

        await Assert.ThrowsExceptionAsync<LoiNghiepVu>(
            () => service.CreateAsync(new YeuCauTaoHoaDon(101, "2026-09", -1m)));
    }

    [DataTestMethod]
    [DataRow("2026-9")]
    [DataRow("26-09")]
    [DataRow("2026-13")]
    [DataRow("")]
    public async Task InvoiceService_RejectsMalformedBillingMonth(string month)
    {
        var service = new HoaDonService(new StubInvoiceRepo());

        var ex = await Assert.ThrowsExceptionAsync<LoiNghiepVu>(
            () => service.CreateAsync(new YeuCauTaoHoaDon(101, month, 0m)));

        StringAssert.Contains(ex.Message, "yyyy-MM");
    }

    // BR-11
    [TestMethod]
    public async Task InvoiceService_PayRejectsAlreadyPaidInvoice()
    {
        var repo = new StubInvoiceRepo { Existing = Invoice(trang_thai: TrangThaiHoaDon.DaThu) };
        var service = new HoaDonService(repo);

        var ex = await Assert.ThrowsExceptionAsync<LoiNghiepVu>(
            () => service.PayAsync(3));

        StringAssert.Contains(ex.Message, "đã thanh toán");
        Assert.IsNull(repo.MarkedPaidId);
    }

    [TestMethod]
    public async Task InvoiceService_PayRejectsMissingInvoice()
    {
        var service = new HoaDonService(new StubInvoiceRepo { Existing = null });

        await Assert.ThrowsExceptionAsync<LoiNghiepVu>(() => service.PayAsync(99));
    }

    // BR-11
    [TestMethod]
    public async Task InvoiceService_PayMarksUnpaidInvoicePaid()
    {
        var repo = new StubInvoiceRepo { Existing = Invoice() };
        var service = new HoaDonService(repo);

        Assert.IsTrue(await service.PayAsync(3));
        Assert.AreEqual(3, repo.MarkedPaidId);
    }

    // BR-11: hai client thu cùng lúc — chỉ một UPDATE chạm được dòng ChuaThu.
    [TestMethod]
    public async Task InvoiceService_PayReportsRaceWhenUpdateHitsNoRow()
    {
        var repo = new StubInvoiceRepo { Existing = Invoice(), MarkPaidSucceeds = false };
        var service = new HoaDonService(repo);

        var ex = await Assert.ThrowsExceptionAsync<LoiNghiepVu>(() => service.PayAsync(3));

        StringAssert.Contains(ex.Message, "đã thanh toán");
    }

    [TestMethod]
    public async Task InvoiceService_GetAllPassesMonthAndRoomFilter()
    {
        var repo = new StubInvoiceRepo { DanhSach = [Invoice()] };
        var service = new HoaDonService(repo);

        var all = await service.GetAllAsync(" 2026-09 ", 101);

        Assert.AreEqual(1, all.Count);
        Assert.AreEqual("2026-09", repo.LastMonth);
        Assert.AreEqual(101, repo.LastRoomId);
    }

    // BR-14
    [TestMethod]
    public async Task InvoiceService_GetMineScopesToTenant()
    {
        var repo = new StubInvoiceRepo { DanhSach = [Invoice()] };
        var service = new HoaDonService(repo);

        var mine = await service.GetMineAsync(5, page: 2, soLuongMoiTrang: 3);

        Assert.AreEqual(1, mine.DanhSach.Count);
        Assert.AreEqual(1, mine.TongSo);
        Assert.AreEqual(2, mine.Trang);
        Assert.AreEqual(3, mine.SoLuongMoiTrang);
        Assert.AreEqual(5, repo.LastTenantId);
        Assert.AreEqual(2, repo.LastPage);
        Assert.AreEqual(3, repo.LastPageSize);
    }

    [TestMethod]
    public async Task InvoiceService_ComputesWaterByPerson_WhenHinhThucNuocIsNguoi()
    {
        var readingByPerson = new ChiSoDienNuocDto(
            4, 101, "2026-09",
            DienCu: 100, DienMoi: 150, GiaDien: 3500m,
            NuocCu: 0, NuocMoi: 0, GiaNuoc: 60_000m,
            HinhThucNuoc: "Nguoi", SoNguoiNuoc: 3);

        var repo = new StubInvoiceRepo { BanGhi = readingByPerson };
        var service = new HoaDonService(repo);

        var created = await service.CreateAsync(new YeuCauTaoHoaDon(101, "2026-09", 0m));

        // 3 người * 60.000 = 180.000 đ
        Assert.AreEqual(180_000m, created.TienNuoc);
        // Điện: (150 - 100) * 3500 = 175.000 đ (Chữ điện = 50 kWh)
        Assert.AreEqual(175_000m, created.TienDien);
        Assert.AreEqual(2_500_000m + 175_000m + 180_000m, created.TongTien);
    }

    [TestMethod]
    public void UtilityService_Validate_RejectsWaterByPerson_WhenSoNguoiZeroOrNegative()
    {
        var reading = new ChiSoDienNuocDto(
            1, 101, "2026-09", 100, 150, 3500m, 0, 0, 60_000m,
            HinhThucNuoc: "Nguoi", SoNguoiNuoc: 0);

        var ex = Assert.ThrowsException<LoiNghiepVu>(() => DienNuocService.Validate(reading));
        StringAssert.Contains(ex.Message, "Số người");
    }

    [TestMethod]
    public void UtilityService_Validate_AcceptsWaterByPerson_WhenSoNguoiGreaterThanZero()
    {
        var reading = new ChiSoDienNuocDto(
            1, 101, "2026-09", 100, 150, 3500m, 10, 10, 60_000m,
            HinhThucNuoc: "Nguoi", SoNguoiNuoc: 2);

        DienNuocService.Validate(reading);
    }
}

[TestClass]
public sealed class ReportServiceTests
{
    [TestMethod]
    public async Task ReportService_ReturnsSummaryForMonth()
    {
        var repo = new StubReportRepo();
        var service = new BaoCaoService(repo);

        var summary = await service.GetSummaryAsync(" 2026-09 ");

        Assert.AreEqual(8, summary.TongSoPhong);
        Assert.AreEqual(900_000m, summary.SoTienDaThu);
        Assert.AreEqual("2026-09", repo.LastMonth);
    }

    [DataTestMethod]
    [DataRow("2026-9")]
    [DataRow("")]
    public async Task ReportService_RejectsMalformedBillingMonth(string month)
    {
        var service = new BaoCaoService(new StubReportRepo());

        var ex = await Assert.ThrowsExceptionAsync<LoiNghiepVu>(
            () => service.GetSummaryAsync(month));

        StringAssert.Contains(ex.Message, "yyyy-MM");
    }

    // US-08: Client tự dựng CSV từ danh sách này.
    [TestMethod]
    public async Task ReportService_ExportsResidents()
    {
        var repo = new StubReportRepo();
        var service = new BaoCaoService(repo);

        var rows = await service.ExportResidenceAsync();

        Assert.AreEqual(1, rows.Count);
        Assert.AreEqual("P101", rows[0].SoPhong);
        Assert.AreEqual("012345678901", rows[0].Cccd);
    }
}

file sealed class StubInvoiceRepo : IHoaDonRepository
{
    public HopDongDto? Contract { get; set; } = new(
        9, 101, 5, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31),
        2_500_000m, 1_000_000m, TrangThaiHopDong.HieuLuc, null);

    public ChiSoDienNuocDto? BanGhi { get; set; } = new(
        4, 101, "2026-09", 100, 150, 3500m, 20, 25, 10_000m);

    public HoaDonDto? Existing { get; set; }
    public bool MarkPaidSucceeds { get; set; } = true;
    public List<HoaDonDto> DanhSach { get; set; } = [];
    public HoaDonDto? Added { get; private set; }
    public int? MarkedPaidId { get; private set; }
    public string? LastMonth { get; private set; }
    public int? LastRoomId { get; private set; }
    public int? LastTenantId { get; private set; }
    public int? LastPage { get; private set; }
    public int? LastPageSize { get; private set; }

    public Task<HopDongDto?> GetActiveContractAsync(int phongId, CancellationToken ct = default) =>
        Task.FromResult(Contract);

    public Task<ChiSoDienNuocDto?> GetUtilityReadingAsync(string kyCuoc, int phongId, CancellationToken ct = default) =>
        Task.FromResult(BanGhi is not null && BanGhi.KyCuoc == kyCuoc ? BanGhi : null);

    public Task<HoaDonDto> AddAsync(HoaDonDto invoice, CancellationToken ct = default)
    {
        Added = invoice with { Id = 3 };
        return Task.FromResult(Added);
    }

    public Task<HoaDonDto?> GetByIdAsync(int hoaDonId, CancellationToken ct = default) =>
        Task.FromResult(Existing);

    public Task<bool> MarkPaidAsync(int hoaDonId, CancellationToken ct = default)
    {
        MarkedPaidId = hoaDonId;
        return Task.FromResult(MarkPaidSucceeds);
    }

    public Task<List<HoaDonDto>> GetAllAsync(string kyCuoc, int? phongId, CancellationToken ct = default)
    {
        LastMonth = kyCuoc;
        LastRoomId = phongId;
        return Task.FromResult(DanhSach);
    }

    public Task<TrangHoaDonCuaToiDto> GetByTenantAsync(
        int khachThueId, int page, int soLuongMoiTrang, CancellationToken ct = default)
    {
        LastTenantId = khachThueId;
        LastPage = page;
        LastPageSize = soLuongMoiTrang;
        return Task.FromResult(new TrangHoaDonCuaToiDto(DanhSach, DanhSach.Count, page, soLuongMoiTrang));
    }
}

file sealed class StubReportRepo : IBaoCaoRepository
{
    public string? LastMonth { get; private set; }

    public Task<BaoCaoTongQuanDto> GetSummaryAsync(string kyCuoc, CancellationToken ct = default)
    {
        LastMonth = kyCuoc;
        return Task.FromResult(new BaoCaoTongQuanDto(8, 3, 5, 12, 900_000m, 400_000m));
    }

    public Task<List<XuatHoSoTamTruDto>> GetResidentsAsync(CancellationToken ct = default) =>
        Task.FromResult(new List<XuatHoSoTamTruDto>
        {
            new("Nguyễn Văn A", new DateOnly(2000, 1, 1), "012345678901", "Đà Nẵng", "P101"),
        });
}
