namespace QuanLyTro.Shared.Models;

public sealed record UtilityReadingDto(
    int Id,
    int RoomId,
    string BillingMonth, // yyyy-MM
    int OldElectricity,
    int NewElectricity,
    decimal ElectricityRate,
    int OldWater,
    int NewWater,
    decimal WaterRate
);
