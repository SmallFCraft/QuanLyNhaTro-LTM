using MySqlConnector;
using QuanLyTro.Server.Data;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Repositories;

/// <summary>Biên truy cập bảng `cau_hinh` (một dòng, id = 1).</summary>
public interface ICauHinhRepository
{
    /// <summary>Bản công khai — KHÔNG có api_token (khách thuê cũng đọc).</summary>
    Task<CauHinhThanhToanDto> GetAsync(CancellationToken ct = default);

    /// <summary>Bản đầy đủ kèm api_token — chỉ dùng cho Chủ trọ.</summary>
    Task<CauHinhDayDuDto> GetFullAsync(CancellationToken ct = default);

    Task SaveAsync(CauHinhThanhToanDto cau_hinh, string? api_token, CancellationToken ct = default);
}

/// <summary>Dùng khi router được dựng trong test không có DB — cấu hình coi như chưa thiết lập.</summary>
public sealed class NullCauHinhRepository : ICauHinhRepository
{
    public Task<CauHinhThanhToanDto> GetAsync(CancellationToken ct = default) =>
        Task.FromResult(new CauHinhThanhToanDto(string.Empty, string.Empty, string.Empty));

    public Task<CauHinhDayDuDto> GetFullAsync(CancellationToken ct = default) =>
        Task.FromResult(new CauHinhDayDuDto(string.Empty, string.Empty, string.Empty, "compact", string.Empty, false));

    public Task SaveAsync(CauHinhThanhToanDto cau_hinh, string? api_token, CancellationToken ct = default) =>
        Task.CompletedTask;
}

/// <summary>
/// Cấu hình nhận tiền dùng cho QR SePay. Bảng luôn có đúng 1 dòng (id = 1): Get đọc dòng đó,
/// Save ghi đè bằng INSERT ... ON DUPLICATE KEY UPDATE nên DB mới chưa seed cũng không lỗi.
/// </summary>
public sealed class CauHinhRepository(Database database) : ICauHinhRepository
{
    public async Task<CauHinhThanhToanDto> GetAsync(CancellationToken ct = default)
    {
        // KHÔNG select api_token ở đây: DTO này còn được trả cho khách thuê qua CAU_HINH_LAY.
        const string sql = """
            SELECT ngan_hang, so_tai_khoan, chu_tai_khoan, template_qr
            FROM cau_hinh WHERE id = 1
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(ct);

        if (!await reader.ReadAsync(ct))
        {
            return new CauHinhThanhToanDto(string.Empty, string.Empty, string.Empty);
        }

        return new CauHinhThanhToanDto(
            reader.GetString("ngan_hang"),
            reader.GetString("so_tai_khoan"),
            reader.GetString("chu_tai_khoan"),
            reader.GetString("template_qr"));
    }

    public async Task<CauHinhDayDuDto> GetFullAsync(CancellationToken ct = default)
    {
        const string sql = """
            SELECT ngan_hang, so_tai_khoan, chu_tai_khoan, template_qr, api_token
            FROM cau_hinh WHERE id = 1
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(ct);

        if (!await reader.ReadAsync(ct))
        {
            return new CauHinhDayDuDto(string.Empty, string.Empty, string.Empty, "compact", string.Empty, false);
        }

        var token = reader.GetString("api_token");
        return new CauHinhDayDuDto(
            reader.GetString("ngan_hang"),
            reader.GetString("so_tai_khoan"),
            reader.GetString("chu_tai_khoan"),
            reader.GetString("template_qr"),
            token,
            !string.IsNullOrWhiteSpace(token));
    }

    public async Task SaveAsync(CauHinhThanhToanDto cau_hinh, string? api_token, CancellationToken ct = default)
    {
        // api_token null = giữ nguyên token cũ; chuỗi rỗng = xoá token. Nhờ vậy form không cần
        // gửi lại token mỗi lần lưu (và token không phải rời khỏi server).
        const string sql = """
            INSERT INTO cau_hinh (id, ngan_hang, so_tai_khoan, chu_tai_khoan, template_qr, api_token)
            VALUES (1, @ngan_hang, @so_tai_khoan, @chu_tai_khoan, @template_qr, COALESCE(@api_token, ''))
            ON DUPLICATE KEY UPDATE
                ngan_hang = VALUES(ngan_hang),
                so_tai_khoan = VALUES(so_tai_khoan),
                chu_tai_khoan = VALUES(chu_tai_khoan),
                template_qr = VALUES(template_qr),
                api_token = COALESCE(@api_token, api_token)
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@ngan_hang", cau_hinh.NganHang);
        command.Parameters.AddWithValue("@so_tai_khoan", cau_hinh.SoTaiKhoan);
        command.Parameters.AddWithValue("@chu_tai_khoan", cau_hinh.ChuTaiKhoan);
        command.Parameters.AddWithValue("@template_qr", cau_hinh.TemplateQr);
        command.Parameters.AddWithValue("@api_token", (object?)api_token ?? DBNull.Value);
        await command.ExecuteNonQueryAsync(ct);
    }
}
