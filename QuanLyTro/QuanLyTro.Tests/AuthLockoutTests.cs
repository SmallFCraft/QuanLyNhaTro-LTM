using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Server.Repositories;
using QuanLyTro.Server.Security;
using QuanLyTro.Server.Services;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Tests;

/// <summary>
/// Khóa đăng nhập phải theo TÀI KHOẢN, không theo đúng chuỗi người dùng gõ. MySQL so khớp
/// `username`/`id_card` bằng utf8mb4_unicode_ci (không phân biệt hoa/thường), nên "admin",
/// "Admin", "ADMIN", " admin " đều trỏ cùng một dòng và phải chung một bộ đếm khóa.
/// </summary>
[TestClass]
public sealed class AuthLockoutTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 10, 8, 0, 0, TimeSpan.Zero);
    private static readonly string Hash = PasswordHasher.Hash("right");

    private static AuthService MakeService(params string[] usernames)
    {
        var clock = new FakeClock(Now);
        return new AuthService(
            new CaseInsensitiveUserRepo(Hash, usernames), new StubTenantRepo(), new SessionStore(), clock.Now);
    }
    private static async Task FailFiveAsync(AuthService auth, string typed)
    {
        for (var i = 0; i < AuthService.MaxFailedAttempts; i++)
        {
            await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
                () => auth.LoginAsync(new LoginRequest(typed, "wrong")));
        }
    }

    [DataTestMethod]
    [DataRow("Admin")]
    [DataRow("ADMIN")]
    [DataRow("aDmIn")]
    [DataRow(" admin ")]
    public async Task Lockout_SurvivesCasingAndWhitespaceVariants(string variant)
    {
        var auth = MakeService("admin");

        await FailFiveAsync(auth, "admin"); // khóa "admin"

        // Biến thể hoa/thường (hoặc thêm khoảng trắng) vẫn là cùng tài khoản → phải bị chặn,
        // dù mật khẩu ĐÚNG. Trước khi sửa, bộ đếm ordinal("admin") bị bỏ qua nên đăng nhập lọt.
        var ex = await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
            () => auth.LoginAsync(new LoginRequest(variant, "right")));
        StringAssert.Contains(ex.Message, "Tài khoản tạm khóa");
    }

    [TestMethod]
    public async Task FailedVariantsAccumulateOnNormalisedKey()
    {
        var auth = MakeService("admin");

        // 3 lần "admin" + 2 lần "ADMIN" đủ 5 → lần thứ 6 (đúng mật khẩu) bị khóa.
        for (var i = 0; i < 3; i++)
        {
            await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
                () => auth.LoginAsync(new LoginRequest("admin", "wrong")));
        }
        for (var i = 0; i < 2; i++)
        {
            await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
                () => auth.LoginAsync(new LoginRequest("ADMIN", "wrong")));
        }

        var ex = await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
            () => auth.LoginAsync(new LoginRequest("Admin", "wrong")));
        StringAssert.Contains(ex.Message, "Tài khoản tạm khóa");
    }

    [TestMethod]
    public async Task SuccessfulLoginResetsCounterRegardlessOfCasing()
    {
        var auth = MakeService("admin");

        for (var i = 0; i < AuthService.MaxFailedAttempts - 1; i++)
        {
            await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
                () => auth.LoginAsync(new LoginRequest("admin", "wrong")));
        }

        // Đăng nhập đúng bằng "Admin" (khác kiểu chữ) phải xóa bộ đếm của tài khoản.
        var ok = await auth.LoginAsync(new LoginRequest("Admin", "right"));
        Assert.AreEqual(UserRole.Landlord, ok.Role);

        var ex = await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(
            () => auth.LoginAsync(new LoginRequest("admin", "wrong")));
        Assert.AreEqual(AuthService.InvalidCredentialsMessage, ex.Message);
    }

    [TestMethod]
    public async Task DistinctUsernamesDoNotShareLockoutBucket()
    {
        var auth = MakeService("admin", "user");

        await FailFiveAsync(auth, "admin"); // khóa "admin"

        // "user" là tài khoản khác → không bị vạ lây; đăng nhập đúng phải thành công.
        var ok = await auth.LoginAsync(new LoginRequest("user", "right"));
        Assert.AreEqual(UserRole.Landlord, ok.Role);
    }
}

file sealed class FakeClock(DateTimeOffset start)
{
    private DateTimeOffset _now = start;

    public DateTimeOffset Now() => _now;

    public void Advance(TimeSpan delta) => _now = _now.Add(delta);
}

/// <summary>
/// So khớp username KHÔNG phân biệt hoa/thường — mô phỏng utf8mb4_unicode_ci của MySQL,
/// đúng như hành vi thật khiến "Admin" tìm thấy dòng "admin".
/// </summary>
file sealed class CaseInsensitiveUserRepo(string hash, params string[] usernames) : IUserRepository
{
    public Task<UserRecord?> FindByUsernameAsync(string username, CancellationToken ct = default) =>
        Task.FromResult(
            usernames.Any(u => string.Equals(u, username, StringComparison.OrdinalIgnoreCase))
                ? new UserRecord(1, hash, "A")
                : null);
}

file sealed class StubTenantRepo : ITenantRepository
{
    public Task<TenantAuthRecord?> FindByCccdAsync(string idCard, CancellationToken ct = default) =>
        Task.FromResult<TenantAuthRecord?>(null);
}
