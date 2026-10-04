using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Shared.Models;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class QrCheckinModelTests
{
    [TestMethod]
    public void TrangThaiHopDong_ChuaNhanPhong_Exists()
    {
        // ChoNhanPhong phải là một giá trị hợp lệ trong TrangThaiHopDong
        Assert.IsTrue(Enum.IsDefined(typeof(TrangThaiHopDong), "ChoNhanPhong"));
    }

    [TestMethod]
    public void HopDongDto_CanHold_MaQrTokenAndMaPin()
    {
        var dto = new HopDongDto(
            Id: 1,
            PhongId: 101,
            NguoiDaiDienId: 5,
            NgayBatDau: new DateOnly(2026, 10, 1),
            NgayKetThuc: new DateOnly(2027, 10, 1),
            GiaThue: 2500000m,
            TienCoc: 2500000m,
            TrangThai: TrangThaiHopDong.ChoNhanPhong,
            GhiChu: "Hợp đồng chờ quét QR",
            TenNguoiDaiDien: "Nguyen Van A",
            SoPhong: "P101",
            MaQrToken: "token_1234567890abcdef1234567890abcdef",
            MaPin: "12345678");

        Assert.AreEqual(TrangThaiHopDong.ChoNhanPhong, dto.TrangThai);
        Assert.AreEqual("token_1234567890abcdef1234567890abcdef", dto.MaQrToken);
        Assert.AreEqual("12345678", dto.MaPin);
    }

    [TestMethod]
    public void ActionNames_All_ContainsNewActions()
    {
        CollectionAssert.Contains((System.Collections.ICollection)ActionNames.All, ActionNames.DangKy);
        CollectionAssert.Contains((System.Collections.ICollection)ActionNames.All, ActionNames.HopDongSinhQr);
        CollectionAssert.Contains((System.Collections.ICollection)ActionNames.All, ActionNames.KhachThueNhanPhongQr);
    }

    [TestMethod]
    public void QuyenMacDinhTheoVaiTro_ContainsNewActions()
    {
        CollectionAssert.Contains((System.Collections.ICollection)QuyenMacDinhTheoVaiTro.QuanLy, ActionNames.HopDongSinhQr);
        CollectionAssert.Contains((System.Collections.ICollection)QuyenMacDinhTheoVaiTro.KhachThue, ActionNames.KhachThueNhanPhongQr);

        var actionsInDanhMuc = QuyenMacDinhTheoVaiTro.DanhMuc.Select(d => d.HanhDong).ToList();
        CollectionAssert.Contains(actionsInDanhMuc, ActionNames.HopDongSinhQr);
        CollectionAssert.Contains(actionsInDanhMuc, ActionNames.KhachThueNhanPhongQr);
    }
}
