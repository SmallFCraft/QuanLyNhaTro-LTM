using System.Globalization;
using System.Text.Json;
using MySqlConnector;
using QuanLyTro.Server.Data;
using QuanLyTro.Server.Repositories;
using QuanLyTro.Server.Security;
using QuanLyTro.Server.Services;
using QuanLyTro.Shared.Models;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Server.Network;

/// <summary>
/// Bộ định tuyến action TCP. Thứ tự enforce mỗi request (BR-14, TRAP §2.4 #6):
/// 1. `AUTH_LOGIN` không cần phiên; mọi action khác phải có token hợp lệ trong <see cref="SessionStore"/>.
/// 2. `PermissionMatrix.IsAllowed(action, role)` — Client chỉ ẩn/hiện menu cho UX, Server mới quyết định.
/// 3. Gọi service; `BusinessRuleException` → giữ nguyên message tiếng Việt cho người dùng.
/// </summary>
public sealed class RequestRouter
{
    private delegate Task<ResponsePacket> Handler(RequestPacket request, Session session, CancellationToken ct);

    private readonly IAuthService _auth;
    private readonly RoomService _rooms;
    private readonly TenantService _tenants;
    private readonly ContractService _contracts;
    private readonly UtilityService _utilities;
    private readonly InvoiceService _invoices;
    private readonly ReportService _reports;
    private readonly ResidenceService _residence;
    private readonly SessionStore _sessions;
    private readonly IPermissionRepository? _permissions;
    private readonly Dictionary<string, Handler> _handlers;

    public RequestRouter(
        IAuthService auth,
        RoomService rooms,
        TenantService tenants,
        ContractService contracts,
        UtilityService utilities,
        InvoiceService invoices,
        ReportService reports,
        ResidenceService residence,
        SessionStore sessions,
        IPermissionRepository? permissions = null)
    {
        _auth = auth;
        _rooms = rooms;
        _tenants = tenants;
        _contracts = contracts;
        _utilities = utilities;
        _invoices = invoices;
        _reports = reports;
        _residence = residence;
        _sessions = sessions;
        _permissions = permissions;

        _handlers = new Dictionary<string, Handler>(StringComparer.Ordinal)
        {
            [ActionNames.AuthLogin] = (r, _, ct) => Ok(_auth.LoginAsync(r.GetData<LoginRequest>(), ct)),

            [ActionNames.RoomGetAll] = (_, _, ct) => Ok(_rooms.GetAllAsync(ct)),
            [ActionNames.RoomAdd] = (r, _, ct) => Ok(_rooms.AddAsync(r.GetData<RoomDto>(), ct)),
            [ActionNames.RoomUpdate] = (r, _, ct) => Ok(_rooms.UpdateAsync(r.GetData<RoomDto>(), ct)),
            [ActionNames.RoomDelete] = (r, _, ct) => Ok(_rooms.DeleteAsync(ReadId(r.Data, "roomId", "id"), ct)),

            [ActionNames.TenantGetByRoom] = (r, _, ct) => Ok(_tenants.GetByRoomAsync(ReadId(r.Data, "roomId", "id"), ct)),
            [ActionNames.TenantAdd] = (r, _, ct) => AddTenantAsync(r, ct),
            [ActionNames.TenantUpdate] = (r, _, ct) => UpdateTenantAsync(r, ct),
            [ActionNames.TenantCheckout] = (r, _, ct) => Ok(_tenants.CheckoutAsync(ReadId(r.Data, "tenantId", "id"), ct)),
            [ActionNames.TenantDelete] = (r, _, ct) => Ok(_tenants.DeleteAsync(ReadId(r.Data, "tenantId", "id"), ct)),

            [ActionNames.ContractCreate] = (r, _, ct) => Ok(_contracts.CreateAsync(r.GetData<ContractDto>(), ct)),
            [ActionNames.ContractTerminate] = (r, _, ct) => Ok(_contracts.TerminateAsync(
                ReadId(r.Data, "contractId", "id"),
                ReadString(r.Data, "notes"),
                ct)),
            [ActionNames.ContractRenew] = (r, _, ct) => Ok(_contracts.RenewAsync(
                ReadId(r.Data, "contractId", "id"),
                ReadDate(r.Data, "newEndDate"),
                ct)),
            [ActionNames.ContractGetAll] = (_, _, ct) => Ok(_contracts.GetAllAsync(ct)),

            // billingMonth tuỳ chọn: có thì lấy chỉ số kỳ TRƯỚC tháng đó (US-13), không thì giữ
            // hành vi cũ (bản ghi mới nhất) để client cũ không vỡ.
            [ActionNames.UtilityGetPrevious] = (r, _, ct) =>
            {
                var roomId = ReadId(r.Data, "roomId", "id");
                var billingMonth = ReadString(r.Data, "billingMonth");

                return Ok(string.IsNullOrWhiteSpace(billingMonth)
                    ? _utilities.GetPreviousReadingAsync(roomId, ct)
                    : _utilities.GetPreviousReadingAsync(roomId, billingMonth, ct));
            },
            [ActionNames.UtilityRecord] = (r, _, ct) => Ok(_utilities.RecordAsync(r.GetData<UtilityReadingDto>(), ct)),

            [ActionNames.InvoiceCreate] = (r, _, ct) => Ok(_invoices.CreateAsync(r.GetData<CreateInvoiceRequest>(), ct)),
            [ActionNames.InvoiceGetAll] = (r, _, ct) => Ok(_invoices.GetAllAsync(
                ReadString(r.Data, "billingMonth") ?? string.Empty,
                ReadOptionalId(r.Data, "roomId"),
                ct)),
            [ActionNames.InvoicePay] = (r, _, ct) => Ok(_invoices.PayAsync(ReadId(r.Data, "invoiceId", "id"), ct)),
            // BR-14: tenantId lấy TỪ PHIÊN, không bao giờ từ dữ liệu client gửi. Hỗ trợ phân trang page/pageSize.
            [ActionNames.InvoiceGetMine] = (r, s, ct) =>
            {
                var page = ReadOptionalId(r.Data, "page") ?? 1;
                var pageSize = ReadOptionalId(r.Data, "pageSize") ?? 10;
                return Ok(_invoices.GetMineAsync(s.UserId, page, pageSize, ct));
            },

            [ActionNames.ReportSummary] = (r, _, ct) =>
                Ok(_reports.GetSummaryAsync(ReadString(r.Data, "billingMonth") ?? string.Empty, ct)),
            [ActionNames.ExportResidence] = (_, _, ct) => Ok(_reports.ExportResidenceAsync(ct)),

            [ActionNames.ResidenceHistoryGet] = (r, _, ct) =>
            {
                var from = ReadOptionalDateTime(r.Data, "from", "fromDate");
                var to = ReadOptionalDateTime(r.Data, "to", "toDate");
                var roomNumber = ReadString(r.Data, "roomNumber") ?? ReadString(r.Data, "room");
                return Ok(_residence.GetHistoryAsync(from, to, roomNumber, ct));
            },
            [ActionNames.ExportResidenceHistory] = (r, _, ct) =>
                Ok(_residence.ExportHistoryAsync(r.GetData<ExportHistoryRequest>(), ct)),

            [ActionNames.PermissionGetMatrix] = (_, s, ct) => GetPermissionMatrixAsync(s, ct),
            [ActionNames.PermissionUpdateRole] = (r, s, ct) => UpdateRolePermissionsAsync(r, s, ct),
        };
    }

    /// <summary>Toàn bộ action có route — dùng cho test đối chiếu với <see cref="ActionNames.All"/>.</summary>
    public IReadOnlyCollection<string> HandledActions => _handlers.Keys;

    public async Task<ResponsePacket> HandleAsync(RequestPacket request, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Action))
            {
                return ResponsePacket.Fail("Thiếu tên hành động.");
            }

            var session = default(Session);
            if (request.Action != ActionNames.AuthLogin)
            {
                if (!_sessions.TryGet(request.Token ?? string.Empty, out var found))
                {
                    return ResponsePacket.Fail("Phiên đăng nhập không hợp lệ hoặc đã hết hạn.");
                }

                if (!PermissionMatrix.IsAllowed(request.Action, found.Role))
                {
                    return ResponsePacket.Fail("Không có quyền.");
                }

                session = new Session(found.UserId, found.Role);
            }

            if (!_handlers.TryGetValue(request.Action, out var handler))
            {
                return ResponsePacket.Fail("Hành động không được hỗ trợ.");
            }

            return await handler(request, session, ct);
        }
        catch (BusinessRuleException ex)
        {
            // Message tiếng Việt hiển thị thẳng cho người dùng (§2.2).
            return ResponsePacket.Fail(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            // AuthService: sai thông tin đăng nhập hoặc tài khoản đang tạm khóa.
            return ResponsePacket.Fail(ex.Message);
        }
        catch (MySqlException ex)
        {
            // Lỗi hạ tầng DB phải nói rõ nguyên nhân: "Lỗi hệ thống" chung khiến người dùng
            // bấm thử lại vô ích trong khi thứ cần bật là MySQL.
            Console.Error.WriteLine($"[{request.Action}] MySQL {ex.Number}: {ex.Message}");
            return ResponsePacket.Fail(Database.DescribeFailure(ex.Number, ex.Message));
        }
        catch (Exception ex)
        {
            // TRAP §2.4 #7: không lộ stack trace ra Client.
            Console.Error.WriteLine($"[{request.Action}] {ex}");
            return ResponsePacket.Fail("Lỗi hệ thống, vui lòng thử lại.");
        }
    }

    private async Task<ResponsePacket> AddTenantAsync(RequestPacket request, CancellationToken ct)
    {
        var (tenant, password) = ReadTenant(request.Data);
        return ResponsePacket.Ok(await _tenants.AddAsync(tenant, password, ct));
    }

    private async Task<ResponsePacket> UpdateTenantAsync(RequestPacket request, CancellationToken ct)
    {
        var (tenant, password) = ReadTenant(request.Data);
        return ResponsePacket.Ok(await _tenants.UpdateAsync(tenant, password, ct));
    }

    private static async Task<ResponsePacket> Ok<T>(Task<T> task) => ResponsePacket.Ok(await task);

    public readonly record struct Session(int UserId, UserRole Role);

    /// <summary>
    /// Chấp nhận cả payload phẳng (`TenantDto` + `plainPassword` cùng cấp) và payload bọc
    /// (`{ "tenant": {...}, "plainPassword": "..." }`) — Client chỉ gửi 1 trong 2.
    /// </summary>
    private static (TenantDto Tenant, string? PlainPassword) ReadTenant(JsonElement data)
    {
        var node = TryGetProperty(data, "tenant", out var nested) ? nested : data;
        var tenant = node.Deserialize<TenantDto>(JsonDefaults.Options)
            ?? throw new BusinessRuleException("Dữ liệu người thuê không hợp lệ.");

        return (tenant, ReadString(data, "plainPassword"));
    }

    /// <summary>
    /// Ma trận quyền động hiện tại + danh mục action có thể gán (màn Phân quyền của Chủ trọ).
    /// Chỉ Chủ trọ gọi được — kiểm tra kép qua PermissionMatrix và session.Role.
    /// </summary>
    private async Task<ResponsePacket> GetPermissionMatrixAsync(Session session, CancellationToken ct)
    {
        if (session.Role != UserRole.Landlord)
        {
            throw new UnauthorizedAccessException("Chỉ Chủ trọ mới có quyền xem phân quyền.");
        }

        if (_permissions is null)
        {
            throw new BusinessRuleException("Server chưa cấu hình kho phân quyền.");
        }

        var roleActions = await _permissions.GetAllAsync(ct);
        return ResponsePacket.Ok(new RolePermissionsMatrixDto(roleActions, [.. DefaultRolePermissions.Catalog]));
    }

    /// <summary>
    /// Chủ trọ cập nhật quyền cho một vai trò. Ghi CSDL rồi nạp lại cache ngay — mọi phiên
    /// đang mở của vai trò đó chịu hiệu lực tức thì, không cần khởi động lại Server.
    /// </summary>
    private async Task<ResponsePacket> UpdateRolePermissionsAsync(RequestPacket request, Session session, CancellationToken ct)
    {
        if (session.Role != UserRole.Landlord)
        {
            throw new UnauthorizedAccessException("Chỉ Chủ trọ mới có quyền điều chỉnh phân quyền.");
        }

        if (_permissions is null)
        {
            throw new BusinessRuleException("Server chưa cấu hình kho phân quyền.");
        }

        var payload = request.GetData<UpdateRolePermissionsRequest>();
        if (string.IsNullOrWhiteSpace(payload.Role))
        {
            throw new BusinessRuleException("Thiếu vai trò cần cập nhật phân quyền.");
        }

        if (!DefaultRolePermissions.All.Keys.Contains(payload.Role, StringComparer.OrdinalIgnoreCase))
        {
            throw new BusinessRuleException($"Không thể phân quyền cho vai trò '{payload.Role}'.");
        }

        // Thiếu mảng Actions ≠ mảng rỗng: null từ chối, rỗng mới xóa sạch.
        if (payload.Actions is null)
        {
            throw new BusinessRuleException("Thiếu danh sách quyền cần cập nhật.");
        }

        // Chỉ nhận action có thật trong danh mục — chặn Client gửi tên bịa để tự mở quyền.
        var known = DefaultRolePermissions.Catalog.Select(c => c.Action).ToHashSet(StringComparer.Ordinal);
        var requested = payload.Actions;
        var rejected = requested.Where(a => !known.Contains(a)).ToList();
        if (rejected.Count > 0)
        {
            throw new BusinessRuleException($"Hành động không hợp lệ: {string.Join(", ", rejected)}");
        }

        await _permissions.UpdateRoleActionsAsync(payload.Role, requested, ct);

        // Nạp lại cache ngay lập tức.
        var matrix = await _permissions.GetAllAsync(ct);
        PermissionMatrix.ApplyMatrix(matrix);

        return ResponsePacket.Ok(new { message = $"Đã cập nhật phân quyền cho vai trò {payload.Role}." });
    }

    private static int ReadId(JsonElement data, params string[] names) =>
        ReadOptionalId(data, names) ?? throw new BusinessRuleException("Thiếu mã định danh trong yêu cầu.");

    private static int? ReadOptionalId(JsonElement data, params string[] names)
    {
        foreach (var name in names)
        {
            if (TryGetProperty(data, name, out var value)
                && value.ValueKind == JsonValueKind.Number
                && value.TryGetInt32(out var id))
            {
                return id;
            }
        }

        return null;
    }

    private static string? ReadString(JsonElement data, string name) =>
        TryGetProperty(data, name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static DateOnly ReadDate(JsonElement data, string name)
    {
        var raw = ReadString(data, name);
        return DateOnly.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : throw new BusinessRuleException("Ngày không hợp lệ.");
    }

    /// <summary>Đọc ngày dạng chuỗi ISO (yyyy-MM-dd) theo tên đầu tiên có mặt.</summary>
    private static DateTime ReadDateTime(JsonElement data, params string[] names)
    {
        foreach (var name in names)
        {
            var raw = ReadString(data, name);
            if (raw is null)
            {
                continue;
            }

            return DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
                ? date
                : throw new BusinessRuleException("Ngày không hợp lệ.");
        }

        throw new BusinessRuleException("Thiếu ngày trong yêu cầu.");
    }

    /// <summary>Như <see cref="ReadDateTime"/> nhưng thiếu/để trống → null để service áp khoảng mặc định.</summary>
    private static DateTime? ReadOptionalDateTime(JsonElement data, params string[] names)
    {
        foreach (var name in names)
        {
            var raw = ReadString(data, name);
            if (string.IsNullOrWhiteSpace(raw))
            {
                continue;
            }

            return DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
                ? date
                : throw new BusinessRuleException("Ngày không hợp lệ.");
        }

        return null;
    }

    /// <summary>`JsonElement.TryGetProperty` phân biệt hoa thường; giao thức dùng camelCase nên quét tay.</summary>
    private static bool TryGetProperty(JsonElement data, string name, out JsonElement value)
    {
        if (data.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in data.EnumerateObject())
            {
                if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }
        }

        value = default;
        return false;
    }
}
