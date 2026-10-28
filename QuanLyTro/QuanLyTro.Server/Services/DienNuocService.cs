using System.Globalization;
using QuanLyTro.Server.Repositories;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Services;

/// <summary>
/// Nghiệp vụ điện nước: BR-07 (chỉ số mới &gt;= chỉ số cũ), BR-08 (1 bản ghi / phòng / tháng,
/// chặn bởi DB UNIQUE uq_room_month — xem DienNuocRepository).
/// </summary>
public sealed class DienNuocService(IDienNuocRepository banGhi, IHopDongRepository? hop_dong = null)
{
    /// <summary>
    /// Chỉ số cũ kỳ này: bản ghi gần nhất TRƯỚC `kyCuoc` (null nếu chưa từng chốt).
    /// Overload 2 tham số (phongId, ct) giữ nguyên cho call site cũ — không có chặn dưới.
    /// </summary>
    public Task<ChiSoDienNuocDto?> GetPreviousReadingAsync(int phongId, CancellationToken ct = default) =>
        banGhi.GetLatestAsync(phongId, ct);

    /// <summary>Chỉ số cũ của kỳ trước `kyCuoc` — bắt buộc có tháng, `yyyy-MM`.</summary>
    public async Task<ChiSoDienNuocDto?> GetPreviousReadingAsync(
        int phongId, string kyCuoc, CancellationToken ct = default)
    {
        if (!IsValidMonth(kyCuoc))
        {
            throw new LoiNghiepVu("Tháng chốt điện nước phải theo định dạng yyyy-MM.");
        }

        return await banGhi.GetLatestBeforeAsync(phongId, kyCuoc.Trim(), ct);
    }

    /// <summary>BR-07. Tháng hóa đơn là chuỗi `yyyy-MM`.</summary>
    public async Task<ChiSoDienNuocDto> RecordAsync(ChiSoDienNuocDto reading, CancellationToken ct = default)
    {
        Validate(reading);
        var kyCuoc = reading.KyCuoc.Trim();
        await ChanKyTruocHopDongAsync(reading.PhongId, kyCuoc, ct);

        var previous = await banGhi.GetLatestBeforeAsync(reading.PhongId, kyCuoc, ct);
        if (previous is not null && reading.DienCu != previous.DienMoi)
        {
            throw new LoiNghiepVu("Chỉ số cũ điện phải tiếp nối chỉ số mới của kỳ trước.");
        }
        var isNguoi = string.Equals(reading.HinhThucNuoc, "Nguoi", StringComparison.OrdinalIgnoreCase);
        if (!isNguoi && previous is not null && reading.NuocCu != previous.NuocMoi)
        {
            throw new LoiNghiepVu("Chỉ số cũ nước phải tiếp nối chỉ số mới của kỳ trước.");
        }

        return await banGhi.AddAsync(reading with { KyCuoc = kyCuoc }, ct);
    }

    /// <summary>
    /// Không cho chốt chỉ số cho kỳ TRƯỚC tháng bắt đầu hợp đồng: phòng chưa có người ở thì
    /// không tiêu thụ gì, và một bản ghi rác ở kỳ đó sẽ thành "chỉ số cũ" của kỳ thật → tính sai tiền.
    /// Chỉ áp khi phòng đang có HĐ hiệu lực / chờ nhận phòng; phòng chưa có HĐ thì bỏ qua.
    /// </summary>
    private async Task ChanKyTruocHopDongAsync(int phongId, string kyCuoc, CancellationToken ct)
    {
        if (hop_dong is null) return;

        var tat_ca = await hop_dong.GetAllAsync(ct);
        var hop_dong_cua_phong = tat_ca
            .Where(c => c.Contract.PhongId == phongId
                && c.Contract.TrangThai is TrangThaiHopDong.HieuLuc or TrangThaiHopDong.ChoNhanPhong)
            .Select(c => c.Contract)
            .OrderByDescending(c => c.NgayBatDau)
            .FirstOrDefault();

        if (hop_dong_cua_phong is null) return;

        var thang_bat_dau = hop_dong_cua_phong.NgayBatDau.ToString("yyyy-MM", CultureInfo.InvariantCulture);
        // kyCuoc là chuỗi cố định yyyy-MM nên so sánh chuỗi trùng thứ tự thời gian.
        if (string.CompareOrdinal(kyCuoc, thang_bat_dau) < 0)
        {
            throw new LoiNghiepVu(
                $"Kỳ {kyCuoc} trước ngày bắt đầu hợp đồng ({hop_dong_cua_phong.NgayBatDau:dd/MM/yyyy}).");
        }
    }

    public static void Validate(ChiSoDienNuocDto reading)
    {
        if (!IsValidMonth(reading.KyCuoc))
        {
            throw new LoiNghiepVu("Tháng chốt điện nước phải theo định dạng yyyy-MM.");
        }

        if (reading.DienMoi < reading.DienCu)
        {
            throw new LoiNghiepVu("Chỉ số điện mới phải lớn hơn hoặc bằng chỉ số cũ.");
        }

        var isNguoi = string.Equals(reading.HinhThucNuoc, "Nguoi", StringComparison.OrdinalIgnoreCase);
        if (isNguoi)
        {
            if (reading.SoNguoiNuoc <= 0)
            {
                throw new LoiNghiepVu("Số người dùng nước phải lớn hơn 0.");
            }
        }
        else
        {
            if (reading.NuocMoi < reading.NuocCu)
            {
                throw new LoiNghiepVu("Chỉ số nước mới phải lớn hơn hoặc bằng chỉ số cũ.");
            }
        }

        if (reading.GiaDien <= 0 || reading.GiaNuoc <= 0)
        {
            throw new LoiNghiepVu("Đơn giá điện và nước phải lớn hơn 0.");
        }
    }

    private static bool IsValidMonth(string? month) =>
        DateOnly.TryParseExact((month ?? string.Empty).Trim(), "yyyy-MM",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
}
