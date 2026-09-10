using System.Globalization;
using QuanLyTro.Server.Repositories;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Services;

/// <summary>
/// Nghiệp vụ điện nước: BR-07 (chỉ số mới &gt;= chỉ số cũ), BR-08 (1 bản ghi / phòng / tháng,
/// chặn bởi DB UNIQUE uq_room_month — xem UtilityRepository).
/// </summary>
public sealed class UtilityService(IUtilityRepository readings)
{
    /// <summary>
    /// Chỉ số cũ kỳ này: bản ghi gần nhất TRƯỚC `billingMonth` (null nếu chưa từng chốt).
    /// Overload 2 tham số (roomId, ct) giữ nguyên cho call site cũ — không có chặn dưới.
    /// </summary>
    public Task<UtilityReadingDto?> GetPreviousReadingAsync(int roomId, CancellationToken ct = default) =>
        readings.GetLatestAsync(roomId, ct);

    /// <summary>Chỉ số cũ của kỳ trước `billingMonth` — bắt buộc có tháng, `yyyy-MM`.</summary>
    public async Task<UtilityReadingDto?> GetPreviousReadingAsync(
        int roomId, string billingMonth, CancellationToken ct = default)
    {
        if (!IsValidMonth(billingMonth))
        {
            throw new BusinessRuleException("Tháng chốt điện nước phải theo định dạng yyyy-MM.");
        }

        return await readings.GetLatestBeforeAsync(roomId, billingMonth.Trim(), ct);
    }

    /// <summary>BR-07. Tháng hóa đơn là chuỗi `yyyy-MM`.</summary>
    public async Task<UtilityReadingDto> RecordAsync(UtilityReadingDto reading, CancellationToken ct = default)
    {
        Validate(reading);
        var billingMonth = reading.BillingMonth.Trim();
        var previous = await readings.GetLatestBeforeAsync(reading.RoomId, billingMonth, ct);
        if (previous is not null && reading.OldElectricity != previous.NewElectricity)
        {
            throw new BusinessRuleException("Chỉ số cũ điện phải tiếp nối chỉ số mới của kỳ trước.");
        }
        if (previous is not null && reading.OldWater != previous.NewWater)
        {
            throw new BusinessRuleException("Chỉ số cũ nước phải tiếp nối chỉ số mới của kỳ trước.");
        }

        return await readings.AddAsync(reading with { BillingMonth = billingMonth }, ct);
    }

    public static void Validate(UtilityReadingDto reading)
    {
        if (!IsValidMonth(reading.BillingMonth))
        {
            throw new BusinessRuleException("Tháng chốt điện nước phải theo định dạng yyyy-MM.");
        }

        if (reading.NewElectricity < reading.OldElectricity)
        {
            throw new BusinessRuleException("Chỉ số điện mới phải lớn hơn hoặc bằng chỉ số cũ.");
        }

        if (reading.NewWater < reading.OldWater)
        {
            throw new BusinessRuleException("Chỉ số nước mới phải lớn hơn hoặc bằng chỉ số cũ.");
        }

        if (reading.ElectricityRate <= 0 || reading.WaterRate <= 0)
        {
            throw new BusinessRuleException("Đơn giá điện và nước phải lớn hơn 0.");
        }
    }

    private static bool IsValidMonth(string? month) =>
        DateOnly.TryParseExact((month ?? string.Empty).Trim(), "yyyy-MM",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
}
