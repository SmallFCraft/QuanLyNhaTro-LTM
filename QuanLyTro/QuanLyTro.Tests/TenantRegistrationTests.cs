using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Server.Services;
using QuanLyTro.Shared.Models;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class TenantRegistrationTests
{
    [TestMethod]
    public void DangKyDto_Validation_RejectsMissingPassword()
    {
        var tenant = new KhachThueDto(
            Id: 0,
            PhongId: null,
            HoTen: "Tran Van Test",
            NgaySinh: new DateOnly(2000, 1, 1),
            Cccd: "048200112233",
            SoDienThoai: "0905111222",
            QueQuan: "Da Nang",
            NoiLamViec: "Cong ty ABC",
            DaDangKyTamTru: false);

        var ex = Assert.ThrowsException<LoiNghiepVu>(() =>
            KhachThueService.ValidateRegistration(tenant, plainPassword: ""));
        Assert.AreEqual("Mật khẩu không được để trống.", ex.Message);
    }

    [TestMethod]
    public void DangKyDto_Validation_RejectsShortPassword()
    {
        var tenant = new KhachThueDto(
            Id: 0,
            PhongId: null,
            HoTen: "Tran Van Test",
            NgaySinh: new DateOnly(2000, 1, 1),
            Cccd: "048200112233",
            SoDienThoai: "0905111222",
            QueQuan: "Da Nang",
            NoiLamViec: null,
            DaDangKyTamTru: false);

        var ex = Assert.ThrowsException<LoiNghiepVu>(() =>
            KhachThueService.ValidateRegistration(tenant, plainPassword: "123"));
        Assert.AreEqual("Mật khẩu phải từ 6 ký tự trở lên.", ex.Message);
    }
}
