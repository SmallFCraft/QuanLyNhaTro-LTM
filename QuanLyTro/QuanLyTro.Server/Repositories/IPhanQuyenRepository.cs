namespace QuanLyTro.Server.Repositories;

/// <summary>
/// Truy cập bảng `quyen_vai_tro` lưu ma trận quyền động theo vai trò.
/// </summary>
public interface IPhanQuyenRepository
{
    /// <summary>
    /// Đọc ma trận quyền hiện tại từ CSDL. Key là tên vai trò ("QuanLy", "CongAn", "KhachThue"),
    /// Value là tập các hanh_dong được phép. Nếu bảng trống (DB mới chưa seed), trả về từ QuyenMacDinhTheoVaiTro.
    /// </summary>
    Task<Dictionary<string, List<string>>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Cập nhật toàn bộ quyền cho một vai trò cụ thể. Thực hiện trong GiaoDich:
    /// Xóa toàn bộ quyền cũ của vai trò đó và ghi đè danh sách mới.
    /// </summary>
    Task UpdateRoleActionsAsync(string vai_tro, IEnumerable<string> hanh_dong, CancellationToken ct = default);
}
