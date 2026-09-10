using QuanLyTro.Server.Repositories;
using QuanLyTro.Server.Security;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Services;

/// <summary>
/// Đăng nhập chỉ dành cho chủ trọ (vai Landlord). Người thuê phân vai ở AuthService hai bảng (Task 5).
/// </summary>
public sealed class AuthService(UserRepository users, SessionStore sessions)
{
    /// <summary>Thông báo sai thông tin chung — không tiết lộ tài khoản có tồn tại.</summary>
    public const string InvalidCredentialsMessage = "Tên đăng nhập hoặc mật khẩu không đúng.";

    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await users.FindByUsernameAsync(request.Username, ct);
        if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException(InvalidCredentialsMessage);
        }

        var token = sessions.Create(user.Id);
        return new LoginResult(token, user.FullName);
    }
}
