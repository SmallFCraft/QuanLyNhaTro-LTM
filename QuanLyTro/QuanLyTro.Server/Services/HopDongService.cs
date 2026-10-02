using QuanLyTro.Server.Repositories;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Services;

/// <summary>
/// Nghiệp vụ hợp đồng: BR-04 (1 HĐ HieuLuc / phòng), BR-05 (đại diện phải ở trong phòng),
/// BR-06 (NgayKetThuc &gt; NgayBatDau), US-10 (gia hạn), US-11 (danh sách sắp hết hạn).
/// </summary>
public sealed class HopDongService(IHopDongRepository hop_dong)
{
    /// <summary>BR-04, BR-05, BR-06. Hợp đồng tạo mới luôn ở trạng thái HieuLuc.</summary>
    public async Task<HopDongDto> CreateAsync(HopDongDto contract, CancellationToken ct = default)
    {
        ValidateDates(contract.NgayBatDau, contract.NgayKetThuc);

        if (contract.GiaThue <= 0)
        {
            throw new LoiNghiepVu("Giá thuê phải lớn hơn 0.");
        }

        if (contract.TienCoc < 0)
        {
            throw new LoiNghiepVu("Tiền cọc không được âm.");
        }

        if (await hop_dong.HasActiveContractAsync(contract.PhongId, ct))
        {
            throw new LoiNghiepVu("Phòng này đang có hợp đồng hiệu lực.");
        }

        if (!await hop_dong.IsTenantInRoomAsync(contract.NguoiDaiDienId, contract.PhongId, ct))
        {
            throw new LoiNghiepVu("Người đại diện phải là người đang ở trong phòng này.");
        }

        var toSave = contract with
        {
            TrangThai = TrangThaiHopDong.HieuLuc,
            GhiChu = contract.GhiChu?.Trim(),
        };

        return await hop_dong.AddAsync(toSave, ct);
    }

    public async Task<bool> TerminateAsync(int hopDongId, string? ghi_chu, CancellationToken ct = default)
    {
        if (!await hop_dong.TerminateAsync(hopDongId, ghi_chu?.Trim(), ct))
        {
            throw new LoiNghiepVu("Không tìm thấy hợp đồng đang hiệu lực để chấm dứt.");
        }

        return true;
    }

    /// <summary>US-10: chỉ gia hạn hợp đồng đang hiệu lực, ngày mới phải sau ngày kết thúc hiện tại.</summary>
    public async Task<bool> RenewAsync(int hopDongId, DateOnly newEndDate, CancellationToken ct = default)
    {
        var existing = await hop_dong.GetByIdAsync(hopDongId, ct)
            ?? throw new LoiNghiepVu("Không tìm thấy hợp đồng.");

        if (existing.TrangThai != TrangThaiHopDong.HieuLuc)
        {
            throw new LoiNghiepVu("Chỉ gia hạn được hợp đồng đang hiệu lực.");
        }

        if (newEndDate <= existing.NgayKetThuc)
        {
            throw new LoiNghiepVu("Ngày gia hạn phải sau ngày kết thúc hiện tại.");
        }

        if (!await hop_dong.UpdateEndDateAsync(hopDongId, newEndDate, ct))
        {
            // UPDATE có điều kiện ngay_ket_thuc < @ngayKetThuc trả 0 dòng: dữ liệu đã đổi giữa chừng
            // (race) hoặc hợp đồng không còn HieuLuc/tồn tại. Báo lỗi nghiệp vụ thay vì "thành công".
            throw new LoiNghiepVu("Ngày gia hạn phải sau ngày kết thúc hiện tại.");
        }

        return true;
    }

    /// <summary>US-11: kèm số phòng + tên đại diện, xếp theo ngay_ket_thuc tăng dần.</summary>
    public Task<List<MucHopDongItem>> GetAllAsync(CancellationToken ct = default) =>
        hop_dong.GetAllAsync(ct);

    /// <summary>BR-06.</summary>
    public static void ValidateDates(DateOnly ngayBatDau, DateOnly ngayKetThuc)
    {
        if (ngayKetThuc <= ngayBatDau)
        {
            throw new LoiNghiepVu("Ngày kết thúc phải sau ngày bắt đầu.");
        }
    }
}
