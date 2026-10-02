using QuanLyTro.Server.Repositories;
using QuanLyTro.Server.Security;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Services;

/// <summary>
/// Nghiệp vụ người thuê: BR-02 (không vượt sức chứa, giao_dich + khóa phòng),
/// BR-03 (CCCD duy nhất), US-06 (chỉ xóa hồ sơ đã trả phòng), mật khẩu mặc định 6 số cuối CCCD.
/// </summary>
public sealed class KhachThueService(KhachThueRepository khach_thue)
{
    private const int IdCardLength = 12;
    private const int DefaultPasswordDigits = 6;
    private const int MaxNameLength = 100;
    private const int MaxPhoneLength = 20;
    private const int MaxHometownLength = 150;
    private const int MaxWorkplaceLength = 150;

    public Task<List<KhachThueDto>> GetByRoomAsync(int phongId, CancellationToken ct = default) =>
        khach_thue.GetByRoomAsync(phongId, ct);

    public async Task<KhachThueDto> AddAsync(KhachThueDto tenant, string? plainPassword, CancellationToken ct = default)
    {
        Validate(tenant);
        var normalized = Normalize(tenant);

        if (normalized.PhongId is { } phongId && !await khach_thue.RoomExistsAsync(phongId, ct))
        {
            throw new LoiNghiepVu("Phòng không tồn tại.");
        }

        var hash = PasswordHasher.Hash(ResolvePassword(normalized, plainPassword));
        return await khach_thue.AddAsync(normalized, hash, ct);
    }

    public async Task<bool> UpdateAsync(KhachThueDto tenant, string? plainPassword, CancellationToken ct = default)
    {
        Validate(tenant);

        var hash = string.IsNullOrWhiteSpace(plainPassword)
            ? null
            : PasswordHasher.Hash(plainPassword);

        return await khach_thue.UpdateAsync(Normalize(tenant), hash, ct);
    }

    /// <summary>Trả phòng: phong_id = NULL; phòng không còn ai và không còn HĐ HieuLuc thì về Trong.</summary>
    public async Task<bool> CheckoutAsync(int khachThueId, CancellationToken ct = default)
    {
        if (await khach_thue.GetRoomIdAsync(khachThueId, ct) is null)
        {
            throw new LoiNghiepVu("Người thuê không tồn tại hoặc đã trả phòng.");
        }

        return await khach_thue.CheckoutAsync(khachThueId, ct);
    }

    /// <summary>US-06: chỉ xóa hồ sơ đã trả phòng (phong_id IS NULL).</summary>
    public async Task<bool> DeleteAsync(int khachThueId, CancellationToken ct = default)
    {
        return await khach_thue.DeleteAsync(khachThueId, ct) switch
        {
            KetQuaXoaKhachThue.DaXoa => true,
            KetQuaXoaKhachThue.ConTrongPhong => throw new LoiNghiepVu("Chỉ xóa được hồ sơ đã trả phòng."),
            _ => throw new LoiNghiepVu("Người thuê không tồn tại."),
        };
    }

    /// <summary>Mật khẩu mặc định khi chủ trọ để trống: 6 số cuối CCCD.</summary>
    public static string DefaultPassword(string cccd)
    {
        var digits = cccd.Trim();
        return digits.Length > DefaultPasswordDigits ? digits[^DefaultPasswordDigits..] : digits;
    }

    private static string ResolvePassword(KhachThueDto tenant, string? plainPassword) =>
        string.IsNullOrWhiteSpace(plainPassword) ? DefaultPassword(tenant.Cccd) : plainPassword;

    /// <summary>Ném LoiNghiepVu với thông báo tiếng Việt hiển thị thẳng cho người dùng.</summary>
    public static void Validate(KhachThueDto tenant)
    {
        if (string.IsNullOrWhiteSpace(tenant.HoTen))
        {
            throw new LoiNghiepVu("Họ tên không được để trống.");
        }

        if (tenant.HoTen.Trim().Length > MaxNameLength)
        {
            throw new LoiNghiepVu($"Họ tên tối đa {MaxNameLength} ký tự.");
        }

        var cccd = tenant.Cccd?.Trim() ?? string.Empty;
        if (cccd.Length != IdCardLength || !cccd.All(char.IsAsciiDigit))
        {
            throw new LoiNghiepVu($"Số CCCD phải gồm đúng {IdCardLength} chữ số.");
        }

        if (string.IsNullOrWhiteSpace(tenant.SoDienThoai))
        {
            throw new LoiNghiepVu("Số điện thoại không được để trống.");
        }

        if (tenant.SoDienThoai.Trim().Length > MaxPhoneLength)
        {
            throw new LoiNghiepVu($"Số điện thoại tối đa {MaxPhoneLength} ký tự.");
        }

        if (string.IsNullOrWhiteSpace(tenant.QueQuan))
        {
            throw new LoiNghiepVu("Quê quán không được để trống.");
        }

        if (tenant.QueQuan.Trim().Length > MaxHometownLength)
        {
            throw new LoiNghiepVu($"Quê quán tối đa {MaxHometownLength} ký tự.");
        }

        if (tenant.NoiLamViec?.Trim().Length > MaxWorkplaceLength)
        {
            throw new LoiNghiepVu($"Nơi làm việc tối đa {MaxWorkplaceLength} ký tự.");
        }

        if (tenant.NgaySinh == default)
        {
            throw new LoiNghiepVu("Ngày sinh không hợp lệ.");
        }
    }

    private static KhachThueDto Normalize(KhachThueDto tenant) =>
        tenant with
        {
            HoTen = tenant.HoTen.Trim(),
            Cccd = tenant.Cccd.Trim(),
            SoDienThoai = tenant.SoDienThoai.Trim(),
            QueQuan = tenant.QueQuan.Trim(),
            NoiLamViec = string.IsNullOrWhiteSpace(tenant.NoiLamViec) ? null : tenant.NoiLamViec.Trim(),
        };
}
