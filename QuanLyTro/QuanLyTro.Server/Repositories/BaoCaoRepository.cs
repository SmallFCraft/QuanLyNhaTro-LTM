using MySqlConnector;
using QuanLyTro.Server.Data;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Repositories;

/// <summary>Biên đọc dữ liệu thống kê — interface để BaoCaoService test không cần DB.</summary>
public interface IBaoCaoRepository
{
    Task<BaoCaoTongQuanDto> GetSummaryAsync(string kyCuoc, CancellationToken ct = default);
    Task<List<XuatHoSoTamTruDto>> GetResidentsAsync(CancellationToken ct = default);
}

/// <summary>Truy vấn tổng hợp cho dashboard (US-04, US-18, US-19) và xuất tạm trú (US-08).</summary>
public sealed class BaoCaoRepository(Database database) : IBaoCaoRepository
{
    /// <summary>Đếm phòng/người và cộng tiền đã thu / còn nợ của tháng hóa đơn.</summary>
    public async Task<BaoCaoTongQuanDto> GetSummaryAsync(string kyCuoc, CancellationToken ct = default)
    {
        const string sql = """
            SELECT
                (SELECT COUNT(*) FROM phong) AS tong_so_phong,
                (SELECT COUNT(*) FROM phong WHERE trang_thai = 'Trong') AS phong_trong,
                (SELECT COUNT(*) FROM phong WHERE trang_thai = 'DaThue') AS phong_da_thue,
                (SELECT COUNT(*) FROM khach_thue WHERE phong_id IS NOT NULL) AS khach_hien_tai,
                (SELECT COALESCE(SUM(tong_tien), 0) FROM hoa_don
                 WHERE ky_cuoc = @kyCuoc AND trang_thai = 'DaThu') AS so_tien_da_thu,
                (SELECT COALESCE(SUM(tong_tien), 0) FROM hoa_don
                 WHERE ky_cuoc = @kyCuoc AND trang_thai = 'ChuaThu') AS so_tien_chua_thu
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@kyCuoc", kyCuoc);

        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
        {
            return new BaoCaoTongQuanDto(0, 0, 0, 0, 0m, 0m);
        }

        return new BaoCaoTongQuanDto(
            Convert.ToInt32(reader.GetInt64("tong_so_phong")),
            Convert.ToInt32(reader.GetInt64("phong_trong")),
            Convert.ToInt32(reader.GetInt64("phong_da_thue")),
            Convert.ToInt32(reader.GetInt64("khach_hien_tai")),
            reader.GetDecimal("so_tien_da_thu"),
            reader.GetDecimal("so_tien_chua_thu"));
    }

    /// <summary>US-08: người đang ở (có phòng), xếp theo số phòng rồi họ tên.</summary>
    public async Task<List<XuatHoSoTamTruDto>> GetResidentsAsync(CancellationToken ct = default)
    {
        const string sql = """
            SELECT t.ho_ten, t.ngay_sinh, t.cccd, t.que_quan, r.so_phong
            FROM khach_thue t
            JOIN phong r ON r.id = t.phong_id
            ORDER BY r.so_phong, t.ho_ten
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(ct);

        var residents = new List<XuatHoSoTamTruDto>();
        while (await reader.ReadAsync(ct))
        {
            residents.Add(new XuatHoSoTamTruDto(
                reader.GetString("ho_ten"),
                reader.GetDateOnly("ngay_sinh"),
                reader.GetString("cccd"),
                reader.GetString("que_quan"),
                reader.GetString("so_phong")));
        }

        return residents;
    }
}
