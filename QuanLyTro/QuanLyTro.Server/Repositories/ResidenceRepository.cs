using MySqlConnector;
using QuanLyTro.Server.Data;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Repositories;

public sealed class ResidenceRepository(Database database)
{
    // ponytail: derive history from tenants + rooms because dedicated residence_history table is not in schema. Upgrade to dedicated audit table if event history tracking is added.
    public async Task<List<ResidenceHistoryDto>> GetHistoryAsync(
        DateTime from,
        DateTime to,
        string? roomNumber,
        CancellationToken ct = default)
    {
        const string sql = """
            SELECT t.id, t.full_name, t.id_card, COALESCE(r.room_number, '') AS room_number,
                   t.created_at, t.hometown
            FROM tenants t
            LEFT JOIN rooms r ON r.id = t.room_id
            WHERE t.created_at >= @from AND t.created_at <= @to
              AND (@roomNumber IS NULL OR r.room_number = @roomNumber)
            ORDER BY t.created_at DESC
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@from", from);
        command.Parameters.AddWithValue("@to", to);
        command.Parameters.AddWithValue("@roomNumber", (object?)roomNumber ?? DBNull.Value);

        await using var reader = await command.ExecuteReaderAsync(ct);
        var result = new List<ResidenceHistoryDto>();
        while (await reader.ReadAsync(ct))
        {
            result.Add(new ResidenceHistoryDto(
                reader.GetInt32("id"),
                reader.GetString("full_name"),
                reader.GetString("id_card"),
                reader.GetString("room_number"),
                "Vào",
                reader.GetDateTime("created_at"),
                reader.IsDBNull(reader.GetOrdinal("hometown")) ? null : reader.GetString("hometown")));
        }

        return result;
    }
}
