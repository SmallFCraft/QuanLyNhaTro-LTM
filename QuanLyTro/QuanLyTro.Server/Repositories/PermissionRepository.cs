using MySqlConnector;
using QuanLyTro.Server.Data;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Server.Repositories;

/// <summary>Truy vấn bảng `role_permissions` — ma trận quyền động theo vai trò.</summary>
public sealed class PermissionRepository(Database database) : IPermissionRepository
{
    public async Task<Dictionary<string, List<string>>> GetAllAsync(CancellationToken ct = default)
    {
        const string sql = "SELECT role, action FROM role_permissions ORDER BY role, action";

        var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        await using var connection = await database.OpenAsync(ct);
        await using var command = new MySqlCommand(sql, connection);

        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var role = reader.GetString("role");
            if (!result.TryGetValue(role, out var actions))
            {
                actions = [];
                result[role] = actions;
            }

            actions.Add(reader.GetString("action"));
        }

        // Bảng trống (DB mới hoặc bị xóa tay): dùng quyền mặc định để hệ thống không chặn hết mọi người.
        if (result.Count == 0)
        {
            foreach (var (role, actions) in DefaultRolePermissions.All)
            {
                result[role] = [.. actions];
            }
        }

        return result;
    }

    public async Task UpdateRoleActionsAsync(string role, IEnumerable<string> actions, CancellationToken ct = default)
    {
        await using var connection = await database.OpenAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);

        const string deleteSql = "DELETE FROM role_permissions WHERE role = @role";
        await using (var deleteCmd = new MySqlCommand(deleteSql, connection, transaction))
        {
            deleteCmd.Parameters.AddWithValue("@role", role);
            await deleteCmd.ExecuteNonQueryAsync(ct);
        }

        const string insertSql = "INSERT INTO role_permissions (role, action) VALUES (@role, @action)";
        foreach (var action in actions.Distinct(StringComparer.Ordinal))
        {
            await using var insertCmd = new MySqlCommand(insertSql, connection, transaction);
            insertCmd.Parameters.AddWithValue("@role", role);
            insertCmd.Parameters.AddWithValue("@action", action);
            await insertCmd.ExecuteNonQueryAsync(ct);
        }

        await transaction.CommitAsync(ct);
    }
}
