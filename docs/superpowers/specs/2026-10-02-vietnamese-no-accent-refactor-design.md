# Thiết kế Tái cấu trúc Toàn diện Tiếng Việt Không Dấu (End-to-End Refactor)

**Ngày:** 2026-10-02  
**Mục tiêu:** Chuyển đổi toàn bộ tên thực thể tiếng Anh sang tiếng Việt không dấu trên toàn hệ thống: CSDL, Giao thức TCP, Mã nguồn C#, Giao diện Web, và Tài liệu chỉ dẫn.

---

## 1. Quy ước Phân vai (Roles)
- `Landlord` → `ChuTro`
- `Manager` → `QuanLy`
- `Tenant` → `KhachThue`
- `Police` → `CongAn`

---

## 2. Cơ sở dữ liệu (MySQL 8.0, snake_case)

### Bảng `tai_khoan` (cũ `users`)
- `id` INT AUTO_INCREMENT PRIMARY KEY
- `ten_dang_nhap` VARCHAR(50) NOT NULL UNIQUE
- `mat_khau_hash` VARCHAR(255) NOT NULL
- `ho_ten` VARCHAR(100) NOT NULL
- `vai_tro` ENUM('ChuTro', 'QuanLy', 'CongAn', 'KhachThue') NOT NULL DEFAULT 'ChuTro'
- `ngay_tao` DATETIME DEFAULT CURRENT_TIMESTAMP

### Bảng `quyen_vai_tro` (cũ `role_permissions`)
- `vai_tro` VARCHAR(20) NOT NULL
- `hanh_dong` VARCHAR(50) NOT NULL
- `ngay_tao` DATETIME DEFAULT CURRENT_TIMESTAMP
- PRIMARY KEY (`vai_tro`, `hanh_dong`)

### Bảng `phong` (cũ `rooms`)
- `id` INT AUTO_INCREMENT PRIMARY KEY
- `so_phong` VARCHAR(20) NOT NULL UNIQUE
- `gia_thue` DECIMAL(12, 2) NOT NULL
- `so_nguoi_toi_da` INT NOT NULL DEFAULT 2
- `trang_thai` ENUM('Trong', 'DaThue', 'BaoTri') DEFAULT 'Trong'
- `mo_ta` VARCHAR(255) NULL
- `ngay_tao` DATETIME DEFAULT CURRENT_TIMESTAMP

### Bảng `khach_thue` (cũ `tenants`)
- `id` INT AUTO_INCREMENT PRIMARY KEY
- `phong_id` INT NULL
- `ho_ten` VARCHAR(100) NOT NULL
- `ngay_sinh` DATE NOT NULL
- `cccd` VARCHAR(20) NOT NULL UNIQUE
- `mat_khau_hash` VARCHAR(255) NULL
- `so_dien_thoai` VARCHAR(20) NOT NULL
- `que_quan` VARCHAR(150) NOT NULL
- `noi_lam_viec` VARCHAR(150) NULL
- `da_dang_ky_tam_tru` BOOLEAN DEFAULT FALSE
- `ngay_tao` DATETIME DEFAULT CURRENT_TIMESTAMP
- FOREIGN KEY (`phong_id`) REFERENCES `phong`(`id`)

### Bảng `hop_dong` (cũ `contracts`)
- `id` INT AUTO_INCREMENT PRIMARY KEY
- `phong_id` INT NOT NULL
- `nguoi_dai_dien_id` INT NOT NULL
- `ngay_bat_dau` DATE NOT NULL
- `ngay_ket_thuc` DATE NOT NULL
- `gia_thue` DECIMAL(12, 2) NOT NULL
- `tien_coc` DECIMAL(12, 2) NOT NULL DEFAULT 0
- `trang_thai` ENUM('HieuLuc', 'HetHan', 'ChamDut') DEFAULT 'HieuLuc'
- `ghi_chu` TEXT NULL
- `ngay_tao` DATETIME DEFAULT CURRENT_TIMESTAMP
- FOREIGN KEY (`phong_id`) REFERENCES `phong`(`id`)
- FOREIGN KEY (`nguoi_dai_dien_id`) REFERENCES `khach_thue`(`id`)

### Bảng `chi_so_dien_nuoc` (cũ `utility_readings`)
- `id` INT AUTO_INCREMENT PRIMARY KEY
- `phong_id` INT NOT NULL
- `thang_ghi_so` VARCHAR(7) NOT NULL
- `dien_cu` INT NOT NULL
- `dien_moi` INT NOT NULL
- `gia_dien` DECIMAL(10, 2) NOT NULL DEFAULT 3500
- `nuoc_cu` INT NOT NULL
- `nuoc_moi` INT NOT NULL
- `gia_nuoc` DECIMAL(10, 2) NOT NULL DEFAULT 10000
- `ngay_ghi` DATETIME DEFAULT CURRENT_TIMESTAMP
- FOREIGN KEY (`phong_id`) REFERENCES `phong`(`id`)
- UNIQUE KEY `uq_phong_thang` (`phong_id`, `thang_ghi_so`)

### Bảng `hoa_don` (cũ `invoices`)
- `id` INT AUTO_INCREMENT PRIMARY KEY
- `phong_id` INT NOT NULL
- `hop_dong_id` INT NOT NULL
- `ky_cuoc` VARCHAR(7) NOT NULL
- `tien_phong` DECIMAL(12, 2) NOT NULL
- `tien_dien` DECIMAL(12, 2) NOT NULL
- `tien_nuoc` DECIMAL(12, 2) NOT NULL
- `phi_khac` DECIMAL(12, 2) NOT NULL DEFAULT 0
- `tong_tien` DECIMAL(12, 2) NOT NULL
- `trang_thai` ENUM('ChuaThu', 'DaThu') DEFAULT 'ChuaThu'
- `ngay_dong` DATETIME NULL
- `ngay_tao` DATETIME DEFAULT CURRENT_TIMESTAMP
- FOREIGN KEY (`phong_id`) REFERENCES `phong`(`id`)
- FOREIGN KEY (`hop_dong_id`) REFERENCES `hop_dong`(`id`)
- UNIQUE KEY `uq_hoa_don_phong_ky` (`phong_id`, `ky_cuoc`)

---

## 3. Giao thức TCP (ActionNames, SCREAMING_SNAKE_CASE)

- Đăng nhập: `DANG_NHAP` (cũ `AUTH_LOGIN`)
- Phòng:
  - `PHONG_LAY_TAT_CA` (`ROOM_GET_ALL`)
  - `PHONG_THEM` (`ROOM_ADD`)
  - `PHONG_CAP_NHAT` (`ROOM_UPDATE`)
  - `PHONG_XOA` (`ROOM_DELETE`)
- Khách thuê:
  - `KHACH_THUE_THEO_PHONG` (`TENANT_GET_BY_ROOM`)
  - `KHACH_THUE_THEM` (`TENANT_ADD`)
  - `KHACH_THUE_CAP_NHAT` (`TENANT_UPDATE`)
  - `KHACH_THUE_TRA_PHONG` (`TENANT_CHECKOUT`)
  - `KHACH_THUE_XOA` (`TENANT_DELETE`)
- Hợp đồng:
  - `HOP_DONG_TAO` (`CONTRACT_CREATE`)
  - `HOP_DONG_GIA_HAN` (`CONTRACT_RENEW`)
  - `HOP_DONG_CHAM_DUT` (`CONTRACT_TERMINATE`)
  - `HOP_DONG_LAY_TAT_CA` (`CONTRACT_GET_ALL`)
- Điện nước:
  - `DIEN_NUOC_LAY_KY_TRUOC` (`UTILITY_GET_PREVIOUS`)
  - `DIEN_NUOC_GHI_SO` (`UTILITY_RECORD`)
- Hóa đơn:
  - `HOA_DON_TAO` (`INVOICE_CREATE`)
  - `HOA_DON_LAY_TAT_CA` (`INVOICE_GET_ALL`)
  - `HOA_DON_THANH_TOAN` (`INVOICE_PAY`)
  - `HOA_DON_CUA_TOI` (`INVOICE_GET_MINE`)
- Báo cáo & Lưu trú:
  - `BAO_CAO_TONG_QUAN` (`REPORT_SUMMARY`)
  - `XUAT_HO_SO_TAM_TRU` (`EXPORT_RESIDENCE`)
  - `LICH_SU_CU_TRU_LAY` (`RESIDENCE_HISTORY_GET`)
  - `XUAT_LICH_SU_CU_TRU` (`EXPORT_RESIDENCE_HISTORY`)
- Quản trị phân quyền:
  - `PHAN_QUYEN_LAY_MA_TRAN` (`PERMISSION_GET_MATRIX`)
  - `PHAN_QUYEN_CAP_NHAT_VAI_TRO` (`PERMISSION_UPDATE_ROLE`)

---

## 4. Kiến trúc Mã nguồn C#

### Enums
- `VaiTroNguoiDung`: `ChuTro`, `QuanLy`, `CongAn`, `KhachThue`
- `TrangThaiPhong`: `Trong`, `DaThue`, `BaoTri`
- `TrangThaiHopDong`: `HieuLuc`, `HetHan`, `ChamDut`
- `TrangThaiHoaDon`: `ChuaThu`, `DaThu`

### DTOs (QuanLyTro.Shared/Models)
- `PhongDto(int Id, string SoPhong, decimal GiaThue, int SoNguoiToiDa, TrangThaiPhong TrangThai, string? MoTa, int SoNguoiHienTai = 0)`
- `KhachThueDto(int Id, int? PhongId, string HoTen, DateOnly NgaySinh, string Cccd, string SoDienThoai, string QueQuan, string? NoiLamViec, bool DaDangKyTamTru, string? SoPhong = null)`
- `HopDongDto(int Id, int PhongId, int NguoiDaiDienId, DateOnly NgayBatDau, DateOnly NgayKetThuc, decimal GiaThue, decimal TienCoc, TrangThaiHopDong TrangThai, string? GhiChu, string? TenNguoiDaiDien = null, string? SoPhong = null)`
- `ChiSoDienNuocDto(int Id, int PhongId, string ThangGhiSo, int DienCu, int DienMoi, decimal GiaDien, int NuocCu, int NuocMoi, decimal GiaNuoc, DateTime NgayGhi, string? SoPhong = null)`
- `HoaDonDto(int Id, int PhongId, int HopDongId, string KyCuoc, decimal TienPhong, decimal TienDien, decimal TienNuoc, decimal PhiKhac, decimal TongTien, TrangThaiHoaDon TrangThai, DateTime? NgayDong, string? SoPhong = null)`
- `TrangHoaDonCuaToiDto(List<HoaDonDto> DanhSach, int TongSo, int Trang, int SoLuongMoiTrang)`
- `YeuCauDangNhap(string TenDangNhap, string MatKhau)`
- `KetQuaDangNhap(string Token, string HoTen, VaiTroNguoiDung VaiTro)`
- `BaoCaoTongQuanDto(int TongSoPhong, int PhongTrong, int PhongDaThue, int KhachHienTai, decimal SoTienDaThu, decimal SoTienChuaThu)`
- `MucQuyenVaiTroDto(string HanhDong, string MoTa, string Nhom)`
- `MaTranQuyenVaiTroDto(Dictionary<string, List<string>> QuyenTheoVaiTro, List<MucQuyenVaiTroDto> DanhSachHanhDong)`

### Repositories & Services
- Đổi tên toàn bộ interface và class tương ứng sang tiếng Việt không dấu: `ITaiKhoanRepository`, `IPhongRepository`, `IKhachThueRepository`, `IHopDongRepository`, `IDienNuocRepository`, `IHoaDonRepository`, `ICuTruRepository`, `IPhanQuyenRepository`.
- `MaTranPhanQuyen` (cũ `PermissionMatrix`): kiểm tra bypass cứng cho `ChuTro`, phân quyền động cho `QuanLy`, `CongAn`, `KhachThue`.
- `DieuPhoiYeuCau` (cũ `RequestRouter`): điều hướng theo `ActionNames` mới.

---

## 5. Cấu trúc Giao diện Web (Assets/wwwroot/)

- `Assets/wwwroot/chutro/` (cũ `landlord/`): Shell Chủ trọ và Quản lý.
- `Assets/wwwroot/congan/` (cũ `police/`): Shell Công an phường tra cứu lưu trú.
- `Assets/wwwroot/khachthue/` (cũ `tenant/`): Shell Khách thuê xem hóa đơn.
- `Assets/wwwroot/auth/`: Màn đăng nhập thống nhất, chuyển hướng tương ứng sang `/chutro/`, `/congan/`, `/khachthue/`.
- Cập nhật `Form1.cs` để tải đúng đường dẫn trang web.

---

## 6. Hướng dẫn & Tài liệu
- Cập nhật `CLAUDE.md`, `AGENTS.md`, memory để phản ánh chính xác các vai trò `ChuTro`, `QuanLy`, `CongAn`, `KhachThue`, các bảng CSDL và quy tắc vận hành mới.
