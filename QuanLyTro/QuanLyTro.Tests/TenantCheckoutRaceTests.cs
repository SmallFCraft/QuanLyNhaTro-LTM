using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using QuanLyTro.Server.Data;
using QuanLyTro.Server.Repositories;
using QuanLyTro.Server.Services;

namespace QuanLyTro.Tests;

/// <summary>
/// Trả phòng trên MySQL thật (Laragon 127.0.0.1:3306). Dải id 9300-9399, tự dọn ở cả
/// TestInitialize lẫn TestCleanup nên chạy lại được nhiều lần.
/// Test race dựng thứ tự tất định: giao_dich thứ hai giữ người thuê mới CHƯA commit
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
    private static readonly KhachThueService Tenants = new(new KhachThueRepository(Db));

    [TestInitialize]
    public Task CleanupBeforeAsync() => CleanupAsync();

    [TestCleanup]
    public Task CleanupAfterAsync() => CleanupAsync();

    /// <summary>
    /// Khe hở: checkout đọc snapshot cũ (0 người) rồi ghi Trong đè lên DaThue mà giao_dich
    /// thêm người thuê vừa commit. Phòng còn người phải giữ DaThue.
    /// </summary>
    [TestMethod]
    [Timeout(30_000)]
    public async Task CheckoutAsync_TenantAddedConcurrently_RoomStaysRented()
    {
        await SeedRoomAsync(RaceRoomId, "DaThue");
        await SeedTenantAsync(RaceLeavingTenantId, RaceRoomId);

        await using var concurrent = await Db.OpenAsync();
        await using var giao_dich = await concurrent.BeginTransactionAsync();
        // Mô phỏng đúng AddAsync: khóa phòng, thêm người thuê, giữ giao_dich để điều khiển lúc commit.
        await using (var lockRoom = new MySqlCommand(
            "SELECT id FROM phong WHERE id = @phongId FOR UPDATE", concurrent, giao_dich))
        {
            lockRoom.Parameters.AddWithValue("@phongId", RaceRoomId);
            Assert.AreEqual(RaceRoomId, Convert.ToInt32(await lockRoom.ExecuteScalarAsync()));
        }

        await using (var insert = new MySqlCommand(
            """
            INSERT INTO khach_thue (id, phong_id, ho_ten, ngay_sinh, cccd, so_dien_thoai, que_quan, noi_lam_viec, da_dang_ky_tam_tru)
            VALUES (@id, @phongId, 'Race arriving tenant', '1995-01-01', @cccd, '0900000001', 'Đà Nẵng', NULL, 1)
            """, concurrent, giao_dich))
        {
            insert.Parameters.AddWithValue("@id", RaceArrivingTenantId);
            insert.Parameters.AddWithValue("@phongId", RaceRoomId);
            insert.Parameters.AddWithValue("@cccd", IdCardOf(RaceArrivingTenantId));
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
        await giao_dich.CommitAsync();

        Assert.IsTrue(await checkoutTask, "Người thuê đang ở phải trả phòng được.");
        Assert.AreEqual("DaThue", await RoomStatusAsync(RaceRoomId),
            "Phòng vừa có người thuê mới không được bị ghi Trong.");
        Assert.AreEqual(RaceRoomId, await RoomIdOfTenantAsync(RaceArrivingTenantId),
            "Người thuê mới không được mồ côi (phong_id = NULL).");
        Assert.IsNull(await RoomIdOfTenantAsync(RaceLeavingTenantId),
            "Người trả phòng phải có phong_id = NULL.");
    }

    /// <summary>Đường hợp lệ: người cuối cùng của phòng trả phòng, không còn HĐ HieuLuc → phòng về Trong.</summary>
    [TestMethod]
    public async Task CheckoutAsync_LastTenant_ReleasesRoomToAvailable()
    {
        await SeedRoomAsync(LastTenantRoomId, "DaThue");
        await SeedTenantAsync(LastTenantId, LastTenantRoomId);

        Assert.IsTrue(await Tenants.CheckoutAsync(LastTenantId));

        Assert.AreEqual("Trong", await RoomStatusAsync(LastTenantRoomId),
            "Phòng trống hết người và không còn hợp đồng HieuLuc phải về Trong.");
        Assert.IsNull(await RoomIdOfTenantAsync(LastTenantId));
    }

    /// <summary>Còn người khác trong phòng → trạng thái DaThue giữ nguyên.</summary>
    [TestMethod]
    public async Task CheckoutAsync_RoomStillHasOtherTenant_StaysRented()
    {
        await SeedRoomAsync(SharedRoomId, "DaThue");
        await SeedTenantAsync(SharedLeavingTenantId, SharedRoomId);
        await SeedTenantAsync(SharedStayingTenantId, SharedRoomId);

        Assert.IsTrue(await Tenants.CheckoutAsync(SharedLeavingTenantId));

        Assert.AreEqual("DaThue", await RoomStatusAsync(SharedRoomId),
            "Phòng còn người thuê phải giữ DaThue.");
        Assert.AreEqual(SharedRoomId, await RoomIdOfTenantAsync(SharedStayingTenantId));
    }

    private static async Task SeedRoomAsync(int phongId, string trang_thai)
    {
        await using var connection = await Db.OpenAsync();
        await using var command = new MySqlCommand(
            """
            INSERT INTO phong (id, so_phong, gia_thue, so_nguoi_toi_da, trang_thai, mo_ta)
            VALUES (@id, @number, 1000000, 2, @trang_thai, 'TenantCheckoutRaceTests')
            """, connection);
        command.Parameters.AddWithValue("@id", phongId);
        command.Parameters.AddWithValue("@number", $"CHECKOUT-RACE-{phongId}");
        command.Parameters.AddWithValue("@trang_thai", trang_thai);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task SeedTenantAsync(int khachThueId, int phongId)
    {
        await using var connection = await Db.OpenAsync();
        await using var command = new MySqlCommand(
            """
            INSERT INTO khach_thue (id, phong_id, ho_ten, ngay_sinh, cccd, so_dien_thoai, que_quan, noi_lam_viec, da_dang_ky_tam_tru)
            VALUES (@id, @phongId, @hoTen, '1995-01-01', @cccd, @so_dien_thoai, 'Đà Nẵng', NULL, 1)
            """, connection);
        command.Parameters.AddWithValue("@id", khachThueId);
        command.Parameters.AddWithValue("@phongId", phongId);
        command.Parameters.AddWithValue("@hoTen", $"Checkout race {khachThueId}");
        command.Parameters.AddWithValue("@cccd", IdCardOf(khachThueId));
        command.Parameters.AddWithValue("@so_dien_thoai", $"09{khachThueId:D8}");
        await command.ExecuteNonQueryAsync();
    }

    private static string IdCardOf(int khachThueId) => $"98{khachThueId:D10}";

    /// <summary>Chờ tới khi checkout đang nằm trong hàng đợi khóa: đã qua bước đọc, đang chờ khóa dòng phòng.</summary>
    private static async Task WaitUntilBlockedAsync(Task task)
    {
        for (var attempt = 0; attempt < 100 && !task.IsCompleted; attempt++)
        {
            await Task.Delay(50);
        }

        Assert.IsFalse(task.IsCompleted, "Checkout phải đang chờ khóa dòng phòng của giao_dich kia.");
    }

    private static async Task<string?> RoomStatusAsync(int phongId)
    {
        await using var connection = await Db.OpenAsync();
        await using var command = new MySqlCommand("SELECT trang_thai FROM phong WHERE id = @id", connection);
        command.Parameters.AddWithValue("@id", phongId);
        return (string?)await command.ExecuteScalarAsync();
    }

    private static async Task<int?> RoomIdOfTenantAsync(int khachThueId)
    {
        await using var connection = await Db.OpenAsync();
        await using var command = new MySqlCommand("SELECT phong_id FROM khach_thue WHERE id = @id", connection);
        command.Parameters.AddWithValue("@id", khachThueId);
        var value = await command.ExecuteScalarAsync();
        return value is null or DBNull ? null : Convert.ToInt32(value);
    }

    /// <summary>Dọn theo thứ tự an toàn khóa ngoại: hoa_don → chi_so_dien_nuoc → hop_dong → khach_thue → phong.</summary>
    private static async Task CleanupAsync()
    {
        await using var connection = await Db.OpenAsync();
        await using var giao_dich = await connection.BeginTransactionAsync();

        foreach (var sql in new[]
        {
            "DELETE FROM hoa_don WHERE phong_id BETWEEN @minId AND @maxId",
            "DELETE FROM chi_so_dien_nuoc WHERE phong_id BETWEEN @minId AND @maxId",
            "DELETE FROM hop_dong WHERE phong_id BETWEEN @minId AND @maxId",
            "DELETE FROM khach_thue WHERE id BETWEEN @minId AND @maxId OR cccd IN (@card0, @card1, @card2, @card3)",
            "DELETE FROM phong WHERE id BETWEEN @minId AND @maxId",
        })
        {
            await using var command = new MySqlCommand(sql, connection, giao_dich);
            command.Parameters.AddWithValue("@minId", MinId);
            command.Parameters.AddWithValue("@maxId", MaxId);
            command.Parameters.AddWithValue("@card0", IdCardOf(RaceLeavingTenantId));
            command.Parameters.AddWithValue("@card1", IdCardOf(RaceArrivingTenantId));
            command.Parameters.AddWithValue("@card2", IdCardOf(LastTenantId));
            command.Parameters.AddWithValue("@card3", IdCardOf(SharedStayingTenantId));
            await command.ExecuteNonQueryAsync();
        }

        await giao_dich.CommitAsync();
    }
}
