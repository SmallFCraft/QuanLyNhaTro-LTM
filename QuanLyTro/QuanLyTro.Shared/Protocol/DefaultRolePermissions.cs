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

    /// <summary>
    /// Danh mục action CÓ THỂ gán quyền, kèm nhóm và mô tả tiếng Việt — màn Phân quyền của Chủ trọ
    /// render thẳng từ đây nên thêm action mới chỉ cần sửa một chỗ.
    /// `AUTH_LOGIN` và 2 action quản trị phân quyền KHÔNG có mặt: đăng nhập ai cũng có,
    /// còn phân quyền là đặc quyền cứng của Chủ trọ.
    /// </summary>
    public static IReadOnlyList<Models.RolePermissionItemDto> Catalog { get; } =
    [
        new(ActionNames.RoomGetAll, "Xem danh sách phòng", "Phòng trọ"),
        new(ActionNames.RoomAdd, "Thêm phòng", "Phòng trọ"),
        new(ActionNames.RoomUpdate, "Sửa phòng", "Phòng trọ"),
        new(ActionNames.RoomDelete, "Xóa phòng", "Phòng trọ"),

        new(ActionNames.TenantGetByRoom, "Xem khách thuê theo phòng", "Khách thuê"),
        new(ActionNames.TenantAdd, "Thêm khách thuê", "Khách thuê"),
        new(ActionNames.TenantUpdate, "Sửa thông tin khách thuê", "Khách thuê"),
        new(ActionNames.TenantCheckout, "Trả phòng", "Khách thuê"),
        new(ActionNames.TenantDelete, "Xóa khách thuê", "Khách thuê"),

        new(ActionNames.ContractCreate, "Tạo hợp đồng", "Hợp đồng"),
        new(ActionNames.ContractRenew, "Gia hạn hợp đồng", "Hợp đồng"),
        new(ActionNames.ContractTerminate, "Chấm dứt hợp đồng", "Hợp đồng"),
        new(ActionNames.ContractGetAll, "Xem danh sách hợp đồng", "Hợp đồng"),

        new(ActionNames.UtilityGetPrevious, "Xem chỉ số điện nước kỳ trước", "Điện nước"),
        new(ActionNames.UtilityRecord, "Ghi chỉ số điện nước", "Điện nước"),

        new(ActionNames.InvoiceCreate, "Tạo hóa đơn", "Hóa đơn"),
        new(ActionNames.InvoiceGetAll, "Xem danh sách hóa đơn", "Hóa đơn"),
        new(ActionNames.InvoicePay, "Thu tiền / thanh toán hóa đơn", "Hóa đơn"),
        new(ActionNames.InvoiceGetMine, "Xem hóa đơn của chính mình", "Hóa đơn"),

        new(ActionNames.ReportSummary, "Xem thống kê doanh thu", "Báo cáo & cư trú"),
        new(ActionNames.ExportResidence, "Xuất hồ sơ tạm trú", "Báo cáo & cư trú"),
        new(ActionNames.ResidenceHistoryGet, "Tra cứu lịch sử cư trú", "Báo cáo & cư trú"),
        new(ActionNames.ExportResidenceHistory, "Xuất lịch sử cư trú", "Báo cáo & cư trú"),
    ];
}
