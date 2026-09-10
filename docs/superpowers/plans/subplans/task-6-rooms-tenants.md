# Sub-Plan A: Quản Lý Phòng & Người Thuê (Task 6)

> **Agent Worker:** Subagent A (Chạy độc lập sau khi Task 3-5 đã DONE).
> **Phối hợp:** Thuộc nhóm Wave 1, có thể chạy song song với Sub-Plan B (Task 7) trên tầng nghiệp vụ riêng.
> **Master Plan:** `docs/superpowers/plans/2026-09-09-quanly-phongtro-net8.md`

**Goal:** Triển khai tầng Repository và Service cho danh mục phòng (BR-01, BR-12) và người thuê trọ (BR-02, BR-03, BR-14, US-06).

**Files:**
- Create/Modify: `QuanLyTro/QuanLyTro.Server/Repositories/RoomRepository.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Repositories/TenantRepository.cs`
- Create/Modify: `QuanLyTro/QuanLyTro.Server/Services/BusinessRuleException.cs`
- Create/Modify: `QuanLyTro/QuanLyTro.Server/Services/RoomService.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Services/TenantService.cs`
- Test: `QuanLyTro/QuanLyTro.Tests/BusinessRuleTests.cs`

**Interfaces:**
- Produces: 
  - `RoomService`: `GetAllAsync()`, `AddAsync(RoomDto)`, `UpdateAsync(RoomDto)`, `DeleteAsync(int roomId)`.
  - `TenantService`: `GetByRoomAsync(int roomId)`, `AddAsync(TenantDto, string? plainPassword)`, `UpdateAsync(TenantDto, string? plainPassword)`, `CheckoutAsync(int tenantId)`, `DeleteAsync(int tenantId)`.
  - `BusinessRuleException`: Message tiếng Việt thân thiện, không ném lỗi kỹ thuật MySQL.

## Các bước thực hiện

- [ ] **Step 1: Viết test validation nghiệp vụ phòng thất bại**
  - Validation: Số phòng không rỗng, giá > 0, sức chứa > 0 và <= 50.
  - File: `QuanLyTro/QuanLyTro.Tests/BusinessRuleTests.cs`

- [ ] **Step 2: Chạy test xác nhận fail**
  - Lệnh: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter RoomService_RejectsInvalidRoom`

- [ ] **Step 3: Hoàn thiện `RoomRepository` & `RoomService`**
  - Mọi câu lệnh SQL parameter hóa (chống SQL Injection).
  - Bắt mã lỗi MySQL 1062 chuyển thành `BusinessRuleException("Số phòng đã tồn tại.")`.
  - `CanDeleteAsync`: kiểm tra phòng không còn tenant và không còn hợp đồng `Active` (BR-12).

- [ ] **Step 4: Hoàn thiện `TenantRepository` & `TenantService`**
  - Thêm người thuê (BR-02): mở Transaction, khóa phòng bằng `SELECT ... FOR UPDATE`, đếm số lượng người hiện tại. Nếu đủ, ném `BusinessRuleException("Phòng đã đủ sức chứa.")`.
  - Nếu thành công, tự động cập nhật trạng thái phòng sang `Rented`.
  - Mật khẩu: nếu `plainPassword` null/rỗng, tự băm 6 số cuối CCCD (delta 2 tác nhân).
  - Trả phòng (`CheckoutAsync`): gán `room_id = NULL`. Nếu phòng không còn ai và không còn HĐ Active, trả trạng thái phòng về `Available`.
  - Xóa người thuê (`DeleteAsync` - US-06): chỉ cho phép xóa khi `room_id IS NULL` (đã checkout).

- [ ] **Step 5: Chạy unit tests và integration scenario**
  - Lệnh: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj"`
  - Yêu cầu: Build sạch, mọi test liên quan đến Room và Tenant PASS.

- [ ] **Step 6: Commit**
  - Commit message: `feat: add room and tenant business logic (Task 6)`
