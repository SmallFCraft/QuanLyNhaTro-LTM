using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Server.Network;
using QuanLyTro.Server.Repositories;
using QuanLyTro.Server.Security;
using QuanLyTro.Server.Services;
using QuanLyTro.Shared.Models;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class TenantLockTests
{
    [TestMethod]
    public async Task LockedTenant_WithCorrectPassword_IsRejectedWithReason()
    {
        var hash = PasswordHasher.Hash("123456");
        var tenant = new KhachThueAuthRecord(10, "048203012345", hash, "Trần Văn B", 101, BiKhoa: true, LyDoKhoa: "Nợ tiền phòng");
        var auth = new XacThucService(new StubUserRepo(), new StubTenantRepo(tenant), new SessionStore());

        var ex = await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
            () => auth.LoginAsync(new YeuCauDangNhap("048203012345", "123456")));

        StringAssert.Contains(ex.Message, XacThucService.LockedAccountMessage);
        StringAssert.Contains(ex.Message, "Nợ tiền phòng");
    }

    [TestMethod]
    public async Task LockedTenant_WithWrongPassword_GetsGenericMessage()
    {
        // Không lộ trạng thái khóa cho người chưa chứng minh được mật khẩu.
        var hash = PasswordHasher.Hash("123456");
        var tenant = new KhachThueAuthRecord(10, "048203012345", hash, "Trần Văn B", 101, BiKhoa: true);
        var auth = new XacThucService(new StubUserRepo(), new StubTenantRepo(tenant), new SessionStore());

        var ex = await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
            () => auth.LoginAsync(new YeuCauDangNhap("048203012345", "sai")));

        Assert.AreEqual(XacThucService.InvalidCredentialsMessage, ex.Message);
    }

    [TestMethod]
    public void TenantLock_Action_IsAllowedOnlyForLandlordAndManager()
    {
        Assert.IsTrue(MaTranPhanQuyen.IsAllowed(ActionNames.KhachThueKhoa, VaiTroNguoiDung.ChuTro));
        Assert.IsTrue(MaTranPhanQuyen.IsAllowed(ActionNames.KhachThueKhoa, VaiTroNguoiDung.QuanLy));
        Assert.IsFalse(MaTranPhanQuyen.IsAllowed(ActionNames.KhachThueKhoa, VaiTroNguoiDung.CongAn));
        Assert.IsFalse(MaTranPhanQuyen.IsAllowed(ActionNames.KhachThueKhoa, VaiTroNguoiDung.KhachThue));
    }

    [TestMethod]
    public void TenantLock_Action_IsRegistered()
    {
        CollectionAssert.Contains((System.Collections.ICollection)ActionNames.All, ActionNames.KhachThueKhoa);
    }

    /// <summary>
    /// Tab Người thuê phải gọi đúng action và render đúng trạng thái khóa — nếu thiếu một trong hai,
    /// server khóa được nhưng chủ trọ không thấy gì (đúng kiểu lệch tầng đã xảy ra với P203).
    /// </summary>
    [TestMethod]
    public void TenantTab_RendersLockState_AndCallsLockAction()
    {
        var js = File.ReadAllText(Path.Combine(Wwwroot, "chutro", "js", "khach_thue.js"));
        StringAssert.Contains(js, "'KHACH_THUE_KHOA'");
        StringAssert.Contains(js, "t.biKhoa");
        StringAssert.Contains(js, "toggleTenantLock");

        var html = File.ReadAllText(Path.Combine(Wwwroot, "chutro", "index.html"));
        StringAssert.Contains(html, "<th>Tài khoản</th>");
    }

    private static string Wwwroot =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "wwwroot");

    private sealed class StubUserRepo : ITaiKhoanRepository
    {
        public Task<TaiKhoanRecord?> FindByUsernameAsync(string tenDangNhap, CancellationToken ct = default) =>
            Task.FromResult<TaiKhoanRecord?>(null);
    }

    private sealed class StubTenantRepo(KhachThueAuthRecord? tenant) : IKhachThueRepository
    {
        public Task<KhachThueAuthRecord?> FindByCccdAsync(string cccd, CancellationToken ct = default) =>
            Task.FromResult(tenant is not null && tenant.Cccd == cccd ? tenant : null);
    }
}
