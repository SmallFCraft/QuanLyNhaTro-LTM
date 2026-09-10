using MySqlConnector;
using QuanLyTro.Server.Data;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Repositories;

/// <summary>Hợp đồng kèm số phòng + tên đại diện (US-11), xếp theo end_date tăng dần.</summary>
public sealed record ContractListItem(ContractDto Contract, string RoomNumber, string RepresentativeName);

/// <summary>Biên truy cập bảng `contracts` — interface để ContractService test được không cần DB.</summary>
public interface IContractRepository
{
    Task<ContractDto?> GetByIdAsync(int contractId, CancellationToken ct = default);
    Task<bool> HasActiveContractAsync(int roomId, CancellationToken ct = default);
    Task<bool> IsTenantInRoomAsync(int tenantId, int roomId, CancellationToken ct = default);
    Task<ContractDto> AddAsync(ContractDto contract, CancellationToken ct = default);
    Task<bool> TerminateAsync(int contractId, string? notes, CancellationToken ct = default);
    Task<bool> UpdateEndDateAsync(int contractId, DateOnly newEndDate, CancellationToken ct = default);
    Task<List<ContractListItem>> GetAllAsync(CancellationToken ct = default);
}

/// <summary>CRUD hợp đồng. MySQL duplicate key chuyển thành BusinessRuleException.</summary>
public sealed class ContractRepository(Database database) : IContractRepository
{
    public async Task<ContractDto?> GetByIdAsync(int contractId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, room_id, representative_tenant_id, start_date, end_date,
                   rental_price, deposit_amount, status, notes
            FROM contracts
            WHERE id = @id
            LIMIT 1
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", contractId);

        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
        {
            return null;
        }

        return Map(reader);
    }

    /// <summary>BR-04: đếm hợp đồng `Active` của phòng.</summary>
    public async Task<bool> HasActiveContractAsync(int roomId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT COUNT(*)
            FROM contracts
            WHERE room_id = @roomId AND status = 'Active'
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@roomId", roomId);

        return Convert.ToInt64(await command.ExecuteScalarAsync(ct)) > 0;
    }

    /// <summary>BR-05: đại diện ký HĐ phải đang ở chính phòng đó.</summary>
    public async Task<bool> IsTenantInRoomAsync(int tenantId, int roomId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT COUNT(*)
            FROM tenants
            WHERE id = @tenantId AND room_id = @roomId
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@tenantId", tenantId);
        command.Parameters.AddWithValue("@roomId", roomId);

        return Convert.ToInt64(await command.ExecuteScalarAsync(ct)) > 0;
    }

    /// <summary>
    /// BR-04 + BR-05: khóa dòng phòng (SELECT ... FOR UPDATE) rồi mới kiểm tra và chèn — tất cả
    /// trong MỘT transaction. Hai client đồng thời không cùng lọt qua được khe kiểm tra.
    /// </summary>
    public async Task<ContractDto> AddAsync(ContractDto contract, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO contracts (room_id, representative_tenant_id, start_date, end_date,
                                   rental_price, deposit_amount, status, notes)
            VALUES (@roomId, @representativeTenantId, @startDate, @endDate,
                    @rentalPrice, @depositAmount, @status, @notes);
            SELECT LAST_INSERT_ID();
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);

        await EnsureContractCanBeAddedAsync(
            connection, transaction, contract.RoomId, contract.RepresentativeTenantId, ct);

        await using var command = new MySqlCommand(sql, connection, transaction);
        AddContractParameters(command, contract);
        var id = Convert.ToInt32(await command.ExecuteScalarAsync(ct));

        await transaction.CommitAsync(ct);
        return contract with { Id = id };
    }

    /// <summary>Chấm dứt hợp đồng: status = 'Terminated', ghi chú thêm lý do.</summary>
    public async Task<bool> TerminateAsync(int contractId, string? notes, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE contracts
            SET status = 'Terminated',
                notes = CONCAT_WS(' — ', NULLIF(notes, ''), @notes)
            WHERE id = @id AND status = 'Active'
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", contractId);
        command.Parameters.AddWithValue("@notes", (object?)notes ?? DBNull.Value);

        return await command.ExecuteNonQueryAsync(ct) > 0;
    }

    /// <summary>US-10: gia hạn — chỉ hợp đồng `Active`, mới chạm được dòng.</summary>
    public async Task<bool> UpdateEndDateAsync(int contractId, DateOnly newEndDate, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE contracts
            SET end_date = @endDate
            WHERE id = @id AND status = 'Active'
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@endDate", newEndDate);
        command.Parameters.AddWithValue("@id", contractId);

        return await command.ExecuteNonQueryAsync(ct) > 0;
    }

    public async Task<List<ContractListItem>> GetAllAsync(CancellationToken ct = default)
    {
        const string sql = """
            SELECT c.id, c.room_id, c.representative_tenant_id, c.start_date, c.end_date,
                   c.rental_price, c.deposit_amount, c.status, c.notes,
                   r.room_number, t.full_name
            FROM contracts c
            JOIN rooms r ON r.id = c.room_id
            JOIN tenants t ON t.id = c.representative_tenant_id
            ORDER BY c.end_date
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(ct);

        var contracts = new List<ContractListItem>();
        while (await reader.ReadAsync(ct))
        {
            contracts.Add(new ContractListItem(
                Map(reader),
                reader.GetString("room_number"),
                reader.GetString("full_name")));
        }

        return contracts;
    }

    private static ContractDto Map(MySqlDataReader reader) => new(
        reader.GetInt32("id"),
        reader.GetInt32("room_id"),
        reader.GetInt32("representative_tenant_id"),
        reader.GetDateOnly("start_date"),
        reader.GetDateOnly("end_date"),
        reader.GetDecimal("rental_price"),
        reader.GetDecimal("deposit_amount"),
        Enum.Parse<ContractStatus>(reader.GetString("status")),
        reader.IsDBNull(reader.GetOrdinal("notes")) ? null : reader.GetString("notes"));

    /// <summary>BR-04 + BR-05: kiểm tra sau khi đã khóa dòng phòng trong cùng transaction.</summary>
    private static async Task EnsureContractCanBeAddedAsync(
        MySqlConnection connection, MySqlTransaction transaction,
        int roomId, int representativeTenantId, CancellationToken ct)
    {
        const string lockRoomSql = "SELECT id FROM rooms WHERE id = @roomId FOR UPDATE";
        // FOR UPDATE = current read: đọc bản mới nhất đã commit, không dùng snapshot REPEATABLE READ.
        const string hasActiveContractSql = """
            SELECT COUNT(*)
            FROM contracts
            WHERE room_id = @roomId AND status = 'Active'
            FOR UPDATE
            """;
        const string tenantInRoomSql = """
            SELECT COUNT(*)
            FROM tenants
            WHERE id = @tenantId AND room_id = @roomId
            FOR UPDATE
            """;

        await using (var command = new MySqlCommand(lockRoomSql, connection, transaction))
        {
            command.Parameters.AddWithValue("@roomId", roomId);
            if (await command.ExecuteScalarAsync(ct) is null)
            {
                throw new Services.BusinessRuleException("Phòng không tồn tại.");
            }
        }

        await using (var command = new MySqlCommand(hasActiveContractSql, connection, transaction))
        {
            command.Parameters.AddWithValue("@roomId", roomId);
            if (Convert.ToInt64(await command.ExecuteScalarAsync(ct)) > 0)
            {
                throw new Services.BusinessRuleException("Phòng này đang có hợp đồng hiệu lực.");
            }
        }

        await using var tenantCommand = new MySqlCommand(tenantInRoomSql, connection, transaction);
        tenantCommand.Parameters.AddWithValue("@tenantId", representativeTenantId);
        tenantCommand.Parameters.AddWithValue("@roomId", roomId);
        if (Convert.ToInt64(await tenantCommand.ExecuteScalarAsync(ct)) == 0)
        {
            throw new Services.BusinessRuleException("Người đại diện phải là người đang ở trong phòng này.");
        }
    }

    private static void AddContractParameters(MySqlCommand command, ContractDto contract)
    {
        command.Parameters.AddWithValue("@roomId", contract.RoomId);
        command.Parameters.AddWithValue("@representativeTenantId", contract.RepresentativeTenantId);
        command.Parameters.AddWithValue("@startDate", contract.StartDate);
        command.Parameters.AddWithValue("@endDate", contract.EndDate);
        command.Parameters.AddWithValue("@rentalPrice", contract.RentalPrice);
        command.Parameters.AddWithValue("@depositAmount", contract.DepositAmount);
        command.Parameters.AddWithValue("@status", contract.Status.ToString());
        command.Parameters.AddWithValue("@notes", (object?)contract.Notes ?? DBNull.Value);
    }
}
