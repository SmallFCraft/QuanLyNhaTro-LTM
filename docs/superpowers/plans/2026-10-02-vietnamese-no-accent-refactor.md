# Kế hoạch Thực thi Refactor Toàn diện Sang Tiếng Việt Không Dấu

> **Dành cho agent:** REQUIRED SUB-SKILL: Sử dụng `superpowers:executing-plans` (chạy trực tiếp trong phiên) để thực thi từng tác vụ theo thứ tự. Các bước dùng cú pháp checklist (`- [ ]`).

**Mục tiêu:** Chuyển đổi toàn diện toàn bộ định danh tiếng Anh sang tiếng Việt không dấu (CSDL MySQL, C# Enums/DTOs/Repositories/Services/Router, ActionNames TCP, UI Web wwwroot, Test suite, và tài liệu CLAUDE.md/AGENTS.md/Memory).

**Kiến trúc:** 
Tách biệt theo tầng từ dưới lên: 
1. `QuanLyTro.Shared` (Enums, Models DTO, ActionNames, QuyenMacDinhTheoVaiTro).
2. CSDL & Seeder (`database/schema.sql`, `SchemaInitializer.cs`, `DemoSeeder.cs`).
3. `QuanLyTro.Server` (Repositories, Services, MaTranPhanQuyen, DieuPhoiYeuCau).
4. `QuanLyTro` Client & Web Assets (`Form1.cs`, `WebMessageBridge.cs`, đổi tên thư mục wwwroot sang `chutro`, `congan`, `khachthue`, cập nhật JS).
5. `QuanLyTro.Tests` (Cập nhật toàn bộ 201 test case khớp với tên tiếng Việt mới).
6. Cập nhật `CLAUDE.md`, `AGENTS.md` và memory.

**Tech Stack:** .NET 8.0 (C# 12), WinForms, Microsoft.Web.WebView2, MySqlConnector, Dapper / ADO.NET, Vanilla JS / HTML5 / CSS3.

**Spec:** `docs/superpowers/specs/2026-10-02-vietnamese-no-accent-refactor-design.md`

## Global Constraints

- Không dùng dấu tiếng Việt trong định danh mã nguồn, CSDL, action name (dùng `ChuTro`, `quan_ly`, `PHONG_LAY_TAT_CA`).
- CSDL MySQL: snake_case không dấu (`tai_khoan`, `khach_thue`, `so_phong`, `tien_dien`...).
- C# Types / Properties: PascalCase tiếng Việt không dấu (`PhongDto`, `SoPhong`, `GiaThue`...).
- Giao thức TCP: SCREAMING_SNAKE_CASE tiếng Việt không dấu (`DANG_NHAP`, `PHONG_LAY_TAT_CA`, `HOA_DON_CUA_TOI`...).
- Thư mục giao diện Web: `Assets/wwwroot/chutro/`, `Assets/wwwroot/congan/`, `Assets/wwwroot/khachthue/`, `Assets/wwwroot/auth/`, `Assets/wwwroot/shared/`.
- 100% test case (201+ tests) phải pass sau khi refactor.

## Review Focus

1. Khớp chuỗi ENUM trong CSDL với C# Enum: MySQL `vai_tro` ('ChuTro', 'QuanLy', 'CongAn', 'KhachThue') phải map đúng `VaiTroNguoiDung`.
2. Kiểm tra router TCP `DieuPhoiYeuCau`: toàn bộ 26 action names phải khớp giữa Client JS, `ActionNames.cs` và Handler.
3. Không làm hỏng logic phân quyền: `ChuTro` luôn bypass cứng (trừ `HOA_DON_CUA_TOI`), `QuanLy`/`CongAn`/`KhachThue` tra cứu bảng `quyen_vai_tro`.
4. WebView2 assets: Đường dẫn file trong `Form1.cs` và điều hướng JS trong `auth.js` phải trỏ đúng `chutro/index.html`, `congan/index.html`, `khachthue/index.html`.
5. Đảm bảo bảo mật tài khoản: mật khẩu hash BCrypt, không lộ secret, token sinh an toàn.

---

### Tác vụ 1: Chuyển đổi `QuanLyTro.Shared` (Enums, Models, Protocol)

**Files:**
- Sửa/Đổi tên:
  - `QuanLyTro/QuanLyTro.Shared/Models/Enums.cs` (tạo mới hoặc sửa enum)
  - `QuanLyTro/QuanLyTro.Shared/Models/PhongDto.cs` (thay thế `RoomDto.cs`)
  - `QuanLyTro/QuanLyTro.Shared/Models/KhachThueDto.cs` (thay thế `TenantDto.cs`)
  - `QuanLyTro/QuanLyTro.Shared/Models/HopDongDto.cs` (thay thế `ContractDto.cs`)
  - `QuanLyTro/QuanLyTro.Shared/Models/ChiSoDienNuocDto.cs` (thay thế `UtilityReadingDto.cs`)
  - `QuanLyTro/QuanLyTro.Shared/Models/HoaDonDto.cs` (thay thế `InvoiceDto.cs`)
  - `QuanLyTro/QuanLyTro.Shared/Models/XacThucVaBaoCaoDtos.cs` (thay thế `AuthAndReportDtos.cs`)
  - `QuanLyTro/QuanLyTro.Shared/Models/CuTruDtos.cs` (thay thế `ResidenceDtos.cs`)
  - `QuanLyTro/QuanLyTro.Shared/Protocol/ActionNames.cs`
  - `QuanLyTro/QuanLyTro.Shared/Protocol/QuyenMacDinhTheoVaiTro.cs` (thay thế `DefaultRolePermissions.cs`)

**Interfaces:**
- Produces:
  - `enum VaiTroNguoiDung { ChuTro, QuanLy, CongAn, KhachThue }`
  - `enum TrangThaiPhong { Trong, DaThue, BaoTri }`
  - `enum TrangThaiHopDong { HieuLuc, HetHan, ChamDut }`
  - `enum TrangThaiHoaDon { ChuaThu, DaThu }`
  - Các DTOs: `PhongDto`, `KhachThueDto`, `HopDongDto`, `ChiSoDienNuocDto`, `HoaDonDto`, `TrangHoaDonCuaToiDto`, `YeuCauDangNhap`, `KetQuaDangNhap`, `BaoCaoTongQuanDto`, `MucQuyenVaiTroDto`, `MaTranQuyenVaiTroDto`
  - `ActionNames` với 26 action constants mới bằng tiếng Việt không dấu.
  - `QuyenMacDinhTheoVaiTro` ánh xạ quyền mặc định cho `QuanLy`, `CongAn`, `KhachThue`.

- [ ] **Bước 1: Viết các Enums và DTOs mới trong `QuanLyTro.Shared`**
- [ ] **Bước 2: Cập nhật `ActionNames.cs` và `QuyenMacDinhTheoVaiTro.cs`**
- [ ] **Bước 3: Xóa các file Model cũ không còn dùng trong `QuanLyTro.Shared`**
- [ ] **Bước 4: Kiểm tra build dự án `QuanLyTro.Shared`**
  Chạy: `dotnet build QuanLyTro/QuanLyTro.Shared/QuanLyTro.Shared.csproj`
  Kỳ vọng: Build thành công (0 errors).
- [ ] **Bước 5: Commit Tác vụ 1**
  `git add QuanLyTro/QuanLyTro.Shared`
  `git commit -m "refactor(shared): rename models, enums and actions to unaccented vietnamese"`

---

### Tác vụ 2: Chuyển đổi CSDL & Seeder (`database/schema.sql`, `SchemaInitializer.cs`, `DemoSeeder.cs`)

**Files:**
- Sửa: `QuanLyTro/database/schema.sql`
- Sửa: `QuanLyTro/QuanLyTro.Server/Data/SchemaInitializer.cs`
- Sửa: `QuanLyTro/QuanLyTro.Server/Data/DemoSeeder.cs`
- Sửa: `QuanLyTro/QuanLyTro.Server/Data/Database.cs`

**Interfaces:**
- Consumes: `QuanLyTro.Shared` models và action names mới.
- Produces: CSDL gồm các bảng `tai_khoan`, `quyen_vai_tro`, `phong`, `khach_thue`, `hop_dong`, `chi_so_dien_nuoc`, `hoa_don`.

- [ ] **Bước 1: Viết lại `QuanLyTro/database/schema.sql` theo bảng và cột tiếng Việt**
- [ ] **Bước 2: Sửa `DemoSeeder.cs` insert dữ liệu mẫu vào các bảng mới bằng tiếng Việt**
  Gieo tài khoản demo: `chutro|chutro`, `quanly|quanly`, `congan|congan`, `100000000001|100000000001`.
- [ ] **Bước 3: Sửa `SchemaInitializer.cs` để hỗ trợ migration / init đúng schema mới**
- [ ] **Bước 4: Commit Tác vụ 2**
  `git add QuanLyTro/database/schema.sql QuanLyTro/QuanLyTro.Server/Data/`
  `git commit -m "refactor(db): migrate schema and demo seeder to unaccented vietnamese"`

---

### Tác vụ 3: Chuyển đổi Repositories trong `QuanLyTro.Server`

**Files:**
- Đổi tên & Sửa:
  - `QuanLyTro/QuanLyTro.Server/Repositories/ITaiKhoanRepository.cs` (cũ `IUserRepository.cs`, `TenantAuthRepository.cs`)
  - `QuanLyTro/QuanLyTro.Server/Repositories/TaiKhoanRepository.cs`
  - `QuanLyTro/QuanLyTro.Server/Repositories/IPhongRepository.cs` (cũ `IRoomRepository.cs`)
  - `QuanLyTro/QuanLyTro.Server/Repositories/PhongRepository.cs`
  - `QuanLyTro/QuanLyTro.Server/Repositories/IKhachThueRepository.cs` (cũ `ITenantRepository.cs`)
  - `QuanLyTro/QuanLyTro.Server/Repositories/KhachThueRepository.cs`
  - `QuanLyTro/QuanLyTro.Server/Repositories/IHopDongRepository.cs` (cũ `IContractRepository.cs`)
  - `QuanLyTro/QuanLyTro.Server/Repositories/HopDongRepository.cs`
  - `QuanLyTro/QuanLyTro.Server/Repositories/IDienNuocRepository.cs` (cũ `IUtilityRepository.cs`)
  - `QuanLyTro/QuanLyTro.Server/Repositories/DienNuocRepository.cs`
  - `QuanLyTro/QuanLyTro.Server/Repositories/IHoaDonRepository.cs` (cũ `IInvoiceRepository.cs`)
  - `QuanLyTro/QuanLyTro.Server/Repositories/HoaDonRepository.cs`
  - `QuanLyTro/QuanLyTro.Server/Repositories/ICuTruRepository.cs` (cũ `IResidenceRepository.cs`, `ReportRepository.cs`)
  - `QuanLyTro/QuanLyTro.Server/Repositories/CuTruRepository.cs`
  - `QuanLyTro/QuanLyTro.Server/Repositories/IPhanQuyenRepository.cs` (cũ `IPermissionRepository.cs`)
  - `QuanLyTro/QuanLyTro.Server/Repositories/PhanQuyenRepository.cs`

**Interfaces:**
- Consumes: DTOs từ `QuanLyTro.Shared`, bảng CSDL mới.
- Produces: Các interface truy xuất dữ liệu `ITaiKhoanRepository`, `IPhongRepository`, `IKhachThueRepository`, v.v.

- [ ] **Bước 1: Viết các interface và class repository mới, cập nhật câu lệnh SQL query theo bảng/cột mới**
- [ ] **Bước 2: Xóa các file repository cũ**
- [ ] **Bước 3: Commit Tác vụ 3**
  `git add QuanLyTro/QuanLyTro.Server/Repositories/`
  `git commit -m "refactor(server): rename repositories and sql queries to unaccented vietnamese"`

---

### Tác vụ 4: Chuyển đổi Services, Router & Phân quyền trong `QuanLyTro.Server`

**Files:**
- Đổi tên & Sửa:
  - `QuanLyTro/QuanLyTro.Server/Services/XacThucService.cs` (cũ `AuthService.cs`)
  - `QuanLyTro/QuanLyTro.Server/Services/PhongService.cs` (cũ `RoomService.cs`)
  - `QuanLyTro/QuanLyTro.Server/Services/KhachThueService.cs` (cũ `TenantService.cs`)
  - `QuanLyTro/QuanLyTro.Server/Services/HopDongService.cs` (cũ `ContractService.cs`)
  - `QuanLyTro/QuanLyTro.Server/Services/DienNuocService.cs` (cũ `UtilityService.cs`)
  - `QuanLyTro/QuanLyTro.Server/Services/HoaDonService.cs` (cũ `InvoiceService.cs`)
  - `QuanLyTro/QuanLyTro.Server/Services/CuTruService.cs` (cũ `ResidenceService.cs`, `ReportService.cs`)
  - `QuanLyTro/QuanLyTro.Server/Network/MaTranPhanQuyen.cs` (cũ `PermissionMatrix.cs`)
  - `QuanLyTro/QuanLyTro.Server/Network/DieuPhoiYeuCau.cs` (cũ `RequestRouter.cs`)
  - `QuanLyTro/QuanLyTro.Server/Network/TcpListenerServer.cs`, `ClientHandler.cs`
  - `QuanLyTro/QuanLyTro.Server/Program.cs`

**Interfaces:**
- Consumes: Repositories và DTOs mới.
- Produces: Bộ dịch vụ nghiệp vụ và bộ định tuyến TCP xử lý 26 action names tiếng Việt không dấu.

- [ ] **Bước 1: Cập nhật các Services sang tiếng Việt không dấu**
- [ ] **Bước 2: Cập nhật `MaTranPhanQuyen.cs` và `DieuPhoiYeuCau.cs`**
- [ ] **Bước 3: Đấu nối vào `Program.cs` và `ClientHandler.cs` của Server**
- [ ] **Bước 4: Kiểm tra build `QuanLyTro.Server`**
  Chạy: `dotnet build QuanLyTro/QuanLyTro.Server/QuanLyTro.Server.csproj`
  Kỳ vọng: Build thành công (0 errors).
- [ ] **Bước 5: Commit Tác vụ 4**
  `git add QuanLyTro/QuanLyTro.Server/`
  `git commit -m "refactor(server): rename services, router and permission matrix to unaccented vietnamese"`

---

### Tác vụ 5: Chuyển đổi Client WinForms & Giao diện Web (Assets/wwwroot)

**Files:**
- Sửa: `QuanLyTro/Form1.cs`, `Form1.Designer.cs`, `QuanLyTro/Network/WebMessageBridge.cs`, `QuanLyTro/Network/TcpClientService.cs`
- Thư mục wwwroot:
  - Di chuyển: `QuanLyTro/Assets/wwwroot/landlord` → `QuanLyTro/Assets/wwwroot/chutro`
  - Di chuyển: `QuanLyTro/Assets/wwwroot/police` → `QuanLyTro/Assets/wwwroot/congan`
  - Di chuyển: `QuanLyTro/Assets/wwwroot/tenant` → `QuanLyTro/Assets/wwwroot/khachthue`
- Sửa JS/HTML:
  - `QuanLyTro/Assets/wwwroot/auth/auth.js`
  - `QuanLyTro/Assets/wwwroot/chutro/js/*.js`
  - `QuanLyTro/Assets/wwwroot/congan/js/*.js`
  - `QuanLyTro/Assets/wwwroot/khachthue/js/*.js`
  - `QuanLyTro/QuanLyTro.csproj` (cập nhật khai báo None/Content cho assets mới)

**Interfaces:**
- Consumes: ActionNames mới và payload thuộc tính tiếng Việt (`soPhong`, `giaThue`, `hoTen`...).
- Produces: Giao diện người dùng gọi các action tiếng Việt và điều hướng đúng các thư mục `chutro`, `congan`, `khachthue`.

- [ ] **Bước 1: Đổi tên thư mục và cập nhật `QuanLyTro.csproj`**
- [ ] **Bước 2: Cập nhật `auth.js` điều hướng sang `/chutro/`, `/congan/`, `/khachthue/` dựa trên `vaiTro`**
- [ ] **Bước 3: Cập nhật các file JS trong `chutro`, `congan`, `khachthue` gọi đúng ActionNames và thuộc tính DTO mới**
- [ ] **Bước 4: Cập nhật `Form1.cs` và `WebMessageBridge.cs`**
- [ ] **Bước 5: Kiểm tra build `QuanLyTro` WinForms**
  Chạy: `dotnet build QuanLyTro/QuanLyTro.csproj`
  Kỳ vọng: Build thành công (0 errors).
- [ ] **Bước 6: Commit Tác vụ 5**
  `git add QuanLyTro/Assets/ QuanLyTro/Form1.cs QuanLyTro/Network/ QuanLyTro/QuanLyTro.csproj`
  `git commit -m "refactor(client): rename actor folders and update bridge calls to unaccented vietnamese"`

---

### Tác vụ 6: Cập nhật Test Suite & Kiểm thử Toàn diện (`QuanLyTro.Tests`)

**Files:**
- Sửa: Toàn bộ file trong `QuanLyTro/QuanLyTro.Tests/` (đổi tên các class/method gọi service, repository, action names, DTOs).

- [ ] **Bước 1: Cập nhật các unit test và acceptance test sang DTOs và Services mới**
- [ ] **Bước 2: Chạy kiểm thử toàn bộ giải pháp**
  Chạy: `dotnet test`
  Kỳ vọng: Toàn bộ 201+ test cases passed (0 failures).
- [ ] **Bước 3: Commit Tác vụ 6**
  `git add QuanLyTro/QuanLyTro.Tests/`
  `git commit -m "test: update full test suite for unaccented vietnamese refactoring"`

---

### Tác vụ 7: Cập nhật Tài liệu & Memory (`CLAUDE.md`, `AGENTS.md`, memory)

**Files:**
- Sửa: `CLAUDE.md`
- Sửa: `AGENTS.md`
- Sửa: `C:\Users\HUY\.claude\projects\D--Dai-Hoc-LapTrinhMang\memory\MEMORY.md` và các memory liên quan
- Sửa: `credentials.txt` (cập nhật tài khoản demo: `chutro|chutro`, `quanly|quanly`, `congan|congan`, `100000000001|100000000001`)

- [ ] **Bước 1: Cập nhật `CLAUDE.md` và `AGENTS.md`**
- [ ] **Bước 2: Cập nhật file memory**
- [ ] **Bước 3: Cập nhật `credentials.txt`**
- [ ] **Bước 4: Commit Tác vụ 7**
  `git add CLAUDE.md AGENTS.md`
  `git commit -m "docs: update CLAUDE.md, AGENTS.md, and memory for unaccented vietnamese entities"`
