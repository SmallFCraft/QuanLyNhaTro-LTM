namespace QuanLyTro.Shared.Protocol;

/// <summary>
/// Tên 22 hành động của giao thức TCP: 18 gốc + 4 bổ sung cho phân vai người thuê.
/// </summary>
public static class ActionNames
{
    public const string AuthLogin = "AUTH_LOGIN";

    public const string RoomGetAll = "ROOM_GET_ALL";
    public const string RoomAdd = "ROOM_ADD";
    public const string RoomUpdate = "ROOM_UPDATE";
    public const string RoomDelete = "ROOM_DELETE";

    public const string TenantGetByRoom = "TENANT_GET_BY_ROOM";
    public const string TenantAdd = "TENANT_ADD";
    public const string TenantUpdate = "TENANT_UPDATE";
    public const string TenantCheckout = "TENANT_CHECKOUT";
    public const string TenantDelete = "TENANT_DELETE";

    public const string ContractCreate = "CONTRACT_CREATE";
    public const string ContractTerminate = "CONTRACT_TERMINATE";
    public const string ContractRenew = "CONTRACT_RENEW";
    public const string ContractGetAll = "CONTRACT_GET_ALL";

    public const string UtilityGetPrevious = "UTILITY_GET_PREVIOUS";
    public const string UtilityRecord = "UTILITY_RECORD";

    public const string InvoiceCreate = "INVOICE_CREATE";
    public const string InvoiceGetAll = "INVOICE_GET_ALL";
    public const string InvoicePay = "INVOICE_PAY";
    public const string InvoiceGetMine = "INVOICE_GET_MINE";

    public const string ReportSummary = "REPORT_SUMMARY";
    public const string ExportResidence = "EXPORT_RESIDENCE";

    public const string ResidenceHistoryGet = "RESIDENCE_HISTORY_GET";
    public const string ExportResidenceHistory = "EXPORT_RESIDENCE_HISTORY";

    // Quản lý phân quyền động — CHỈ Chủ trọ (Landlord) được gọi.
    public const string PermissionGetMatrix = "PERMISSION_GET_MATRIX";
    public const string PermissionUpdateRole = "PERMISSION_UPDATE_ROLE";

    /// <summary>Toàn bộ tên action hợp lệ — dùng cho test và kiểm tra router.</summary>
    public static readonly IReadOnlyList<string> All =
    [
        AuthLogin,
        RoomGetAll, RoomAdd, RoomUpdate, RoomDelete,
        TenantGetByRoom, TenantAdd, TenantUpdate, TenantCheckout, TenantDelete,
        ContractCreate, ContractTerminate, ContractRenew, ContractGetAll,
        UtilityGetPrevious, UtilityRecord,
        InvoiceCreate, InvoiceGetAll, InvoicePay, InvoiceGetMine,
        ReportSummary, ExportResidence,
        ResidenceHistoryGet, ExportResidenceHistory,
        PermissionGetMatrix, PermissionUpdateRole,
    ];
}
