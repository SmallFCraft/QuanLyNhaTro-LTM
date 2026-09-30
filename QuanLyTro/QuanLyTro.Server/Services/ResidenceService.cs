using QuanLyTro.Server.Repositories;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Services;

/// <summary>Lịch sử lưu trú cho Công an (PM-04, PM-05): tra cứu theo khoảng ngày và xuất CSV.</summary>
public sealed class ResidenceService
{
    private readonly ResidenceRepository _repo;

    public ResidenceService(ResidenceRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<ResidenceHistoryDto>> GetHistoryAsync(DateTime from, DateTime to, string? room, CancellationToken ct)
    {
        if (to < from) throw new BusinessRuleException("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");
        return await _repo.GetHistoryAsync(from, to, room, ct);
    }

    public async Task<ExportResult> ExportHistoryAsync(ExportHistoryRequest req, CancellationToken ct)
    {
        if (req.ToDate < req.FromDate) throw new BusinessRuleException("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");
        var data = await _repo.GetHistoryAsync(req.FromDate, req.ToDate, req.RoomNumber, ct);

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
}
