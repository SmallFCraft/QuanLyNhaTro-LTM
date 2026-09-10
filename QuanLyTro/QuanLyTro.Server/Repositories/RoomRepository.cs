using MySqlConnector;
using QuanLyTro.Server.Data;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Repositories;

/// <summary>CRUD phòng. Mọi truy vấn parameter hóa; MySQL duplicate key chuyển thành BusinessRuleException.</summary>
public sealed class RoomRepository(Database database)
{
    private const int MySqlDuplicateKey = 1062;
    private const int MySqlRowIsReferenced = 1451;

    /// <summary>BR-12 — thông báo dùng chung cho cả đường kiểm tra lẫn khi MySQL chặn khóa ngoại.</summary>
    public const string OccupiedMessage =
        "Không thể xóa: phòng còn người thuê hoặc hợp đồng đang hiệu lực.";

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

    /// <summary>
    /// BR-12: khóa dòng phòng, kiểm tra người thuê/hợp đồng, rồi xóa trong cùng transaction.
    /// Khóa này tuần tự hóa với TenantRepository.AddAsync và ContractRepository.AddAsync.
    /// </summary>
    public async Task<bool> DeleteAsync(int roomId, CancellationToken ct = default)
    {
        const string lockRoomSql = "SELECT id FROM rooms WHERE id = @id FOR UPDATE";
        const string occupancySql = """
            SELECT
                (SELECT COUNT(*) FROM tenants WHERE room_id = @id) AS tenant_count,
                (SELECT COUNT(*) FROM contracts WHERE room_id = @id AND status = 'Active') AS active_contracts
            """;
        const string deleteSql = "DELETE FROM rooms WHERE id = @id";

        try
        {
            await using var connection = await database.OpenAsync(ct);
            await using var transaction = await connection.BeginTransactionAsync(ct);

            await using (var lockCommand = new MySqlCommand(lockRoomSql, connection, transaction))
            {
                lockCommand.Parameters.AddWithValue("@id", roomId);
                if (await lockCommand.ExecuteScalarAsync(ct) is null)
                {
                    return false;
                }
            }

            await using (var occupancyCommand = new MySqlCommand(occupancySql, connection, transaction))
            {
                occupancyCommand.Parameters.AddWithValue("@id", roomId);
                await using var reader = await occupancyCommand.ExecuteReaderAsync(ct);
                await reader.ReadAsync(ct);
                if (reader.GetInt64("tenant_count") > 0 || reader.GetInt64("active_contracts") > 0)
                {
                    throw new Services.BusinessRuleException(OccupiedMessage);
                }
            }

            await using var deleteCommand = new MySqlCommand(deleteSql, connection, transaction);
            deleteCommand.Parameters.AddWithValue("@id", roomId);
            var deleted = await deleteCommand.ExecuteNonQueryAsync(ct) > 0;
            await transaction.CommitAsync(ct);
            return deleted;
        }
        catch (MySqlException ex) when (ex.Number == MySqlRowIsReferenced)
        {
            throw new Services.BusinessRuleException(OccupiedMessage);
        }
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
