using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Server.Network;
using QuanLyTro.Shared.Models;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Tests;

/// <summary>
/// BR-16: CongAn chỉ đọc/xuất hồ sơ, mọi nghiệp vụ ghi bị từ chối (server-enforced).
/// Test pure/static — không chạm MySQL nên chạy được cả khi DB down.
/// </summary>
[TestClass]
public sealed class PoliceIntegrationTests
{
    /// <summary>
    /// Allowlist read-only của CongAn, khai báo tường minh. Cố ý KHÔNG dẫn xuất từ
    /// MaTranPhanQuyen: hanh_dong mới thêm vào ActionNames mà ma trận chưa có quyết định
    /// quyền cho CongAn thì test dưới fail, buộc review (BR-16).
    /// </summary>
    private static readonly string[] PoliceReadOnlyActions =
    [
        ActionNames.DangNhap,
        ActionNames.PhongLayTatCa,
        ActionNames.KhachThueTheoPhong,
        ActionNames.XuatHoSoTamTru,
        ActionNames.LichSuCuTruLay,
        ActionNames.XuatLichSuCuTru,
    ];

    [TestMethod]
    public void VerifyPoliceEnforcement_CannotWrite_CanRead()
    {
        // Đọc/xuất hồ sơ thường trú: cho phép (PM-04, PM-05).
        Assert.IsTrue(MaTranPhanQuyen.IsAllowed(ActionNames.LichSuCuTruLay, VaiTroNguoiDung.CongAn));
        Assert.IsTrue(MaTranPhanQuyen.IsAllowed(ActionNames.XuatLichSuCuTru, VaiTroNguoiDung.CongAn));

        // Nghiệp vụ ghi: từ chối.
        Assert.IsFalse(MaTranPhanQuyen.IsAllowed(ActionNames.PhongXoa, VaiTroNguoiDung.CongAn));
        Assert.IsFalse(MaTranPhanQuyen.IsAllowed(ActionNames.KhachThueTraPhong, VaiTroNguoiDung.CongAn));
        Assert.IsFalse(MaTranPhanQuyen.IsAllowed(ActionNames.HoaDonThanhToan, VaiTroNguoiDung.CongAn));
    }

    /// <summary>
    /// Quét TOÀN BỘ ActionNames.All thay vì liệt kê tay: tập hanh_dong CongAn được phép phải
    /// ĐÚNG BẰNG allowlist. Action chưa khai báo ma trận → IsAllowed false → vào vế "cấm",
    /// nên lọt qua đây cũng là fail.
    /// </summary>
    [TestMethod]
    public void Police_AllowedActionsMatchReadOnlyAllowlistExactly()
    {
        var actualAllowed = ActionNames.All
            .Where(hanh_dong => MaTranPhanQuyen.IsAllowed(hanh_dong, VaiTroNguoiDung.CongAn))
            .ToArray();

        CollectionAssert.AreEquivalent(
            PoliceReadOnlyActions,
            actualAllowed,
            "Quyền CongAn lệch khỏi allowlist read-only (BR-16).");
    }
}
