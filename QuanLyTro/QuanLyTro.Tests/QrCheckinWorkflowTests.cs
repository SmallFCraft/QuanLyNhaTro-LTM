using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Server.Repositories;
using QuanLyTro.Server.Services;
using QuanLyTro.Shared.Models;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class QrCheckinWorkflowTests
{
    private static readonly DateOnly Start = new(2026, 10, 1);
    private static readonly DateOnly End = new(2027, 10, 1);

    [TestMethod]
    public void SinhMaQrToken_Dung128BitEntropyVaDinhDang32Hex()
    {
        var token1 = HopDongService.SinhMaQrToken();
        var token2 = HopDongService.SinhMaQrToken();

        Assert.AreEqual(32, token1.Length);
        Assert.AreEqual(32, token2.Length);
        Assert.AreNotEqual(token1, token2);
        Assert.IsTrue(token1.All(char.IsAsciiHexDigit));
    }

    [TestMethod]
    public void SinhMaPin_Dung8ChuSo()
    {
        var pin = HopDongService.SinhMaPin();
        Assert.AreEqual(8, pin.Length);
        Assert.IsTrue(pin.All(char.IsAsciiDigit));
    }

    [TestMethod]
    public void DinhDangQrDataString_DungChuanGiaoThuc()
    {
        const string token = "0123456789abcdef0123456789abcdef";
        var payload = HopDongService.DinhDangQrPayload(token);
        Assert.AreEqual("QUANLYTRO:NHANPHONG:0123456789abcdef0123456789abcdef", payload);

        var extracted = HopDongService.TrichXuatToken(payload);
        Assert.AreEqual(token, extracted);

        var plainToken = HopDongService.TrichXuatToken(token);
        Assert.AreEqual(token, plainToken);
    }

    [TestMethod]
    public async Task CreateAsync_ChoNhanPhong_TuDongSinhTokenVaPin()
    {
        var repo = new FakeHopDongRepo();
        var service = new HopDongService(repo);

        var req = new HopDongDto(
            Id: 0,
            PhongId: 10,
            NguoiDaiDienId: 99,
            NgayBatDau: Start,
            NgayKetThuc: End,
            GiaThue: 2_000_000m,
            TienCoc: 1_000_000m,
            TrangThai: TrangThaiHopDong.ChoNhanPhong,
            GhiChu: "Chờ nhận phòng"
        );

        var created = await service.CreateAsync(req);

        Assert.AreEqual(TrangThaiHopDong.ChoNhanPhong, created.TrangThai);
        Assert.IsNotNull(created.MaQrToken);
        Assert.AreEqual(32, created.MaQrToken.Length);
        Assert.IsNotNull(created.MaPin);
        Assert.AreEqual(8, created.MaPin.Length);
    }
}

file sealed class FakeHopDongRepo : IHopDongRepository
{
    public HopDongDto? Added { get; private set; }

    public Task<HopDongDto?> GetByIdAsync(int hopDongId, CancellationToken ct = default) =>
        Task.FromResult<HopDongDto?>(Added?.Id == hopDongId ? Added : null);

    public Task<bool> HasActiveContractAsync(int phongId, CancellationToken ct = default) =>
        Task.FromResult(false);

    public Task<bool> IsTenantInRoomAsync(int khachThueId, int phongId, CancellationToken ct = default) =>
        Task.FromResult(false);

    public Task<HopDongDto> AddAsync(HopDongDto contract, CancellationToken ct = default)
    {
        Added = contract with { Id = 123 };
        return Task.FromResult(Added);
    }

    public Task<bool> TerminateAsync(int hopDongId, string? ghi_chu, DateOnly ngayThongBao, CancellationToken ct = default) =>
        Task.FromResult(true);

    public Task<bool> UpdateEndDateAsync(int hopDongId, DateOnly newEndDate, CancellationToken ct = default) =>
        Task.FromResult(true);

    public Task<List<MucHopDongItem>> GetAllAsync(CancellationToken ct = default) =>
        Task.FromResult(new List<MucHopDongItem>());

    public Task<KetQuaNhanPhongDto> CheckinByQrAsync(int khachThueId, string tokenOrPin, CancellationToken ct = default) =>
        Task.FromResult(new KetQuaNhanPhongDto(123, "P1", 100m, new DateOnly(2026, 10, 1), new DateOnly(2027, 10, 1)));
}
