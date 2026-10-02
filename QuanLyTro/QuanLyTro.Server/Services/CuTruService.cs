using QuanLyTro.Server.Repositories;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Services;

/// <summary>Lịch sử lưu trú cho Công an (PM-04, PM-05): tra cứu theo khoảng ngày và xuất CSV.</summary>
public sealed class CuTruService
{
    /// <summary>Khoảng mặc định khi Client không gửi ngày — chặn truy vấn toàn bảng.</summary>
    public const int DefaultWindowDays = 90;

    /// <summary>Trần khoảng ngày: quá rộng thì từ chối rõ ràng thay vì quét cả bảng.</summary>
    public const int MaxWindowDays = 366;

    /// <summary>Spec §2.3 — tab "Biến động" chỉ xem trước 10 dòng trước khi xuất.</summary>
    public const int PreviewRowLimit = 10;

    private readonly CuTruRepository _repo;

    public CuTruService(CuTruRepository repo)
    {
        _repo = repo;
    }

    /// <summary>Xem trước: bị chặn <see cref="PreviewRowLimit"/> dòng (PM-05).</summary>
    public Task<List<LichSuCuTruDto>> GetHistoryAsync(DateTime? from, DateTime? to, string? room, CancellationToken ct)
    {
        var (start, end) = NormalizeWindow(from, to);
        return _repo.GetHistoryAsync(start, end, room, PreviewRowLimit, ct);
    }

    public async Task<KetQuaXuatFile> ExportHistoryAsync(YeuCauXuatLichSu req, CancellationToken ct)
    {
        var (start, end) = NormalizeWindow(req.TuNgay, req.DenNgay);
        var data = await _repo.GetHistoryAsync(start, end, req.SoPhong, null, ct);

        // ponytail: CSV đơn giản, không thư viện ngoài. Nâng lên Excel/PDF khi thật cần định dạng.
        var path = Path.Combine(Path.GetTempPath(), $"LuuTru_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        using var writer = new StreamWriter(path, false, new System.Text.UTF8Encoding(true));
        await writer.WriteLineAsync("Họ tên,CCCD,Số phòng,Sự kiện,Ngày,Ghi chú");
        foreach (var r in data)
        {
            await writer.WriteLineAsync(
                $"{r.HoTen},{r.Cccd},{r.SoPhong},{r.LoaiBienDong},{r.NgayBienDong:dd/MM/yyyy},{r.GhiChu}");
        }

        return new KetQuaXuatFile(path, data.Count);
    }

    /// <summary>
    /// Thiếu/để trống ngày → mặc định <see cref="DefaultWindowDays"/> ngày gần nhất; ngày cuối
    /// được mở rộng hết ngày đó (Client gửi yyyy-MM-dd nên nếu không mở rộng sẽ mất bản ghi trong ngày).
    /// </summary>
    private static (DateTime From, DateTime To) NormalizeWindow(DateTime? from, DateTime? to)
    {
        var endDay = to is { } t && t != default ? t.Date : DateTime.Now.Date;
        var startDay = from is { } f && f != default ? f.Date : endDay.AddDays(-DefaultWindowDays);

        if (endDay < startDay)
        {
            throw new LoiNghiepVu("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");
        }

        if ((endDay - startDay).TotalDays > MaxWindowDays)
        {
            throw new LoiNghiepVu($"Khoảng ngày tối đa {MaxWindowDays} ngày. Vui lòng thu hẹp khoảng tra cứu.");
        }

        return (startDay, endDay.AddDays(1).AddTicks(-1));
    }
}
