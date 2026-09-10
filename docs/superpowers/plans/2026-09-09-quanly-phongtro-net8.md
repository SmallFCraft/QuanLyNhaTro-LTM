# Quản Lý Phòng Trọ .NET 8 — MASTER PLAN (Leader Dispatch)

> **Cho Leader Agent:** Đọc hết file này trước. Bạn điều phối worker theo từng wave ở §5. Mỗi worker nhận ĐÚNG MỘT file sub-plan trong `docs/superpowers/plans/subplans/`.
>
> **Cho Worker Agent:** Nếu bạn được giao một sub-plan, đọc §2 (bối cảnh code) và §4 (ràng buộc chung) trước, rồi làm đúng các bước trong file sub-plan của bạn. KHÔNG đọc các sub-plan khác.

**Goal:** Hoàn thiện hệ thống quản lý phòng trọ Client–Server TCP/IP trên .NET 8: Server console sở hữu toàn bộ nghiệp vụ + MySQL, Client WinForms chỉ gửi request và hiển thị kết quả, 2 vai (chủ trọ / người thuê).

**Architecture:** Ba project — `QuanLyTro.Shared` (DTO + hợp đồng giao thức, không phụ thuộc gì), `QuanLyTro.Server` (nghiệp vụ + TCP + MySQL), `QuanLyTro` (WinForms Client, không chạm DB). JSON UTF-8 phân khung một dòng mỗi packet; transaction và unique constraint bảo vệ dữ liệu khi nhiều Client ghi đồng thời.

**Tech Stack:** C# 12, .NET 8 (`net8.0` cho Shared/Server/Tests; `net8.0-windows` cho Client), WinForms, `TcpClient`/`TcpListener`, `System.Text.Json`, ADO.NET + MySqlConnector 2.4.0, MySQL 8.0.30 (Laragon), MSTest 3.7.

**Spec (đọc kèm):**
- `docs/superpowers/specs/2026-09-09-quanly-phongtro-srs-design.md` — SRS gốc
- `docs/superpowers/specs/2026-09-10-quanly-phongtro-multi-actor-design.md` — delta 2 tác nhân
- `DESIGN.md` — hệ thống thiết kế UI (nguồn token duy nhất)

---

## 1. TRẠNG THÁI HIỆN TẠI

**Nhánh:** `feat/multi-actor-implementation` · **Cập nhật:** 2026-09-10 · **MySQL:** đã verify trên 8.0.30, Laragon bật, cổng 3306

### 1.1. Đã xong

| Task | Nội dung | Commit |
|---|---|---|
| 1 | Nâng WinForms + tạo solution 3 project | (trước) |
| 2 | DTO + giao thức dùng chung | (trước) |
| 3 | Schema MySQL + cấu hình Server | `c8dc8d3`, `ca949ea` |
| 4 | Password, đăng nhập, session | `6942145` |
| 5 | Phân vai người thuê | `b6a37cc` |
| 6 | `RoomRepository/Service` + `TenantRepository/Service` | `7af650a`, `740db46` |
| 7 | `ContractRepository/Service` + `UtilityRepository/Service` | `d67fd24` |
| 8 | `InvoiceRepository/Service` + `ReportRepository/Service` | `38d44c4` |

### 1.2. Đã verify thật trên MySQL sống

6 bảng: `users`, `rooms`, `tenants`, `contracts`, `utility_readings`, `invoices`.
5 CHECK constraint: `chk_rooms_price`, `chk_rooms_max_occupants`, `chk_contracts_dates`, `chk_utility_electricity`, `chk_utility_water`.
4 index: `idx_contracts_room_status`, `idx_invoices_status_month`, `uq_room_month`, `uq_invoice_room_month`.
Cột `tenants.password_hash` tồn tại (đăng nhập người thuê bằng CCCD).

`dotnet run --project "QuanLyTro/QuanLyTro.Server" -- --initialize-only` chạy **hai lần liên tiếp** đều in `Database initialized.` (idempotent). CHECK constraint đã test chặn đúng: giá âm bị `chk_rooms_price` từ chối, chỉ số điện giảm bị `chk_utility_electricity` từ chối.

### 1.3. Còn lại — 7 sub-plan

| Sub-plan | Task | File | Trạng thái |
|---|---|---|---|
| A | 6 | [task-6-rooms-tenants.md](subplans/task-6-rooms-tenants.md) | ✅ XONG (`7af650a`, `740db46`) |
| B | 7 | [task-7-contracts-utilities.md](subplans/task-7-contracts-utilities.md) | ✅ XONG (`d67fd24`) |
| C | 8 | [task-8-invoices-reports.md](subplans/task-8-invoices-reports.md) | ✅ XONG (`38d44c4`) |
| D | 9 | [task-9-tcp-server-router.md](subplans/task-9-tcp-server-router.md) | ⬜ chưa |
| E | 10 | [task-10-client-service-shell.md](subplans/task-10-client-service-shell.md) | ⬜ chưa |
| F | 11 | [task-11-winforms-screens.md](subplans/task-11-winforms-screens.md) | ⬜ chưa |
| G | 12 | [task-12-final-tests-docs.md](subplans/task-12-final-tests-docs.md) | ⬜ chưa |

**Tổng test hiện tại:** 80 PASS, 0 FAIL.

---

## 2. BỐI CẢNH CODE (worker phải biết trước khi viết dòng nào)

### 2.1. Cây file hiện có

```text
QuanLyTro/
├── QuanLyTro.slnx
├── QuanLyTro.csproj                    # WinForms Client, net8.0-windows
├── App.config                          # CHƯA có ServerHost/ServerPort (Task 10 thêm)
├── Program.cs, Form1.cs, Form1.Designer.cs
│
├── database/schema.sql                 # 6 bảng, marker "-- statement" trên dòng riêng
│
├── QuanLyTro.Shared/                   # net8.0, KHÔNG tham chiếu gì
│   ├── SharedConstants.cs
│   ├── Models/
│   │   ├── RoomDto.cs                  # RoomStatus enum + RoomDto
│   │   ├── TenantDto.cs
│   │   ├── ContractDto.cs              # ContractStatus enum
│   │   ├── UtilityReadingDto.cs
│   │   ├── InvoiceDto.cs               # InvoiceStatus enum
│   │   └── AuthAndReportDtos.cs        # UserRole, LoginRequest, LoginResult,
│   │                                   # RoomQuery, CreateInvoiceRequest,
│   │                                   # SummaryReportDto, ResidenceExportDto
│   └── Protocol/
│       ├── ActionNames.cs              # 22 action + danh sách All
│       ├── RequestPacket.cs            # Create<T>, GetData<T>
│       ├── ResponsePacket.cs           # Ok<T>, Fail
│       └── JsonDefaults.cs             # Options dùng chung 2 phía
│
├── QuanLyTro.Server/                   # net8.0, Console App
│   ├── Program.cs                      # có --initialize-only
│   ├── appsettings.json                # ConnectionString + Port 8888
│   ├── Config/ServerOptions.cs         # Load(path), Validate()
│   ├── Data/
│   │   ├── Database.cs                 # OpenAsync, OpenServerLevelAsync
│   │   └── SchemaInitializer.cs        # ReadEmbeddedSchema, SplitStatements
│   ├── Security/
│   │   ├── PasswordHasher.cs           # Hash, Verify (PBKDF2-SHA256, 210k vòng)
│   │   └── SessionStore.cs             # Create(userId, role), TryGet, Remove
│   ├── Repositories/
│   │   ├── IAuthRepositories.cs        # IUserRepository, ITenantRepository + records
│   │   ├── UserRepository.cs           # IUserRepository, bảng users
│   │   ├── TenantAuthRepository.cs     # ITenantRepository, tra CCCD để login
│   │   └── RoomRepository.cs           # CRUD phòng + CanDeleteAsync
│   ├── Network/PermissionMatrix.cs     # IsAllowed(action, role), IsKnown(action)
│   └── Services/
│       ├── BusinessRuleException.cs    # message tiếng Việt hiển thị thẳng cho user
│       ├── AuthService.cs              # IAuthService + AuthService (2 bảng, lockout)
│       └── RoomService.cs              # GetAll/Add/Update/Delete + Validate
│
└── QuanLyTro.Tests/                    # net8.0, MSTest
    ├── ProtocolTests.cs (4)            # Task 2
    ├── SanityTests.cs (1)
    ├── ServerOptionsTests.cs (2)       # Task 3
    ├── SchemaInitializerTests.cs (4)   # Task 3
    ├── PasswordHasherTests.cs (3)      # Task 4
    ├── AuthTests.cs (8)                # Task 4-5, gồm SessionStore
    ├── PermissionMatrixTests.cs (3)    # Task 5
    └── BusinessRuleTests.cs (5)        # Task 6 phần phòng
```

### 2.2. Hợp đồng interface worker PHẢI dùng đúng tên

**Từ `QuanLyTro.Shared` (đã có, không sửa nếu không được giao):**

```csharp
// Protocol/ActionNames.cs — 22 hằng số dạng string
ActionNames.AuthLogin          // "AUTH_LOGIN"
ActionNames.RoomGetAll, RoomAdd, RoomUpdate, RoomDelete
ActionNames.TenantGetByRoom, TenantAdd, TenantUpdate, TenantCheckout, TenantDelete
ActionNames.ContractCreate, ContractTerminate, ContractRenew, ContractGetAll
ActionNames.UtilityGetPrevious, UtilityRecord
ActionNames.InvoiceCreate, InvoiceGetAll, InvoicePay, InvoiceGetMine
ActionNames.ReportSummary, ExportResidence
ActionNames.All                // IReadOnlyList<string>, dùng để test

// Models — record bất biến, khởi tạo bằng positional
RoomDto(int Id, string RoomNumber, decimal Price, int MaxOccupants,
        RoomStatus Status, string? Description, int CurrentOccupants = 0)
TenantDto(int Id, int? RoomId, string FullName, DateOnly DateOfBirth, string IdCard,
          string Phone, string Hometown, string? Workplace, bool IsTemporaryRegistered)
ContractDto(int Id, int RoomId, int RepresentativeTenantId, DateOnly StartDate, DateOnly EndDate,
            decimal RentalPrice, decimal DepositAmount, ContractStatus Status, string? Notes)
UtilityReadingDto(int Id, int RoomId, string BillingMonth, int OldElectricity, int NewElectricity,
                  decimal ElectricityRate, int OldWater, int NewWater, decimal WaterRate)
InvoiceDto(int Id, int RoomId, int ContractId, string BillingMonth, decimal RoomAmount,
           decimal ElectricityAmount, decimal WaterAmount, decimal OtherFees,
           decimal TotalAmount, InvoiceStatus Status, DateTime? PaidAt)
CreateInvoiceRequest(int RoomId, string BillingMonth, decimal OtherFees)
RoomQuery(int RoomId)
SummaryReportDto(int TotalRooms, int AvailableRooms, int RentedRooms, int CurrentTenants,
                 decimal PaidAmount, decimal UnpaidAmount)
ResidenceExportDto(string FullName, DateOnly DateOfBirth, string IdCard, string Hometown, string RoomNumber)
LoginRequest(string Username, string Password)
LoginResult(string Token, string FullName, UserRole Role)
enum UserRole { Landlord, Tenant }
enum RoomStatus { Available, Rented, Maintenance }
enum ContractStatus { Active, Expired, Terminated }
enum InvoiceStatus { Unpaid, Paid }

// Protocol
RequestPacket.Create<T>(string action, string? token, T data)
RequestPacket.GetData<T>()
ResponsePacket.Ok<T>(T data)
ResponsePacket.Fail(string message)
JsonDefaults.Options           // JsonSerializerOptions dùng chung 2 phía
```

**Từ `QuanLyTro.Server` (đã có):**

```csharp
// Data/Database.cs
new Database(string connectionString)
await database.OpenAsync(ct)              // có chọn Database
await database.OpenServerLevelAsync(ct)   // KHÔNG chọn Database — cho CREATE DATABASE

// Security
PasswordHasher.Hash(string password) -> string
PasswordHasher.Verify(string password, string hash) -> bool
new SessionStore()
store.Create(int userId, UserRole role) -> string token
store.TryGet(string token, out (int UserId, UserRole Role) session) -> bool
store.Remove(string token)

// Repositories
IAuthRepositories.cs:
  record UserRecord(int Id, string PasswordHash, string FullName)
  record TenantAuthRecord(int Id, string IdCard, string PasswordHash, string FullName, int? RoomId)
  interface IUserRepository   { Task<UserRecord?> FindByUsernameAsync(string, CancellationToken) }
  interface ITenantRepository { Task<TenantAuthRecord?> FindByCccdAsync(string, CancellationToken) }

RoomRepository(Database database):
  Task<List<RoomDto>> GetAllAsync(ct)
  Task<RoomDto> AddAsync(RoomDto, ct)        // bắt MySQL 1062 -> BusinessRuleException
  Task<bool> UpdateAsync(RoomDto, ct)
  Task<bool> DeleteAsync(int roomId, ct)
  Task<bool> CanDeleteAsync(int roomId, ct)  // BR-12

// Services
BusinessRuleException(string message) : Exception
  // Message là câu tiếng Việt hiển thị THẲNG cho người dùng
  // Ví dụ: "Số phòng đã tồn tại.", "Giá thuê phải lớn hơn 0."
interface IAuthService { Task<LoginResult> LoginAsync(LoginRequest, CancellationToken) }
AuthService(IUserRepository, ITenantRepository, SessionStore, Func<DateTimeOffset>? clock = null)
  AuthService.MaxFailedAttempts = 5
  AuthService.LockoutWindow = TimeSpan.FromMinutes(1)
  AuthService.InvalidCredentialsMessage
RoomService(RoomRepository rooms)
  GetAllAsync / AddAsync / UpdateAsync / DeleteAsync
  static void Validate(RoomDto)   // ném BusinessRuleException
```

### 2.3. Lệnh hay dùng

```bash
# Build toàn solution
dotnet build "QuanLyTro/QuanLyTro.slnx" --nologo

# Chạy test (kèm build)
dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --nologo

# Lọc test theo tên
dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter <TênTest>

# Khởi tạo DB (idempotent, chạy lại được)
dotnet run --project "QuanLyTro/QuanLyTro.Server" -- --initialize-only

# MySQL CLI (Laragon)
"E:/Apps/laragon/bin/mysql/mysql-8.0.30-winx64/bin/mysql.exe" -h 127.0.0.1 -P 3306 -u root -D quanly_phongtro_nhs -e "<SQL>"
```

### 2.4. Bẫy đã gặp — đừng lặp lại

1. **`internal` không test được:** MSTest project không có `InternalsVisibleTo`. Helper muốn test → khai `public`.
2. **MSTest cần using tường minh:** project không bật global usings cho MSTest. Mọi file test phải có `using Microsoft.VisualStudio.TestTools.UnitTesting;`.
3. **Static initializer chạy theo thứ tự khai báo:** trong `PermissionMatrix`, `LandlordOnly` phải khai TRƯỚC dictionary `Allowed`, nếu không sẽ `NullReferenceException`.
4. **`SchemaInitializer` phải dùng `OpenServerLevelAsync`:** `appsettings.json` trỏ tới database chưa tồn tại; mở connection có chọn Database sẽ fail trước khi chạy được `CREATE DATABASE`.
5. **Marker `-- statement` phải nằm trên DÒNG RIÊNG:** nếu chỉ `Split("-- statement")` thì dòng comment mô tả trong `schema.sql` cũng bị tách, sinh SQL rác.
6. **Kiểm tra quyền trước khi gọi service:** `PermissionMatrix` là server-enforced (BR-14). Client chỉ ẩn/hiện menu cho UX.
7. **`ResponsePacket` không lộ stack trace:** exception ngoài `BusinessRuleException` → log Console, trả message chung.

---

## 3. BẢNG PHÂN CÔNG SUB-PLAN

| Sub-plan | Task | File | Phụ thuộc | Chạm file chính |
|---|---|---|---|---|
| A | 6 | [task-6-rooms-tenants.md](subplans/task-6-rooms-tenants.md) | Task 5 | `TenantRepository.cs`, `TenantService.cs` |
| B | 7 | [task-7-contracts-utilities.md](subplans/task-7-contracts-utilities.md) | Task 6 | `ContractRepository/Service`, `UtilityRepository/Service` |
| C | 8 | [task-8-invoices-reports.md](subplans/task-8-invoices-reports.md) | A, B | `InvoiceRepository/Service`, `ReportRepository/Service` |
| D | 9 | [task-9-tcp-server-router.md](subplans/task-9-tcp-server-router.md) | C | `RequestRouter.cs`, `TcpListenerServer.cs`, `ClientHandler.cs` |
| E | 10 | [task-10-client-service-shell.md](subplans/task-10-client-service-shell.md) | Task 2 | `Network/TcpClientService.cs`, `Form1.cs` |
| F | 11 | [task-11-winforms-screens.md](subplans/task-11-winforms-screens.md) | D, E | `Forms/*.cs` (8 màn), `Form1.cs` |
| G | 12 | [task-12-final-tests-docs.md](subplans/task-12-final-tests-docs.md) | F | `AcceptanceTests.cs`, `README.md` |

---

## 4. RÀNG BUỘC CHUNG (mọi worker phải tuân)

### 4.1. Kiến trúc

- Target `net8.0` cho Shared/Server/Tests; `net8.0-windows` cho WinForms Client.
- Client **KHÔNG** được tham chiếu `MySqlConnector` hoặc mở kết nối DB. Mọi thao tác qua TCP.
- Mọi quy tắc BR-01..BR-14 enforce tại **Server**; ràng buộc DB bảo vệ uniqueness và foreign key.
- Mỗi packet JSON UTF-8 kết thúc bằng `\n`; giới hạn một dòng packet là **1 MiB**.
- Mọi SQL nhận dữ liệu người dùng phải **parameter hóa**. Không nối chuỗi SQL.
- Tiền dùng `decimal` (**không** `double`); tháng hóa đơn là chuỗi `yyyy-MM`; ngày dùng `DateOnly` trong DTO.
- Mật khẩu dùng PBKDF2 (`PasswordHasher`), **không** tự viết lại.
- Không thêm abstraction ngoài interface tại biên Repository/Network cần cho kiểm thử.

### 4.2. Giao diện

- Màu, cỡ chữ, bán kính bo góc lấy từ **`DESIGN.md`** — không hardcode màu mới trong C#.
- Bản mẫu trực quan: `docs/superpowers/mockups/wireframe-quanly-phongtro.html`.
- Ánh xạ: `ColorTranslator.FromHtml("<hex trong DESIGN.md>")`; font `Segoe UI 9pt` chữ thường, `Consolas 9.5pt` cho số liệu/tiền.
- **Không dùng pill bo tròn.** Góc bo 4px (component) / 8px (card).
- Dòng đang chọn trong mọi `DataGridView`: nền `#21262B` + dải trái 3px `#D95D39`.
- Tag trạng thái: sage `#8BD7A3` (tốt) / terracotta `#D95D39` (nợ, khẩn) / amber `#E0AF68` (sắp hết hạn, bảo trì) / neutral `#CAC6C1` (trống, chờ).
- Mọi control đặt `AccessibleName`; lỗi hiển thị bằng `MessageBox` và **giữ nguyên input**.

### 4.3. Quy tắc làm việc của worker

1. **Ranh giới file:** chỉ sửa file trong mục **Files** của sub-plan mình. Cần sửa file ngoài danh sách → **DỪNG, báo Leader**, không tự mở rộng.
2. **Không sửa:** `DESIGN.md`, `PRODUCT.md`, file spec, master plan này. Ngoại lệ duy nhất: Sub-plan G được đánh dấu checkbox trong master plan.
3. **TDD bắt buộc:** viết test thất bại → chạy xác nhận fail → implement tối thiểu → chạy xác nhận pass → commit.
4. **Verify bằng chứng thật:** phải chạy `dotnet test` và **dán output thật**. Không báo "PASS" mà không có log.
5. **Commit riêng:** mỗi worker tự commit, message ghi rõ số Task. Không gộp nhiều task một commit. Kết thúc commit message bằng:
   ```
   Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>
   ```
6. **File dùng chung `BusinessRuleTests.cs`:** nếu sub-plan bạn cần thêm test vào file này mà worker khác đang chạy song song cùng wave → tạo file test riêng tên khác (`RoomTests.cs`, `ContractTests.cs`…), đừng cùng ghi một file.
7. **Báo cáo xong theo mẫu §7.**

---

## 5. WAVE TRIỂN KHAI — tối đa 3 AGENT SONG SONG

Leader chạy tuần tự từng wave, verify xong mới sang wave sau.

### Wave 1 — Nghiệp vụ nền

| Slot | Sub-plan | Ghi chú |
|---|---|---|
| Agent 1 | **A** — Task 6 | Phần phòng đã xong; chỉ còn `TenantRepository` + `TenantService` |
| Agent 2 | **B** — Task 7 | Chạm file khác hoàn toàn với A → song song an toàn |
| Agent 3 | — | Trống |

**Điều kiện vào:** Task 5 đã xong (đã xong). **Điều kiện ra:** `dotnet test` 30+ test PASS; cả A và B báo xong kèm log.

### Wave 2 — Nghiệp vụ phụ thuộc

| Slot | Sub-plan | Ghi chú |
|---|---|---|
| Agent 1 | **C** — Task 8 | Cần bảng `rooms`, `contracts`, `utility_readings` đã có service từ wave 1 |
| Agent 2, 3 | — | Trống |

**Điều kiện vào:** A và B xong. **Điều kiện ra:** test PASS, có ít nhất 1 test cho BR-09/10/11.

### Wave 3 — Tầng mạng (3 agent song song)

| Slot | Sub-plan | Ghi chú |
|---|---|---|
| Agent 1 | **D** — Task 9 | `RequestRouter` + TCP listener. Cần toàn bộ service từ wave 1–2 |
| Agent 2 | **E** — Task 10 | Chỉ cần `QuanLyTro.Shared` → chạy được ngay từ đầu wave |
| Agent 3 | — | Trống (hoặc tách D thành router/listener nếu muốn 3 slot) |

**Điều kiện vào:** C xong. **Điều kiện ra:** integration test round-trip qua TCP thật PASS; `Form1` designer mở không lỗi.

### Wave 4 — Giao diện

| Slot | Sub-plan | Ghi chú |
|---|---|---|
| Agent 1 | **F** — Task 11 | 8 màn hình `UserControl` |
| Agent 2, 3 | — | Có thể tách F thành 3 nhóm màn nếu cần tốc độ: xem ghi chú dưới |

**Tách F nếu chạy 3 agent:** nhóm 1 = `DashboardForm` + `RoomsForm` + `TenantsForm`; nhóm 2 = `ContractsForm` + `UtilitiesForm`; nhóm 3 = `InvoicesForm` + `ReportsForm` + `MyInvoicesForm`. Mỗi nhóm tạo file riêng, **chỉ một nhóm được sửa `Form1.cs`** (nhóm 1), hai nhóm kia chỉ tạo UserControl rồi Leader gắn tab sau.

**Điều kiện vào:** D và E xong. **Điều kiện ra:** `dotnet build "QuanLyTro/QuanLyTro.slnx"` 0 error; chạy tay luồng tháng thông suốt.

### Wave 5 — Chốt sổ

| Slot | Sub-plan | Ghi chú |
|---|---|---|
| Agent 1 | **G** — Task 12 | Test acceptance + README + đánh dấu plan |

**Điều kiện vào:** F xong. **Điều kiện ra:** 0 test fail; coverage đo được; README có lệnh chạy demo chính xác.

---

## 6. NHIỆM VỤ CỦA LEADER

1. **Đọc** file này + §1 để biết đang ở đâu.
2. **Chọn wave** tiếp theo theo bảng §5.
3. **Dispatch** worker: mỗi agent nhận prompt gồm đường dẫn file master plan (để đọc §2, §4) và đường dẫn sub-plan của mình.
4. **Thu báo cáo** theo mẫu §7; thiếu log test → trả lại, không chấp nhận.
5. **Verify độc lập:** tự chạy `dotnet test` + `dotnet build` sau mỗi wave, không tin báo cáo suông.
6. **Kiểm tra ranh giới:** xem `git show --stat` của commit worker; phát hiện sửa file ngoài danh sách → nhắc nhở, xét revert.
7. **Cập nhật** bảng §1.3 (trạng thái sub-plan) và §1.1 (task đã xong) sau mỗi wave.
8. **Gỡ chặn:** worker báo kẹt → Leader quyết định: sửa sub-plan, đổi thứ tự wave, hay escalate lên người dùng.
9. **Không tự implement:** Leader điều phối. Nếu buộc phải tự làm một task nhỏ để gỡ chặn thì ghi rõ trong báo cáo.

---

## 7. MẪU BÁO CÁO CỦA WORKER

Worker kết thúc phải trả về đúng mẫu này:

```markdown
## Sub-plan <X>: <Tên>

**Trạng thái:** XONG / KẸT / XONG MỘT PHẦN

**Commit:** <hash> — <message>

**File đã tạo/sửa:**
- `đường/dẫn/file.cs` (tạo mới / sửa)

**Bằng chứng test:**
```
<dán nguyên output của dotnet test, gồm dòng Passed!/Failed! với số liệu>
```

**Bằng chứng build:**
```
<dán dòng "0 Error(s)" hoặc lỗi nếu có>
```

**Kiểm tra thật trên MySQL (nếu sub-plan yêu cầu):**
```
<dán lệnh + output thật>
```

**Quyết định thiết kế cần Leader biết:**
- <ví dụ: chọn dùng transaction ở AddAsync để tránh race>

**Kẹt / cần gỡ (nếu có):**
- <mô tả chính xác + đã thử gì>
```

---

## 8. ĐỊNH NGHĨA HOÀN THÀNH

Một sub-plan chỉ được coi là XONG khi:

- [ ] Mọi bước trong file sub-plan đã đánh dấu `[x]`.
- [ ] `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj"` chạy 0 failed — có log dán kèm.
- [ ] `dotnet build "QuanLyTro/QuanLyTro.slnx"` 0 error (client `net8.0-windows` build được).
- [ ] Đã commit, message ghi rõ số Task, không lẫn file ngoài danh sách.
- [ ] Nếu sub-plan yêu cầu verify trên MySQL thật → có log truy vấn thật.
- [ ] Không có TODO/TBD/hàm rỗng bỏ lại trong code.

---

## 9. GHI CHÚ VỀ CÁC RÀNG BUỘC NGHIỆP VỤ (BR) THEO TASK

Để worker không phải đọc lại toàn bộ SRS:

| BR | Nội dung | Task xử lý | Tầng |
|---|---|---|---|
| BR-01 | Số phòng duy nhất, giá > 0, sức chứa hợp lệ | 6 | Service + DB CHECK |
| BR-02 | Không vượt `MaxOccupants` | 6 | Service + transaction |
| BR-03 | CCCD duy nhất | 6 | DB UNIQUE |
| BR-04 | Tối đa 1 hợp đồng `Active` / phòng | 7 | Service |
| BR-05 | Đại diện ký HĐ phải ở trong phòng đó | 7 | Service |
| BR-06 | `EndDate > StartDate` | 7 | Service + DB CHECK |
| BR-07 | Chỉ số mới >= chỉ số cũ | 7 | Service + DB CHECK |
| BR-08 | 1 bản ghi điện nước / phòng / tháng | 7 | DB UNIQUE `uq_room_month` |
| BR-09 | Lập HĐơn cần HĐ Active + đã chốt điện nước | 8 | Service |
| BR-10 | Công thức tổng tiền (server tự tính) | 8 | Service |
| BR-11 | Hóa đơn `Paid` bất biến | 8 | Service |
| BR-12 | Chỉ xóa phòng trống | 6 | Service |
| BR-13 | Transaction cho thao tác nhiều bảng | 8 | Service |
| BR-14 | Người thuê chỉ xem hóa đơn phòng mình | 5, 9 | `PermissionMatrix` + router |

---

## 10. RỦI RO ĐÃ BIẾT

| Rủi ro | Ảnh hưởng | Cách xử |
|---|---|---|
| MySQL tắt giữa chừng | Task 6–8 verify được (unit test dùng stub), nhưng không chạy được integration | Task 3–8 tự test bằng stub; verify DB thật ở cuối mỗi wave |
| `Form1.Designer.cs` sửa tay bị hỏng | Designer không mở được, mất công dựng lại | Luồng chính: dựng control bằng code trong `InitializeComponent` do agent viết, mở Designer kiểm tra ngay sau khi sửa |
| Nhiều worker cùng ghi một file test | Conflict, mất test | Quy tắc §4.3.6 — mỗi nhóm tạo file test riêng |
| WinForms chỉ build trên Windows | Không build được trên CI Linux | Chấp nhận: đồ án chạy trên Windows, dùng `dotnet build` local |
| `appsettings.json` hardcode mật khẩu rỗng của Laragon | Không dùng được cho môi trường khác | Chấp nhận cho demo; ghi rõ trong README Task 12 |
