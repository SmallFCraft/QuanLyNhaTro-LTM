using MySqlConnector;
using QuanLyTro.Server.Data;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Server.Repositories;

/// <summary>Truy vấn bảng `quyen_vai_tro` — ma trận quyền động theo vai trò.</summary>
public sealed class PhanQuyenRepository(Database database) : IPhanQuyenRepository
{
    public async Task<Dictionary<string, List<string>>> GetAllAsync(CancellationToken ct = default)
    {
        const string sql = "SELECT vai_tro, hanh_dong FROM quyen_vai_tro ORDER BY vai_tro, hanh_dong";

        var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);

        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var vai_tro = reader.GetString("vai_tro");
            if (!result.TryGetValue(vai_tro, out var hanh_dong))
            {
                hanh_dong = [];
                result[vai_tro] = hanh_dong;
            }

            hanh_dong.Add(reader.GetString("hanh_dong"));
        }

        // Bảng trống (DB mới hoặc bị xóa tay): dùng quyền mặc định để hệ thống không chặn hết mọi người.
        if (result.Count == 0)
        {
            foreach (var (vai_tro, hanh_dong) in QuyenMacDinhTheoVaiTro.All)
            {
                result[vai_tro] = [.. hanh_dong];
            }
        }

        return result;
    }

    public async Task UpdateRoleActionsAsync(string vai_tro, IEnumerable<string> hanh_dong, CancellationToken ct = default)
    {
        await using var connection = await database.OpenAsync(ct);
        await using var giao_dich = await connection.BeginTransactionAsync(ct);

        const string deleteSql = "DELETE FROM quyen_vai_tro WHERE vai_tro = @vai_tro";
        await using (var deleteCmd = new MySqlCommand(deleteSql, connection, giao_dich))
        {
            deleteCmd.Parameters.AddWithValue("@vai_tro", vai_tro);
            await deleteCmd.ExecuteNonQueryAsync(ct);
        }

        const string insertSql = "INSERT INTO quyen_vai_tro (vai_tro, hanh_dong) VALUES (@vai_tro, @hanh_dong)";
        foreach (var actionName in hanh_dong.Distinct(StringComparer.Ordinal))
        {
            await using var insertCmd = new MySqlCommand(insertSql, connection, giao_dich);
            insertCmd.Parameters.AddWithValue("@vai_tro", vai_tro);
            insertCmd.Parameters.AddWithValue("@hanh_dong", actionName);
            await insertCmd.ExecuteNonQueryAsync(ct);
        }

        await giao_dich.CommitAsync(ct);
    }
}
