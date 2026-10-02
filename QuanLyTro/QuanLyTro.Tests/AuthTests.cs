using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Server.Repositories;
using QuanLyTro.Server.Security;
using QuanLyTro.Server.Services;
using QuanLyTro.Shared.Models;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class AuthTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 10, 8, 0, 0, TimeSpan.Zero);

    private static (XacThucService Auth, SessionStore Sessions) MakeService(
        TaiKhoanRecord? user,
        KhachThueAuthRecord? tenant,
        Func<DateTimeOffset>? clock = null)
    {
        var sessions = new SessionStore();
        var auth = new XacThucService(new StubUserRepo(user), new StubTenantRepo(tenant), sessions, clock);
        return (auth, sessions);
    }

    [TestMethod]
    public async Task AuthService_LogsInLandlord()
    {
        var hash = PasswordHasher.Hash("admin-pass");
        var (auth, sessions) = MakeService(new TaiKhoanRecord(1, hash, "Nguyễn Văn A"), null);

        var result = await auth.LoginAsync(new YeuCauDangNhap("admin", "admin-pass"));

        Assert.AreEqual(VaiTroNguoiDung.ChuTro, result.VaiTro);
        Assert.IsTrue(sessions.TryGet(result.Token, out var s));
        Assert.AreEqual((1, VaiTroNguoiDung.ChuTro), s);
    }

    [TestMethod]
    public async Task AuthService_LogsInPolice_ReturnsPoliceRoleAndCreatesPoliceSession()
    {
        var hash = PasswordHasher.Hash("police-pass");
        var (auth, sessions) = MakeService(
            new TaiKhoanRecord(2, hash, "Đại úy Lê Văn C", VaiTroNguoiDung.CongAn), null);

        var result = await auth.LoginAsync(new YeuCauDangNhap("police_nhs", "police-pass"));

        Assert.AreEqual(VaiTroNguoiDung.CongAn, result.VaiTro, "C3: XacThucService phải trả VaiTroNguoiDung.CongAn cho tài khoản công an.");
        Assert.AreEqual("Đại úy Lê Văn C", result.HoTen);
        Assert.IsTrue(sessions.TryGet(result.Token, out var s), "Session phải được tạo trong SessionStore.");
        Assert.AreEqual((2, VaiTroNguoiDung.CongAn), s, "Session vai_tro phải là CongAn, không phải ChuTro.");
    }

    [TestMethod]
    public async Task AuthService_LogsInTenantByCccd()
    {
        var hash = PasswordHasher.Hash("123456");
        var (auth, sessions) = MakeService(null, new KhachThueAuthRecord(10, "048203012345", hash, "Trần Văn B", 101));

        var result = await auth.LoginAsync(new YeuCauDangNhap("048203012345", "123456"));

        Assert.AreEqual(VaiTroNguoiDung.KhachThue, result.VaiTro);
        Assert.AreEqual("Trần Văn B", result.HoTen);
        Assert.IsTrue(sessions.TryGet(result.Token, out var s));
        Assert.AreEqual((10, VaiTroNguoiDung.KhachThue), s);
    }

    [TestMethod]
    public async Task AuthService_SameMessageForUnknownUserAndWrongPassword()
    {
        var (auth, _) = MakeService(new TaiKhoanRecord(1, PasswordHasher.Hash("pass"), "A"), null);

        var exUnknown = await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
            () => auth.LoginAsync(new YeuCauDangNhap("ghost", "x")));
        var exWrongPassword = await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
            () => auth.LoginAsync(new YeuCauDangNhap("admin", "sai")));

        Assert.AreEqual(exUnknown.Message, exWrongPassword.Message);
        Assert.AreEqual(XacThucService.InvalidCredentialsMessage, exUnknown.Message);
    }

    [TestMethod]
    public async Task AuthService_TenantWithoutPasswordCannotLogin()
    {
        // mat_khau_hash IS NULL → repo trả null → đăng nhập từ chối.
        var (auth, _) = MakeService(null, null);
        await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
            () => auth.LoginAsync(new YeuCauDangNhap("048203012345", "123456")));
    }

    [TestMethod]
    public async Task AuthService_LocksAfterFiveFailuresForOneMinute()
    {
        var clock = new FakeClock(Now);
        var (auth, _) = MakeService(null, null, clock.Now);

        for (var i = 0; i < XacThucService.MaxFailedAttempts; i++)
        {
            await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
                () => auth.LoginAsync(new YeuCauDangNhap("victim", "wrong")));
        }

        // Lần thứ 6 (đúng mật khẩu cũng bị chặn) → thông báo khóa.
        var ex = await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
            () => auth.LoginAsync(new YeuCauDangNhap("victim", "wrong")));
        StringAssert.Contains(ex.Message, "Tài khoản tạm khóa");

        clock.Advance(TimeSpan.FromSeconds(59));
        await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
            () => auth.LoginAsync(new YeuCauDangNhap("victim", "wrong")));

        clock.Advance(TimeSpan.FromSeconds(2)); // vượt qua 1 phút
        var exAfter = await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
            () => auth.LoginAsync(new YeuCauDangNhap("victim", "wrong")));
        Assert.AreEqual(XacThucService.InvalidCredentialsMessage, exAfter.Message);
    }

    [TestMethod]
    public async Task AuthService_SuccessfulLoginResetsFailureCounter()
    {
        var clock = new FakeClock(Now);
        var (auth, _) = MakeService(
            new TaiKhoanRecord(1, PasswordHasher.Hash("right"), "A"), null, clock.Now);

        for (var i = 0; i < XacThucService.MaxFailedAttempts - 1; i++)
        {
            await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
                () => auth.LoginAsync(new YeuCauDangNhap("admin", "wrong")));
        }

        var ok = await auth.LoginAsync(new YeuCauDangNhap("admin", "right"));
        Assert.AreEqual(VaiTroNguoiDung.ChuTro, ok.VaiTro);

        // Sai tiếp không bị khóa ngay vì bộ đếm đã reset.
        var ex = await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
            () => auth.LoginAsync(new YeuCauDangNhap("admin", "wrong")));
        Assert.AreEqual(XacThucService.InvalidCredentialsMessage, ex.Message);
    }

    [TestMethod]
    public void SessionStore_CreatesUniqueTokens()
    {
        var store = new SessionStore();
        var t1 = store.Create(1, VaiTroNguoiDung.ChuTro);
        var t2 = store.Create(1, VaiTroNguoiDung.ChuTro);

        Assert.AreNotEqual(t1, t2);
        Assert.IsTrue(store.TryGet(t1, out var s));
        Assert.AreEqual((1, VaiTroNguoiDung.ChuTro), s);
    }

    [TestMethod]
    public void SessionStore_RejectsInvalidOrRemovedTokens()
    {
        var store = new SessionStore();
        var t = store.Create(42, VaiTroNguoiDung.KhachThue);

        Assert.IsFalse(store.TryGet("not-a-token", out _));
        Assert.IsFalse(store.TryGet("", out _));

        store.Remove(t);
        Assert.IsFalse(store.TryGet(t, out _));
    }
}

file sealed class FakeClock(DateTimeOffset start)
{
    private DateTimeOffset _now = start;

    public DateTimeOffset Now() => _now;

    public void Advance(TimeSpan delta) => _now = _now.Add(delta);
}

file sealed class StubUserRepo(TaiKhoanRecord? user) : ITaiKhoanRepository
{
    // Stub không có tenDangNhap: trả record cho mọi tenDangNhap khi user != null (test dùng tenDangNhap cố định).
    public Task<TaiKhoanRecord?> FindByUsernameAsync(string tenDangNhap, CancellationToken ct = default) =>
        Task.FromResult(user);
}

file sealed class StubTenantRepo(KhachThueAuthRecord? tenant) : IKhachThueRepository
{
    public Task<KhachThueAuthRecord?> FindByCccdAsync(string cccd, CancellationToken ct = default) =>
        Task.FromResult(tenant is not null && tenant.Cccd == cccd ? tenant : null);
}
