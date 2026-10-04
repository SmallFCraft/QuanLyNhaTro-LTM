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

    private static HopDongDto Contract(
        int id = 0,
        DateOnly? start = null,
        DateOnly? end = null,
        TrangThaiHopDong trang_thai = TrangThaiHopDong.HieuLuc) =>
        new(id, 101, 5, start ?? Start, end ?? End, 2_500_000m, 1_000_000m, trang_thai, null);

    // BR-06
    [DataTestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    public async Task ContractService_RejectsEndDateNotAfterStartDate(int dayOffset)
    {
        var service = new HopDongService(new StubContractRepo());

        await Assert.ThrowsExceptionAsync<LoiNghiepVu>(
            () => service.CreateAsync(Contract(end: Start.AddDays(dayOffset))));
    }

    // BR-05
    [TestMethod]
    public async Task ContractService_RejectsRepresentativeNotInRoom()
    {
        var repo = new StubContractRepo { TenantInRoom = false };
        var service = new HopDongService(repo);

        var ex = await Assert.ThrowsExceptionAsync<LoiNghiepVu>(
            () => service.CreateAsync(Contract()));

        StringAssert.Contains(ex.Message, "đại diện");
    }

    // BR-04
    [TestMethod]
    public async Task ContractService_RejectsSecondActiveContractForRoom()
    {
        var repo = new StubContractRepo { HasActive = true };
        var service = new HopDongService(repo);

        var ex = await Assert.ThrowsExceptionAsync<LoiNghiepVu>(
            () => service.CreateAsync(Contract()));

        StringAssert.Contains(ex.Message, "hiệu lực");
    }

    [TestMethod]
    public async Task ContractService_CreateForcesActiveStatus()
    {
        var repo = new StubContractRepo();
        var service = new HopDongService(repo);

        var created = await service.CreateAsync(Contract(trang_thai: TrangThaiHopDong.ChamDut));

        Assert.AreEqual(7, created.Id);
        Assert.AreEqual(TrangThaiHopDong.HieuLuc, created.TrangThai);
        Assert.AreEqual(TrangThaiHopDong.HieuLuc, repo.Added!.TrangThai);
    }

    [TestMethod]
    public async Task ContractService_TerminatePassesNotesAndReportsMissingContract()
    {
        var repo = new StubContractRepo();
        var service = new HopDongService(repo);

        Assert.IsTrue(await service.TerminateAsync(11, "Trả phòng"));
        Assert.AreEqual("Trả phòng", repo.TerminateNotes);

        repo.TerminateSucceeds = false;
        await Assert.ThrowsExceptionAsync<LoiNghiepVu>(
            () => service.TerminateAsync(11, null));
    }

    // US-10
    [TestMethod]
    public async Task ContractService_RenewRejectsDateNotAfterCurrentEndDate()
    {
        var repo = new StubContractRepo { Existing = Contract(1) };
        var service = new HopDongService(repo);

        var ex = await Assert.ThrowsExceptionAsync<LoiNghiepVu>(
            () => service.RenewAsync(1, End));

        StringAssert.Contains(ex.Message, "gia hạn");
    }

    [TestMethod]
    public async Task ContractService_RenewRejectsNonActiveContract()
    {
        var repo = new StubContractRepo { Existing = Contract(1, trang_thai: TrangThaiHopDong.ChamDut) };
        var service = new HopDongService(repo);

        await Assert.ThrowsExceptionAsync<LoiNghiepVu>(
            () => service.RenewAsync(1, End.AddYears(1)));

        repo.Existing = null;
        await Assert.ThrowsExceptionAsync<LoiNghiepVu>(
            () => service.RenewAsync(99, End.AddYears(1)));
    }

    [TestMethod]
    public async Task ContractService_RenewExtendsActiveContract()
    {
        var newEnd = End.AddYears(1);
        var repo = new StubContractRepo { Existing = Contract(1) };
        var service = new HopDongService(repo);

        Assert.IsTrue(await service.RenewAsync(1, newEnd));
        Assert.AreEqual(newEnd, repo.UpdatedEndDate);
    }

    // US-11
    [TestMethod]
    public async Task ContractService_GetAllCarriesRoomNumberAndRepresentativeName()
    {
        var repo = new StubContractRepo
        {
            Item = new MucHopDongItem(Contract(1), "P101", "Nguyễn Văn A"),
        };
        var service = new HopDongService(repo);

        var all = await service.GetAllAsync();

        Assert.AreEqual(1, all.Count);
        Assert.AreEqual("P101", all[0].SoPhong);
        Assert.AreEqual("Nguyễn Văn A", all[0].RepresentativeName);
    }
}

[TestClass]
public sealed class UtilityServiceTests
{
    private static ChiSoDienNuocDto BanGhi(
        string month = "2026-09",
        int oldElec = 100,
        int newElec = 150,
        int nuocCu = 20,
        int nuocMoi = 25) =>
        new(0, 101, month, oldElec, newElec, 3500m, nuocCu, nuocMoi, 10_000m);

    [TestMethod]
    public async Task UtilityService_ReturnsLatestReadingOrNull()
    {
        var repo = new StubUtilityRepo { Latest = BanGhi("2026-08") };
        var service = new DienNuocService(repo);

        Assert.AreEqual("2026-08", (await service.GetPreviousReadingAsync(101))!.KyCuoc);

        repo.Latest = null;
        Assert.IsNull(await service.GetPreviousReadingAsync(101));
    }

    // BR-07
    [TestMethod]
    public async Task UtilityService_RejectsDecreasingElectricity()
    {
        var service = new DienNuocService(new StubUtilityRepo());

        var ex = await Assert.ThrowsExceptionAsync<LoiNghiepVu>(
            () => service.RecordAsync(BanGhi(newElec: 99)));

        StringAssert.Contains(ex.Message, "điện");
    }

    [TestMethod]
    public async Task UtilityService_RejectsDecreasingWater()
    {
        var service = new DienNuocService(new StubUtilityRepo());

        var ex = await Assert.ThrowsExceptionAsync<LoiNghiepVu>(
            () => service.RecordAsync(BanGhi(nuocMoi: 19)));

        StringAssert.Contains(ex.Message, "nước");
    }

    [DataTestMethod]
    [DataRow("2026-9")]
    [DataRow("26-09")]
    [DataRow("2026-13")]
    [DataRow("")]
    public async Task UtilityService_RejectsMalformedBillingMonth(string month)
    {
        var service = new DienNuocService(new StubUtilityRepo());

        var ex = await Assert.ThrowsExceptionAsync<LoiNghiepVu>(
            () => service.RecordAsync(BanGhi(month)));

        StringAssert.Contains(ex.Message, "yyyy-MM");
    }

    [TestMethod]
    public async Task UtilityService_RejectsNonPositiveRates()
    {
        var service = new DienNuocService(new StubUtilityRepo());

        await Assert.ThrowsExceptionAsync<LoiNghiepVu>(
            () => service.RecordAsync(BanGhi() with { GiaDien = 0m }));
        await Assert.ThrowsExceptionAsync<LoiNghiepVu>(
            () => service.RecordAsync(BanGhi() with { GiaNuoc = -1m }));
    }

    [TestMethod]
    public async Task UtilityService_RecordsReading()
    {
        var repo = new StubUtilityRepo();
        var service = new DienNuocService(repo);

        var saved = await service.RecordAsync(BanGhi());

        Assert.AreEqual(3, saved.Id);
        Assert.AreEqual("2026-09", repo.Added!.KyCuoc);
    }
}

file sealed class StubContractRepo : IHopDongRepository
{
    public bool TenantInRoom { get; set; } = true;
    public bool HasActive { get; set; }
    public HopDongDto? Existing { get; set; }
    public MucHopDongItem? Item { get; set; }
    public bool TerminateSucceeds { get; set; } = true;
    public HopDongDto? Added { get; private set; }
    public string? TerminateNotes { get; private set; }
    public DateOnly? UpdatedEndDate { get; private set; }

    public Task<HopDongDto?> GetByIdAsync(int hopDongId, CancellationToken ct = default) =>
        Task.FromResult(Existing);

    public Task<bool> HasActiveContractAsync(int phongId, CancellationToken ct = default) =>
        Task.FromResult(HasActive);

    public Task<bool> IsTenantInRoomAsync(int khachThueId, int phongId, CancellationToken ct = default) =>
        Task.FromResult(TenantInRoom);

    public Task<HopDongDto> AddAsync(HopDongDto contract, CancellationToken ct = default)
    {
        Added = contract with { Id = 7 };
        return Task.FromResult(Added);
    }

    public Task<bool> TerminateAsync(int hopDongId, string? ghi_chu, CancellationToken ct = default)
    {
        TerminateNotes = ghi_chu;
        return Task.FromResult(TerminateSucceeds);
    }

    public Task<bool> UpdateEndDateAsync(int hopDongId, DateOnly newEndDate, CancellationToken ct = default)
    {
        UpdatedEndDate = newEndDate;
        return Task.FromResult(true);
    }

    public Task<List<MucHopDongItem>> GetAllAsync(CancellationToken ct = default) =>
        Task.FromResult(Item is null ? [] : new List<MucHopDongItem> { Item });

    public Task<KetQuaNhanPhongDto> CheckinByQrAsync(int khachThueId, string tokenOrPin, CancellationToken ct = default) =>
        Task.FromResult(new KetQuaNhanPhongDto(7, "P101", 2_500_000m, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)));
}

file sealed class StubUtilityRepo : IDienNuocRepository
{
    public ChiSoDienNuocDto? Latest { get; set; }
    public ChiSoDienNuocDto? Added { get; private set; }

    public Task<ChiSoDienNuocDto?> GetLatestAsync(int phongId, CancellationToken ct = default) =>
        Task.FromResult(Latest);

    public Task<ChiSoDienNuocDto> AddAsync(ChiSoDienNuocDto reading, CancellationToken ct = default)
    {
        Added = reading with { Id = 3 };
        return Task.FromResult(Added);
    }
}
