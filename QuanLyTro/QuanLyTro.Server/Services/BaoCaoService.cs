using QuanLyTro.Server.Repositories;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Services;

/// <summary>
/// Dịch vụ báo cáo: US-04/US-18 (tổng quan phòng, khách, tiền thu/nợ), US-08 (dữ liệu tạm trú).
/// Lưu ý: Client tự lắp ráp chuỗi CSV UTF-8 BOM bằng StreamWriter (Task 11).
/// </summary>
public sealed class BaoCaoService(IBaoCaoRepository reports)
{
    public Task<BaoCaoTongQuanDto> GetSummaryAsync(string kyCuoc, CancellationToken ct = default) =>
        reports.GetSummaryAsync(HoaDonService.NormalizeMonth(kyCuoc), ct);

    public Task<List<XuatHoSoTamTruDto>> ExportResidenceAsync(CancellationToken ct = default) =>
        reports.GetResidentsAsync(ct);
}
