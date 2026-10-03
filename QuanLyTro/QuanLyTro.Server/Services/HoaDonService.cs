using System.Globalization;
using QuanLyTro.Server.Repositories;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Services;

/// <summary>
/// Nghiệp vụ hóa đơn: BR-09 (cần HĐ HieuLuc + đã chốt điện nước), BR-10 (server tự tính tiền),
/// BR-11 (hóa đơn DaThu bất biến), BR-14 (người thuê chỉ thấy hóa đơn phòng mình).
/// </summary>
public sealed class HoaDonService(IHoaDonRepository hoa_don)
{
    /// <summary>BR-09, BR-10, BR-13. Client chỉ gửi PhongId, tháng và PhiKhac.</summary>
    public async Task<HoaDonDto> CreateAsync(YeuCauTaoHoaDon request, CancellationToken ct = default)
    {
        var kyCuoc = NormalizeMonth(request.KyCuoc);

        if (request.PhiKhac < 0)
        {
            throw new LoiNghiepVu("Phí khác không được âm.");
        }

        var contract = await hoa_don.GetActiveContractAsync(request.PhongId, ct)
            ?? throw new LoiNghiepVu("Phòng này không có hợp đồng đang hiệu lực.");

        var reading = await hoa_don.GetUtilityReadingAsync(kyCuoc, request.PhongId, ct)
            ?? throw new LoiNghiepVu("Phòng này chưa chốt điện nước tháng này.");

        var tienPhong = contract.GiaThue;
        var tienDien = (reading.DienMoi - reading.DienCu) * reading.GiaDien;
        // Nước có 2 hình thức: theo khối (m³ tiêu thụ) hoặc theo đầu người (số người × đơn giá/người).
        var tienNuoc = string.Equals(reading.HinhThucNuoc, "Nguoi", StringComparison.OrdinalIgnoreCase)
            ? reading.SoNguoiNuoc * reading.GiaNuoc
            : (reading.NuocMoi - reading.NuocCu) * reading.GiaNuoc;

        var invoice = new HoaDonDto(
            0,
            request.PhongId,
            contract.Id,
            kyCuoc,
            tienPhong,
            tienDien,
            tienNuoc,
            request.PhiKhac,
            tienPhong + tienDien + tienNuoc + request.PhiKhac,
            TrangThaiHoaDon.ChuaThu,
            null);

        return await hoa_don.AddAsync(invoice, ct);
    }

    /// <summary>US-15: lọc theo tháng (bắt buộc) và phòng (tùy chọn).</summary>
    public Task<List<HoaDonDto>> GetAllAsync(string kyCuoc, int? phongId = null, CancellationToken ct = default) =>
        hoa_don.GetAllAsync(NormalizeMonth(kyCuoc), phongId, ct);

    /// <summary>BR-11: chỉ hóa đơn `ChuaThu` mới thu được; set ngay_dong = NOW() trong repository.</summary>
    public async Task<bool> PayAsync(int hoaDonId, CancellationToken ct = default)
    {
        var invoice = await hoa_don.GetByIdAsync(hoaDonId, ct)
            ?? throw new LoiNghiepVu("Không tìm thấy hóa đơn.");

        if (invoice.TrangThai == TrangThaiHoaDon.DaThu)
        {
            throw new LoiNghiepVu("Hóa đơn đã thanh toán.");
        }

        // Hai client thu cùng lúc: kẻ thua không chạm được dòng ChuaThu nào.
        if (!await hoa_don.MarkPaidAsync(hoaDonId, ct))
        {
            throw new LoiNghiepVu("Hóa đơn đã thanh toán.");
        }

        return true;
    }

    /// <summary>BR-14: server suy phòng từ khachThueId trong phiên, client không gửi phongId. Hỗ trợ phân trang server-side.</summary>
    public Task<TrangHoaDonCuaToiDto> GetMineAsync(
        int khachThueId, int page = 1, int soLuongMoiTrang = 10, CancellationToken ct = default) =>
        hoa_don.GetByTenantAsync(khachThueId, page, soLuongMoiTrang, ct);

    /// <summary>Tháng hóa đơn là chuỗi `yyyy-MM` — dùng chung với DienNuocService.</summary>
    public static string NormalizeMonth(string? month)
    {
        var trimmed = (month ?? string.Empty).Trim();
        if (!DateOnly.TryParseExact(trimmed, "yyyy-MM", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out _))
        {
            throw new LoiNghiepVu("Tháng hóa đơn phải theo định dạng yyyy-MM.");
        }

        return trimmed;
    }
}
