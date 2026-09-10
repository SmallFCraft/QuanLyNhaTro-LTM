using MySqlConnector;

namespace QuanLyTro.Server.Data;

/// <summary>Connection factory — mọi kết nối MySQL đi qua đây.</summary>
public sealed class Database(string connectionString)
{
    public string ConnectionString { get; } = connectionString;

    public async Task<MySqlConnection> OpenAsync(CancellationToken ct = default)
    {
        var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync(ct);
        return connection;
    }

    /// <summary>
    /// Mở kết nối cấp máy chủ (không chọn Database) — dùng riêng cho SchemaInitializer
    /// để chạy `CREATE DATABASE IF NOT EXISTS`.
    /// </summary>
    public async Task<MySqlConnection> OpenServerLevelAsync(CancellationToken ct = default)
    {
        var builder = new MySqlConnectionStringBuilder(ConnectionString)
        {
            Database = string.Empty,
        };
        var connection = new MySqlConnection(builder.ConnectionString);
        await connection.OpenAsync(ct);
        return connection;
    }
}
