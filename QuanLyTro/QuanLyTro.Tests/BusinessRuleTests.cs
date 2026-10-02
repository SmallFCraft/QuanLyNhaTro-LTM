using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using QuanLyTro.Server.Data;
using QuanLyTro.Server.Repositories;
using QuanLyTro.Server.Services;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Tests;

[TestClass]
[DoNotParallelize]
public sealed class BusinessRuleTests
{
    [DataTestMethod]
    [DataRow("", 1000000, 2)]
    [DataRow("P1", 0, 2)]
    [DataRow("P1", -1, 2)]
    [DataRow("P1", 1000000, 0)]
    [DataRow("P1", 1000000, -1)]
    public void RoomService_RejectsInvalidRoom(string number, double gia_thue, int capacity)
    {
        Assert.ThrowsException<LoiNghiepVu>(() =>
            PhongService.Validate(new PhongDto(0, number, (decimal)gia_thue, capacity, TrangThaiPhong.Trong, null, 0)));
    }

    [TestMethod]
    public void RoomService_AcceptsValidRoom()
    {
        var room = new PhongDto(0, "P101", 2_500_000m, 2, TrangThaiPhong.Trong, "Tầng 1", 0);
        PhongService.Validate(room);
    }

    private static KhachThueDto KhachThue(
        int id = 0, int? phongId = 1, string hoTen = "Nguyễn Văn A", string cccd = "012345678901",
        string so_dien_thoai = "0901234567", string que_quan = "Đà Nẵng") =>
        new(id, phongId, hoTen, new DateOnly(2000, 1, 1), cccd, so_dien_thoai, que_quan, null, false);

    [DataTestMethod]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("01234567890")]    // 11 số
    [DataRow("0123456789012")]  // 13 số
    [DataRow("01234567890a")]   // có chữ
    public void TenantService_RejectsInvalidIdCard(string cccd)
    {
        Assert.ThrowsException<LoiNghiepVu>(() =>
            KhachThueService.Validate(KhachThue(cccd: cccd)));
    }

    [DataTestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void TenantService_RejectsEmptyFullName(string hoTen)
    {
        Assert.ThrowsException<LoiNghiepVu>(() =>
            KhachThueService.Validate(KhachThue(hoTen: hoTen)));
    }

    [DataTestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void TenantService_RejectsEmptyPhone(string so_dien_thoai)
    {
        Assert.ThrowsException<LoiNghiepVu>(() =>
            KhachThueService.Validate(KhachThue(so_dien_thoai: so_dien_thoai)));
    }

    [DataTestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void TenantService_RejectsEmptyHometown(string que_quan)
    {
        Assert.ThrowsException<LoiNghiepVu>(() =>
            KhachThueService.Validate(KhachThue(que_quan: que_quan)));
    }

    [TestMethod]
    public void TenantService_AcceptsValidTenant()
    {
        KhachThueService.Validate(KhachThue());
    }

    [TestMethod]
    public void TenantService_DefaultPasswordIsLastSixDigitsOfIdCard()
    {
        Assert.AreEqual("678901", KhachThueService.DefaultPassword("012345678901"));
    }

    // ------------------------------------------------ BR-02/BR-03 trên MySQL thật
    // Dải riêng: phòng id 9400, CCCD 99999999940x — không trùng wave khác (9188-9215, 990001+).

    private const string ConnectionString =
        "Server=127.0.0.1;Port=3306;Database=quanly_phongtro_nhs;User Id=root;Password=;SslMode=None;";
    private const int CapacityRoomId = 9400;
    private const string CapacityCccd = "999999999400";
    private const string ExtraCccd = "999999999401";

    private static readonly Database Db = new(ConnectionString);
    private static readonly KhachThueService Tenants = new(new KhachThueRepository(Db));

    [TestInitialize]
    public Task CleanBeforeAsync() => CleanupAsync();

    [TestCleanup]
    public Task CleanAfterAsync() => CleanupAsync();

    /// <summary>BR-02: phòng so_nguoi_toi_da = 1 — người thứ hai bị chặn, phòng vẫn đúng 1 người.</summary>
    [TestMethod]
    public async Task TenantAdd_RoomAtCapacity_RejectsExactMessage()
    {
        await using (var connection = await Db.OpenAsync())
        await using (var command = new MySqlCommand(
            """
            INSERT INTO phong (id, so_phong, gia_thue, so_nguoi_toi_da, trang_thai, mo_ta)
            VALUES (@id, @number, 1000000, 1, 'Trong', 'BusinessRuleTests BR-02')
            """, connection))
        {
            command.Parameters.AddWithValue("@id", CapacityRoomId);
            command.Parameters.AddWithValue("@number", $"BR02-ROOM-{CapacityRoomId}");
            await command.ExecuteNonQueryAsync();
        }

        var added = await Tenants.AddAsync(KhachThue(phongId: CapacityRoomId, cccd: CapacityCccd), plainPassword: null);
        Assert.IsTrue(added.Id > 0);

        var ex = await Assert.ThrowsExceptionAsync<LoiNghiepVu>(
            () => Tenants.AddAsync(KhachThue(phongId: CapacityRoomId, cccd: ExtraCccd), plainPassword: null));

        Assert.AreEqual("Phòng đã đủ sức chứa.", ex.Message);
        Assert.AreEqual(1L, await ScalarAsync(
            "SELECT COUNT(*) FROM khach_thue WHERE phong_id = @r", ("@r", CapacityRoomId)),
            "Người thứ hai không được vào phòng.");
    }

    /// <summary>BR-03: hai người trùng CCCD — lần hai bị UNIQUE(cccd) chặn (MySQL 1062).</summary>
    [TestMethod]
    public async Task TenantAdd_DuplicateIdCard_RejectsExactMessage()
    {
        await Tenants.AddAsync(KhachThue(phongId: null, cccd: CapacityCccd), plainPassword: null);

        var ex = await Assert.ThrowsExceptionAsync<LoiNghiepVu>(
            () => Tenants.AddAsync(KhachThue(phongId: null, cccd: CapacityCccd), plainPassword: null));

        Assert.AreEqual("Số CCCD đã tồn tại trong hệ thống.", ex.Message);
        Assert.AreEqual(1L, await ScalarAsync(
            "SELECT COUNT(*) FROM khach_thue WHERE cccd = @c", ("@c", CapacityCccd)));
    }

    private static async Task<long> ScalarAsync(string sql, params (string Name, object Value)[] parameters)
    {
        await using var connection = await Db.OpenAsync();
        await using var command = new MySqlCommand(sql, connection);
        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        return Convert.ToInt64(await command.ExecuteScalarAsync());
    }

    private static async Task CleanupAsync()
    {
        await using var connection = await Db.OpenAsync();
        await using var giao_dich = await connection.BeginTransactionAsync();

        // FK-safe: người thuê trước, phòng sau. CCCD lấy từ hằng số — không sửa literal tay.
        await using (var khach_thue = new MySqlCommand(
            $"DELETE FROM khach_thue WHERE cccd IN ('{CapacityCccd}', '{ExtraCccd}')", connection, giao_dich))
        {
            await khach_thue.ExecuteNonQueryAsync();
        }

        await using (var phong = new MySqlCommand(
            "DELETE FROM phong WHERE id = @id OR so_phong LIKE 'BR02-%'", connection, giao_dich))
        {
            phong.Parameters.AddWithValue("@id", CapacityRoomId);
            await phong.ExecuteNonQueryAsync();
        }

        await giao_dich.CommitAsync();
    }
}
