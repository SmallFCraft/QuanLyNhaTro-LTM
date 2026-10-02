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
    private static readonly DienNuocService Service = new(new DienNuocRepository(Db));

    private const int PhongId = 990001;
    private const int OtherRoomId = 990002;

    private static ChiSoDienNuocDto BanGhi(
        string month,
        int oldElec,
        int newElec,
        int nuocCu,
        int nuocMoi,
        int phongId = PhongId) =>
        new(0, phongId, month, oldElec, newElec, 3500m, nuocCu, nuocMoi, 10_000m);

    [TestInitialize]
    public async Task CleanupBefore() => await CleanupAsync();

    [TestCleanup]
    public async Task CleanupAfter() => await CleanupAsync();

    private static async Task CleanupAsync()
    {
        await using var connection = await Db.OpenAsync();
        await using var command = new MySqlCommand(
            "DELETE FROM chi_so_dien_nuoc WHERE phong_id IN (@a, @b)", connection);
        command.Parameters.AddWithValue("@a", PhongId);
        command.Parameters.AddWithValue("@b", OtherRoomId);
        await command.ExecuteNonQueryAsync();

        await using var phong = new MySqlCommand(
            "DELETE FROM phong WHERE id IN (@a, @b)", connection);
        phong.Parameters.AddWithValue("@a", PhongId);
        phong.Parameters.AddWithValue("@b", OtherRoomId);
        await phong.ExecuteNonQueryAsync();
    }

    /// <summary>Phòng thuộc FK chi_so_dien_nuoc → phong, phải tạo trước mỗi case.</summary>
    private static async Task EnsureRoomAsync()
    {
        await using var connection = await Db.OpenAsync();
        await using var room = new MySqlCommand(
            """
            INSERT INTO phong (id, so_phong, gia_thue, so_nguoi_toi_da)
            VALUES (@id, @number, 1000000, 2)
            ON DUPLICATE KEY UPDATE so_phong = so_phong
            """, connection);
        room.Parameters.AddWithValue("@id", PhongId);
        room.Parameters.AddWithValue("@number", $"TMP{PhongId}");
        await room.ExecuteNonQueryAsync();
    }

    private static async Task SeedAsync(params ChiSoDienNuocDto[] banGhi)
    {
        await EnsureRoomAsync();

        foreach (var reading in banGhi)
        {
            await Service.RecordAsync(reading);
        }
    }

    [TestMethod]
    public async Task GetPreviousReading_ReturnsReadingOfMonthBeforeRecordingMonth()
    {
        await SeedAsync(BanGhi("2026-09", 100, 150, 20, 25));

        var previous = await Service.GetPreviousReadingAsync(PhongId, "2026-10");

        Assert.IsNotNull(previous);
        Assert.AreEqual("2026-09", previous.KyCuoc);
        Assert.AreEqual(150, previous.DienMoi);
        Assert.AreEqual(25, previous.NuocMoi);
    }

    // Regression: trước khi chặn dưới, bản ghi 2026-09 bị trả về làm chỉ số cũ cho chính tháng 2026-09.
    [TestMethod]
    public async Task GetPreviousReading_DoesNotReturnReadingOfSameOrLaterMonth()
    {
        await SeedAsync(BanGhi("2026-09", 100, 150, 20, 25));

        Assert.IsNull(await Service.GetPreviousReadingAsync(PhongId, "2026-09"));

        // Chỉ số cũ của tháng 8 không được là chỉ số tháng 9 (bản ghi tương lai).
        Assert.IsNull(await Service.GetPreviousReadingAsync(PhongId, "2026-08"));
    }

    [TestMethod]
    public async Task GetPreviousReading_ReturnsNullWhenRoomHasNoReading()
    {
        Assert.IsNull(await Service.GetPreviousReadingAsync(PhongId, "2026-09"));
        Assert.IsNull(await Service.GetPreviousReadingAsync(PhongId));
    }

    // Không truyền tháng → giữ hành vi cũ cho call site TruyVanPhong trần.
    [TestMethod]
    public async Task GetPreviousReading_WithoutMonthKeepsLatestBehaviour()
    {
        await SeedAsync(BanGhi("2026-08", 100, 150, 20, 25));

        var previous = await Service.GetPreviousReadingAsync(PhongId);

        Assert.IsNotNull(previous);
        Assert.AreEqual("2026-08", previous.KyCuoc);
    }

    [DataTestMethod]
    [DataRow("2026-9")]
    [DataRow("26-09")]
    [DataRow("2026-13")]
    public async Task GetPreviousReading_RejectsMalformedMonth(string month)
    {
        var ex = await Assert.ThrowsExceptionAsync<LoiNghiepVu>(
            () => Service.GetPreviousReadingAsync(PhongId, month));

        StringAssert.Contains(ex.Message, "yyyy-MM");
    }

    [TestMethod]
    public async Task RecordAsync_RejectsOldReadingsNotContinuingPreviousReading()
    {
        await SeedAsync(BanGhi("2026-08", 100, 150, 20, 25));

        var ex = await Assert.ThrowsExceptionAsync<LoiNghiepVu>(
            () => Service.RecordAsync(BanGhi("2026-09", 100, 180, 20, 30)));

        StringAssert.Contains(ex.Message, "tiếp nối");
        Assert.AreEqual(1, await CountAsync(PhongId), "Bản ghi sai không được lưu.");
    }

    [TestMethod]
    public async Task RecordAsync_AcceptsCorrectContinuation()
    {
        await SeedAsync(BanGhi("2026-08", 100, 150, 20, 25));

        var saved = await Service.RecordAsync(BanGhi("2026-09", 150, 180, 25, 30));

        Assert.IsTrue(saved.Id > 0);
        Assert.AreEqual(2, await CountAsync(PhongId));
    }

    [TestMethod]
    public async Task RecordAsync_AcceptsFirstEverReadingWithAnyOldValues()
    {
        await EnsureRoomAsync();

        var saved = await Service.RecordAsync(BanGhi("2026-09", 777, 800, 42, 60));

        Assert.IsTrue(saved.Id > 0);
        Assert.AreEqual(1, await CountAsync(PhongId));
    }

    // Đối chứng: chặn dưới nằm trong SQL nên chỉ bản ghi trước tháng mới được lấy, không phải "mới nhất".
    [TestMethod]
    public async Task RecordAsync_ComparesAgainstPreviousMonthNotLatestReading()
    {
        await SeedAsync(
            BanGhi("2026-08", 100, 150, 20, 25),
            BanGhi("2026-10", 150, 200, 25, 40));

        // Kế tiếp tháng 8 (chứ không phải tháng 10) → hợp lệ.
        var saved = await Service.RecordAsync(BanGhi("2026-09", 150, 180, 25, 30));
        Assert.IsTrue(saved.Id > 0);

        Assert.IsNotNull(await Service.GetPreviousReadingAsync(PhongId, "2026-10"));
        Assert.AreEqual("2026-09", (await Service.GetPreviousReadingAsync(PhongId, "2026-10"))!.KyCuoc);
    }

    private static async Task<int> CountAsync(int phongId)
    {
        await using var connection = await Db.OpenAsync();
        await using var command = new MySqlCommand(
            "SELECT COUNT(*) FROM chi_so_dien_nuoc WHERE phong_id = @id", connection);
        command.Parameters.AddWithValue("@id", phongId);
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }
}
