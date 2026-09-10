using QuanLyTro.Server.Repositories;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Services;

/// <summary>
/// Dịch vụ báo cáo: US-04/US-18 (tổng quan phòng, khách, tiền thu/nợ), US-08 (dữ liệu tạm trú).
/// Lưu ý: Client tự lắp ráp chuỗi CSV UTF-8 BOM bằng StreamWriter (Task 11).
/// </summary>
public sealed class ReportService(IReportRepository reports)
{
    public Task<SummaryReportDto> GetSummaryAsync(string billingMonth, CancellationToken ct = default) =>
        reports.GetSummaryAsync(InvoiceService.NormalizeMonth(billingMonth), ct);

    public Task<List<ResidenceExportDto>> ExportResidenceAsync(CancellationToken ct = default) =>
        reports.GetResidentsAsync(ct);
}
