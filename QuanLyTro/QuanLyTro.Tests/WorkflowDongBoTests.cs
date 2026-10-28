using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Server.Repositories;
using QuanLyTro.Server.Services;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Tests;

/// <summary>
/// Ba điểm lệch workflow giữa trang Chủ trọ và Khách thuê:
/// 1. Chủ trọ không thấy phòng đã có HĐ mà chưa lập hóa đơn.
/// 2. Khách thuê đọc "0 đ" thành "không phải trả gì" khi chủ trọ chưa lập hóa đơn.
/// 3. Chốt được chỉ số cho kỳ trước ngày bắt đầu hợp đồng → sinh chỉ số cũ rác, tính sai tiền.
/// </summary>
[TestClass]
public sealed class WorkflowDongBoTests
{
    private static string Wwwroot =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "wwwroot");

    // ---- Sửa 3: chặn kỳ trước ngày hợp đồng ----

    [TestMethod]
    public async Task GhiChiSo_KyTruocNgayBatDauHopDong_BiChan()
    {
        var service = DienNuoc(new DateOnly(2026, 10, 25));

        var loi = await Assert.ThrowsExceptionAsync<LoiNghiepVu>(() =>
            service.RecordAsync(ChiSo("2026-09")));

        StringAssert.Contains(loi.Message, "trước ngày bắt đầu hợp đồng");
    }

    [TestMethod]
    public async Task GhiChiSo_DungThangBatDau_ChoPhep()
    {
        var service = DienNuoc(new DateOnly(2026, 10, 25));

        var ket_qua = await service.RecordAsync(ChiSo("2026-10"));

        Assert.AreEqual("2026-10", ket_qua.KyCuoc);
    }

    [TestMethod]
    public async Task GhiChiSo_PhongChuaCoHopDong_KhongChan()
    {
        // Không có hợp đồng → không có mốc nào để so, vẫn cho chốt (dữ liệu lịch sử).
        var service = DienNuoc(ngayBatDau: null);

        var ket_qua = await service.RecordAsync(ChiSo("2020-01"));

        Assert.AreEqual("2020-01", ket_qua.KyCuoc);
    }

    // ---- Sửa 1: cảnh báo thiếu hóa đơn ----

    [TestMethod]
    public void Dash_CoCanhBaoThieuHoaDon()
    {
        var html = File.ReadAllText(Path.Combine(Wwwroot, "chutro", "index.html"));
        Assert.IsTrue(html.Contains("id=\"dash-thieu-hd\""), "Tổng quan thiếu khối cảnh báo thiếu hóa đơn");
        Assert.IsTrue(html.Contains("id=\"dash-thieu-hd-text\""), "Thiếu chỗ hiển thị danh sách phòng");

        var js = File.ReadAllText(Path.Combine(Wwwroot, "chutro", "js", "dash.js"));
        Assert.IsTrue(js.Contains("renderThieuHoaDon"), "dash.js chưa render cảnh báo thiếu hóa đơn");
        Assert.IsTrue(js.Contains("soPhongThieuHoaDon"), "dash.js chưa đọc số phòng thiếu hóa đơn từ server");
    }

    [TestMethod]
    public void BaoCaoDto_CoTruongThieuHoaDon()
    {
        var dto = new BaoCaoTongQuanDto(10, 2, 8, 20, 1_000m, 500m, 2, ["P105", "P107"]);

        Assert.AreEqual(2, dto.SoPhongThieuHoaDon);
        CollectionAssert.AreEqual(new[] { "P105", "P107" }, dto.PhongThieuHoaDon);
    }

    [TestMethod]
    public void BaoCaoDto_MacDinhKhongCoPhongThieu()
    {
        // Call site cũ (5 tham số) vẫn hợp lệ — tham số mới có giá trị mặc định.
        var dto = new BaoCaoTongQuanDto(10, 2, 8, 20, 1_000m, 500m);

        Assert.AreEqual(0, dto.SoPhongThieuHoaDon);
        Assert.IsNull(dto.PhongThieuHoaDon);
    }

    // ---- Sửa 2: khách thuê phân biệt hai trạng thái ----

    [TestMethod]
    public void Tenant_PhanBietChuaLapHoaDonVaChuaToiKy()
    {
        var js = File.ReadAllText(Path.Combine(Wwwroot, "khachthue", "js", "khach_thue.js"));

        Assert.IsTrue(js.Contains("Chủ trọ chưa lập hóa đơn kỳ này"),
            "khách phải được nói rõ chủ trọ chưa lập hóa đơn, không phải 'chưa phát sinh'");
        Assert.IsTrue(js.Contains("Chưa phát sinh kỳ cước nào"),
            "vẫn phải giữ câu 'chưa phát sinh' cho trường hợp chưa tới kỳ");
        Assert.IsTrue(js.Contains("ngayBatDau"),
            "phải dùng ngày bắt đầu hợp đồng để phân biệt hai trường hợp");
    }

    [TestMethod]
    public void Tenant_CoNutKiemTraThanhToan_KhongGoiActionChuTro()
    {
        var html = File.ReadAllText(Path.Combine(Wwwroot, "khachthue", "index.html"));
        Assert.IsTrue(html.Contains("kiemTraThanhToan()"), "khách thuê thiếu nút kiểm tra thanh toán");

        var js = File.ReadAllText(Path.Combine(Wwwroot, "khachthue", "js", "khach_thue.js"));
        Assert.IsTrue(js.Contains("async function kiemTraThanhToan"), "thiếu hàm kiểm tra thanh toán");
        // Đối soát là việc của Chủ trọ: mỗi lần gọi là 1 request SePay, khách không có quyền.
        // Chỉ chặn lời gọi thật (bridge.call), không chặn việc nhắc tên trong comment.
        Assert.IsFalse(js.Contains("'DOI_SOAT_CHAY'") || js.Contains("\"DOI_SOAT_CHAY\""),
            "khách thuê không được gọi DOI_SOAT_CHAY — chỉ nạp lại hóa đơn của chính mình");
    }

    private static DienNuocService DienNuoc(DateOnly? ngayBatDau)
    {
        var hop_dong = new StubHopDong(ngayBatDau);
        return new DienNuocService(new StubDienNuoc(), hop_dong);
    }

    private static ChiSoDienNuocDto ChiSo(string kyCuoc) =>
        new(0, 990177, kyCuoc, 0, 60, 3500m, 0, 0, 12000m);

    private sealed class StubHopDong(DateOnly? ngayBatDau) : IHopDongRepository
    {
        public Task<List<MucHopDongItem>> GetAllAsync(CancellationToken ct = default)
        {
            if (ngayBatDau is null) return Task.FromResult(new List<MucHopDongItem>());
            var contract = new HopDongDto(
                1, 990177, 5, ngayBatDau.Value, ngayBatDau.Value.AddYears(1),
                50_000m, 0m, TrangThaiHopDong.HieuLuc, null);
            return Task.FromResult(new List<MucHopDongItem> { new(contract, "P105", "Test") });
        }

        public Task<HopDongDto?> GetByIdAsync(int hopDongId, CancellationToken ct = default) =>
            Task.FromResult<HopDongDto?>(null);
        public Task<bool> HasActiveContractAsync(int phongId, CancellationToken ct = default) =>
            Task.FromResult(false);
        public Task<bool> IsTenantInRoomAsync(int khachThueId, int phongId, CancellationToken ct = default) =>
            Task.FromResult(false);
        public Task<HopDongDto> AddAsync(HopDongDto contract, CancellationToken ct = default) =>
            Task.FromResult(contract);
        public Task<bool> TerminateAsync(int hopDongId, string? ghi_chu, DateOnly ngayThongBao, CancellationToken ct = default) =>
            Task.FromResult(false);
        public Task<bool> UpdateEndDateAsync(int hopDongId, DateOnly newEndDate, CancellationToken ct = default) =>
            Task.FromResult(false);
        public Task<KetQuaNhanPhongDto> CheckinByQrAsync(int khachThueId, string tokenOrPin, CancellationToken ct = default) =>
            Task.FromResult(new KetQuaNhanPhongDto(0, string.Empty, 0m, default, default));
    }

    private sealed class StubDienNuoc : IDienNuocRepository
    {
        public Task<ChiSoDienNuocDto?> GetLatestAsync(int phongId, CancellationToken ct = default) =>
            Task.FromResult<ChiSoDienNuocDto?>(null);
        public Task<ChiSoDienNuocDto> AddAsync(ChiSoDienNuocDto reading, CancellationToken ct = default) =>
            Task.FromResult(reading with { Id = 1 });
    }
}
