using MySqlConnector;
using QuanLyTro.Server.Data;

namespace QuanLyTro.Server.Repositories;

/// <summary>Tra cứu người thuê theo CCCD để đăng nhập (vai KhachThue).</summary>
public sealed class KhachThueAuthRepository(Database database) : IKhachThueRepository
{
    public async Task<KhachThueAuthRecord?> FindByCccdAsync(string cccd, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, cccd, mat_khau_hash, ho_ten, phong_id
            FROM khach_thue
            WHERE cccd = @cccd
              AND mat_khau_hash IS NOT NULL
            LIMIT 1
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@cccd", cccd);

        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
        {
            return null;
        }

        return new KhachThueAuthRecord(
            reader.GetInt32("id"),
            reader.GetString("cccd"),
            reader.GetString("mat_khau_hash"),
            reader.GetString("ho_ten"),
            reader.IsDBNull(reader.GetOrdinal("phong_id")) ? null : reader.GetInt32("phong_id"));
    }
}
