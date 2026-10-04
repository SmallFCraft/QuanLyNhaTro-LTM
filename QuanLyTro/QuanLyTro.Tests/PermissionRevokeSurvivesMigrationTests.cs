using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using QuanLyTro.Server.Data;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Tests;

/// <summary>
/// Sổ cái `quyen_mac_dinh_da_ap_dung` phải ngăn migration hồi sinh quyền Chủ trọ đã thu hồi.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class PermissionRevokeSurvivesMigrationTests
{
    private const string ConnectionString =
        "Server=127.0.0.1;Port=3306;Database=quanly_phongtro_nhs;User Id=root;Password=;SslMode=None;";

    private static readonly Database Db = new(ConnectionString);

    [TestMethod]
    public async Task RevokedPermission_IsNotRestoredByNextMigrationRun()
    {
        const string role = "QuanLy";
        var action = ActionNames.PhongXoa;

        try
        {
            await using (var conn = await Db.OpenAsync())
            {
                // Chủ trọ thu hồi PHONG_XOA khỏi Quản lý (giống thao tác trên màn Phân quyền).
                await using var del = new MySqlCommand(
                    "DELETE FROM quyen_vai_tro WHERE vai_tro = @r AND hanh_dong = @a", conn);
                del.Parameters.AddWithValue("@r", role);
                del.Parameters.AddWithValue("@a", action);
                await del.ExecuteNonQueryAsync();
            }

            // Khởi động lại Server (chạy migration lần nữa).
            await new KhoiTaoSchema(Db).InitializeAsync();

            await using var check = await Db.OpenAsync();
            await using var cmd = new MySqlCommand(
                "SELECT COUNT(*) FROM quyen_vai_tro WHERE vai_tro = @r AND hanh_dong = @a", check);
            cmd.Parameters.AddWithValue("@r", role);
            cmd.Parameters.AddWithValue("@a", action);
            var count = Convert.ToInt64(await cmd.ExecuteScalarAsync());

            Assert.AreEqual(0L, count,
                "Migration KHÔNG được hồi sinh quyền đã bị Chủ trọ thu hồi.");
        }
        finally
        {
            // Luôn trả lại DB về trạng thái ban đầu, kể cả khi assert fail:
            // dòng này nằm trong sổ cái nên migration không tự chèn lại → phải chèn tường minh.
            await using var restoreConn = await Db.OpenAsync();
            await using var restore = new MySqlCommand(
                "INSERT IGNORE INTO quyen_vai_tro (vai_tro, hanh_dong) VALUES (@r, @a)", restoreConn);
            restore.Parameters.AddWithValue("@r", role);
            restore.Parameters.AddWithValue("@a", action);
            await restore.ExecuteNonQueryAsync();
        }
    }
}
