using MySqlConnector;
using QuanLyTro.Server.Data;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Repositories;

public class CuTruRepository(Database database)
{
    // ponytail: derive history from khach_thue + phong because dedicated residence_history table is not in schema. Upgrade to dedicated audit table if event history tracking is added.
    /// <param name="limit">Trần số dòng; null = không giới hạn (đường xuất file).</param>
    public virtual async Task<List<LichSuCuTruDto>> GetHistoryAsync(
        DateTime from,
        DateTime to,
        string? soPhong,
        int? limit = null,
        CancellationToken ct = default)
    {
        const string sql = """
            SELECT t.id, t.ho_ten, t.cccd, COALESCE(r.so_phong, '') AS so_phong,
                   t.ngay_tao, t.que_quan, t.phong_id
            FROM khach_thue t
            LEFT JOIN phong r ON r.id = t.phong_id
            WHERE t.ngay_tao >= @from AND t.ngay_tao <= @to
              AND (@soPhong IS NULL OR r.so_phong = @soPhong)
            ORDER BY t.ngay_tao DESC
            LIMIT @limit
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@from", from);
        command.Parameters.AddWithValue("@to", to);
        command.Parameters.AddWithValue("@soPhong", (object?)soPhong ?? DBNull.Value);
        // MySQL LIMIT nhận tham số; 18446744073709551615 = "không giới hạn" của chính MySQL.
        command.Parameters.AddWithValue("@limit", (object?)limit ?? ulong.MaxValue);

        await using var reader = await command.ExecuteReaderAsync(ct);
        var result = new List<LichSuCuTruDto>();
        while (await reader.ReadAsync(ct))
        {
            // Suy loại biến động từ phòng hiện tại: null = đã trả phòng (Ra).
            var loaiBienDong = reader.IsDBNull(reader.GetOrdinal("phong_id")) ? "Ra" : "Vào";
            var queQuan = reader.IsDBNull(reader.GetOrdinal("que_quan")) ? null : reader.GetString("que_quan");
            result.Add(new LichSuCuTruDto(
                reader.GetInt32("id"),
                reader.GetString("ho_ten"),
                reader.GetString("cccd"),
                reader.GetString("so_phong"),
                loaiBienDong,
                reader.GetDateTime("ngay_tao"),
                string.IsNullOrWhiteSpace(queQuan) ? null : $"Quê: {queQuan}"));
        }

        return result;
    }
}
