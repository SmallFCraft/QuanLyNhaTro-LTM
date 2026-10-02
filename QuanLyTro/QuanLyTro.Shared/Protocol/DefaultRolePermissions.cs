namespace QuanLyTro.Shared.Protocol;

/// <summary>
/// Bộ quyền MẶC ĐỊNH cho từng vai trò cấp dưới (Manager/Police/Tenant). Một nguồn sự thật duy nhất:
/// DemoSeeder dùng để gieo vào bảng `role_permissions`, PermissionMatrix dùng làm fallback khi
/// DB chưa có dòng nào (DB cũ, hoặc Chủ trọ chưa từng mở màn Phân quyền).
///
/// Chủ trọ (Landlord) KHÔNG có mặt ở đây — quyền Chủ trọ bypass cứng trong PermissionMatrix.
/// </summary>
public static class DefaultRolePermissions
{
    /// <summary>Quản lý: toàn bộ nghiệp vụ vận hành cơ sở. Không có quyền quản trị phân quyền.</summary>
    public static readonly IReadOnlyList<string> Manager =
    [
        ActionNames.RoomGetAll, ActionNames.RoomAdd, ActionNames.RoomUpdate, ActionNames.RoomDelete,
        ActionNames.TenantGetByRoom, ActionNames.TenantAdd, ActionNames.TenantUpdate,
        ActionNames.TenantCheckout, ActionNames.TenantDelete,
        ActionNames.ContractCreate, ActionNames.ContractTerminate, ActionNames.ContractRenew,
        ActionNames.ContractGetAll,
        ActionNames.UtilityGetPrevious, ActionNames.UtilityRecord,
        ActionNames.InvoiceCreate, ActionNames.InvoiceGetAll, ActionNames.InvoicePay,
        ActionNames.ReportSummary,
        ActionNames.ExportResidence, ActionNames.ResidenceHistoryGet, ActionNames.ExportResidenceHistory,
    ];

    /// <summary>Công an phường: chỉ đọc phòng / người thuê / hồ sơ tạm trú (BR-16).</summary>
    public static readonly IReadOnlyList<string> Police =
    [
        ActionNames.RoomGetAll,
        ActionNames.TenantGetByRoom,
        ActionNames.ExportResidence,
        ActionNames.ResidenceHistoryGet,
        ActionNames.ExportResidenceHistory,
    ];

    /// <summary>Người thuê: chỉ xem hóa đơn của chính mình.</summary>
    public static readonly IReadOnlyList<string> Tenant =
    [
        ActionNames.InvoiceGetMine,
    ];

    /// <summary>Role (đúng chính tả ENUM) → danh sách action mặc định.</summary>
    public static IReadOnlyDictionary<string, IReadOnlyList<string>> All { get; } =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Manager"] = Manager,
            ["Police"] = Police,
            ["Tenant"] = Tenant,
        };
}
