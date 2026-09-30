using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Repositories;

/// <summary>Bản ghi tài khoản người dùng đọc từ bảng `users` (Chủ trọ hoặc Công an).</summary>
public sealed record UserRecord(int Id, string PasswordHash, string FullName, UserRole Role = UserRole.Landlord);

/// <summary>Bản ghi người thuê đọc từ bảng `tenants` (đăng nhập bằng CCCD).</summary>
public sealed record TenantAuthRecord(int Id, string IdCard, string PasswordHash, string FullName, int? RoomId);

/// <summary>Biên truy cập bảng `users` — interface để AuthService test được không cần DB.</summary>
public interface IUserRepository
{
    Task<UserRecord?> FindByUsernameAsync(string username, CancellationToken ct = default);
}

/// <summary>Biên truy cập bảng `tenants` — interface để AuthService test được không cần DB.</summary>
public interface ITenantRepository
{
    Task<TenantAuthRecord?> FindByCccdAsync(string idCard, CancellationToken ct = default);
}
