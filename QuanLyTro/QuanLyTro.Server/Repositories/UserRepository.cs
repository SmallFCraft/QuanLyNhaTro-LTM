using MySqlConnector;
using QuanLyTro.Server.Data;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Repositories;

/// <summary>Truy vấn tài khoản trong bảng `users` (chủ trọ hoặc công an phường).</summary>
public sealed class UserRepository(Database database) : IUserRepository
{
    public async Task<UserRecord?> FindByUsernameAsync(string username, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, password_hash, full_name, role
            FROM users
            WHERE username = @username
            LIMIT 1
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@username", username);

        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
        {
            return null;
        }

        return new UserRecord(
            reader.GetInt32("id"),
            reader.GetString("password_hash"),
            reader.GetString("full_name"),
            ParseRole(reader.IsDBNull(reader.GetOrdinal("role")) ? null : reader.GetString("role")));
    }

    /// <summary>Cột `role` là ENUM chuỗi; giá trị lạ (DB cũ) coi như Landlord để không chặn đăng nhập.</summary>
    private static UserRole ParseRole(string? raw) =>
        Enum.TryParse<UserRole>(raw, ignoreCase: true, out var role) ? role : UserRole.Landlord;
}
