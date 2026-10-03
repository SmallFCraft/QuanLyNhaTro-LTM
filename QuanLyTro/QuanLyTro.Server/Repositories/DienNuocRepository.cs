using MySqlConnector;
using QuanLyTro.Server.Data;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Repositories;

/// <summary>Biên truy cập bảng `chi_so_dien_nuoc` — interface để DienNuocService test không cần DB.</summary>
public interface IDienNuocRepository
{
    Task<ChiSoDienNuocDto?> GetLatestAsync(int phongId, CancellationToken ct = default);
    Task<ChiSoDienNuocDto> AddAsync(ChiSoDienNuocDto reading, CancellationToken ct = default);

    /// <summary>
    /// Bản ghi gần nhất có `ky_cuoc` STRICTLY NHỎ HƠN tháng truyền vào (`yyyy-MM`).
    /// Cột là chuỗi cố định 7 ký tự nên so sánh chuỗi trùng thứ tự thời gian.
    /// Cài đặt mặc định lọc từ <see cref="GetLatestAsync"/> — chỉ đúng cho stub giữ 1 bản ghi;
    /// <see cref="DienNuocRepository"/> ghi đè bằng truy vấn SQL có chặn dưới.
    /// </summary>
    async Task<ChiSoDienNuocDto?> GetLatestBeforeAsync(
        int phongId, string kyCuoc, CancellationToken ct = default)
    {
        var latest = await GetLatestAsync(phongId, ct);
        return latest is not null && string.CompareOrdinal(latest.KyCuoc, kyCuoc) < 0
            ? latest
            : null;
    }
}

/// <summary>Chỉ số điện nước theo phòng/tháng. MySQL 1062 (uq_room_month) → LoiNghiepVu.</summary>
public sealed class DienNuocRepository(Database database) : IDienNuocRepository
{
    private const int MySqlDuplicateKey = 1062;

    public async Task<ChiSoDienNuocDto?> GetLatestAsync(int phongId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, phong_id, ky_cuoc, dien_cu, dien_moi, gia_dien,
                   nuoc_cu, nuoc_moi, gia_nuoc,
                   COALESCE(hinh_thuc_nuoc, 'Khoi') AS hinh_thuc_nuoc,
                   COALESCE(so_nguoi_nuoc, 0) AS so_nguoi_nuoc
            FROM chi_so_dien_nuoc
            WHERE phong_id = @phongId
            ORDER BY ky_cuoc DESC
            LIMIT 1
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@phongId", phongId);

        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
        {
            return null;
        }

        return Map(reader);
    }

    /// <summary>Bản ghi gần nhất TRƯỚC `kyCuoc` — chặn dưới nằm trong SQL, không lấy kỳ tương lai.</summary>
    public async Task<ChiSoDienNuocDto?> GetLatestBeforeAsync(
        int phongId, string kyCuoc, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, phong_id, ky_cuoc, dien_cu, dien_moi, gia_dien,
                   nuoc_cu, nuoc_moi, gia_nuoc,
                   COALESCE(hinh_thuc_nuoc, 'Khoi') AS hinh_thuc_nuoc,
                   COALESCE(so_nguoi_nuoc, 0) AS so_nguoi_nuoc
            FROM chi_so_dien_nuoc
            WHERE phong_id = @phongId AND ky_cuoc < @kyCuoc
            ORDER BY ky_cuoc DESC
            LIMIT 1
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@phongId", phongId);
        command.Parameters.AddWithValue("@kyCuoc", kyCuoc);

        await using var reader = await command.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    public async Task<ChiSoDienNuocDto> AddAsync(ChiSoDienNuocDto reading, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO chi_so_dien_nuoc (phong_id, ky_cuoc, dien_cu, dien_moi,
                                          gia_dien, nuoc_cu, nuoc_moi, gia_nuoc,
                                          hinh_thuc_nuoc, so_nguoi_nuoc)
            VALUES (@phongId, @kyCuoc, @dienCu, @dienMoi,
                    @giaDien, @nuocCu, @nuocMoi, @giaNuoc,
                    @hinhThucNuoc, @soNguoiNuoc);
            SELECT LAST_INSERT_ID();
            """;

        try
        {
            await using var connection = await database.OpenAsync(ct);
            await using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@phongId", reading.PhongId);
            command.Parameters.AddWithValue("@kyCuoc", reading.KyCuoc);
            command.Parameters.AddWithValue("@dienCu", reading.DienCu);
            command.Parameters.AddWithValue("@dienMoi", reading.DienMoi);
            command.Parameters.AddWithValue("@giaDien", reading.GiaDien);
            command.Parameters.AddWithValue("@nuocCu", reading.NuocCu);
            command.Parameters.AddWithValue("@nuocMoi", reading.NuocMoi);
            command.Parameters.AddWithValue("@giaNuoc", reading.GiaNuoc);
            command.Parameters.AddWithValue("@hinhThucNuoc", string.IsNullOrWhiteSpace(reading.HinhThucNuoc) ? "Khoi" : reading.HinhThucNuoc);
            command.Parameters.AddWithValue("@soNguoiNuoc", reading.SoNguoiNuoc);

            var id = Convert.ToInt32(await command.ExecuteScalarAsync(ct));
            return reading with { Id = id };
        }
        catch (MySqlException ex) when (ex.Number == MySqlDuplicateKey)
        {
            throw new Services.LoiNghiepVu("Phòng này đã chốt điện nước tháng này.");
        }
    }

    private static ChiSoDienNuocDto Map(MySqlDataReader reader) => new(
        reader.GetInt32("id"),
        reader.GetInt32("phong_id"),
        reader.GetString("ky_cuoc"),
        reader.GetInt32("dien_cu"),
        reader.GetInt32("dien_moi"),
        reader.GetDecimal("gia_dien"),
        reader.GetInt32("nuoc_cu"),
        reader.GetInt32("nuoc_moi"),
        reader.GetDecimal("gia_nuoc"),
        reader.GetString("hinh_thuc_nuoc"),
        reader.GetInt32("so_nguoi_nuoc"));
}
