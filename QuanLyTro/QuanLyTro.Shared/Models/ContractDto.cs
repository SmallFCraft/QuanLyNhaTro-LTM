namespace QuanLyTro.Shared.Models;

public enum ContractStatus
{
    Active,
    Expired,
    Terminated
}

public sealed record ContractDto(
    int Id,
    int RoomId,
    int RepresentativeTenantId,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal RentalPrice,
    decimal DepositAmount,
    ContractStatus Status,
    string? Notes
);
