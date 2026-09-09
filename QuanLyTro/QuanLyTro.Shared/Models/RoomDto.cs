namespace QuanLyTro.Shared.Models;

public enum RoomStatus
{
    Available,
    Rented,
    Maintenance
}

public sealed record RoomDto(
    int Id,
    string RoomNumber,
    decimal Price,
    int MaxOccupants,
    RoomStatus Status,
    string? Description,
    int CurrentOccupants = 0
);
