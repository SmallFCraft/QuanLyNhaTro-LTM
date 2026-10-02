using QuanLyTro.Shared.Models;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Server.Network;

/// <summary>
/// Ma trận quyền động `Action → AllowedRoles`. Server-enforced (BR-14): Client chỉ ẩn/hiện menu
/// theo Role cho UX, không tự quyết định quyền.
///
/// Nguyên tắc Hybrid RBAC:
/// 1. `AUTH_LOGIN` luôn cho phép mọi vai trò.
/// 2. `Landlord` (Chủ trọ): Toàn quyền mọi action quản trị (trừ `INVOICE_GET_MINE` của người thuê).
///    Bypass cứng trong code để KHÔNG BAO GIỜ bị khóa ngoài hệ thống bởi bất kỳ cấu hình CSDL nào.
/// 3. `Manager`, `Police`, `Tenant`: Tra cứu từ bộ nhớ đệm in-memory (O(1)), nạp từ CSDL bảng `role_permissions`.
/// </summary>
public static class PermissionMatrix
{
    // Hoán đổi NGUYÊN TỬ cả bảng, không sửa từng phần tử: `ApplyMatrix` chạy trong lúc các phiên
    // Manager/Police/Tenant đang gọi IsAllowed. Nếu xóa-rồi-nạp tại chỗ, cửa sổ giữa 2 bước sẽ
    // từ chối oan mọi request của họ (bảng rỗng tạm thời).
    private static IReadOnlyDictionary<string, HashSet<UserRole>> _allowed =
        Build(DefaultRolePermissions.All.ToDictionary(
            kv => kv.Key,
            kv => (List<string>)kv.Value.ToList(),
            StringComparer.OrdinalIgnoreCase));

    /// <summary>Kiểm tra vai trò có được phép thực hiện hành động này không.</summary>
    public static bool IsAllowed(string action, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(action)) return false;

        // AUTH_LOGIN luôn mở cho tất cả
        if (action == ActionNames.AuthLogin) return true;

        // Chủ trọ (Landlord) luôn có toàn quyền mọi action quản trị
        if (role == UserRole.Landlord)
        {
            return action != ActionNames.InvoiceGetMine;
        }

        // Các vai trò khác: tra cứu bộ đệm động
        var snapshot = Volatile.Read(ref _allowed);
        return snapshot.TryGetValue(action, out var roles) && roles.Contains(role);
    }

    /// <summary>Action có tồn tại trong danh mục hệ thống không.</summary>
    public static bool IsKnown(string action) => ActionNames.All.Contains(action);

    /// <summary>
    /// Áp dụng ma trận quyền động mới từ CSDL (gọi khi khởi động Server hoặc khi Chủ trọ cập nhật phân quyền).
    /// </summary>
    public static void ApplyMatrix(IReadOnlyDictionary<string, List<string>> roleActions) =>
        // Hoán đổi nguyên tử con trỏ bảng quyền — luồng TCP đang đọc không bao giờ thấy bảng rỗng.
        Volatile.Write(ref _allowed, Build(roleActions));

    /// <summary>Khôi phục quyền mặc định từ code (dùng khi khởi động hoặc reset test).</summary>
    public static void ResetToDefaults() =>
        Volatile.Write(ref _allowed, Build(DefaultRolePermissions.All.ToDictionary(
            kv => kv.Key,
            kv => (List<string>)kv.Value.ToList(),
            StringComparer.OrdinalIgnoreCase)));

    private static Dictionary<string, HashSet<UserRole>> Build(IReadOnlyDictionary<string, List<string>> roleActions)
    {
        var next = new Dictionary<string, HashSet<UserRole>>(StringComparer.OrdinalIgnoreCase)
        {
            // Đăng nhập ai cũng phải gọi được, không phụ thuộc cấu hình phân quyền.
            [ActionNames.AuthLogin] = [UserRole.Landlord, UserRole.Manager, UserRole.Police, UserRole.Tenant],
        };

        foreach (var (roleName, actions) in roleActions)
        {
            if (!Enum.TryParse<UserRole>(roleName, ignoreCase: true, out var role))
            {
                continue;
            }

            foreach (var action in actions)
            {
                if (!next.TryGetValue(action, out var set))
                {
                    set = [];
                    next[action] = set;
                }
                set.Add(role);
            }
        }

        return next;
    }
}
