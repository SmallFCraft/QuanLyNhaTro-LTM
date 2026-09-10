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
/// Session/Auth dùng stub repo (không cần MySQL); chỉ ReportService trả BusinessRuleException để kiểm tra
/// thông báo tiếng Việt đi nguyên vẹn qua mạng.
/// </summary>
[TestClass]
public sealed class TcpRoundTripTests
{
    private const string LandlordUser = "admin";
    private const string LandlordPassword = "admin-pass";
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
            ActionNames.AuthLogin, null, new LoginRequest(LandlordUser, LandlordPassword)));

        Assert.IsTrue(response.Success, response.Message);
        var result = response.GetData<LoginResult>();
        Assert.IsNotNull(result);
        Assert.AreEqual(UserRole.Landlord, result.Role);
        Assert.AreEqual("Nguyễn Văn A", result.FullName);
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.Token));
    }

    [TestMethod]
    public async Task TcpRoundTrip_BusinessRuleMessageSurvivesVerbatim()
    {
        var login = await LoginAsync(LandlordUser, LandlordPassword);

        var response = await SendAsync(
            RequestPacket.Create(ActionNames.ReportSummary, login.Token, new { BillingMonth = "2026-09" }));

        Assert.IsFalse(response.Success);
        Assert.AreEqual(StubRuleMessage, response.Message);
    }

    [TestMethod]
    public async Task TcpRoundTrip_UnauthenticatedProtectedActionIsRejected()
    {
        var response = await SendAsync(RequestPacket.Create(ActionNames.RoomGetAll, null, new { }));

        Assert.IsFalse(response.Success);
        Assert.IsFalse(string.IsNullOrWhiteSpace(response.Message));
    }

    [TestMethod]
    public async Task TcpRoundTrip_TenantCannotCallLandlordAction()
    {
        var login = await LoginAsync(TenantCccd, TenantPassword);
        Assert.AreEqual(UserRole.Tenant, login.Role);

        var response = await SendAsync(RequestPacket.Create(ActionNames.RoomGetAll, login.Token, new { }));

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
            RequestPacket.Create(ActionNames.AuthLogin, null,
                new LoginRequest(LandlordUser, LandlordPassword)), JsonDefaults.Options) + "\n");
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
    public void RequestRouter_RegistersAllKnownProtocolActions()
    {
        var router = MakeRouter();
        foreach (var action in ActionNames.All)
        {
            Assert.IsTrue(router.HandledActions.Contains(action),
                $"Router thiếu handler cho action: {action}");
        }
    }

    private async Task<LoginResult> LoginAsync(string username, string password)
    {
        var response = await SendAsync(RequestPacket.Create(
            ActionNames.AuthLogin, null, new LoginRequest(username, password)));

        Assert.IsTrue(response.Success, response.Message);
        return response.GetData<LoginResult>()!;
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

    private static RequestRouter MakeRouter()
    {
        var database = new Database("Server=127.0.0.1;Database=unused_for_router_tests;User Id=root;Password=;");
        var sessions = new SessionStore();
        var auth = new AuthService(new StubUserRepository(), new StubTenantRepository(), sessions);

        return new RequestRouter(
            auth,
            new RoomService(new RoomRepository(database)),
            new TenantService(new TenantRepository(database)),
            new ContractService(new ContractRepository(database)),
            new UtilityService(new UtilityRepository(database)),
            new InvoiceService(new InvoiceRepository(database)),
            new ReportService(new StubReportRepository()),
            sessions);
    }

    private sealed class StubUserRepository : IUserRepository
    {
        private readonly UserRecord _user =
            new(1, PasswordHasher.Hash(LandlordPassword), "Nguyễn Văn A");

        public Task<UserRecord?> FindByUsernameAsync(string username, CancellationToken ct = default) =>
            Task.FromResult(username == LandlordUser ? _user : null);
    }

    private sealed class StubTenantRepository : ITenantRepository
    {
        private readonly TenantAuthRecord _tenant =
            new(10, TenantCccd, PasswordHasher.Hash(TenantPassword), "Trần Văn B", 101);

        public Task<TenantAuthRecord?> FindByCccdAsync(string idCard, CancellationToken ct = default) =>
            Task.FromResult(idCard == TenantCccd ? _tenant : null);
    }

    /// <summary>Ném BusinessRuleException để kiểm tra thông báo lỗi nghiệp vụ đi qua TCP nguyên văn.</summary>
    private sealed class StubReportRepository : IReportRepository
    {
        public Task<SummaryReportDto> GetSummaryAsync(string billingMonth, CancellationToken ct = default) =>
            throw new BusinessRuleException(StubRuleMessage);

        public Task<List<ResidenceExportDto>> GetResidentsAsync(CancellationToken ct = default) =>
            throw new BusinessRuleException(StubRuleMessage);
    }
}
