using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using QuanLyTro.Server.Data;
using QuanLyTro.Server.Network;
using QuanLyTro.Server.Repositories;
using QuanLyTro.Server.Security;
using QuanLyTro.Server.Services;
using QuanLyTro.Shared.Models;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Tests;

/// <summary>
/// Test acceptance SRS §7 trên MySQL thật (Laragon 127.0.0.1:3306) + router thật.
/// Dải dữ liệu riêng "T12" / id 9204..9215 / CCCD 999999999204.. — không đụng demo,
/// tự dọn theo thứ tự FK trong TestInitialize + TestCleanup để chạy lại được.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class AcceptanceTests
{
    private const string ConnectionString =
        "Server=127.0.0.1;Port=3306;Database=quanly_phongtro_nhs;User Id=root;Password=;SslMode=None;";

    // Task 12 reserve id 9204..9215, CCCD 9999999992xx, phòng T12-xx — không trùng wave khác.
    private const int MainRoomId = 9204;
    private const int SecondRoomId = 9205;
    private const int LandlordRoomId = 9206;
    private const int MainTenantId = 9204;
    private const int ExtraTenantId = 9205;
    private const int LandlordTenantId = 9206;
    private const string MainRoomNumber = "T12-ROOM-9204";
    private const string SecondRoomNumber = "T12-ROOM-9205";
    private const string LandlordRoomNumber = "T12-ROOM-9206";
    private const string FlowRoomNumber = "T12-FLOW-9210";
    private const string MainCccd = "999999999204";
    private const string ExtraCccd = "999999999205";
    private const string LandlordCccd = "999999999206";
    private const string FlowCccd = "999999999210";
    private const string BillingMonth = "2099-09";
    private const string OtherMonth = "2099-10";

    private static readonly Database Db = new(ConnectionString);

    [TestInitialize]
    public async Task SetUpAsync()
    {
        await CleanupAsync();
        await SeedRoomsAsync();
    }

    [TestCleanup]
    public async Task TearDownAsync() => await CleanupAsync();

    // --------------------------------------------------------------- SRS §7 luồng tháng

    /// <summary>Luồng tháng đầy đủ: phòng → người → hợp đồng → điện nước → hóa đơn → thu → báo cáo → xuất.</summary>
    [TestMethod]
    public async Task MonthlyFlow_CreateToPaymentAndReport()
    {
        var router = Router();
        var token = await LandlordTokenAsync(router);

        // 1. Tạo phòng
        var room = await router.HandleAsync(RequestPacket.Create(
            ActionNames.RoomAdd, token, new RoomDto(0, FlowRoomNumber, 3_500_000m, 4, RoomStatus.Available, "T12", 0)));
        Assert.IsTrue(room.Success, room.Message);
        var created = room.GetData<RoomDto>()!;
        Assert.IsTrue(created.Id > 0);
        Assert.AreEqual(1L, await ScalarAsync("SELECT COUNT(*) FROM rooms WHERE id = @id", ("@id", created.Id)),
            "Bước 1: phòng phải nằm trong DB.");

        // 2. Thêm người thuê (mật khẩu mặc định 6 số cuối CCCD)
        var tenant = await router.HandleAsync(RequestPacket.Create(ActionNames.TenantAdd, token,
            new { tenant = TenantDto(FlowCccd, MainTenantName, created.Id), plainPassword = (string?)null }));
        Assert.IsTrue(tenant.Success, tenant.Message);
        var tenantDto = tenant.GetData<TenantDto>()!;
        Assert.IsTrue(tenantDto.Id > 0);
        Assert.AreEqual(created.Id, tenantDto.RoomId);
        Assert.AreEqual(1L, await ScalarAsync(
            "SELECT COUNT(*) FROM tenants WHERE id_card = @c AND room_id = @r", ("@c", FlowCccd), ("@r", created.Id)),
            "Bước 2: người thuê đã gán phòng trong DB.");

        // 3. Hợp đồng Active
        var contract = await router.HandleAsync(RequestPacket.Create(ActionNames.ContractCreate, token,
            new ContractDto(0, created.Id, tenantDto.Id, new DateOnly(2099, 9, 1), new DateOnly(2100, 8, 31),
                3_500_000m, 7_000_000m, ContractStatus.Active, "T12 acceptance")));
        Assert.IsTrue(contract.Success, contract.Message);
        var contractDto = contract.GetData<ContractDto>()!;
        Assert.AreEqual(ContractStatus.Active, contractDto.Status);
        Assert.AreEqual(1L, await ScalarAsync(
            "SELECT COUNT(*) FROM contracts WHERE room_id = @r AND status = 'Active'",
            ("@r", created.Id)),
            "Bước 3: hợp đồng Active trong DB.");

        // 4. Chốt chỉ số tháng 2099-09
        var reading = await router.HandleAsync(RequestPacket.Create(ActionNames.UtilityRecord, token,
            new UtilityReadingDto(0, created.Id, BillingMonth, 100, 250, 3_500m, 20, 45, 10_000m)));
        Assert.IsTrue(reading.Success, reading.Message);
        Assert.AreEqual(1L, await ScalarAsync(
            "SELECT COUNT(*) FROM utility_readings WHERE room_id = @r AND billing_month = @m",
            ("@r", created.Id), ("@m", BillingMonth)),
            "Bước 4: bản ghi chỉ số trong DB.");

        // 5. Lập hóa đơn — BR-10: server tự tính tổng
        var invoice = await router.HandleAsync(RequestPacket.Create(ActionNames.InvoiceCreate, token,
            new CreateInvoiceRequest(created.Id, BillingMonth, 50_000m)));
        Assert.IsTrue(invoice.Success, invoice.Message);
        var invoiceDto = invoice.GetData<InvoiceDto>()!;
        // room 3.500.000 + điện (250-100)*3.500 = 525.000 + nước (45-20)*10.000 = 250.000 + phí 50.000
        Assert.AreEqual(3_500_000m, invoiceDto.RoomAmount);
        Assert.AreEqual(525_000m, invoiceDto.ElectricityAmount);
        Assert.AreEqual(250_000m, invoiceDto.WaterAmount);
        Assert.AreEqual(50_000m, invoiceDto.OtherFees);
        Assert.AreEqual(4_325_000m, invoiceDto.TotalAmount, "BR-10: server tự tính tổng.");
        Assert.AreEqual(InvoiceStatus.Unpaid, invoiceDto.Status);
        Assert.IsNull(invoiceDto.PaidAt);
        Assert.AreEqual(1L, await ScalarAsync(
            "SELECT COUNT(*) FROM invoices WHERE room_id = @r AND billing_month = @m AND total_amount = @t",
            ("@r", created.Id), ("@m", BillingMonth), ("@t", 4_325_000m)),
            "Bước 5: hóa đơn đúng tổng tiền trong DB.");

        // 6. Thanh toán
        var pay = await router.HandleAsync(RequestPacket.Create(ActionNames.InvoicePay, token,
            new { invoiceId = invoiceDto.Id }));
        Assert.IsTrue(pay.Success, pay.Message);
        var paidStatus = await ScalarAsync(
            "SELECT status FROM invoices WHERE id = @id", ("@id", invoiceDto.Id));
        Assert.AreEqual("Paid", paidStatus, "Bước 6: hóa đơn sang Paid.");
        var paidAt = await ScalarAsync("SELECT paid_at FROM invoices WHERE id = @id", ("@id", invoiceDto.Id));
        Assert.AreNotEqual(DBNull.Value, paidAt, "Bước 6: paid_at phải được ghi.");
        Assert.IsNotNull(Convert.ToDateTime(paidAt, CultureInfo.InvariantCulture));

        // 7. Báo cáo tổng quan tháng
        var report = await router.HandleAsync(RequestPacket.Create(ActionNames.ReportSummary, token,
            new { BillingMonth }));
        Assert.IsTrue(report.Success, report.Message);
        var summary = report.GetData<SummaryReportDto>()!;
        Assert.AreEqual(4_325_000m, summary.PaidAmount,
            "Bước 7: tiền đã thu của tháng phải gồm hóa đơn vừa thanh toán.");
        Assert.AreEqual(0m, summary.UnpaidAmount, "Bước 7: tháng 2099-09 không còn nợ.");

        // 8. Xuất danh sách tạm trú
        var export = await router.HandleAsync(RequestPacket.Create(ActionNames.ExportResidence, token, new { }));
        Assert.IsTrue(export.Success, export.Message);
        var residents = export.GetData<List<ResidenceExportDto>>()!;
        var row = residents.FirstOrDefault(r => r.IdCard == FlowCccd);
        Assert.IsNotNull(row, "Bước 8: người thuê T12 phải xuất hiện trong danh sách tạm trú.");
        Assert.AreEqual(FlowRoomNumber, row.RoomNumber);
        Assert.AreEqual(MainTenantName, row.FullName);
    }

    // --------------------------------------------------------------- BR-14 / US-24 phân quyền

    /// <summary>
    /// BR-14 + US-24: tenant gọi action ngoài AUTH_LOGIN + INVOICE_GET_MINE → "Không có quyền.";
    /// landlord gọi INVOICE_GET_MINE cũng bị từ chối. Test qua RequestRouter thật, session thật.
    /// </summary>
    [TestMethod]
    public async Task PermissionMatrix_TenantRejectedOnEveryLandlordAction()
    {
        var router = Router();
        var login = await router.HandleAsync(RequestPacket.Create(
            ActionNames.AuthLogin, null, new LoginRequest(MainCccd, TenantPasswordOf(MainCccd))));
        Assert.IsTrue(login.Success, login.Message);
        var tenantLogin = login.GetData<LoginResult>()!;
        Assert.AreEqual(UserRole.Tenant, tenantLogin.Role);

        // Landlord login để chứng minh cùng action thì LANDLORD được phép (đối chứng).
        var landlordLogin = await router.HandleAsync(RequestPacket.Create(
            ActionNames.AuthLogin, null, new LoginRequest(AdminUsername, AdminPassword)));
        Assert.IsTrue(landlordLogin.Success, landlordLogin.Message);
        var landlordToken = landlordLogin.GetData<LoginResult>()!.Token;

        foreach (var action in ActionNames.All)
        {
            if (action is ActionNames.AuthLogin or ActionNames.InvoiceGetMine)
            {
                continue;
            }

            var response = await router.HandleAsync(RequestPacket.Create(action, tenantLogin.Token, new { }));
            Assert.IsFalse(response.Success, $"Tenant không được phép gọi {action}.");
            Assert.AreEqual("Không có quyền.", response.Message, $"Action {action} phải chặn đúng thông báo.");
        }

        // Đối chứng: landlord được phép ROOM_GET_ALL (không phải "Không có quyền.").
        var landlordRoom = await router.HandleAsync(
            RequestPacket.Create(ActionNames.RoomGetAll, landlordToken, new { }));
        Assert.IsTrue(landlordRoom.Success, $"Landlord phải được ROOM_GET_ALL: {landlordRoom.Message}");
    }

    /// <summary>Landlord gọi INVOICE_GET_MINE → từ chối (US-24: chỉ tenant dùng action này).</summary>
    [TestMethod]
    public async Task PermissionMatrix_LandlordRejectedOnInvoiceGetMine()
    {
        var router = Router();
        var login = await router.HandleAsync(RequestPacket.Create(
            ActionNames.AuthLogin, null, new LoginRequest(AdminUsername, AdminPassword)));
        Assert.IsTrue(login.Success, login.Message);
        var token = login.GetData<LoginResult>()!.Token;

        var response = await router.HandleAsync(
            RequestPacket.Create(ActionNames.InvoiceGetMine, token, new { }));

        Assert.IsFalse(response.Success);
        Assert.AreEqual("Không có quyền.", response.Message);
    }

    /// <summary>BR-14: INVOICE_GET_MINE chỉ trả hóa đơn phòng tenant đang ở, không lây phòng khác.</summary>
    [TestMethod]
    public async Task InvoiceGetMine_ReturnsOnlyInvoicesOfTenantsCurrentRoom()
    {
        await SeedSecondRoomWithInvoiceAsync();

        var router = Router();
        var login = await router.HandleAsync(RequestPacket.Create(
            ActionNames.AuthLogin, null, new LoginRequest(MainCccd, TenantPasswordOf(MainCccd))));
        Assert.IsTrue(login.Success, login.Message);
        var tenantToken = login.GetData<LoginResult>()!.Token;

        // Hóa đơn phòng chính (MainRoomId) tháng BillingMonth đã seed.
        var mine = await router.HandleAsync(
            RequestPacket.Create(ActionNames.InvoiceGetMine, tenantToken, new { }));

        Assert.IsTrue(mine.Success, mine.Message);
        var page = mine.GetData<InvoiceMinePageDto>()!;
        Assert.IsNotNull(page);
        Assert.AreEqual(1, page.TotalCount, "Tenant chỉ thấy đúng 1 hóa đơn phòng mình.");
        Assert.AreEqual(1, page.Items.Count);
        Assert.AreEqual(MainRoomId, page.Items[0].RoomId);
        Assert.AreEqual(BillingMonth, page.Items[0].BillingMonth);
        Assert.AreEqual(2_000_000m, page.Items[0].TotalAmount);
    }

    /// <summary>US-24: INVOICE_GET_MINE hỗ trợ phân trang server-side page/pageSize.</summary>
    [TestMethod]
    public async Task InvoiceGetMine_SupportsServerSidePagination()
    {
        await SeedSecondRoomWithInvoiceAsync();
        var router = Router();
        var login = await router.HandleAsync(RequestPacket.Create(
            ActionNames.AuthLogin, null, new LoginRequest(MainCccd, TenantPasswordOf(MainCccd))));
        Assert.IsTrue(login.Success, login.Message);
        var tenantToken = login.GetData<LoginResult>()!.Token;

        // Trang 1, pageSize 1: lấy đúng 1 phần tử
        var page1Res = await router.HandleAsync(
            RequestPacket.Create(ActionNames.InvoiceGetMine, tenantToken, new { page = 1, pageSize = 1 }));
        Assert.IsTrue(page1Res.Success, page1Res.Message);
        var page1 = page1Res.GetData<InvoiceMinePageDto>()!;
        Assert.AreEqual(1, page1.Page);
        Assert.AreEqual(1, page1.PageSize);
        Assert.IsTrue(page1.TotalCount >= 1);
        Assert.AreEqual(1, page1.Items.Count);

        // Trang 999: vượt quá số lượng -> trả danh sách rỗng, TotalCount vẫn giữ nguyên
        var pageFarRes = await router.HandleAsync(
            RequestPacket.Create(ActionNames.InvoiceGetMine, tenantToken, new { page = 999, pageSize = 10 }));
        Assert.IsTrue(pageFarRes.Success, pageFarRes.Message);
        var pageFar = pageFarRes.GetData<InvoiceMinePageDto>()!;
        Assert.AreEqual(999, pageFar.Page);
        Assert.AreEqual(10, pageFar.PageSize);
        Assert.AreEqual(page1.TotalCount, pageFar.TotalCount);
        Assert.AreEqual(0, pageFar.Items.Count);
    }

    // --------------------------------------------------------------- US-23 khóa đăng nhập

    /// <summary>
    /// US-23 qua router thật: sai 5 lần → lần 6 đúng mật khẩu vẫn bị khóa; kim đồng hồ fake
    /// tiến 1 phút thì mở khóa và đăng nhập thành công; thành công reset bộ đếm.
    /// </summary>
    [TestMethod]
    public async Task Lockout_FiveWrongAttemptsBlockSixthUntilClockAdvancesThenSuccessResets()
    {
        var clock = new FakeClock();
        var sessions = new SessionStore();
        var auth = new AuthService(
            new AdminUserRepository(), new AcceptanceTenantRepository(), sessions, clock.Now);
        var router = MakeRouter(auth, sessions);

        var request = new LoginRequest(AdminUsername, AdminPassword);
        for (var i = 0; i < AuthService.MaxFailedAttempts; i++)
        {
            var wrong = await router.HandleAsync(RequestPacket.Create(
                ActionNames.AuthLogin, null, new LoginRequest(AdminUsername, "sai-mat-khau")));
            Assert.IsFalse(wrong.Success, $"Lần sai thứ {i + 1} phải thất bại.");
        }

        // Lần 6 — đúng mật khẩu vẫn bị khóa (US-23).
        var locked = await router.HandleAsync(RequestPacket.Create(ActionNames.AuthLogin, null, request));
        Assert.IsFalse(locked.Success, "Lần 6 đúng mật khẩu vẫn phải bị khóa sau 5 lần sai.");
        StringAssert.Contains(locked.Message, "Tài khoản tạm khóa");

        // Tiến kim đồng hồ 1 phút → mở khóa, đăng nhập thành công.
        clock.Advance(AuthService.LockoutWindow);
        var unlocked = await router.HandleAsync(RequestPacket.Create(ActionNames.AuthLogin, null, request));
        Assert.IsTrue(unlocked.Success, $"Sau 1 phút phải đăng nhập được: {unlocked.Message}");
        var result = unlocked.GetData<LoginResult>()!;
        Assert.AreEqual(UserRole.Landlord, result.Role);

        // Thành công reset bộ đếm: sai tiếp 4 lần không khóa ngay (đủ 5 mới khóa).
        for (var i = 0; i < AuthService.MaxFailedAttempts - 1; i++)
        {
            var wrong = await router.HandleAsync(RequestPacket.Create(
                ActionNames.AuthLogin, null, new LoginRequest(AdminUsername, "sai-nua")));
            Assert.IsFalse(wrong.Success);
        }
        var stillOk = await router.HandleAsync(RequestPacket.Create(ActionNames.AuthLogin, null, request));
        Assert.IsTrue(stillOk.Success, "Bộ đếm đã reset sau thành công — đăng nhập đúng vẫn qua.");
    }

    // --------------------------------------------------------------- BR còn thiếu qua MySQL thật

    /// <summary>BR-04: phòng chỉ được 1 hợp đồng Active; tạo thứ hai → BusinessRuleException.</summary>
    [TestMethod]
    public async Task Br04_SecondActiveContractForSameRoomRejected()
    {
        await SeedContractAsync();

        var service = new ContractService(new ContractRepository(Db));
        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => service.CreateAsync(new ContractDto(
                0, MainRoomId, MainTenantId, new DateOnly(2099, 10, 1), new DateOnly(2100, 9, 30),
                3_000_000m, 0m, ContractStatus.Active, "BR-04")));

        StringAssert.Contains(ex.Message, "hiệu lực");
        Assert.AreEqual(1L, await ScalarAsync(
            "SELECT COUNT(*) FROM contracts WHERE room_id = @r AND status = 'Active'", ("@r", MainRoomId)));
    }

    /// <summary>BR-05: đại diện ký HĐ phải đang ở trong phòng đó.</summary>
    [TestMethod]
    public async Task Br05_RepresentativeNotInRoomRejected()
    {
        // LandlordTenantId ở LandlordRoomId — không thuộc MainRoomId.
        var service = new ContractService(new ContractRepository(Db));
        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => service.CreateAsync(new ContractDto(
                0, MainRoomId, LandlordTenantId, new DateOnly(2099, 9, 1), new DateOnly(2100, 8, 31),
                3_000_000m, 0m, ContractStatus.Active, "BR-05")));

        StringAssert.Contains(ex.Message, "đang ở trong phòng này");
    }

    /// <summary>BR-06: EndDate phải sau StartDate.</summary>
    [TestMethod]
    public async Task Br06_EndDateNotAfterStartDateRejected()
    {
        var service = new ContractService(new ContractRepository(Db));
        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => service.CreateAsync(new ContractDto(
                0, MainRoomId, MainTenantId, new DateOnly(2099, 9, 1), new DateOnly(2099, 9, 1),
                3_000_000m, 0m, ContractStatus.Active, "BR-06")));

        StringAssert.Contains(ex.Message, "sau ngày bắt đầu");
        Assert.AreEqual(0L, await ScalarAsync(
            "SELECT COUNT(*) FROM contracts WHERE notes = 'BR-06'"), "HĐ sai ngày không được vào DB.");
    }

    /// <summary>BR-09: lập hóa đơn cần HĐ Active + chỉ số điện nước đã chốt của tháng.</summary>
    [TestMethod]
    public async Task Br09_InvoiceRequiresActiveContractAndUtilityReading()
    {
        var contractId = await SeedContractAsync();

        var service = new InvoiceService(new InvoiceRepository(Db));

        // Chưa chốt điện nước tháng 2099-10 → chặn.
        var noReading = await Assert.ThrowsExceptionAsync<BusinessRuleException>(
            () => service.CreateAsync(new CreateInvoiceRequest(MainRoomId, OtherMonth, 0m)));
        StringAssert.Contains(noReading.Message, "điện nước");

        // Chốt điện nước xong → tạo hóa đơn được (đủ điều kiện).
        var utility = new UtilityService(new UtilityRepository(Db));
        await utility.RecordAsync(new UtilityReadingDto(
            0, MainRoomId, OtherMonth, 100, 110, 3_500m, 20, 22, 10_000m));
        var created = await service.CreateAsync(new CreateInvoiceRequest(MainRoomId, OtherMonth, 0m));
        Assert.AreEqual(InvoiceStatus.Unpaid, created.Status);
        Assert.AreEqual(3_055_000m, created.TotalAmount); // 3.000.000 + 10*3.500 + 2*10.000
        Assert.IsTrue(created.Id > 0);
        Assert.AreEqual(1L, await ScalarAsync(
            "SELECT COUNT(*) FROM invoices WHERE room_id = @r AND billing_month = @m",
            ("@r", MainRoomId), ("@m", OtherMonth)), "Hóa đơn đủ điều kiện phải vào DB.");
    }

    /// <summary>BR-11: hóa đơn Paid không thanh toán lần hai; status/paid_at giữ nguyên.</summary>
    [TestMethod]
    public async Task Br11_PaidInvoiceCannotBePaidAgain()
    {
        var invoiceId = await SeedPaidInvoiceAsync();
        var paidAtBefore = await ScalarAsync("SELECT paid_at FROM invoices WHERE id = @id", ("@id", invoiceId));

        var service = new InvoiceService(new InvoiceRepository(Db));
        var ex = await Assert.ThrowsExceptionAsync<BusinessRuleException>(() => service.PayAsync(invoiceId));

        StringAssert.Contains(ex.Message, "đã thanh toán");
        Assert.AreEqual(1L, await ScalarAsync(
            "SELECT COUNT(*) FROM invoices WHERE id = @id AND status = 'Paid'",
            ("@id", invoiceId)),
            "Trạng thái phải giữ nguyên là Paid.");
        Assert.AreEqual(paidAtBefore,
            await ScalarAsync("SELECT paid_at FROM invoices WHERE id = @id", ("@id", invoiceId)),
            "paid_at không được đổi.");
    }

    // --------------------------------------------------------------- helpers

    private const string AdminUsername = "admin";
    private const string AdminPassword = "admin-pass";
    private const string MainTenantName = "T12 Người Chính";

    /// <summary>Router thật + session store dùng chung với AuthService (token do auth tạo phải tra được).</summary>
    private static RequestRouter Router()
    {
        var sessions = new SessionStore();
        var auth = new AuthService(new AdminUserRepository(), new AcceptanceTenantRepository(), sessions);
        return MakeRouter(auth, sessions);
    }

    private static RequestRouter MakeRouter(AuthService auth, SessionStore sessions) => new(
        auth,
        new RoomService(new RoomRepository(Db)),
        new TenantService(new TenantRepository(Db)),
        new ContractService(new ContractRepository(Db)),
        new UtilityService(new UtilityRepository(Db)),
        new InvoiceService(new InvoiceRepository(Db)),
        new ReportService(new ReportRepository(Db)),
        new ResidenceService(new ResidenceRepository(Db)),
        sessions);

    private static TenantDto TenantDto(string cccd, string fullName, int roomId) =>
        new(0, roomId, fullName, new DateOnly(1999, 5, 10), cccd, "09000000" + cccd[^4..], "Đà Nẵng", null, false);

    private static string TenantPasswordOf(string cccd) => cccd[^6..];

    /// <summary>Đăng nhập admin qua router thật, trả token (dùng cho các action của chủ trọ).</summary>
    private static async Task<string> LandlordTokenAsync(RequestRouter router)
    {
        var login = await router.HandleAsync(RequestPacket.Create(
            ActionNames.AuthLogin, null, new LoginRequest(AdminUsername, AdminPassword)));
        Assert.IsTrue(login.Success, $"Đăng nhập admin phải thành công: {login.Message}");
        return login.GetData<LoginResult>()!.Token;
    }

    /// <summary>Phòng nền + người thuê nền cho các BR test. Idempotent sau Cleanup.</summary>
    private static async Task SeedRoomsAsync()
    {
        await using var connection = await Db.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        foreach (var (id, number) in new[]
        {
            (MainRoomId, MainRoomNumber), (SecondRoomId, SecondRoomNumber), (LandlordRoomId, LandlordRoomNumber),
        })
        {
            await using var command = new MySqlCommand(
                """
                INSERT INTO rooms (id, room_number, price, max_occupants, status, description)
                VALUES (@id, @number, 1000000, 4, 'Available', 'AcceptanceTests T12')
                ON DUPLICATE KEY UPDATE room_number = room_number
                """, connection, transaction);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@number", number);
            await command.ExecuteNonQueryAsync();
        }

        foreach (var (id, roomId, name, cccd) in new[]
        {
            (MainTenantId, MainRoomId, MainTenantName, MainCccd),
            (ExtraTenantId, SecondRoomId, "T12 Người Phòng Khác", ExtraCccd),
            (LandlordTenantId, LandlordRoomId, "T12 Người Sai Phòng", LandlordCccd),
        })
        {
            await using var command = new MySqlCommand(
                """
                INSERT INTO tenants (id, room_id, full_name, dob, id_card, password_hash, phone, hometown, workplace, is_temporary_registered)
                VALUES (@id, @roomId, @name, '1999-05-10', @cccd, @hash, @phone, 'Đà Nẵng', NULL, 1)
                ON DUPLICATE KEY UPDATE room_id = @roomId
                """, connection, transaction);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@roomId", roomId);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@cccd", cccd);
            command.Parameters.AddWithValue("@hash", PasswordHasher.Hash(TenantPasswordOf(cccd)));
            command.Parameters.AddWithValue("@phone", $"090000{id:D4}");
            await command.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }

    private static async Task SeedSecondRoomWithInvoiceAsync()
    {
        await using var connection = await Db.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var mainContractId = await InsertContractAsync(
            connection, transaction, MainRoomId, MainTenantId, 2_000_000m, "AcceptanceTests T12 second");
        var otherContractId = await InsertContractAsync(
            connection, transaction, SecondRoomId, ExtraTenantId, 900_000m, "AcceptanceTests T12 other room");

        await InsertInvoiceAsync(connection, transaction, MainRoomId, mainContractId, 2_000_000m);
        await InsertInvoiceAsync(connection, transaction, SecondRoomId, otherContractId, 900_000m);

        await transaction.CommitAsync();
    }

    private static async Task<int> InsertContractAsync(
        MySqlConnection connection, MySqlTransaction transaction, int roomId, int tenantId, decimal price, string notes)
    {
        await using var command = new MySqlCommand(
            """
            INSERT INTO contracts (room_id, representative_tenant_id, start_date, end_date, rental_price, deposit_amount, status, notes)
            VALUES (@roomId, @tenantId, '2099-09-01', '2100-08-31', @price, 0, 'Active', @notes);
            SELECT LAST_INSERT_ID();
            """, connection, transaction);
        command.Parameters.AddWithValue("@roomId", roomId);
        command.Parameters.AddWithValue("@tenantId", tenantId);
        command.Parameters.AddWithValue("@price", price);
        command.Parameters.AddWithValue("@notes", notes);
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    private static async Task InsertInvoiceAsync(
        MySqlConnection connection, MySqlTransaction transaction, int roomId, int contractId, decimal total)
    {
        await using var command = new MySqlCommand(
            """
            INSERT INTO invoices (room_id, contract_id, billing_month, room_amount, electricity_amount,
                                  water_amount, other_fees, total_amount, status)
            VALUES (@roomId, @contractId, @month, @total, 0, 0, 0, @total, 'Unpaid')
            """, connection, transaction);
        command.Parameters.AddWithValue("@roomId", roomId);
        command.Parameters.AddWithValue("@contractId", contractId);
        command.Parameters.AddWithValue("@month", BillingMonth);
        command.Parameters.AddWithValue("@total", total);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<int> SeedContractAsync()
    {
        await using var connection = await Db.OpenAsync();
        await using var command = new MySqlCommand(
            """
            INSERT INTO contracts (room_id, representative_tenant_id, start_date, end_date, rental_price, deposit_amount, status, notes)
            VALUES (@roomId, @tenantId, '2099-09-01', '2100-08-31', 3000000, 0, 'Active', 'AcceptanceTests BR')
            """, connection);
        command.Parameters.AddWithValue("@roomId", MainRoomId);
        command.Parameters.AddWithValue("@tenantId", MainTenantId);
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    private static async Task<int> SeedPaidInvoiceAsync()
    {
        await using var connection = await Db.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        var contractId = await InsertContractAsync(
            connection, transaction, MainRoomId, MainTenantId, 3_000_000m, "AcceptanceTests BR");

        await using var command = new MySqlCommand(
            """
            INSERT INTO invoices (room_id, contract_id, billing_month, room_amount, electricity_amount,
                                  water_amount, other_fees, total_amount, status, paid_at)
            VALUES (@roomId, @contractId, @month, 3000000, 0, 0, 0, 3000000, 'Paid', NOW())
            """, connection, transaction);
        command.Parameters.AddWithValue("@roomId", MainRoomId);
        command.Parameters.AddWithValue("@contractId", contractId);
        command.Parameters.AddWithValue("@month", BillingMonth);
        await command.ExecuteNonQueryAsync();

        await transaction.CommitAsync();

        await using var idConnection = await Db.OpenAsync();
        await using var idCommand = new MySqlCommand(
            "SELECT id FROM invoices WHERE room_id = @r AND billing_month = @m", idConnection);
        idCommand.Parameters.AddWithValue("@r", MainRoomId);
        idCommand.Parameters.AddWithValue("@m", BillingMonth);
        return Convert.ToInt32(await idCommand.ExecuteScalarAsync());
    }

    private static async Task<object?> ScalarAsync(string sql, params (string Name, object Value)[] parameters)
    {
        await using var connection = await Db.OpenAsync();
        await using var command = new MySqlCommand(sql, connection);
        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        return await command.ExecuteScalarAsync();
    }

    private static async Task CleanupAsync()
    {
        await using var connection = await Db.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        foreach (var sql in new[]
        {
            // Phòng luồng tháng do service tự cấp id (auto_increment) nên dọn theo tiền tố số phòng.
            "DELETE i FROM invoices i JOIN rooms r ON r.id = i.room_id WHERE r.room_number LIKE 'T12-%' OR r.id BETWEEN 9204 AND 9215",
            "DELETE u FROM utility_readings u JOIN rooms r ON r.id = u.room_id WHERE r.room_number LIKE 'T12-%' OR r.id BETWEEN 9204 AND 9215",
            "DELETE c FROM contracts c JOIN rooms r ON r.id = c.room_id WHERE r.room_number LIKE 'T12-%' OR r.id BETWEEN 9204 AND 9215",
            // Chỉ CCCD của Task 12 — KHÔNG dùng LIKE để khỏi quét trúng dải của test khác.
            $"DELETE FROM tenants WHERE id_card IN ('{MainCccd}', '{ExtraCccd}', '{LandlordCccd}', '{FlowCccd}')",
            "DELETE FROM rooms WHERE room_number LIKE 'T12-%' OR id BETWEEN 9204 AND 9215",
        })
        {
            await using var command = new MySqlCommand(sql, connection, transaction);
            await command.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }

    private sealed class FakeClock
    {
        private DateTimeOffset _now = new(2026, 9, 11, 8, 0, 0, TimeSpan.Zero);
        public DateTimeOffset Now() => _now;
        public void Advance(TimeSpan delta) => _now = _now.Add(delta);
    }

    /// <summary>Repo user admin — trả record admin cho đúng username, ngoài ra null.</summary>
    private sealed class AdminUserRepository : IUserRepository
    {
        private readonly UserRecord _admin = new(1, PasswordHasher.Hash(AdminPassword), "Chủ Trọ Demo");

        public Task<UserRecord?> FindByUsernameAsync(string username, CancellationToken ct = default) =>
            Task.FromResult(string.Equals(username, AdminUsername, StringComparison.OrdinalIgnoreCase)
                ? _admin
                : null);
    }

    /// <summary>Repo tenant qua MySQL thật — router cần tra CCCD để đăng nhập tenant.</summary>
    private sealed class AcceptanceTenantRepository : ITenantRepository
    {
        public async Task<TenantAuthRecord?> FindByCccdAsync(string idCard, CancellationToken ct = default)
        {
            await using var connection = await Db.OpenAsync(ct);
            await using var command = new MySqlCommand(
                "SELECT id, id_card, password_hash, full_name, room_id FROM tenants WHERE id_card = @c",
                connection);
            command.Parameters.AddWithValue("@c", idCard);

            await using var reader = await command.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
            {
                return null;
            }

            return new TenantAuthRecord(
                reader.GetInt32("id"),
                reader.GetString("id_card"),
                reader.IsDBNull(reader.GetOrdinal("password_hash")) ? string.Empty : reader.GetString("password_hash"),
                reader.GetString("full_name"),
                reader.IsDBNull(reader.GetOrdinal("room_id")) ? null : reader.GetInt32("room_id"));
        }
    }
}
