using MySqlConnector;
using QuanLyTro.Server.Data;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Repositories;

/// <summary>CRUD phòng. Mọi truy vấn parameter hóa; MySQL duplicate key chuyển thành BusinessRuleException.</summary>
public sealed class RoomRepository(Database database)
{
    private const int MySqlDuplicateKey = 1062;

    public async Task<List<RoomDto>> GetAllAsync(CancellationToken ct = default)
    {
        const string sql = """
            SELECT r.id, r.room_number, r.price, r.max_occupants, r.status, r.description,
                   COUNT(t.id) AS current_occupants
            FROM rooms r
            LEFT JOIN tenants t ON t.room_id = r.id
            GROUP BY r.id, r.room_number, r.price, r.max_occupants, r.status, r.description
            ORDER BY r.room_number
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(ct);

        var rooms = new List<RoomDto>();
        while (await reader.ReadAsync(ct))
        {
            rooms.Add(new RoomDto(
                reader.GetInt32("id"),
                reader.GetString("room_number"),
                reader.GetDecimal("price"),
                reader.GetInt32("max_occupants"),
                Enum.Parse<RoomStatus>(reader.GetString("status")),
                reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString("description"),
                reader.GetInt32("current_occupants")));
        }

        return rooms;
    }

    public async Task<RoomDto> AddAsync(RoomDto room, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO rooms (room_number, price, max_occupants, status, description)
            VALUES (@roomNumber, @price, @maxOccupants, @status, @description);
            SELECT LAST_INSERT_ID();
            """;

        try
        {
            await using var connection = await database.OpenAsync(ct);
            await using var command = new MySqlCommand(sql, connection);
            AddRoomParameters(command, room);

            var id = Convert.ToInt32(await command.ExecuteScalarAsync(ct));
            return room with { Id = id };
        }
        catch (MySqlException ex) when (ex.Number == MySqlDuplicateKey)
        {
            throw new Services.BusinessRuleException("Số phòng đã tồn tại.");
        }
    }

    public async Task<bool> UpdateAsync(RoomDto room, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE rooms
            SET room_number = @roomNumber,
                price = @price,
                max_occupants = @maxOccupants,
                status = @status,
                description = @description
            WHERE id = @id
            """;

        try
        {
            await using var connection = await database.OpenAsync(ct);
            await using var command = new MySqlCommand(sql, connection);
            AddRoomParameters(command, room);
            command.Parameters.AddWithValue("@id", room.Id);

            return await command.ExecuteNonQueryAsync(ct) > 0;
        }
        catch (MySqlException ex) when (ex.Number == MySqlDuplicateKey)
        {
            throw new Services.BusinessRuleException("Số phòng đã tồn tại.");
        }
    }

    public async Task<bool> DeleteAsync(int roomId, CancellationToken ct = default)
    {
        const string sql = "DELETE FROM rooms WHERE id = @id";

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", roomId);

        return await command.ExecuteNonQueryAsync(ct) > 0;
    }

    /// <summary>BR-12: chỉ xóa phòng khi không còn người thuê và không có hợp đồng Active.</summary>
    public async Task<bool> CanDeleteAsync(int roomId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                (SELECT COUNT(*) FROM tenants t WHERE t.room_id = @id) AS tenant_count,
                (SELECT COUNT(*) FROM contracts c WHERE c.room_id = @id AND c.status = 'Active') AS active_contracts
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", roomId);

        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
        {
            return false;
        }

        return reader.GetInt64("tenant_count") == 0 && reader.GetInt64("active_contracts") == 0;
    }

    private static void AddRoomParameters(MySqlCommand command, RoomDto room)
    {
        command.Parameters.AddWithValue("@roomNumber", room.RoomNumber);
        command.Parameters.AddWithValue("@price", room.Price);
        command.Parameters.AddWithValue("@maxOccupants", room.MaxOccupants);
        command.Parameters.AddWithValue("@status", room.Status.ToString());
        command.Parameters.AddWithValue("@description", (object?)room.Description ?? DBNull.Value);
    }
}
