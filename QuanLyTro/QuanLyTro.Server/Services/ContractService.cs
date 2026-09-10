using QuanLyTro.Server.Repositories;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Services;

/// <summary>
/// Nghiệp vụ hợp đồng: BR-04 (1 HĐ Active / phòng), BR-05 (đại diện phải ở trong phòng),
/// BR-06 (EndDate &gt; StartDate), US-10 (gia hạn), US-11 (danh sách sắp hết hạn).
/// </summary>
public sealed class ContractService(IContractRepository contracts)
{
    /// <summary>BR-04, BR-05, BR-06. Hợp đồng tạo mới luôn ở trạng thái Active.</summary>
    public async Task<ContractDto> CreateAsync(ContractDto contract, CancellationToken ct = default)
    {
        ValidateDates(contract.StartDate, contract.EndDate);

        if (contract.RentalPrice <= 0)
        {
            throw new BusinessRuleException("Giá thuê phải lớn hơn 0.");
        }

        if (contract.DepositAmount < 0)
        {
            throw new BusinessRuleException("Tiền cọc không được âm.");
        }

        if (await contracts.HasActiveContractAsync(contract.RoomId, ct))
        {
            throw new BusinessRuleException("Phòng này đang có hợp đồng hiệu lực.");
        }

        if (!await contracts.IsTenantInRoomAsync(contract.RepresentativeTenantId, contract.RoomId, ct))
        {
            throw new BusinessRuleException("Người đại diện phải là người đang ở trong phòng này.");
        }

        var toSave = contract with
        {
            Status = ContractStatus.Active,
            Notes = contract.Notes?.Trim(),
        };

        return await contracts.AddAsync(toSave, ct);
    }

    public async Task<bool> TerminateAsync(int contractId, string? notes, CancellationToken ct = default)
    {
        if (!await contracts.TerminateAsync(contractId, notes?.Trim(), ct))
        {
            throw new BusinessRuleException("Không tìm thấy hợp đồng đang hiệu lực để chấm dứt.");
        }

        return true;
    }

    /// <summary>US-10: chỉ gia hạn hợp đồng đang hiệu lực, ngày mới phải sau ngày kết thúc hiện tại.</summary>
    public async Task<bool> RenewAsync(int contractId, DateOnly newEndDate, CancellationToken ct = default)
    {
        var existing = await contracts.GetByIdAsync(contractId, ct)
            ?? throw new BusinessRuleException("Không tìm thấy hợp đồng.");

        if (existing.Status != ContractStatus.Active)
        {
            throw new BusinessRuleException("Chỉ gia hạn được hợp đồng đang hiệu lực.");
        }

        if (newEndDate <= existing.EndDate)
        {
            throw new BusinessRuleException("Ngày gia hạn phải sau ngày kết thúc hiện tại.");
        }

        return await contracts.UpdateEndDateAsync(contractId, newEndDate, ct);
    }

    /// <summary>US-11: kèm số phòng + tên đại diện, xếp theo end_date tăng dần.</summary>
    public Task<List<ContractListItem>> GetAllAsync(CancellationToken ct = default) =>
        contracts.GetAllAsync(ct);

    /// <summary>BR-06.</summary>
    public static void ValidateDates(DateOnly startDate, DateOnly endDate)
    {
        if (endDate <= startDate)
        {
            throw new BusinessRuleException("Ngày kết thúc phải sau ngày bắt đầu.");
        }
    }
}
