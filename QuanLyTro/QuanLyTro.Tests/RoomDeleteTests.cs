using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using QuanLyTro.Server.Data;
using QuanLyTro.Server.Repositories;
using QuanLyTro.Server.Services;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Tests;

/// <summary>
/// BR-12 trên MySQL thật (Laragon 127.0.0.1:3306). Dữ liệu nằm trong dải id 9200-9203 và
/// tự dọn trong cả TestInitialize lẫn TestCleanup để chạy lại được nhiều lần.
/// Hai test race tái hiện khe hở "kiểm tra rồi mới xóa": chèn con (người thuê / hợp đồng)
/// chưa commit giữ khóa S trên dòng phòng qua kiểm tra khóa ngoại, nên lần xóa chắc chắn
/// kẹt ở giữa bước kiểm tra và bước DELETE — thứ tự tất định, không phụ thuộc may rủi.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class RoomDeleteTests
{
    private const string ConnectionString =
        "Server=127.0.0.1;Port=3306;Database=quanly_phongtro_nhs;User Id=root;Password=;SslMode=None;";
    private const string OccupiedMessage =
        "Không thể xóa: phòng còn người thuê hoặc hợp đồng đang hiệu lực.";

    private const int MinId = 9200;
    private const int MaxId = 9203;

    private const int EmptyRoomId = 9200;
    private const int TenantRoomId = 9201;
    private const int ContractRoomId = 9202;
    private const int RaceRoomId = 9203;

    private const int TenantId = 9200;
    private const int ContractTenantId = 9201;
    private const int ContractRaceTenantId = 9202;
    private const int TenantRaceTenantId = 9203;

    private static readonly Database Db = new(ConnectionString);
    private static readonly RoomService Rooms = new(new RoomRepository(Db));

    [TestInitialize]
    public Task CleanupBeforeAsync() => CleanupAsync();

    [TestCleanup]
    public Task CleanupAfterAsync() => CleanupAsync();

    [TestMethod]
    public async Task DeleteAsync_EmptyRoom_DeletesSuccessfully()
    {
        await SeedRoomAsync(EmptyRoomId);

        Assert.IsTrue(await Rooms.DeleteAsync(EmptyRoomId));
        Assert.IsFalse(await RoomExistsAsync(EmptyRoomId));
    }

    [TestMethod]
    public async Task DeleteAsync_RoomWithTenant_RejectsExactMessage()
    {
        await SeedRoomAsync(TenantRoomId);
        await SeedTenantAsync(TenantId, TenantRoomId);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => Rooms.DeleteAsync(TenantRoomId));

        Assert.AreEqual(OccupiedMessage, ex.Message);
        Assert.IsTrue(await RoomExistsAsync(TenantRoomId));
    }

    [TestMethod]
    public async Task DeleteAsync_RoomWithActiveContract_RejectsExactMessage()
    {
        await SeedRoomAsync(ContractRoomId);
        await SeedTenantAsync(ContractTenantId, null);
        await SeedContractAsync(ContractRoomId, ContractTenantId);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => Rooms.DeleteAsync(ContractRoomId));

        Assert.AreEqual(OccupiedMessage, ex.Message);
        Assert.IsTrue(await RoomExistsAsync(ContractRoomId));
    }

    /// <summary>Người thuê thêm vào sau bước kiểm tra nhưng trước DELETE: phòng không được biến mất.</summary>
    [TestMethod]
    [Timeout(30_000)]
    public async Task DeleteAsync_TenantAddedAfterCheck_DoesNotDeleteOrOrphan()
    {
        await SeedRoomAsync(RaceRoomId);

        await using var concurrent = await Db.OpenAsync();
        await using var transaction = await concurrent.BeginTransactionAsync();
        await using (var insert = new MySqlCommand(
            """
            INSERT INTO tenants (id, room_id, full_name, dob, id_card, phone, hometown, workplace, is_temporary_registered)
            VALUES (@id, @roomId, 'Race tenant', '1995-01-01', @idCard, '0900000003', 'Đà Nẵng', NULL, 1)
            """, concurrent, transaction))
        {
            insert.Parameters.AddWithValue("@id", TenantRaceTenantId);
            insert.Parameters.AddWithValue("@roomId", RaceRoomId);
            insert.Parameters.AddWithValue("@idCard", IdCardOf(TenantRaceTenantId));
            await insert.ExecuteNonQueryAsync();
        }

        Exception? deleteError = null;
        var deleteTask = Task.Run(async () =>
        {
            try
            {
                return await Rooms.DeleteAsync(RaceRoomId);
            }
            catch (Exception ex)
            {
                deleteError = ex;
                return false;
            }
        });

        await WaitUntilBlockedAsync(deleteTask);
        await transaction.CommitAsync();

        var deleteSucceeded = await deleteTask;

        Assert.IsTrue(await RoomExistsAsync(RaceRoomId), "Phòng vừa có người thuê không được bị xóa.");
        Assert.AreEqual(RaceRoomId, await RoomIdOfTenantAsync(TenantRaceTenantId),
            "Người thuê vừa thêm không được bị mồ côi (room_id = NULL).");
        Assert.IsFalse(deleteSucceeded, "Lần xóa phải thất bại thay vì xóa phòng đang có người.");
        Assert.IsInstanceOfType<BusinessRuleException>(deleteError, $"Lỗi trả về: {deleteError}");
        Assert.AreEqual(OccupiedMessage, deleteError!.Message);
    }

    /// <summary>Hợp đồng Active thêm vào sau bước kiểm tra: phải trả lỗi nghiệp vụ, không phải MySQL 1451 thô.</summary>
    [TestMethod]
    [Timeout(30_000)]
    public async Task DeleteAsync_ContractAddedAfterCheck_RejectsWithBusinessRule()
    {
        await SeedRoomAsync(RaceRoomId);
        await SeedTenantAsync(ContractRaceTenantId, null);

        await using var concurrent = await Db.OpenAsync();
        await using var transaction = await concurrent.BeginTransactionAsync();
        await using (var insert = new MySqlCommand(
            """
            INSERT INTO contracts (room_id, representative_tenant_id, start_date, end_date,
                                   rental_price, deposit_amount, status, notes)
            VALUES (@roomId, @tenantId, '2026-01-01', '2026-12-31', 1000000, 0, 'Active', 'RoomDeleteTests race')
            """, concurrent, transaction))
        {
            insert.Parameters.AddWithValue("@roomId", RaceRoomId);
            insert.Parameters.AddWithValue("@tenantId", ContractRaceTenantId);
            await insert.ExecuteNonQueryAsync();
        }

        Exception? deleteError = null;
        var deleteTask = Task.Run(async () =>
        {
            try
            {
                return await Rooms.DeleteAsync(RaceRoomId);
            }
            catch (Exception ex)
            {
                deleteError = ex;
                return false;
            }
        });

        await WaitUntilBlockedAsync(deleteTask);
        await transaction.CommitAsync();

        var deleteSucceeded = await deleteTask;

        Assert.IsTrue(await RoomExistsAsync(RaceRoomId), "Phòng đang có hợp đồng không được bị xóa.");
        Assert.IsFalse(deleteSucceeded, "Lần xóa phải thất bại thay vì xóa phòng đang có hợp đồng.");
        Assert.IsInstanceOfType<BusinessRuleException>(deleteError, $"Lỗi trả về: {deleteError}");
        Assert.AreEqual(OccupiedMessage, deleteError!.Message);
    }

    private static async Task SeedRoomAsync(int roomId)
    {
        await using var connection = await Db.OpenAsync();
        await using var command = new MySqlCommand(
            """
            INSERT INTO rooms (id, room_number, price, max_occupants, status, description)
            VALUES (@id, @number, 1000000, 2, 'Available', 'RoomDeleteTests')
            """, connection);
        command.Parameters.AddWithValue("@id", roomId);
        command.Parameters.AddWithValue("@number", $"DELETE-TEST-{roomId}");
        await command.ExecuteNonQueryAsync();
    }

    private static async Task SeedTenantAsync(int tenantId, int? roomId)
    {
        await using var connection = await Db.OpenAsync();
        await using var command = new MySqlCommand(
            """
            INSERT INTO tenants (id, room_id, full_name, dob, id_card, phone, hometown, workplace, is_temporary_registered)
            VALUES (@id, @roomId, @fullName, '1995-01-01', @idCard, @phone, 'Đà Nẵng', NULL, 1)
            """, connection);
        command.Parameters.AddWithValue("@id", tenantId);
        command.Parameters.AddWithValue("@roomId", (object?)roomId ?? DBNull.Value);
        command.Parameters.AddWithValue("@fullName", $"Delete test {tenantId}");
        command.Parameters.AddWithValue("@idCard", IdCardOf(tenantId));
        command.Parameters.AddWithValue("@phone", $"09{tenantId:D8}");
        await command.ExecuteNonQueryAsync();
    }

    private static string IdCardOf(int tenantId) => $"98{tenantId:D10}";

    private static async Task SeedContractAsync(int roomId, int tenantId)
    {
        await using var connection = await Db.OpenAsync();
        await using var command = new MySqlCommand(
            """
            INSERT INTO contracts (room_id, representative_tenant_id, start_date, end_date,
                                   rental_price, deposit_amount, status, notes)
            VALUES (@roomId, @tenantId, '2026-01-01', '2026-12-31', 1000000, 0, 'Active', 'RoomDeleteTests')
            """, connection);
        command.Parameters.AddWithValue("@roomId", roomId);
        command.Parameters.AddWithValue("@tenantId", tenantId);
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Chờ tới khi lần xóa đang nằm trong hàng đợi khóa của MySQL: bước kiểm tra đã đi qua,
    /// DELETE (hoặc FOR UPDATE) đang chờ khóa S do transaction chưa commit giữ trên dòng phòng.
    /// </summary>
    private static async Task WaitUntilBlockedAsync(Task task)
    {
        for (var attempt = 0; attempt < 100 && !task.IsCompleted; attempt++)
        {
            await Task.Delay(50);
        }

        Assert.IsFalse(task.IsCompleted, "Lần xóa phải đang chờ khóa dòng phòng.");
    }

    private static async Task<bool> RoomExistsAsync(int roomId)
    {
        await using var connection = await Db.OpenAsync();
        await using var command = new MySqlCommand(
            "SELECT COUNT(*) FROM rooms WHERE id = @id", connection);
        command.Parameters.AddWithValue("@id", roomId);
        return Convert.ToInt64(await command.ExecuteScalarAsync()) > 0;
    }

    private static async Task<int?> RoomIdOfTenantAsync(int tenantId)
    {
        await using var connection = await Db.OpenAsync();
        await using var command = new MySqlCommand(
            "SELECT room_id FROM tenants WHERE id = @id", connection);
        command.Parameters.AddWithValue("@id", tenantId);
        var value = await command.ExecuteScalarAsync();
        return value is null or DBNull ? null : Convert.ToInt32(value);
    }

    private static async Task CleanupAsync()
    {
        await using var connection = await Db.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        foreach (var sql in new[]
        {
            "DELETE FROM invoices WHERE room_id BETWEEN @minId AND @maxId",
            "DELETE FROM utility_readings WHERE room_id BETWEEN @minId AND @maxId",
            "DELETE FROM contracts WHERE room_id BETWEEN @minId AND @maxId",
            // Người thuê do service tự cấp id (auto_increment) nên dọn theo CCCD, không theo id.
            "DELETE FROM tenants WHERE id_card IN (@card0, @card1, @card2, @card3)",
            "DELETE FROM rooms WHERE id BETWEEN @minId AND @maxId",
        })
        {
            await using var command = new MySqlCommand(sql, connection, transaction);
            command.Parameters.AddWithValue("@minId", MinId);
            command.Parameters.AddWithValue("@maxId", MaxId);
            command.Parameters.AddWithValue("@card0", IdCardOf(TenantId));
            command.Parameters.AddWithValue("@card1", IdCardOf(ContractTenantId));
            command.Parameters.AddWithValue("@card2", IdCardOf(ContractRaceTenantId));
            command.Parameters.AddWithValue("@card3", IdCardOf(TenantRaceTenantId));
            await command.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }
}
