using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Server.Network;
using QuanLyTro.Shared.Models;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Tests;

/// <summary>
/// BR-16: Police chỉ đọc/xuất hồ sơ, mọi nghiệp vụ ghi bị từ chối (server-enforced).
/// Test pure/static — không chạm MySQL nên chạy được cả khi DB down.
/// </summary>
[TestClass]
public sealed class PoliceIntegrationTests
{
    /// <summary>
    /// Allowlist read-only của Police, khai báo tường minh. Cố ý KHÔNG dẫn xuất từ
    /// PermissionMatrix: action mới thêm vào ActionNames mà ma trận chưa có quyết định
    /// quyền cho Police thì test dưới fail, buộc review (BR-16).
    /// </summary>
    private static readonly string[] PoliceReadOnlyActions =
    [
        ActionNames.AuthLogin,
        ActionNames.RoomGetAll,
        ActionNames.TenantGetByRoom,
        ActionNames.ExportResidence,
        ActionNames.ResidenceHistoryGet,
        ActionNames.ExportResidenceHistory,
    ];

    [TestMethod]
    public void VerifyPoliceEnforcement_CannotWrite_CanRead()
    {
        // Đọc/xuất hồ sơ thường trú: cho phép (PM-04, PM-05).
        Assert.IsTrue(PermissionMatrix.IsAllowed(ActionNames.ResidenceHistoryGet, UserRole.Police));
        Assert.IsTrue(PermissionMatrix.IsAllowed(ActionNames.ExportResidenceHistory, UserRole.Police));

        // Nghiệp vụ ghi: từ chối.
        Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.RoomDelete, UserRole.Police));
        Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.TenantCheckout, UserRole.Police));
        Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.InvoicePay, UserRole.Police));
    }

    /// <summary>
    /// Quét TOÀN BỘ ActionNames.All thay vì liệt kê tay: tập action Police được phép phải
    /// ĐÚNG BẰNG allowlist. Action chưa khai báo ma trận → IsAllowed false → vào vế "cấm",
    /// nên lọt qua đây cũng là fail.
    /// </summary>
    [TestMethod]
    public void Police_AllowedActionsMatchReadOnlyAllowlistExactly()
    {
        var actualAllowed = ActionNames.All
            .Where(action => PermissionMatrix.IsAllowed(action, UserRole.Police))
            .ToArray();

        CollectionAssert.AreEquivalent(
            PoliceReadOnlyActions,
            actualAllowed,
            "Quyền Police lệch khỏi allowlist read-only (BR-16).");
    }
}
