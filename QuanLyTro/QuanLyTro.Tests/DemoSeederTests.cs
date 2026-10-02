using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Server.Data;

namespace QuanLyTro.Tests;

/// <summary>
/// Dữ liệu mẫu trên MySQL thật. Quy ước: mật khẩu TRÙNG tên đăng nhập cho mọi tài khoản.
/// Idempotent — chạy nhiều lần vẫn đúng số dòng. Dải id 7001-7009, CCCD 1000000000xx.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class DemoSeederTests
{
    private const string ConnectionString =
        "Server=127.0.0.1;Port=3306;Database=quanly_phongtro_nhs;User Id=root;Password=;SslMode=None;";

    private static readonly Database Db = new(ConnectionString);

    [TestInitialize]
    public Task SeedAsync() => DemoSeeder.SeedAsync(Db);

    [TestMethod]
    public async Task Seed_CreatesManagerAccount_WithPasswordMatchingUsername()
    {
        var account = await new QuanLyTro.Server.Repositories.UserRepository(Db)
            .FindByUsernameAsync(DemoSeeder.ManagerUsername);

        Assert.IsNotNull(account, "Thiếu tài khoản demo manager");
        Assert.AreEqual("Manager", account.Role.ToString(), "Sai vai cho manager");
        Assert.IsTrue(
            QuanLyTro.Server.Security.PasswordHasher.Verify(DemoSeeder.ManagerUsername, account.PasswordHash),
            "Mật khẩu của manager phải bằng chính tên đăng nhập.");
    }

    [TestMethod]
    public async Task Seed_CreatesLandlordAndPoliceAccounts_WithPasswordMatchingUsername()
    {
        foreach (var (username, role) in new[]
                 {
                     (DemoSeeder.LandlordUsername, "Landlord"),
                     (DemoSeeder.PoliceUsername, "Police"),
                 })
        {
            var account = await new QuanLyTro.Server.Repositories.UserRepository(Db)
                .FindByUsernameAsync(username);

            Assert.IsNotNull(account, $"Thiếu tài khoản demo {username}");
            Assert.AreEqual(role, account.Role.ToString(), $"Sai vai cho {username}");
            Assert.IsTrue(
                QuanLyTro.Server.Security.PasswordHasher.Verify(username, account.PasswordHash),
                $"Mật khẩu của {username} phải bằng chính tên đăng nhập.");
        }
    }

    [TestMethod]
    public async Task Seed_CreatesTenantLoggingInByIdCard_WithPasswordMatchingIdCard()
    {
        var tenant = await new QuanLyTro.Server.Repositories.TenantAuthRepository(Db)
            .FindByCccdAsync(DemoSeeder.TenantIdCard);

        Assert.IsNotNull(tenant, "Phải có người thuê mẫu đăng nhập được bằng CCCD.");
        Assert.AreEqual(7001, tenant.RoomId, "Người thuê mẫu phải ở phòng 7001.");
        Assert.IsTrue(
            QuanLyTro.Server.Security.PasswordHasher.Verify(DemoSeeder.TenantIdCard, tenant.PasswordHash),
            "Mật khẩu người thuê phải trùng số CCCD (tên đăng nhập).");
    }

    [TestMethod]
    public async Task Seed_IsIdempotent_SecondRunDoesNotDuplicate()
    {
        await DemoSeeder.SeedAsync(Db);
        await DemoSeeder.SeedAsync(Db);

        foreach (var table in new[] { "rooms", "tenants", "contracts", "utility_readings", "invoices" })
        {
            var count = Convert.ToInt32(await ScalarAsync($"SELECT COUNT(*) FROM {table} WHERE id BETWEEN 7001 AND 7009"));
            var expected = table switch
            {
                "rooms" => 3,
                _ => 1,
            };

            Assert.AreEqual((int)expected, count, $"{table} bị nhân bản khi seed lại.");
        }
    }

    [TestMethod]
    public async Task Seed_InvoiceMatchesRoomContractAndCurrentMonth()
    {
        var columns = await ScalarAsync("""
            SELECT CONCAT_WS('|', i.billing_month, i.total_amount, i.status, r.room_number, c.status)
            FROM invoices i
            JOIN rooms r ON r.id = i.room_id
            JOIN contracts c ON c.id = i.contract_id
            WHERE i.id = 7001
            """);

        var expected = $"{DateTime.UtcNow:yyyy-MM}|2245000.00|Unpaid|P201|Active";
        Assert.AreEqual(expected, columns, "Hóa đơn mẫu phải khớp phòng P201, hợp đồng Active, kỳ hiện tại.");
    }

    private async Task<string> ScalarAsync(string sql)
    {
        await using var connection = await Db.OpenAsync();
        await using var command = new MySqlConnector.MySqlCommand(sql, connection);
        return Convert.ToString(await command.ExecuteScalarAsync()) ?? string.Empty;
    }
}
