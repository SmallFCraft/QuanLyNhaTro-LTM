using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using QuanLyTro.Server.Data;
using QuanLyTro.Server.Network;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Tests;

[TestClass]
[DoNotParallelize]
public sealed class PermissionAutoMigrateTests
{
    private const string ConnectionString =
        "Server=127.0.0.1;Port=3306;Database=quanly_phongtro_nhs;User Id=root;Password=;SslMode=None;";

    private static readonly Database Db = new(ConnectionString);

    [TestMethod]
    public async Task EnsureRolePermissionsUpToDate_InsertsMissingDefaultActions_ForExistingRoles()
    {
        // 1. Chạy migration đảm bảo quyền được nạp đủ
        await new KhoiTaoSchema(Db).InitializeAsync();

        // 2. Tra cứu trực tiếp bảng quyen_vai_tro trong MySQL
        await using var conn = await Db.OpenAsync();
        await using var cmd = new MySqlCommand(
            "SELECT COUNT(*) FROM quyen_vai_tro WHERE vai_tro = 'KhachThue' AND hanh_dong = @a", conn);
        cmd.Parameters.AddWithValue("@a", ActionNames.KhachThueNhanPhongQr);
        var khachThueHasAction = Convert.ToInt64(await cmd.ExecuteScalarAsync());

        await using var cmd2 = new MySqlCommand(
            "SELECT COUNT(*) FROM quyen_vai_tro WHERE vai_tro = 'QuanLy' AND hanh_dong = @a", conn);
        cmd2.Parameters.AddWithValue("@a", ActionNames.HopDongSinhQr);
        var quanLyHasAction = Convert.ToInt64(await cmd2.ExecuteScalarAsync());

        Assert.AreEqual(1L, khachThueHasAction, "Bảng quyen_vai_tro phải có dòng KhachThue: KHACH_THUE_NHAN_PHONG_QR.");
        Assert.AreEqual(1L, quanLyHasAction, "Bảng quyen_vai_tro phải có dòng QuanLy: HOP_DONG_SINH_QR.");

        var repository = new QuanLyTro.Server.Repositories.PhanQuyenRepository(Db);
        MaTranPhanQuyen.ApplyMatrix(await repository.GetAllAsync());
        try
        {
            Assert.IsTrue(MaTranPhanQuyen.IsAllowed(ActionNames.KhachThueNhanPhongQr,
                QuanLyTro.Shared.Models.VaiTroNguoiDung.KhachThue));
            Assert.IsTrue(MaTranPhanQuyen.IsAllowed(ActionNames.HopDongSinhQr,
                QuanLyTro.Shared.Models.VaiTroNguoiDung.QuanLy));
        }
        finally
        {
            MaTranPhanQuyen.ResetToDefaults();
        }
    }
}
