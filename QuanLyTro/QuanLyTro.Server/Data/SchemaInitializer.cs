using System.Reflection;

namespace QuanLyTro.Server.Data;

/// <summary>
/// Đọc embedded resource `database/schema.sql`, tách theo marker "-- statement"
/// và chạy tuần tự. Idempotent: CREATE TABLE IF NOT EXISTS nên chạy lại vẫn thành công.
/// </summary>
public sealed class SchemaInitializer(Database database)
{
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        var script = ReadEmbeddedSchema();

        await using var connection = await database.OpenAsync(ct);
        foreach (var statement in SplitStatements(script))
        {
            await using var command = connection.CreateCommand();
            command.CommandText = statement;
            await command.ExecuteNonQueryAsync(ct);
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

    public static IEnumerable<string> SplitStatements(string script)
    {
        var statements = script
            .Split(["-- statement"], StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length > 0);

        foreach (var statement in statements)
        {
            yield return statement;
        }
    }
}
