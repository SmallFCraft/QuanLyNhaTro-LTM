using QuanLyTro.Shared.Models;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Server.Network;

/// <summary>
/// Ma trận quyền động `Action → AllowedRoles`. Server-enforced (BR-14): Client chỉ ẩn/hiện menu
/// theo VaiTro cho UX, không tự quyết định quyền.
///
/// Nguyên tắc Hybrid RBAC:
/// 1. `DANG_NHAP` luôn cho phép mọi vai trò.
/// 2. `ChuTro` (Chủ trọ): Toàn quyền mọi hanh_dong quản trị (trừ `HOA_DON_CUA_TOI` của người thuê).
///    Bypass cứng trong code để KHÔNG BAO GIỜ bị khóa ngoài hệ thống bởi bất kỳ cấu hình CSDL nào.
/// 3. `QuanLy`, `CongAn`, `KhachThue`: Tra cứu từ bộ nhớ đệm in-memory (O(1)), nạp từ CSDL bảng `quyen_vai_tro`.
/// </summary>
public static class MaTranPhanQuyen
{
    // Hoán đổi NGUYÊN TỬ cả bảng, không sửa từng phần tử: `ApplyMatrix` chạy trong lúc các phiên
    // QuanLy/CongAn/KhachThue đang gọi IsAllowed. Nếu xóa-rồi-nạp tại chỗ, cửa sổ giữa 2 bước sẽ
    // từ chối oan mọi request của họ (bảng rỗng tạm thời).
    private static IReadOnlyDictionary<string, HashSet<VaiTroNguoiDung>> _allowed =
        Build(QuyenMacDinhTheoVaiTro.All.ToDictionary(
            kv => kv.Key,
            kv => (List<string>)kv.Value.ToList(),
            StringComparer.OrdinalIgnoreCase));

    /// <summary>Kiểm tra vai trò có được phép thực hiện hành động này không.</summary>
    public static bool IsAllowed(string hanh_dong, VaiTroNguoiDung vai_tro)
    {
        if (string.IsNullOrWhiteSpace(hanh_dong)) return false;

        // DANG_NHAP luôn mở cho tất cả
        if (hanh_dong == ActionNames.DangNhap) return true;

        // Chủ trọ (ChuTro) luôn có toàn quyền mọi hanh_dong quản trị
        if (vai_tro == VaiTroNguoiDung.ChuTro)
        {
            return hanh_dong != ActionNames.HoaDonCuaToi;
        }

        // Các vai trò khác: tra cứu bộ đệm động
        var snapshot = Volatile.Read(ref _allowed);
        return snapshot.TryGetValue(hanh_dong, out var allowed) && allowed.Contains(vai_tro);
    }

    /// <summary>Action có tồn tại trong danh mục hệ thống không.</summary>
    public static bool IsKnown(string hanh_dong) => ActionNames.All.Contains(hanh_dong);

    /// <summary>
    /// Áp dụng ma trận quyền động mới từ CSDL (gọi khi khởi động Server hoặc khi Chủ trọ cập nhật phân quyền).
    /// </summary>
    public static void ApplyMatrix(IReadOnlyDictionary<string, List<string>> quyenTheoVaiTro) =>
        // Hoán đổi nguyên tử con trỏ bảng quyền — luồng TCP đang đọc không bao giờ thấy bảng rỗng.
        Volatile.Write(ref _allowed, Build(quyenTheoVaiTro));

    /// <summary>Khôi phục quyền mặc định từ code (dùng khi khởi động hoặc reset test).</summary>
    public static void ResetToDefaults() =>
        Volatile.Write(ref _allowed, Build(QuyenMacDinhTheoVaiTro.All.ToDictionary(
            kv => kv.Key,
            kv => (List<string>)kv.Value.ToList(),
            StringComparer.OrdinalIgnoreCase)));

    private static Dictionary<string, HashSet<VaiTroNguoiDung>> Build(IReadOnlyDictionary<string, List<string>> quyenTheoVaiTro)
    {
        var next = new Dictionary<string, HashSet<VaiTroNguoiDung>>(StringComparer.OrdinalIgnoreCase)
        {
            // Đăng nhập ai cũng phải gọi được, không phụ thuộc cấu hình phân quyền.
            [ActionNames.DangNhap] = [VaiTroNguoiDung.ChuTro, VaiTroNguoiDung.QuanLy, VaiTroNguoiDung.CongAn, VaiTroNguoiDung.KhachThue],
        };

        foreach (var (roleName, actionsList) in quyenTheoVaiTro)
        {
            if (!Enum.TryParse<VaiTroNguoiDung>(roleName, ignoreCase: true, out var vai_tro))
            {
                continue;
            }

            foreach (var a in actionsList)
            {
                if (!next.TryGetValue(a, out var set))
                {
                    set = [];
                    next[a] = set;
                }
                set.Add(vai_tro);
            }
        }

        return next;
    }
}
