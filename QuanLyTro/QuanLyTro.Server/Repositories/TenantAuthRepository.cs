using MySqlConnector;
using QuanLyTro.Server.Data;

namespace QuanLyTro.Server.Repositories;

/// <summary>Tra cứu người thuê theo CCCD để đăng nhập (vai Tenant).</summary>
public sealed class TenantAuthRepository(Database database) : ITenantRepository
{
    public async Task<TenantAuthRecord?> FindByCccdAsync(string idCard, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, id_card, password_hash, full_name, room_id
            FROM tenants
            WHERE id_card = @idCard
              AND password_hash IS NOT NULL
            LIMIT 1
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@idCard", idCard);

        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
        {
            return null;
        }

        return new TenantAuthRecord(
            reader.GetInt32("id"),
            reader.GetString("id_card"),
            reader.GetString("password_hash"),
            reader.GetString("full_name"),
            reader.IsDBNull(reader.GetOrdinal("room_id")) ? null : reader.GetInt32("room_id"));
    }
}
