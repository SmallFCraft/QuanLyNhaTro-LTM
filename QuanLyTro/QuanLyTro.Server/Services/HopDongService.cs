using System.Security.Cryptography;
using QuanLyTro.Server.Repositories;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Services;

/// <summary>
/// Nghiệp vụ hợp đồng: BR-04 (1 HĐ HieuLuc / phòng), BR-05 (đại diện phải ở trong phòng),
/// BR-06 (NgayKetThuc &gt; NgayBatDau), US-10 (gia hạn), US-11 (danh sách sắp hết hạn),
/// BR-18/BR-19 (sinh mã QR/PIN và xác thực nhận phòng một lần).
/// </summary>
public sealed class HopDongService(IHopDongRepository hop_dong)
{
    private const int QrTokenHexLength = 32;
    private const string QrPrefix = "QUANLYTRO:NHANPHONG:";

    /// <summary>BR-18: 32 ký tự hex = 128-bit entropy từ CSPRNG.</summary>
    public static string SinhMaQrToken() => RandomNumberGenerator.GetHexString(QrTokenHexLength);

    /// <summary>BR-18: PIN 8 chữ số, sinh bằng CSPRNG (không dùng Random).</summary>
    public static string SinhMaPin() =>
        RandomNumberGenerator.GetInt32(0, 100_000_000).ToString("D8");

    /// <summary>BR-18: định dạng chuỗi nhúng vào ảnh QR.</summary>
    public static string DinhDangQrPayload(string maQrToken) => QrPrefix + maQrToken;

    /// <summary>BR-19: nhận cả payload QR đầy đủ lẫn token/PIN thô do khách nhập tay.</summary>
    public static string TrichXuatToken(string tokenOrPayload)
    {
        var trimmed = tokenOrPayload.Trim();
        return trimmed.StartsWith(QrPrefix, StringComparison.Ordinal)
            ? trimmed[QrPrefix.Length..]
            : trimmed;
    }

    /// <summary>BR-04, BR-05, BR-06, BR-18. HĐ `ChoNhanPhong` sinh kèm token + PIN dùng một lần.</summary>
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

        var choNhanPhong = contract.TrangThai == TrangThaiHopDong.ChoNhanPhong;

        // BR-05 chỉ áp cho bàn giao trực tiếp: HĐ chờ nhận phòng có đại diện chưa ở trong phòng.
        if (!choNhanPhong && !await hop_dong.IsTenantInRoomAsync(contract.NguoiDaiDienId, contract.PhongId, ct))
        {
            throw new LoiNghiepVu("Người đại diện phải là người đang ở trong phòng này.");
        }

        var toSave = contract with
        {
            TrangThai = choNhanPhong ? TrangThaiHopDong.ChoNhanPhong : TrangThaiHopDong.HieuLuc,
            GhiChu = contract.GhiChu?.Trim(),
            MaQrToken = choNhanPhong ? contract.MaQrToken ?? SinhMaQrToken() : null,
            MaPin = choNhanPhong ? contract.MaPin ?? SinhMaPin() : null,
        };

        return await hop_dong.AddAsync(toSave, ct);
    }

    /// <summary>BR-19: khách thuê quét QR / nhập PIN để kích hoạt nhận phòng (dùng một lần).</summary>
    public Task<KetQuaNhanPhongDto> CheckinByQrAsync(
        int khachThueId, string tokenOrPin, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tokenOrPin))
        {
            throw new LoiNghiepVu("Mã nhận phòng không hợp lệ hoặc đã được sử dụng.");
        }

        return hop_dong.CheckinByQrAsync(khachThueId, TrichXuatToken(tokenOrPin), ct);
    }

    /// <summary>BR-18: chủ trọ lấy lại thông tin QR/PIN của HĐ đang chờ nhận phòng.</summary>
    public async Task<ThongTinSinhQrDto> SinhQrAsync(int hopDongId, CancellationToken ct = default)
    {
        var contract = await hop_dong.GetByIdAsync(hopDongId, ct)
            ?? throw new LoiNghiepVu("Không tìm thấy hợp đồng.");

        if (contract.TrangThai != TrangThaiHopDong.ChoNhanPhong)
        {
            throw new LoiNghiepVu("Hợp đồng này không ở trạng thái chờ nhận phòng.");
        }

        if (contract.MaQrToken is null || contract.MaPin is null)
        {
            throw new LoiNghiepVu("Hợp đồng chưa có mã QR/PIN.");
        }

        return new ThongTinSinhQrDto(
            contract.Id, contract.MaQrToken, contract.MaPin, DinhDangQrPayload(contract.MaQrToken));
    }

    /// <summary>
    /// Chấm dứt hợp đồng còn thời hạn = chủ trọ đơn phương chấm dứt sớm (Điều 172 Luật Nhà ở 2023).
    /// Bắt buộc nhập LÝ DO để lưu thành căn cứ thông báo; hợp đồng phải có lý do hợp lệ mới chấm dứt.
    /// Lưu cả ngày thông báo để sau này truy vết/thanh toán cước tồn.
    /// </summary>
    public async Task<bool> TerminateAsync(int hopDongId, string? ghi_chu, CancellationToken ct = default)
    {
        var contract = await hop_dong.GetByIdAsync(hopDongId, ct)
            ?? throw new LoiNghiepVu("Không tìm thấy hợp đồng.");

        if (contract.TrangThai is not (TrangThaiHopDong.HieuLuc or TrangThaiHopDong.ChoNhanPhong))
        {
            throw new LoiNghiepVu("Chỉ chấm dứt được hợp đồng đang hiệu lực hoặc đang chờ nhận phòng.");
        }

        var canhHiep = contract.TrangThai == TrangThaiHopDong.ChoNhanPhong
            || contract.NgayKetThuc > DateOnly.FromDateTime(DateTime.Today);
        if (canhHiep && string.IsNullOrWhiteSpace(ghi_chu))
        {
            throw new LoiNghiepVu("Chấm dứt hợp đồng còn thời hạn phải ghi rõ lý do (nợ tiền thuê 3 tháng, dùng sai mục đích, tự ý cải tạo...).");
        }

        if (!await hop_dong.TerminateAsync(hopDongId, ghi_chu?.Trim(), DateOnly.FromDateTime(DateTime.Today), ct))
        {
            throw new LoiNghiepVu("Không tìm thấy hợp đồng có thể chấm dứt (đang hiệu lực hoặc chờ nhận phòng).");
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
