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

/// <summary>
/// Shell khách thuê cần hợp đồng đang hiệu lực + chỉ số điện nước của kỳ để tự kiểm tra cách tính.
/// Một lời gọi GetByTenantAsync phải mang đủ, không mở thêm hành động TCP.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class TenantShellDataTests
{
    private const string ConnectionString =
        "Server=127.0.0.1;Port=3306;Database=quanly_phongtro_nhs;User Id=root;Password=;SslMode=None;";

    private static readonly Database Db = new(ConnectionString);

    private const int RoomId = 9231;
    private const string RoomNumber = "P931";
    private const string TenantCccd = "999999999231";
    private const string KyCuoc = "2099-07";
    private const string KyCuocMoiHon = "2099-08";

    [TestMethod]
    public async Task GetByTenantAsync_TraHopDongDangHieuLucVaChiSoKy()
    {
        var khachThueId = await SeedTenantWithContractAsync();

        var repo = new QuanLyTro.Server.Repositories.HoaDonRepository(Db);
        var dto = await repo.GetByTenantAsync(khachThueId, 1, 10);

        Assert.IsNotNull(dto.HopDong, "Khách có hợp đồng HieuLuc thì HopDong phải khác null.");
        Assert.AreEqual(RoomNumber, dto.HopDong.SoPhong);
        Assert.AreEqual(3_000_000m, dto.HopDong.GiaThue);
        Assert.AreEqual(1_500_000m, dto.HopDong.TienCoc);
        Assert.AreEqual(new DateOnly(2099, 1, 1), dto.HopDong.NgayBatDau);
        Assert.AreEqual(new DateOnly(2099, 12, 31), dto.HopDong.NgayKetThuc);

        Assert.IsTrue(dto.DanhSach.Count > 0, "Phải có hóa đơn để gắn chỉ số kỳ.");
        Assert.IsNotNull(dto.ChiSoKyNay, "Có chỉ số đã chốt của kỳ hóa đơn thì ChiSoKyNay phải khác null.");
        Assert.AreEqual(dto.DanhSach[0].KyCuoc, dto.ChiSoKyNay.KyCuoc,
            "ChiSoKyNay phải gắn với kỳ của hóa đơn đang hiển thị, không phải kỳ mới nhất của phòng.");
        Assert.AreEqual(KyCuoc, dto.ChiSoKyNay.KyCuoc);
        Assert.AreEqual(100, dto.ChiSoKyNay.DienCu);
        Assert.AreEqual(150, dto.ChiSoKyNay.DienMoi);
        Assert.AreEqual(3500m, dto.ChiSoKyNay.GiaDien);
        Assert.AreEqual(20, dto.ChiSoKyNay.NuocCu);
        Assert.AreEqual(25, dto.ChiSoKyNay.NuocMoi);
        Assert.AreEqual(10_000m, dto.ChiSoKyNay.GiaNuoc);

        // Hành vi cũ giữ nguyên.
        Assert.AreEqual(RoomId, dto.PhongId);
        Assert.AreEqual(RoomNumber, dto.SoPhong);
    }

    [TestMethod]
    public async Task GetByTenantAsync_ChiSoKyNay_LoaiBoChiSoKyKhac()
    {
        var khachThueId = await SeedTenantWithContractAsync();

        var repo = new QuanLyTro.Server.Repositories.HoaDonRepository(Db);
        var dto = await repo.GetByTenantAsync(khachThueId, 1, 10);

        Assert.IsTrue(dto.DanhSach.Count > 0);
        Assert.IsNotNull(dto.ChiSoKyNay);
        Assert.AreEqual(dto.DanhSach[0].KyCuoc, dto.ChiSoKyNay.KyCuoc);
        Assert.AreNotEqual(KyCuocMoiHon, dto.ChiSoKyNay.KyCuoc,
            "Chỉ số của kỳ khác (dù mới hơn) không được lọt vào ChiSoKyNay.");
    }

    [TestMethod]
    public async Task GetByTenantAsync_KhachChuaNhanPhong_TraNullKhongNem()
    {
        int khachThueId;
        await using (var conn = await Db.OpenAsync())
        {
            await using var cmd = new MySqlCommand(
                """
                INSERT INTO khach_thue (phong_id, ho_ten, ngay_sinh, cccd, mat_khau_hash,
                                        so_dien_thoai, que_quan, da_dang_ky_tam_tru)
                VALUES (NULL, 'Chua Nhan Phong Shell', '2000-01-01', @cccd, NULL,
                        '09000000231', 'Da Nang', 0)
                ON DUPLICATE KEY UPDATE phong_id = NULL;
                SELECT id FROM khach_thue WHERE cccd = @cccd;
                """, conn);
            cmd.Parameters.AddWithValue("@cccd", TenantCccd);
            khachThueId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        var repo = new QuanLyTro.Server.Repositories.HoaDonRepository(Db);
        var dto = await repo.GetByTenantAsync(khachThueId, 1, 10);

        Assert.IsNull(dto.PhongId);
        Assert.IsNull(dto.HopDong);
        Assert.IsNull(dto.ChiSoKyNay);
        Assert.AreEqual(0, dto.DanhSach.Count);
    }

    private static async Task<int> SeedTenantWithContractAsync()
    {
        await using var conn = await Db.OpenAsync();

        await using (var cmd = new MySqlCommand(
            """
            INSERT INTO phong (id, so_phong, gia_thue, so_nguoi_toi_da, trang_thai, mo_ta)
            VALUES (@id, @number, 3000000, 2, 'DaThue', 'TenantShellDataTests')
            ON DUPLICATE KEY UPDATE trang_thai = 'DaThue', so_phong = @number
            """, conn))
        {
            cmd.Parameters.AddWithValue("@id", RoomId);
            cmd.Parameters.AddWithValue("@number", RoomNumber);
            await cmd.ExecuteNonQueryAsync();
        }

        await using (var cmd = new MySqlCommand(
            """
            INSERT INTO khach_thue (phong_id, ho_ten, ngay_sinh, cccd, mat_khau_hash,
                                    so_dien_thoai, que_quan, da_dang_ky_tam_tru)
            VALUES (@phongId, 'Khach Shell', '2000-01-01', @cccd, NULL,
                    '09000000231', 'Da Nang', 0)
            ON DUPLICATE KEY UPDATE phong_id = @phongId
            """, conn))
        {
            cmd.Parameters.AddWithValue("@phongId", RoomId);
            cmd.Parameters.AddWithValue("@cccd", TenantCccd);
            await cmd.ExecuteNonQueryAsync();
        }

        int khachThueId;
        await using (var cmd = new MySqlCommand(
            "SELECT id FROM khach_thue WHERE cccd = @cccd", conn))
        {
            cmd.Parameters.AddWithValue("@cccd", TenantCccd);
            khachThueId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        await using (var cmd = new MySqlCommand(
            "DELETE FROM hoa_don WHERE phong_id = @phongId", conn))
        {
            cmd.Parameters.AddWithValue("@phongId", RoomId);
            await cmd.ExecuteNonQueryAsync();
        }

        await using (var cmd = new MySqlCommand(
            "DELETE FROM hop_dong WHERE phong_id = @phongId", conn))
        {
            cmd.Parameters.AddWithValue("@phongId", RoomId);
            await cmd.ExecuteNonQueryAsync();
        }

        int hopDongId;
        await using (var cmd = new MySqlCommand(
            """
            INSERT INTO hop_dong (phong_id, nguoi_dai_dien_id, ngay_bat_dau, ngay_ket_thuc,
                                  gia_thue, tien_coc, trang_thai, ghi_chu)
            VALUES (@phongId, @khachThueId, '2099-01-01', '2099-12-31',
                    3000000, 1500000, 'HieuLuc', 'TenantShellDataTests');
            SELECT LAST_INSERT_ID();
            """, conn))
        {
            cmd.Parameters.AddWithValue("@phongId", RoomId);
            cmd.Parameters.AddWithValue("@khachThueId", khachThueId);
            hopDongId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        // Hóa đơn kỳ KyCuoc — chỉ số phải khớp đúng kỳ này.
        await using (var cmd = new MySqlCommand(
            """
            INSERT INTO hoa_don (phong_id, hop_dong_id, ky_cuoc, tien_phong, tien_dien,
                                 tien_nuoc, phi_khac, tong_tien, trang_thai)
            VALUES (@phongId, @hopDongId, @kyCuoc, 3000000, 175000, 50000, 0, 3225000, 'ChuaThu')
            """, conn))
        {
            cmd.Parameters.AddWithValue("@phongId", RoomId);
            cmd.Parameters.AddWithValue("@hopDongId", hopDongId);
            cmd.Parameters.AddWithValue("@kyCuoc", KyCuoc);
            await cmd.ExecuteNonQueryAsync();
        }

        await using (var cmd = new MySqlCommand(
            """
            INSERT INTO chi_so_dien_nuoc (phong_id, ky_cuoc, dien_cu, dien_moi, gia_dien,
                                          nuoc_cu, nuoc_moi, gia_nuoc, hinh_thuc_nuoc, so_nguoi_nuoc)
            VALUES (@phongId, @kyCuoc, 100, 150, 3500, 20, 25, 10000, 'Khoi', 0)
            ON DUPLICATE KEY UPDATE dien_cu = 100, dien_moi = 150, nuoc_cu = 20, nuoc_moi = 25
            """, conn))
        {
            cmd.Parameters.AddWithValue("@phongId", RoomId);
            cmd.Parameters.AddWithValue("@kyCuoc", KyCuoc);
            await cmd.ExecuteNonQueryAsync();
        }

        // Kỳ mới hơn nhưng KHÔNG có hóa đơn — không được lọt vào ChiSoKyNay.
        await using (var cmd = new MySqlCommand(
            """
            INSERT INTO chi_so_dien_nuoc (phong_id, ky_cuoc, dien_cu, dien_moi, gia_dien,
                                          nuoc_cu, nuoc_moi, gia_nuoc, hinh_thuc_nuoc, so_nguoi_nuoc)
            VALUES (@phongId, @kyCuoc, 999, 9999, 9999, 99, 999, 9999, 'Khoi', 0)
            ON DUPLICATE KEY UPDATE dien_cu = 999, dien_moi = 9999
            """, conn))
        {
            cmd.Parameters.AddWithValue("@phongId", RoomId);
            cmd.Parameters.AddWithValue("@kyCuoc", KyCuocMoiHon);
            await cmd.ExecuteNonQueryAsync();
        }

        return khachThueId;
    }

    [TestCleanup]
    public async Task CleanupAsync()
    {
        try
        {
            await using var conn = await Db.OpenAsync();
            await using (var cmd = new MySqlCommand("DELETE FROM hoa_don WHERE phong_id = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", RoomId);
                await cmd.ExecuteNonQueryAsync();
            }

            await using (var cmd = new MySqlCommand("DELETE FROM hop_dong WHERE phong_id = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", RoomId);
                await cmd.ExecuteNonQueryAsync();
            }

            await using (var cmd = new MySqlCommand("DELETE FROM chi_so_dien_nuoc WHERE phong_id = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", RoomId);
                await cmd.ExecuteNonQueryAsync();
            }

            await using (var cmd = new MySqlCommand("DELETE FROM khach_thue WHERE cccd = @cccd", conn))
            {
                cmd.Parameters.AddWithValue("@cccd", TenantCccd);
                await cmd.ExecuteNonQueryAsync();
            }

            await using (var cmd = new MySqlCommand("DELETE FROM phong WHERE id = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", RoomId);
                await cmd.ExecuteNonQueryAsync();
            }
        }
        catch
        {
            // DB chưa sẵn sàng — bỏ qua.
        }
    }
}
