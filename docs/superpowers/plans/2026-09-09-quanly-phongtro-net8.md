# Quản Lý Phòng Trọ .NET 8 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement plan task-by-task.

**Goal:** Nâng solution WinForms hiện có lên .NET 8 và xây hệ thống quản lý phòng trọ Client–Server TCP/IP theo SRS.

**Architecture:** Ba project: `QuanLyTro.Shared` chứa DTO và hợp đồng giao thức; `QuanLyTro.Server` sở hữu nghiệp vụ, TCP và MySQL; `QuanLyTro` là WinForms Client chỉ gửi yêu cầu rồi hiển thị kết quả. JSON UTF-8 phân khung bằng một dòng cho mỗi packet; transaction và unique constraint bảo vệ dữ liệu khi nhiều Client ghi đồng thời.

**Tech Stack:** C# 12, .NET 8, WinForms, `TcpClient`/`TcpListener`, `System.Text.Json`, ADO.NET, MySqlConnector, MySQL 8/InnoDB, MSTest.

**Spec:** `docs/superpowers/specs/2026-09-09-quanly-phongtro-srs-design.md`

---

## Trạng thái thực thi

**Cập nhật lần cuối:** 2026-09-10 · **Nhánh:** `feat/multi-actor-implementation` · **MySQL:** đã verify trên 8.0.30 (Laragon, cổng 3306)

| Task | Nội dung | Trạng thái | Commit |
|---|---|---|---|
| 1 | Nâng WinForms + solution 3 project | ✅ Xong | (trước) |
| 2 | DTO + giao thức dùng chung | ✅ Xong | (trước) |
| 3 | Schema MySQL + cấu hình Server | ✅ Xong, verify MySQL thật | `c8dc8d3`, `ca949ea` |
| 4 | Password, đăng nhập, session | ✅ Xong | `6942145` |
| 5 | Phân vai người thuê | ✅ Xong | `b6a37cc` |

**Đã verify trên MySQL sống:** 6 bảng (`users`, `rooms`, `tenants`, `contracts`, `utility_readings`, `invoices`), 5 CHECK constraint (`chk_rooms_price`, `chk_rooms_max_occupants`, `chk_contracts_dates`, `chk_utility_electricity`, `chk_utility_water`), 4 index (`idx_contracts_room_status`, `idx_invoices_status_month`, `uq_room_month`, `uq_invoice_room_month`), cột `tenants.password_hash`; chạy `--initialize-only` hai lần liên tiếp đều thành công (idempotent); CHECK constraint chặn đúng dữ liệu sai.

**Còn lại:** Task 6–12, chia thành 7 sub-plan trong thư mục `docs/superpowers/plans/subplans/`.

---

## Điều phối Sub-Plan (Leader Agent)

Các task 6–12 quá dài để chạy tuần tự một mạch, nên tách thành 7 file plan con. **Leader** đọc mục này, dispatch worker theo từng wave, mỗi worker nhận ĐÚNG MỘT file sub-plan.

### Bảng phân công

| Sub-plan | Task | File | Phụ thuộc | Chạy song song với |
|---|---|---|---|---|
| A | 6 | [task-6-rooms-tenants.md](subplans/task-6-rooms-tenants.md) | Task 5 | B |
| B | 7 | [task-7-contracts-utilities.md](subplans/task-7-contracts-utilities.md) | Task 5 | A |
| C | 8 | [task-8-invoices-reports.md](subplans/task-8-invoices-reports.md) | A, B | — |
| D | 9 | [task-9-tcp-server-router.md](subplans/task-9-tcp-server-router.md) | C | E |
| E | 10 | [task-10-client-service-shell.md](subplans/task-10-client-service-shell.md) | Task 2 (Shared) | D |
| F | 11 | [task-11-winforms-screens.md](subplans/task-11-winforms-screens.md) | D, E | — |
| G | 12 | [task-12-final-tests-docs.md](subplans/task-12-final-tests-docs.md) | F | — |

### Wave triển khai

1. **Wave 1 — nghiệp vụ nền (2 agent song song):** A (phòng + người thuê) và B (hợp đồng + điện nước). Hai sub-plan này chỉ chạm file khác nhau (`RoomRepository`/`TenantRepository`/`RoomService`/`TenantService` so với `ContractRepository`/`UtilityRepository`/`ContractService`/`UtilityService`), KHÔNG cùng sửa một file.
2. **Wave 2 — nghiệp vụ phụ thuộc (1 agent):** C (hóa đơn + báo cáo). Phải chờ A và B vì cần truy vấn phòng, hợp đồng Active và chỉ số điện nước.
3. **Wave 3 — tầng mạng (2 agent song song):** D (TCP Server + router) và E (TCP Client service + shell). E chỉ phụ thuộc `QuanLyTro.Shared` nên chạy được song song với D.
4. **Wave 4 — giao diện (1 agent):** F (8 màn hình). Chờ D và E.
5. **Wave 5 — chốt sổ (1 agent):** G (test acceptance + README + đánh dấu plan).

### Quy tắc an toàn cho worker

- **Ranh giới file:** mỗi worker chỉ sửa file trong mục **Files** của sub-plan mình. Thấy cần sửa file ngoài danh sách → dừng, báo Leader, không tự mở rộng.
- **Xung đột:** file dùng chung `BusinessRuleTests.cs` (Sub-plan A, B, C đều ghi). Chạy tuần tự trong cùng wave hoặc chỉ agent đầu tiên tạo file, các agent sau chỉ append test của mình.
- **Verify trước khi báo xong:** mỗi worker phải chạy `dotnet test` và dán output thật; không báo "PASS" mà không có log.
- **Commit riêng:** mỗi worker tự commit với message ghi rõ số Task, không gộp nhiều task vào một commit.
- **Không vượt quyền:** worker không sửa `DESIGN.md`, `PRODUCT.md`, spec, hay master plan — trừ Sub-plan G được phép đánh dấu checkbox trong master plan.

### Trạng thái sub-plan

- [x] A — Task 6 phần phòng: `RoomRepository` + `RoomService` + `BusinessRuleException` + test (commit `7af650a`). **Còn lại:** `TenantRepository`, `TenantService`, test BR-02/03.
- [ ] B — Task 7
- [ ] C — Task 8
- [ ] D — Task 9
- [ ] E — Task 10
- [ ] F — Task 11
- [ ] G — Task 12

---

## Sub-Plan A — Chi tiết mở rộng (Task 6)

Phần phòng đã xong. Phần người thuê còn lại tách tiếp thành 2 file nhỏ để 2 worker chạy song song được.

| Sub-plan | Nội dung | File | Phụ thuộc | Song song với |
|---|---|---|---|---|
| A1 | `TenantRepository` + `TenantService` (thêm/sửa/trả phòng/xóa) | [task-6a-tenant-repo-service.md](subplans/task-6a-tenant-repo-service.md) | Task 5 | A2 |
| A2 | Quy tắc sức chứa BR-02 + trạng thái phòng tự động | [task-6b-capacity-rules.md](subplans/task-6b-capacity-rules.md) | A1 | A1 |

**Lưu ý phối hợp A1/A2:** hai sub-plan này cùng chạm `TenantService.cs`. Nếu chạy song song thật, A1 tạo file trước, A2 chỉ sửa phần `AddAsync`/`CheckoutAsync` — hoặc chạy tuần tự A1 → A2 để tránh conflict. Khuyến nghị **tuần tự**.

### Chi tiết: Sub-Plan A1

**Files:**
- Create: `QuanLyTro/QuanLyTro.Server/Repositories/TenantRepository.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Services/TenantService.cs`
- Modify: `QuanLyTro/QuanLyTro.Server/Security/PasswordHasher.cs` (dùng lại, không sửa)
- Test: `QuanLyTro/QuanLyTro.Tests/TenantTests.cs`

**Interfaces:**
- Consumes: `Database`, `PasswordHasher.Hash`, `TenantDto`.
- Produces: `TenantService.GetByRoomAsync(int roomId)`, `AddAsync(TenantDto, string? plainPassword)`, `UpdateAsync(TenantDto, string? plainPassword)`, `CheckoutAsync(int tenantId)`, `DeleteAsync(int tenantId)`.

- [ ] **Step 1: Viết test thất bại — thêm người thuê vào phòng**
  - Test: thêm 1 tenant vào phòng có sức chứa 2 → `AddAsync` trả `TenantDto` có `Id > 0`.
  - Test: `GetByRoomAsync(roomId)` trả đúng 1 bản ghi.

- [ ] **Step 2: Chạy test xác nhận fail**
  - Lệnh: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter TenantService`

- [ ] **Step 3: Cài `TenantRepository`**
  - `GetByRoomAsync`: `SELECT ... FROM tenants WHERE room_id = @roomId ORDER BY full_name`.
  - `InsertAsync(TenantDto, string passwordHash)`: INSERT đủ cột, trả `LAST_INSERT_ID()`.
  - `UpdateAsync(TenantDto, string? passwordHash)`: UPDATE; chỉ đổi `password_hash` khi tham số khác null.
  - `CheckoutAsync(int tenantId)`: `UPDATE tenants SET room_id = NULL WHERE id = @id`.
  - `DeleteAsync(int tenantId)`: `DELETE FROM tenants WHERE id = @id AND room_id IS NULL` (US-06) — trả về false nếu chưa trả phòng.
  - `CountInRoomAsync(int roomId)`: `SELECT COUNT(*) FROM tenants WHERE room_id = @roomId`.
  - Bắt MySQL 1062 (CCCD trùng) → `BusinessRuleException("CCCD này đã có trong hệ thống.")`.

- [ ] **Step 4: Cài `TenantService` khung (chưa có BR-02)**
  - Validate cơ bản: họ tên không rỗng, CCCD đúng 12 chữ số, SĐT 10 chữ số, `DateOfBirth` không ở tương lai.
  - `AddAsync`: nếu `plainPassword` rỗng → băm 6 số cuối CCCD; nếu có → băm nguyên văn.
  - `DeleteAsync`: gọi repo; false → `BusinessRuleException("Chỉ xóa được người đã trả phòng.")`.

- [ ] **Step 5: Chạy test**
  - Lệnh: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter TenantService`
  - Expected: PASS.

- [ ] **Step 6: Commit**
  - Commit message: `feat: add tenant repository and service (Task 6-A1)`

### Chi tiết: Sub-Plan A2

**Files:**
- Modify: `QuanLyTro/QuanLyTro.Server/Services/TenantService.cs`
- Modify: `QuanLyTro/QuanLyTro.Server/Repositories/RoomRepository.cs`
- Test: `QuanLyTro/QuanLyTro.Tests/TenantTests.cs`

**Interfaces:**
- Consumes: `TenantRepository.CountInRoomAsync`, `RoomRepository`.
- Produces: hành vi BR-02 — vượt sức chứa thì `BusinessRuleException`; phòng tự chuyển `Rented`/`Available`.

- [ ] **Step 1: Viết test thất bại — vượt sức chứa**
  - Phòng sức chứa 2, đã có 2 người → `AddAsync` ném `BusinessRuleException` chứa chữ `"đủ sức chứa"`.

- [ ] **Step 2: Chạy test xác nhận fail**
  - Lệnh: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter TenantService_RejectsOverCapacity`

- [ ] **Step 3: Cài BR-02 trong transaction**
  - Mở transaction; `SELECT ... FOR UPDATE` khóa dòng phòng để tránh 2 client cùng thêm vượt sức chứa.
  - Đếm `CountInRoomAsync`; nếu `count >= room.MaxOccupants` → rollback + ném `BusinessRuleException($"Phòng {room.RoomNumber} đã đủ sức chứa.")`.
  - Insert tenant; `UPDATE rooms SET status = 'Rented'` nếu phòng đang `Available`; commit.

- [ ] **Step 4: Cài tự động trả trạng thái phòng khi checkout**
  - Sau `CheckoutAsync`: nếu phòng không còn tenant VÀ không có hợp đồng `Active` → `UPDATE rooms SET status = 'Available'`.

- [ ] **Step 5: Chạy test và verify trên MySQL thật**
  - Lệnh: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj"`
  - Verify tay: insert 3 tenant vào phòng sức chứa 2 qua MySQL CLI → dòng thứ 3 phải bị chặn.

- [ ] **Step 6: Commit**
  - Commit message: `feat: enforce room capacity and auto status (Task 6-A2)`

---

## Sub-Plan B — Chi tiết mở rộng (Task 7)

Tách Task 7 thành 2 file nhỏ vì hợp đồng và điện nước độc lập nhau.

| Sub-plan | Nội dung | File | Phụ thuộc | Song song với |
|---|---|---|---|---|
| B1 | `ContractRepository` + `ContractService` (BR-04/05/06, US-10/11) | [task-7a-contracts.md](subplans/task-7a-contracts.md) | Task 6 | B2 |
| B2 | `UtilityRepository` + `UtilityService` (BR-07/08, US-12/13) | [task-7b-utilities.md](subplans/task-7b-utilities.md) | Task 6 | B1 |

**Phối hợp:** B1 và B2 chạm file hoàn toàn khác nhau → **chạy song song an toàn**.

### Chi tiết: Sub-Plan B1

**Files:**
- Create: `QuanLyTro/QuanLyTro.Server/Repositories/ContractRepository.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Services/ContractService.cs`
- Test: `QuanLyTro/QuanLyTro.Tests/ContractTests.cs`

**Interfaces:**
- Consumes: `Database`, `ContractDto`, `RoomRepository`.
- Produces: `ContractService.CreateAsync(ContractDto)`, `TerminateAsync(int contractId, string? notes)`, `RenewAsync(int contractId, DateOnly newEndDate)`, `GetAllAsync()`.

- [ ] **Step 1: Viết test thất bại — BR-06 ngày hợp lệ**
  - `CreateAsync` với `EndDate <= StartDate` → `BusinessRuleException` chứa `"Ngày kết thúc"`.

- [ ] **Step 2: Chạy test xác nhận fail**
  - Lệnh: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter ContractService`

- [ ] **Step 3: Cài `ContractRepository`**
  - `HasActiveContractAsync(int roomId)`: `SELECT COUNT(*) FROM contracts WHERE room_id = @id AND status = 'Active'`.
  - `IsRepresentativeInRoomAsync(int tenantId, int roomId)`: kiểm tra `tenants.room_id = @roomId`.
  - `GetAllAsync()`: JOIN rooms + tenants, trả kèm `RoomNumber` và `RepresentativeName`, `ORDER BY end_date`.
  - `InsertAsync`, `TerminateAsync`, `RenewAsync(newEndDate)`.

- [ ] **Step 4: Cài `ContractService` với BR-04/05/06**
  - BR-06: `EndDate > StartDate`, ngược lại ném `BusinessRuleException("Ngày kết thúc phải sau ngày bắt đầu.")`.
  - BR-05: đại diện phải đang ở trong phòng → ném `BusinessRuleException("Người đại diện không ở trong phòng này.")`.
  - BR-04: phòng đã có HĐ Active → ném `BusinessRuleException("Phòng đã có hợp đồng đang hiệu lực.")`.
  - `RenewAsync`: `newEndDate > EndDate` hiện tại; chỉ HĐ `Active`.

- [ ] **Step 5: Chạy test**
  - Lệnh: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter Contract`

- [ ] **Step 6: Commit**
  - Commit message: `feat: add contract service with BR-04/05/06 (Task 7-B1)`

### Chi tiết: Sub-Plan B2

**Files:**
- Create: `QuanLyTro/QuanLyTro.Server/Repositories/UtilityRepository.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Services/UtilityService.cs`
- Test: `QuanLyTro/QuanLyTro.Tests/UtilityTests.cs`

**Interfaces:**
- Consumes: `Database`, `UtilityReadingDto`, `RoomQuery`.
- Produces: `UtilityService.GetPreviousAsync(int roomId)`, `RecordAsync(UtilityReadingDto)`.

- [ ] **Step 1: Viết test thất bại — BR-07 chỉ số tăng dần**
  - `RecordAsync` với `NewElectricity < OldElectricity` → `BusinessRuleException` chứa `"chỉ số"`.

- [ ] **Step 2: Chạy test xác nhận fail**
  - Lệnh: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter UtilityService`

- [ ] **Step 3: Cài `UtilityRepository`**
  - `GetLatestAsync(int roomId)`: `SELECT new_electricity, new_water FROM utility_readings WHERE room_id = @id ORDER BY billing_month DESC LIMIT 1`.
  - `InsertAsync(UtilityReadingDto)`: INSERT đủ cột; bắt MySQL 1062 → `BusinessRuleException($"Phòng đã chốt điện nước tháng {month}.")` (BR-08).

- [ ] **Step 4: Cài `UtilityService`**
  - `GetPreviousAsync`: chưa có bản ghi nào → trả `(0, 0)`.
  - `RecordAsync`: BR-07 (`newElec >= oldElec`, `newWater >= oldWater`), giá > 0, tháng đúng định dạng `yyyy-MM`.
  - BR-08 nhờ unique key ở DB, không cần kiểm tra trước (tránh race).

- [ ] **Step 5: Chạy test**
  - Lệnh: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter Utility`

- [ ] **Step 6: Commit**
  - Commit message: `feat: add utility reading service with BR-07/08 (Task 7-B2)`

---

## Sub-Plan C — Chi tiết mở rộng (Task 8)

| Sub-plan | Nội dung | File | Phụ thuộc | Song song với |
|---|---|---|---|---|
| C1 | `InvoiceRepository` + `InvoiceService` (BR-09/10/11/13) | [task-8a-invoices.md](subplans/task-8a-invoices.md) | B1, B2 | C2 |
| C2 | `ReportRepository` + `ReportService` (US-04/08/18/19) | [task-8b-reports.md](subplans/task-8b-reports.md) | A1, C1 | C1 |

**Phối hợp:** C1 và C2 chạm file khác nhau nhưng C2 cần bảng invoices đã có dữ liệu → **chạy song song được**, C2 chỉ đọc.

### Chi tiết: Sub-Plan C1

**Files:**
- Create: `QuanLyTro/QuanLyTro.Server/Repositories/InvoiceRepository.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Services/InvoiceService.cs`
- Test: `QuanLyTro/QuanLyTro.Tests/InvoiceTests.cs`

**Interfaces:**
- Consumes: `Database`, `CreateInvoiceRequest`, `InvoiceDto`, `ActionNames`.
- Produces: `InvoiceService.CreateAsync(CreateInvoiceRequest)`, `GetAllAsync(string billingMonth, int? roomId)`, `PayAsync(int invoiceId)`, `GetMineAsync(int tenantId)`.

- [ ] **Step 1: Viết test thất bại — BR-11 hóa đơn Paid bất biến**
  - `PayAsync(invoiceId)` gọi lần 2 trên hóa đơn đã `Paid` → `BusinessRuleException` chứa `"đã thanh toán"`.

- [ ] **Step 2: Chạy test xác nhận fail**
  - Lệnh: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter InvoiceService`

- [ ] **Step 3: Cài `InvoiceRepository`**
  - `GetActiveContractAsync(int roomId)`: trả `(contractId, rentalPrice)` của HĐ `Active`.
  - `GetReadingAsync(int roomId, string month)`: trả chỉ số + đơn giá của tháng.
  - `InsertAsync(...)`: bắt MySQL 1062 → `BusinessRuleException($"Tháng {month} đã lập hóa đơn cho phòng này.")`.
  - `GetAllAsync(string? month, int? roomId)`: cả hai tham số tùy chọn, ít nhất một (US-15, US-17).
  - `PayAsync(int invoiceId)`: `UPDATE invoices SET status='Paid', paid_at=NOW() WHERE id=@id AND status='Unpaid'` — trả số dòng bị ảnh hưởng.
  - `GetMineAsync(int tenantId)`: JOIN tenants để suy `room_id` từ token (BR-14).

- [ ] **Step 4: Cài `InvoiceService` với BR-09/10**
  - BR-09: thiếu HĐ Active hoặc chưa chốt điện nước → `BusinessRuleException("Phòng chưa có hợp đồng hiệu lực hoặc chưa chốt điện nước tháng này.")`.
  - BR-10: `Total = RentalPrice + (newElec-oldElec)*elecRate + (newWater-oldWater)*waterRate + OtherFees`. Dùng `decimal`, không `double`.
  - `PayAsync`: 0 dòng bị ảnh hưởng → `BusinessRuleException("Hóa đơn đã thanh toán hoặc không tồn tại.")` (BR-11).
  - BR-13: `CreateAsync` bọc trong transaction.

- [ ] **Step 5: Chạy test**
  - Lệnh: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter Invoice`

- [ ] **Step 6: Commit**
  - Commit message: `feat: add invoice service with BR-09/10/11/13 (Task 8-C1)`

### Chi tiết: Sub-Plan C2

**Files:**
- Create: `QuanLyTro/QuanLyTro.Server/Repositories/ReportRepository.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Services/ReportService.cs`
- Test: `QuanLyTro/QuanLyTro.Tests/ReportTests.cs`

**Interfaces:**
- Consumes: `Database`, `SummaryReportDto`, `ResidenceExportDto`.
- Produces: `ReportService.GetSummaryAsync(string billingMonth)`, `ExportResidenceAsync()`.

- [ ] **Step 1: Viết test thất bại — SummaryReportDto tính đúng số phòng trống**
  - DB rỗng → `TotalRooms = 0`, `AvailableRooms = 0`, `UnpaidAmount = 0m`.

- [ ] **Step 2: Chạy test xác nhận fail**
  - Lệnh: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter ReportService`

- [ ] **Step 3: Cài `ReportRepository`**
  - `GetSummaryAsync(month)`: 1 câu SQL trả tổng phòng, trống, đang thuê, số người, đã thu, còn nợ — dùng subquery, KHÔNG N+1.
  - `GetResidenceAsync()`: JOIN tenants + rooms, chỉ người đang ở (`room_id IS NOT NULL`), sắp theo số phòng.

- [ ] **Step 4: Cài `ReportService`**
  - `GetSummaryAsync`: map sang `SummaryReportDto`; tháng sai định dạng → `BusinessRuleException`.
  - `ExportResidenceAsync`: trả danh sách; Client tự dựng CSV.

- [ ] **Step 5: Chạy test**
  - Lệnh: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter Report`

- [ ] **Step 6: Commit**
  - Commit message: `feat: add report service (Task 8-C2)`

---

## Sub-Plan D/E — Chi tiết mở rộng (Task 9, 10)

| Sub-plan | Nội dung | File | Phụ thuộc | Song song với |
|---|---|---|---|---|
| D1 | `RequestRouter` + `PermissionMatrix` enforcement | [task-9a-router.md](subplans/task-9a-router.md) | Task 5, 8 | D2 |
| D2 | `TcpListenerServer` + `ClientHandler` đa kết nối | [task-9b-tcp-listener.md](subplans/task-9b-tcp-listener.md) | Task 5 | D1 |
| E | `TcpClientService` + shell `Form1` | [task-10-client-service-shell.md](subplans/task-10-client-service-shell.md) | Task 2 | D1, D2 |

**Phối hợp:** D1, D2, E chạm 3 nhóm file rời nhau → **3 agent chạy song song được**. D1 và D2 gặp nhau ở `Program.cs`; D2 giữ quyền sửa `Program.cs`, D1 chỉ tạo `RequestRouter.cs`.

### Chi tiết: Sub-Plan D1

**Files:**
- Create: `QuanLyTro/QuanLyTro.Server/Network/RequestRouter.cs`
- Test: `QuanLyTro/QuanLyTro.Tests/RouterTests.cs`

**Interfaces:**
- Consumes: `IAuthService`, `RoomService`, `TenantService`, `ContractService`, `UtilityService`, `InvoiceService`, `ReportService`, `SessionStore`, `PermissionMatrix`.
- Produces: `RequestRouter.HandleAsync(RequestPacket packet, CancellationToken ct)` → `ResponsePacket`.

- [ ] **Step 1: Viết test thất bại — Tenant gọi action của chủ trọ bị từ chối**
  - Gọi `ROOM_GET_ALL` với token vai Tenant → `Success = false`, `Message = "Không có quyền."`.

- [ ] **Step 2: Chạy test xác nhận fail**
  - Lệnh: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter Router_TenantCannotCallLandlordAction`

- [ ] **Step 3: Cài `RequestRouter`**
  - `AUTH_LOGIN` → đi thẳng `AuthService`, không cần token.
  - Mọi action khác: `SessionStore.TryGet(token)` → sai → `ResponsePacket.Fail("Phiên đăng nhập không hợp lệ.")`.
  - `PermissionMatrix.IsAllowed(action, role)` → false → `ResponsePacket.Fail("Không có quyền.")` (BR-14).
  - `switch` trên action gọi đúng service; bọc `BusinessRuleException` → `Fail(message)`.
  - Exception khác → log ra Console, trả `Fail("Lỗi hệ thống, vui lòng thử lại.")` — không lộ stack trace.

- [ ] **Step 4: Chạy test**
  - Lệnh: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter Router`

- [ ] **Step 5: Commit**
  - Commit message: `feat: add request router with permission enforcement (Task 9-D1)`

### Chi tiết: Sub-Plan D2

**Files:**
- Create: `QuanLyTro/QuanLyTro.Server/Network/TcpListenerServer.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Network/ClientHandler.cs`
- Modify: `QuanLyTro/QuanLyTro.Server/Program.cs`
- Test: `QuanLyTro/QuanLyTro.Tests/TcpRoundTripTests.cs`

**Interfaces:**
- Consumes: `RequestRouter`, `ServerOptions`.
- Produces: `TcpListenerServer.StartAsync(CancellationToken)`, `ClientHandler.ServeAsync(TcpClient, CancellationToken)`.

- [ ] **Step 1: Viết integration test round-trip thất bại**
  - Khởi động listener cổng ngẫu nhiên → gửi `AUTH_LOGIN` thô qua `TcpClient` → đọc response JSON, `Success` phải là `false` (tài khoản không tồn tại).

- [ ] **Step 2: Chạy test xác nhận fail**
  - Lệnh: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter TcpRoundTrip`

- [ ] **Step 3: Cài `ClientHandler`**
  - Mỗi kết nối một `async Task` độc lập — KHÔNG `new Thread`.
  - `StreamReader.ReadLineAsync()`; packet > 1 MiB → trả `Fail` rồi đóng.
  - `StreamWriter.WriteLineAsync()` với `AutoFlush = true`; dùng chung `JsonDefaults.Options`.
  - Bọc toàn bộ trong try/catch: một client hỏng không được sập server.

- [ ] **Step 4: Cài `TcpListenerServer` và nối `Program.cs`**
  - `TcpListener(IPAddress.Any, options.Port)`, vòng `AcceptTcpClientAsync`.
  - Ghi log mỗi kết nối: `[HH:mm:ss] Client connected from <endpoint>`.
  - `CancellationTokenSource` để Ctrl+C tắt êm.

- [ ] **Step 5: Chạy test và smoke-test**
  - Lệnh: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj"`
  - Chạy `dotnet run --project "QuanLyTro/QuanLyTro.Server"`, gửi 1 request bằng script, xác nhận response JSON.

- [ ] **Step 6: Commit**
  - Commit message: `feat: add concurrent TCP listener (Task 9-D2)`

---

## Sub-Plan F — Chi tiết mở rộng (Task 11)

Task 11 dài nhất (8 màn hình) → tách thành 3 file theo nhóm màn hình, chạy song song được.

| Sub-plan | Nội dung | File | Phụ thuộc | Song song với |
|---|---|---|---|---|
| F1 | `DashboardForm` + `RoomsForm` + `TenantsForm` | [task-11a-dashboard-rooms-tenants.md](subplans/task-11a-dashboard-rooms-tenants.md) | Task 10 | F2, F3 |
| F2 | `ContractsForm` + `UtilitiesForm` | [task-11b-contracts-utilities.md](subplans/task-11b-contracts-utilities.md) | Task 10 | F1, F3 |
| F3 | `InvoicesForm` + `ReportsForm` + `MyInvoicesForm` | [task-11c-invoices-reports-tenant.md](subplans/task-11c-invoices-reports-tenant.md) | Task 10 | F1, F2 |

**Phối hợp:** mỗi nhóm tạo file `Forms/*.cs` riêng. Cả 3 cùng cần `Form1.cs` để gắn tab → **giao `Form1.cs` cho F1**; F2 và F3 chỉ tạo UserControl, Leader gắn tab sau khi cả 3 xong (hoặc mỗi nhóm dùng partial class riêng).

### Chi tiết: Sub-Plan F1

**Files:**
- Create: `QuanLyTro/Forms/DashboardForm.cs` (+ `.Designer.cs`)
- Create: `QuanLyTro/Forms/RoomsForm.cs` (+ `.Designer.cs`)
- Create: `QuanLyTro/Forms/TenantsForm.cs` (+ `.Designer.cs`)
- Modify: `QuanLyTro/Form1.cs`, `QuanLyTro/Form1.Designer.cs`

**Interfaces:**
- Consumes: `TcpClientService`, `ActionNames.RoomGetAll`, `ActionNames.TenantGetByRoom`, `ActionNames.ReportSummary`, `ActionNames.ContractGetAll`, `ActionNames.InvoiceGetAll`.
- Produces: 3 `UserControl` công khai; `Form1` dựng tab theo `Role`.

- [ ] **Step 1: Tạo `DashboardForm`**
  - 4 thẻ KPI (tổng phòng, trống + tỷ lệ lấp đầy, người đang ở, còn nợ tháng).
  - 2 bảng: "Còn nợ — đôn đốc" (US-17) và "HĐ sắp hết hạn" (US-11), cột `Còn` tô `#D95D39` khi < 30 ngày.
  - Gọi 3 action một lần khi load; click dòng → raise event để `Form1` chuyển tab.

- [ ] **Step 2: Tạo `RoomsForm`**
  - `DataGridView` readonly 6 cột + toolbar Thêm/Sửa/Xóa/F5; nút Xóa có `MessageBox` xác nhận.
  - Reload grid sau mỗi thao tác thành công.

- [ ] **Step 3: Tạo `TenantsForm`**
  - ComboBox phòng + grid + input; ô mật khẩu có ghi chú "để trống = 6 số cuối CCCD".
  - 4 nút: Thêm/Sửa/Trả phòng/Xóa hồ sơ.

- [ ] **Step 4: Áp design system và accessibility**
  - Token `DESIGN.md`: nền `#101417`, card `#181C1F`, header grid `#1A2025` chữ `#767E88` in hoa, dòng chọn `#21262B` + dải trái 3px `#D95D39`.
  - `AccessibleName`, tab order, label liên kết, font `Segoe UI 9pt`.

- [ ] **Step 5: Gắn tab vào `Form1` và build**
  - `Form1` dựng `TabControl` theo `Role`: Landlord 7 tab, Tenant 1 tab.
  - Lệnh: `dotnet build "QuanLyTro/QuanLyTro.slnx"`

- [ ] **Step 6: Commit**
  - Commit message: `feat: add dashboard, rooms and tenants screens (Task 11-F1)`

### Chi tiết: Sub-Plan F2

**Files:**
- Create: `QuanLyTro/Forms/ContractsForm.cs` (+ `.Designer.cs`)
- Create: `QuanLyTro/Forms/UtilitiesForm.cs` (+ `.Designer.cs`)

**Interfaces:**
- Consumes: `TcpClientService`, `ActionNames.ContractCreate`, `ContractTerminate`, `ContractRenew`, `ContractGetAll`, `UtilityGetPrevious`, `UtilityRecord`.

- [ ] **Step 1: Tạo `ContractsForm`**
  - Chọn phòng, đại diện, ngày bắt đầu/kết thúc, giá thuê, tiền cọc. Nút Tạo/Chấm dứt/Gia hạn.
  - ComboBox lọc 4 mức: Tất cả / Đang hiệu lực / Sắp hết hạn ≤ 30 ngày / Đã thanh lý.
  - Cột `Còn` tô `#D95D39` khi < 30 ngày. Hiện message lỗi Server nguyên văn.

- [ ] **Step 2: Tạo `UtilitiesForm`**
  - Chọn phòng/tháng; `UTILITY_GET_PREVIOUS` điền chỉ số cũ vào ô `disabled`.
  - `NumericUpDown` cho chỉ số mới + đơn giá; preview tiền chỉ để UX (US-13), kết quả chính thức từ Server.

- [ ] **Step 3: Áp design system và accessibility**
  - Cùng token như F1; ô `disabled` nền `#12161A` chữ `#767E88`.

- [ ] **Step 4: Build và commit**
  - Lệnh: `dotnet build "QuanLyTro/QuanLyTro.slnx"`
  - Commit message: `feat: add contracts and utilities screens (Task 11-F2)`

### Chi tiết: Sub-Plan F3

**Files:**
- Create: `QuanLyTro/Forms/InvoicesForm.cs` (+ `.Designer.cs`)
- Create: `QuanLyTro/Forms/ReportsForm.cs` (+ `.Designer.cs`)
- Create: `QuanLyTro/Forms/MyInvoicesForm.cs` (+ `.Designer.cs`)

**Interfaces:**
- Consumes: `TcpClientService`, `ActionNames.InvoiceCreate`, `InvoiceGetAll`, `InvoicePay`, `InvoiceGetMine`, `ReportSummary`, `ExportResidence`.

- [ ] **Step 1: Tạo `InvoicesForm`**
  - Chọn tháng + phòng, list hóa đơn 8 cột, filter chưa thu. Nút Lập hóa đơn / Thu.
  - Dòng `Paid` khóa nút Thu (BR-11).

- [ ] **Step 2: Tạo `ReportsForm` và xuất CSV bằng stdlib**
  - KPI + bảng nhiều tháng (US-19: gọi `REPORT_SUMMARY` từng tháng rồi cộng dòng tổng).
  - `EXPORT_RESIDENCE` → `StreamWriter` UTF-8 BOM, escape bằng bọc `"` và nhân đôi `"`. KHÔNG thêm package Excel.

- [ ] **Step 3: Tạo `MyInvoicesForm` (người thuê)**
  - Thẻ hồ sơ + banner quá hạn + bảng quyết toán có dòng phụ chỉ số + khối tổng tiền + lịch sử.
  - Gọi `INVOICE_GET_MINE`. **Không có nút thao tác ghi.**

- [ ] **Step 4: Áp design system và accessibility**
  - Cùng token; banner quá hạn nền `#1F1715` viền `rgba(217,93,57,.40)`.

- [ ] **Step 5: Build và commit**
  - Lệnh: `dotnet build "QuanLyTro/QuanLyTro.slnx"`
  - Commit message: `feat: add invoices, reports and tenant screens (Task 11-F3)`

---

## Sub-Plan G — Chi tiết mở rộng (Task 12)

Giữ nguyên 1 file, xem [task-12-final-tests-docs.md](subplans/task-12-final-tests-docs.md) — 8 bước đã đủ nhỏ.

---

## Bảng tổng hợp: chạy tối đa 3 agent song song

| Wave | Agent 1 | Agent 2 | Agent 3 |
|---|---|---|---|
| 1 | A1 (tenant repo/service) | A2 (sức chứa BR-02) | — (tuần tự sau A1) |
| 2 | B1 (hợp đồng) | B2 (điện nước) | — |
| 3 | C1 (hóa đơn) | C2 (báo cáo) | — |
| 4 | D1 (router) | D2 (TCP listener) | E (client + shell) |
| 5 | F1 (dashboard/rooms/tenants) | F2 (contracts/utilities) | F3 (invoices/reports/tenant) |
| 6 | G (test cuối + README) | — | — |

**Tổng: 6 wave.** Mỗi wave chạy tối đa 3 agent, verify bằng `dotnet test` trước khi sang wave sau.
- [ ] C — Task 8
- [ ] D — Task 9
- [ ] E — Task 10
- [ ] F — Task 11
- [ ] G — Task 12

---

## Global Constraints
- Target `net8.0` cho Shared/Server/Tests; target `net8.0-windows` cho WinForms Client.
- Client không được tham chiếu `MySqlConnector` hoặc mở kết nối DB.
- Mọi quy tắc BR-01 đến BR-13 chạy tại Server; ràng buộc DB bảo vệ uniqueness và foreign keys.
- Mỗi packet JSON UTF-8 kết thúc bằng `\n`; giới hạn một dòng packet là 1 MiB.
- Mọi SQL nhận dữ liệu người dùng phải dùng parameter; không nối chuỗi SQL.
- Tiền dùng `decimal`; tháng hóa đơn dùng chuỗi chuẩn `yyyy-MM`; ngày dùng `DateOnly` trong DTO.
- Mật khẩu dùng PBKDF2 (`Rfc2898DeriveBytes.Pbkdf2`) với salt riêng; token phiên sinh bằng `RandomNumberGenerator`.
- Không thêm abstraction ngoài interfaces tại biên Repository/Network cần cho kiểm thử.
- **Giao diện:** mọi màu, cỡ chữ, bán kính bo góc lấy từ `DESIGN.md` ở gốc repo — không hardcode màu mới trong code C#. Bản mẫu trực quan: `docs/superpowers/mockups/wireframe-quanly-phongtro.html`.
- **Ánh xạ WinForms:** dùng `ColorTranslator.FromHtml("<hex trong DESIGN.md>")`; font `Segoe UI 9pt` cho chữ thường, `Consolas 9.5pt` cho số liệu/tiền tệ. Không dùng pill bo tròn.

---

## Bản đồ file

```text
QuanLyTro/
├── QuanLyTro.slnx
├── QuanLyTro.csproj                    # WinForms Client, nâng lên SDK-style .NET 8
├── Program.cs
├── Form1.cs / Form1.Designer.cs        # đổi thành shell chính, chưa đổi tên để giữ designer
├── Network/TcpClientService.cs
├── Protocol/ClientRequestException.cs
├── Forms/RoomsForm.cs (+ Designer)
├── Forms/TenantsForm.cs (+ Designer)
├── Forms/ContractsForm.cs (+ Designer)
├── Forms/UtilitiesForm.cs (+ Designer)
├── Forms/InvoicesForm.cs (+ Designer)
├── Forms/ReportsForm.cs (+ Designer)
├── QuanLyTro.Shared/
│   ├── QuanLyTro.Shared.csproj
│   ├── Models/*.cs
│   └── Protocol/{ActionNames,RequestPacket,ResponsePacket,JsonDefaults}.cs
├── QuanLyTro.Server/
│   ├── QuanLyTro.Server.csproj
│   ├── Program.cs
│   ├── appsettings.json
│   ├── Config/ServerOptions.cs
│   ├── Data/{Database,SchemaInitializer}.cs
│   ├── Security/{PasswordHasher,SessionStore}.cs
│   ├── Repositories/*.cs
│   ├── Services/*.cs
│   └── Network/{TcpServer,ClientHandler,RequestRouter}.cs
├── QuanLyTro.Tests/
│   ├── QuanLyTro.Tests.csproj
│   ├── ProtocolTests.cs
│   ├── BusinessRuleTests.cs
│   ├── PasswordHasherTests.cs
│   └── TcpRoundTripTests.cs
└── database/schema.sql
```

### Task 1: Nâng WinForms hiện có và tạo solution ba project

**Files:**
- Modify: `QuanLyTro/QuanLyTro.csproj`
- Modify: `QuanLyTro/Program.cs`
- Modify: `QuanLyTro/QuanLyTro.slnx`
- Create: `QuanLyTro/QuanLyTro.Shared/QuanLyTro.Shared.csproj`
- Create: `QuanLyTro/QuanLyTro.Server/QuanLyTro.Server.csproj`
- Create: `QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj`
- Delete: `QuanLyTro/App.config`
- Delete: `QuanLyTro/Properties/AssemblyInfo.cs`
- Delete: `QuanLyTro/Properties/Settings.settings`
- Delete: `QuanLyTro/Properties/Settings.Designer.cs`

**Interfaces:**
- Produces: buildable solution; Client references Shared, Server references Shared, Tests reference Shared and Server.

- [x] **Step 1: Thay project Client bằng SDK-style**
- [x] **Step 2: Hiện đại hóa entry point WinForms**
- [x] **Step 3: Tạo Shared, Server và Tests project**
- [x] **Step 4: Cập nhật `.slnx` và xóa metadata .NET Framework cũ**
- [x] **Step 5: Restore và build**
- [x] **Step 6: Commit**

### Task 2: Định nghĩa DTO và giao thức dùng chung

**Files:**
- Create: `QuanLyTro/QuanLyTro.Shared/Models/RoomDto.cs`
- Create: `QuanLyTro/QuanLyTro.Shared/Models/TenantDto.cs`
- Create: `QuanLyTro/QuanLyTro.Shared/Models/ContractDto.cs`
- Create: `QuanLyTro/QuanLyTro.Shared/Models/UtilityReadingDto.cs`
- Create: `QuanLyTro/QuanLyTro.Shared/Models/InvoiceDto.cs`
- Create: `QuanLyTro/QuanLyTro.Shared/Models/AuthAndReportDtos.cs`
- Create: `QuanLyTro/QuanLyTro.Shared/Protocol/ActionNames.cs`
- Create: `QuanLyTro/QuanLyTro.Shared/Protocol/RequestPacket.cs`
- Create: `QuanLyTro/QuanLyTro.Shared/Protocol/ResponsePacket.cs`
- Create: `QuanLyTro/QuanLyTro.Shared/Protocol/JsonDefaults.cs`
- Test: `QuanLyTro/QuanLyTro.Tests/ProtocolTests.cs`

**Interfaces:**
- Produces: `RequestPacket.Create<T>(string action, string? token, T data)`, `T GetData<T>()`, `ResponsePacket.Ok<T>()`, `ResponsePacket.Fail()`, `JsonDefaults.Options`.

- [x] **Step 1: Viết test protocol thất bại**
- [x] **Step 2: Chạy test để xác nhận fail**
- [x] **Step 3: Tạo DTO bất biến và enum**
- [x] **Step 4: Tạo packet giữ `JsonElement` thay vì JSON lồng dạng string**
- [x] **Step 5: Khai báo đủ 18 Action và các payload nhỏ**
- [x] **Step 6: Chạy test và toàn solution**
- [x] **Step 7: Commit**

### Task 3: Tạo schema MySQL và cấu hình Server

**Files:**
- Create: `QuanLyTro/database/schema.sql`
- Create: `QuanLyTro/QuanLyTro.Server/appsettings.json`
- Create: `QuanLyTro/QuanLyTro.Server/Config/ServerOptions.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Data/Database.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Data/SchemaInitializer.cs`
- Modify: `QuanLyTro/QuanLyTro.Server/QuanLyTro.Server.csproj`

**Interfaces:**
- Produces: `ServerOptions.Load(string path)`, `Database.OpenAsync(CancellationToken)`, `SchemaInitializer.InitializeAsync()`.

- [x] **Step 1: Viết test cấu hình thất bại**

```csharp
[TestMethod]
public void ServerOptions_RejectsInvalidPort()
{
    Assert.ThrowsException<InvalidDataException>(() =>
        ServerOptions.Validate(new ServerOptions("server=localhost", 0)));
}
```

- [x] **Step 2: Chạy test xác nhận fail**

```bash
dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter ServerOptions_RejectsInvalidPort
```

Expected: FAIL vì `ServerOptions` chưa tồn tại.

- [x] **Step 3: Tạo cấu hình JSON và copy khi build**

```json
{
  "ConnectionString": "Server=127.0.0.1;Port=3306;Database=quanly_phongtro_nhs;User ID=root;Password=;SslMode=None;",
  "Port": 8888
}
```

```xml
<ItemGroup>
  <None Update="appsettings.json"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></None>
</ItemGroup>
```

- [x] **Step 4: Tạo schema đúng BR-01, BR-03, BR-08 và FK**

Dùng SQL trong mục 3 của SRS; thêm `CHECK (price > 0)`, `CHECK (max_occupants > 0)`, `CHECK (end_date > start_date)`, `CHECK (new_electricity >= old_electricity)`, `CHECK (new_water >= old_water)`, index `(room_id,status)` cho hợp đồng và index `(status,billing_month)` cho hóa đơn. `SchemaInitializer` đọc embedded `schema.sql`, tách bằng marker `-- statement`, chạy tuần tự.

- [x] **Step 5: Tạo connection factory**

```csharp
public sealed class Database(string connectionString)
{
    public async Task<MySqlConnection> OpenAsync(CancellationToken ct = default)
    {
        var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync(ct);
        return connection;
    }
}
```

- [x] **Step 6: Chạy test và build Server**

```bash
dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter ServerOptions_RejectsInvalidPort
```

```bash
dotnet build "QuanLyTro/QuanLyTro.Server/QuanLyTro.Server.csproj"
```

Expected: PASS; build 0 errors.

- [x] **Step 7: Smoke-test schema với Laragon đang chạy**

```bash
dotnet run --project "QuanLyTro/QuanLyTro.Server" -- --initialize-only
```

Expected: `Database initialized.`; chạy lần hai vẫn thành công.

- [x] **Step 8: Commit**

```bash
git add QuanLyTro/database QuanLyTro/QuanLyTro.Server QuanLyTro/QuanLyTro.Tests
```

```bash
git commit -m "feat: add MySQL schema and server configuration"
```

### Task 4: Password, đăng nhập và session

**Files:**
- Create: `QuanLyTro/QuanLyTro.Server/Security/PasswordHasher.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Security/SessionStore.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Repositories/UserRepository.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Services/AuthService.cs`
- Test: `QuanLyTro/QuanLyTro.Tests/PasswordHasherTests.cs`

**Interfaces:**
- Produces: `PasswordHasher.Hash(string)`, `Verify(string,string)`, `SessionStore.Create(int)`, `TryGetUser(string,out int)`, `AuthService.LoginAsync(LoginRequest,CancellationToken)`.

- [x] **Step 1: Viết test hash thất bại**

```csharp
[TestMethod]
public void PasswordHash_VerifiesCorrectPasswordOnly()
{
    var hash = PasswordHasher.Hash("MatKhau!123");
    Assert.IsTrue(PasswordHasher.Verify("MatKhau!123", hash));
    Assert.IsFalse(PasswordHasher.Verify("sai", hash));
    Assert.AreNotEqual(hash, PasswordHasher.Hash("MatKhau!123"));
}
```

- [x] **Step 2: Chạy test xác nhận fail**

```bash
dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter PasswordHash_VerifiesCorrectPasswordOnly
```

- [x] **Step 3: Implement PBKDF2 và so sánh constant-time**

```csharp
private const int Iterations = 210_000;
public static string Hash(string password)
{
    ArgumentException.ThrowIfNullOrWhiteSpace(password);
    var salt = RandomNumberGenerator.GetBytes(16);
    var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, 32);
    return $"pbkdf2-sha256${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
}
```

`Verify` parse bốn phần, giới hạn iteration hợp lệ, derive lại 32 byte, dùng `CryptographicOperations.FixedTimeEquals`.

- [x] **Step 4: Tạo session in-memory hết hạn sau 8 giờ**

```csharp
public string Create(int userId)
{
    var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    sessions[token] = (userId, DateTimeOffset.UtcNow.AddHours(8));
    return token;
}
```

Dùng `ConcurrentDictionary`; xóa token hết hạn khi truy cập.

- [x] **Step 5: Tạo UserRepository và AuthService**

SQL: `SELECT id, password_hash, full_name FROM users WHERE username=@username LIMIT 1`. Nếu sai thông tin, trả cùng thông báo `Tên đăng nhập hoặc mật khẩu không đúng.`; không tiết lộ tài khoản tồn tại.

- [x] **Step 6: Chạy test**

```bash
dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter PasswordHash
```

Expected: PASS.

- [x] **Step 7: Commit**

```bash
git add QuanLyTro/QuanLyTro.Server/Security QuanLyTro/QuanLyTro.Server/Repositories/UserRepository.cs QuanLyTro/QuanLyTro.Server/Services/AuthService.cs QuanLyTro/QuanLyTro.Tests/PasswordHasherTests.cs
```

```bash
git commit -m "feat: add secure server authentication"
```

### Task 5: Phân vai người thuê (delta 2 tác nhân)

**Spec:** `docs/superpowers/specs/2026-09-10-quanly-phongtro-multi-actor-design.md`

**Files:**
- Modify: `QuanLyTro/database/schema.sql`
- Modify: `QuanLyTro/QuanLyTro.Shared/Protocol/ActionNames.cs`
- Modify: `QuanLyTro/QuanLyTro.Shared/Models/AuthAndReportDtos.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Security/SessionStore.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Services/AuthService.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Network/PermissionMatrix.cs`
- Test: `QuanLyTro/QuanLyTro.Tests/AuthTests.cs`

**Interfaces:**
- Consumes: `PasswordHasher.Verify`, `UserRepository`.
- Produces: `AuthService.LoginAsync(username, password)` → `LoginResultDto(Token, FullName, Role)`; `SessionStore.Get(token)` → `(int UserId, UserRole Role)?`; `PermissionMatrix.IsAllowed(string action, UserRole role)`.

- [x] **Step 1: Viết test đăng nhập 2 bảng thất bại**

```csharp
[TestMethod]
public async Task AuthService_LogsInTenantByCccd()
{
    var auth = new AuthService(new StubUserRepo(landlord: null), new StubTenantRepo("048203012345", "hash"));
    var r = await auth.LoginAsync("048203012345", "123456");
    Assert.AreEqual(UserRole.Tenant, r.Role);
}
```

- [x] **Step 2: Chạy test xác nhận fail**

```bash
dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter AuthService_LogsInTenantByCccd
```

- [x] **Step 3: Thêm cột `password_hash` vào `schema.sql`**

```sql
ALTER TABLE tenants ADD COLUMN password_hash VARCHAR(255) NULL AFTER id_card;
```

- [x] **Step 4: Khai báo 4 action mới và `Role` trong DTO**

Thêm vào `ActionNames`: `TenantDelete`, `ContractRenew`, `ContractGetAll`, `InvoiceGetMine`. Thêm `enum UserRole { Landlord, Tenant }` và trường `Role` vào `LoginResultDto`.

- [x] **Step 5: Cài `SessionStore` lưu `(UserId, Role)`**

Token sinh bằng `RandomNumberGenerator`. `SessionStore` là `ConcurrentDictionary<string, (int, UserRole)>` với `TryAdd`/`TryRemove`.

- [x] **Step 6: Cài `AuthService` 2 bảng + lockout 5 lần/1 phút**

Thử `users` trước, không khớp thì thử `tenants` theo `username = id_card`. Sai thông tin trả **cùng một thông báo** cho cả 2 bảng. Đếm sai **theo username** trong `ConcurrentDictionary`, reset khi thành công; quá 5 lần → từ chối 1 phút, thông báo thời gian còn lại. Đồng hồ injectable để test không phải chờ thật. Tenant có `password_hash IS NULL` → không đăng nhập được.

- [x] **Step 7: Cài `PermissionMatrix` (server-enforced, BR-14)**

Bảng tĩnh `Action → AllowedRoles`. Mọi action trừ `AUTH_LOGIN` kiểm tra token **và** role trước khi gọi service. Tenant gọi action khác → `Success=false`, `"Không có quyền."`. Client chỉ ẩn/hiện menu theo `Role` cho UX, không tự quyết định quyền.

- [x] **Step 8: Chạy tests**

```bash
dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj"
```

- [x] **Step 9: Commit**

```bash
git add QuanLyTro/database/schema.sql QuanLyTro/QuanLyTro.Shared QuanLyTro/QuanLyTro.Server/Security QuanLyTro/QuanLyTro.Server/Services QuanLyTro/QuanLyTro.Server/Network QuanLyTro/QuanLyTro.Tests/AuthTests.cs
```

```bash
git commit -m "feat: add tenant role, auth service and permission matrix"
```

### Task 6: Quản lý phòng và người thuê

**Files:**
- Create: `QuanLyTro/QuanLyTro.Server/Repositories/RoomRepository.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Repositories/TenantRepository.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Services/RoomService.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Services/TenantService.cs`
- Test: `QuanLyTro/QuanLyTro.Tests/BusinessRuleTests.cs`

**Interfaces:**
- Produces: CRUD phòng; list/add/update/checkout người thuê; `BusinessRuleException` chứa thông báo thân thiện.

- [ ] **Step 1: Viết test validation thuần thất bại**

```csharp
[DataTestMethod]
[DataRow("", 1000000, 2)]
[DataRow("P1", 0, 2)]
[DataRow("P1", 1000000, 0)]
public void RoomService_RejectsInvalidRoom(string number, double price, int capacity)
{
    Assert.ThrowsException<BusinessRuleException>(() =>
        RoomService.Validate(new RoomDto(0, number, (decimal)price, capacity, RoomStatus.Available, null, 0)));
}
```

- [ ] **Step 2: Chạy test xác nhận fail**

```bash
dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter RoomService_RejectsInvalidRoom
```

- [ ] **Step 3: Implement RoomRepository parameter hóa**

Cung cấp `GetAllAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`, `CanDeleteAsync`. `GetAllAsync` dùng `LEFT JOIN tenants t ON t.room_id=r.id`, `COUNT(t.id)`, `GROUP BY r.id`. Bắt MySQL duplicate key `1062`, chuyển thành `BusinessRuleException("Số phòng đã tồn tại.")`.

- [ ] **Step 4: Implement RoomService BR-01, BR-12**

Validation: số phòng trim/non-empty/max 20; giá > 0; sức chứa > 0. Xóa chỉ khi repository xác nhận không tenant và không active contract.

- [ ] **Step 5: Implement TenantRepository/Service BR-02, BR-03**

`TenantService.AddAsync` mở transaction, khóa room bằng `SELECT ... FOR UPDATE`, đếm tenant, từ chối nếu đủ; insert tenant parameter hóa; cập nhật room thành `Rented`; commit. `CheckoutAsync` set `room_id=NULL`; nếu count còn lại bằng 0 và không active contract, set room `Available`.

- [ ] **Step 6: Chạy unit tests và DB integration scenario**

```bash
dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj"
```

Chạy Server với DB test, gửi hai lệnh thêm cùng `RoomNumber`; Expected: một success, một `Số phòng đã tồn tại.`.

- [ ] **Step 7: Commit**

```bash
git add QuanLyTro/QuanLyTro.Server/Repositories QuanLyTro/QuanLyTro.Server/Services QuanLyTro/QuanLyTro.Tests/BusinessRuleTests.cs
```

```bash
git commit -m "feat: add room and tenant business logic"
```

### Task 7: Hợp đồng và điện nước

**Files:**
- Create: `QuanLyTro/QuanLyTro.Server/Repositories/ContractRepository.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Repositories/UtilityRepository.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Services/ContractService.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Services/UtilityService.cs`
- Modify: `QuanLyTro/QuanLyTro.Tests/BusinessRuleTests.cs`

**Interfaces:**
- Produces: create/terminate contract; get previous/record utility; calculation helpers.

- [ ] **Step 1: Viết test BR-06/BR-07 và công thức điện nước**

```csharp
[TestMethod]
public void Contract_RequiresEndAfterStart() =>
    Assert.ThrowsException<BusinessRuleException>(() => ContractService.Validate(
        new ContractDto(0, 1, 1, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 1), 2_000_000, 0, ContractStatus.Active, null)));

[TestMethod]
public void Utility_CalculatesAmounts()
{
    var reading = new UtilityReadingDto(0, 1, "2026-09", 100, 125, 3500, 10, 13, 10000);
    Assert.AreEqual(87_500m, UtilityService.ElectricityAmount(reading));
    Assert.AreEqual(30_000m, UtilityService.WaterAmount(reading));
}
```

- [ ] **Step 2: Chạy test xác nhận fail**

```bash
dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter "Contract|Utility"
```

- [ ] **Step 3: Implement ContractService BR-04, BR-05, BR-06, BR-13**

Trong transaction: khóa room; kiểm tra representative có `room_id` đúng; kiểm tra không active contract; insert contract; cập nhật room `Rented`; commit. Terminate chỉ active contract, lưu lý do bắt buộc, set `Terminated`.

- [ ] **Step 4: Implement UtilityService BR-07, BR-08**

Validate tháng bằng `DateOnly.TryParseExact(value + "-01", "yyyy-MM-dd", ...)`; chỉ số/rate không âm; new >= old. `GetPreviousAsync` lấy record tháng mới nhất nhỏ hơn tháng yêu cầu. Duplicate `(room_id,billing_month)` map thành thông báo rõ.

- [ ] **Step 5: Chạy tests**

```bash
dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj"
```

Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add QuanLyTro/QuanLyTro.Server/Repositories/ContractRepository.cs QuanLyTro/QuanLyTro.Server/Repositories/UtilityRepository.cs QuanLyTro/QuanLyTro.Server/Services/ContractService.cs QuanLyTro/QuanLyTro.Server/Services/UtilityService.cs QuanLyTro/QuanLyTro.Tests/BusinessRuleTests.cs
```

```bash
git commit -m "feat: add contracts and utility readings"
```

### Task 8: Hóa đơn, thanh toán và báo cáo

**Files:**
- Create: `QuanLyTro/QuanLyTro.Server/Repositories/InvoiceRepository.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Repositories/ReportRepository.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Services/InvoiceService.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Services/ReportService.cs`
- Modify: `QuanLyTro/QuanLyTro.Tests/BusinessRuleTests.cs`

**Interfaces:**
- Produces: create/list/pay invoices; summary and temporary residence rows.

- [ ] **Step 1: Viết test BR-10 thất bại**

```csharp
[TestMethod]
public void Invoice_TotalEqualsAllComponents()
{
    Assert.AreEqual(2_167_500m, InvoiceService.CalculateTotal(2_000_000m, 87_500m, 30_000m, 50_000m));
    Assert.ThrowsException<BusinessRuleException>(() => InvoiceService.CalculateTotal(1, 0, 0, -2));
}
```

- [ ] **Step 2: Chạy test xác nhận fail**

```bash
dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter Invoice_TotalEqualsAllComponents
```

- [ ] **Step 3: Implement tạo hóa đơn BR-08 đến BR-10, BR-13**

Trong transaction: khóa room và utility record; lấy active contract; từ chối thiếu contract/reading; tính từng khoản server-side; insert invoice với unique room/month; commit. Không nhận amount điện/nước/tổng từ Client.

- [ ] **Step 4: Implement thanh toán BR-11**

SQL atomic: `UPDATE invoices SET status='Paid', paid_at=UTC_TIMESTAMP() WHERE id=@id AND status='Unpaid'`; affected rows phải là 1. Không cung cấp endpoint update/delete hóa đơn.

- [ ] **Step 5: Implement báo cáo**

`SummaryReportDto`: total rooms, available/rented rooms, current tenants, paid/unpaid amount theo tháng. `ResidenceExportDto`: full name, DOB, ID card, hometown, room number; query chỉ tenant có room.

- [ ] **Step 6: Chạy tests**

```bash
dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj"
```

Expected: PASS.

- [ ] **Step 7: Commit**

```bash
git add QuanLyTro/QuanLyTro.Server/Repositories/InvoiceRepository.cs QuanLyTro/QuanLyTro.Server/Repositories/ReportRepository.cs QuanLyTro/QuanLyTro.Server/Services/InvoiceService.cs QuanLyTro/QuanLyTro.Server/Services/ReportService.cs QuanLyTro/QuanLyTro.Tests/BusinessRuleTests.cs
```

```bash
git commit -m "feat: add invoices payments and reports"
```

### Task 9: TCP Server, router và xử lý nhiều Client

**Files:**
- Create: `QuanLyTro/QuanLyTro.Server/Network/TcpServer.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Network/ClientHandler.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Network/RequestRouter.cs`
- Modify: `QuanLyTro/QuanLyTro.Server/Program.cs`
- Test: `QuanLyTro/QuanLyTro.Tests/TcpRoundTripTests.cs`

**Interfaces:**
- Produces: `TcpServer.RunAsync(CancellationToken)`, `RequestRouter.RouteAsync(RequestPacket,CancellationToken)`.

- [ ] **Step 1: Viết TCP round-trip test thất bại**

```csharp
[TestMethod]
public async Task Server_ReturnsOneJsonResponsePerRequestLine()
{
    var router = new StubRouter(ResponsePacket.Ok(new { Pong = true }));
    await using var server = await TestTcpServer.StartAsync(router);
    using var client = new TcpClient();
    await client.ConnectAsync(IPAddress.Loopback, server.Port);
    using var writer = new StreamWriter(client.GetStream(), new UTF8Encoding(false)) { AutoFlush = true };
    using var reader = new StreamReader(client.GetStream(), Encoding.UTF8);
    await writer.WriteLineAsync("{\"Action\":\"PING\",\"Token\":null,\"Data\":{}}");
    var response = JsonSerializer.Deserialize<ResponsePacket>(await reader.ReadLineAsync(), JsonDefaults.Options)!;
    Assert.IsTrue(response.Success);
}
```

- [ ] **Step 2: Chạy test xác nhận fail**

```bash
dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter Server_ReturnsOneJsonResponsePerRequestLine
```

- [ ] **Step 3: Implement accept loop không tạo raw Thread**

```csharp
while (!ct.IsCancellationRequested)
{
    var client = await listener.AcceptTcpClientAsync(ct);
    _ = HandleSafelyAsync(client, ct);
}
```

Theo yêu cầu đa Client, mỗi connection có một async Task độc lập. Không dùng `new Thread`; async I/O nhẹ và vẫn đáp ứng độc lập.

- [ ] **Step 4: Implement ClientHandler an toàn**

Dùng UTF-8 strict; `ReadLineAsync(ct)`; nếu dòng > 1 MiB, phản hồi lỗi rồi đóng; deserialize lỗi trả `JSON không hợp lệ.`; mọi request nhận đúng một response line; log exception server-side, không gửi stack trace cho Client.

- [ ] **Step 5: Implement RequestRouter đủ 18 action**

`AUTH_LOGIN` không cần token. Mọi action khác kiểm tra `SessionStore.TryGetUser`; switch Action gọi đúng service; action lạ trả `Hành động không được hỗ trợ.`; `BusinessRuleException` thành `Success=false` và message; exception DB thành thông báo chung.

- [ ] **Step 6: Wire Program và Ctrl+C cancellation**

Load config; `--initialize-only` chạy schema rồi exit; mode thường initialize schema, tạo repositories/services/router/server, lắng nghe `IPAddress.Any:8888`; Ctrl+C cancel token và dừng sạch.

- [ ] **Step 7: Chạy TCP tests và tải đồng thời**

```bash
dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter Tcp
```

Thêm test mở 20 Client đồng thời gửi `ROOM_GET_ALL`; Expected: 20 response hợp lệ, không treo trong 5 giây.

- [ ] **Step 8: Commit**

```bash
git add QuanLyTro/QuanLyTro.Server/Network QuanLyTro/QuanLyTro.Server/Program.cs QuanLyTro/QuanLyTro.Tests/TcpRoundTripTests.cs
```

```bash
git commit -m "feat: add concurrent TCP server and request routing"
```

### Task 10: TCP Client service và kết nối UI

**Files:**
- Create: `QuanLyTro/Network/TcpClientService.cs`
- Create: `QuanLyTro/Protocol/ClientRequestException.cs`
- Modify: `QuanLyTro/Form1.cs`
- Modify: `QuanLyTro/Form1.Designer.cs`
- Test: `QuanLyTro/QuanLyTro.Tests/TcpRoundTripTests.cs`

**Interfaces:**
- Produces: `ConnectAsync(host,port,ct)`, `SendAsync<TRequest,TResponse>(action,data,ct)`, `Disconnected` event, `Token` property.

- [ ] **Step 1: Viết integration test client-server thất bại**

```csharp
[TestMethod]
public async Task TcpClientService_DeserializesTypedResponse()
{
    await using var server = await TestTcpServer.StartAsync(new StubRouter(
        ResponsePacket.Ok(new[] { new RoomDto(1, "P1", 1_000_000, 2, RoomStatus.Available, null, 0) })));
    await using var client = new TcpClientService();
    await client.ConnectAsync("127.0.0.1", server.Port, CancellationToken.None);
    var rooms = await client.SendAsync<object, List<RoomDto>>(ActionNames.RoomGetAll, new { }, CancellationToken.None);
    Assert.AreEqual("P1", rooms[0].RoomNumber);
}
```

- [ ] **Step 2: Chạy test xác nhận fail**

```bash
dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter TcpClientService_DeserializesTypedResponse
```

- [ ] **Step 3: Implement Client service với một request tại một thời điểm**

Dùng một connection lâu dài, `SemaphoreSlim(1,1)` quanh write/read pair để response không lệch request; timeout 10 giây bằng linked `CancellationTokenSource`; EOF phát `Disconnected`; `Success=false` ném `ClientRequestException(Message)`.

- [ ] **Step 4: Biến Form1 thành shell chính**

Đọc địa chỉ Server từ `App.config` (`127.0.0.1:8888`), **không có ô nhập IP/Port trên giao diện** — người dùng cuối không cấu hình hạ tầng. `Form1` gồm:
- Title bar 32px nền `#0B0F12`, icon Terracotta `#D95D39`.
- Menu strip 36px: Hệ thống · Quản lý · Báo cáo · Trợ giúp.
- Vùng đăng nhập: segmented 2 vai (Chủ trọ / Khách thuê), ô tài khoản + mật khẩu, nút `Đăng nhập`.
- Sau khi đăng nhập: `TabControl` dựng tab theo `Role` trả về (Landlord 7 tab, Tenant 1 tab).
- Status strip đáy 22px: vai hiện tại + `.NET 8.0 CLR` (không hiển thị IP/latency ra UI).

Event handler async, bắt exception bằng `MessageBox`, không block UI thread. Token màu/chữ lấy từ `DESIGN.md`.

- [ ] **Step 5: Chạy tests và mở Designer**

```bash
dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj"
```

Mở `Form1.cs [Design]` trong Visual Studio; Expected: designer tải không lỗi. Chạy Server + Client rồi đăng nhập; Expected: `TabControl` hiện đúng số tab theo vai (chủ trọ 7, người thuê 1).

- [ ] **Step 6: Commit**

```bash
git add QuanLyTro/Network QuanLyTro/Protocol QuanLyTro/Form1.cs QuanLyTro/Form1.Designer.cs QuanLyTro/QuanLyTro.Tests/TcpRoundTripTests.cs
```

```bash
git commit -m "feat: connect WinForms client over TCP"
```

### Task 11: Các màn hình nghiệp vụ WinForms

**Files:**
- Create: `QuanLyTro/Forms/RoomsForm.cs` and `.Designer.cs`
- Create: `QuanLyTro/Forms/TenantsForm.cs` and `.Designer.cs`
- Create: `QuanLyTro/Forms/ContractsForm.cs` and `.Designer.cs`
- Create: `QuanLyTro/Forms/UtilitiesForm.cs` and `.Designer.cs`
- Create: `QuanLyTro/Forms/InvoicesForm.cs` and `.Designer.cs`
- Create: `QuanLyTro/Forms/ReportsForm.cs` and `.Designer.cs`
- Modify: `QuanLyTro/Form1.cs`
- Modify: `QuanLyTro/Form1.Designer.cs`

**Interfaces:**
- Consumes: singleton `TcpClientService` từ Form1.
- Produces: UI cho US-01 đến US-22; không chứa business-rule decisions.

- [ ] **Step 0: Tạo tab `Tổng quan` (Dashboard)**

Tab đầu tiên của `Landlord`, gọi song song `REPORT_SUMMARY` + `CONTRACT_GET_ALL` + `INVOICE_GET_ALL` một lần khi mở tab.
- 4 thẻ KPI: Tổng số phòng · Phòng trống (kèm tỷ lệ lấp đầy) · Người đang ở · Còn nợ tháng hiện tại.
- Bảng "Còn nợ — đôn đốc" (US-17): phòng, kỳ, số còn nợ tô `#D95D39`, đại diện.
- Bảng "HĐ sắp hết hạn" (US-11): phòng, đại diện, ngày hết hạn, số ngày còn lại.
- Không có nút ghi; click dòng điều hướng sang tab tương ứng.

- [ ] **Step 1: Tạo `RoomsForm`**

DataGridView readonly hiển thị số phòng, giá, sức chứa, số người, trạng thái; controls add/update; nút delete có `MessageBox` xác nhận; mọi thao tác gọi `ROOM_*`, sau success reload grid.

- [ ] **Step 2: Tạo `TenantsForm`**

ComboBox phòng; grid tenant; inputs bắt buộc; Add/Update/Checkout gọi `TENANT_*`; Client chỉ kiểm tra trường rỗng/định dạng ngày, Server quyết định capacity và CCCD uniqueness.

- [ ] **Step 3: Tạo `ContractsForm`**

Chọn room, representative, dates, rental/deposit; Create/Terminate; hiển thị thông báo lỗi Server. Thêm tab `Sắp hết hạn` lọc client-side từ dữ liệu Server trong 30 ngày; nếu protocol thiếu list contract, bổ sung action `CONTRACT_GET_ALL` vào spec/protocol trước khi code.

- [ ] **Step 4: Tạo `UtilitiesForm`**

Chọn room/month; gọi `UTILITY_GET_PREVIOUS` điền chỉ số cũ; NumericUpDown cho chỉ số mới và rates; preview tiền chỉ để UX, kết quả chính thức lấy từ Server khi `UTILITY_RECORD`.

- [ ] **Step 5: Tạo `InvoicesForm`**

Chọn tháng, list invoices và detail breakdown; nút Create và Pay; paid rows khóa action; filter unpaid. Server tính toàn bộ amount ngoại trừ `OtherFees`.

- [ ] **Step 6: Tạo `ReportsForm` và CSV bằng stdlib**

Gọi `REPORT_SUMMARY`; hiển thị KPI và bảng tháng. Gọi `EXPORT_RESIDENCE`; dùng `StreamWriter` UTF-8 BOM, escape CSV bằng cách bọc dấu `"` và nhân đôi dấu `"` bên trong. Không thêm Excel package.

- [ ] **Step 7: Gắn forms vào tab shell, áp design system, kiểm tra accessibility cơ bản**

Áp token từ `DESIGN.md` cho mọi form: nền `#101417`, card `#181C1F`, DataGridView header `#1A2025` chữ `#767E88` in hoa, dòng đang chọn nền `#21262B` + dải trái 3px `#D95D39`, cột số canh phải + `tabular-nums`, tag trạng thái sage/terracotta/amber theo bảng ngữ nghĩa, nút chính `#D95D39` chữ trắng, góc bo 4px/8px. Không dùng pill.

Đặt `AccessibleName`, tab order hợp lý, labels liên kết controls, font `Segoe UI 9pt` (số liệu `Consolas 9.5pt`), button có text rõ; keyboard navigation hoạt động; lỗi hiển thị bằng MessageBox và giữ input.

- [ ] **Step 8: Build và manual flow**

```bash
dotnet build "QuanLyTro/QuanLyTro.slnx"
```

Chạy flow: phòng trống → thêm tenant → tạo contract → chốt utility → tạo invoice → pay → report; Expected: trạng thái đúng từng bước, Client không treo.

- [ ] **Step 9: Commit**

```bash
git add QuanLyTro/Forms QuanLyTro/Form1.cs QuanLyTro/Form1.Designer.cs
```

```bash
git commit -m "feat: add WinForms management screens"
```

### Task 12: Hoàn thiện kiểm thử SRS và tài liệu chạy

**Files:**
- Create: `QuanLyTro/README.md`
- Create: `QuanLyTro/docs/test-cases.md`
- Modify: `QuanLyTro/QuanLyTro.Tests/BusinessRuleTests.cs`
- Modify: `QuanLyTro/QuanLyTro.Tests/TcpRoundTripTests.cs`

**Interfaces:**
- Produces: hướng dẫn setup/reproducible acceptance checklist; bằng chứng US/BR coverage.

- [ ] **Step 1: Thêm test matrix BR-01 đến BR-13**

Mỗi BR có ít nhất một happy path và một rejection path. Với BR cần DB, dùng database riêng `quanly_phongtro_nhs_test`, reset tables trong test setup; không chạy vào DB dev.

- [ ] **Step 2: Thêm test mất kết nối và malformed input**

Test Server đóng connection khi Client đang chờ; Expected: `IOException`/`ClientRequestException`, event `Disconnected` một lần. Test JSON lỗi, action lạ, packet >1 MiB; Server sống và Client kế tiếp vẫn kết nối được.

- [ ] **Step 3: Thêm concurrent uniqueness test US-22**

Hai Client gửi `ROOM_ADD` cùng `RoomNumber` qua `Task.WhenAll`; assert đúng một `Success=true`, một `Success=false`; DB có đúng một row.

- [ ] **Step 4: Viết README**

Bao gồm prerequisites: Visual Studio 2022 17.8+ với `.NET desktop development`, .NET 8 SDK, Laragon/MySQL; cách đổi connection string; initialize schema; seed admin bằng utility CLI có hash PBKDF2; startup multiple projects; port/firewall LAN; command build/test.

- [ ] **Step 5: Viết acceptance checklist ánh xạ US-01–US-22 và BR-01–BR-13**

Mỗi dòng gồm mã, bước chuẩn bị, thao tác, kết quả mong đợi, trạng thái. Không thêm US-23 vì báo cáo đánh dấu Could; ghi rõ deferred.

- [ ] **Step 6: Chạy full verification**

```bash
dotnet format "QuanLyTro/QuanLyTro.slnx" --verify-no-changes
```

```bash
dotnet build "QuanLyTro/QuanLyTro.slnx" -c Release --no-restore
```

```bash
dotnet test "QuanLyTro/QuanLyTro.slnx" -c Release --no-build
```

Expected: format sạch, build 0 warnings/errors, tất cả tests PASS.

- [ ] **Step 7: Chạy acceptance flow LAN**

Server máy chính; hai Client từ hai tiến trình/máy; hoàn thành luồng tháng và concurrent duplicate room; đo request LAN <500 ms trong điều kiện lab.

- [ ] **Step 8: Commit**

```bash
git add QuanLyTro/README.md QuanLyTro/docs QuanLyTro/QuanLyTro.Tests
```

```bash
git commit -m "test: verify SRS acceptance scenarios"
```
