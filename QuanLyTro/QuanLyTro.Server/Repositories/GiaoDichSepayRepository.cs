using MySqlConnector;
using QuanLyTro.Server.Data;
using QuanLyTro.Server.Services;

namespace QuanLyTro.Server.Repositories;

/// <summary>Biên truy cập bảng `giao_dich_sepay`.</summary>
public interface IGiaoDichSepayRepository
{
    /// <summary>Ghi giao dịch nếu chưa có. False = đã tồn tại (đã thấy ở lần poll trước).</summary>
    Task<bool> ThemNeuChuaCoAsync(GiaoDichSepay giao_dich, CancellationToken ct = default);

    /// <summary>True khi giao dịch đã khớp được hóa đơn — chỉ những giao dịch này mới được bỏ qua.</summary>
    Task<bool> DaKhopChuaAsync(string sepayId, CancellationToken ct = default);

    /// <summary>Đánh dấu giao dịch đã khớp hóa đơn — hoặc ghi lý do không khớp.</summary>
    Task DanhDauAsync(string sepayId, int? hoaDonId, string? lyDoLoai, CancellationToken ct = default);

    /// <summary>Mốc thời gian giao dịch lớn nhất đã đọc — lần poll sau chỉ hỏi từ đây.</summary>
    Task<DateTime?> LayMocThoiDiemAsync(CancellationToken ct = default);
}

/// <summary>Dùng khi router được dựng trong test không có DB — coi như không có giao dịch nào.</summary>
public sealed class NullGiaoDichSepayRepository : IGiaoDichSepayRepository
{
    public Task<bool> ThemNeuChuaCoAsync(GiaoDichSepay giao_dich, CancellationToken ct = default) =>
        Task.FromResult(false);

    public Task<bool> DaKhopChuaAsync(string sepayId, CancellationToken ct = default) =>
        Task.FromResult(false);

    public Task DanhDauAsync(string sepayId, int? hoaDonId, string? lyDoLoai, CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task<DateTime?> LayMocThoiDiemAsync(CancellationToken ct = default) =>
        Task.FromResult<DateTime?>(null);
}

public sealed class GiaoDichSepayRepository(Database database) : IGiaoDichSepayRepository
{
    public async Task<bool> ThemNeuChuaCoAsync(GiaoDichSepay giao_dich, CancellationToken ct = default)
    {
        const string sql = """
            INSERT IGNORE INTO giao_dich_sepay (sepay_id, noi_dung, so_tien, thoi_diem, ma_tham_chieu)
            VALUES (@sepay_id, @noi_dung, @so_tien, @thoi_diem, @ma_tham_chieu)
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@sepay_id", giao_dich.Id);
        command.Parameters.AddWithValue("@noi_dung", Cat(giao_dich.NoiDung, 500));
        command.Parameters.AddWithValue("@so_tien", giao_dich.SoTien);
        command.Parameters.AddWithValue("@thoi_diem", giao_dich.ThoiDiem);
        command.Parameters.AddWithValue("@ma_tham_chieu", (object?)Cat(giao_dich.MaThamChieu, 100) ?? DBNull.Value);
        return await command.ExecuteNonQueryAsync(ct) > 0;
    }

    public async Task<bool> DaKhopChuaAsync(string sepayId, CancellationToken ct = default)
    {
        const string sql = "SELECT da_khop FROM giao_dich_sepay WHERE sepay_id = @sepay_id";

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@sepay_id", sepayId);
        var ket_qua = await command.ExecuteScalarAsync(ct);
        return ket_qua is true;
    }

    public async Task DanhDauAsync(string sepayId, int? hoaDonId, string? lyDoLoai, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE giao_dich_sepay
            SET hoa_don_id = @hoa_don_id, da_khop = @da_khop, ly_do_loai = @ly_do_loai
            WHERE sepay_id = @sepay_id
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@sepay_id", sepayId);
        command.Parameters.AddWithValue("@hoa_don_id", (object?)hoaDonId ?? DBNull.Value);
        command.Parameters.AddWithValue("@da_khop", hoaDonId.HasValue);
        command.Parameters.AddWithValue("@ly_do_loai", (object?)Cat(lyDoLoai, 255) ?? DBNull.Value);
        await command.ExecuteNonQueryAsync(ct);
    }

    public async Task<DateTime?> LayMocThoiDiemAsync(CancellationToken ct = default)
    {
        const string sql = "SELECT MAX(thoi_diem) FROM giao_dich_sepay";

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        var ket_qua = await command.ExecuteScalarAsync(ct);
        return ket_qua is DateTime moc ? moc : null;
    }

    private static string? Cat(string? value, int max) =>
        value is null ? null : value.Length <= max ? value : value[..max];
}
