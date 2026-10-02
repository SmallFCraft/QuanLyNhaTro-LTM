using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Server.Data;
using QuanLyTro.Server.Network;
using QuanLyTro.Server.Repositories;
using QuanLyTro.Server.Security;
using QuanLyTro.Server.Services;
using QuanLyTro.Shared.Models;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Tests;

/// <summary>
/// Round-trip thật qua TCP: server thật (cổng ngẫu nhiên) + <see cref="TcpClient"/> thật, gói tin JSON thô.
/// Session/Auth dùng stub repo (không cần MySQL); chỉ BaoCaoService trả LoiNghiepVu để kiểm tra
/// thông báo tiếng Việt đi nguyên vẹn qua mạng.
/// </summary>
[TestClass]
public sealed class TcpRoundTripTests
{
    private const string LandlordUser = "admin";
    private const string LandlordPassword = "admin-pass";
    private const string ManagerUser = "manager";
    private const string ManagerPassword = "manager-pass";
    private const string TenantCccd = "048203012345";
    private const string TenantPassword = "123456";
    private const string StubRuleMessage = "Phòng này chưa chốt điện nước tháng này.";

    private TcpListenerServer _server = null!;
    private CancellationTokenSource _cts = null!;

    [TestInitialize]
    public void StartServer()
    {
        _cts = new CancellationTokenSource();
        _server = new TcpListenerServer(MakeRouter());
        // StartAsync bind cổng đồng bộ trước await đầu tiên → Port đọc được ngay.
        _ = _server.StartAsync(0, _cts.Token);
    }

    [TestCleanup]
    public void StopServer()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    [TestMethod]
    public async Task TcpRoundTrip_LoginReturnsTokenAndRole()
    {
        var response = await SendAsync(RequestPacket.Create(
            ActionNames.DangNhap, null, new YeuCauDangNhap(LandlordUser, LandlordPassword)));

        Assert.IsTrue(response.Success, response.Message);
        var result = response.GetData<KetQuaDangNhap>();
        Assert.IsNotNull(result);
        Assert.AreEqual(VaiTroNguoiDung.ChuTro, result.VaiTro);
        Assert.AreEqual("Nguyễn Văn A", result.HoTen);
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.Token));
    }

    [TestMethod]
    public async Task TcpRoundTrip_BusinessRuleMessageSurvivesVerbatim()
    {
        var login = await LoginAsync(LandlordUser, LandlordPassword);

        var response = await SendAsync(
            RequestPacket.Create(ActionNames.BaoCaoTongQuan, login.Token, new { KyCuoc = "2026-09" }));

        Assert.IsFalse(response.Success);
        Assert.AreEqual(StubRuleMessage, response.Message);
    }

    [TestMethod]
    public async Task TcpRoundTrip_DbDown_ExplainsHowToFix()
    {
        var login = await LoginAsync(LandlordUser, LandlordPassword);

        // PhongRepository thật + chuỗi kết nối tới cổng đóng → MySqlException 1042.
        var response = await SendAsync(
            RequestPacket.Create(ActionNames.PhongLayTatCa, login.Token, new { }));

        Assert.IsFalse(response.Success);
        StringAssert.Contains(response.Message, "MySQL", "Client phải được nói rõ lỗi do cơ sở dữ liệu.");
        StringAssert.Contains(response.Message, "Laragon");
        Assert.IsFalse(response.Message.Contains("MySqlConnector"), "Không lộ type/stack trace ra Client.");
    }

    [TestMethod]
    public async Task TcpRoundTrip_UnauthenticatedProtectedActionIsRejected()
    {
        var response = await SendAsync(RequestPacket.Create(ActionNames.PhongLayTatCa, null, new { }));

        Assert.IsFalse(response.Success);
        Assert.IsFalse(string.IsNullOrWhiteSpace(response.Message));
    }

    [TestMethod]
    public async Task TcpRoundTrip_TenantCannotCallLandlordAction()
    {
        var login = await LoginAsync(TenantCccd, TenantPassword);
        Assert.AreEqual(VaiTroNguoiDung.KhachThue, login.VaiTro);

        var response = await SendAsync(RequestPacket.Create(ActionNames.PhongLayTatCa, login.Token, new { }));

        Assert.IsFalse(response.Success);
        StringAssert.Contains(response.Message, "quyền");
    }

    [TestMethod]
    public async Task TcpRoundTrip_ResponseHasNoBomAndEndsWithNewline()
    {
        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, _server.Port);
        await using var stream = client.GetStream();

        var payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(
            RequestPacket.Create(ActionNames.DangNhap, null,
                new YeuCauDangNhap(LandlordUser, LandlordPassword)), JsonDefaults.Options) + "\n");
        await stream.WriteAsync(payload);
        await stream.FlushAsync();

        var buffer = new byte[8192];
        var read = await stream.ReadAsync(buffer).AsTask().WaitAsync(TimeSpan.FromSeconds(10));

        // BOM đầu gói làm Client JSON parse hỏng — bắt ở test thay vì mở WinForms chạy thử.
        Assert.IsFalse(buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF, "Response bắt đầu bằng BOM.");
        Assert.AreEqual((byte)'\n', buffer[read - 1], "Response không kết thúc bằng \\n.");
        Assert.IsTrue(read > 3);
    }

    [TestMethod]
    public async Task TcpRoundTrip_LandlordCanGetAndSetPermissions_ManagerCannot()
    {
        try
        {
            // 1. Chủ trọ đăng nhập -> lấy ma trận quyền thành công
            var landlordLogin = await LoginAsync(LandlordUser, LandlordPassword);
            Assert.AreEqual(VaiTroNguoiDung.ChuTro, landlordLogin.VaiTro);

            var getResponse = await SendAsync(RequestPacket.Create(
                ActionNames.PhanQuyenLayMaTran, landlordLogin.Token, new { }));
            Assert.IsTrue(getResponse.Success, getResponse.Message);

            var matrix = getResponse.GetData<MaTranQuyenVaiTroDto>();
            Assert.IsNotNull(matrix);
            Assert.IsTrue(matrix.QuyenTheoVaiTro.ContainsKey("QuanLy"));
            Assert.IsTrue(matrix.DanhSachHanhDong.Count > 0);

            // 2. Chủ trọ cập nhật quyền của QuanLy
            var updateResponse = await SendAsync(RequestPacket.Create(
                ActionNames.PhanQuyenCapNhatVaiTro, landlordLogin.Token,
                new YeuCauCapNhatQuyenVaiTro("QuanLy", [ActionNames.PhongLayTatCa, ActionNames.HoaDonLayTatCa])));
            Assert.IsTrue(updateResponse.Success, updateResponse.Message);

            // 3. Quản lý đăng nhập
            var managerLogin = await LoginAsync(ManagerUser, ManagerPassword);
            Assert.AreEqual(VaiTroNguoiDung.QuanLy, managerLogin.VaiTro);

            // Quản lý gọi hanh_dong quản lý phân quyền -> Bị từ chối vì không có quyền
            var managerGetAttempt = await SendAsync(RequestPacket.Create(
                ActionNames.PhanQuyenLayMaTran, managerLogin.Token, new { }));
            Assert.IsFalse(managerGetAttempt.Success);
            StringAssert.Contains(managerGetAttempt.Message, "quyền");

            var managerUpdateAttempt = await SendAsync(RequestPacket.Create(
                ActionNames.PhanQuyenCapNhatVaiTro, managerLogin.Token,
                new YeuCauCapNhatQuyenVaiTro("QuanLy", [ActionNames.PhongLayTatCa])));
            Assert.IsFalse(managerUpdateAttempt.Success);
            StringAssert.Contains(managerUpdateAttempt.Message, "quyền");
        }
        finally
        {
            // Khôi phục quyền mặc định sau test — luôn chạy dù assert phía trên fail
            MaTranPhanQuyen.ResetToDefaults();
        }
    }

    [TestMethod]
    public void RequestRouter_RegistersAllKnownProtocolActions()
    {
        var router = MakeRouter();
        foreach (var hanh_dong in ActionNames.All)
        {
            Assert.IsTrue(router.HandledActions.Contains(hanh_dong),
                $"Router thiếu handler cho hanh_dong: {hanh_dong}");
        }
    }

    private async Task<KetQuaDangNhap> LoginAsync(string tenDangNhap, string password)
    {
        var response = await SendAsync(RequestPacket.Create(
            ActionNames.DangNhap, null, new YeuCauDangNhap(tenDangNhap, password)));

        Assert.IsTrue(response.Success, response.Message);
        return response.GetData<KetQuaDangNhap>()!;
    }

    private async Task<ResponsePacket> SendAsync(RequestPacket packet)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, _server.Port);
        await using var stream = client.GetStream();

        var payload = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(packet, JsonDefaults.Options) + "\n");
        await stream.WriteAsync(payload);
        await stream.FlushAsync();

        using var reader = new StreamReader(stream, Encoding.UTF8);
        var line = await reader.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(10));
        Assert.IsNotNull(line, "Server đóng kết nối mà không trả phản hồi.");

        return JsonSerializer.Deserialize<ResponsePacket>(line, JsonDefaults.Options)
            ?? throw new AssertFailedException($"Phản hồi không đọc được: {line}");
    }

    private static DieuPhoiYeuCau MakeRouter()
    {
        // Cổng 1 luôn đóng trên Windows → MySqlErrorCode.UnableToConnectToHost, không phụ thuộc
        // việc Laragon có đang chạy lúc test hay không.
        var database = new Database("Server=127.0.0.1;Port=1;Database=unused_for_router_tests;User Id=root;Password=;");
        var sessions = new SessionStore();
        var auth = new XacThucService(new StubUserRepository(), new StubTenantRepository(), sessions);

        return new DieuPhoiYeuCau(
            auth,
            new PhongService(new PhongRepository(database)),
            new KhachThueService(new KhachThueRepository(database)),
            new HopDongService(new HopDongRepository(database)),
            new DienNuocService(new DienNuocRepository(database)),
            new HoaDonService(new HoaDonRepository(database)),
            new BaoCaoService(new StubReportRepository()),
            new CuTruService(new CuTruRepository(database)),
            sessions,
            new StubPermissionRepository());
    }

    private sealed class StubUserRepository : ITaiKhoanRepository
    {
        private readonly TaiKhoanRecord _user =
            new(1, PasswordHasher.Hash(LandlordPassword), "Nguyễn Văn A");

        // QuanLy dùng chung mật khẩu demo để test phân quyền mà không cần MySQL.
        private readonly TaiKhoanRecord _manager =
            new(2, PasswordHasher.Hash(ManagerPassword), "Quản Lý Thử Nghiệm", VaiTroNguoiDung.QuanLy);

        public Task<TaiKhoanRecord?> FindByUsernameAsync(string tenDangNhap, CancellationToken ct = default) =>
            Task.FromResult(tenDangNhap switch
            {
                LandlordUser => _user,
                ManagerUser => _manager,
                _ => null,
            });
    }

    /// <summary>Kho phân quyền in-memory cho test — không cần MySQL.</summary>
    private sealed class StubPermissionRepository : IPhanQuyenRepository
    {
        private readonly Dictionary<string, List<string>> _matrix =
            QuyenMacDinhTheoVaiTro.All.ToDictionary(
                kv => kv.Key,
                kv => kv.Value.ToList(),
                StringComparer.OrdinalIgnoreCase);

        public Task<Dictionary<string, List<string>>> GetAllAsync(CancellationToken ct = default) =>
            Task.FromResult(_matrix.ToDictionary(kv => kv.Key, kv => kv.Value.ToList()));

        public Task UpdateRoleActionsAsync(string vai_tro, IEnumerable<string> hanh_dong, CancellationToken ct = default)
        {
            _matrix[vai_tro] = hanh_dong.ToList();
            return Task.CompletedTask;
        }
    }

    private sealed class StubTenantRepository : IKhachThueRepository
    {
        private readonly KhachThueAuthRecord _tenant =
            new(10, TenantCccd, PasswordHasher.Hash(TenantPassword), "Trần Văn B", 101);

        public Task<KhachThueAuthRecord?> FindByCccdAsync(string cccd, CancellationToken ct = default) =>
            Task.FromResult(cccd == TenantCccd ? _tenant : null);
    }

    /// <summary>Ném LoiNghiepVu để kiểm tra thông báo lỗi nghiệp vụ đi qua TCP nguyên văn.</summary>
    private sealed class StubReportRepository : IBaoCaoRepository
    {
        public Task<BaoCaoTongQuanDto> GetSummaryAsync(string kyCuoc, CancellationToken ct = default) =>
            throw new LoiNghiepVu(StubRuleMessage);

        public Task<List<XuatHoSoTamTruDto>> GetResidentsAsync(CancellationToken ct = default) =>
            throw new LoiNghiepVu(StubRuleMessage);
    }
}
