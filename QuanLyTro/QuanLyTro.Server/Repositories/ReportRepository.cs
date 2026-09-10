using MySqlConnector;
using QuanLyTro.Server.Data;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Repositories;

/// <summary>Biên đọc dữ liệu thống kê — interface để ReportService test không cần DB.</summary>
public interface IReportRepository
{
    Task<SummaryReportDto> GetSummaryAsync(string billingMonth, CancellationToken ct = default);
    Task<List<ResidenceExportDto>> GetResidentsAsync(CancellationToken ct = default);
}

/// <summary>Truy vấn tổng hợp cho dashboard (US-04, US-18, US-19) và xuất tạm trú (US-08).</summary>
public sealed class ReportRepository(Database database) : IReportRepository
{
    /// <summary>Đếm phòng/người và cộng tiền đã thu / còn nợ của tháng hóa đơn.</summary>
    public async Task<SummaryReportDto> GetSummaryAsync(string billingMonth, CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                (SELECT COUNT(*) FROM rooms) AS total_rooms,
                (SELECT COUNT(*) FROM rooms WHERE status = 'Available') AS available_rooms,
                (SELECT COUNT(*) FROM rooms WHERE status = 'Rented') AS rented_rooms,
                (SELECT COUNT(*) FROM tenants WHERE room_id IS NOT NULL) AS current_tenants,
                (SELECT COALESCE(SUM(total_amount), 0) FROM invoices
                 WHERE billing_month = @billingMonth AND status = 'Paid') AS paid_amount,
                (SELECT COALESCE(SUM(total_amount), 0) FROM invoices
                 WHERE billing_month = @billingMonth AND status = 'Unpaid') AS unpaid_amount
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@billingMonth", billingMonth);

        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
        {
            return new SummaryReportDto(0, 0, 0, 0, 0m, 0m);
        }

        return new SummaryReportDto(
            Convert.ToInt32(reader.GetInt64("total_rooms")),
            Convert.ToInt32(reader.GetInt64("available_rooms")),
            Convert.ToInt32(reader.GetInt64("rented_rooms")),
            Convert.ToInt32(reader.GetInt64("current_tenants")),
            reader.GetDecimal("paid_amount"),
            reader.GetDecimal("unpaid_amount"));
    }

    /// <summary>US-08: người đang ở (có phòng), xếp theo số phòng rồi họ tên.</summary>
    public async Task<List<ResidenceExportDto>> GetResidentsAsync(CancellationToken ct = default)
    {
        const string sql = """
            SELECT t.full_name, t.dob, t.id_card, t.hometown, r.room_number
            FROM tenants t
            JOIN rooms r ON r.id = t.room_id
            ORDER BY r.room_number, t.full_name
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(ct);

        var residents = new List<ResidenceExportDto>();
        while (await reader.ReadAsync(ct))
        {
            residents.Add(new ResidenceExportDto(
                reader.GetString("full_name"),
                reader.GetDateOnly("dob"),
                reader.GetString("id_card"),
                reader.GetString("hometown"),
                reader.GetString("room_number")));
        }

        return residents;
    }
}
