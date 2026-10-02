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
    /// Mở kết nối cấp máy chủ (không chọn Database) — dùng riêng cho KhoiTaoSchema
    /// để chạy `CREATE DATABASE IF NOT EXISTS`.
    /// Bật AllowUserVariables vì script schema dùng biến `@role_col`/`@sql` cho ALTER
    /// có điều kiện; MySqlConnector mặc định coi mọi `@tên` là tham số command.
    /// </summary>
    public async Task<MySqlConnection> OpenServerLevelAsync(CancellationToken ct = default)
    {
        var builder = new MySqlConnectionStringBuilder(ConnectionString)
        {
            Database = string.Empty,
            AllowUserVariables = true,
        };
        var connection = new MySqlConnection(builder.ConnectionString);
        await connection.OpenAsync(ct);
        return connection;
    }

    /// <summary>
    /// Đổi mã lỗi MySQL sang thông báo tiếng Việt cho Client. Lỗi kết nối phải nói rõ là do
    /// MySQL chưa chạy — "Lỗi hệ thống" chung khiến người dùng bấm thử lại vô ích.
    /// Nhận số nguyên thay vì <see cref="MySqlException"/> để test không cần dựng exception.
    /// </summary>
    public static string DescribeFailure(int number, string message) =>
        number is 1042 or 1044 or 1045 or 1053 or 1129 or 1130 or 2002 or 2003
            || message.Contains("Unable to connect", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Connection refused", StringComparison.OrdinalIgnoreCase)
            ? "Không kết nối được MySQL. Bật Laragon (MySQL, cổng 3306) rồi thử lại."
            : "Lỗi cơ sở dữ liệu, vui lòng thử lại.";
}
