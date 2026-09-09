namespace QuanLyTro.Shared.Models;

public enum InvoiceStatus
{
    Unpaid,
    Paid
}

public sealed record InvoiceDto(
    int Id,
    int RoomId,
    int ContractId,
    string BillingMonth, // yyyy-MM
    decimal RoomAmount,
    decimal ElectricityAmount,
    decimal WaterAmount,
    decimal OtherFees,
    decimal TotalAmount,
    InvoiceStatus Status,
    DateTime? PaidAt
);
