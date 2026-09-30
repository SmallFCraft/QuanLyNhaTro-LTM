namespace QuanLyTro.Shared.Models;

public sealed record ResidenceHistoryDto(
    int Id,
    string FullName,
    string IdCard,
    string RoomNumber,
    string EventType, // "Vào", "Ra", "Chuyển phòng"
    DateTime EventDate,
    string? Notes);

public sealed record ExportHistoryRequest(
    DateTime FromDate,
    DateTime ToDate,
    string Format, // "CSV", "Excel", "PDF"
    string? RoomNumber,
    string? EventType);

public sealed record ExportResult(
    string FilePath,
    int RowCount);
