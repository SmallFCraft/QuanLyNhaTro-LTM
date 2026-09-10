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
    public void RoomService_RejectsInvalidRoom(string number, double price, int capacity)
    {
        Assert.ThrowsException<BusinessRuleException>(() =>
            RoomService.Validate(new RoomDto(0, number, (decimal)price, capacity, RoomStatus.Available, null, 0)));
    }

    [TestMethod]
    public void RoomService_AcceptsValidRoom()
    {
        var room = new RoomDto(0, "P101", 2_500_000m, 2, RoomStatus.Available, "Tầng 1", 0);
        RoomService.Validate(room);
    }

    private static TenantDto Tenant(
        int id = 0, int? roomId = 1, string fullName = "Nguyễn Văn A", string idCard = "012345678901",
        string phone = "0901234567", string hometown = "Đà Nẵng") =>
        new(id, roomId, fullName, new DateOnly(2000, 1, 1), idCard, phone, hometown, null, false);

    [DataTestMethod]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("01234567890")]    // 11 số
    [DataRow("0123456789012")]  // 13 số
    [DataRow("01234567890a")]   // có chữ
    public void TenantService_RejectsInvalidIdCard(string idCard)
    {
        Assert.ThrowsException<BusinessRuleException>(() =>
            TenantService.Validate(Tenant(idCard: idCard)));
    }

    [DataTestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void TenantService_RejectsEmptyFullName(string fullName)
    {
        Assert.ThrowsException<BusinessRuleException>(() =>
            TenantService.Validate(Tenant(fullName: fullName)));
    }

    [DataTestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void TenantService_RejectsEmptyPhone(string phone)
    {
        Assert.ThrowsException<BusinessRuleException>(() =>
            TenantService.Validate(Tenant(phone: phone)));
    }

    [DataTestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void TenantService_RejectsEmptyHometown(string hometown)
    {
        Assert.ThrowsException<BusinessRuleException>(() =>
            TenantService.Validate(Tenant(hometown: hometown)));
    }

    [TestMethod]
    public void TenantService_AcceptsValidTenant()
    {
        TenantService.Validate(Tenant());
    }

    [TestMethod]
    public void TenantService_DefaultPasswordIsLastSixDigitsOfIdCard()
    {
        Assert.AreEqual("678901", TenantService.DefaultPassword("012345678901"));
    }

    // ------------------------------------------------ BR-02/BR-03 trên MySQL thật
    // Dải riêng: phòng id 9400, CCCD 99999999940x — không trùng wave khác (9188-9215, 990001+).

    private const string ConnectionString =
        "Server=127.0.0.1;Port=3306;Database=quanly_phongtro_nhs;User Id=root;Password=;SslMode=None;";
    private const int CapacityRoomId = 9400;
    private const string CapacityCccd = "999999999400";
    private const string ExtraCccd = "999999999401";

    private static readonly Database Db = new(ConnectionString);
    private static readonly TenantService Tenants = new(new TenantRepository(Db));

    [TestInitialize]
    public Task CleanBeforeAsync() => CleanupAsync();

    [TestCleanup]
    public Task CleanAfterAsync() => CleanupAsync();

    /// <summary>BR-02: phòng max_occupants = 1 — người thứ hai bị chặn, phòng vẫn đúng 1 người.</summary>
    [TestMethod]
    public async Task TenantAdd_RoomAtCapacity_RejectsExactMessage()
    {
        await using (var connection = await Db.OpenAsync())
        await using (var command = new MySqlCommand(
            """
            INSERT INTO rooms (id, room_number, price, max_occupants, status, description)
            VALUES (@id, @number, 1000000, 1, 'Available', 'BusinessRuleTests BR-02')
            """, connection))
        {
            command.Parameters.AddWithValue("@id", CapacityRoomId);
            command.Parameters.AddWithValue("@number", $"BR02-ROOM-{CapacityRoomId}");
            await command.ExecuteNonQueryAsync();
        }

        var added = await Tenants.AddAsync(Tenant(roomId: CapacityRoomId, idCard: CapacityCccd), plainPassword: null);
        Assert.IsTrue(added.Id > 0);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => Tenants.AddAsync(Tenant(roomId: CapacityRoomId, idCard: ExtraCccd), plainPassword: null));

        Assert.AreEqual("Phòng đã đủ sức chứa.", ex.Message);
        Assert.AreEqual(1L, await ScalarAsync(
            "SELECT COUNT(*) FROM tenants WHERE room_id = @r", ("@r", CapacityRoomId)),
            "Người thứ hai không được vào phòng.");
    }

    /// <summary>BR-03: hai người trùng CCCD — lần hai bị UNIQUE(id_card) chặn (MySQL 1062).</summary>
    [TestMethod]
    public async Task TenantAdd_DuplicateIdCard_RejectsExactMessage()
    {
        await Tenants.AddAsync(Tenant(roomId: null, idCard: CapacityCccd), plainPassword: null);

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => Tenants.AddAsync(Tenant(roomId: null, idCard: CapacityCccd), plainPassword: null));

        Assert.AreEqual("Số CCCD đã tồn tại trong hệ thống.", ex.Message);
        Assert.AreEqual(1L, await ScalarAsync(
            "SELECT COUNT(*) FROM tenants WHERE id_card = @c", ("@c", CapacityCccd)));
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
        await using var transaction = await connection.BeginTransactionAsync();

        // FK-safe: người thuê trước, phòng sau. CCCD lấy từ hằng số — không sửa literal tay.
        await using (var tenants = new MySqlCommand(
            $"DELETE FROM tenants WHERE id_card IN ('{CapacityCccd}', '{ExtraCccd}')", connection, transaction))
        {
            await tenants.ExecuteNonQueryAsync();
        }

        await using (var rooms = new MySqlCommand(
            "DELETE FROM rooms WHERE id = @id OR room_number LIKE 'BR02-%'", connection, transaction))
        {
            rooms.Parameters.AddWithValue("@id", CapacityRoomId);
            await rooms.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }
}
