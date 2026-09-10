using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using QuanLyTro.Server.Data;
using QuanLyTro.Server.Repositories;
using QuanLyTro.Server.Services;

namespace QuanLyTro.Tests;

/// <summary>
/// Trả phòng trên MySQL thật (Laragon 127.0.0.1:3306). Dải id 9300-9399, tự dọn ở cả
/// TestInitialize lẫn TestCleanup nên chạy lại được nhiều lần.
/// Test race dựng thứ tự tất định: transaction thứ hai giữ người thuê mới CHƯA commit
/// (khóa S trên dòng phòng qua kiểm tra khóa ngoại), nên lần checkout bắt buộc phải chờ
/// ở bước quyết định trạng thái phòng — không phụ thuộc may rủi.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class TenantCheckoutRaceTests
{
    private const string ConnectionString =
        "Server=127.0.0.1;Port=3306;Database=quanly_phongtro_nhs;User Id=root;Password=;SslMode=None;";

    private const int MinId = 9300;
    private const int MaxId = 9399;

    private const int RaceRoomId = 9300;
    private const int RaceLeavingTenantId = 9300;   // người trả phòng
    private const int RaceArrivingTenantId = 9301;  // người vừa được thêm vào cùng phòng

    private const int LastTenantRoomId = 9310;
    private const int LastTenantId = 9310;

    private const int SharedRoomId = 9320;
    private const int SharedLeavingTenantId = 9320;
    private const int SharedStayingTenantId = 9321;

    private static readonly Database Db = new(ConnectionString);
    private static readonly TenantService Tenants = new(new TenantRepository(Db));

    [TestInitialize]
    public Task CleanupBeforeAsync() => CleanupAsync();

    [TestCleanup]
    public Task CleanupAfterAsync() => CleanupAsync();

    /// <summary>
    /// Khe hở: checkout đọc snapshot cũ (0 người) rồi ghi Available đè lên Rented mà transaction
    /// thêm người thuê vừa commit. Phòng còn người phải giữ Rented.
    /// </summary>
    [TestMethod]
    [Timeout(30_000)]
    public async Task CheckoutAsync_TenantAddedConcurrently_RoomStaysRented()
    {
        await SeedRoomAsync(RaceRoomId, "Rented");
        await SeedTenantAsync(RaceLeavingTenantId, RaceRoomId);

        await using var concurrent = await Db.OpenAsync();
        await using var transaction = await concurrent.BeginTransactionAsync();
        // Mô phỏng đúng AddAsync: khóa phòng, thêm người thuê, giữ transaction để điều khiển lúc commit.
        await using (var lockRoom = new MySqlCommand(
            "SELECT id FROM rooms WHERE id = @roomId FOR UPDATE", concurrent, transaction))
        {
            lockRoom.Parameters.AddWithValue("@roomId", RaceRoomId);
            Assert.AreEqual(RaceRoomId, Convert.ToInt32(await lockRoom.ExecuteScalarAsync()));
        }

        await using (var insert = new MySqlCommand(
            """
            INSERT INTO tenants (id, room_id, full_name, dob, id_card, phone, hometown, workplace, is_temporary_registered)
            VALUES (@id, @roomId, 'Race arriving tenant', '1995-01-01', @idCard, '0900000001', 'Đà Nẵng', NULL, 1)
            """, concurrent, transaction))
        {
            insert.Parameters.AddWithValue("@id", RaceArrivingTenantId);
            insert.Parameters.AddWithValue("@roomId", RaceRoomId);
            insert.Parameters.AddWithValue("@idCard", IdCardOf(RaceArrivingTenantId));
            await insert.ExecuteNonQueryAsync();
        }

        var checkoutTask = Task.Run(async () =>
        {
            try
            {
                return await Tenants.CheckoutAsync(RaceLeavingTenantId);
            }
            catch
            {
                return false;
            }
        });

        await WaitUntilBlockedAsync(checkoutTask);
        await transaction.CommitAsync();

        Assert.IsTrue(await checkoutTask, "Người thuê đang ở phải trả phòng được.");
        Assert.AreEqual("Rented", await RoomStatusAsync(RaceRoomId),
            "Phòng vừa có người thuê mới không được bị ghi Available.");
        Assert.AreEqual(RaceRoomId, await RoomIdOfTenantAsync(RaceArrivingTenantId),
            "Người thuê mới không được mồ côi (room_id = NULL).");
        Assert.IsNull(await RoomIdOfTenantAsync(RaceLeavingTenantId),
            "Người trả phòng phải có room_id = NULL.");
    }

    /// <summary>Đường hợp lệ: người cuối cùng của phòng trả phòng, không còn HĐ Active → phòng về Available.</summary>
    [TestMethod]
    public async Task CheckoutAsync_LastTenant_ReleasesRoomToAvailable()
    {
        await SeedRoomAsync(LastTenantRoomId, "Rented");
        await SeedTenantAsync(LastTenantId, LastTenantRoomId);

        Assert.IsTrue(await Tenants.CheckoutAsync(LastTenantId));

        Assert.AreEqual("Available", await RoomStatusAsync(LastTenantRoomId),
            "Phòng trống hết người và không còn hợp đồng Active phải về Available.");
        Assert.IsNull(await RoomIdOfTenantAsync(LastTenantId));
    }

    /// <summary>Còn người khác trong phòng → trạng thái Rented giữ nguyên.</summary>
    [TestMethod]
    public async Task CheckoutAsync_RoomStillHasOtherTenant_StaysRented()
    {
        await SeedRoomAsync(SharedRoomId, "Rented");
        await SeedTenantAsync(SharedLeavingTenantId, SharedRoomId);
        await SeedTenantAsync(SharedStayingTenantId, SharedRoomId);

        Assert.IsTrue(await Tenants.CheckoutAsync(SharedLeavingTenantId));

        Assert.AreEqual("Rented", await RoomStatusAsync(SharedRoomId),
            "Phòng còn người thuê phải giữ Rented.");
        Assert.AreEqual(SharedRoomId, await RoomIdOfTenantAsync(SharedStayingTenantId));
    }

    private static async Task SeedRoomAsync(int roomId, string status)
    {
        await using var connection = await Db.OpenAsync();
        await using var command = new MySqlCommand(
            """
            INSERT INTO rooms (id, room_number, price, max_occupants, status, description)
            VALUES (@id, @number, 1000000, 2, @status, 'TenantCheckoutRaceTests')
            """, connection);
        command.Parameters.AddWithValue("@id", roomId);
        command.Parameters.AddWithValue("@number", $"CHECKOUT-RACE-{roomId}");
        command.Parameters.AddWithValue("@status", status);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task SeedTenantAsync(int tenantId, int roomId)
    {
        await using var connection = await Db.OpenAsync();
        await using var command = new MySqlCommand(
            """
            INSERT INTO tenants (id, room_id, full_name, dob, id_card, phone, hometown, workplace, is_temporary_registered)
            VALUES (@id, @roomId, @fullName, '1995-01-01', @idCard, @phone, 'Đà Nẵng', NULL, 1)
            """, connection);
        command.Parameters.AddWithValue("@id", tenantId);
        command.Parameters.AddWithValue("@roomId", roomId);
        command.Parameters.AddWithValue("@fullName", $"Checkout race {tenantId}");
        command.Parameters.AddWithValue("@idCard", IdCardOf(tenantId));
        command.Parameters.AddWithValue("@phone", $"09{tenantId:D8}");
        await command.ExecuteNonQueryAsync();
    }

    private static string IdCardOf(int tenantId) => $"98{tenantId:D10}";

    /// <summary>Chờ tới khi checkout đang nằm trong hàng đợi khóa: đã qua bước đọc, đang chờ khóa dòng phòng.</summary>
    private static async Task WaitUntilBlockedAsync(Task task)
    {
        for (var attempt = 0; attempt < 100 && !task.IsCompleted; attempt++)
        {
            await Task.Delay(50);
        }

        Assert.IsFalse(task.IsCompleted, "Checkout phải đang chờ khóa dòng phòng của transaction kia.");
    }

    private static async Task<string?> RoomStatusAsync(int roomId)
    {
        await using var connection = await Db.OpenAsync();
        await using var command = new MySqlCommand("SELECT status FROM rooms WHERE id = @id", connection);
        command.Parameters.AddWithValue("@id", roomId);
        return (string?)await command.ExecuteScalarAsync();
    }

    private static async Task<int?> RoomIdOfTenantAsync(int tenantId)
    {
        await using var connection = await Db.OpenAsync();
        await using var command = new MySqlCommand("SELECT room_id FROM tenants WHERE id = @id", connection);
        command.Parameters.AddWithValue("@id", tenantId);
        var value = await command.ExecuteScalarAsync();
        return value is null or DBNull ? null : Convert.ToInt32(value);
    }

    /// <summary>Dọn theo thứ tự an toàn khóa ngoại: invoices → utility_readings → contracts → tenants → rooms.</summary>
    private static async Task CleanupAsync()
    {
        await using var connection = await Db.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        foreach (var sql in new[]
        {
            "DELETE FROM invoices WHERE room_id BETWEEN @minId AND @maxId",
            "DELETE FROM utility_readings WHERE room_id BETWEEN @minId AND @maxId",
            "DELETE FROM contracts WHERE room_id BETWEEN @minId AND @maxId",
            "DELETE FROM tenants WHERE id BETWEEN @minId AND @maxId OR id_card IN (@card0, @card1, @card2, @card3)",
            "DELETE FROM rooms WHERE id BETWEEN @minId AND @maxId",
        })
        {
            await using var command = new MySqlCommand(sql, connection, transaction);
            command.Parameters.AddWithValue("@minId", MinId);
            command.Parameters.AddWithValue("@maxId", MaxId);
            command.Parameters.AddWithValue("@card0", IdCardOf(RaceLeavingTenantId));
            command.Parameters.AddWithValue("@card1", IdCardOf(RaceArrivingTenantId));
            command.Parameters.AddWithValue("@card2", IdCardOf(LastTenantId));
            command.Parameters.AddWithValue("@card3", IdCardOf(SharedStayingTenantId));
            await command.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }
}
