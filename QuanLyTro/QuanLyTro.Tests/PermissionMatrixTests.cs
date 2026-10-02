using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Server.Network;
using QuanLyTro.Shared.Models;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class PermissionMatrixTests
{
    [TestMethod]
    public void AllActions_AreRegisteredInMatrix()
    {
        foreach (var hanh_dong in ActionNames.All)
        {
            Assert.IsTrue(MaTranPhanQuyen.IsKnown(hanh_dong), $"Action chưa khai báo quyền: {hanh_dong}");
        }
    }

    [TestMethod]
    public void Tenant_HasAccessOnlyToAuthAndInvoiceGetMine()
    {
        Assert.IsTrue(MaTranPhanQuyen.IsAllowed(ActionNames.DangNhap, VaiTroNguoiDung.KhachThue));
        Assert.IsTrue(MaTranPhanQuyen.IsAllowed(ActionNames.HoaDonCuaToi, VaiTroNguoiDung.KhachThue));

        // Mọi hanh_dong ghi hoặc list khác đều bị từ chối với KhachThue (BR-14).
        Assert.IsFalse(MaTranPhanQuyen.IsAllowed(ActionNames.PhongLayTatCa, VaiTroNguoiDung.KhachThue));
        Assert.IsFalse(MaTranPhanQuyen.IsAllowed(ActionNames.PhongThem, VaiTroNguoiDung.KhachThue));
        Assert.IsFalse(MaTranPhanQuyen.IsAllowed(ActionNames.HoaDonLayTatCa, VaiTroNguoiDung.KhachThue));
        Assert.IsFalse(MaTranPhanQuyen.IsAllowed(ActionNames.HoaDonThanhToan, VaiTroNguoiDung.KhachThue));
        Assert.IsFalse(MaTranPhanQuyen.IsAllowed(ActionNames.HopDongTao, VaiTroNguoiDung.KhachThue));
        Assert.IsFalse(MaTranPhanQuyen.IsAllowed(ActionNames.KhachThueThem, VaiTroNguoiDung.KhachThue));
        Assert.IsFalse(MaTranPhanQuyen.IsAllowed(ActionNames.BaoCaoTongQuan, VaiTroNguoiDung.KhachThue));
        Assert.IsFalse(MaTranPhanQuyen.IsAllowed(ActionNames.XuatHoSoTamTru, VaiTroNguoiDung.KhachThue));
    }

    [TestMethod]
    public void Police_HasReadOnlyAccessToAllowedActions_AndRejectedOnAllWrites()
    {
        // Được phép đọc
        Assert.IsTrue(MaTranPhanQuyen.IsAllowed(ActionNames.DangNhap, VaiTroNguoiDung.CongAn));
        Assert.IsTrue(MaTranPhanQuyen.IsAllowed(ActionNames.PhongLayTatCa, VaiTroNguoiDung.CongAn));
        Assert.IsTrue(MaTranPhanQuyen.IsAllowed(ActionNames.KhachThueTheoPhong, VaiTroNguoiDung.CongAn));
        Assert.IsTrue(MaTranPhanQuyen.IsAllowed(ActionNames.XuatHoSoTamTru, VaiTroNguoiDung.CongAn));
        Assert.IsTrue(MaTranPhanQuyen.IsAllowed(ActionNames.LichSuCuTruLay, VaiTroNguoiDung.CongAn));
        Assert.IsTrue(MaTranPhanQuyen.IsAllowed(ActionNames.XuatLichSuCuTru, VaiTroNguoiDung.CongAn));

        // Bị cấm toàn bộ thao tác ghi (BR-16)
        Assert.IsFalse(MaTranPhanQuyen.IsAllowed(ActionNames.PhongThem, VaiTroNguoiDung.CongAn));
        Assert.IsFalse(MaTranPhanQuyen.IsAllowed(ActionNames.PhongCapNhat, VaiTroNguoiDung.CongAn));
        Assert.IsFalse(MaTranPhanQuyen.IsAllowed(ActionNames.PhongXoa, VaiTroNguoiDung.CongAn));
        Assert.IsFalse(MaTranPhanQuyen.IsAllowed(ActionNames.KhachThueThem, VaiTroNguoiDung.CongAn));
        Assert.IsFalse(MaTranPhanQuyen.IsAllowed(ActionNames.KhachThueCapNhat, VaiTroNguoiDung.CongAn));
        Assert.IsFalse(MaTranPhanQuyen.IsAllowed(ActionNames.KhachThueTraPhong, VaiTroNguoiDung.CongAn));
        Assert.IsFalse(MaTranPhanQuyen.IsAllowed(ActionNames.KhachThueXoa, VaiTroNguoiDung.CongAn));
        Assert.IsFalse(MaTranPhanQuyen.IsAllowed(ActionNames.HopDongTao, VaiTroNguoiDung.CongAn));
        Assert.IsFalse(MaTranPhanQuyen.IsAllowed(ActionNames.HopDongChamDut, VaiTroNguoiDung.CongAn));
        Assert.IsFalse(MaTranPhanQuyen.IsAllowed(ActionNames.DienNuocGhiSo, VaiTroNguoiDung.CongAn));
        Assert.IsFalse(MaTranPhanQuyen.IsAllowed(ActionNames.HoaDonTao, VaiTroNguoiDung.CongAn));
        Assert.IsFalse(MaTranPhanQuyen.IsAllowed(ActionNames.HoaDonThanhToan, VaiTroNguoiDung.CongAn));
        Assert.IsFalse(MaTranPhanQuyen.IsAllowed(ActionNames.HoaDonCuaToi, VaiTroNguoiDung.CongAn));
    }

    [TestMethod]
    public void Landlord_HasAccessToAllActionsExceptInvoiceGetMine()
    {
        foreach (var hanh_dong in ActionNames.All)
        {
            if (hanh_dong == ActionNames.HoaDonCuaToi)
            {
                Assert.IsFalse(MaTranPhanQuyen.IsAllowed(hanh_dong, VaiTroNguoiDung.ChuTro));
            }
            else
            {
                Assert.IsTrue(MaTranPhanQuyen.IsAllowed(hanh_dong, VaiTroNguoiDung.ChuTro), $"ChuTro thiếu quyền: {hanh_dong}");
            }
        }
    }

    [TestMethod]
    public void Manager_HasOperationalPermissions_ButNotPermissionAdmin()
    {
        Assert.IsTrue(MaTranPhanQuyen.IsAllowed(ActionNames.PhongLayTatCa, VaiTroNguoiDung.QuanLy));
        Assert.IsTrue(MaTranPhanQuyen.IsAllowed(ActionNames.PhongThem, VaiTroNguoiDung.QuanLy));
        Assert.IsTrue(MaTranPhanQuyen.IsAllowed(ActionNames.HoaDonThanhToan, VaiTroNguoiDung.QuanLy));
        Assert.IsTrue(MaTranPhanQuyen.IsAllowed(ActionNames.HopDongTao, VaiTroNguoiDung.QuanLy));

        // Quản lý KHÔNG được phép quản lý phân quyền (chỉ Chủ trọ tối cao mới được)
        Assert.IsFalse(MaTranPhanQuyen.IsAllowed(ActionNames.PhanQuyenLayMaTran, VaiTroNguoiDung.QuanLy));
        Assert.IsFalse(MaTranPhanQuyen.IsAllowed(ActionNames.PhanQuyenCapNhatVaiTro, VaiTroNguoiDung.QuanLy));

        // Quản lý không xem hóa đơn cá nhân của người thuê
        Assert.IsFalse(MaTranPhanQuyen.IsAllowed(ActionNames.HoaDonCuaToi, VaiTroNguoiDung.QuanLy));
    }

    [TestMethod]
    public void Landlord_HasAllPermissions_IncludingPermissionAdmin()
    {
        Assert.IsTrue(MaTranPhanQuyen.IsAllowed(ActionNames.PhanQuyenLayMaTran, VaiTroNguoiDung.ChuTro));
        Assert.IsTrue(MaTranPhanQuyen.IsAllowed(ActionNames.PhanQuyenCapNhatVaiTro, VaiTroNguoiDung.ChuTro));
    }

    [TestMethod]
    public void PermissionMatrix_SupportsDynamicReload_AllowsRevokingAndGranting()
    {
        try
        {
            // Ban đầu QuanLy được thêm phòng
            Assert.IsTrue(MaTranPhanQuyen.IsAllowed(ActionNames.PhongThem, VaiTroNguoiDung.QuanLy));

            // Thu hồi quyền PhongThem của QuanLy
            var updated = QuyenMacDinhTheoVaiTro.All.ToDictionary(
                kv => kv.Key,
                kv => kv.Key.Equals("QuanLy", StringComparison.OrdinalIgnoreCase)
                    ? kv.Value.Where(a => a != ActionNames.PhongThem).ToList()
                    : kv.Value.ToList(),
                StringComparer.OrdinalIgnoreCase);

            MaTranPhanQuyen.ApplyMatrix(updated);

            // Giờ QuanLy bị từ chối PhongThem
            Assert.IsFalse(MaTranPhanQuyen.IsAllowed(ActionNames.PhongThem, VaiTroNguoiDung.QuanLy));

            // Nhưng ChuTro vẫn luôn được phép (bypass cứng trong code)
            Assert.IsTrue(MaTranPhanQuyen.IsAllowed(ActionNames.PhongThem, VaiTroNguoiDung.ChuTro));
        }
        finally
        {
            // Khôi phục lại ma trận mặc định
            MaTranPhanQuyen.ResetToDefaults();
        }
    }
}
