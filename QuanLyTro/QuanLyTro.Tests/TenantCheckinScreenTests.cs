using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using QuanLyTro.Server.Data;
using QuanLyTro.Shared.Models;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Tests;

/// <summary>
/// Màn "Nhận phòng trọ" phải chỉ hiện khi khách CHƯA CÓ PHÒNG — không được suy ra từ
/// việc chưa có hóa đơn (khách vừa nhận phòng chưa phát sinh kỳ cước nào).
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class TenantCheckinScreenTests
{
    private const string ConnectionString =
        "Server=127.0.0.1;Port=3306;Database=quanly_phongtro_nhs;User Id=root;Password=;SslMode=None;";

    private static readonly Database Db = new(ConnectionString);

    [TestMethod]
    public void HoaDonCuaToi_Dto_MangTruongPhongId_DePhanBietChuaNhanPhong()
    {
        var truong = typeof(TrangHoaDonCuaToiDto).GetProperties()
            .FirstOrDefault(p => p.Name is "PhongId");

        Assert.IsNotNull(truong, "TrangHoaDonCuaToiDto phải có PhongId (null = khách chưa nhận phòng).");
        Assert.AreEqual(typeof(int?), truong.PropertyType, "PhongId phải là int? — null nghĩa là chưa có phòng.");
    }

    [TestMethod]
    public async Task HoaDonCuaToi_TraPhongIdCuaChinhKhachTrongPhien()
    {
        // Khách chưa nhận phòng: phong_id IS NULL.
        await using (var conn = await Db.OpenAsync())
        {
            await using var cmd = new MySqlCommand(
                """
                INSERT INTO khach_thue (phong_id, ho_ten, ngay_sinh, cccd, mat_khau_hash,
                                        so_dien_thoai, que_quan, da_dang_ky_tam_tru)
                VALUES (NULL, 'Chua Nhan Phong', '2000-01-01', '999999999250', NULL,
                        '09000000250', 'Da Nang', 0)
                ON DUPLICATE KEY UPDATE phong_id = NULL;
                """, conn);
            await cmd.ExecuteNonQueryAsync();
        }

        int khachThueId;
        await using (var conn = await Db.OpenAsync())
        {
            await using var cmd = new MySqlCommand(
                "SELECT id FROM khach_thue WHERE cccd = '999999999250'", conn);
            khachThueId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        var repo = new QuanLyTro.Server.Repositories.HoaDonRepository(Db);
        var dto = await repo.GetByTenantAsync(khachThueId, 1, 10);

        Assert.IsNull(dto.PhongId,
            $"Khách chưa nhận phòng thì PhongId phải null, server trả: {dto.PhongId}");
    }

    [TestMethod]
    public void KhachThueJs_KhongDuocSuyRaTuSoHoaDon()
    {
        var js = File.ReadAllText(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "Assets", "wwwroot", "khachthue", "js", "khach_thue.js"));

        Assert.IsFalse(js.Contains("tenantHistoryTotal === 0"),
            "Không được suy 'chưa có phòng' từ số hóa đơn = 0 — khách mới nhận phòng chưa có kỳ cước nào.");
        Assert.IsTrue(js.Contains("phongId"),
            "khach_thue.js phải đọc phongId từ DTO để quyết định hiển thị màn nhận phòng.");
    }

    [TestCleanup]
    public async Task CleanupAsync()
    {
        try
        {
            await using var conn = await Db.OpenAsync();
            await using var cmd = new MySqlCommand(
                "DELETE FROM khach_thue WHERE cccd = '999999999250'", conn);
            await cmd.ExecuteNonQueryAsync();
        }
        catch
        {
            // DB chưa sẵn sàng — bỏ qua.
        }
    }
}
