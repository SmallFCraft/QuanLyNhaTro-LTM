using MySqlConnector;
using QuanLyTro.Server.Data;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Repositories;

/// <summary>Biên truy cập bảng `hoa_don` — interface để HoaDonService test không cần DB.</summary>
public interface IHoaDonRepository
{
    Task<HopDongDto?> GetActiveContractAsync(int phongId, CancellationToken ct = default);
    Task<ChiSoDienNuocDto?> GetUtilityReadingAsync(string kyCuoc, int phongId, CancellationToken ct = default);
    Task<HoaDonDto> AddAsync(HoaDonDto invoice, CancellationToken ct = default);
    Task<HoaDonDto?> GetByIdAsync(int hoaDonId, CancellationToken ct = default);
    Task<bool> MarkPaidAsync(int hoaDonId, CancellationToken ct = default);
    Task<List<HoaDonDto>> GetAllAsync(string kyCuoc, int? phongId, CancellationToken ct = default);
    Task<TrangHoaDonCuaToiDto> GetByTenantAsync(int khachThueId, int page, int soLuongMoiTrang, CancellationToken ct = default);
}

/// <summary>
/// Hóa đơn tháng. MySQL 1062 (uq_invoice_room_month) → LoiNghiepVu.
/// Tra hợp đồng HieuLuc và chỉ số điện nước ngay trong giao_dich của BR-13.
/// </summary>
public sealed class HoaDonRepository(Database database) : IHoaDonRepository
{
    private const int MySqlDuplicateKey = 1062;

    private const string InvoiceColumns =
        "id, phong_id, hop_dong_id, ky_cuoc, tien_phong, tien_dien, " +
        "tien_nuoc, phi_khac, tong_tien, trang_thai, ngay_dong";

    /// <summary>BR-04/BR-09: hợp đồng `HieuLuc` mới nhất của phòng.</summary>
    public async Task<HopDongDto?> GetActiveContractAsync(int phongId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, phong_id, nguoi_dai_dien_id, ngay_bat_dau, ngay_ket_thuc,
                   gia_thue, tien_coc, trang_thai, ghi_chu
            FROM hop_dong
            WHERE phong_id = @phongId AND trang_thai = 'HieuLuc'
            ORDER BY ngay_bat_dau DESC
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

        return new HopDongDto(
            reader.GetInt32("id"),
            reader.GetInt32("phong_id"),
            reader.GetInt32("nguoi_dai_dien_id"),
            reader.GetDateOnly("ngay_bat_dau"),
            reader.GetDateOnly("ngay_ket_thuc"),
            reader.GetDecimal("gia_thue"),
            reader.GetDecimal("tien_coc"),
            Enum.Parse<TrangThaiHopDong>(reader.GetString("trang_thai")),
            reader.IsDBNull(reader.GetOrdinal("ghi_chu")) ? null : reader.GetString("ghi_chu"));
    }

    /// <summary>BR-09: chỉ số điện nước đã chốt của phòng trong tháng.</summary>
    public async Task<ChiSoDienNuocDto?> GetUtilityReadingAsync(
        string kyCuoc, int phongId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, phong_id, ky_cuoc, dien_cu, dien_moi, gia_dien,
                   nuoc_cu, nuoc_moi, gia_nuoc,
                   COALESCE(hinh_thuc_nuoc, 'Khoi') AS hinh_thuc_nuoc,
                   COALESCE(so_nguoi_nuoc, 0) AS so_nguoi_nuoc
            FROM chi_so_dien_nuoc
            WHERE phong_id = @phongId AND ky_cuoc = @kyCuoc
            LIMIT 1
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@phongId", phongId);
        command.Parameters.AddWithValue("@kyCuoc", kyCuoc);

        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
        {
            return null;
        }

        return new ChiSoDienNuocDto(
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

    /// <summary>BR-13: insert nằm trong giao_dich của chính nó.</summary>
    // ponytail: không khóa phòng trước khi insert — uq_invoice_room_month đã chặn trùng ở tầng DB.
    // Thêm SELECT ... FOR UPDATE nếu sau này BR-09 đổi thành ghi thêm bảng khác trong cùng thao tác.
    public async Task<HoaDonDto> AddAsync(HoaDonDto invoice, CancellationToken ct = default)
    {
        const string insertSql = """
            INSERT INTO hoa_don (phong_id, hop_dong_id, ky_cuoc, tien_phong, tien_dien,
                                  tien_nuoc, phi_khac, tong_tien, trang_thai, ngay_dong)
            VALUES (@phongId, @hopDongId, @kyCuoc, @tienPhong, @tienDien,
                    @tienNuoc, @phiKhac, @tongTien, @trang_thai, @ngayDong);
            SELECT LAST_INSERT_ID();
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var giao_dich = await connection.BeginTransactionAsync(ct);

        int id;
        try
        {
            await using var command = new MySqlCommand(insertSql, connection, giao_dich);
            command.Parameters.AddWithValue("@phongId", invoice.PhongId);
            command.Parameters.AddWithValue("@hopDongId", invoice.HopDongId);
            command.Parameters.AddWithValue("@kyCuoc", invoice.KyCuoc);
            command.Parameters.AddWithValue("@tienPhong", invoice.TienPhong);
            command.Parameters.AddWithValue("@tienDien", invoice.TienDien);
            command.Parameters.AddWithValue("@tienNuoc", invoice.TienNuoc);
            command.Parameters.AddWithValue("@phiKhac", invoice.PhiKhac);
            command.Parameters.AddWithValue("@tongTien", invoice.TongTien);
            command.Parameters.AddWithValue("@trang_thai", invoice.TrangThai.ToString());
            command.Parameters.AddWithValue("@ngayDong", (object?)invoice.NgayDong ?? DBNull.Value);

            id = Convert.ToInt32(await command.ExecuteScalarAsync(ct));
        }
        catch (MySqlException ex) when (ex.Number == MySqlDuplicateKey)
        {
            throw new Services.LoiNghiepVu("Tháng này đã lập hóa đơn cho phòng.");
        }

        await giao_dich.CommitAsync(ct);
        return invoice with { Id = id };
    }

    public async Task<HoaDonDto?> GetByIdAsync(int hoaDonId, CancellationToken ct = default)
    {
        var sql = $"SELECT {InvoiceColumns} FROM hoa_don WHERE id = @id LIMIT 1";

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", hoaDonId);

        await using var reader = await command.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    /// <summary>BR-11: điều kiện `trang_thai = 'ChuaThu'` nằm trong WHERE — hóa đơn DaThu bất biến ở tầng DB.</summary>
    public async Task<bool> MarkPaidAsync(int hoaDonId, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE hoa_don
            SET trang_thai = 'DaThu', ngay_dong = NOW()
            WHERE id = @id AND trang_thai = 'ChuaThu'
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", hoaDonId);

        return await command.ExecuteNonQueryAsync(ct) > 0;
    }

    public async Task<List<HoaDonDto>> GetAllAsync(
        string kyCuoc, int? phongId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, phong_id, hop_dong_id, ky_cuoc, tien_phong, tien_dien,
                   tien_nuoc, phi_khac, tong_tien, trang_thai, ngay_dong
            FROM hoa_don
            WHERE ky_cuoc = @kyCuoc
              AND (@phongId IS NULL OR phong_id = @phongId)
            ORDER BY phong_id
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@kyCuoc", kyCuoc);
        command.Parameters.AddWithValue("@phongId", (object?)phongId ?? DBNull.Value);

        await using var reader = await command.ExecuteReaderAsync(ct);
        var hoa_don = new List<HoaDonDto>();
        while (await reader.ReadAsync(ct))
        {
            hoa_don.Add(Map(reader));
        }

        return hoa_don;
    }

    /// <summary>BR-14: suy phòng từ chính khachThueId, không nhận phongId từ client. Phân trang server-side.</summary>
    public async Task<TrangHoaDonCuaToiDto> GetByTenantAsync(
        int khachThueId, int page, int soLuongMoiTrang, CancellationToken ct = default)
    {
        var safePage = page < 1 ? 1 : page;
        var safePageSize = soLuongMoiTrang switch
        {
            < 1 => 10,
            > 50 => 50,
            _ => soLuongMoiTrang
        };
        var offset = (safePage - 1) * safePageSize;

        await using var connection = await database.OpenAsync(ct);

        // BR-14: hóa đơn thuộc về người thuê QUA HỢP ĐỒNG (hop_dong.nguoi_dai_dien_id),
        // không phải qua phòng hiện tại — join theo phong_id sẽ lộ hóa đơn của khách cũ
        // ở cùng phòng và làm mất lịch sử của khách sau khi trả phòng (phong_id = NULL).
        const string countSql = """
            SELECT COUNT(*)
            FROM hoa_don i
            JOIN hop_dong c ON c.id = i.hop_dong_id
            WHERE c.nguoi_dai_dien_id = @khachThueId
            """;
        await using var countCmd = new MySqlCommand(countSql, connection);
        countCmd.Parameters.AddWithValue("@khachThueId", khachThueId);
        var tongSo = Convert.ToInt32(await countCmd.ExecuteScalarAsync(ct));

        const string sql = """
            SELECT i.id, i.phong_id, i.hop_dong_id, i.ky_cuoc, i.tien_phong, i.tien_dien,
                   i.tien_nuoc, i.phi_khac, i.tong_tien, i.trang_thai, i.ngay_dong
            FROM hoa_don i
            JOIN hop_dong c ON c.id = i.hop_dong_id
            WHERE c.nguoi_dai_dien_id = @khachThueId
            ORDER BY i.ky_cuoc DESC, i.id DESC
            LIMIT @limit OFFSET @offset
            """;

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@khachThueId", khachThueId);
        command.Parameters.AddWithValue("@limit", safePageSize);
        command.Parameters.AddWithValue("@offset", offset);

        await using var reader = await command.ExecuteReaderAsync(ct);
        var hoa_don = new List<HoaDonDto>();
        while (await reader.ReadAsync(ct))
        {
            hoa_don.Add(Map(reader));
        }

        await reader.DisposeAsync();

        // BR-19: khách CHƯA nhận phòng (phong_id IS NULL) → client hiện màn quét QR.
        // Không suy từ số hóa đơn: khách vừa nhận phòng chưa có kỳ cước nào vẫn phải vào app.
        const string roomSql = """
            SELECT t.phong_id, r.so_phong
            FROM khach_thue t
            LEFT JOIN phong r ON r.id = t.phong_id
            WHERE t.id = @khachThueId
            """;
        int? phongId = null;
        string? soPhong = null;
        await using (var roomCmd = new MySqlCommand(roomSql, connection))
        {
            roomCmd.Parameters.AddWithValue("@khachThueId", khachThueId);
            await using var roomReader = await roomCmd.ExecuteReaderAsync(ct);
            if (await roomReader.ReadAsync(ct) && !roomReader.IsDBNull(roomReader.GetOrdinal("phong_id")))
            {
                phongId = roomReader.GetInt32("phong_id");
                soPhong = roomReader.IsDBNull(roomReader.GetOrdinal("so_phong"))
                    ? null
                    : roomReader.GetString("so_phong");
            }
        }

        HopDongCuaToiDto? hopDong = null;
        const string hopDongSql = """
            SELECT c.id, c.ngay_bat_dau, c.ngay_ket_thuc, c.gia_thue, c.tien_coc, r.so_phong
            FROM hop_dong c
            LEFT JOIN phong r ON r.id = c.phong_id
            WHERE c.nguoi_dai_dien_id = @khachThueId AND c.trang_thai = 'HieuLuc'
            ORDER BY c.ngay_bat_dau DESC
            LIMIT 1
            """;
        await using (var hopDongCmd = new MySqlCommand(hopDongSql, connection))
        {
            hopDongCmd.Parameters.AddWithValue("@khachThueId", khachThueId);
            await using var hopDongReader = await hopDongCmd.ExecuteReaderAsync(ct);
            if (await hopDongReader.ReadAsync(ct))
            {
                hopDong = new HopDongCuaToiDto(
                    hopDongReader.GetInt32("id"),
                    hopDongReader.IsDBNull(hopDongReader.GetOrdinal("so_phong"))
                        ? null
                        : hopDongReader.GetString("so_phong"),
                    hopDongReader.GetDateOnly("ngay_bat_dau"),
                    hopDongReader.GetDateOnly("ngay_ket_thuc"),
                    hopDongReader.GetDecimal("gia_thue"),
                    hopDongReader.GetDecimal("tien_coc"));
            }
        }

        // Chỉ số gắn với hóa đơn đang hiển thị (DanhSach[0] = kỳ mới nhất của trang),
        // không phải kỳ mới nhất của phòng — số liệu phải khớp đúng tờ hóa đơn trên màn hình.
        ChiSoKyNayDto? chiSoKyNay = null;
        if (phongId is { } room && hoa_don.Count > 0)
        {
            const string chiSoSql = """
                SELECT ky_cuoc, dien_cu, dien_moi, gia_dien, nuoc_cu, nuoc_moi, gia_nuoc,
                       COALESCE(hinh_thuc_nuoc, 'Khoi') AS hinh_thuc_nuoc,
                       COALESCE(so_nguoi_nuoc, 0) AS so_nguoi_nuoc
                FROM chi_so_dien_nuoc
                WHERE phong_id = @phongId AND ky_cuoc = @kyCuoc
                """;
            await using (var chiSoCmd = new MySqlCommand(chiSoSql, connection))
            {
                chiSoCmd.Parameters.AddWithValue("@phongId", room);
                chiSoCmd.Parameters.AddWithValue("@kyCuoc", hoa_don[0].KyCuoc);
                await using var chiSoReader = await chiSoCmd.ExecuteReaderAsync(ct);
                if (await chiSoReader.ReadAsync(ct))
                {
                    chiSoKyNay = new ChiSoKyNayDto(
                        chiSoReader.GetString("ky_cuoc"),
                        chiSoReader.GetInt32("dien_cu"),
                        chiSoReader.GetInt32("dien_moi"),
                        chiSoReader.GetDecimal("gia_dien"),
                        chiSoReader.GetInt32("nuoc_cu"),
                        chiSoReader.GetInt32("nuoc_moi"),
                        chiSoReader.GetDecimal("gia_nuoc"),
                        chiSoReader.GetString("hinh_thuc_nuoc"),
                        chiSoReader.GetInt32("so_nguoi_nuoc"));
                }
            }
        }

        return new TrangHoaDonCuaToiDto(
            hoa_don, tongSo, safePage, safePageSize, phongId, soPhong, hopDong, chiSoKyNay);
    }

    private static HoaDonDto Map(MySqlDataReader reader) => new(
        reader.GetInt32("id"),
        reader.GetInt32("phong_id"),
        reader.GetInt32("hop_dong_id"),
        reader.GetString("ky_cuoc"),
        reader.GetDecimal("tien_phong"),
        reader.GetDecimal("tien_dien"),
        reader.GetDecimal("tien_nuoc"),
        reader.GetDecimal("phi_khac"),
        reader.GetDecimal("tong_tien"),
        Enum.Parse<TrangThaiHoaDon>(reader.GetString("trang_thai")),
        reader.IsDBNull(reader.GetOrdinal("ngay_dong")) ? null : reader.GetDateTime("ngay_dong"));
}
