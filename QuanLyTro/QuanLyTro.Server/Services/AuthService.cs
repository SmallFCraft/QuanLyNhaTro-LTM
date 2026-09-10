using System.Collections.Concurrent;
using QuanLyTro.Server.Repositories;
using QuanLyTro.Server.Security;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Services;

/// <summary>
/// Đăng nhập 2 tác nhân: thử bảng `users` (Landlord) trước, không khớp thì thử bảng `tenants`
/// theo `username = id_card` (Tenant). Sai thông tin trả CÙNG MỘT thông báo — không tiết lộ
/// bảng nào tồn tại. Khóa tài khoản theo username: quá 5 lần sai liên tiếp → từ chối 1 phút.
/// </summary>
public sealed class AuthService(IUserRepository users, ITenantRepository tenants, SessionStore sessions) : IAuthService
{
    public const int MaxFailedAttempts = 5;
    public static readonly TimeSpan LockoutWindow = TimeSpan.FromMinutes(1);
    public const string InvalidCredentialsMessage = "Tên đăng nhập hoặc mật khẩu không đúng.";

    private readonly ConcurrentDictionary<string, (int Count, DateTimeOffset LockedUntil)> _failures = new();
    private readonly Func<DateTimeOffset> _clock;

    public AuthService(IUserRepository users, ITenantRepository tenants, SessionStore sessions, Func<DateTimeOffset>? clock = null)
        : this(users, tenants, sessions)
    {
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var username = request.Username.Trim();

        if (_failures.TryGetValue(username, out var state) && state.LockedUntil > _clock())
        {
            var remaining = (int)Math.Ceiling((state.LockedUntil - _clock()).TotalSeconds);
            throw new UnauthorizedAccessException($"Tài khoản tạm khóa. Thử lại sau {remaining} giây.");
        }

        var user = await users.FindByUsernameAsync(username, ct);
        if (user is not null && PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            _failures.TryRemove(username, out _);
            var token = sessions.Create(user.Id, UserRole.Landlord);
            return new LoginResult(token, user.FullName, UserRole.Landlord);
        }

        var tenant = await tenants.FindByCccdAsync(username, ct);
        if (tenant is not null && tenant.PasswordHash is not null && PasswordHasher.Verify(request.Password, tenant.PasswordHash))
        {
            _failures.TryRemove(username, out _);
            var token = sessions.Create(tenant.Id, UserRole.Tenant);
            return new LoginResult(token, tenant.FullName, UserRole.Tenant);
        }

        RegisterFailure(username);
        throw new UnauthorizedAccessException(InvalidCredentialsMessage);
    }

    private void RegisterFailure(string username)
    {
        _failures.AddOrUpdate(
            username,
            _ => (1, DateTimeOffset.MinValue),
            (_, existing) =>
            {
                var count = existing.Count + 1;
                var lockedUntil = count >= MaxFailedAttempts ? _clock().Add(LockoutWindow) : existing.LockedUntil;
                return (count, lockedUntil);
            });
    }
}

/// <summary>Biên AuthService — cho phép controller/router test không phụ thuộc DB.</summary>
public interface IAuthService
{
    Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken ct = default);
}
