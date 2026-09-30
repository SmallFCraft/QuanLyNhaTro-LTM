using QuanLyTro.Shared.Models;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Server.Network;

/// <summary>
/// Ma trận quyền tĩnh `Action → AllowedRoles`. Server-enforced (BR-14): Client chỉ ẩn/hiện menu
/// theo Role cho UX, không tự quyết định quyền.
/// </summary>
public static class PermissionMatrix
{
    // Phải khai báo TRƯỚC `Allowed`: static initializer chạy theo thứ tự khai báo.
    private static readonly HashSet<UserRole> LandlordOnly = [UserRole.Landlord];

    // Police chỉ được đọc danh sách phòng / người thuê / xuất hồ sơ thường trú (BR-16).
    private static readonly HashSet<UserRole> LandlordAndPolice = [UserRole.Landlord, UserRole.Police];

    private static readonly IReadOnlyDictionary<string, HashSet<UserRole>> Allowed =
        new Dictionary<string, HashSet<UserRole>>(StringComparer.Ordinal)
        {
            [ActionNames.AuthLogin] = Roles(UserRole.Landlord, UserRole.Tenant, UserRole.Police),

            [ActionNames.RoomGetAll] = LandlordAndPolice,
            [ActionNames.RoomAdd] = LandlordOnly,
            [ActionNames.RoomUpdate] = LandlordOnly,
            [ActionNames.RoomDelete] = LandlordOnly,

            [ActionNames.TenantGetByRoom] = LandlordAndPolice,
            [ActionNames.TenantAdd] = LandlordOnly,
            [ActionNames.TenantUpdate] = LandlordOnly,
            [ActionNames.TenantCheckout] = LandlordOnly,
            [ActionNames.TenantDelete] = LandlordOnly,

            [ActionNames.ContractCreate] = LandlordOnly,
            [ActionNames.ContractTerminate] = LandlordOnly,
            [ActionNames.ContractRenew] = LandlordOnly,
            [ActionNames.ContractGetAll] = LandlordOnly,

            [ActionNames.UtilityGetPrevious] = LandlordOnly,
            [ActionNames.UtilityRecord] = LandlordOnly,

            [ActionNames.InvoiceCreate] = LandlordOnly,
            [ActionNames.InvoiceGetAll] = LandlordOnly,
            [ActionNames.InvoicePay] = LandlordOnly,
            [ActionNames.InvoiceGetMine] = Roles(UserRole.Tenant),

            [ActionNames.ReportSummary] = LandlordOnly,
            [ActionNames.ExportResidence] = LandlordAndPolice,

            [ActionNames.ResidenceHistoryGet] = LandlordAndPolice,
            [ActionNames.ExportResidenceHistory] = LandlordAndPolice,
        };

    public static bool IsAllowed(string action, UserRole role) =>
        Allowed.TryGetValue(action, out var roles) && roles is not null && roles.Contains(role);

    /// <summary>Action không nằm trong ma trận (chưa khai báo) coi như bị từ chối.</summary>
    public static bool IsKnown(string action) => Allowed.ContainsKey(action);

    private static HashSet<UserRole> Roles(params UserRole[] roles) => [.. roles];
}
