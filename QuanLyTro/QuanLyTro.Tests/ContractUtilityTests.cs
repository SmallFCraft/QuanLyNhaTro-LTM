using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Server.Repositories;
using QuanLyTro.Server.Services;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class ContractServiceTests
{
    private static readonly DateOnly Start = new(2026, 1, 1);
    private static readonly DateOnly End = new(2026, 12, 31);

    private static ContractDto Contract(
        int id = 0,
        DateOnly? start = null,
        DateOnly? end = null,
        ContractStatus status = ContractStatus.Active) =>
        new(id, 101, 5, start ?? Start, end ?? End, 2_500_000m, 1_000_000m, status, null);

    // BR-06
    [DataTestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    public async Task ContractService_RejectsEndDateNotAfterStartDate(int dayOffset)
    {
        var service = new ContractService(new StubContractRepo());

        await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => service.CreateAsync(Contract(end: Start.AddDays(dayOffset))));
    }

    // BR-05
    [TestMethod]
    public async Task ContractService_RejectsRepresentativeNotInRoom()
    {
        var repo = new StubContractRepo { TenantInRoom = false };
        var service = new ContractService(repo);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => service.CreateAsync(Contract()));

        StringAssert.Contains(ex.Message, "đại diện");
    }

    // BR-04
    [TestMethod]
    public async Task ContractService_RejectsSecondActiveContractForRoom()
    {
        var repo = new StubContractRepo { HasActive = true };
        var service = new ContractService(repo);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => service.CreateAsync(Contract()));

        StringAssert.Contains(ex.Message, "hiệu lực");
    }

    [TestMethod]
    public async Task ContractService_CreateForcesActiveStatus()
    {
        var repo = new StubContractRepo();
        var service = new ContractService(repo);

        var created = await service.CreateAsync(Contract(status: ContractStatus.Terminated));

        Assert.AreEqual(7, created.Id);
        Assert.AreEqual(ContractStatus.Active, created.Status);
        Assert.AreEqual(ContractStatus.Active, repo.Added!.Status);
    }

    [TestMethod]
    public async Task ContractService_TerminatePassesNotesAndReportsMissingContract()
    {
        var repo = new StubContractRepo();
        var service = new ContractService(repo);

        Assert.IsTrue(await service.TerminateAsync(11, "Trả phòng"));
        Assert.AreEqual("Trả phòng", repo.TerminateNotes);

        repo.TerminateSucceeds = false;
        await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => service.TerminateAsync(11, null));
    }

    // US-10
    [TestMethod]
    public async Task ContractService_RenewRejectsDateNotAfterCurrentEndDate()
    {
        var repo = new StubContractRepo { Existing = Contract(1) };
        var service = new ContractService(repo);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => service.RenewAsync(1, End));

        StringAssert.Contains(ex.Message, "gia hạn");
    }

    [TestMethod]
    public async Task ContractService_RenewRejectsNonActiveContract()
    {
        var repo = new StubContractRepo { Existing = Contract(1, status: ContractStatus.Terminated) };
        var service = new ContractService(repo);

        await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => service.RenewAsync(1, End.AddYears(1)));

        repo.Existing = null;
        await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => service.RenewAsync(99, End.AddYears(1)));
    }

    [TestMethod]
    public async Task ContractService_RenewExtendsActiveContract()
    {
        var newEnd = End.AddYears(1);
        var repo = new StubContractRepo { Existing = Contract(1) };
        var service = new ContractService(repo);

        Assert.IsTrue(await service.RenewAsync(1, newEnd));
        Assert.AreEqual(newEnd, repo.UpdatedEndDate);
    }

    // US-11
    [TestMethod]
    public async Task ContractService_GetAllCarriesRoomNumberAndRepresentativeName()
    {
        var repo = new StubContractRepo
        {
            Item = new ContractListItem(Contract(1), "P101", "Nguyễn Văn A"),
        };
        var service = new ContractService(repo);

        var all = await service.GetAllAsync();

        Assert.AreEqual(1, all.Count);
        Assert.AreEqual("P101", all[0].RoomNumber);
        Assert.AreEqual("Nguyễn Văn A", all[0].RepresentativeName);
    }
}

[TestClass]
public sealed class UtilityServiceTests
{
    private static UtilityReadingDto Reading(
        string month = "2026-09",
        int oldElec = 100,
        int newElec = 150,
        int oldWater = 20,
        int newWater = 25) =>
        new(0, 101, month, oldElec, newElec, 3500m, oldWater, newWater, 10_000m);

    [TestMethod]
    public async Task UtilityService_ReturnsLatestReadingOrNull()
    {
        var repo = new StubUtilityRepo { Latest = Reading("2026-08") };
        var service = new UtilityService(repo);

        Assert.AreEqual("2026-08", (await service.GetPreviousReadingAsync(101))!.BillingMonth);

        repo.Latest = null;
        Assert.IsNull(await service.GetPreviousReadingAsync(101));
    }

    // BR-07
    [TestMethod]
    public async Task UtilityService_RejectsDecreasingElectricity()
    {
        var service = new UtilityService(new StubUtilityRepo());

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => service.RecordAsync(Reading(newElec: 99)));

        StringAssert.Contains(ex.Message, "điện");
    }

    [TestMethod]
    public async Task UtilityService_RejectsDecreasingWater()
    {
        var service = new UtilityService(new StubUtilityRepo());

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => service.RecordAsync(Reading(newWater: 19)));

        StringAssert.Contains(ex.Message, "nước");
    }

    [DataTestMethod]
    [DataRow("2026-9")]
    [DataRow("26-09")]
    [DataRow("2026-13")]
    [DataRow("")]
    public async Task UtilityService_RejectsMalformedBillingMonth(string month)
    {
        var service = new UtilityService(new StubUtilityRepo());

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => service.RecordAsync(Reading(month)));

        StringAssert.Contains(ex.Message, "yyyy-MM");
    }

    [TestMethod]
    public async Task UtilityService_RejectsNonPositiveRates()
    {
        var service = new UtilityService(new StubUtilityRepo());

        await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => service.RecordAsync(Reading() with { ElectricityRate = 0m }));
        await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => service.RecordAsync(Reading() with { WaterRate = -1m }));
    }

    [TestMethod]
    public async Task UtilityService_RecordsReading()
    {
        var repo = new StubUtilityRepo();
        var service = new UtilityService(repo);

        var saved = await service.RecordAsync(Reading());

        Assert.AreEqual(3, saved.Id);
        Assert.AreEqual("2026-09", repo.Added!.BillingMonth);
    }
}

file sealed class StubContractRepo : IContractRepository
{
    public bool TenantInRoom { get; set; } = true;
    public bool HasActive { get; set; }
    public ContractDto? Existing { get; set; }
    public ContractListItem? Item { get; set; }
    public bool TerminateSucceeds { get; set; } = true;
    public ContractDto? Added { get; private set; }
    public string? TerminateNotes { get; private set; }
    public DateOnly? UpdatedEndDate { get; private set; }

    public Task<ContractDto?> GetByIdAsync(int contractId, CancellationToken ct = default) =>
        Task.FromResult(Existing);

    public Task<bool> HasActiveContractAsync(int roomId, CancellationToken ct = default) =>
        Task.FromResult(HasActive);

    public Task<bool> IsTenantInRoomAsync(int tenantId, int roomId, CancellationToken ct = default) =>
        Task.FromResult(TenantInRoom);

    public Task<ContractDto> AddAsync(ContractDto contract, CancellationToken ct = default)
    {
        Added = contract with { Id = 7 };
        return Task.FromResult(Added);
    }

    public Task<bool> TerminateAsync(int contractId, string? notes, CancellationToken ct = default)
    {
        TerminateNotes = notes;
        return Task.FromResult(TerminateSucceeds);
    }

    public Task<bool> UpdateEndDateAsync(int contractId, DateOnly newEndDate, CancellationToken ct = default)
    {
        UpdatedEndDate = newEndDate;
        return Task.FromResult(true);
    }

    public Task<List<ContractListItem>> GetAllAsync(CancellationToken ct = default) =>
        Task.FromResult(Item is null ? [] : new List<ContractListItem> { Item });
}

file sealed class StubUtilityRepo : IUtilityRepository
{
    public UtilityReadingDto? Latest { get; set; }
    public UtilityReadingDto? Added { get; private set; }

    public Task<UtilityReadingDto?> GetLatestAsync(int roomId, CancellationToken ct = default) =>
        Task.FromResult(Latest);

    public Task<UtilityReadingDto> AddAsync(UtilityReadingDto reading, CancellationToken ct = default)
    {
        Added = reading with { Id = 3 };
        return Task.FromResult(Added);
    }
}
