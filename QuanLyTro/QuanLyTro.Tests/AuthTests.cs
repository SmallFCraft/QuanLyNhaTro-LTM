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

    private static (AuthService Auth, SessionStore Sessions) MakeService(
        UserRecord? user,
        TenantAuthRecord? tenant,
        Func<DateTimeOffset>? clock = null)
    {
        var sessions = new SessionStore();
        var auth = new AuthService(new StubUserRepo(user), new StubTenantRepo(tenant), sessions, clock);
        return (auth, sessions);
    }

    [TestMethod]
    public async Task AuthService_LogsInLandlord()
    {
        var hash = PasswordHasher.Hash("admin-pass");
        var (auth, sessions) = MakeService(new UserRecord(1, hash, "Nguyễn Văn A"), null);

        var result = await auth.LoginAsync(new LoginRequest("admin", "admin-pass"));

        Assert.AreEqual(UserRole.Landlord, result.Role);
        Assert.IsTrue(sessions.TryGet(result.Token, out var s));
        Assert.AreEqual((1, UserRole.Landlord), s);
    }

    [TestMethod]
    public async Task AuthService_LogsInPolice_ReturnsPoliceRoleAndCreatesPoliceSession()
    {
        var hash = PasswordHasher.Hash("police-pass");
        var (auth, sessions) = MakeService(
            new UserRecord(2, hash, "Đại úy Lê Văn C", UserRole.Police), null);

        var result = await auth.LoginAsync(new LoginRequest("police_nhs", "police-pass"));

        Assert.AreEqual(UserRole.Police, result.Role, "C3: AuthService phải trả UserRole.Police cho tài khoản công an.");
        Assert.AreEqual("Đại úy Lê Văn C", result.FullName);
        Assert.IsTrue(sessions.TryGet(result.Token, out var s), "Session phải được tạo trong SessionStore.");
        Assert.AreEqual((2, UserRole.Police), s, "Session role phải là Police, không phải Landlord.");
    }

    [TestMethod]
    public async Task AuthService_LogsInTenantByCccd()
    {
        var hash = PasswordHasher.Hash("123456");
        var (auth, sessions) = MakeService(null, new TenantAuthRecord(10, "048203012345", hash, "Trần Văn B", 101));

        var result = await auth.LoginAsync(new LoginRequest("048203012345", "123456"));

        Assert.AreEqual(UserRole.Tenant, result.Role);
        Assert.AreEqual("Trần Văn B", result.FullName);
        Assert.IsTrue(sessions.TryGet(result.Token, out var s));
        Assert.AreEqual((10, UserRole.Tenant), s);
    }

    [TestMethod]
    public async Task AuthService_SameMessageForUnknownUserAndWrongPassword()
    {
        var (auth, _) = MakeService(new UserRecord(1, PasswordHasher.Hash("pass"), "A"), null);

        var exUnknown = await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
            () => auth.LoginAsync(new LoginRequest("ghost", "x")));
        var exWrongPassword = await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
            () => auth.LoginAsync(new LoginRequest("admin", "sai")));

        Assert.AreEqual(exUnknown.Message, exWrongPassword.Message);
        Assert.AreEqual(AuthService.InvalidCredentialsMessage, exUnknown.Message);
    }

    [TestMethod]
    public async Task AuthService_TenantWithoutPasswordCannotLogin()
    {
        // password_hash IS NULL → repo trả null → đăng nhập từ chối.
        var (auth, _) = MakeService(null, null);
        await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
            () => auth.LoginAsync(new LoginRequest("048203012345", "123456")));
    }

    [TestMethod]
    public async Task AuthService_LocksAfterFiveFailuresForOneMinute()
    {
        var clock = new FakeClock(Now);
        var (auth, _) = MakeService(null, null, clock.Now);

        for (var i = 0; i < AuthService.MaxFailedAttempts; i++)
        {
            await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
                () => auth.LoginAsync(new LoginRequest("victim", "wrong")));
        }

        // Lần thứ 6 (đúng mật khẩu cũng bị chặn) → thông báo khóa.
        var ex = await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
            () => auth.LoginAsync(new LoginRequest("victim", "wrong")));
        StringAssert.Contains(ex.Message, "Tài khoản tạm khóa");

        clock.Advance(TimeSpan.FromSeconds(59));
        await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
            () => auth.LoginAsync(new LoginRequest("victim", "wrong")));

        clock.Advance(TimeSpan.FromSeconds(2)); // vượt qua 1 phút
        var exAfter = await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
            () => auth.LoginAsync(new LoginRequest("victim", "wrong")));
        Assert.AreEqual(AuthService.InvalidCredentialsMessage, exAfter.Message);
    }

    [TestMethod]
    public async Task AuthService_SuccessfulLoginResetsFailureCounter()
    {
        var clock = new FakeClock(Now);
        var (auth, _) = MakeService(
            new UserRecord(1, PasswordHasher.Hash("right"), "A"), null, clock.Now);

        for (var i = 0; i < AuthService.MaxFailedAttempts - 1; i++)
        {
            await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
                () => auth.LoginAsync(new LoginRequest("admin", "wrong")));
        }

        var ok = await auth.LoginAsync(new LoginRequest("admin", "right"));
        Assert.AreEqual(UserRole.Landlord, ok.Role);

        // Sai tiếp không bị khóa ngay vì bộ đếm đã reset.
        var ex = await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
            () => auth.LoginAsync(new LoginRequest("admin", "wrong")));
        Assert.AreEqual(AuthService.InvalidCredentialsMessage, ex.Message);
    }

    [TestMethod]
    public void SessionStore_CreatesUniqueTokens()
    {
        var store = new SessionStore();
        var t1 = store.Create(1, UserRole.Landlord);
        var t2 = store.Create(1, UserRole.Landlord);

        Assert.AreNotEqual(t1, t2);
        Assert.IsTrue(store.TryGet(t1, out var s));
        Assert.AreEqual((1, UserRole.Landlord), s);
    }

    [TestMethod]
    public void SessionStore_RejectsInvalidOrRemovedTokens()
    {
        var store = new SessionStore();
        var t = store.Create(42, UserRole.Tenant);

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

file sealed class StubUserRepo(UserRecord? user) : IUserRepository
{
    // Stub không có username: trả record cho mọi username khi user != null (test dùng username cố định).
    public Task<UserRecord?> FindByUsernameAsync(string username, CancellationToken ct = default) =>
        Task.FromResult(user);
}

file sealed class StubTenantRepo(TenantAuthRecord? tenant) : ITenantRepository
{
    public Task<TenantAuthRecord?> FindByCccdAsync(string idCard, CancellationToken ct = default) =>
        Task.FromResult(tenant is not null && tenant.IdCard == idCard ? tenant : null);
}
