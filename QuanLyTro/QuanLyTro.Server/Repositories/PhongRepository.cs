using MySqlConnector;
using QuanLyTro.Server.Data;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Repositories;

/// <summary>CRUD phòng. Mọi truy vấn parameter hóa; MySQL duplicate key chuyển thành LoiNghiepVu.</summary>
public sealed class PhongRepository(Database database)
{
    private const int MySqlDuplicateKey = 1062;
    private const int MySqlRowIsReferenced = 1451;

    /// <summary>BR-12 — thông báo dùng chung cho cả đường kiểm tra lẫn khi MySQL chặn khóa ngoại.</summary>
    public const string OccupiedMessage =
        "Không thể xóa: phòng còn người thuê hoặc hợp đồng đang hiệu lực.";

    public async Task<List<PhongDto>> GetAllAsync(CancellationToken ct = default)
    {
        const string sql = """
            SELECT r.id, r.so_phong, r.gia_thue, r.so_nguoi_toi_da, r.trang_thai, r.mo_ta,
                   COUNT(t.id) AS so_nguoi_hien_tai
            FROM phong r
            LEFT JOIN khach_thue t ON t.phong_id = r.id
            GROUP BY r.id, r.so_phong, r.gia_thue, r.so_nguoi_toi_da, r.trang_thai, r.mo_ta
            ORDER BY r.so_phong
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(ct);

        var phong = new List<PhongDto>();
        while (await reader.ReadAsync(ct))
        {
            phong.Add(new PhongDto(
                reader.GetInt32("id"),
                reader.GetString("so_phong"),
                reader.GetDecimal("gia_thue"),
                reader.GetInt32("so_nguoi_toi_da"),
                Enum.Parse<TrangThaiPhong>(reader.GetString("trang_thai")),
                reader.IsDBNull(reader.GetOrdinal("mo_ta")) ? null : reader.GetString("mo_ta"),
                reader.GetInt32("so_nguoi_hien_tai")));
        }

        return phong;
    }

    public async Task<PhongDto> AddAsync(PhongDto room, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO phong (so_phong, gia_thue, so_nguoi_toi_da, trang_thai, mo_ta)
            VALUES (@soPhong, @gia_thue, @soNguoiToiDa, @trang_thai, @mo_ta);
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
            throw new Services.LoiNghiepVu("Số phòng đã tồn tại.");
        }
    }

    public async Task<bool> UpdateAsync(PhongDto room, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE phong
            SET so_phong = @soPhong,
                gia_thue = @gia_thue,
                so_nguoi_toi_da = @soNguoiToiDa,
                trang_thai = @trang_thai,
                mo_ta = @mo_ta
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
            throw new Services.LoiNghiepVu("Số phòng đã tồn tại.");
        }
    }

    /// <summary>
    /// BR-12: khóa dòng phòng, kiểm tra người thuê/hợp đồng, rồi xóa trong cùng giao_dich.
    /// Khóa này tuần tự hóa với KhachThueRepository.AddAsync và HopDongRepository.AddAsync.
    /// </summary>
    public async Task<bool> DeleteAsync(int phongId, CancellationToken ct = default)
    {
        const string lockRoomSql = "SELECT id FROM phong WHERE id = @id FOR UPDATE";
        const string occupancySql = """
            SELECT
                (SELECT COUNT(*) FROM khach_thue WHERE phong_id = @id) AS tenant_count,
                (SELECT COUNT(*) FROM hop_dong WHERE phong_id = @id AND trang_thai = 'HieuLuc') AS hop_dong_hieu_luc
            """;
        const string deleteSql = "DELETE FROM phong WHERE id = @id";

        try
        {
            await using var connection = await database.OpenAsync(ct);
            await using var giao_dich = await connection.BeginTransactionAsync(ct);

            await using (var lockCommand = new MySqlCommand(lockRoomSql, connection, giao_dich))
            {
                lockCommand.Parameters.AddWithValue("@id", phongId);
                if (await lockCommand.ExecuteScalarAsync(ct) is null)
                {
                    return false;
                }
            }

            await using (var occupancyCommand = new MySqlCommand(occupancySql, connection, giao_dich))
            {
                occupancyCommand.Parameters.AddWithValue("@id", phongId);
                await using var reader = await occupancyCommand.ExecuteReaderAsync(ct);
                await reader.ReadAsync(ct);
                if (reader.GetInt64("tenant_count") > 0 || reader.GetInt64("hop_dong_hieu_luc") > 0)
                {
                    throw new Services.LoiNghiepVu(OccupiedMessage);
                }
            }

            await using var deleteCommand = new MySqlCommand(deleteSql, connection, giao_dich);
            deleteCommand.Parameters.AddWithValue("@id", phongId);
            var deleted = await deleteCommand.ExecuteNonQueryAsync(ct) > 0;
            await giao_dich.CommitAsync(ct);
            return deleted;
        }
        catch (MySqlException ex) when (ex.Number == MySqlRowIsReferenced)
        {
            throw new Services.LoiNghiepVu(OccupiedMessage);
        }
    }

    /// <summary>BR-12: chỉ xóa phòng khi không còn người thuê và không có hợp đồng HieuLuc.</summary>
    public async Task<bool> CanDeleteAsync(int phongId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                (SELECT COUNT(*) FROM khach_thue t WHERE t.phong_id = @id) AS tenant_count,
                (SELECT COUNT(*) FROM hop_dong c WHERE c.phong_id = @id AND c.trang_thai = 'HieuLuc') AS hop_dong_hieu_luc
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", phongId);

        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
        {
            return false;
        }

        return reader.GetInt64("tenant_count") == 0 && reader.GetInt64("hop_dong_hieu_luc") == 0;
    }

    private static void AddRoomParameters(MySqlCommand command, PhongDto room)
    {
        command.Parameters.AddWithValue("@soPhong", room.SoPhong);
        command.Parameters.AddWithValue("@gia_thue", room.GiaThue);
        command.Parameters.AddWithValue("@soNguoiToiDa", room.SoNguoiToiDa);
        command.Parameters.AddWithValue("@trang_thai", room.TrangThai.ToString());
        command.Parameters.AddWithValue("@mo_ta", (object?)room.MoTa ?? DBNull.Value);
    }
}
