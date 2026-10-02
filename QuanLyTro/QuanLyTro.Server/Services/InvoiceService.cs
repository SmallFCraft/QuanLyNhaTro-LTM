using System.Globalization;
using QuanLyTro.Server.Repositories;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Services;

/// <summary>
/// Nghiệp vụ hóa đơn: BR-09 (cần HĐ Active + đã chốt điện nước), BR-10 (server tự tính tiền),
/// BR-11 (hóa đơn Paid bất biến), BR-14 (người thuê chỉ thấy hóa đơn phòng mình).
/// </summary>
public sealed class InvoiceService(IInvoiceRepository invoices)
{
    /// <summary>BR-09, BR-10, BR-13. Client chỉ gửi RoomId, tháng và OtherFees.</summary>
    public async Task<InvoiceDto> CreateAsync(CreateInvoiceRequest request, CancellationToken ct = default)
    {
        var billingMonth = NormalizeMonth(request.BillingMonth);

        if (request.OtherFees < 0)
        {
            throw new BusinessRuleException("Phí khác không được âm.");
        }

        var contract = await invoices.GetActiveContractAsync(request.RoomId, ct)
            ?? throw new BusinessRuleException("Phòng này không có hợp đồng đang hiệu lực.");

        var reading = await invoices.GetUtilityReadingAsync(billingMonth, request.RoomId, ct)
            ?? throw new BusinessRuleException("Phòng này chưa chốt điện nước tháng này.");

        var roomAmount = contract.RentalPrice;
        var electricityAmount = (reading.NewElectricity - reading.OldElectricity) * reading.ElectricityRate;
        var waterAmount = (reading.NewWater - reading.OldWater) * reading.WaterRate;

        var invoice = new InvoiceDto(
            0,
            request.RoomId,
            contract.Id,
            billingMonth,
            roomAmount,
            electricityAmount,
            waterAmount,
            request.OtherFees,
            roomAmount + electricityAmount + waterAmount + request.OtherFees,
            InvoiceStatus.Unpaid,
            null);

        return await invoices.AddAsync(invoice, ct);
    }

    /// <summary>US-15: lọc theo tháng (bắt buộc) và phòng (tùy chọn).</summary>
    public Task<List<InvoiceDto>> GetAllAsync(string billingMonth, int? roomId = null, CancellationToken ct = default) =>
        invoices.GetAllAsync(NormalizeMonth(billingMonth), roomId, ct);

    /// <summary>BR-11: chỉ hóa đơn `Unpaid` mới thu được; set paid_at = NOW() trong repository.</summary>
    public async Task<bool> PayAsync(int invoiceId, CancellationToken ct = default)
    {
        var invoice = await invoices.GetByIdAsync(invoiceId, ct)
            ?? throw new BusinessRuleException("Không tìm thấy hóa đơn.");

        if (invoice.Status == InvoiceStatus.Paid)
        {
            throw new BusinessRuleException("Hóa đơn đã thanh toán.");
        }

        // Hai client thu cùng lúc: kẻ thua không chạm được dòng Unpaid nào.
        if (!await invoices.MarkPaidAsync(invoiceId, ct))
        {
            throw new BusinessRuleException("Hóa đơn đã thanh toán.");
        }

        return true;
    }

    /// <summary>BR-14: server suy phòng từ tenantId trong phiên, client không gửi roomId. Hỗ trợ phân trang server-side.</summary>
    public Task<InvoiceMinePageDto> GetMineAsync(
        int tenantId, int page = 1, int pageSize = 10, CancellationToken ct = default) =>
        invoices.GetByTenantAsync(tenantId, page, pageSize, ct);

    /// <summary>Tháng hóa đơn là chuỗi `yyyy-MM` — dùng chung với UtilityService.</summary>
    public static string NormalizeMonth(string? month)
    {
        var trimmed = (month ?? string.Empty).Trim();
        if (!DateOnly.TryParseExact(trimmed, "yyyy-MM", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out _))
        {
            throw new BusinessRuleException("Tháng hóa đơn phải theo định dạng yyyy-MM.");
        }

        return trimmed;
    }
}
