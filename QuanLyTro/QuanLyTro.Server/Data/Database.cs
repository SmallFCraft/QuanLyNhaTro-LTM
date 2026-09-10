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
}
