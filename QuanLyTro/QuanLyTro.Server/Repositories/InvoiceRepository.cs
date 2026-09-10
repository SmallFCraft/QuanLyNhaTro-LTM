using MySqlConnector;
using QuanLyTro.Server.Data;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Repositories;

/// <summary>Biên truy cập bảng `invoices` — interface để InvoiceService test không cần DB.</summary>
public interface IInvoiceRepository
{
    Task<ContractDto?> GetActiveContractAsync(int roomId, CancellationToken ct = default);
    Task<UtilityReadingDto?> GetUtilityReadingAsync(string billingMonth, int roomId, CancellationToken ct = default);
    Task<InvoiceDto> AddAsync(InvoiceDto invoice, CancellationToken ct = default);
    Task<InvoiceDto?> GetByIdAsync(int invoiceId, CancellationToken ct = default);
    Task<bool> MarkPaidAsync(int invoiceId, CancellationToken ct = default);
    Task<List<InvoiceDto>> GetAllAsync(string billingMonth, int? roomId, CancellationToken ct = default);
    Task<List<InvoiceDto>> GetByTenantAsync(int tenantId, CancellationToken ct = default);
}

/// <summary>
/// Hóa đơn tháng. MySQL 1062 (uq_invoice_room_month) → BusinessRuleException.
/// Tra hợp đồng Active và chỉ số điện nước ngay trong transaction của BR-13.
/// </summary>
public sealed class InvoiceRepository(Database database) : IInvoiceRepository
{
    private const int MySqlDuplicateKey = 1062;

    private const string InvoiceColumns =
        "id, room_id, contract_id, billing_month, room_amount, electricity_amount, " +
        "water_amount, other_fees, total_amount, status, paid_at";

    /// <summary>BR-04/BR-09: hợp đồng `Active` mới nhất của phòng.</summary>
    public async Task<ContractDto?> GetActiveContractAsync(int roomId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, room_id, representative_tenant_id, start_date, end_date,
                   rental_price, deposit_amount, status, notes
            FROM contracts
            WHERE room_id = @roomId AND status = 'Active'
            ORDER BY start_date DESC
            LIMIT 1
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@roomId", roomId);

        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
        {
            return null;
        }

        return new ContractDto(
            reader.GetInt32("id"),
            reader.GetInt32("room_id"),
            reader.GetInt32("representative_tenant_id"),
            reader.GetDateOnly("start_date"),
            reader.GetDateOnly("end_date"),
            reader.GetDecimal("rental_price"),
            reader.GetDecimal("deposit_amount"),
            Enum.Parse<ContractStatus>(reader.GetString("status")),
            reader.IsDBNull(reader.GetOrdinal("notes")) ? null : reader.GetString("notes"));
    }

    /// <summary>BR-09: chỉ số điện nước đã chốt của phòng trong tháng.</summary>
    public async Task<UtilityReadingDto?> GetUtilityReadingAsync(
        string billingMonth, int roomId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, room_id, billing_month, old_electricity, new_electricity, electricity_rate,
                   old_water, new_water, water_rate
            FROM utility_readings
            WHERE room_id = @roomId AND billing_month = @billingMonth
            LIMIT 1
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@roomId", roomId);
        command.Parameters.AddWithValue("@billingMonth", billingMonth);

        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
        {
            return null;
        }

        return new UtilityReadingDto(
            reader.GetInt32("id"),
            reader.GetInt32("room_id"),
            reader.GetString("billing_month"),
            reader.GetInt32("old_electricity"),
            reader.GetInt32("new_electricity"),
            reader.GetDecimal("electricity_rate"),
            reader.GetInt32("old_water"),
            reader.GetInt32("new_water"),
            reader.GetDecimal("water_rate"));
    }

    /// <summary>BR-13: insert nằm trong transaction của chính nó.</summary>
    // ponytail: không khóa phòng trước khi insert — uq_invoice_room_month đã chặn trùng ở tầng DB.
    // Thêm SELECT ... FOR UPDATE nếu sau này BR-09 đổi thành ghi thêm bảng khác trong cùng thao tác.
    public async Task<InvoiceDto> AddAsync(InvoiceDto invoice, CancellationToken ct = default)
    {
        const string insertSql = """
            INSERT INTO invoices (room_id, contract_id, billing_month, room_amount, electricity_amount,
                                  water_amount, other_fees, total_amount, status, paid_at)
            VALUES (@roomId, @contractId, @billingMonth, @roomAmount, @electricityAmount,
                    @waterAmount, @otherFees, @totalAmount, @status, @paidAt);
            SELECT LAST_INSERT_ID();
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);

        int id;
        try
        {
            await using var command = new MySqlCommand(insertSql, connection, transaction);
            command.Parameters.AddWithValue("@roomId", invoice.RoomId);
            command.Parameters.AddWithValue("@contractId", invoice.ContractId);
            command.Parameters.AddWithValue("@billingMonth", invoice.BillingMonth);
            command.Parameters.AddWithValue("@roomAmount", invoice.RoomAmount);
            command.Parameters.AddWithValue("@electricityAmount", invoice.ElectricityAmount);
            command.Parameters.AddWithValue("@waterAmount", invoice.WaterAmount);
            command.Parameters.AddWithValue("@otherFees", invoice.OtherFees);
            command.Parameters.AddWithValue("@totalAmount", invoice.TotalAmount);
            command.Parameters.AddWithValue("@status", invoice.Status.ToString());
            command.Parameters.AddWithValue("@paidAt", (object?)invoice.PaidAt ?? DBNull.Value);

            id = Convert.ToInt32(await command.ExecuteScalarAsync(ct));
        }
        catch (MySqlException ex) when (ex.Number == MySqlDuplicateKey)
        {
            throw new Services.BusinessRuleException("Tháng này đã lập hóa đơn cho phòng.");
        }

        await transaction.CommitAsync(ct);
        return invoice with { Id = id };
    }

    public async Task<InvoiceDto?> GetByIdAsync(int invoiceId, CancellationToken ct = default)
    {
        var sql = $"SELECT {InvoiceColumns} FROM invoices WHERE id = @id LIMIT 1";

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", invoiceId);

        await using var reader = await command.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    /// <summary>BR-11: điều kiện `status = 'Unpaid'` nằm trong WHERE — hóa đơn Paid bất biến ở tầng DB.</summary>
    public async Task<bool> MarkPaidAsync(int invoiceId, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE invoices
            SET status = 'Paid', paid_at = NOW()
            WHERE id = @id AND status = 'Unpaid'
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", invoiceId);

        return await command.ExecuteNonQueryAsync(ct) > 0;
    }

    public async Task<List<InvoiceDto>> GetAllAsync(
        string billingMonth, int? roomId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, room_id, contract_id, billing_month, room_amount, electricity_amount,
                   water_amount, other_fees, total_amount, status, paid_at
            FROM invoices
            WHERE billing_month = @billingMonth
              AND (@roomId IS NULL OR room_id = @roomId)
            ORDER BY room_id
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@billingMonth", billingMonth);
        command.Parameters.AddWithValue("@roomId", (object?)roomId ?? DBNull.Value);

        await using var reader = await command.ExecuteReaderAsync(ct);
        var invoices = new List<InvoiceDto>();
        while (await reader.ReadAsync(ct))
        {
            invoices.Add(Map(reader));
        }

        return invoices;
    }

    /// <summary>BR-14: suy phòng từ chính tenantId, không nhận roomId từ client.</summary>
    public async Task<List<InvoiceDto>> GetByTenantAsync(int tenantId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT i.id, i.room_id, i.contract_id, i.billing_month, i.room_amount, i.electricity_amount,
                   i.water_amount, i.other_fees, i.total_amount, i.status, i.paid_at
            FROM invoices i
            JOIN tenants t ON t.room_id = i.room_id
            WHERE t.id = @tenantId
            ORDER BY i.billing_month DESC
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@tenantId", tenantId);

        await using var reader = await command.ExecuteReaderAsync(ct);
        var invoices = new List<InvoiceDto>();
        while (await reader.ReadAsync(ct))
        {
            invoices.Add(Map(reader));
        }

        return invoices;
    }

    private static InvoiceDto Map(MySqlDataReader reader) => new(
        reader.GetInt32("id"),
        reader.GetInt32("room_id"),
        reader.GetInt32("contract_id"),
        reader.GetString("billing_month"),
        reader.GetDecimal("room_amount"),
        reader.GetDecimal("electricity_amount"),
        reader.GetDecimal("water_amount"),
        reader.GetDecimal("other_fees"),
        reader.GetDecimal("total_amount"),
        Enum.Parse<InvoiceStatus>(reader.GetString("status")),
        reader.IsDBNull(reader.GetOrdinal("paid_at")) ? null : reader.GetDateTime("paid_at"));
}
