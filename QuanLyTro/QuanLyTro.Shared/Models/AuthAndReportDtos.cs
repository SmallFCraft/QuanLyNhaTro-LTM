namespace QuanLyTro.Shared.Models;

/// <summary>Vai trò phiên đăng nhập.</summary>
public enum UserRole
{
    Landlord,
    Tenant,
    Police,
}

/// <summary>
/// Payload đăng nhập: { Username, Password }
/// </summary>
public sealed record LoginRequest(string Username, string Password);

/// <summary>
/// Kết quả đăng nhập: { Token, FullName, Role }
/// </summary>
public sealed record LoginResult(string Token, string FullName, UserRole Role);

/// <summary>
/// Payload lấy chỉ số kỳ trước: { RoomId } -> { OldElectricity, OldWater }
/// </summary>
public sealed record RoomQuery(int RoomId);

/// <summary>
/// Payload tạo hóa đơn (Client chỉ gửi RoomId, tháng, phí khác — Server tự tính).
/// </summary>
public sealed record CreateInvoiceRequest(int RoomId, string BillingMonth, decimal OtherFees);

/// <summary>
/// Thống kê tổng quan: US-04, US-18, US-19.
/// </summary>
public sealed record SummaryReportDto(
    int TotalRooms,
    int AvailableRooms,
    int RentedRooms,
    int CurrentTenants,
    decimal PaidAmount,
    decimal UnpaidAmount
);

/// <summary>
/// Dòng xuất danh sách tạm trú (US-08): họ tên, ngày sinh, CCCD, quê quán, phòng.
/// </summary>
public sealed record ResidenceExportDto(
    string FullName,
    DateOnly DateOfBirth,
    string IdCard,
    string Hometown,
    string RoomNumber
);
