using MySqlConnector;
using QuanLyTro.Server.Data;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Repositories;

/// <summary>Truy vấn tài khoản trong bảng `tai_khoan` (chủ trọ hoặc công an phường).</summary>
public sealed class TaiKhoanRepository(Database database) : ITaiKhoanRepository
{
    public async Task<TaiKhoanRecord?> FindByUsernameAsync(string tenDangNhap, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, mat_khau_hash, ho_ten, vai_tro
            FROM tai_khoan
            WHERE ten_dang_nhap = @tenDangNhap
            LIMIT 1
            """;

        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@tenDangNhap", tenDangNhap);

        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
        {
            return null;
        }

        return new TaiKhoanRecord(
            reader.GetInt32("id"),
            reader.GetString("mat_khau_hash"),
            reader.GetString("ho_ten"),
            ParseRole(reader.IsDBNull(reader.GetOrdinal("vai_tro")) ? null : reader.GetString("vai_tro")));
    }

    /// <summary>Cột `vai_tro` là ENUM chuỗi; giá trị lạ/null không bao giờ được fallback ChuTro để tránh leo thang đặc quyền.</summary>
    private static VaiTroNguoiDung ParseRole(string? raw) =>
        Enum.TryParse<VaiTroNguoiDung>(raw, ignoreCase: true, out var vai_tro) ? vai_tro : VaiTroNguoiDung.KhachThue;
}
