using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Server.Repositories;
using QuanLyTro.Server.Services;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class DoiSoatServiceTests
{
    private const string KyCuoc = "2026-09";

    [TestMethod]
    public async Task Chay_KhopDungPhongVaDuTien_DanhDauDaThu()
    {
        var (service, hoaDon, giaoDich) = Dung(giaoDichs:
        [
            Gd("1", "P102 09-2026", 2_500_000m)
        ]);
        hoaDon.HoaDons.Add((7, "P102", 2_500_000m));

        var ket_qua = await service.ChayAsync();

        Assert.AreEqual(1, ket_qua.SoGiaoDichMoi);
        Assert.AreEqual(1, ket_qua.SoKhop);
        Assert.AreEqual(7, hoaDon.MarkedPaidId);
        Assert.IsTrue(giaoDich.DaKhop.ContainsKey("1"));
    }

    [TestMethod]
    public async Task Chay_ChuyenThuaVanThu()
    {
        var (service, hoaDon, _) = Dung(giaoDichs: [Gd("1", "P102 09-2026", 3_000_000m)]);
        hoaDon.HoaDons.Add((7, "P102", 2_500_000m));

        var ket_qua = await service.ChayAsync();

        Assert.AreEqual(1, ket_qua.SoKhop);
        Assert.AreEqual(7, hoaDon.MarkedPaidId);
    }

    [TestMethod]
    public async Task Chay_ChuyenThieu_KhongThu()
    {
        var (service, hoaDon, giaoDich) = Dung(giaoDichs: [Gd("1", "P102 09-2026", 1_000_000m)]);
        hoaDon.HoaDons.Add((7, "P102", 2_500_000m));

        var ket_qua = await service.ChayAsync();

        Assert.AreEqual(0, ket_qua.SoKhop);
        Assert.AreEqual(1, ket_qua.SoBoQua);
        Assert.IsNull(hoaDon.MarkedPaidId, "Chuyển thiếu không được đánh dấu đã thu");
        StringAssert.Contains(giaoDich.LyDo["1"], "Chuyển thiếu");
    }

    [TestMethod]
    public async Task Chay_NoiDungKhongCoPhong_KhongThu()
    {
        var (service, hoaDon, giaoDich) = Dung(giaoDichs: [Gd("1", "chuyen tien thang 9", 2_500_000m)]);
        hoaDon.HoaDons.Add((7, "P102", 2_500_000m));

        var ket_qua = await service.ChayAsync();

        Assert.AreEqual(0, ket_qua.SoKhop);
        Assert.AreEqual(1, ket_qua.SoBoQua);
        Assert.IsNull(hoaDon.MarkedPaidId);
        StringAssert.Contains(giaoDich.LyDo["1"], "Nội dung");
    }

    [TestMethod]
    public async Task Chay_PhongKhongCoHoaDon_KhongThu()
    {
        var (service, hoaDon, _) = Dung(giaoDichs: [Gd("1", "P999 09-2026", 2_500_000m)]);
        hoaDon.HoaDons.Add((7, "P102", 2_500_000m));

        var ket_qua = await service.ChayAsync();

        Assert.AreEqual(0, ket_qua.SoKhop);
        Assert.IsNull(hoaDon.MarkedPaidId);
    }

    [TestMethod]
    public async Task Chay_GiaoDichDaKhop_KhongThuLai()
    {
        var (service, hoaDon, giaoDich) = Dung(giaoDichs: [Gd("1", "P102 09-2026", 2_500_000m)]);
        hoaDon.HoaDons.Add((7, "P102", 2_500_000m));
        giaoDich.DaTonTai.Add("1"); // đã thấy ở lượt poll trước
        giaoDich.DaKhop["1"] = true; // và đã khớp hóa đơn — chỉ trường hợp này mới bỏ qua

        var ket_qua = await service.ChayAsync();

        Assert.AreEqual(0, ket_qua.SoGiaoDichMoi, "Giao dịch đã khớp không được tính là mới");
        Assert.IsNull(hoaDon.MarkedPaidId, "Không được thu lại lần hai");
    }

    [TestMethod]
    public async Task Chay_ChuaCoToken_BaoLoiNghiepVu()
    {
        var (service, hoaDon, _) = Dung(giaoDichs: [Gd("1", "P102 09-2026", 2_500_000m)], token: "");
        hoaDon.HoaDons.Add((7, "P102", 2_500_000m));

        var ex = await Assert.ThrowsExceptionAsync<LoiNghiepVu>(() => service.ChayAsync());

        StringAssert.Contains(ex.Message, "API token");
        Assert.IsNull(hoaDon.MarkedPaidId);
    }

    /// <summary>Giao dịch ghi nhận trước khi hóa đơn tồn tại phải được khớp lại ở lượt sau.</summary>
    [TestMethod]
    public async Task Chay_GiaoDichChuaKhopDuocThuLai()
    {
        var (service, hoaDon, giaoDich) = Dung(giaoDichs: [Gd("1", "P102 09-2026", 2_500_000m)]);

        // Lượt 1: chưa có hóa đơn → bỏ qua, ghi lý do.
        var luot1 = await service.ChayAsync();
        Assert.AreEqual(0, luot1.SoKhop);
        Assert.IsNotNull(giaoDich.LyDo["1"]);

        // Lượt 2: hóa đơn xuất hiện → giao dịch cũ phải được thử lại và thu.
        hoaDon.HoaDons.Add((7, "P102", 2_500_000m));
        var luot2 = await service.ChayAsync();

        Assert.AreEqual(1, luot2.SoKhop, "Giao dịch chưa khớp phải được thử lại khi hóa đơn xuất hiện");
        Assert.AreEqual(7, hoaDon.MarkedPaidId);
    }

    /// <summary>Nội dung chỉ ghi số phòng, thiếu kỳ cước → vẫn dò trúng hóa đơn nợ cũ nhất.</summary>
    [TestMethod]
    public async Task Chay_NoiDungKhongCoKy_DungHoaDonCuNhat()
    {
        var (service, hoaDon, _) = Dung(giaoDichs: [Gd("1", "Tien phong P102", 2_500_000m)]);
        hoaDon.HoaDons.Add((11, "P102", 2_500_000m));
        hoaDon.HoaDons.Add((12, "P102", 2_500_000m));

        var ket_qua = await service.ChayAsync();

        Assert.AreEqual(1, ket_qua.SoKhop, "Thiếu kỳ cước vẫn phải khớp được phòng và thu hóa đơn cũ nhất");
        Assert.AreEqual(11, hoaDon.MarkedPaidId);
    }

    [TestMethod]
    public async Task Chay_HoaDonVuaThuNoiKhac_KhongGhiKhop()
    {
        var (service, hoaDon, giaoDich) = Dung(giaoDichs: [Gd("1", "P102 09-2026", 2_500_000m)]);
        hoaDon.HoaDons.Add((7, "P102", 2_500_000m));
        hoaDon.MarkPaidSucceeds = false; // người khác vừa bấm thu

        var ket_qua = await service.ChayAsync();

        Assert.AreEqual(0, ket_qua.SoKhop);
        StringAssert.Contains(giaoDich.LyDo["1"], "vừa được thu ở nơi khác");
    }

    private static GiaoDichSepay Gd(string id, string noiDung, decimal soTien) =>
        new(id, noiDung, soTien, new DateTime(2026, 9, 20, 10, 0, 0), "FT" + id);

    private static (DoiSoatService, StubHoaDon, StubGiaoDich) Dung(
        List<GiaoDichSepay> giaoDichs, string token = "TOKEN")
    {
        var hoaDon = new StubHoaDon();
        var giaoDich = new StubGiaoDich();
        var service = new DoiSoatService(
            new StubCauHinh(token), giaoDich, hoaDon, new StubSePay(giaoDichs));
        return (service, hoaDon, giaoDich);
    }

    private sealed class StubCauHinh(string token) : ICauHinhRepository
    {
        public Task<CauHinhThanhToanDto> GetAsync(CancellationToken ct = default) =>
            Task.FromResult(new CauHinhThanhToanDto("MBBank", "0387368890", "NGUYEN VAN DUC"));

        public Task<CauHinhDayDuDto> GetFullAsync(CancellationToken ct = default) =>
            Task.FromResult(new CauHinhDayDuDto("MBBank", "0387368890", "NGUYEN VAN DUC", "compact", token, token.Length > 0));

        public Task SaveAsync(CauHinhThanhToanDto cau_hinh, string? api_token, CancellationToken ct = default) =>
            Task.CompletedTask;
    }

    private sealed class StubGiaoDich : IGiaoDichSepayRepository
    {
        public HashSet<string> DaTonTai { get; } = [];
        public Dictionary<string, bool> DaKhop { get; } = [];
        public Dictionary<string, string> LyDo { get; } = [];

        public Task<bool> ThemNeuChuaCoAsync(GiaoDichSepay giao_dich, CancellationToken ct = default) =>
            Task.FromResult(DaTonTai.Add(giao_dich.Id));

        public Task<bool> DaKhopChuaAsync(string sepayId, CancellationToken ct = default) =>
            Task.FromResult(DaKhop.TryGetValue(sepayId, out var da_khop) && da_khop);

        public Task DanhDauAsync(string sepayId, int? hoaDonId, string? lyDoLoai, CancellationToken ct = default)
        {
            DaKhop[sepayId] = hoaDonId.HasValue;
            if (lyDoLoai is not null) LyDo[sepayId] = lyDoLoai;
            return Task.CompletedTask;
        }

        public Task<DateTime?> LayMocThoiDiemAsync(CancellationToken ct = default) =>
            Task.FromResult<DateTime?>(null);
    }

    private sealed class StubSePay(List<GiaoDichSepay> danh_sach) : ISePayClient
    {
        public Task<List<GiaoDichSepay>> LayGiaoDichVaoAsync(
            string apiToken, DateTime tu, DateTime den, CancellationToken ct = default) =>
            Task.FromResult(danh_sach);
    }

    private sealed class StubHoaDon : IHoaDonRepository
    {
        public List<(int Id, string SoPhong, decimal TongTien)> HoaDons { get; } = [];
        public int? MarkedPaidId { get; private set; }
        public bool MarkPaidSucceeds { get; set; } = true;

        public Task<HopDongDto?> GetActiveContractAsync(int phongId, CancellationToken ct = default) =>
            Task.FromResult<HopDongDto?>(null);

        public Task<ChiSoDienNuocDto?> GetUtilityReadingAsync(string kyCuoc, int phongId, CancellationToken ct = default) =>
            Task.FromResult<ChiSoDienNuocDto?>(null);

        public Task<HoaDonDto> AddAsync(HoaDonDto invoice, CancellationToken ct = default) =>
            Task.FromResult(invoice);

        public Task<HoaDonDto?> GetByIdAsync(int hoaDonId, CancellationToken ct = default) =>
            Task.FromResult<HoaDonDto?>(null);

        public Task<bool> MarkPaidAsync(int hoaDonId, CancellationToken ct = default)
        {
            if (!MarkPaidSucceeds) return Task.FromResult(false);
            MarkedPaidId = hoaDonId;
            return Task.FromResult(true);
        }

        public Task<List<HoaDonDto>> GetAllAsync(string kyCuoc, int? phongId, CancellationToken ct = default) =>
            Task.FromResult(new List<HoaDonDto>());

        public Task<List<(int Id, string SoPhong, decimal TongTien)>> GetUnpaidWithRoomAsync(
            string kyCuoc, CancellationToken ct = default) =>
            Task.FromResult(HoaDons);

        public Task<TrangHoaDonCuaToiDto> GetByTenantAsync(
            int khachThueId, int page, int soLuongMoiTrang, CancellationToken ct = default) =>
            Task.FromResult(new TrangHoaDonCuaToiDto([], 0, page, soLuongMoiTrang));
    }
}
