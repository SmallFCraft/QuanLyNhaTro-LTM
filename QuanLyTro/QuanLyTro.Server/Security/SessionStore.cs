using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace QuanLyTro.Server.Security;

/// <summary>Phiên đăng nhập in-memory, hết hạn sau 8 giờ.</summary>
public sealed class SessionStore
{
    private const int SessionLifetimeHours = 8;

    private readonly ConcurrentDictionary<string, (int UserId, DateTimeOffset ExpiresAt)> _sessions = new();

    public string Create(int userId)
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        _sessions[token] = (userId, DateTimeOffset.UtcNow.AddHours(SessionLifetimeHours));
        return token;
    }

    public bool TryGetUser(string token, out int userId)
    {
        userId = 0;
        if (string.IsNullOrWhiteSpace(token) || !_sessions.TryGetValue(token, out var entry))
        {
            return false;
        }

        if (entry.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            _sessions.TryRemove(token, out _);
            return false;
        }

        userId = entry.UserId;
        return true;
    }

    public void Remove(string token) => _sessions.TryRemove(token, out _);
}
