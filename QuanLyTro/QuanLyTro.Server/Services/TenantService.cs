using QuanLyTro.Server.Repositories;
using QuanLyTro.Server.Security;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Services;

/// <summary>
/// Nghiệp vụ người thuê: BR-02 (không vượt sức chứa, transaction + khóa phòng),
/// BR-03 (CCCD duy nhất), US-06 (chỉ xóa hồ sơ đã trả phòng), mật khẩu mặc định 6 số cuối CCCD.
/// </summary>
public sealed class TenantService(TenantRepository tenants)
{
    private const int IdCardLength = 12;
    private const int DefaultPasswordDigits = 6;
    private const int MaxNameLength = 100;
    private const int MaxPhoneLength = 20;
    private const int MaxHometownLength = 150;
    private const int MaxWorkplaceLength = 150;

    public Task<List<TenantDto>> GetByRoomAsync(int roomId, CancellationToken ct = default) =>
        tenants.GetByRoomAsync(roomId, ct);

    public async Task<TenantDto> AddAsync(TenantDto tenant, string? plainPassword, CancellationToken ct = default)
    {
        Validate(tenant);
        var normalized = Normalize(tenant);

        if (normalized.RoomId is { } roomId && !await tenants.RoomExistsAsync(roomId, ct))
        {
            throw new BusinessRuleException("Phòng không tồn tại.");
        }

        var hash = PasswordHasher.Hash(ResolvePassword(normalized, plainPassword));
        return await tenants.AddAsync(normalized, hash, ct);
    }

    public async Task<bool> UpdateAsync(TenantDto tenant, string? plainPassword, CancellationToken ct = default)
    {
        Validate(tenant);

        var hash = string.IsNullOrWhiteSpace(plainPassword)
            ? null
            : PasswordHasher.Hash(plainPassword);

        return await tenants.UpdateAsync(Normalize(tenant), hash, ct);
    }

    /// <summary>Trả phòng: room_id = NULL; phòng không còn ai và không còn HĐ Active thì về Available.</summary>
    public async Task<bool> CheckoutAsync(int tenantId, CancellationToken ct = default)
    {
        if (await tenants.GetRoomIdAsync(tenantId, ct) is null)
        {
            throw new BusinessRuleException("Người thuê không tồn tại hoặc đã trả phòng.");
        }

        return await tenants.CheckoutAsync(tenantId, ct);
    }

    /// <summary>US-06: chỉ xóa hồ sơ đã trả phòng (room_id IS NULL).</summary>
    public async Task<bool> DeleteAsync(int tenantId, CancellationToken ct = default)
    {
        return await tenants.DeleteAsync(tenantId, ct) switch
        {
            TenantDeleteResult.Deleted => true,
            TenantDeleteResult.StillInRoom => throw new BusinessRuleException("Chỉ xóa được hồ sơ đã trả phòng."),
            _ => throw new BusinessRuleException("Người thuê không tồn tại."),
        };
    }

    /// <summary>Mật khẩu mặc định khi chủ trọ để trống: 6 số cuối CCCD.</summary>
    public static string DefaultPassword(string idCard)
    {
        var digits = idCard.Trim();
        return digits.Length > DefaultPasswordDigits ? digits[^DefaultPasswordDigits..] : digits;
    }

    private static string ResolvePassword(TenantDto tenant, string? plainPassword) =>
        string.IsNullOrWhiteSpace(plainPassword) ? DefaultPassword(tenant.IdCard) : plainPassword;

    /// <summary>Ném BusinessRuleException với thông báo tiếng Việt hiển thị thẳng cho người dùng.</summary>
    public static void Validate(TenantDto tenant)
    {
        if (string.IsNullOrWhiteSpace(tenant.FullName))
        {
            throw new BusinessRuleException("Họ tên không được để trống.");
        }

        if (tenant.FullName.Trim().Length > MaxNameLength)
        {
            throw new BusinessRuleException($"Họ tên tối đa {MaxNameLength} ký tự.");
        }

        var idCard = tenant.IdCard?.Trim() ?? string.Empty;
        if (idCard.Length != IdCardLength || !idCard.All(char.IsAsciiDigit))
        {
            throw new BusinessRuleException($"Số CCCD phải gồm đúng {IdCardLength} chữ số.");
        }

        if (string.IsNullOrWhiteSpace(tenant.Phone))
        {
            throw new BusinessRuleException("Số điện thoại không được để trống.");
        }

        if (tenant.Phone.Trim().Length > MaxPhoneLength)
        {
            throw new BusinessRuleException($"Số điện thoại tối đa {MaxPhoneLength} ký tự.");
        }

        if (string.IsNullOrWhiteSpace(tenant.Hometown))
        {
            throw new BusinessRuleException("Quê quán không được để trống.");
        }

        if (tenant.Hometown.Trim().Length > MaxHometownLength)
        {
            throw new BusinessRuleException($"Quê quán tối đa {MaxHometownLength} ký tự.");
        }

        if (tenant.Workplace?.Trim().Length > MaxWorkplaceLength)
        {
            throw new BusinessRuleException($"Nơi làm việc tối đa {MaxWorkplaceLength} ký tự.");
        }

        if (tenant.DateOfBirth == default)
        {
            throw new BusinessRuleException("Ngày sinh không hợp lệ.");
        }
    }

    private static TenantDto Normalize(TenantDto tenant) =>
        tenant with
        {
            FullName = tenant.FullName.Trim(),
            IdCard = tenant.IdCard.Trim(),
            Phone = tenant.Phone.Trim(),
            Hometown = tenant.Hometown.Trim(),
            Workplace = string.IsNullOrWhiteSpace(tenant.Workplace) ? null : tenant.Workplace.Trim(),
        };
}
