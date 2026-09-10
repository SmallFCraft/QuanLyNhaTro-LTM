using MySqlConnector;
using QuanLyTro.Server.Data;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Repositories;

/// <summary>Kết quả xóa hồ sơ người thuê (US-06) — tách "không tồn tại" khỏi "còn đang ở".</summary>
public enum TenantDeleteResult
{
    Deleted,
    StillInRoom,
    NotFound,
}

/// <summary>CRUD người thuê. Mọi truy vấn parameter hóa; thêm mới khóa phòng bằng SELECT ... FOR UPDATE (BR-02).</summary>
public sealed class TenantRepository(Database database)
{
    private const int MySqlDuplicateKey = 1062;

    private const string TenantColumns =
        "id, room_id, full_name, dob, id_card, phone, hometown, workplace, is_temporary_registered";

    public async Task<List<TenantDto>> GetByRoomAsync(int roomId, CancellationToken ct = default)
    {
        const string sql = $"""
            SELECT {TenantColumns}
            FROM tenants
            WHERE room_id = @roomId
            ORDER BY full_name
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@roomId", roomId);

        await using var reader = await command.ExecuteReaderAsync(ct);
        var tenants = new List<TenantDto>();
        while (await reader.ReadAsync(ct))
        {
            tenants.Add(ReadTenant(reader));
        }

        return tenants;
    }

    /// <summary>BR-02: khóa phòng bằng SELECT ... FOR UPDATE để đếm sức chứa không bị race hai tác nhân.</summary>
    public async Task<TenantDto> AddAsync(TenantDto tenant, string passwordHash, CancellationToken ct = default)
    {
        const string sql = $"""
            INSERT INTO tenants
                (room_id, full_name, dob, id_card, password_hash, phone, hometown, workplace, is_temporary_registered)
            VALUES
                (@roomId, @fullName, @dob, @idCard, @passwordHash, @phone, @hometown, @workplace, @isTemporaryRegistered);
            SELECT LAST_INSERT_ID();
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);

        if (tenant.RoomId is { } roomId)
        {
            await EnsureRoomHasSpaceAsync(connection, transaction, roomId, ct);
        }

        int id;
        try
        {
            await using var command = new MySqlCommand(sql, connection, transaction);
            AddTenantParameters(command, tenant);
            command.Parameters.AddWithValue("@passwordHash", passwordHash);
            id = Convert.ToInt32(await command.ExecuteScalarAsync(ct));
        }
        catch (MySqlException ex) when (ex.Number == MySqlDuplicateKey)
        {
            throw new Services.BusinessRuleException("Số CCCD đã tồn tại trong hệ thống.");
        }

        if (tenant.RoomId is { } occupyRoomId)
        {
            await SetRoomStatusAsync(connection, transaction, occupyRoomId, RoomStatus.Rented, ct);
        }

        await transaction.CommitAsync(ct);
        return tenant with { Id = id };
    }

    /// <summary>Đổi hồ sơ, có thể gán sang phòng khác — BR-02 áp dụng cả ở đường này, không chỉ lúc thêm.</summary>
    public async Task<bool> UpdateAsync(TenantDto tenant, string? passwordHash, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE tenants
            SET room_id = @roomId,
                full_name = @fullName,
                dob = @dob,
                id_card = @idCard,
                phone = @phone,
                hometown = @hometown,
                workplace = @workplace,
                is_temporary_registered = @isTemporaryRegistered
            WHERE id = @id
            """;
        const string passwordSql = "UPDATE tenants SET password_hash = @passwordHash WHERE id = @id";

        await using var connection = await database.OpenAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);

        var previousRoomId = await GetRoomIdAsync(connection, transaction, tenant.Id, ct);
        var roomChanged = previousRoomId != tenant.RoomId;

        if (roomChanged && tenant.RoomId is { } newRoomId)
        {
            await EnsureRoomHasSpaceAsync(connection, transaction, newRoomId, ct);
        }

        try
        {
            await using var command = new MySqlCommand(sql, connection, transaction);
            AddTenantParameters(command, tenant);
            command.Parameters.AddWithValue("@id", tenant.Id);

            if (await command.ExecuteNonQueryAsync(ct) == 0)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(passwordHash))
            {
                await using var passwordCommand = new MySqlCommand(passwordSql, connection, transaction);
                passwordCommand.Parameters.AddWithValue("@passwordHash", passwordHash);
                passwordCommand.Parameters.AddWithValue("@id", tenant.Id);
                await passwordCommand.ExecuteNonQueryAsync(ct);
            }
        }
        catch (MySqlException ex) when (ex.Number == MySqlDuplicateKey)
        {
            throw new Services.BusinessRuleException("Số CCCD đã tồn tại trong hệ thống.");
        }

        if (roomChanged)
        {
            if (previousRoomId is { } vacatedRoomId)
            {
                await FreeRoomIfEmptyAsync(connection, transaction, vacatedRoomId, ct);
            }

            if (tenant.RoomId is { } occupiedRoomId)
            {
                await SetRoomStatusAsync(connection, transaction, occupiedRoomId, RoomStatus.Rented, ct);
            }
        }

        await transaction.CommitAsync(ct);
        return true;
    }

    /// <summary>Gán room_id = NULL (đã trả phòng) và trả phòng về Available nếu đã trống hết.</summary>
    public async Task<bool> CheckoutAsync(int tenantId, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE tenants
            SET room_id = NULL
            WHERE id = @id AND room_id IS NOT NULL
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);

        var roomId = await GetRoomIdAsync(connection, transaction, tenantId, ct);

        await using (var command = new MySqlCommand(sql, connection, transaction))
        {
            command.Parameters.AddWithValue("@id", tenantId);
            if (await command.ExecuteNonQueryAsync(ct) == 0)
            {
                return false;
            }
        }

        if (roomId is { } freedRoomId)
        {
            await FreeRoomIfEmptyAsync(connection, transaction, freedRoomId, ct);
        }

        await transaction.CommitAsync(ct);
        return true;
    }

    /// <summary>US-06: chỉ xóa hồ sơ đã trả phòng. Phân biệt rõ "không có" và "còn đang ở".</summary>
    public async Task<TenantDeleteResult> DeleteAsync(int tenantId, CancellationToken ct = default)
    {
        const string sql = """
            DELETE FROM tenants WHERE id = @id AND room_id IS NULL;
            SELECT ROW_COUNT();
            """;
        const string existsSql = "SELECT COUNT(*) FROM tenants WHERE id = @id";

        await using var connection = await database.OpenAsync(ct);
        await using (var command = new MySqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@id", tenantId);
            if (Convert.ToInt32(await command.ExecuteScalarAsync(ct)) > 0)
            {
                return TenantDeleteResult.Deleted;
            }
        }

        await using var existsCommand = new MySqlCommand(existsSql, connection);
        existsCommand.Parameters.AddWithValue("@id", tenantId);
        return Convert.ToInt32(await existsCommand.ExecuteScalarAsync(ct)) > 0
            ? TenantDeleteResult.StillInRoom
            : TenantDeleteResult.NotFound;
    }

    /// <summary>Id phòng hiện tại của người thuê; null nghĩa là không tồn tại HOẶC đã trả phòng.</summary>
    public async Task<int?> GetRoomIdAsync(int tenantId, CancellationToken ct = default)
    {
        await using var connection = await database.OpenAsync(ct);
        return await GetRoomIdAsync(connection, null, tenantId, ct);
    }

    public async Task<bool> RoomExistsAsync(int roomId, CancellationToken ct = default)
    {
        const string sql = "SELECT COUNT(*) FROM rooms WHERE id = @id";

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", roomId);
        return Convert.ToInt32(await command.ExecuteScalarAsync(ct)) > 0;
    }

    private static async Task<int?> GetRoomIdAsync(
        MySqlConnection connection, MySqlTransaction? transaction, int tenantId, CancellationToken ct)
    {
        const string sql = "SELECT room_id FROM tenants WHERE id = @id";

        await using var command = new MySqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@id", tenantId);

        var value = await command.ExecuteScalarAsync(ct);
        return value is null or DBNull ? null : Convert.ToInt32(value);
    }

    /// <summary>Phòng về Available khi không còn người thuê và không còn hợp đồng Active.</summary>
    private static async Task FreeRoomIfEmptyAsync(
        MySqlConnection connection, MySqlTransaction transaction, int roomId, CancellationToken ct)
    {
        const string sql = """
            SELECT
                (SELECT COUNT(*) FROM tenants t WHERE t.room_id = @id) AS tenant_count,
                (SELECT COUNT(*) FROM contracts c WHERE c.room_id = @id AND c.status = 'Active') AS active_contracts
            """;

        await using var command = new MySqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@id", roomId);

        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
        {
            return;
        }

        var empty = reader.GetInt64("tenant_count") == 0 && reader.GetInt64("active_contracts") == 0;
        await reader.CloseAsync();

        if (empty)
        {
            await SetRoomStatusAsync(connection, transaction, roomId, RoomStatus.Available, ct);
        }
    }

    /// <summary>Không ghi đè phòng đang bảo trì.</summary>
    private static async Task SetRoomStatusAsync(
        MySqlConnection connection, MySqlTransaction? transaction, int roomId, RoomStatus status, CancellationToken ct)
    {
        const string sql = "UPDATE rooms SET status = @status WHERE id = @id AND status <> 'Maintenance'";

        await using var command = new MySqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@status", status.ToString());
        command.Parameters.AddWithValue("@id", roomId);
        await command.ExecuteNonQueryAsync(ct);
    }

    /// <summary>BR-02: đếm người hiện tại sau khi khóa phòng; ném BusinessRuleException nếu đã đủ sức chứa.</summary>
    private static async Task EnsureRoomHasSpaceAsync(
        MySqlConnection connection, MySqlTransaction transaction, int roomId, CancellationToken ct)
    {
        const string sql = """
            SELECT r.max_occupants,
                   (SELECT COUNT(*) FROM tenants t WHERE t.room_id = r.id) AS current_occupants
            FROM rooms r
            WHERE r.id = @id
            FOR UPDATE
            """;

        await using var command = new MySqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@id", roomId);

        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
        {
            throw new Services.BusinessRuleException("Phòng không tồn tại.");
        }

        var maxOccupants = reader.GetInt32("max_occupants");
        var currentOccupants = Convert.ToInt32(reader.GetInt64("current_occupants"));
        if (currentOccupants >= maxOccupants)
        {
            throw new Services.BusinessRuleException("Phòng đã đủ sức chứa.");
        }
    }

    private static void AddTenantParameters(MySqlCommand command, TenantDto tenant)
    {
        command.Parameters.AddWithValue("@roomId", (object?)tenant.RoomId ?? DBNull.Value);
        command.Parameters.AddWithValue("@fullName", tenant.FullName);
        command.Parameters.AddWithValue("@dob", tenant.DateOfBirth.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@idCard", tenant.IdCard);
        command.Parameters.AddWithValue("@phone", tenant.Phone);
        command.Parameters.AddWithValue("@hometown", tenant.Hometown);
        command.Parameters.AddWithValue("@workplace", (object?)tenant.Workplace ?? DBNull.Value);
        command.Parameters.AddWithValue("@isTemporaryRegistered", tenant.IsTemporaryRegistered);
    }

    private static TenantDto ReadTenant(MySqlDataReader reader) =>
        new(
            reader.GetInt32("id"),
            reader.IsDBNull(reader.GetOrdinal("room_id")) ? null : reader.GetInt32("room_id"),
            reader.GetString("full_name"),
            DateOnly.FromDateTime(reader.GetDateTime("dob")),
            reader.GetString("id_card"),
            reader.GetString("phone"),
            reader.GetString("hometown"),
            reader.IsDBNull(reader.GetOrdinal("workplace")) ? null : reader.GetString("workplace"),
            reader.GetBoolean("is_temporary_registered"));
}
