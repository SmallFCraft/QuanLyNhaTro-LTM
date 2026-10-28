using QuanLyTro.Server.Data;

namespace QuanLyTro.Tests;

/// <summary>
/// DB riêng cho test — KHÔNG BAO GIỜ dùng `quanly_phongtro_nhs`.
///
/// Trước đây mọi test trỏ thẳng vào DB thật, nên một test ghi `api_token` giả đã ghi đè token
/// SePay của người dùng. Đổi sang DB `_test` và cho <see cref="KhoiTaoSchema"/> đổi tên DB trong
/// schema.sql theo connection string là cách duy nhất để test không chạm dữ liệu thật.
/// </summary>
internal static class TestDatabase
{
    public const string Ten = "quanly_phongtro_nhs_test";

    public static readonly string ConnectionString =
        $"Server=127.0.0.1;Port=3306;Database={Ten};User Id=root;Password=;SslMode=None;";

    /// <summary>Database trỏ vào DB test — dùng thay cho `new Database(chuỗi thật)`.</summary>
    public static Database Create() => new(ConnectionString);
}
