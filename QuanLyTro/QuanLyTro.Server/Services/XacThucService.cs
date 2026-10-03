using System.Collections.Concurrent;
using QuanLyTro.Server.Repositories;
using QuanLyTro.Server.Security;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Services;

/// <summary>
/// Đăng nhập 2 tác nhân: thử bảng `tai_khoan` (ChuTro) trước, không khớp thì thử bảng `khach_thue`
/// theo `tenDangNhap = cccd` (KhachThue). Sai thông tin trả CÙNG MỘT thông báo — không tiết lộ
/// bảng nào tồn tại. Khóa tài khoản theo tenDangNhap: quá 5 lần sai liên tiếp → từ chối 1 phút.
/// </summary>
public sealed class XacThucService(ITaiKhoanRepository tai_khoan, IKhachThueRepository khach_thue, SessionStore sessions) : IAuthService
{
    public const int MaxFailedAttempts = 5;
    public static readonly TimeSpan LockoutWindow = TimeSpan.FromMinutes(1);
    public const string InvalidCredentialsMessage = "Tên đăng nhập hoặc mật khẩu không đúng.";

    private readonly ConcurrentDictionary<string, (int Count, DateTimeOffset LockedUntil)> _failures = new();
    private readonly Func<DateTimeOffset> _clock = () => DateTimeOffset.UtcNow;

    public XacThucService(ITaiKhoanRepository tai_khoan, IKhachThueRepository khach_thue, SessionStore sessions, Func<DateTimeOffset>? clock = null)
        : this(tai_khoan, khach_thue, sessions)
    {
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
    }

    public async Task<KetQuaDangNhap> LoginAsync(YeuCauDangNhap request, CancellationToken ct = default)
    {
        var tenDangNhap = request.TenDangNhap.Trim();
        var lockKey = LockoutKey(tenDangNhap);

        if (_failures.TryGetValue(lockKey, out var state) && state.LockedUntil > _clock())
        {
            var remaining = (int)Math.Ceiling((state.LockedUntil - _clock()).TotalSeconds);
            throw new UnauthorizedAccessException($"Tài khoản tạm khóa. Thử lại sau {remaining} giây.");
        }

        var user = await tai_khoan.FindByUsernameAsync(tenDangNhap, ct);
        if (user is not null && PasswordHasher.Verify(request.MatKhau, user.MatKhauHash))
        {
            _failures.TryRemove(lockKey, out _);
            var token = sessions.Create(user.Id, user.VaiTro);
            return new KetQuaDangNhap(token, user.HoTen, user.VaiTro);
        }

        var tenant = await khach_thue.FindByCccdAsync(tenDangNhap, ct);
        if (tenant is not null && tenant.MatKhauHash is not null && PasswordHasher.Verify(request.MatKhau, tenant.MatKhauHash))
        {
            _failures.TryRemove(lockKey, out _);
            var token = sessions.Create(tenant.Id, VaiTroNguoiDung.KhachThue);
            return new KetQuaDangNhap(token, tenant.HoTen, VaiTroNguoiDung.KhachThue);
        }

        RegisterFailure(lockKey);
        throw new UnauthorizedAccessException(InvalidCredentialsMessage);
    }

    // MySQL so khớp tenDangNhap/CCCD không phân biệt hoa/thường (utf8mb4_unicode_ci), nên "Admin" và
    // "admin" là CÙNG tài khoản. Bộ đếm khóa phải theo cùng quy tắc, nếu không kẻ tấn công né khóa
    // bằng cách đổi kiểu chữ sau khi tài khoản bị khóa.
    private static string LockoutKey(string tenDangNhap) => tenDangNhap.ToLowerInvariant();

    private void RegisterFailure(string tenDangNhap)
    {
        _failures.AddOrUpdate(
            tenDangNhap,
            _ => (1, DateTimeOffset.MinValue),
            (_, existing) =>
            {
                // Hết hạn khóa rồi mới sai tiếp → bắt đầu đếm lại từ 1, không kéo dài khóa vô lý.
                var count = existing.LockedUntil > DateTimeOffset.MinValue && existing.LockedUntil <= _clock()
                    ? 1
                    : existing.Count + 1;
                var lockedUntil = count >= MaxFailedAttempts ? _clock().Add(LockoutWindow) : existing.LockedUntil;
                return (count, lockedUntil);
            });
    }
}

/// <summary>Biên XacThucService — cho phép controller/router test không phụ thuộc DB.</summary>
public interface IAuthService
{
    Task<KetQuaDangNhap> LoginAsync(YeuCauDangNhap request, CancellationToken ct = default);
}
