using QuanLyTro.Server.Repositories;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Services;

/// <summary>Nghiệp vụ phòng: BR-01 (số phòng hợp lệ, giá &gt; 0), BR-12 (chỉ xóa phòng trống).</summary>
public sealed class RoomService(RoomRepository rooms)
{
    private const int MaxRoomNumberLength = 20;

    public Task<List<RoomDto>> GetAllAsync(CancellationToken ct = default) => rooms.GetAllAsync(ct);

    public async Task<RoomDto> AddAsync(RoomDto room, CancellationToken ct = default)
    {
        Validate(room);
        return await rooms.AddAsync(Normalize(room), ct);
    }

    public async Task<bool> UpdateAsync(RoomDto room, CancellationToken ct = default)
    {
        Validate(room);
        return await rooms.UpdateAsync(Normalize(room), ct);
    }

    public async Task<bool> DeleteAsync(int roomId, CancellationToken ct = default)
    {
        if (!await rooms.CanDeleteAsync(roomId, ct))
        {
            throw new BusinessRuleException("Không thể xóa: phòng còn người thuê hoặc hợp đồng đang hiệu lực.");
        }

        return await rooms.DeleteAsync(roomId, ct);
    }

    /// <summary>BR-01. Ném BusinessRuleException với thông báo tiếng Việt hiển thị thẳng cho người dùng.</summary>
    public static void Validate(RoomDto room)
    {
        var number = room.RoomNumber?.Trim() ?? string.Empty;
        if (number.Length == 0)
        {
            throw new BusinessRuleException("Số phòng không được để trống.");
        }

        if (number.Length > MaxRoomNumberLength)
        {
            throw new BusinessRuleException($"Số phòng tối đa {MaxRoomNumberLength} ký tự.");
        }

        if (room.Price <= 0)
        {
            throw new BusinessRuleException("Giá thuê phải lớn hơn 0.");
        }

        if (room.MaxOccupants <= 0)
        {
            throw new BusinessRuleException("Sức chứa phải lớn hơn 0.");
        }

        if (room.MaxOccupants > 50)
        {
            throw new BusinessRuleException("Sức chứa tối đa 50 người.");
        }
    }

    private static RoomDto Normalize(RoomDto room) =>
        room with { RoomNumber = room.RoomNumber.Trim() };
}
