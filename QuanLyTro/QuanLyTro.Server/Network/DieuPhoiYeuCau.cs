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
/// Bộ định tuyến hanh_dong TCP. Thứ tự enforce mỗi request (BR-14, TRAP §2.4 #6):
/// 1. `DANG_NHAP` không cần phiên; mọi hanh_dong khác phải có token hợp lệ trong <see cref="SessionStore"/>.
/// 2. `MaTranPhanQuyen.IsAllowed(hanh_dong, vai_tro)` — Client chỉ ẩn/hiện menu cho UX, Server mới quyết định.
/// 3. Gọi service; `LoiNghiepVu` → giữ nguyên message tiếng Việt cho người dùng.
/// </summary>
public sealed class DieuPhoiYeuCau
{
    private delegate Task<ResponsePacket> Handler(RequestPacket request, Session session, CancellationToken ct);

    private readonly IAuthService _auth;
    private readonly PhongService _rooms;
    private readonly KhachThueService _tenants;
    private readonly HopDongService _contracts;
    private readonly DienNuocService _utilities;
    private readonly HoaDonService _invoices;
    private readonly BaoCaoService _reports;
    private readonly CuTruService _residence;
    private readonly SessionStore _sessions;
    private readonly IPhanQuyenRepository? _permissions;
    private readonly Dictionary<string, Handler> _handlers;

    public DieuPhoiYeuCau(
        IAuthService auth,
        PhongService phong,
        KhachThueService khach_thue,
        HopDongService hop_dong,
        DienNuocService utilities,
        HoaDonService hoa_don,
        BaoCaoService reports,
        CuTruService residence,
        SessionStore sessions,
        IPhanQuyenRepository? permissions = null)
    {
        _auth = auth;
        _rooms = phong;
        _tenants = khach_thue;
        _contracts = hop_dong;
        _utilities = utilities;
        _invoices = hoa_don;
        _reports = reports;
        _residence = residence;
        _sessions = sessions;
        _permissions = permissions;

        _handlers = new Dictionary<string, Handler>(StringComparer.Ordinal)
        {
            [ActionNames.DangNhap] = (r, _, ct) => Ok(_auth.LoginAsync(r.GetData<YeuCauDangNhap>(), ct)),

            [ActionNames.PhongLayTatCa] = (_, _, ct) => Ok(_rooms.GetAllAsync(ct)),
            [ActionNames.PhongThem] = (r, _, ct) => Ok(_rooms.AddAsync(r.GetData<PhongDto>(), ct)),
            [ActionNames.PhongCapNhat] = (r, _, ct) => Ok(_rooms.UpdateAsync(r.GetData<PhongDto>(), ct)),
            [ActionNames.PhongXoa] = (r, _, ct) => Ok(_rooms.DeleteAsync(ReadId(r.Data, "phongId", "id"), ct)),

            [ActionNames.KhachThueTheoPhong] = (r, _, ct) => Ok(_tenants.GetByRoomAsync(ReadId(r.Data, "phongId", "id"), ct)),
            [ActionNames.KhachThueThem] = (r, _, ct) => AddTenantAsync(r, ct),
            [ActionNames.KhachThueCapNhat] = (r, _, ct) => UpdateTenantAsync(r, ct),
            [ActionNames.KhachThueTraPhong] = (r, _, ct) => Ok(_tenants.CheckoutAsync(ReadId(r.Data, "khachThueId", "id"), ct)),
            [ActionNames.KhachThueXoa] = (r, _, ct) => Ok(_tenants.DeleteAsync(ReadId(r.Data, "khachThueId", "id"), ct)),

            [ActionNames.HopDongTao] = (r, _, ct) => Ok(_contracts.CreateAsync(r.GetData<HopDongDto>(), ct)),
            [ActionNames.HopDongChamDut] = (r, _, ct) => Ok(_contracts.TerminateAsync(
                ReadId(r.Data, "hopDongId", "id"),
                ReadString(r.Data, "ghi_chu"),
                ct)),
            [ActionNames.HopDongGiaHan] = (r, _, ct) => Ok(_contracts.RenewAsync(
                ReadId(r.Data, "hopDongId", "id"),
                ReadDate(r.Data, "newEndDate"),
                ct)),
            [ActionNames.HopDongLayTatCa] = (_, _, ct) => Ok(_contracts.GetAllAsync(ct)),

            // kyCuoc tuỳ chọn: có thì lấy chỉ số kỳ TRƯỚC tháng đó (US-13), không thì giữ
            // hành vi cũ (bản ghi mới nhất) để client cũ không vỡ.
            [ActionNames.DienNuocLayKyTruoc] = (r, _, ct) =>
            {
                var phongId = ReadId(r.Data, "phongId", "id");
                var kyCuoc = ReadString(r.Data, "kyCuoc");

                return Ok(string.IsNullOrWhiteSpace(kyCuoc)
                    ? _utilities.GetPreviousReadingAsync(phongId, ct)
                    : _utilities.GetPreviousReadingAsync(phongId, kyCuoc, ct));
            },
            [ActionNames.DienNuocGhiSo] = (r, _, ct) => Ok(_utilities.RecordAsync(r.GetData<ChiSoDienNuocDto>(), ct)),

            [ActionNames.HoaDonTao] = (r, _, ct) => Ok(_invoices.CreateAsync(r.GetData<YeuCauTaoHoaDon>(), ct)),
            [ActionNames.HoaDonLayTatCa] = (r, _, ct) => Ok(_invoices.GetAllAsync(
                ReadString(r.Data, "kyCuoc") ?? string.Empty,
                ReadOptionalId(r.Data, "phongId"),
                ct)),
            [ActionNames.HoaDonThanhToan] = (r, _, ct) => Ok(_invoices.PayAsync(ReadId(r.Data, "hoaDonId", "id"), ct)),
            // BR-14: khachThueId lấy TỪ PHIÊN, không bao giờ từ dữ liệu client gửi. Hỗ trợ phân trang page/soLuongMoiTrang.
            [ActionNames.HoaDonCuaToi] = (r, s, ct) =>
            {
                var page = ReadOptionalId(r.Data, "page") ?? 1;
                var soLuongMoiTrang = ReadOptionalId(r.Data, "soLuongMoiTrang") ?? 10;
                return Ok(_invoices.GetMineAsync(s.UserId, page, soLuongMoiTrang, ct));
            },

            [ActionNames.BaoCaoTongQuan] = (r, _, ct) =>
                Ok(_reports.GetSummaryAsync(ReadString(r.Data, "kyCuoc") ?? string.Empty, ct)),
            [ActionNames.XuatHoSoTamTru] = (_, _, ct) => Ok(_reports.ExportResidenceAsync(ct)),

            [ActionNames.LichSuCuTruLay] = (r, _, ct) =>
            {
                var from = ReadOptionalDateTime(r.Data, "from", "fromDate");
                var to = ReadOptionalDateTime(r.Data, "to", "toDate");
                var soPhong = ReadString(r.Data, "soPhong") ?? ReadString(r.Data, "room");
                return Ok(_residence.GetHistoryAsync(from, to, soPhong, ct));
            },
            [ActionNames.XuatLichSuCuTru] = (r, _, ct) =>
                Ok(_residence.ExportHistoryAsync(r.GetData<YeuCauXuatLichSu>(), ct)),

            [ActionNames.PhanQuyenLayMaTran] = (_, s, ct) => GetPermissionMatrixAsync(s, ct),
            [ActionNames.PhanQuyenCapNhatVaiTro] = (r, s, ct) => UpdateRolePermissionsAsync(r, s, ct),
        };
    }

    /// <summary>Toàn bộ hanh_dong có route — dùng cho test đối chiếu với <see cref="ActionNames.All"/>.</summary>
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
            if (request.Action != ActionNames.DangNhap)
            {
                if (!_sessions.TryGet(request.Token ?? string.Empty, out var found))
                {
                    return ResponsePacket.Fail("Phiên đăng nhập không hợp lệ hoặc đã hết hạn.");
                }

                if (!MaTranPhanQuyen.IsAllowed(request.Action, found.VaiTro))
                {
                    return ResponsePacket.Fail("Không có quyền.");
                }

                session = new Session(found.UserId, found.VaiTro);
            }

            if (!_handlers.TryGetValue(request.Action, out var handler))
            {
                return ResponsePacket.Fail("Hành động không được hỗ trợ.");
            }

            return await handler(request, session, ct);
        }
        catch (LoiNghiepVu ex)
        {
            // Message tiếng Việt hiển thị thẳng cho người dùng (§2.2).
            return ResponsePacket.Fail(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            // XacThucService: sai thông tin đăng nhập hoặc tài khoản đang tạm khóa.
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

    public readonly record struct Session(int UserId, VaiTroNguoiDung VaiTro);

    /// <summary>
    /// Chấp nhận cả payload phẳng (`KhachThueDto` + `plainPassword` cùng cấp) và payload bọc
    /// (`{ "tenant": {...}, "plainPassword": "..." }`) — Client chỉ gửi 1 trong 2.
    /// </summary>
    private static (KhachThueDto KhachThue, string? PlainPassword) ReadTenant(JsonElement data)
    {
        var node = TryGetProperty(data, "tenant", out var nested) ? nested : data;
        var tenant = node.Deserialize<KhachThueDto>(JsonDefaults.Options)
            ?? throw new LoiNghiepVu("Dữ liệu người thuê không hợp lệ.");

        return (tenant, ReadString(data, "plainPassword"));
    }

    /// <summary>
    /// Ma trận quyền động hiện tại + danh mục hanh_dong có thể gán (màn Phân quyền của Chủ trọ).
    /// Chỉ Chủ trọ gọi được — kiểm tra kép qua MaTranPhanQuyen và session.VaiTro.
    /// </summary>
    private async Task<ResponsePacket> GetPermissionMatrixAsync(Session session, CancellationToken ct)
    {
        if (session.VaiTro != VaiTroNguoiDung.ChuTro)
        {
            throw new UnauthorizedAccessException("Chỉ Chủ trọ mới có quyền xem phân quyền.");
        }

        if (_permissions is null)
        {
            throw new LoiNghiepVu("Server chưa cấu hình kho phân quyền.");
        }

        var quyenTheoVaiTro = await _permissions.GetAllAsync(ct);
        return ResponsePacket.Ok(new MaTranQuyenVaiTroDto(quyenTheoVaiTro, [.. QuyenMacDinhTheoVaiTro.DanhMuc]));
    }

    /// <summary>
    /// Chủ trọ cập nhật quyền cho một vai trò. Ghi CSDL rồi nạp lại cache ngay — mọi phiên
    /// đang mở của vai trò đó chịu hiệu lực tức thì, không cần khởi động lại Server.
    /// </summary>
    private async Task<ResponsePacket> UpdateRolePermissionsAsync(RequestPacket request, Session session, CancellationToken ct)
    {
        if (session.VaiTro != VaiTroNguoiDung.ChuTro)
        {
            throw new UnauthorizedAccessException("Chỉ Chủ trọ mới có quyền điều chỉnh phân quyền.");
        }

        if (_permissions is null)
        {
            throw new LoiNghiepVu("Server chưa cấu hình kho phân quyền.");
        }

        var payload = request.GetData<YeuCauCapNhatQuyenVaiTro>();
        if (string.IsNullOrWhiteSpace(payload.VaiTro))
        {
            throw new LoiNghiepVu("Thiếu vai trò cần cập nhật phân quyền.");
        }

        if (!QuyenMacDinhTheoVaiTro.All.Keys.Contains(payload.VaiTro, StringComparer.OrdinalIgnoreCase))
        {
            throw new LoiNghiepVu($"Không thể phân quyền cho vai trò '{payload.VaiTro}'.");
        }

        // Thiếu mảng DanhSachHanhDong ≠ mảng rỗng: null từ chối, rỗng mới xóa sạch.
        if (payload.DanhSachHanhDong is null)
        {
            throw new LoiNghiepVu("Thiếu danh sách quyền cần cập nhật.");
        }

        // Chỉ nhận hanh_dong có thật trong danh mục — chặn Client gửi tên bịa để tự mở quyền.
        var known = QuyenMacDinhTheoVaiTro.DanhMuc.Select(c => c.HanhDong).ToHashSet(StringComparer.Ordinal);
        var requested = payload.DanhSachHanhDong;
        var rejected = requested.Where(a => !known.Contains(a)).ToList();
        if (rejected.Count > 0)
        {
            throw new LoiNghiepVu($"Hành động không hợp lệ: {string.Join(", ", rejected)}");
        }

        await _permissions.UpdateRoleActionsAsync(payload.VaiTro, requested, ct);

        // Nạp lại cache ngay lập tức.
        var matrix = await _permissions.GetAllAsync(ct);
        MaTranPhanQuyen.ApplyMatrix(matrix);

        return ResponsePacket.Ok(new { message = $"Đã cập nhật phân quyền cho vai trò {payload.VaiTro}." });
    }

    private static int ReadId(JsonElement data, params string[] names) =>
        ReadOptionalId(data, names) ?? throw new LoiNghiepVu("Thiếu mã định danh trong yêu cầu.");

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
            : throw new LoiNghiepVu("Ngày không hợp lệ.");
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
                : throw new LoiNghiepVu("Ngày không hợp lệ.");
        }

        throw new LoiNghiepVu("Thiếu ngày trong yêu cầu.");
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
                : throw new LoiNghiepVu("Ngày không hợp lệ.");
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
