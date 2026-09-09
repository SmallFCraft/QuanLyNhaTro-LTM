namespace QuanLyTro.Shared.Models;

public sealed record TenantDto(
    int Id,
    int? RoomId,
    string FullName,
    DateOnly DateOfBirth,
    string IdCard,
    string Phone,
    string Hometown,
    string? Workplace,
    bool IsTemporaryRegistered
);
