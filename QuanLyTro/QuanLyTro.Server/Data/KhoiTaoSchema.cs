using System.Reflection;
using MySqlConnector;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Server.Data;

/// <summary>
/// Đọc embedded resource `database/schema.sql`, tách theo marker "-- statement"
/// và chạy tuần tự. Idempotent: CREATE TABLE IF NOT EXISTS nên chạy lại vẫn thành công.
/// </summary>
public sealed class KhoiTaoSchema(Database database)
{
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        var script = ReadEmbeddedSchema();

        await using var connection = await database.OpenServerLevelAsync(ct);
        foreach (var statement in SplitStatements(script))
        {
            await using var command = connection.CreateCommand();
            command.CommandText = statement;
            await command.ExecuteNonQueryAsync(ct);
        }

        await EnsureMigrationColumnsAsync(connection, ct);
        await EnsureHopDongQrColumnsAsync(connection, ct);
        await EnsureRolePermissionsAsync(connection, ct);
    }

    /// <summary>
    /// Bổ sung các hanh_dong mặc định MỚI vào bảng `quyen_vai_tro`. Bảng này là nguồn duy nhất
    /// nạp vào <see cref="Network.MaTranPhanQuyen"/> lúc Server khởi động — thiếu dòng nghĩa là
    /// vai trò đó bị "Không có quyền." dù code đã khai báo quyền.
    ///
    /// Sổ cái `quyen_mac_dinh_da_ap_dung` ghi nhớ từng cặp (vai_tro, hanh_dong) đã từng gieo:
    /// Chủ trọ thu hồi quyền chỉ xóa khỏi `quyen_vai_tro`, cặp đó vẫn nằm trong sổ cái nên
    /// lần khởi động sau KHÔNG hồi sinh quyền đã thu hồi.
    /// </summary>
    private static async Task EnsureRolePermissionsAsync(MySqlConnection connection, CancellationToken ct)
    {
        const string createLedgerSql = """
            CREATE TABLE IF NOT EXISTS quyen_mac_dinh_da_ap_dung (
                vai_tro VARCHAR(20) NOT NULL,
                hanh_dong VARCHAR(50) NOT NULL,
                ngay_tao DATETIME DEFAULT CURRENT_TIMESTAMP,
                PRIMARY KEY (vai_tro, hanh_dong)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
            """;
        await using (var createCmd = connection.CreateCommand())
        {
            createCmd.CommandText = createLedgerSql;
            await createCmd.ExecuteNonQueryAsync(ct);
        }

        // Sổ cái rỗng (DB có trước khi có tính năng này): coi mọi dòng ĐANG CÓ trong
        // quyen_vai_tro là "đã gieo" để không ghi đè cấu hình Chủ trọ đã tùy biến thu hồi.
        const string backfillLedgerSql = """
            INSERT IGNORE INTO quyen_mac_dinh_da_ap_dung (vai_tro, hanh_dong)
            SELECT vai_tro, hanh_dong FROM quyen_vai_tro;
            """;
        await using (var backfillCmd = connection.CreateCommand())
        {
            backfillCmd.CommandText = backfillLedgerSql;
            await backfillCmd.ExecuteNonQueryAsync(ct);
        }

        // 1 câu đọc toàn bộ sổ cái (~60 dòng) + INSERT batch 2 lần = 3 round-trip
        // thay vì N round-trip SELECT/INSERT từng cặp.
        var da_gieo = new HashSet<(string VaiTro, string HanhDong)>();
        await using (var ledgerReadCmd = connection.CreateCommand())
        {
            ledgerReadCmd.CommandText = "SELECT vai_tro, hanh_dong FROM quyen_mac_dinh_da_ap_dung";
            await using var reader = await ledgerReadCmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                da_gieo.Add((reader.GetString(0), reader.GetString(1)));
            }
        }

        var missing = QuyenMacDinhTheoVaiTro.All
            .SelectMany(p => p.Value.Select(a => (VaiTro: p.Key, HanhDong: a)))
            .Where(p => !da_gieo.Contains(p))
            .ToList();

        if (missing.Count == 0)
        {
            return;
        }

        foreach (var table in new[] { "quyen_vai_tro", "quyen_mac_dinh_da_ap_dung" })
        {
            foreach (var chunk in missing.Chunk(400))
            {
                var values = string.Join(", ", chunk.Select((_, i) => $"(@vt{i}, @hd{i})"));
                await using var cmd = connection.CreateCommand();
                cmd.CommandText = $"INSERT IGNORE INTO {table} (vai_tro, hanh_dong) VALUES {values}";
                for (var i = 0; i < chunk.Length; i++)
                {
                    cmd.Parameters.AddWithValue($"@vt{i}", chunk[i].VaiTro);
                    cmd.Parameters.AddWithValue($"@hd{i}", chunk[i].HanhDong);
                }

                await cmd.ExecuteNonQueryAsync(ct);
            }
        }
    }

    /// <summary>
    /// BR-18: bảng hop_dong cũ chưa có cột QR/PIN và ENUM thiếu 'ChoNhanPhong'.
    /// Chạy idempotent theo information_schema thay vì ADD COLUMN IF NOT EXISTS
    /// (MySQL 8.0 không hỗ trợ IF NOT EXISTS cho ADD COLUMN).
    /// </summary>
    private static async Task EnsureHopDongQrColumnsAsync(MySqlConnection connection, CancellationToken ct)
    {
        // Kiểm tra TỪNG cột + enum riêng: DB đã có ma_qr_token nhưng thiếu ma_pin hoặc thiếu
        // giá trị enum 'ChoNhanPhong' thì vẫn phải ALTER (không được return sớm).
        if (!await HasHopDongColumnAsync(connection, "ma_qr_token", ct))
        {
            await ExecAsync(connection, """
                ALTER TABLE hop_dong
                  ADD COLUMN ma_qr_token VARCHAR(64) NULL UNIQUE,
                  ADD COLUMN ma_pin VARCHAR(8) NULL;
                """, ct);
        }
        else if (!await HasHopDongColumnAsync(connection, "ma_pin", ct))
        {
            await ExecAsync(connection, "ALTER TABLE hop_dong ADD COLUMN ma_pin VARCHAR(8) NULL", ct);
        }

        // ENUM có chứa 'ChoNhanPhong' chưa? (MODIFY idempotent, chỉ chạy khi thiếu giá trị)
        await using var enumCmd = connection.CreateCommand();
        enumCmd.CommandText = """
            SELECT COUNT(*)
            FROM information_schema.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
              AND TABLE_NAME = 'hop_dong'
              AND COLUMN_NAME = 'trang_thai'
              AND COLUMN_TYPE LIKE '%ChoNhanPhong%';
            """;
        if (Convert.ToInt32(await enumCmd.ExecuteScalarAsync(ct)) == 0)
        {
            await ExecAsync(connection,
                "ALTER TABLE hop_dong MODIFY COLUMN trang_thai ENUM('ChoNhanPhong', 'HieuLuc', 'HetHan', 'ChamDut') DEFAULT 'HieuLuc'",
                ct);
        }
    }

    private static async Task<bool> HasHopDongColumnAsync(MySqlConnection connection, string column, CancellationToken ct)
    {
        await using var checkCmd = connection.CreateCommand();
        checkCmd.CommandText = """
            SELECT COUNT(*)
            FROM information_schema.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
              AND TABLE_NAME = 'hop_dong'
              AND COLUMN_NAME = @column;
            """;
        checkCmd.Parameters.AddWithValue("@column", column);
        return Convert.ToInt32(await checkCmd.ExecuteScalarAsync(ct)) > 0;
    }

    private static async Task ExecAsync(MySqlConnection connection, string sql, CancellationToken ct)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static async Task EnsureMigrationColumnsAsync(MySqlConnection connection, CancellationToken ct)
    {
        const string checkSql = """
            SELECT COUNT(*)
            FROM information_schema.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
              AND TABLE_NAME = 'chi_so_dien_nuoc'
              AND COLUMN_NAME = 'hinh_thuc_nuoc';
            """;
        await using var checkCmd = connection.CreateCommand();
        checkCmd.CommandText = checkSql;
        var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync(ct));
        if (count == 0)
        {
            const string alterSql = """
                ALTER TABLE chi_so_dien_nuoc
                ADD COLUMN hinh_thuc_nuoc VARCHAR(10) NOT NULL DEFAULT 'Khoi',
                ADD COLUMN so_nguoi_nuoc INT NOT NULL DEFAULT 0;
                """;
            await using var alterCmd = connection.CreateCommand();
            alterCmd.CommandText = alterSql;
            await alterCmd.ExecuteNonQueryAsync(ct);
        }
    }

    public static string ReadEmbeddedSchema()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .Single(n => n.EndsWith("schema.sql", StringComparison.OrdinalIgnoreCase));

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource không tìm thấy: {resourceName}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Tách script thành từng statement. Chỉ nhận marker nằm trên DÒNG RIÊNG — marker trong
    /// comment mô tả (ví dụ chính dòng mô tả trong schema.sql) không được tính.
    /// </summary>
    public static IEnumerable<string> SplitStatements(string script)
    {
        var statements = new List<string>();
        var buffer = new List<string>();

        foreach (var line in script.Split('\n'))
        {
            if (line.Trim() == "-- statement")
            {
                Flush(statements, buffer);
                continue;
            }

            buffer.Add(line);
        }

        Flush(statements, buffer);
        return statements;
    }

    private static void Flush(List<string> statements, List<string> buffer)
    {
        var text = string.Join('\n', buffer).Trim();
        buffer.Clear();

        if (text.Length > 0)
        {
            statements.Add(text);
        }
    }
}
