using MySqlConnector;
using QuanLyTro.Server.Data;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Repositories;

/// <summary>Kết quả xóa hồ sơ người thuê (US-06) — tách "không tồn tại" khỏi "còn đang ở".</summary>
public enum KetQuaXoaKhachThue
{
    DaXoa,
    ConTrongPhong,
    KhongTimThay,
}

/// <summary>CRUD người thuê. Mọi truy vấn parameter hóa; thêm mới khóa phòng bằng SELECT ... FOR UPDATE (BR-02).</summary>
public sealed class KhachThueRepository(Database database)
{
    private const int MySqlDuplicateKey = 1062;

    private const string TenantColumns =
        "id, phong_id, ho_ten, ngay_sinh, cccd, so_dien_thoai, que_quan, noi_lam_viec, da_dang_ky_tam_tru";

    public async Task<List<KhachThueDto>> GetByRoomAsync(int phongId, CancellationToken ct = default)
    {
        // phongId <= 0: khách tự đăng ký đang chờ gán phòng (phong_id IS NULL).
        // CHỈ dùng cho màn lập hợp đồng của Chủ trọ/Quản lý — router đã chặn vai trò khác trước khi tới đây.
        var sql = phongId <= 0
            ? $"""
                SELECT {TenantColumns}
                FROM khach_thue
                WHERE phong_id IS NULL
                ORDER BY ngay_tao DESC, ho_ten
                """
            : $"""
                SELECT {TenantColumns}
                FROM khach_thue
                WHERE phong_id = @phongId
                ORDER BY ho_ten
                """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        if (phongId > 0)
        {
            command.Parameters.AddWithValue("@phongId", phongId);
        }

        await using var reader = await command.ExecuteReaderAsync(ct);
        var khach_thue = new List<KhachThueDto>();
        while (await reader.ReadAsync(ct))
        {
            khach_thue.Add(ReadTenant(reader));
        }

        return khach_thue;
    }

    /// <summary>BR-02: khóa phòng bằng SELECT ... FOR UPDATE để đếm sức chứa không bị race hai tác nhân.</summary>
    public async Task<KhachThueDto> AddAsync(KhachThueDto tenant, string matKhauHash, CancellationToken ct = default)
    {
        const string sql = $"""
            INSERT INTO khach_thue
                (phong_id, ho_ten, ngay_sinh, cccd, mat_khau_hash, so_dien_thoai, que_quan, noi_lam_viec, da_dang_ky_tam_tru)
            VALUES
                (@phongId, @hoTen, @ngay_sinh, @cccd, @matKhauHash, @so_dien_thoai, @que_quan, @noi_lam_viec, @daDangKyTamTru);
            SELECT LAST_INSERT_ID();
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var giao_dich = await connection.BeginTransactionAsync(ct);

        if (tenant.PhongId is { } phongId)
        {
            await EnsureRoomHasSpaceAsync(connection, giao_dich, phongId, ct);
        }

        int id;
        try
        {
            await using var command = new MySqlCommand(sql, connection, giao_dich);
            AddTenantParameters(command, tenant);
            command.Parameters.AddWithValue("@matKhauHash", matKhauHash);
            id = Convert.ToInt32(await command.ExecuteScalarAsync(ct));
        }
        catch (MySqlException ex) when (ex.Number == MySqlDuplicateKey)
        {
            throw new Services.LoiNghiepVu("Số CCCD đã tồn tại trong hệ thống.");
        }

        if (tenant.PhongId is { } occupyRoomId)
        {
            await SetRoomStatusAsync(connection, giao_dich, occupyRoomId, TrangThaiPhong.DaThue, ct);
        }

        await giao_dich.CommitAsync(ct);
        return tenant with { Id = id };
    }

    /// <summary>Đổi hồ sơ, có thể gán sang phòng khác — BR-02 áp dụng cả ở đường này, không chỉ lúc thêm.</summary>
    public async Task<bool> UpdateAsync(KhachThueDto tenant, string? matKhauHash, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE khach_thue
            SET phong_id = @phongId,
                ho_ten = @hoTen,
                ngay_sinh = @ngay_sinh,
                cccd = @cccd,
                so_dien_thoai = @so_dien_thoai,
                que_quan = @que_quan,
                noi_lam_viec = @noi_lam_viec,
                da_dang_ky_tam_tru = @daDangKyTamTru
            WHERE id = @id
            """;
        const string passwordSql = "UPDATE khach_thue SET mat_khau_hash = @matKhauHash WHERE id = @id";

        await using var connection = await database.OpenAsync(ct);
        await using var giao_dich = await connection.BeginTransactionAsync(ct);

        var previousRoomId = await GetRoomIdAsync(connection, giao_dich, tenant.Id, ct);
        var roomChanged = previousRoomId != tenant.PhongId;

        if (roomChanged && tenant.PhongId is { } newRoomId)
        {
            await EnsureRoomHasSpaceAsync(connection, giao_dich, newRoomId, ct);
        }

        try
        {
            await using var command = new MySqlCommand(sql, connection, giao_dich);
            AddTenantParameters(command, tenant);
            command.Parameters.AddWithValue("@id", tenant.Id);

            if (await command.ExecuteNonQueryAsync(ct) == 0)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(matKhauHash))
            {
                await using var passwordCommand = new MySqlCommand(passwordSql, connection, giao_dich);
                passwordCommand.Parameters.AddWithValue("@matKhauHash", matKhauHash);
                passwordCommand.Parameters.AddWithValue("@id", tenant.Id);
                await passwordCommand.ExecuteNonQueryAsync(ct);
            }
        }
        catch (MySqlException ex) when (ex.Number == MySqlDuplicateKey)
        {
            throw new Services.LoiNghiepVu("Số CCCD đã tồn tại trong hệ thống.");
        }

        if (roomChanged)
        {
            if (previousRoomId is { } vacatedRoomId)
            {
                await FreeRoomIfEmptyAsync(connection, giao_dich, vacatedRoomId, ct);
            }

            if (tenant.PhongId is { } occupiedRoomId)
            {
                await SetRoomStatusAsync(connection, giao_dich, occupiedRoomId, TrangThaiPhong.DaThue, ct);
            }
        }

        await giao_dich.CommitAsync(ct);
        return true;
    }

    /// <summary>Gán phong_id = NULL (đã trả phòng) và trả phòng về Trong nếu đã trống hết.</summary>
    public async Task<bool> CheckoutAsync(int khachThueId, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE khach_thue
            SET phong_id = NULL
            WHERE id = @id AND phong_id IS NOT NULL
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var giao_dich = await connection.BeginTransactionAsync(ct);

        var phongId = await GetRoomIdAsync(connection, giao_dich, khachThueId, ct);

        await using (var command = new MySqlCommand(sql, connection, giao_dich))
        {
            command.Parameters.AddWithValue("@id", khachThueId);
            if (await command.ExecuteNonQueryAsync(ct) == 0)
            {
                return false;
            }
        }

        if (phongId is { } freedRoomId)
        {
            await FreeRoomIfEmptyAsync(connection, giao_dich, freedRoomId, ct);
        }

        await giao_dich.CommitAsync(ct);
        return true;
    }

    /// <summary>US-06: chỉ xóa hồ sơ đã trả phòng. Phân biệt rõ "không có" và "còn đang ở".</summary>
    public async Task<KetQuaXoaKhachThue> DeleteAsync(int khachThueId, CancellationToken ct = default)
    {
        const string sql = """
            DELETE FROM khach_thue WHERE id = @id AND phong_id IS NULL;
            SELECT ROW_COUNT();
            """;
        const string existsSql = "SELECT COUNT(*) FROM khach_thue WHERE id = @id";

        await using var connection = await database.OpenAsync(ct);
        try
        {
            await using (var command = new MySqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@id", khachThueId);
                if (Convert.ToInt32(await command.ExecuteScalarAsync(ct)) > 0)
                {
                    return KetQuaXoaKhachThue.DaXoa;
                }
            }
        }
        catch (MySqlException ex) when (ex.Number == 1451)
        {
            throw new Services.LoiNghiepVu("Không thể xóa: người thuê còn đứng tên trên hợp đồng lưu trữ.");
        }

        await using var existsCommand = new MySqlCommand(existsSql, connection);
        existsCommand.Parameters.AddWithValue("@id", khachThueId);
        return Convert.ToInt32(await existsCommand.ExecuteScalarAsync(ct)) > 0
            ? KetQuaXoaKhachThue.ConTrongPhong
            : KetQuaXoaKhachThue.KhongTimThay;
    }

    /// <summary>Id phòng hiện tại của người thuê; null nghĩa là không tồn tại HOẶC đã trả phòng.</summary>
    public async Task<int?> GetRoomIdAsync(int khachThueId, CancellationToken ct = default)
    {
        await using var connection = await database.OpenAsync(ct);
        return await GetRoomIdAsync(connection, null, khachThueId, ct);
    }

    public async Task<bool> RoomExistsAsync(int phongId, CancellationToken ct = default)
    {
        const string sql = "SELECT COUNT(*) FROM phong WHERE id = @id";

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", phongId);
        return Convert.ToInt32(await command.ExecuteScalarAsync(ct)) > 0;
    }

    private static async Task<int?> GetRoomIdAsync(
        MySqlConnection connection, MySqlTransaction? giao_dich, int khachThueId, CancellationToken ct)
    {
        const string sql = "SELECT phong_id FROM khach_thue WHERE id = @id";

        await using var command = new MySqlCommand(sql, connection, giao_dich);
        command.Parameters.AddWithValue("@id", khachThueId);

        var value = await command.ExecuteScalarAsync(ct);
        return value is null or DBNull ? null : Convert.ToInt32(value);
    }

    /// <summary>
    /// Phòng về Trong khi không còn người thuê và không còn hợp đồng HieuLuc.
    /// Một UPDATE dùng current read thay vì snapshot REPEATABLE READ của SELECT thường;
    /// NOT EXISTS chờ giao_dich đang thêm người thuê/hợp đồng nên không bỏ sót dữ liệu vừa commit.
    /// </summary>
    private static async Task FreeRoomIfEmptyAsync(
        MySqlConnection connection, MySqlTransaction giao_dich, int phongId, CancellationToken ct)
    {
        const string sql = """
            UPDATE phong
            SET trang_thai = @trang_thai
            WHERE id = @id
              AND trang_thai <> 'BaoTri'
              AND NOT EXISTS (SELECT 1 FROM khach_thue WHERE phong_id = @id)
              AND NOT EXISTS (SELECT 1 FROM hop_dong WHERE phong_id = @id AND trang_thai = 'HieuLuc')
            """;

        await using var command = new MySqlCommand(sql, connection, giao_dich);
        command.Parameters.AddWithValue("@trang_thai", TrangThaiPhong.Trong.ToString());
        command.Parameters.AddWithValue("@id", phongId);
        await command.ExecuteNonQueryAsync(ct);
    }

    /// <summary>Không ghi đè phòng đang bảo trì.</summary>
    private static async Task SetRoomStatusAsync(
        MySqlConnection connection, MySqlTransaction? giao_dich, int phongId, TrangThaiPhong trang_thai, CancellationToken ct)
    {
        const string sql = "UPDATE phong SET trang_thai = @trang_thai WHERE id = @id AND trang_thai <> 'BaoTri'";

        await using var command = new MySqlCommand(sql, connection, giao_dich);
        command.Parameters.AddWithValue("@trang_thai", trang_thai.ToString());
        command.Parameters.AddWithValue("@id", phongId);
        await command.ExecuteNonQueryAsync(ct);
    }

    /// <summary>BR-02: đếm người hiện tại sau khi khóa phòng; ném LoiNghiepVu nếu đã đủ sức chứa.</summary>
    private static async Task EnsureRoomHasSpaceAsync(
        MySqlConnection connection, MySqlTransaction giao_dich, int phongId, CancellationToken ct)
    {
        const string sql = """
            SELECT r.so_nguoi_toi_da,
                   (SELECT COUNT(*) FROM khach_thue t WHERE t.phong_id = r.id) AS so_nguoi_hien_tai
            FROM phong r
            WHERE r.id = @id
            FOR UPDATE
            """;

        await using var command = new MySqlCommand(sql, connection, giao_dich);
        command.Parameters.AddWithValue("@id", phongId);

        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
        {
            throw new Services.LoiNghiepVu("Phòng không tồn tại.");
        }

        var soNguoiToiDa = reader.GetInt32("so_nguoi_toi_da");
        var soNguoiHienTai = Convert.ToInt32(reader.GetInt64("so_nguoi_hien_tai"));
        if (soNguoiHienTai >= soNguoiToiDa)
        {
            throw new Services.LoiNghiepVu("Phòng đã đủ sức chứa.");
        }
    }

    private static void AddTenantParameters(MySqlCommand command, KhachThueDto tenant)
    {
        command.Parameters.AddWithValue("@phongId", (object?)tenant.PhongId ?? DBNull.Value);
        command.Parameters.AddWithValue("@hoTen", tenant.HoTen);
        command.Parameters.AddWithValue("@ngay_sinh", tenant.NgaySinh.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@cccd", tenant.Cccd);
        command.Parameters.AddWithValue("@so_dien_thoai", tenant.SoDienThoai);
        command.Parameters.AddWithValue("@que_quan", tenant.QueQuan);
        command.Parameters.AddWithValue("@noi_lam_viec", (object?)tenant.NoiLamViec ?? DBNull.Value);
        command.Parameters.AddWithValue("@daDangKyTamTru", tenant.DaDangKyTamTru);
    }

    private static KhachThueDto ReadTenant(MySqlDataReader reader) =>
        new(
            reader.GetInt32("id"),
            reader.IsDBNull(reader.GetOrdinal("phong_id")) ? null : reader.GetInt32("phong_id"),
            reader.GetString("ho_ten"),
            DateOnly.FromDateTime(reader.GetDateTime("ngay_sinh")),
            reader.GetString("cccd"),
            reader.GetString("so_dien_thoai"),
            reader.GetString("que_quan"),
            reader.IsDBNull(reader.GetOrdinal("noi_lam_viec")) ? null : reader.GetString("noi_lam_viec"),
            reader.GetBoolean("da_dang_ky_tam_tru"));
}
