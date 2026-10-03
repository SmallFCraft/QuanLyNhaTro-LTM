using System.Reflection;
using MySqlConnector;

namespace QuanLyTro.Server.Data;

/// <summary>
/// Đọc embedded resource `database/schema.sql`, tách theo marker "-- statement"
/// và chạy tuần tự. Idempotent: CREATE TABLE IF NOT EXISTS nên chạy lại vẫn thành công.
/// </summary>
public sealed class KhoiTaoSchema(Database database)
{
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        var script = ReadEmbeddedSchema();

        await using var connection = await database.OpenServerLevelAsync(ct);
        foreach (var statement in SplitStatements(script))
        {
            await using var command = connection.CreateCommand();
            command.CommandText = statement;
            await command.ExecuteNonQueryAsync(ct);
        }

        await EnsureMigrationColumnsAsync(connection, ct);
    }

    private static async Task EnsureMigrationColumnsAsync(MySqlConnection connection, CancellationToken ct)
    {
        const string checkSql = """
            SELECT COUNT(*)
            FROM information_schema.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
              AND TABLE_NAME = 'chi_so_dien_nuoc'
              AND COLUMN_NAME = 'hinh_thuc_nuoc';
            """;
        await using var checkCmd = connection.CreateCommand();
        checkCmd.CommandText = checkSql;
        var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync(ct));
        if (count == 0)
        {
            const string alterSql = """
                ALTER TABLE chi_so_dien_nuoc
                ADD COLUMN hinh_thuc_nuoc VARCHAR(10) NOT NULL DEFAULT 'Khoi',
                ADD COLUMN so_nguoi_nuoc INT NOT NULL DEFAULT 0;
                """;
            await using var alterCmd = connection.CreateCommand();
            alterCmd.CommandText = alterSql;
            await alterCmd.ExecuteNonQueryAsync(ct);
        }
    }

    public static string ReadEmbeddedSchema()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .Single(n => n.EndsWith("schema.sql", StringComparison.OrdinalIgnoreCase));

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource không tìm thấy: {resourceName}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Tách script thành từng statement. Chỉ nhận marker nằm trên DÒNG RIÊNG — marker trong
    /// comment mô tả (ví dụ chính dòng mô tả trong schema.sql) không được tính.
    /// </summary>
    public static IEnumerable<string> SplitStatements(string script)
    {
        var statements = new List<string>();
        var buffer = new List<string>();

        foreach (var line in script.Split('\n'))
        {
            if (line.Trim() == "-- statement")
            {
                Flush(statements, buffer);
                continue;
            }

            buffer.Add(line);
        }

        Flush(statements, buffer);
        return statements;
    }

    private static void Flush(List<string> statements, List<string> buffer)
    {
        var text = string.Join('\n', buffer).Trim();
        buffer.Clear();

        if (text.Length > 0)
        {
            statements.Add(text);
        }
    }
}
