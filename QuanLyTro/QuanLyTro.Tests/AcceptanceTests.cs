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
    private const string KyCuoc = "2099-09";
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
            ActionNames.PhongThem, token, new PhongDto(0, FlowRoomNumber, 3_500_000m, 4, TrangThaiPhong.Trong, "T12", 0)));
        Assert.IsTrue(room.Success, room.Message);
        var created = room.GetData<PhongDto>()!;
        Assert.IsTrue(created.Id > 0);
        Assert.AreEqual(1L, await ScalarAsync("SELECT COUNT(*) FROM phong WHERE id = @id", ("@id", created.Id)),
            "Bước 1: phòng phải nằm trong DB.");

        // 2. Thêm người thuê (mật khẩu mặc định 6 số cuối CCCD)
        var tenant = await router.HandleAsync(RequestPacket.Create(ActionNames.KhachThueThem, token,
            new { tenant = KhachThueDto(FlowCccd, MainTenantName, created.Id), plainPassword = (string?)null }));
        Assert.IsTrue(tenant.Success, tenant.Message);
        var tenantDto = tenant.GetData<KhachThueDto>()!;
        Assert.IsTrue(tenantDto.Id > 0);
        Assert.AreEqual(created.Id, tenantDto.PhongId);
        Assert.AreEqual(1L, await ScalarAsync(
            "SELECT COUNT(*) FROM khach_thue WHERE cccd = @c AND phong_id = @r", ("@c", FlowCccd), ("@r", created.Id)),
            "Bước 2: người thuê đã gán phòng trong DB.");

        // 3. Hợp đồng HieuLuc
        var contract = await router.HandleAsync(RequestPacket.Create(ActionNames.HopDongTao, token,
            new HopDongDto(0, created.Id, tenantDto.Id, new DateOnly(2099, 9, 1), new DateOnly(2100, 8, 31),
                3_500_000m, 7_000_000m, TrangThaiHopDong.HieuLuc, "T12 acceptance")));
        Assert.IsTrue(contract.Success, contract.Message);
        var contractDto = contract.GetData<HopDongDto>()!;
        Assert.AreEqual(TrangThaiHopDong.HieuLuc, contractDto.TrangThai);
        Assert.AreEqual(1L, await ScalarAsync(
            "SELECT COUNT(*) FROM hop_dong WHERE phong_id = @r AND trang_thai = 'HieuLuc'",
            ("@r", created.Id)),
            "Bước 3: hợp đồng HieuLuc trong DB.");

        // 4. Chốt chỉ số tháng 2099-09
        var reading = await router.HandleAsync(RequestPacket.Create(ActionNames.DienNuocGhiSo, token,
            new ChiSoDienNuocDto(0, created.Id, KyCuoc, 100, 250, 3_500m, 20, 45, 10_000m)));
        Assert.IsTrue(reading.Success, reading.Message);
        Assert.AreEqual(1L, await ScalarAsync(
            "SELECT COUNT(*) FROM chi_so_dien_nuoc WHERE phong_id = @r AND ky_cuoc = @m",
            ("@r", created.Id), ("@m", KyCuoc)),
            "Bước 4: bản ghi chỉ số trong DB.");

        // 5. Lập hóa đơn — BR-10: server tự tính tổng
        var invoice = await router.HandleAsync(RequestPacket.Create(ActionNames.HoaDonTao, token,
            new YeuCauTaoHoaDon(created.Id, KyCuoc, 50_000m)));
        Assert.IsTrue(invoice.Success, invoice.Message);
        var invoiceDto = invoice.GetData<HoaDonDto>()!;
        // room 3.500.000 + điện (250-100)*3.500 = 525.000 + nước (45-20)*10.000 = 250.000 + phí 50.000
        Assert.AreEqual(3_500_000m, invoiceDto.TienPhong);
        Assert.AreEqual(525_000m, invoiceDto.TienDien);
        Assert.AreEqual(250_000m, invoiceDto.TienNuoc);
        Assert.AreEqual(50_000m, invoiceDto.PhiKhac);
        Assert.AreEqual(4_325_000m, invoiceDto.TongTien, "BR-10: server tự tính tổng.");
        Assert.AreEqual(TrangThaiHoaDon.ChuaThu, invoiceDto.TrangThai);
        Assert.IsNull(invoiceDto.NgayDong);
        Assert.AreEqual(1L, await ScalarAsync(
            "SELECT COUNT(*) FROM hoa_don WHERE phong_id = @r AND ky_cuoc = @m AND tong_tien = @t",
            ("@r", created.Id), ("@m", KyCuoc), ("@t", 4_325_000m)),
            "Bước 5: hóa đơn đúng tổng tiền trong DB.");

        // 6. Thanh toán
        var pay = await router.HandleAsync(RequestPacket.Create(ActionNames.HoaDonThanhToan, token,
            new { hoaDonId = invoiceDto.Id }));
        Assert.IsTrue(pay.Success, pay.Message);
        var paidStatus = await ScalarAsync(
            "SELECT trang_thai FROM hoa_don WHERE id = @id", ("@id", invoiceDto.Id));
        Assert.AreEqual("DaThu", paidStatus, "Bước 6: hóa đơn sang DaThu.");
        var ngayDong = await ScalarAsync("SELECT ngay_dong FROM hoa_don WHERE id = @id", ("@id", invoiceDto.Id));
        Assert.AreNotEqual(DBNull.Value, ngayDong, "Bước 6: ngay_dong phải được ghi.");
        Assert.IsNotNull(Convert.ToDateTime(ngayDong, CultureInfo.InvariantCulture));

        // 7. Báo cáo tổng quan tháng
        var report = await router.HandleAsync(RequestPacket.Create(ActionNames.BaoCaoTongQuan, token,
            new { KyCuoc }));
        Assert.IsTrue(report.Success, report.Message);
        var summary = report.GetData<BaoCaoTongQuanDto>()!;
        Assert.AreEqual(4_325_000m, summary.SoTienDaThu,
            "Bước 7: tiền đã thu của tháng phải gồm hóa đơn vừa thanh toán.");
        Assert.AreEqual(0m, summary.SoTienChuaThu, "Bước 7: tháng 2099-09 không còn nợ.");

        // 8. Xuất danh sách tạm trú
        var export = await router.HandleAsync(RequestPacket.Create(ActionNames.XuatHoSoTamTru, token, new { }));
        Assert.IsTrue(export.Success, export.Message);
        var residents = export.GetData<List<XuatHoSoTamTruDto>>()!;
        var row = residents.FirstOrDefault(r => r.Cccd == FlowCccd);
        Assert.IsNotNull(row, "Bước 8: người thuê T12 phải xuất hiện trong danh sách tạm trú.");
        Assert.AreEqual(FlowRoomNumber, row.SoPhong);
        Assert.AreEqual(MainTenantName, row.HoTen);
    }

    // --------------------------------------------------------------- BR-14 / US-24 phân quyền

    /// <summary>
    /// BR-14 + US-24: tenant gọi hanh_dong ngoài DANG_NHAP + HOA_DON_CUA_TOI → "Không có quyền.";
    /// landlord gọi HOA_DON_CUA_TOI cũng bị từ chối. Test qua DieuPhoiYeuCau thật, session thật.
    /// </summary>
    [TestMethod]
    public async Task PermissionMatrix_TenantRejectedOnEveryLandlordAction()
    {
        var router = Router();
        var login = await router.HandleAsync(RequestPacket.Create(
            ActionNames.DangNhap, null, new YeuCauDangNhap(MainCccd, TenantPasswordOf(MainCccd))));
        Assert.IsTrue(login.Success, login.Message);
        var tenantLogin = login.GetData<KetQuaDangNhap>()!;
        Assert.AreEqual(VaiTroNguoiDung.KhachThue, tenantLogin.VaiTro);

        // ChuTro login để chứng minh cùng hanh_dong thì LANDLORD được phép (đối chứng).
        var landlordLogin = await router.HandleAsync(RequestPacket.Create(
            ActionNames.DangNhap, null, new YeuCauDangNhap(AdminUsername, AdminPassword)));
        Assert.IsTrue(landlordLogin.Success, landlordLogin.Message);
        var landlordToken = landlordLogin.GetData<KetQuaDangNhap>()!.Token;

        foreach (var hanh_dong in ActionNames.All)
        {
            // DangNhap/DangKy: công khai, không cần phiên (BR-17).
            // HoaDonCuaToi, KhachThueNhanPhongQr: hành động RIÊNG của KhachThue (US-24, BR-19) — không phải hành động chủ trọ.
            if (hanh_dong is ActionNames.DangNhap or ActionNames.DangKy
                or ActionNames.HoaDonCuaToi or ActionNames.KhachThueNhanPhongQr)
            {
                continue;
            }

            var response = await router.HandleAsync(RequestPacket.Create(hanh_dong, tenantLogin.Token, new { }));
            Assert.IsFalse(response.Success, $"KhachThue không được phép gọi {hanh_dong}.");
            Assert.AreEqual("Không có quyền.", response.Message, $"Action {hanh_dong} phải chặn đúng thông báo.");
        }

        // Đối chứng: landlord được phép PHONG_LAY_TAT_CA (không phải "Không có quyền.").
        var landlordRoom = await router.HandleAsync(
            RequestPacket.Create(ActionNames.PhongLayTatCa, landlordToken, new { }));
        Assert.IsTrue(landlordRoom.Success, $"ChuTro phải được PHONG_LAY_TAT_CA: {landlordRoom.Message}");
    }

    /// <summary>ChuTro gọi HOA_DON_CUA_TOI → từ chối (US-24: chỉ tenant dùng hanh_dong này).</summary>
    [TestMethod]
    public async Task PermissionMatrix_LandlordRejectedOnInvoiceGetMine()
    {
        var router = Router();
        var login = await router.HandleAsync(RequestPacket.Create(
            ActionNames.DangNhap, null, new YeuCauDangNhap(AdminUsername, AdminPassword)));
        Assert.IsTrue(login.Success, login.Message);
        var token = login.GetData<KetQuaDangNhap>()!.Token;

        var response = await router.HandleAsync(
            RequestPacket.Create(ActionNames.HoaDonCuaToi, token, new { }));

        Assert.IsFalse(response.Success);
        Assert.AreEqual("Không có quyền.", response.Message);
    }

    /// <summary>BR-14: HOA_DON_CUA_TOI chỉ trả hóa đơn phòng tenant đang ở, không lây phòng khác.</summary>
    [TestMethod]
    public async Task InvoiceGetMine_ReturnsOnlyInvoicesOfTenantsCurrentRoom()
    {
        await SeedSecondRoomWithInvoiceAsync();

        var router = Router();
        var login = await router.HandleAsync(RequestPacket.Create(
            ActionNames.DangNhap, null, new YeuCauDangNhap(MainCccd, TenantPasswordOf(MainCccd))));
        Assert.IsTrue(login.Success, login.Message);
        var tenantToken = login.GetData<KetQuaDangNhap>()!.Token;

        // Hóa đơn phòng chính (MainRoomId) tháng KyCuoc đã seed.
        var mine = await router.HandleAsync(
            RequestPacket.Create(ActionNames.HoaDonCuaToi, tenantToken, new { }));

        Assert.IsTrue(mine.Success, mine.Message);
        var page = mine.GetData<TrangHoaDonCuaToiDto>()!;
        Assert.IsNotNull(page);
        Assert.AreEqual(1, page.TongSo, "KhachThue chỉ thấy đúng 1 hóa đơn phòng mình.");
        Assert.AreEqual(1, page.DanhSach.Count);
        Assert.AreEqual(MainRoomId, page.DanhSach[0].PhongId);
        Assert.AreEqual(KyCuoc, page.DanhSach[0].KyCuoc);
        Assert.AreEqual(2_000_000m, page.DanhSach[0].TongTien);
    }

    /// <summary>US-24: HOA_DON_CUA_TOI hỗ trợ phân trang server-side page/soLuongMoiTrang.</summary>
    [TestMethod]
    public async Task InvoiceGetMine_SupportsServerSidePagination()
    {
        await SeedSecondRoomWithInvoiceAsync();
        var router = Router();
        var login = await router.HandleAsync(RequestPacket.Create(
            ActionNames.DangNhap, null, new YeuCauDangNhap(MainCccd, TenantPasswordOf(MainCccd))));
        Assert.IsTrue(login.Success, login.Message);
        var tenantToken = login.GetData<KetQuaDangNhap>()!.Token;

        // Trang 1, soLuongMoiTrang 1: lấy đúng 1 phần tử
        var page1Res = await router.HandleAsync(
            RequestPacket.Create(ActionNames.HoaDonCuaToi, tenantToken, new { page = 1, soLuongMoiTrang = 1 }));
        Assert.IsTrue(page1Res.Success, page1Res.Message);
        var page1 = page1Res.GetData<TrangHoaDonCuaToiDto>()!;
        Assert.AreEqual(1, page1.Trang);
        Assert.AreEqual(1, page1.SoLuongMoiTrang);
        Assert.IsTrue(page1.TongSo >= 1);
        Assert.AreEqual(1, page1.DanhSach.Count);

        // Trang 999: vượt quá số lượng -> trả danh sách rỗng, TongSo vẫn giữ nguyên
        var pageFarRes = await router.HandleAsync(
            RequestPacket.Create(ActionNames.HoaDonCuaToi, tenantToken, new { page = 999, soLuongMoiTrang = 10 }));
        Assert.IsTrue(pageFarRes.Success, pageFarRes.Message);
        var pageFar = pageFarRes.GetData<TrangHoaDonCuaToiDto>()!;
        Assert.AreEqual(999, pageFar.Trang);
        Assert.AreEqual(10, pageFar.SoLuongMoiTrang);
        Assert.AreEqual(page1.TongSo, pageFar.TongSo);
        Assert.AreEqual(0, pageFar.DanhSach.Count);
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
        var auth = new XacThucService(
            new AdminUserRepository(), new AcceptanceTenantRepository(), sessions, clock.Now);
        var router = MakeRouter(auth, sessions);

        var request = new YeuCauDangNhap(AdminUsername, AdminPassword);
        for (var i = 0; i < XacThucService.MaxFailedAttempts; i++)
        {
            var wrong = await router.HandleAsync(RequestPacket.Create(
                ActionNames.DangNhap, null, new YeuCauDangNhap(AdminUsername, "sai-mat-khau")));
            Assert.IsFalse(wrong.Success, $"Lần sai thứ {i + 1} phải thất bại.");
        }

        // Lần 6 — đúng mật khẩu vẫn bị khóa (US-23).
        var locked = await router.HandleAsync(RequestPacket.Create(ActionNames.DangNhap, null, request));
        Assert.IsFalse(locked.Success, "Lần 6 đúng mật khẩu vẫn phải bị khóa sau 5 lần sai.");
        StringAssert.Contains(locked.Message, "Tài khoản tạm khóa");

        // Tiến kim đồng hồ 1 phút → mở khóa, đăng nhập thành công.
        clock.Advance(XacThucService.LockoutWindow);
        var unlocked = await router.HandleAsync(RequestPacket.Create(ActionNames.DangNhap, null, request));
        Assert.IsTrue(unlocked.Success, $"Sau 1 phút phải đăng nhập được: {unlocked.Message}");
        var result = unlocked.GetData<KetQuaDangNhap>()!;
        Assert.AreEqual(VaiTroNguoiDung.ChuTro, result.VaiTro);

        // Thành công reset bộ đếm: sai tiếp 4 lần không khóa ngay (đủ 5 mới khóa).
        for (var i = 0; i < XacThucService.MaxFailedAttempts - 1; i++)
        {
            var wrong = await router.HandleAsync(RequestPacket.Create(
                ActionNames.DangNhap, null, new YeuCauDangNhap(AdminUsername, "sai-nua")));
            Assert.IsFalse(wrong.Success);
        }
        var stillOk = await router.HandleAsync(RequestPacket.Create(ActionNames.DangNhap, null, request));
        Assert.IsTrue(stillOk.Success, "Bộ đếm đã reset sau thành công — đăng nhập đúng vẫn qua.");
    }

    // --------------------------------------------------------------- BR còn thiếu qua MySQL thật

    /// <summary>BR-04: phòng chỉ được 1 hợp đồng HieuLuc; tạo thứ hai → LoiNghiepVu.</summary>
    [TestMethod]
    public async Task Br04_SecondActiveContractForSameRoomRejected()
    {
        await SeedContractAsync();

        var service = new HopDongService(new HopDongRepository(Db));
        var ex = await Assert.ThrowsExceptionAsync<LoiNghiepVu>(
            () => service.CreateAsync(new HopDongDto(
                0, MainRoomId, MainTenantId, new DateOnly(2099, 10, 1), new DateOnly(2100, 9, 30),
                3_000_000m, 0m, TrangThaiHopDong.HieuLuc, "BR-04")));

        StringAssert.Contains(ex.Message, "hiệu lực");
        Assert.AreEqual(1L, await ScalarAsync(
            "SELECT COUNT(*) FROM hop_dong WHERE phong_id = @r AND trang_thai = 'HieuLuc'", ("@r", MainRoomId)));
    }

    /// <summary>BR-05: đại diện ký HĐ phải đang ở trong phòng đó.</summary>
    [TestMethod]
    public async Task Br05_RepresentativeNotInRoomRejected()
    {
        // LandlordTenantId ở LandlordRoomId — không thuộc MainRoomId.
        var service = new HopDongService(new HopDongRepository(Db));
        var ex = await Assert.ThrowsExceptionAsync<LoiNghiepVu>(
            () => service.CreateAsync(new HopDongDto(
                0, MainRoomId, LandlordTenantId, new DateOnly(2099, 9, 1), new DateOnly(2100, 8, 31),
                3_000_000m, 0m, TrangThaiHopDong.HieuLuc, "BR-05")));

        StringAssert.Contains(ex.Message, "đang ở trong phòng này");
    }

    /// <summary>BR-06: NgayKetThuc phải sau NgayBatDau.</summary>
    [TestMethod]
    public async Task Br06_EndDateNotAfterStartDateRejected()
    {
        var service = new HopDongService(new HopDongRepository(Db));
        var ex = await Assert.ThrowsExceptionAsync<LoiNghiepVu>(
            () => service.CreateAsync(new HopDongDto(
                0, MainRoomId, MainTenantId, new DateOnly(2099, 9, 1), new DateOnly(2099, 9, 1),
                3_000_000m, 0m, TrangThaiHopDong.HieuLuc, "BR-06")));

        StringAssert.Contains(ex.Message, "sau ngày bắt đầu");
        Assert.AreEqual(0L, await ScalarAsync(
            "SELECT COUNT(*) FROM hop_dong WHERE ghi_chu = 'BR-06'"), "HĐ sai ngày không được vào DB.");
    }

    /// <summary>BR-09: lập hóa đơn cần HĐ HieuLuc + chỉ số điện nước đã chốt của tháng.</summary>
    [TestMethod]
    public async Task Br09_InvoiceRequiresActiveContractAndUtilityReading()
    {
        var hopDongId = await SeedContractAsync();

        var service = new HoaDonService(new HoaDonRepository(Db));

        // Chưa chốt điện nước tháng 2099-10 → chặn.
        var noReading = await Assert.ThrowsExceptionAsync<LoiNghiepVu>(
            () => service.CreateAsync(new YeuCauTaoHoaDon(MainRoomId, OtherMonth, 0m)));
        StringAssert.Contains(noReading.Message, "điện nước");

        // Chốt điện nước xong → tạo hóa đơn được (đủ điều kiện).
        var utility = new DienNuocService(new DienNuocRepository(Db));
        await utility.RecordAsync(new ChiSoDienNuocDto(
            0, MainRoomId, OtherMonth, 100, 110, 3_500m, 20, 22, 10_000m));
        var created = await service.CreateAsync(new YeuCauTaoHoaDon(MainRoomId, OtherMonth, 0m));
        Assert.AreEqual(TrangThaiHoaDon.ChuaThu, created.TrangThai);
        Assert.AreEqual(3_055_000m, created.TongTien); // 3.000.000 + 10*3.500 + 2*10.000
        Assert.IsTrue(created.Id > 0);
        Assert.AreEqual(1L, await ScalarAsync(
            "SELECT COUNT(*) FROM hoa_don WHERE phong_id = @r AND ky_cuoc = @m",
            ("@r", MainRoomId), ("@m", OtherMonth)), "Hóa đơn đủ điều kiện phải vào DB.");
    }

    /// <summary>BR-11: hóa đơn DaThu không thanh toán lần hai; trang_thai/ngay_dong giữ nguyên.</summary>
    [TestMethod]
    public async Task Br11_PaidInvoiceCannotBePaidAgain()
    {
        var hoaDonId = await SeedPaidInvoiceAsync();
        var paidAtBefore = await ScalarAsync("SELECT ngay_dong FROM hoa_don WHERE id = @id", ("@id", hoaDonId));

        var service = new HoaDonService(new HoaDonRepository(Db));
        var ex = await Assert.ThrowsExceptionAsync<LoiNghiepVu>(() => service.PayAsync(hoaDonId));

        StringAssert.Contains(ex.Message, "đã thanh toán");
        Assert.AreEqual(1L, await ScalarAsync(
            "SELECT COUNT(*) FROM hoa_don WHERE id = @id AND trang_thai = 'DaThu'",
            ("@id", hoaDonId)),
            "Trạng thái phải giữ nguyên là DaThu.");
        Assert.AreEqual(paidAtBefore,
            await ScalarAsync("SELECT ngay_dong FROM hoa_don WHERE id = @id", ("@id", hoaDonId)),
            "ngay_dong không được đổi.");
    }

    // --------------------------------------------------------------- helpers

    private const string AdminUsername = "admin";
    private const string AdminPassword = "admin-pass";
    private const string MainTenantName = "T12 Người Chính";

    /// <summary>Router thật + session store dùng chung với XacThucService (token do auth tạo phải tra được).</summary>
    private static DieuPhoiYeuCau Router()
    {
        var sessions = new SessionStore();
        var auth = new XacThucService(new AdminUserRepository(), new AcceptanceTenantRepository(), sessions);
        return MakeRouter(auth, sessions);
    }

    private static DieuPhoiYeuCau MakeRouter(XacThucService auth, SessionStore sessions) => new(
        auth,
        new PhongService(new PhongRepository(Db)),
        new KhachThueService(new KhachThueRepository(Db)),
        new HopDongService(new HopDongRepository(Db)),
        new DienNuocService(new DienNuocRepository(Db)),
        new HoaDonService(new HoaDonRepository(Db)),
        new BaoCaoService(new BaoCaoRepository(Db)),
        new CuTruService(new CuTruRepository(Db)),
        sessions);

    private static KhachThueDto KhachThueDto(string cccd, string hoTen, int phongId) =>
        new(0, phongId, hoTen, new DateOnly(1999, 5, 10), cccd, "09000000" + cccd[^4..], "Đà Nẵng", null, false);

    private static string TenantPasswordOf(string cccd) => cccd[^6..];

    /// <summary>Đăng nhập admin qua router thật, trả token (dùng cho các hanh_dong của chủ trọ).</summary>
    private static async Task<string> LandlordTokenAsync(DieuPhoiYeuCau router)
    {
        var login = await router.HandleAsync(RequestPacket.Create(
            ActionNames.DangNhap, null, new YeuCauDangNhap(AdminUsername, AdminPassword)));
        Assert.IsTrue(login.Success, $"Đăng nhập admin phải thành công: {login.Message}");
        return login.GetData<KetQuaDangNhap>()!.Token;
    }

    /// <summary>Phòng nền + người thuê nền cho các BR test. Idempotent sau Cleanup.</summary>
    private static async Task SeedRoomsAsync()
    {
        await using var connection = await Db.OpenAsync();
        await using var giao_dich = await connection.BeginTransactionAsync();
        foreach (var (id, number) in new[]
        {
            (MainRoomId, MainRoomNumber), (SecondRoomId, SecondRoomNumber), (LandlordRoomId, LandlordRoomNumber),
        })
        {
            await using var command = new MySqlCommand(
                """
                INSERT INTO phong (id, so_phong, gia_thue, so_nguoi_toi_da, trang_thai, mo_ta)
                VALUES (@id, @number, 1000000, 4, 'Trong', 'AcceptanceTests T12')
                ON DUPLICATE KEY UPDATE so_phong = so_phong
                """, connection, giao_dich);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@number", number);
            await command.ExecuteNonQueryAsync();
        }

        foreach (var (id, phongId, name, cccd) in new[]
        {
            (MainTenantId, MainRoomId, MainTenantName, MainCccd),
            (ExtraTenantId, SecondRoomId, "T12 Người Phòng Khác", ExtraCccd),
            (LandlordTenantId, LandlordRoomId, "T12 Người Sai Phòng", LandlordCccd),
        })
        {
            await using var command = new MySqlCommand(
                """
                INSERT INTO khach_thue (id, phong_id, ho_ten, ngay_sinh, cccd, mat_khau_hash, so_dien_thoai, que_quan, noi_lam_viec, da_dang_ky_tam_tru)
                VALUES (@id, @phongId, @name, '1999-05-10', @cccd, @hash, @so_dien_thoai, 'Đà Nẵng', NULL, 1)
                ON DUPLICATE KEY UPDATE phong_id = @phongId
                """, connection, giao_dich);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@phongId", phongId);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@cccd", cccd);
            command.Parameters.AddWithValue("@hash", PasswordHasher.Hash(TenantPasswordOf(cccd)));
            command.Parameters.AddWithValue("@so_dien_thoai", $"090000{id:D4}");
            await command.ExecuteNonQueryAsync();
        }

        await giao_dich.CommitAsync();
    }

    private static async Task SeedSecondRoomWithInvoiceAsync()
    {
        await using var connection = await Db.OpenAsync();
        await using var giao_dich = await connection.BeginTransactionAsync();

        var mainContractId = await InsertContractAsync(
            connection, giao_dich, MainRoomId, MainTenantId, 2_000_000m, "AcceptanceTests T12 second");
        var otherContractId = await InsertContractAsync(
            connection, giao_dich, SecondRoomId, ExtraTenantId, 900_000m, "AcceptanceTests T12 other room");

        await InsertInvoiceAsync(connection, giao_dich, MainRoomId, mainContractId, 2_000_000m);
        await InsertInvoiceAsync(connection, giao_dich, SecondRoomId, otherContractId, 900_000m);

        await giao_dich.CommitAsync();
    }

    private static async Task<int> InsertContractAsync(
        MySqlConnection connection, MySqlTransaction giao_dich, int phongId, int khachThueId, decimal gia_thue, string ghi_chu)
    {
        await using var command = new MySqlCommand(
            """
            INSERT INTO hop_dong (phong_id, nguoi_dai_dien_id, ngay_bat_dau, ngay_ket_thuc, gia_thue, tien_coc, trang_thai, ghi_chu)
            VALUES (@phongId, @khachThueId, '2099-09-01', '2100-08-31', @gia_thue, 0, 'HieuLuc', @ghi_chu);
            SELECT LAST_INSERT_ID();
            """, connection, giao_dich);
        command.Parameters.AddWithValue("@phongId", phongId);
        command.Parameters.AddWithValue("@khachThueId", khachThueId);
        command.Parameters.AddWithValue("@gia_thue", gia_thue);
        command.Parameters.AddWithValue("@ghi_chu", ghi_chu);
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    private static async Task InsertInvoiceAsync(
        MySqlConnection connection, MySqlTransaction giao_dich, int phongId, int hopDongId, decimal total)
    {
        await using var command = new MySqlCommand(
            """
            INSERT INTO hoa_don (phong_id, hop_dong_id, ky_cuoc, tien_phong, tien_dien,
                                  tien_nuoc, phi_khac, tong_tien, trang_thai)
            VALUES (@phongId, @hopDongId, @month, @total, 0, 0, 0, @total, 'ChuaThu')
            """, connection, giao_dich);
        command.Parameters.AddWithValue("@phongId", phongId);
        command.Parameters.AddWithValue("@hopDongId", hopDongId);
        command.Parameters.AddWithValue("@month", KyCuoc);
        command.Parameters.AddWithValue("@total", total);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<int> SeedContractAsync()
    {
        await using var connection = await Db.OpenAsync();
        await using var command = new MySqlCommand(
            """
            INSERT INTO hop_dong (phong_id, nguoi_dai_dien_id, ngay_bat_dau, ngay_ket_thuc, gia_thue, tien_coc, trang_thai, ghi_chu)
            VALUES (@phongId, @khachThueId, '2099-09-01', '2100-08-31', 3000000, 0, 'HieuLuc', 'AcceptanceTests BR')
            """, connection);
        command.Parameters.AddWithValue("@phongId", MainRoomId);
        command.Parameters.AddWithValue("@khachThueId", MainTenantId);
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    private static async Task<int> SeedPaidInvoiceAsync()
    {
        await using var connection = await Db.OpenAsync();
        await using var giao_dich = await connection.BeginTransactionAsync();
        var hopDongId = await InsertContractAsync(
            connection, giao_dich, MainRoomId, MainTenantId, 3_000_000m, "AcceptanceTests BR");

        await using var command = new MySqlCommand(
            """
            INSERT INTO hoa_don (phong_id, hop_dong_id, ky_cuoc, tien_phong, tien_dien,
                                  tien_nuoc, phi_khac, tong_tien, trang_thai, ngay_dong)
            VALUES (@phongId, @hopDongId, @month, 3000000, 0, 0, 0, 3000000, 'DaThu', NOW())
            """, connection, giao_dich);
        command.Parameters.AddWithValue("@phongId", MainRoomId);
        command.Parameters.AddWithValue("@hopDongId", hopDongId);
        command.Parameters.AddWithValue("@month", KyCuoc);
        await command.ExecuteNonQueryAsync();

        await giao_dich.CommitAsync();

        await using var idConnection = await Db.OpenAsync();
        await using var idCommand = new MySqlCommand(
            "SELECT id FROM hoa_don WHERE phong_id = @r AND ky_cuoc = @m", idConnection);
        idCommand.Parameters.AddWithValue("@r", MainRoomId);
        idCommand.Parameters.AddWithValue("@m", KyCuoc);
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
        await using var giao_dich = await connection.BeginTransactionAsync();

        foreach (var sql in new[]
        {
            // Phòng luồng tháng do service tự cấp id (auto_increment) nên dọn theo tiền tố số phòng.
            "DELETE i FROM hoa_don i JOIN phong r ON r.id = i.phong_id WHERE r.so_phong LIKE 'T12-%' OR r.id BETWEEN 9204 AND 9215",
            "DELETE u FROM chi_so_dien_nuoc u JOIN phong r ON r.id = u.phong_id WHERE r.so_phong LIKE 'T12-%' OR r.id BETWEEN 9204 AND 9215",
            "DELETE c FROM hop_dong c JOIN phong r ON r.id = c.phong_id WHERE r.so_phong LIKE 'T12-%' OR r.id BETWEEN 9204 AND 9215",
            // Chỉ CCCD của Task 12 — KHÔNG dùng LIKE để khỏi quét trúng dải của test khác.
            $"DELETE FROM khach_thue WHERE cccd IN ('{MainCccd}', '{ExtraCccd}', '{LandlordCccd}', '{FlowCccd}')",
            "DELETE FROM phong WHERE so_phong LIKE 'T12-%' OR id BETWEEN 9204 AND 9215",
        })
        {
            await using var command = new MySqlCommand(sql, connection, giao_dich);
            await command.ExecuteNonQueryAsync();
        }

        await giao_dich.CommitAsync();
    }

    private sealed class FakeClock
    {
        private DateTimeOffset _now = new(2026, 9, 11, 8, 0, 0, TimeSpan.Zero);
        public DateTimeOffset Now() => _now;
        public void Advance(TimeSpan delta) => _now = _now.Add(delta);
    }

    /// <summary>Repo user admin — trả record admin cho đúng tenDangNhap, ngoài ra null.</summary>
    private sealed class AdminUserRepository : ITaiKhoanRepository
    {
        private readonly TaiKhoanRecord _admin = new(1, PasswordHasher.Hash(AdminPassword), "Chủ Trọ Demo");

        public Task<TaiKhoanRecord?> FindByUsernameAsync(string tenDangNhap, CancellationToken ct = default) =>
            Task.FromResult(string.Equals(tenDangNhap, AdminUsername, StringComparison.OrdinalIgnoreCase)
                ? _admin
                : null);
    }

    /// <summary>Repo tenant qua MySQL thật — router cần tra CCCD để đăng nhập tenant.</summary>
    private sealed class AcceptanceTenantRepository : IKhachThueRepository
    {
        public async Task<KhachThueAuthRecord?> FindByCccdAsync(string cccd, CancellationToken ct = default)
        {
            await using var connection = await Db.OpenAsync(ct);
            await using var command = new MySqlCommand(
                "SELECT id, cccd, mat_khau_hash, ho_ten, phong_id FROM khach_thue WHERE cccd = @c",
                connection);
            command.Parameters.AddWithValue("@c", cccd);

            await using var reader = await command.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
            {
                return null;
            }

            return new KhachThueAuthRecord(
                reader.GetInt32("id"),
                reader.GetString("cccd"),
                reader.IsDBNull(reader.GetOrdinal("mat_khau_hash")) ? string.Empty : reader.GetString("mat_khau_hash"),
                reader.GetString("ho_ten"),
                reader.IsDBNull(reader.GetOrdinal("phong_id")) ? null : reader.GetInt32("phong_id"));
        }
    }
}
