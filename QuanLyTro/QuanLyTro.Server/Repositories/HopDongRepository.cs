using MySqlConnector;
using QuanLyTro.Server.Data;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Repositories;

/// <summary>Hợp đồng kèm số phòng + tên đại diện (US-11), xếp theo ngay_ket_thuc tăng dần.</summary>
public sealed record MucHopDongItem(HopDongDto Contract, string SoPhong, string RepresentativeName);

/// <summary>Biên truy cập bảng `hop_dong` — interface để HopDongService test được không cần DB.</summary>
public interface IHopDongRepository
{
    Task<HopDongDto?> GetByIdAsync(int hopDongId, CancellationToken ct = default);
    Task<bool> HasActiveContractAsync(int phongId, CancellationToken ct = default);
    Task<bool> IsTenantInRoomAsync(int khachThueId, int phongId, CancellationToken ct = default);
    Task<HopDongDto> AddAsync(HopDongDto contract, CancellationToken ct = default);
    Task<bool> TerminateAsync(int hopDongId, string? ghi_chu, CancellationToken ct = default);
    Task<bool> UpdateEndDateAsync(int hopDongId, DateOnly newEndDate, CancellationToken ct = default);
    Task<List<MucHopDongItem>> GetAllAsync(CancellationToken ct = default);
}

/// <summary>CRUD hợp đồng. MySQL duplicate key chuyển thành LoiNghiepVu.</summary>
public sealed class HopDongRepository(Database database) : IHopDongRepository
{
    public async Task<HopDongDto?> GetByIdAsync(int hopDongId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, phong_id, nguoi_dai_dien_id, ngay_bat_dau, ngay_ket_thuc,
                   gia_thue, tien_coc, trang_thai, ghi_chu
            FROM hop_dong
            WHERE id = @id
            LIMIT 1
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", hopDongId);

        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
        {
            return null;
        }

        return Map(reader);
    }

    /// <summary>BR-04: đếm hợp đồng `HieuLuc` của phòng.</summary>
    public async Task<bool> HasActiveContractAsync(int phongId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT COUNT(*)
            FROM hop_dong
            WHERE phong_id = @phongId AND trang_thai = 'HieuLuc'
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@phongId", phongId);

        return Convert.ToInt64(await command.ExecuteScalarAsync(ct)) > 0;
    }

    /// <summary>BR-05: đại diện ký HĐ phải đang ở chính phòng đó.</summary>
    public async Task<bool> IsTenantInRoomAsync(int khachThueId, int phongId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT COUNT(*)
            FROM khach_thue
            WHERE id = @khachThueId AND phong_id = @phongId
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@khachThueId", khachThueId);
        command.Parameters.AddWithValue("@phongId", phongId);

        return Convert.ToInt64(await command.ExecuteScalarAsync(ct)) > 0;
    }

    /// <summary>
    /// BR-04 + BR-05: khóa dòng phòng (SELECT ... FOR UPDATE) rồi mới kiểm tra và chèn — tất cả
    /// trong MỘT giao_dich. Hai client đồng thời không cùng lọt qua được khe kiểm tra.
    /// </summary>
    public async Task<HopDongDto> AddAsync(HopDongDto contract, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO hop_dong (phong_id, nguoi_dai_dien_id, ngay_bat_dau, ngay_ket_thuc,
                                   gia_thue, tien_coc, trang_thai, ghi_chu)
            VALUES (@phongId, @nguoiDaiDienId, @ngayBatDau, @ngayKetThuc,
                    @giaThue, @tienCoc, @trang_thai, @ghi_chu);
            SELECT LAST_INSERT_ID();
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var giao_dich = await connection.BeginTransactionAsync(ct);

        await EnsureContractCanBeAddedAsync(
            connection, giao_dich, contract.PhongId, contract.NguoiDaiDienId, ct);

        await using var command = new MySqlCommand(sql, connection, giao_dich);
        AddContractParameters(command, contract);
        var id = Convert.ToInt32(await command.ExecuteScalarAsync(ct));

        await giao_dich.CommitAsync(ct);
        return contract with { Id = id };
    }

    /// <summary>Chấm dứt hợp đồng: trang_thai = 'ChamDut', ghi chú thêm lý do.</summary>
    public async Task<bool> TerminateAsync(int hopDongId, string? ghi_chu, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE hop_dong
            SET trang_thai = 'ChamDut',
                ghi_chu = CONCAT_WS(' — ', NULLIF(ghi_chu, ''), @ghi_chu)
            WHERE id = @id AND trang_thai = 'HieuLuc'
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", hopDongId);
        command.Parameters.AddWithValue("@ghi_chu", (object?)ghi_chu ?? DBNull.Value);

        return await command.ExecuteNonQueryAsync(ct) > 0;
    }

    /// <summary>
    /// US-10: gia hạn — chỉ hợp đồng `HieuLuc` và ngày mới phải sau ngay_ket_thuc hiện tại (điều kiện
    /// ngay trong UPDATE để chặn race: A gia hạn 2027-12-31, B gia hạn 2027-06-30 sau đó → 0 dòng).
    /// </summary>
    public async Task<bool> UpdateEndDateAsync(int hopDongId, DateOnly newEndDate, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE hop_dong
            SET ngay_ket_thuc = @ngayKetThuc
            WHERE id = @id AND trang_thai = 'HieuLuc' AND ngay_ket_thuc < @ngayKetThuc
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@ngayKetThuc", newEndDate);
        command.Parameters.AddWithValue("@id", hopDongId);

        return await command.ExecuteNonQueryAsync(ct) > 0;
    }

    public async Task<List<MucHopDongItem>> GetAllAsync(CancellationToken ct = default)
    {
        const string sql = """
            SELECT c.id, c.phong_id, c.nguoi_dai_dien_id, c.ngay_bat_dau, c.ngay_ket_thuc,
                   c.gia_thue, c.tien_coc, c.trang_thai, c.ghi_chu,
                   r.so_phong, t.ho_ten
            FROM hop_dong c
            JOIN phong r ON r.id = c.phong_id
            JOIN khach_thue t ON t.id = c.nguoi_dai_dien_id
            ORDER BY c.ngay_ket_thuc
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(ct);

        var hop_dong = new List<MucHopDongItem>();
        while (await reader.ReadAsync(ct))
        {
            hop_dong.Add(new MucHopDongItem(
                Map(reader),
                reader.GetString("so_phong"),
                reader.GetString("ho_ten")));
        }

        return hop_dong;
    }

    private static HopDongDto Map(MySqlDataReader reader) => new(
        reader.GetInt32("id"),
        reader.GetInt32("phong_id"),
        reader.GetInt32("nguoi_dai_dien_id"),
        reader.GetDateOnly("ngay_bat_dau"),
        reader.GetDateOnly("ngay_ket_thuc"),
        reader.GetDecimal("gia_thue"),
        reader.GetDecimal("tien_coc"),
        Enum.Parse<TrangThaiHopDong>(reader.GetString("trang_thai")),
        reader.IsDBNull(reader.GetOrdinal("ghi_chu")) ? null : reader.GetString("ghi_chu"));

    /// <summary>BR-04 + BR-05: kiểm tra sau khi đã khóa dòng phòng trong cùng giao_dich.</summary>
    private static async Task EnsureContractCanBeAddedAsync(
        MySqlConnection connection, MySqlTransaction giao_dich,
        int phongId, int nguoiDaiDienId, CancellationToken ct)
    {
        const string lockRoomSql = "SELECT id FROM phong WHERE id = @phongId FOR UPDATE";
        // FOR UPDATE = current read: đọc bản mới nhất đã commit, không dùng snapshot REPEATABLE READ.
        const string hasActiveContractSql = """
            SELECT COUNT(*)
            FROM hop_dong
            WHERE phong_id = @phongId AND trang_thai = 'HieuLuc'
            FOR UPDATE
            """;
        const string tenantInRoomSql = """
            SELECT COUNT(*)
            FROM khach_thue
            WHERE id = @khachThueId AND phong_id = @phongId
            FOR UPDATE
            """;

        await using (var command = new MySqlCommand(lockRoomSql, connection, giao_dich))
        {
            command.Parameters.AddWithValue("@phongId", phongId);
            if (await command.ExecuteScalarAsync(ct) is null)
            {
                throw new Services.LoiNghiepVu("Phòng không tồn tại.");
            }
        }

        await using (var command = new MySqlCommand(hasActiveContractSql, connection, giao_dich))
        {
            command.Parameters.AddWithValue("@phongId", phongId);
            if (Convert.ToInt64(await command.ExecuteScalarAsync(ct)) > 0)
            {
                throw new Services.LoiNghiepVu("Phòng này đang có hợp đồng hiệu lực.");
            }
        }

        await using var tenantCommand = new MySqlCommand(tenantInRoomSql, connection, giao_dich);
        tenantCommand.Parameters.AddWithValue("@khachThueId", nguoiDaiDienId);
        tenantCommand.Parameters.AddWithValue("@phongId", phongId);
        if (Convert.ToInt64(await tenantCommand.ExecuteScalarAsync(ct)) == 0)
        {
            throw new Services.LoiNghiepVu("Người đại diện phải là người đang ở trong phòng này.");
        }
    }

    private static void AddContractParameters(MySqlCommand command, HopDongDto contract)
    {
        command.Parameters.AddWithValue("@phongId", contract.PhongId);
        command.Parameters.AddWithValue("@nguoiDaiDienId", contract.NguoiDaiDienId);
        command.Parameters.AddWithValue("@ngayBatDau", contract.NgayBatDau);
        command.Parameters.AddWithValue("@ngayKetThuc", contract.NgayKetThuc);
        command.Parameters.AddWithValue("@giaThue", contract.GiaThue);
        command.Parameters.AddWithValue("@tienCoc", contract.TienCoc);
        command.Parameters.AddWithValue("@trang_thai", contract.TrangThai.ToString());
        command.Parameters.AddWithValue("@ghi_chu", (object?)contract.GhiChu ?? DBNull.Value);
    }
}
