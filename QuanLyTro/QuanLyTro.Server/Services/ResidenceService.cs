using QuanLyTro.Server.Repositories;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Services;

/// <summary>Lịch sử lưu trú cho Công an (PM-04, PM-05): tra cứu theo khoảng ngày và xuất CSV.</summary>
public sealed class ResidenceService
{
    /// <summary>Khoảng mặc định khi Client không gửi ngày — chặn truy vấn toàn bảng.</summary>
    public const int DefaultWindowDays = 90;

    /// <summary>Trần khoảng ngày: quá rộng thì từ chối rõ ràng thay vì quét cả bảng.</summary>
    public const int MaxWindowDays = 366;

    /// <summary>Spec §2.3 — tab "Biến động" chỉ xem trước 10 dòng trước khi xuất.</summary>
    public const int PreviewRowLimit = 10;

    private readonly ResidenceRepository _repo;

    public ResidenceService(ResidenceRepository repo)
    {
        _repo = repo;
    }

    /// <summary>Xem trước: bị chặn <see cref="PreviewRowLimit"/> dòng (PM-05).</summary>
    public Task<List<ResidenceHistoryDto>> GetHistoryAsync(DateTime? from, DateTime? to, string? room, CancellationToken ct)
    {
        var (start, end) = NormalizeWindow(from, to);
        return _repo.GetHistoryAsync(start, end, room, PreviewRowLimit, ct);
    }

    public async Task<ExportResult> ExportHistoryAsync(ExportHistoryRequest req, CancellationToken ct)
    {
        var (start, end) = NormalizeWindow(req.FromDate, req.ToDate);
        var data = await _repo.GetHistoryAsync(start, end, req.RoomNumber, null, ct);

        // ponytail: CSV đơn giản, không thư viện ngoài. Nâng lên Excel/PDF khi thật cần định dạng.
        var path = Path.Combine(Path.GetTempPath(), $"LuuTru_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        using var writer = new StreamWriter(path, false, new System.Text.UTF8Encoding(true));
        await writer.WriteLineAsync("Họ tên,CCCD,Số phòng,Sự kiện,Ngày,Ghi chú");
        foreach (var r in data)
        {
            await writer.WriteLineAsync(
                $"{r.FullName},{r.IdCard},{r.RoomNumber},{r.EventType},{r.EventDate:dd/MM/yyyy},{r.Notes}");
        }

        return new ExportResult(path, data.Count);
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
            throw new BusinessRuleException("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");
        }

        if ((endDay - startDay).TotalDays > MaxWindowDays)
        {
            throw new BusinessRuleException($"Khoảng ngày tối đa {MaxWindowDays} ngày. Vui lòng thu hẹp khoảng tra cứu.");
        }

        return (startDay, endDay.AddDays(1).AddTicks(-1));
    }
}
