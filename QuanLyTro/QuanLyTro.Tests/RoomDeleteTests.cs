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
    private static readonly PhongService Rooms = new(new PhongRepository(Db));

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

        var ex = await Assert.ThrowsExceptionAsync<LoiNghiepVu>(
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

        var ex = await Assert.ThrowsExceptionAsync<LoiNghiepVu>(
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
        await using var giao_dich = await concurrent.BeginTransactionAsync();
        await using (var insert = new MySqlCommand(
            """
            INSERT INTO khach_thue (id, phong_id, ho_ten, ngay_sinh, cccd, so_dien_thoai, que_quan, noi_lam_viec, da_dang_ky_tam_tru)
            VALUES (@id, @phongId, 'Race tenant', '1995-01-01', @cccd, '0900000003', 'Đà Nẵng', NULL, 1)
            """, concurrent, giao_dich))
        {
            insert.Parameters.AddWithValue("@id", TenantRaceTenantId);
            insert.Parameters.AddWithValue("@phongId", RaceRoomId);
            insert.Parameters.AddWithValue("@cccd", IdCardOf(TenantRaceTenantId));
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
        await giao_dich.CommitAsync();

        var deleteSucceeded = await deleteTask;

        Assert.IsTrue(await RoomExistsAsync(RaceRoomId), "Phòng vừa có người thuê không được bị xóa.");
        Assert.AreEqual(RaceRoomId, await RoomIdOfTenantAsync(TenantRaceTenantId),
            "Người thuê vừa thêm không được bị mồ côi (phong_id = NULL).");
        Assert.IsFalse(deleteSucceeded, "Lần xóa phải thất bại thay vì xóa phòng đang có người.");
        Assert.IsInstanceOfType<LoiNghiepVu>(deleteError, $"Lỗi trả về: {deleteError}");
        Assert.AreEqual(OccupiedMessage, deleteError!.Message);
    }

    /// <summary>Hợp đồng HieuLuc thêm vào sau bước kiểm tra: phải trả lỗi nghiệp vụ, không phải MySQL 1451 thô.</summary>
    [TestMethod]
    [Timeout(30_000)]
    public async Task DeleteAsync_ContractAddedAfterCheck_RejectsWithBusinessRule()
    {
        await SeedRoomAsync(RaceRoomId);
        await SeedTenantAsync(ContractRaceTenantId, null);

        await using var concurrent = await Db.OpenAsync();
        await using var giao_dich = await concurrent.BeginTransactionAsync();
        await using (var insert = new MySqlCommand(
            """
            INSERT INTO hop_dong (phong_id, nguoi_dai_dien_id, ngay_bat_dau, ngay_ket_thuc,
                                   gia_thue, tien_coc, trang_thai, ghi_chu)
            VALUES (@phongId, @khachThueId, '2026-01-01', '2026-12-31', 1000000, 0, 'HieuLuc', 'RoomDeleteTests race')
            """, concurrent, giao_dich))
        {
            insert.Parameters.AddWithValue("@phongId", RaceRoomId);
            insert.Parameters.AddWithValue("@khachThueId", ContractRaceTenantId);
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
        await giao_dich.CommitAsync();

        var deleteSucceeded = await deleteTask;

        Assert.IsTrue(await RoomExistsAsync(RaceRoomId), "Phòng đang có hợp đồng không được bị xóa.");
        Assert.IsFalse(deleteSucceeded, "Lần xóa phải thất bại thay vì xóa phòng đang có hợp đồng.");
        Assert.IsInstanceOfType<LoiNghiepVu>(deleteError, $"Lỗi trả về: {deleteError}");
        Assert.AreEqual(OccupiedMessage, deleteError!.Message);
    }

    private static async Task SeedRoomAsync(int phongId)
    {
        await using var connection = await Db.OpenAsync();
        await using var command = new MySqlCommand(
            """
            INSERT INTO phong (id, so_phong, gia_thue, so_nguoi_toi_da, trang_thai, mo_ta)
            VALUES (@id, @number, 1000000, 2, 'Trong', 'RoomDeleteTests')
            """, connection);
        command.Parameters.AddWithValue("@id", phongId);
        command.Parameters.AddWithValue("@number", $"DELETE-TEST-{phongId}");
        await command.ExecuteNonQueryAsync();
    }

    private static async Task SeedTenantAsync(int khachThueId, int? phongId)
    {
        await using var connection = await Db.OpenAsync();
        await using var command = new MySqlCommand(
            """
            INSERT INTO khach_thue (id, phong_id, ho_ten, ngay_sinh, cccd, so_dien_thoai, que_quan, noi_lam_viec, da_dang_ky_tam_tru)
            VALUES (@id, @phongId, @hoTen, '1995-01-01', @cccd, @so_dien_thoai, 'Đà Nẵng', NULL, 1)
            """, connection);
        command.Parameters.AddWithValue("@id", khachThueId);
        command.Parameters.AddWithValue("@phongId", (object?)phongId ?? DBNull.Value);
        command.Parameters.AddWithValue("@hoTen", $"Delete test {khachThueId}");
        command.Parameters.AddWithValue("@cccd", IdCardOf(khachThueId));
        command.Parameters.AddWithValue("@so_dien_thoai", $"09{khachThueId:D8}");
        await command.ExecuteNonQueryAsync();
    }

    private static string IdCardOf(int khachThueId) => $"98{khachThueId:D10}";

    private static async Task SeedContractAsync(int phongId, int khachThueId)
    {
        await using var connection = await Db.OpenAsync();
        await using var command = new MySqlCommand(
            """
            INSERT INTO hop_dong (phong_id, nguoi_dai_dien_id, ngay_bat_dau, ngay_ket_thuc,
                                   gia_thue, tien_coc, trang_thai, ghi_chu)
            VALUES (@phongId, @khachThueId, '2026-01-01', '2026-12-31', 1000000, 0, 'HieuLuc', 'RoomDeleteTests')
            """, connection);
        command.Parameters.AddWithValue("@phongId", phongId);
        command.Parameters.AddWithValue("@khachThueId", khachThueId);
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Chờ tới khi lần xóa đang nằm trong hàng đợi khóa của MySQL: bước kiểm tra đã đi qua,
    /// DELETE (hoặc FOR UPDATE) đang chờ khóa S do giao_dich chưa commit giữ trên dòng phòng.
    /// </summary>
    private static async Task WaitUntilBlockedAsync(Task task)
    {
        for (var attempt = 0; attempt < 100 && !task.IsCompleted; attempt++)
        {
            await Task.Delay(50);
        }

        Assert.IsFalse(task.IsCompleted, "Lần xóa phải đang chờ khóa dòng phòng.");
    }

    private static async Task<bool> RoomExistsAsync(int phongId)
    {
        await using var connection = await Db.OpenAsync();
        await using var command = new MySqlCommand(
            "SELECT COUNT(*) FROM phong WHERE id = @id", connection);
        command.Parameters.AddWithValue("@id", phongId);
        return Convert.ToInt64(await command.ExecuteScalarAsync()) > 0;
    }

    private static async Task<int?> RoomIdOfTenantAsync(int khachThueId)
    {
        await using var connection = await Db.OpenAsync();
        await using var command = new MySqlCommand(
            "SELECT phong_id FROM khach_thue WHERE id = @id", connection);
        command.Parameters.AddWithValue("@id", khachThueId);
        var value = await command.ExecuteScalarAsync();
        return value is null or DBNull ? null : Convert.ToInt32(value);
    }

    private static async Task CleanupAsync()
    {
        await using var connection = await Db.OpenAsync();
        await using var giao_dich = await connection.BeginTransactionAsync();

        foreach (var sql in new[]
        {
            "DELETE FROM hoa_don WHERE phong_id BETWEEN @minId AND @maxId",
            "DELETE FROM chi_so_dien_nuoc WHERE phong_id BETWEEN @minId AND @maxId",
            "DELETE FROM hop_dong WHERE phong_id BETWEEN @minId AND @maxId",
            // Người thuê do service tự cấp id (auto_increment) nên dọn theo CCCD, không theo id.
            "DELETE FROM khach_thue WHERE cccd IN (@card0, @card1, @card2, @card3)",
            "DELETE FROM phong WHERE id BETWEEN @minId AND @maxId",
        })
        {
            await using var command = new MySqlCommand(sql, connection, giao_dich);
            command.Parameters.AddWithValue("@minId", MinId);
            command.Parameters.AddWithValue("@maxId", MaxId);
            command.Parameters.AddWithValue("@card0", IdCardOf(TenantId));
            command.Parameters.AddWithValue("@card1", IdCardOf(ContractTenantId));
            command.Parameters.AddWithValue("@card2", IdCardOf(ContractRaceTenantId));
            command.Parameters.AddWithValue("@card3", IdCardOf(TenantRaceTenantId));
            await command.ExecuteNonQueryAsync();
        }

        await giao_dich.CommitAsync();
    }
}
