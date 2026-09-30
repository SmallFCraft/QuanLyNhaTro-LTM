using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Server.Network;
using QuanLyTro.Shared.Models;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class PermissionMatrixTests
{
    [TestMethod]
    public void AllActions_AreRegisteredInMatrix()
    {
        foreach (var action in ActionNames.All)
        {
            Assert.IsTrue(PermissionMatrix.IsKnown(action), $"Action chưa khai báo quyền: {action}");
        }
    }

    [TestMethod]
    public void Tenant_HasAccessOnlyToAuthAndInvoiceGetMine()
    {
        Assert.IsTrue(PermissionMatrix.IsAllowed(ActionNames.AuthLogin, UserRole.Tenant));
        Assert.IsTrue(PermissionMatrix.IsAllowed(ActionNames.InvoiceGetMine, UserRole.Tenant));

        // Mọi action ghi hoặc list khác đều bị từ chối với Tenant (BR-14).
        Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.RoomGetAll, UserRole.Tenant));
        Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.RoomAdd, UserRole.Tenant));
        Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.InvoiceGetAll, UserRole.Tenant));
        Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.InvoicePay, UserRole.Tenant));
        Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.ContractCreate, UserRole.Tenant));
        Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.TenantAdd, UserRole.Tenant));
        Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.ReportSummary, UserRole.Tenant));
        Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.ExportResidence, UserRole.Tenant));
    }

    [TestMethod]
    public void Police_HasReadOnlyAccessToAllowedActions_AndRejectedOnAllWrites()
    {
        // Được phép đọc
        Assert.IsTrue(PermissionMatrix.IsAllowed(ActionNames.AuthLogin, UserRole.Police));
        Assert.IsTrue(PermissionMatrix.IsAllowed(ActionNames.RoomGetAll, UserRole.Police));
        Assert.IsTrue(PermissionMatrix.IsAllowed(ActionNames.TenantGetByRoom, UserRole.Police));
        Assert.IsTrue(PermissionMatrix.IsAllowed(ActionNames.ExportResidence, UserRole.Police));
        Assert.IsTrue(PermissionMatrix.IsAllowed(ActionNames.ResidenceHistoryGet, UserRole.Police));
        Assert.IsTrue(PermissionMatrix.IsAllowed(ActionNames.ExportResidenceHistory, UserRole.Police));

        // Bị cấm toàn bộ thao tác ghi (BR-16)
        Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.RoomAdd, UserRole.Police));
        Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.RoomUpdate, UserRole.Police));
        Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.RoomDelete, UserRole.Police));
        Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.TenantAdd, UserRole.Police));
        Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.TenantUpdate, UserRole.Police));
        Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.TenantCheckout, UserRole.Police));
        Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.TenantDelete, UserRole.Police));
        Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.ContractCreate, UserRole.Police));
        Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.ContractTerminate, UserRole.Police));
        Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.UtilityRecord, UserRole.Police));
        Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.InvoiceCreate, UserRole.Police));
        Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.InvoicePay, UserRole.Police));
        Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.InvoiceGetMine, UserRole.Police));
    }

    [TestMethod]
    public void Landlord_HasAccessToAllActionsExceptInvoiceGetMine()
    {
        foreach (var action in ActionNames.All)
        {
            if (action == ActionNames.InvoiceGetMine)
            {
                Assert.IsFalse(PermissionMatrix.IsAllowed(action, UserRole.Landlord));
            }
            else
            {
                Assert.IsTrue(PermissionMatrix.IsAllowed(action, UserRole.Landlord), $"Landlord thiếu quyền: {action}");
            }
        }
    }
}
