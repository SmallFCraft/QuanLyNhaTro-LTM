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
    /// <summary>Chỉ số mới nhất của phòng (null nếu chưa từng chốt) — dùng làm chỉ số cũ kỳ này.</summary>
    public Task<UtilityReadingDto?> GetPreviousReadingAsync(int roomId, CancellationToken ct = default) =>
        readings.GetLatestAsync(roomId, ct);

    /// <summary>BR-07. Tháng hóa đơn là chuỗi `yyyy-MM`.</summary>
    public async Task<UtilityReadingDto> RecordAsync(UtilityReadingDto reading, CancellationToken ct = default)
    {
        Validate(reading);
        return await readings.AddAsync(reading with { BillingMonth = reading.BillingMonth.Trim() }, ct);
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
