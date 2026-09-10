using MySqlConnector;
using QuanLyTro.Server.Data;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Repositories;

/// <summary>Biên truy cập bảng `utility_readings` — interface để UtilityService test không cần DB.</summary>
public interface IUtilityRepository
{
    Task<UtilityReadingDto?> GetLatestAsync(int roomId, CancellationToken ct = default);
    Task<UtilityReadingDto> AddAsync(UtilityReadingDto reading, CancellationToken ct = default);
}

/// <summary>Chỉ số điện nước theo phòng/tháng. MySQL 1062 (uq_room_month) → BusinessRuleException.</summary>
public sealed class UtilityRepository(Database database) : IUtilityRepository
{
    private const int MySqlDuplicateKey = 1062;

    public async Task<UtilityReadingDto?> GetLatestAsync(int roomId, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, room_id, billing_month, old_electricity, new_electricity, electricity_rate,
                   old_water, new_water, water_rate
            FROM utility_readings
            WHERE room_id = @roomId
            ORDER BY billing_month DESC
            LIMIT 1
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@roomId", roomId);

        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
        {
            return null;
        }

        return Map(reader);
    }

    public async Task<UtilityReadingDto> AddAsync(UtilityReadingDto reading, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO utility_readings (room_id, billing_month, old_electricity, new_electricity,
                                          electricity_rate, old_water, new_water, water_rate)
            VALUES (@roomId, @billingMonth, @oldElectricity, @newElectricity,
                    @electricityRate, @oldWater, @newWater, @waterRate);
            SELECT LAST_INSERT_ID();
            """;

        try
        {
            await using var connection = await database.OpenAsync(ct);
            await using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@roomId", reading.RoomId);
            command.Parameters.AddWithValue("@billingMonth", reading.BillingMonth);
            command.Parameters.AddWithValue("@oldElectricity", reading.OldElectricity);
            command.Parameters.AddWithValue("@newElectricity", reading.NewElectricity);
            command.Parameters.AddWithValue("@electricityRate", reading.ElectricityRate);
            command.Parameters.AddWithValue("@oldWater", reading.OldWater);
            command.Parameters.AddWithValue("@newWater", reading.NewWater);
            command.Parameters.AddWithValue("@waterRate", reading.WaterRate);

            var id = Convert.ToInt32(await command.ExecuteScalarAsync(ct));
            return reading with { Id = id };
        }
        catch (MySqlException ex) when (ex.Number == MySqlDuplicateKey)
        {
            throw new Services.BusinessRuleException("Phòng này đã chốt điện nước tháng này.");
        }
    }

    private static UtilityReadingDto Map(MySqlDataReader reader) => new(
        reader.GetInt32("id"),
        reader.GetInt32("room_id"),
        reader.GetString("billing_month"),
        reader.GetInt32("old_electricity"),
        reader.GetInt32("new_electricity"),
        reader.GetDecimal("electricity_rate"),
        reader.GetInt32("old_water"),
        reader.GetInt32("new_water"),
        reader.GetDecimal("water_rate"));
}
