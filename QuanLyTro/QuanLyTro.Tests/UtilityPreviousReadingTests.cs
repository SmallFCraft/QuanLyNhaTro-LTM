using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using QuanLyTro.Server.Data;
using QuanLyTro.Server.Repositories;
using QuanLyTro.Server.Services;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Tests;

/// <summary>
/// Chỉ số cũ phải lấy đúng kỳ TRƯỚC tháng đang chốt, không phải bản ghi mới nhất của phòng.
/// Truy vấn chặn dưới nằm trong SQL nên phải chạy trên MySQL thật (Laragon 127.0.0.1:3306).
/// Dữ liệu tự dọn trong finally để chạy lại được nhiều lần.
/// </summary>
[TestClass]
public sealed class UtilityPreviousReadingTests
{
    private const string ConnectionString =
        "Server=127.0.0.1;Port=3306;Database=quanly_phongtro_nhs;User Id=root;Password=;";

    private static readonly Database Db = new(ConnectionString);
    private static readonly UtilityService Service = new(new UtilityRepository(Db));

    private const int RoomId = 990001;
    private const int OtherRoomId = 990002;

    private static UtilityReadingDto Reading(
        string month,
        int oldElec,
        int newElec,
        int oldWater,
        int newWater,
        int roomId = RoomId) =>
        new(0, roomId, month, oldElec, newElec, 3500m, oldWater, newWater, 10_000m);

    [TestInitialize]
    public async Task CleanupBefore() => await CleanupAsync();

    [TestCleanup]
    public async Task CleanupAfter() => await CleanupAsync();

    private static async Task CleanupAsync()
    {
        await using var connection = await Db.OpenAsync();
        await using var command = new MySqlCommand(
            "DELETE FROM utility_readings WHERE room_id IN (@a, @b)", connection);
        command.Parameters.AddWithValue("@a", RoomId);
        command.Parameters.AddWithValue("@b", OtherRoomId);
        await command.ExecuteNonQueryAsync();

        await using var rooms = new MySqlCommand(
            "DELETE FROM rooms WHERE id IN (@a, @b)", connection);
        rooms.Parameters.AddWithValue("@a", RoomId);
        rooms.Parameters.AddWithValue("@b", OtherRoomId);
        await rooms.ExecuteNonQueryAsync();
    }

    /// <summary>Phòng thuộc FK utility_readings → rooms, phải tạo trước mỗi case.</summary>
    private static async Task EnsureRoomAsync()
    {
        await using var connection = await Db.OpenAsync();
        await using var room = new MySqlCommand(
            """
            INSERT INTO rooms (id, room_number, price, max_occupants)
            VALUES (@id, @number, 1000000, 2)
            ON DUPLICATE KEY UPDATE room_number = room_number
            """, connection);
        room.Parameters.AddWithValue("@id", RoomId);
        room.Parameters.AddWithValue("@number", $"TMP{RoomId}");
        await room.ExecuteNonQueryAsync();
    }

    private static async Task SeedAsync(params UtilityReadingDto[] readings)
    {
        await EnsureRoomAsync();

        foreach (var reading in readings)
        {
            await Service.RecordAsync(reading);
        }
    }

    [TestMethod]
    public async Task GetPreviousReading_ReturnsReadingOfMonthBeforeRecordingMonth()
    {
        await SeedAsync(Reading("2026-09", 100, 150, 20, 25));

        var previous = await Service.GetPreviousReadingAsync(RoomId, "2026-10");

        Assert.IsNotNull(previous);
        Assert.AreEqual("2026-09", previous.BillingMonth);
        Assert.AreEqual(150, previous.NewElectricity);
        Assert.AreEqual(25, previous.NewWater);
    }

    // Regression: trước khi chặn dưới, bản ghi 2026-09 bị trả về làm chỉ số cũ cho chính tháng 2026-09.
    [TestMethod]
    public async Task GetPreviousReading_DoesNotReturnReadingOfSameOrLaterMonth()
    {
        await SeedAsync(Reading("2026-09", 100, 150, 20, 25));

        Assert.IsNull(await Service.GetPreviousReadingAsync(RoomId, "2026-09"));

        // Chỉ số cũ của tháng 8 không được là chỉ số tháng 9 (bản ghi tương lai).
        Assert.IsNull(await Service.GetPreviousReadingAsync(RoomId, "2026-08"));
    }

    [TestMethod]
    public async Task GetPreviousReading_ReturnsNullWhenRoomHasNoReading()
    {
        Assert.IsNull(await Service.GetPreviousReadingAsync(RoomId, "2026-09"));
        Assert.IsNull(await Service.GetPreviousReadingAsync(RoomId));
    }

    // Không truyền tháng → giữ hành vi cũ cho call site RoomQuery trần.
    [TestMethod]
    public async Task GetPreviousReading_WithoutMonthKeepsLatestBehaviour()
    {
        await SeedAsync(Reading("2026-08", 100, 150, 20, 25));

        var previous = await Service.GetPreviousReadingAsync(RoomId);

        Assert.IsNotNull(previous);
        Assert.AreEqual("2026-08", previous.BillingMonth);
    }

    [DataTestMethod]
    [DataRow("2026-9")]
    [DataRow("26-09")]
    [DataRow("2026-13")]
    public async Task GetPreviousReading_RejectsMalformedMonth(string month)
    {
        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => Service.GetPreviousReadingAsync(RoomId, month));

        StringAssert.Contains(ex.Message, "yyyy-MM");
    }

    [TestMethod]
    public async Task RecordAsync_RejectsOldReadingsNotContinuingPreviousReading()
    {
        await SeedAsync(Reading("2026-08", 100, 150, 20, 25));

        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => Service.RecordAsync(Reading("2026-09", 100, 180, 20, 30)));

        StringAssert.Contains(ex.Message, "tiếp nối");
        Assert.AreEqual(1, await CountAsync(RoomId), "Bản ghi sai không được lưu.");
    }

    [TestMethod]
    public async Task RecordAsync_AcceptsCorrectContinuation()
    {
        await SeedAsync(Reading("2026-08", 100, 150, 20, 25));

        var saved = await Service.RecordAsync(Reading("2026-09", 150, 180, 25, 30));

        Assert.IsTrue(saved.Id > 0);
        Assert.AreEqual(2, await CountAsync(RoomId));
    }

    [TestMethod]
    public async Task RecordAsync_AcceptsFirstEverReadingWithAnyOldValues()
    {
        await EnsureRoomAsync();

        var saved = await Service.RecordAsync(Reading("2026-09", 777, 800, 42, 60));

        Assert.IsTrue(saved.Id > 0);
        Assert.AreEqual(1, await CountAsync(RoomId));
    }

    // Đối chứng: chặn dưới nằm trong SQL nên chỉ bản ghi trước tháng mới được lấy, không phải "mới nhất".
    [TestMethod]
    public async Task RecordAsync_ComparesAgainstPreviousMonthNotLatestReading()
    {
        await SeedAsync(
            Reading("2026-08", 100, 150, 20, 25),
            Reading("2026-10", 150, 200, 25, 40));

        // Kế tiếp tháng 8 (chứ không phải tháng 10) → hợp lệ.
        var saved = await Service.RecordAsync(Reading("2026-09", 150, 180, 25, 30));
        Assert.IsTrue(saved.Id > 0);

        Assert.IsNotNull(await Service.GetPreviousReadingAsync(RoomId, "2026-10"));
        Assert.AreEqual("2026-09", (await Service.GetPreviousReadingAsync(RoomId, "2026-10"))!.BillingMonth);
    }

    private static async Task<int> CountAsync(int roomId)
    {
        await using var connection = await Db.OpenAsync();
        await using var command = new MySqlCommand(
            "SELECT COUNT(*) FROM utility_readings WHERE room_id = @id", connection);
        command.Parameters.AddWithValue("@id", roomId);
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }
}
