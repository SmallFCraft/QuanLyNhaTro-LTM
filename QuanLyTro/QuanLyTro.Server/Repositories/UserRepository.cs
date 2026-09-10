using MySqlConnector;
using QuanLyTro.Server.Data;

namespace QuanLyTro.Server.Repositories;

/// <summary>Truy vấn tài khoản quản trị (chủ trọ) trong bảng `users`.</summary>
public sealed class UserRepository(Database database)
{
    public async Task<UserRecord?> FindByUsernameAsync(string username, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, password_hash, full_name
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
            reader.GetString("full_name"));
    }
}

/// <summary>Bản ghi tài khoản chủ trọ đọc từ DB.</summary>
public sealed record UserRecord(int Id, string PasswordHash, string FullName);
