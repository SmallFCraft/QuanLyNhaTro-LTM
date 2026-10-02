using System.Collections.Concurrent;
using System.Security.Cryptography;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Security;

/// <summary>Phiên đăng nhập in-memory giữ `(UserId, VaiTro)`, hết hạn sau 8 giờ.</summary>
public sealed class SessionStore
{
    private const int SessionLifetimeHours = 8;

    private readonly ConcurrentDictionary<string, (int UserId, VaiTroNguoiDung VaiTro, DateTimeOffset ExpiresAt)> _sessions = new();

    public string Create(int userId, VaiTroNguoiDung vai_tro)
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        _sessions[token] = (userId, vai_tro, DateTimeOffset.UtcNow.AddHours(SessionLifetimeHours));
        return token;
    }

    public bool TryGet(string token, out (int UserId, VaiTroNguoiDung VaiTro) session)
    {
        session = default;
        if (string.IsNullOrWhiteSpace(token) || !_sessions.TryGetValue(token, out var entry))
        {
            return false;
        }

        if (entry.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            _sessions.TryRemove(token, out _);
            return false;
        }

        session = (entry.UserId, entry.VaiTro);
        return true;
    }

    public void Remove(string token) => _sessions.TryRemove(token, out _);
}
