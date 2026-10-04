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
/// Task 7: Kiểm thử tích hợp toàn diện (E2E) trên MySQL thật (Laragon 127.0.0.1:3306).
/// Dải dữ liệu riêng id 9220..9222, CCCD 999999999220.., phòng T-QR-9220.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class QrCheckinEndToEndTests
{
    private const string ConnectionString =
        "Server=127.0.0.1;Port=3306;Database=quanly_phongtro_nhs;User Id=root;Password=;SslMode=None;";

    private const int RoomId = 9220;
    private const string RoomNumber = "T-QR-9220";
    private const string TenantACccd = "999999999220";
    private const string TenantBCccd = "999999999221";
    private const string TenantPassword = "matkhau123";

    private static readonly Database Db = new(ConnectionString);

    [TestInitialize]
    public async Task SetUpAsync()
    {
        await CleanupAsync();
        await SeedRoomAsync();
    }

    [TestCleanup]
    public async Task TearDownAsync() => await CleanupAsync();

    [TestMethod]
    public async Task EndToEnd_SelfRegister_CreatePendingContract_CheckinByPin_EnforcesSingleUseAndIdentity()
    {
        var router = MakeRouter();

        // 1. Khách A tự đăng ký (DANG_KY) — phong_id = NULL
        var regAReq = RequestPacket.Create(ActionNames.DangKy, null, new
        {
            HoTen = "Khách A Tự Đăng Ký",
            NgaySinh = "2000-01-01",
            Cccd = TenantACccd,
            SoDienThoai = "0905220220",
            QueQuan = "Quảng Nam",
            NoiLamViec = (string?)null,
            matKhau = TenantPassword,
        });
        var regARes = await router.HandleAsync(regAReq);
        Assert.IsTrue(regARes.Success, $"Đăng ký Khách A thất bại: {regARes.Message}");
        var tenantA = regARes.GetData<KhachThueDto>()!;
        Assert.IsNull(tenantA.PhongId, "Khách vừa tự đăng ký phải có phongId = null");

        // 2. Khách B tự đăng ký (dùng để kiểm thử Review Focus #1: người khác quét mã)
        var regBReq = RequestPacket.Create(ActionNames.DangKy, null, new
        {
            HoTen = "Khách B Tự Đăng Ký",
            NgaySinh = "1998-05-20",
            Cccd = TenantBCccd,
            SoDienThoai = "0905221221",
            QueQuan = "Đà Nẵng",
            NoiLamViec = (string?)null,
            matKhau = TenantPassword,
        });
        var regBRes = await router.HandleAsync(regBReq);
        Assert.IsTrue(regBRes.Success, $"Đăng ký Khách B thất bại: {regBRes.Message}");

        // 3. Đăng nhập 3 tác nhân: Chủ trọ, Khách A, Khách B
        var landlordToken = await LoginAsync(router, "chutro", "chutro");
        var tenantAToken = await LoginAsync(router, TenantACccd, TenantPassword);
        var tenantBToken = await LoginAsync(router, TenantBCccd, TenantPassword);

        // 4. Chủ trọ lập hợp đồng ChoNhanPhong cho phòng 9220 với người đại diện là Khách A
        var contractReq = RequestPacket.Create(ActionNames.HopDongTao, landlordToken, new HopDongDto(
            Id: 0,
            PhongId: RoomId,
            NguoiDaiDienId: tenantA.Id,
            NgayBatDau: new DateOnly(2026, 10, 1),
            NgayKetThuc: new DateOnly(2027, 10, 1),
            GiaThue: 2_500_000m,
            TienCoc: 1_000_000m,
            TrangThai: TrangThaiHopDong.ChoNhanPhong,
            GhiChu: "Hợp đồng chờ nhận phòng E2E"
        ));
        var contractRes = await router.HandleAsync(contractReq);
        Assert.IsTrue(contractRes.Success, $"Tạo HĐ ChoNhanPhong thất bại: {contractRes.Message}");
        var createdContract = contractRes.GetData<HopDongDto>()!;
        Assert.AreEqual(TrangThaiHopDong.ChoNhanPhong, createdContract.TrangThai);
        Assert.IsNotNull(createdContract.MaQrToken);
        Assert.IsNotNull(createdContract.MaPin);

        // 5. Chủ trọ lấy dữ liệu sinh QR (HOP_DONG_SINH_QR)
        var qrReq = RequestPacket.Create(ActionNames.HopDongSinhQr, landlordToken, new
        {
            hopDongId = createdContract.Id
        });
        var qrRes = await router.HandleAsync(qrReq);
        Assert.IsTrue(qrRes.Success, $"Lấy mã QR thất bại: {qrRes.Message}");
        var qrInfo = qrRes.GetData<ThongTinSinhQrDto>()!;
        Assert.AreEqual(createdContract.MaPin, qrInfo.MaPin);
        Assert.IsTrue(qrInfo.QrDataString.StartsWith("QUANLYTRO:NHANPHONG:"));

        // 6. Review Focus #1: Khách B (không phải đại diện) quét mã PIN của Khách A → BỊ TỪ CHỐI
        var checkinBReq = RequestPacket.Create(ActionNames.KhachThueNhanPhongQr, tenantBToken, new
        {
            tokenOrPin = qrInfo.MaPin
        });
        var checkinBRes = await router.HandleAsync(checkinBReq);
        Assert.IsFalse(checkinBRes.Success, "Khách B không được phép nhận phòng của Khách A");
        Assert.AreEqual("Mã nhận phòng này được cấp cho người đại diện khác.", checkinBRes.Message);

        // 7. Khách A quét mã PIN → THÀNH CÔNG
        var checkinAReq = RequestPacket.Create(ActionNames.KhachThueNhanPhongQr, tenantAToken, new
        {
            tokenOrPin = qrInfo.MaPin
        });
        var checkinARes = await router.HandleAsync(checkinAReq);
        Assert.IsTrue(checkinARes.Success, $"Khách A nhận phòng thất bại: {checkinARes.Message}");
        var checkinResult = checkinARes.GetData<KetQuaNhanPhongDto>()!;
        Assert.AreEqual(RoomNumber, checkinResult.SoPhong);

        // 8. Kiểm tra trạng thái DB sau nhận phòng:
        //    - hop_dong chuyển HieuLuc, ma_qr_token = NULL, ma_pin = NULL
        //    - khach_thue gán phong_id = RoomId
        //    - phong chuyển sang DaThue
        await using (var conn = await Db.OpenAsync())
        {
            await using var cmdHd = new MySqlCommand(
                "SELECT trang_thai, ma_qr_token, ma_pin FROM hop_dong WHERE id = @id", conn);
            cmdHd.Parameters.AddWithValue("@id", createdContract.Id);
            await using var rHd = await cmdHd.ExecuteReaderAsync();
            Assert.IsTrue(await rHd.ReadAsync());
            Assert.AreEqual("HieuLuc", rHd.GetString("trang_thai"));
            Assert.IsTrue(rHd.IsDBNull(rHd.GetOrdinal("ma_qr_token")));
            Assert.IsTrue(rHd.IsDBNull(rHd.GetOrdinal("ma_pin")));
        }

        await using (var conn = await Db.OpenAsync())
        {
            await using var cmdKt = new MySqlCommand(
                "SELECT phong_id FROM khach_thue WHERE id = @id", conn);
            cmdKt.Parameters.AddWithValue("@id", tenantA.Id);
            var phongId = Convert.ToInt32(await cmdKt.ExecuteScalarAsync());
            Assert.AreEqual(RoomId, phongId, "Khách A phải được gán vào phòng 9220 sau checkin");
        }

        await using (var conn = await Db.OpenAsync())
        {
            await using var cmdP = new MySqlCommand(
                "SELECT trang_thai FROM phong WHERE id = @id", conn);
            cmdP.Parameters.AddWithValue("@id", RoomId);
            var trangThaiPhong = (string)(await cmdP.ExecuteScalarAsync())!;
            Assert.AreEqual("DaThue", trangThaiPhong, "Phòng phải chuyển sang DaThue");
        }

        // 9. Review Focus #2: Khách A quét lại lần 2 (mã đã hủy) → BỊ TỪ CHỐI
        var replayReq = RequestPacket.Create(ActionNames.KhachThueNhanPhongQr, tenantAToken, new
        {
            tokenOrPin = qrInfo.MaPin
        });
        var replayRes = await router.HandleAsync(replayReq);
        Assert.IsFalse(replayRes.Success, "Quét lại mã đã dùng phải bị từ chối");
        Assert.AreEqual("Mã nhận phòng không hợp lệ hoặc đã được sử dụng.", replayRes.Message);
    }

    private static async Task<string> LoginAsync(DieuPhoiYeuCau router, string username, string password)
    {
        var res = await router.HandleAsync(RequestPacket.Create(
            ActionNames.DangNhap, null, new YeuCauDangNhap(username, password)));
        Assert.IsTrue(res.Success, $"Login {username} failed: {res.Message}");
        return res.GetData<KetQuaDangNhap>()!.Token;
    }

    private static DieuPhoiYeuCau MakeRouter()
    {
        var sessions = new SessionStore();
        var auth = new XacThucService(
            new TaiKhoanRepository(Db),
            new KhachThueAuthRepository(Db),
            sessions);

        return new DieuPhoiYeuCau(
            auth,
            new PhongService(new PhongRepository(Db)),
            new KhachThueService(new KhachThueRepository(Db)),
            new HopDongService(new HopDongRepository(Db)),
            new DienNuocService(new DienNuocRepository(Db)),
            new HoaDonService(new HoaDonRepository(Db)),
            new BaoCaoService(new BaoCaoRepository(Db)),
            new CuTruService(new CuTruRepository(Db)),
            sessions);
    }

    private static async Task SeedRoomAsync()
    {
        await using var conn = await Db.OpenAsync();
        const string sql = """
            INSERT INTO phong (id, so_phong, gia_thue, so_nguoi_toi_da, trang_thai, mo_ta)
            VALUES (@id, @number, 2500000, 2, 'Trong', 'Phòng test E2E QR')
            ON DUPLICATE KEY UPDATE trang_thai = 'Trong', so_phong = @number;
            """;
        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", RoomId);
        cmd.Parameters.AddWithValue("@number", RoomNumber);
        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task CleanupAsync()
    {
        try
        {
            await using var conn = await Db.OpenAsync();
            // Xóa HĐ của phòng test
            await using (var cmd = new MySqlCommand("DELETE FROM hop_dong WHERE phong_id = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", RoomId);
                await cmd.ExecuteNonQueryAsync();
            }

            // Xóa khách test
            await using (var cmd = new MySqlCommand("DELETE FROM khach_thue WHERE cccd IN (@a, @b)", conn))
            {
                cmd.Parameters.AddWithValue("@a", TenantACccd);
                cmd.Parameters.AddWithValue("@b", TenantBCccd);
                await cmd.ExecuteNonQueryAsync();
            }

            // Xóa phòng test
            await using (var cmd = new MySqlCommand("DELETE FROM phong WHERE id = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", RoomId);
                await cmd.ExecuteNonQueryAsync();
            }
        }
        catch
        {
            // Bỏ qua nếu DB chưa mở ở đầu
        }
    }
}
