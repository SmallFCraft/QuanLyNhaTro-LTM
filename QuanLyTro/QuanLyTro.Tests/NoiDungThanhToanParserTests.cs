using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Server.Services;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class NoiDungThanhToanParserTests
{
    private static readonly string[] Phong = ["P101", "P102", "P201"];

    [TestMethod]
    [DataRow("P102 09-2026", "P102", "2026-09")]
    [DataRow("P102 09/2026", "P102", "2026-09")]
    [DataRow("P102 2026-09", "P102", "2026-09")]
    [DataRow("MBVCB.123.P102 09-2026.CT tu 0387368890", "P102", "2026-09")]
    [DataRow("p102 9-2026", "P102", "2026-09")]
    [DataRow("P102 092026", "P102", "2026-09")]
    [DataRow("P102-09-2026", "P102", "2026-09")]
    [DataRow("CK P102 09-2026 thanh toan tien phong", "P102", "2026-09")]
    [DataRow("P101 2026-10", "P101", "2026-10")]
    public void Doc_NhanDangPhongVaKy(string noiDung, string soPhong, string kyCuoc)
    {
        var ket_qua = NoiDungThanhToanParser.Doc(noiDung, Phong);

        Assert.IsNotNull(ket_qua, $"Phải đọc được: {noiDung}");
        Assert.AreEqual(soPhong, ket_qua.SoPhong);
        Assert.AreEqual(kyCuoc, ket_qua.KyCuoc);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("chuyen tien thang 9")]
    [DataRow("P102")]
    [DataRow("09-2026")]
    public void Doc_TraVeNullKhiThieuPhongHoacKy(string noiDung)
    {
        Assert.IsNull(NoiDungThanhToanParser.Doc(noiDung, Phong));
    }

    [TestMethod]
    public void Doc_PhongKhongCoTrongDanhSach_TraVeNull()
    {
        // P999 không nằm trong danh sách phòng đang nợ → không khớp.
        Assert.IsNull(NoiDungThanhToanParser.Doc("P999 09-2026", Phong));
    }

    [TestMethod]
    public void Doc_ChonPhongDaiNhatKhiTrungTienTo()
    {
        // Có cả P10 và P102: nội dung "P102" phải khớp P102, không phải P10.
        var ket_qua = NoiDungThanhToanParser.Doc("P102 09-2026", ["P10", "P102"]);

        Assert.IsNotNull(ket_qua);
        Assert.AreEqual("P102", ket_qua.SoPhong);
    }

    [TestMethod]
    public void Doc_BoDauTruocKhiKhop()
    {
        var ket_qua = NoiDungThanhToanParser.Doc("Chuyển tiền phòng P102 kỳ 09-2026", Phong);

        Assert.IsNotNull(ket_qua);
        Assert.AreEqual("P102", ket_qua.SoPhong);
    }

    [TestMethod]
    public void Doc_ChiHoaDonNhoiKhongNhamMaGiaoDich()
    {
        // "MBVCB.123456" có chuỗi số dài — không được coi là kỳ cước.
        var ket_qua = NoiDungThanhToanParser.Doc("MBVCB.123456.P102.CT tu 0387368890", Phong);

        Assert.IsNull(ket_qua, "Không có kỳ hợp lệ thì không khớp");
    }

    [TestMethod]
    public void ChuanHoaKy_HieuCaBonDinhDang()
    {
        Assert.AreEqual("2026-09", NoiDungThanhToanParser.ChuanHoaKy("09-2026"));
        Assert.AreEqual("2026-09", NoiDungThanhToanParser.ChuanHoaKy("2026-09"));
        Assert.AreEqual("2026-09", NoiDungThanhToanParser.ChuanHoaKy("092026"));
        Assert.AreEqual("2026-09", NoiDungThanhToanParser.ChuanHoaKy("202609"));
    }

    [TestMethod]
    public void ChuanHoaKy_ThangNgoaiPhamVi_TraVeNull()
    {
        Assert.IsNull(NoiDungThanhToanParser.ChuanHoaKy("13-2026"));
        Assert.IsNull(NoiDungThanhToanParser.ChuanHoaKy("00-2026"));
    }
}
