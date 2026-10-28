using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Server.Repositories;
using QuanLyTro.Server.Services;
using QuanLyTro.Shared.Models;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class CauHinhThanhToanTests
{
    private static string Wwwroot =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "wwwroot");

    [TestMethod]
    public async Task Update_ChuanHoaChuTaiKhoan_BoDauVaInHoa()
    {
        var service = new CauHinhService(new NullCauHinhRepository());

        var saved = await service.UpdateAsync(new CauHinhThanhToanDto("MBBank", "0387368890", " Nguyễn  Văn Đức ", "compact"));

        Assert.AreEqual("NGUYEN VAN DUC", saved.ChuTaiKhoan, "Chủ tài khoản phải bỏ dấu, in hoa, gộp khoảng trắng.");
        Assert.IsTrue(saved.DaCauHinh);
    }

    [TestMethod]
    public async Task Update_TuChoiSoTaiKhoanSaiDinhDang()
    {
        var service = new CauHinhService(new NullCauHinhRepository());

        // SePay: `acc` chỉ chữ/số, tối đa 19 ký tự.
        await Assert.ThrowsExceptionAsync<LoiNghiepVu>(() =>
            service.UpdateAsync(new CauHinhThanhToanDto("MBBank", "0387-368-890", "", "compact")));
        await Assert.ThrowsExceptionAsync<LoiNghiepVu>(() =>
            service.UpdateAsync(new CauHinhThanhToanDto("MBBank", new string('1', 20), "", "compact")));
    }

    [TestMethod]
    public async Task Update_TuChoiTemplateLa()
    {
        var service = new CauHinhService(new NullCauHinhRepository());

        await Assert.ThrowsExceptionAsync<LoiNghiepVu>(() =>
            service.UpdateAsync(new CauHinhThanhToanDto("MBBank", "0387368890", "", "khong-ton-tai")));
    }

    [TestMethod]
    public async Task Update_CoSoTaiKhoanThiPhaiCoNganHang()
    {
        var service = new CauHinhService(new NullCauHinhRepository());

        await Assert.ThrowsExceptionAsync<LoiNghiepVu>(() =>
            service.UpdateAsync(new CauHinhThanhToanDto("", "0387368890", "", "compact")));
    }

    [TestMethod]
    public void ChuanHoaChuTaiKhoan_CatVeToiDa80KyTu()
    {
        var dai = new string('A', 120);

        var ket_qua = CauHinhService.ChuanHoaChuTaiKhoan(dai);

        Assert.AreEqual(CauHinhService.ChuTaiKhoanMaxLength, ket_qua.Length);
    }

    [TestMethod]
    public void LandlordSettings_HasTabFormAndPreview()
    {
        var html = File.ReadAllText(Path.Combine(Wwwroot, "chutro", "index.html"));
        Assert.IsTrue(html.Contains("data-tab=\"cai_dat\""), "chutro thiếu tab Cài đặt");
        Assert.IsTrue(html.Contains("id=\"cfg-ngan_hang\""), "thiếu ô chọn ngân hàng");
        Assert.IsTrue(html.Contains("id=\"cfg-so_tai_khoan\""), "thiếu ô số tài khoản");
        Assert.IsTrue(html.Contains("id=\"cfg-qr-preview\""), "thiếu khung xem trước QR");
        Assert.IsTrue(html.Contains("js/cai_dat.js"), "index.html chưa nạp js/cai_dat.js");

        var js = File.ReadAllText(Path.Combine(Wwwroot, "chutro", "js", "cai_dat.js"));
        Assert.IsTrue(js.Contains("CAU_HINH_LAY"), "cai_dat.js chưa đọc cấu hình");
        Assert.IsTrue(js.Contains("CAU_HINH_CAP_NHAT"), "cai_dat.js chưa lưu cấu hình");
    }

    [TestMethod]
    public void TenantPayment_UsesSepayQrAndNoHardcodedBankInfo()
    {
        var html = File.ReadAllText(Path.Combine(Wwwroot, "khachthue", "index.html"));
        Assert.IsTrue(html.Contains("id=\"pay-qr\""), "khachthue thiếu khối QR chuyển khoản");

        var js = File.ReadAllText(Path.Combine(Wwwroot, "khachthue", "js", "khach_thue.js"));
        Assert.IsTrue(js.Contains("CAU_HINH_LAY"), "khách thuê phải đọc cấu hình nhận tiền từ server");
        Assert.IsFalse(js.Contains("const BANK_INFO"), "BANK_INFO hardcode phải bị xóa");

        var ui = File.ReadAllText(Path.Combine(Wwwroot, "shared", "js", "ui.js"));
        Assert.IsTrue(ui.Contains("https://qr.sepay.vn/img"), "thiếu hàm dựng URL QR SePay dùng chung");
    }
}
