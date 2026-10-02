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

    private static ContractDto Contract(decimal rentalPrice = 2_500_000m) =>
        new(9, 101, 5, Start, End, rentalPrice, 1_000_000m, ContractStatus.Active, null);

    private static UtilityReadingDto Reading(
        string month = "2026-09",
        int oldElec = 100,
        int newElec = 150,
        int oldWater = 20,
        int newWater = 25) =>
        new(4, 101, month, oldElec, newElec, 3500m, oldWater, newWater, 10_000m);

    private static InvoiceDto Invoice(int id = 3, InvoiceStatus status = InvoiceStatus.Unpaid) =>
        new(id, 101, 9, "2026-09", 2_500_000m, 175_000m, 50_000m, 0m, 2_725_000m, status,
            status == InvoiceStatus.Paid ? new DateTime(2026, 9, 5) : null);

    // BR-09
    [TestMethod]
    public async Task InvoiceService_RejectsRoomWithoutActiveContract()
    {
        var repo = new StubInvoiceRepo { Contract = null };
        var service = new InvoiceService(repo);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => service.CreateAsync(new CreateInvoiceRequest(101, "2026-09", 0m)));

        StringAssert.Contains(ex.Message, "hợp đồng");
    }

    // BR-09
    [TestMethod]
    public async Task InvoiceService_RejectsMonthWithoutUtilityReading()
    {
        var repo = new StubInvoiceRepo { Reading = null };
        var service = new InvoiceService(repo);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => service.CreateAsync(new CreateInvoiceRequest(101, "2026-09", 0m)));

        StringAssert.Contains(ex.Message, "điện nước");
    }

    // BR-10
    [TestMethod]
    public async Task InvoiceService_ComputesTotalOnServer()
    {
        var repo = new StubInvoiceRepo();
        var service = new InvoiceService(repo);

        var created = await service.CreateAsync(new CreateInvoiceRequest(101, "2026-09", 120_000m));

        var saved = repo.Added!;
        Assert.AreEqual(2_500_000m, saved.RoomAmount);
        Assert.AreEqual(175_000m, saved.ElectricityAmount);   // (150-100) * 3500
        Assert.AreEqual(50_000m, saved.WaterAmount);          // (25-20) * 10000
        Assert.AreEqual(120_000m, saved.OtherFees);
        Assert.AreEqual(2_845_000m, saved.TotalAmount);
        Assert.AreEqual(9, saved.ContractId);
        Assert.AreEqual(InvoiceStatus.Unpaid, saved.Status);
        Assert.IsNull(saved.PaidAt);
        Assert.AreEqual(saved.TotalAmount, created.TotalAmount);
    }

    // BR-10
    [TestMethod]
    public async Task InvoiceService_RejectsNegativeOtherFees()
    {
        var service = new InvoiceService(new StubInvoiceRepo());

        await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => service.CreateAsync(new CreateInvoiceRequest(101, "2026-09", -1m)));
    }

    [DataTestMethod]
    [DataRow("2026-9")]
    [DataRow("26-09")]
    [DataRow("2026-13")]
    [DataRow("")]
    public async Task InvoiceService_RejectsMalformedBillingMonth(string month)
    {
        var service = new InvoiceService(new StubInvoiceRepo());

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => service.CreateAsync(new CreateInvoiceRequest(101, month, 0m)));

        StringAssert.Contains(ex.Message, "yyyy-MM");
    }

    // BR-11
    [TestMethod]
    public async Task InvoiceService_PayRejectsAlreadyPaidInvoice()
    {
        var repo = new StubInvoiceRepo { Existing = Invoice(status: InvoiceStatus.Paid) };
        var service = new InvoiceService(repo);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => service.PayAsync(3));

        StringAssert.Contains(ex.Message, "đã thanh toán");
        Assert.IsNull(repo.MarkedPaidId);
    }

    [TestMethod]
    public async Task InvoiceService_PayRejectsMissingInvoice()
    {
        var service = new InvoiceService(new StubInvoiceRepo { Existing = null });

        await Assert.ThrowsExceptionAsync<BusinessRuleException>(() => service.PayAsync(99));
    }

    // BR-11
    [TestMethod]
    public async Task InvoiceService_PayMarksUnpaidInvoicePaid()
    {
        var repo = new StubInvoiceRepo { Existing = Invoice() };
        var service = new InvoiceService(repo);

        Assert.IsTrue(await service.PayAsync(3));
        Assert.AreEqual(3, repo.MarkedPaidId);
    }

    // BR-11: hai client thu cùng lúc — chỉ một UPDATE chạm được dòng Unpaid.
    [TestMethod]
    public async Task InvoiceService_PayReportsRaceWhenUpdateHitsNoRow()
    {
        var repo = new StubInvoiceRepo { Existing = Invoice(), MarkPaidSucceeds = false };
        var service = new InvoiceService(repo);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(() => service.PayAsync(3));

        StringAssert.Contains(ex.Message, "đã thanh toán");
    }

    [TestMethod]
    public async Task InvoiceService_GetAllPassesMonthAndRoomFilter()
    {
        var repo = new StubInvoiceRepo { Items = [Invoice()] };
        var service = new InvoiceService(repo);

        var all = await service.GetAllAsync(" 2026-09 ", 101);

        Assert.AreEqual(1, all.Count);
        Assert.AreEqual("2026-09", repo.LastMonth);
        Assert.AreEqual(101, repo.LastRoomId);
    }

    // BR-14
    [TestMethod]
    public async Task InvoiceService_GetMineScopesToTenant()
    {
        var repo = new StubInvoiceRepo { Items = [Invoice()] };
        var service = new InvoiceService(repo);

        var mine = await service.GetMineAsync(5, page: 2, pageSize: 3);

        Assert.AreEqual(1, mine.Items.Count);
        Assert.AreEqual(1, mine.TotalCount);
        Assert.AreEqual(2, mine.Page);
        Assert.AreEqual(3, mine.PageSize);
        Assert.AreEqual(5, repo.LastTenantId);
        Assert.AreEqual(2, repo.LastPage);
        Assert.AreEqual(3, repo.LastPageSize);
    }
}

[TestClass]
public sealed class ReportServiceTests
{
    [TestMethod]
    public async Task ReportService_ReturnsSummaryForMonth()
    {
        var repo = new StubReportRepo();
        var service = new ReportService(repo);

        var summary = await service.GetSummaryAsync(" 2026-09 ");

        Assert.AreEqual(8, summary.TotalRooms);
        Assert.AreEqual(900_000m, summary.PaidAmount);
        Assert.AreEqual("2026-09", repo.LastMonth);
    }

    [DataTestMethod]
    [DataRow("2026-9")]
    [DataRow("")]
    public async Task ReportService_RejectsMalformedBillingMonth(string month)
    {
        var service = new ReportService(new StubReportRepo());

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => service.GetSummaryAsync(month));

        StringAssert.Contains(ex.Message, "yyyy-MM");
    }

    // US-08: Client tự dựng CSV từ danh sách này.
    [TestMethod]
    public async Task ReportService_ExportsResidents()
    {
        var repo = new StubReportRepo();
        var service = new ReportService(repo);

        var rows = await service.ExportResidenceAsync();

        Assert.AreEqual(1, rows.Count);
        Assert.AreEqual("P101", rows[0].RoomNumber);
        Assert.AreEqual("012345678901", rows[0].IdCard);
    }
}

file sealed class StubInvoiceRepo : IInvoiceRepository
{
    public ContractDto? Contract { get; set; } = new(
        9, 101, 5, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31),
        2_500_000m, 1_000_000m, ContractStatus.Active, null);

    public UtilityReadingDto? Reading { get; set; } = new(
        4, 101, "2026-09", 100, 150, 3500m, 20, 25, 10_000m);

    public InvoiceDto? Existing { get; set; }
    public bool MarkPaidSucceeds { get; set; } = true;
    public List<InvoiceDto> Items { get; set; } = [];
    public InvoiceDto? Added { get; private set; }
    public int? MarkedPaidId { get; private set; }
    public string? LastMonth { get; private set; }
    public int? LastRoomId { get; private set; }
    public int? LastTenantId { get; private set; }
    public int? LastPage { get; private set; }
    public int? LastPageSize { get; private set; }

    public Task<ContractDto?> GetActiveContractAsync(int roomId, CancellationToken ct = default) =>
        Task.FromResult(Contract);

    public Task<UtilityReadingDto?> GetUtilityReadingAsync(string billingMonth, int roomId, CancellationToken ct = default) =>
        Task.FromResult(Reading is not null && Reading.BillingMonth == billingMonth ? Reading : null);

    public Task<InvoiceDto> AddAsync(InvoiceDto invoice, CancellationToken ct = default)
    {
        Added = invoice with { Id = 3 };
        return Task.FromResult(Added);
    }

    public Task<InvoiceDto?> GetByIdAsync(int invoiceId, CancellationToken ct = default) =>
        Task.FromResult(Existing);

    public Task<bool> MarkPaidAsync(int invoiceId, CancellationToken ct = default)
    {
        MarkedPaidId = invoiceId;
        return Task.FromResult(MarkPaidSucceeds);
    }

    public Task<List<InvoiceDto>> GetAllAsync(string billingMonth, int? roomId, CancellationToken ct = default)
    {
        LastMonth = billingMonth;
        LastRoomId = roomId;
        return Task.FromResult(Items);
    }

    public Task<InvoiceMinePageDto> GetByTenantAsync(
        int tenantId, int page, int pageSize, CancellationToken ct = default)
    {
        LastTenantId = tenantId;
        LastPage = page;
        LastPageSize = pageSize;
        return Task.FromResult(new InvoiceMinePageDto(Items, Items.Count, page, pageSize));
    }
}

file sealed class StubReportRepo : IReportRepository
{
    public string? LastMonth { get; private set; }

    public Task<SummaryReportDto> GetSummaryAsync(string billingMonth, CancellationToken ct = default)
    {
        LastMonth = billingMonth;
        return Task.FromResult(new SummaryReportDto(8, 3, 5, 12, 900_000m, 400_000m));
    }

    public Task<List<ResidenceExportDto>> GetResidentsAsync(CancellationToken ct = default) =>
        Task.FromResult(new List<ResidenceExportDto>
        {
            new("Nguyễn Văn A", new DateOnly(2000, 1, 1), "012345678901", "Đà Nẵng", "P101"),
        });
}
