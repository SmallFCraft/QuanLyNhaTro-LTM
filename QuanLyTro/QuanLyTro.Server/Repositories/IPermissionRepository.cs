namespace QuanLyTro.Server.Repositories;

/// <summary>
/// Truy cập bảng `role_permissions` lưu ma trận quyền động theo vai trò.
/// </summary>
public interface IPermissionRepository
{
    /// <summary>
    /// Đọc ma trận quyền hiện tại từ CSDL. Key là tên vai trò ("Manager", "Police", "Tenant"),
    /// Value là tập các action được phép. Nếu bảng trống (DB mới chưa seed), trả về từ DefaultRolePermissions.
    /// </summary>
    Task<Dictionary<string, List<string>>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Cập nhật toàn bộ quyền cho một vai trò cụ thể. Thực hiện trong Transaction:
    /// Xóa toàn bộ quyền cũ của vai trò đó và ghi đè danh sách mới.
    /// </summary>
    Task UpdateRoleActionsAsync(string role, IEnumerable<string> actions, CancellationToken ct = default);
}
