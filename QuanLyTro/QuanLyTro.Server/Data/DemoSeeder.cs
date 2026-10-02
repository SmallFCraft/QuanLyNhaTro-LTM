using MySqlConnector;
using QuanLyTro.Server.Security;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Server.Data;

/// <summary>
/// Dữ liệu mẫu để demo: mọi tài khoản có MẬT KHẨU TRÙNG TÊN ĐĂNG NHẬP.
///
/// - `landlord` / `landlord` (bảng users, vai Landlord)
/// - `police`   / `police`   (bảng users, vai Police)
/// - `100000000001` / `100000000001` (bảng tenants — người thuê đăng nhập bằng CCCD)
///
/// Chạy: dotnet run --project QuanLyTro/QuanLyTro.Server -- --seed-demo
/// Idempotent: chạy lại chỉ ghi đè đúng các dòng demo, không nhân bản.
/// Dải id 7001-7009, CCCD 1000000000xx — không đụng dải test (9188-9400, 990001+).
/// </summary>
public static class DemoSeeder
{
    public const string LandlordUsername = "landlord";
    public const string PoliceUsername = "police";
    public const string ManagerUsername = "manager";
    public const string TenantIdCard = "100000000001";

    private const int RoomId = 7001;
    private const int TenantId = 7001;
    private const int ContractId = 7001;
    private const int ReadingId = 7001;
    private const int InvoiceId = 7001;

    public static async Task SeedAsync(Database database, CancellationToken ct = default)
    {
        // Luôn bảo đảm DB schema và enum mới nhất đã được áp dụng trước khi gieo dữ liệu.
        await new SchemaInitializer(database).InitializeAsync(ct);

        await using var connection = await database.OpenAsync(ct);
        var month = DateTime.UtcNow.ToString("yyyy-MM", System.Globalization.CultureInfo.InvariantCulture);

        // users: username là khóa tự nhiên (UNIQUE), để id tự tăng — không tranh id với dữ liệu sẵn có.
        await UpsertUserAsync(connection, LandlordUsername, "Chủ Trọ Demo", "Landlord", ct);
        await UpsertUserAsync(connection, PoliceUsername, "Công An Phường Demo", "Police", ct);
        await UpsertUserAsync(connection, ManagerUsername, "Quản Lý Demo", "Manager", ct);

        // Gieo quyền mặc định vào role_permissions (Manager / Police / Tenant).
        await SeedDefaultPermissionsAsync(connection, ct);

        // Phòng cho thuê có người ở (kèm 2 phòng trống để màn Quản lý phòng có dữ liệu).
        await ExecuteAsync(connection, """
            INSERT INTO rooms (id, room_number, price, max_occupants, status, description)
            VALUES
                (7001, 'P201', 2000000, 2, 'Rented',    'Phòng mẫu đang cho thuê'),
                (7002, 'P202', 2200000, 3, 'Available', 'Phòng mẫu còn trống'),
                (7003, 'P203', 1800000, 2, 'Available', 'Phòng mẫu còn trống')
            ON DUPLICATE KEY UPDATE
                room_number = VALUES(room_number), price = VALUES(price),
                max_occupants = VALUES(max_occupants), status = VALUES(status),
                description = VALUES(description)
            """, ct);

        // Người thuê: mật khẩu = chính số CCCD (trùng tên đăng nhập).
        await ExecuteAsync(connection, """
            INSERT INTO tenants (id, room_id, full_name, dob, id_card, password_hash, phone, hometown, workplace, is_temporary_registered)
            VALUES (7001, 7001, 'Nguyễn Văn Demo', '2001-05-10', @idCard, @hash, '0900000001', 'Đà Nẵng', 'Sinh viên', TRUE)
            ON DUPLICATE KEY UPDATE
                room_id = VALUES(room_id), full_name = VALUES(full_name), dob = VALUES(dob),
                password_hash = VALUES(password_hash), phone = VALUES(phone),
                hometown = VALUES(hometown), workplace = VALUES(workplace),
                is_temporary_registered = VALUES(is_temporary_registered)
            """, ct,
            ("@idCard", TenantIdCard),
            ("@hash", PasswordHasher.Hash(TenantIdCard)));

        await ExecuteAsync(connection, """
            INSERT INTO contracts (id, room_id, representative_tenant_id, start_date, end_date, rental_price, deposit_amount, status, notes)
            VALUES (7001, 7001, 7001, '2026-01-01', '2026-12-31', 2000000, 1000000, 'Active', 'Hợp đồng mẫu')
            ON DUPLICATE KEY UPDATE
                room_id = VALUES(room_id), representative_tenant_id = VALUES(representative_tenant_id),
                start_date = VALUES(start_date), end_date = VALUES(end_date),
                rental_price = VALUES(rental_price), deposit_amount = VALUES(deposit_amount),
                status = VALUES(status), notes = VALUES(notes)
            """, ct);

        // Chỉ số điện nước kỳ hiện tại: 1.230→1.280 kWh, 14→16 m³.
        await ExecuteAsync(connection, """
            INSERT INTO utility_readings (id, room_id, billing_month, old_electricity, new_electricity, electricity_rate, old_water, new_water, water_rate)
            VALUES (7001, 7001, @month, 1230, 1280, 3500, 14, 16, 10000)
            ON DUPLICATE KEY UPDATE
                old_electricity = VALUES(old_electricity), new_electricity = VALUES(new_electricity),
                electricity_rate = VALUES(electricity_rate), old_water = VALUES(old_water),
                new_water = VALUES(new_water), water_rate = VALUES(water_rate)
            """, ct, ("@month", month));

        // Hóa đơn chưa thu: 2.000.000 + 175.000 + 20.000 + 50.000 = 2.245.000 đ.
        await ExecuteAsync(connection, """
            INSERT INTO invoices (id, room_id, contract_id, billing_month, room_amount, electricity_amount, water_amount, other_fees, total_amount, status)
            VALUES (7001, 7001, 7001, @month, 2000000, 175000, 20000, 50000, 2245000, 'Unpaid')
            ON DUPLICATE KEY UPDATE
                room_id = VALUES(room_id), contract_id = VALUES(contract_id),
                room_amount = VALUES(room_amount), electricity_amount = VALUES(electricity_amount),
                water_amount = VALUES(water_amount), other_fees = VALUES(other_fees),
                total_amount = VALUES(total_amount), status = VALUES(status)
            """, ct, ("@month", month));
    }

    /// <summary>Mật khẩu = username, đúng quy ước "trùng nhau" của bộ dữ liệu mẫu.</summary>
    private static Task UpsertUserAsync(
        MySqlConnection connection, string username, string fullName, string role, CancellationToken ct)
    {
        const string sql = """
            INSERT INTO users (username, password_hash, full_name, role)
            VALUES (@username, @hash, @fullName, @role)
            ON DUPLICATE KEY UPDATE
                password_hash = VALUES(password_hash), full_name = VALUES(full_name), role = VALUES(role)
            """;

        return ExecuteAsync(connection, sql, ct,
            ("@username", username),
            ("@hash", PasswordHasher.Hash(username)),
            ("@fullName", fullName),
            ("@role", role));
    }

    private static async Task SeedDefaultPermissionsAsync(MySqlConnection connection, CancellationToken ct)
    {
        // Kiểm tra theo từng vai trò: nếu bảng đã có dữ liệu nhưng thiếu vai trò mới (ví dụ Manager vừa bổ sung),
        // vẫn gieo đủ cho vai trò đó mà không đè cấu hình của vai trò đã tùy biến.
        const string checkRoleSql = "SELECT COUNT(*) FROM role_permissions WHERE role = @role";
        const string insertSql = "INSERT IGNORE INTO role_permissions (role, action) VALUES (@role, @action)";

        foreach (var (role, actions) in DefaultRolePermissions.All)
        {
            await using var checkCmd = new MySqlCommand(checkRoleSql, connection);
            checkCmd.Parameters.AddWithValue("@role", role);
            var count = Convert.ToInt64(await checkCmd.ExecuteScalarAsync(ct));
            if (count > 0)
            {
                // Vai trò này đã có dòng trong DB — không ghi đè cấu hình hiện tại.
                continue;
            }

            foreach (var action in actions)
            {
                await ExecuteAsync(connection, insertSql, ct, ("@role", role), ("@action", action));
            }
        }
    }

    private static async Task ExecuteAsync(
        MySqlConnection connection, string sql, CancellationToken ct, params (string Name, object Value)[] parameters)
    {
        await using var command = new MySqlCommand(sql, connection);
        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        await command.ExecuteNonQueryAsync(ct);
    }
}