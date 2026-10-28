using System.Globalization;
using QuanLyTro.Server.Repositories;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Services;

/// <summary>Kết quả một lượt đối soát.</summary>
public sealed record KetQuaDoiSoat(int SoGiaoDichMoi, int SoKhop, int SoBoQua);

/// <summary>
/// Đối soát chuyển khoản: đọc giao dịch tiền vào từ SePay, khớp nội dung với hóa đơn ChuaThu,
/// rồi đánh dấu DaThu. Chạy định kỳ trong server; chủ trọ cũng bấm tay được.
///
/// An toàn:
/// - `giao_dich_sepay.sepay_id` UNIQUE + INSERT IGNORE → mỗi giao dịch chỉ xử lý 1 lần.
/// - Chỉ đánh dấu khi số tiền >= tổng hóa đơn (chuyển thiếu thì để nguyên, ghi lý do).
/// - `MarkPaidAsync` có điều kiện `trang_thai = 'ChuaThu'` nên hai lượt chạy song song không thu 2 lần.
/// </summary>
public sealed class DoiSoatService(
    ICauHinhRepository cau_hinh,
    IGiaoDichSepayRepository giao_dich,
    IHoaDonRepository hoa_don,
    ISePayClient sepay)
{
    /// <summary>Lần poll đầu tiên lùi bao lâu — 30 ngày đủ bao trọn chu kỳ tháng trước.</summary>
    public static readonly TimeSpan KhoangMacDinh = TimeSpan.FromDays(30);

    /// <summary>Một semaphore cho cả server: poller nền và nút "Đối soát ngay" chia sẻ instance,
    /// nên phải chặn hai lượt ChayAsync chạy song song (SePay giới hạn 3 request/giây).</summary>
    private readonly SemaphoreSlim _khoa = new(1, 1);

    public async Task<KetQuaDoiSoat> ChayAsync(CancellationToken ct = default)
    {
        var cfg = await cau_hinh.GetFullAsync(ct);
        if (string.IsNullOrWhiteSpace(cfg.ApiToken))
        {
            throw new LoiNghiepVu("Chưa cấu hình API token SePay. Vui lòng nhập token trong tab Cài đặt.");
        }

        await _khoa.WaitAsync(ct);
        try
        {
            return await ChayNoiBoAsync(cfg.ApiToken, ct);
        }
        finally
        {
            _khoa.Release();
        }
    }

    private async Task<KetQuaDoiSoat> ChayNoiBoAsync(string apiToken, CancellationToken ct)
    {
        var den = DateTime.Now;
        var tu = await giao_dich.LayMocThoiDiemAsync(ct) ?? den - KhoangMacDinh;
        var danh_sach = await sepay.LayGiaoDichVaoAsync(apiToken, tu, den, ct);

        int moi = 0, khop = 0, bo_qua = 0;
        foreach (var gd in danh_sach)
        {
            await giao_dich.ThemNeuChuaCoAsync(gd, ct);

            // Giao dịch đã nằm trong bảng vẫn phải thử khớp lại: lần trước có thể CHƯA có hóa đơn
            // (khách trả trước khi chủ trọ lập hóa đơn) hoặc server sập giữa chừng. Chỉ các giao
            // dịch đã da_khop = TRUE mới được bỏ qua — da_khop là "đã xử lý xong", không phải "đã thấy".
            if (await giao_dich.DaKhopChuaAsync(gd.Id, ct))
            {
                continue;
            }
            moi++;

            var ket_qua = await KhopAsync(gd, ct);
            if (ket_qua.HoaDonId is int id)
            {
                await giao_dich.DanhDauAsync(gd.Id, id, null, ct);
                khop++;
            }
            else
            {
                await giao_dich.DanhDauAsync(gd.Id, null, ket_qua.LyDo, ct);
                bo_qua++;
            }
        }

        return new KetQuaDoiSoat(moi, khop, bo_qua);
    }

    private sealed record KetQuaKhop(int? HoaDonId, string? LyDo);

    private async Task<KetQuaKhop> KhopAsync(GiaoDichSepay gd, CancellationToken ct)
    {
        // Kỳ cước đọc được từ nội dung; nếu không có thì thử mọi kỳ còn nợ của phòng đó.
        var ky = NoiDungThanhToanParser.TimKyTuNoiDung(gd.NoiDung);
        var ung_vien = await TimHoaDonAsync(gd.NoiDung, ky, ct);

        if (ung_vien.Id == 0)
        {
            return new KetQuaKhop(null, ky is null
                ? "Nội dung không chứa số phòng hoặc kỳ cước khớp hóa đơn chưa thu nào."
                : $"Không tìm thấy hóa đơn chưa thu khớp nội dung (kỳ {ky}).");
        }

        if (gd.SoTien < ung_vien.TongTien)
        {
            return new KetQuaKhop(null,
                $"Chuyển thiếu: {gd.SoTien:N0} đ < {ung_vien.TongTien:N0} đ của hóa đơn #{ung_vien.Id}.");
        }

        // MarkPaidAsync trả false khi hóa đơn vừa được người khác thu → coi như bỏ qua, không ghi khớp.
        var da_thu = await hoa_don.MarkPaidAsync(ung_vien.Id, ct);
        return da_thu
            ? new KetQuaKhop(ung_vien.Id, null)
            : new KetQuaKhop(null, $"Hóa đơn #{ung_vien.Id} vừa được thu ở nơi khác.");
    }

    /// <summary>
    /// Tìm hóa đơn chưa thu khớp nội dung. Phòng được khớp trên danh sách phòng THẬT đang nợ
    /// (không đoán mẫu chữ — nội dung ngân hàng đầy mã giao dịch dễ bắt nhầm).
    /// Có kỳ trong nội dung thì chỉ dò kỳ đó; không có thì dò mọi kỳ còn nợ của phòng.
    /// </summary>
    private async Task<(int Id, string SoPhong, decimal TongTien)> TimHoaDonAsync(
        string noiDung, string? ky, CancellationToken ct)
    {
        // Gộp hóa đơn chưa thu của kỳ đọc được (hoặc của 6 tháng gần đây khi không đọc được kỳ).
        var ung_vien = new List<(int Id, string SoPhong, decimal TongTien)>();
        if (ky is not null)
        {
            ung_vien.AddRange(await hoa_don.GetUnpaidWithRoomAsync(ky, ct));
        }
        else
        {
            // Không có kỳ: dò 6 tháng gần đây, đủ cho đồ án và tránh quét toàn bộ lịch sử.
            var moc = DateTime.Now;
            for (var i = 0; i < 6; i++)
            {
                var thang = moc.AddMonths(-i).ToString("yyyy-MM", CultureInfo.InvariantCulture);
                ung_vien.AddRange(await hoa_don.GetUnpaidWithRoomAsync(thang, ct));
            }
        }

        if (ung_vien.Count == 0) return default;

        // Chỉ dò PHÒNG, không dùng Doc(): Doc bắt buộc phải có kỳ nên sẽ luôn trả null ở nhánh
        // không-kỳ, làm hỏng toàn bộ fallback 6 tháng.
        var phong = NoiDungThanhToanParser.TimPhong(noiDung, ung_vien.Select(h => h.SoPhong));
        if (phong is null) return default;

        // Trùng số phòng ở nhiều kỳ: chọn hóa đơn cũ nhất còn nợ — khách thường trả kỳ trước trước.
        return ung_vien
            .Where(h => string.Equals(
                NoiDungThanhToanParser.ChuanHoa(h.SoPhong).Replace(" ", string.Empty),
                NoiDungThanhToanParser.ChuanHoa(phong).Replace(" ", string.Empty),
                StringComparison.OrdinalIgnoreCase))
            .OrderBy(h => h.Id)
            .FirstOrDefault();
    }
}
