namespace QuanLyTro.Shared.Protocol;

/// <summary>
/// Tên 18 hành động của giao thức TCP (đúng phụ lục báo cáo).
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

    public const string ContractCreate = "CONTRACT_CREATE";
    public const string ContractTerminate = "CONTRACT_TERMINATE";

    public const string UtilityGetPrevious = "UTILITY_GET_PREVIOUS";
    public const string UtilityRecord = "UTILITY_RECORD";

    public const string InvoiceCreate = "INVOICE_CREATE";
    public const string InvoiceGetAll = "INVOICE_GET_ALL";
    public const string InvoicePay = "INVOICE_PAY";

    public const string ReportSummary = "REPORT_SUMMARY";
    public const string ExportResidence = "EXPORT_RESIDENCE";
}
