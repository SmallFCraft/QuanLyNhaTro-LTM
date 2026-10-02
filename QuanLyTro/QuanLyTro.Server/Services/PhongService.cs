using QuanLyTro.Server.Repositories;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Services;

/// <summary>Nghiệp vụ phòng: BR-01 (số phòng hợp lệ, giá &gt; 0), BR-12 (chỉ xóa phòng trống).</summary>
public sealed class PhongService(PhongRepository phong)
{
    private const int MaxRoomNumberLength = 20;

    public Task<List<PhongDto>> GetAllAsync(CancellationToken ct = default) => phong.GetAllAsync(ct);

    public async Task<PhongDto> AddAsync(PhongDto room, CancellationToken ct = default)
    {
        Validate(room);
        return await phong.AddAsync(Normalize(room), ct);
    }

    public async Task<bool> UpdateAsync(PhongDto room, CancellationToken ct = default)
    {
        Validate(room);
        return await phong.UpdateAsync(Normalize(room), ct);
    }

    public async Task<bool> DeleteAsync(int phongId, CancellationToken ct = default) =>
        // BR-12: toàn bộ kiểm tra + xóa nằm trong một giao_dich có khóa dòng phòng ở repository.
        await phong.DeleteAsync(phongId, ct);

    /// <summary>BR-01. Ném LoiNghiepVu với thông báo tiếng Việt hiển thị thẳng cho người dùng.</summary>
    public static void Validate(PhongDto room)
    {
        var number = room.SoPhong?.Trim() ?? string.Empty;
        if (number.Length == 0)
        {
            throw new LoiNghiepVu("Số phòng không được để trống.");
        }

        if (number.Length > MaxRoomNumberLength)
        {
            throw new LoiNghiepVu($"Số phòng tối đa {MaxRoomNumberLength} ký tự.");
        }

        if (room.GiaThue <= 0)
        {
            throw new LoiNghiepVu("Giá thuê phải lớn hơn 0.");
        }

        if (room.SoNguoiToiDa <= 0)
        {
            throw new LoiNghiepVu("Sức chứa phải lớn hơn 0.");
        }

        if (room.SoNguoiToiDa > 50)
        {
            throw new LoiNghiepVu("Sức chứa tối đa 50 người.");
        }
    }

    private static PhongDto Normalize(PhongDto room) =>
        room with { SoPhong = room.SoPhong.Trim() };
}
