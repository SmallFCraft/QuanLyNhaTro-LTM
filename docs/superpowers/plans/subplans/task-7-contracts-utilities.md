# Sub-Plan B: Hợp Đồng & Chỉ Số Điện Nước (Task 7)

> **Agent Worker:** Subagent B (Chạy song song hoặc ngay sau Task 6).
> **Phối hợp:** Thuộc nhóm Wave 1, chịu trách nhiệm tầng nghiệp vụ Hợp đồng và Điện nước.
> **Master Plan:** `docs/superpowers/plans/2026-09-09-quanly-phongtro-net8.md`

**Goal:** Triển khai Repository và Service cho Hợp đồng thuê (BR-04, BR-05, BR-06, US-10, US-11) và Chỉ số điện nước hàng tháng (BR-07, BR-08, US-12, US-13).

**Files:**
- Create: `QuanLyTro/QuanLyTro.Server/Repositories/ContractRepository.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Repositories/UtilityRepository.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Services/ContractService.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Services/UtilityService.cs`
- Modify: `QuanLyTro/QuanLyTro.Tests/BusinessRuleTests.cs`

**Interfaces:**
- Produces:
  - `ContractService`: `CreateAsync(ContractDto)`, `TerminateAsync(int contractId, string? notes)`, `RenewAsync(int contractId, DateOnly newEndDate)`, `GetAllAsync()`.
  - `UtilityService`: `GetPreviousReadingAsync(int roomId)`, `RecordAsync(UtilityReadingDto)`.

## Các bước thực hiện

- [ ] **Step 1: Viết test validation nghiệp vụ hợp đồng và điện nước**
  - Hợp đồng: `EndDate > StartDate` (BR-06), kiểm tra đại diện phải là thành viên trong phòng (BR-05), tối đa 1 hợp đồng Active / phòng (BR-04).
  - Gia hạn (US-10): `newEndDate > oldEndDate`.
  - Điện nước: `newElec >= oldElec`, `newWater >= oldWater` (BR-07). Mỗi tháng chốt 1 lần (BR-08).

- [ ] **Step 2: Chạy test xác nhận fail**
  - Lệnh: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter ContractService`

- [ ] **Step 3: Cài đặt `ContractRepository` & `ContractService`**
  - Lập hợp đồng: kiểm tra phòng có HĐ Active chưa (BR-04). Đại diện ký HĐ phải đang ở phòng đó (BR-05).
  - Lấy tất cả HĐ (`GetAllAsync`): trả về kèm số phòng + tên đại diện, xếp theo `end_date` tăng dần (phục vụ US-11 đôn đốc hết hạn).

- [ ] **Step 4: Cài đặt `UtilityRepository` & `UtilityService`**
  - `GetPreviousReadingAsync`: lấy chỉ số mới nhất của phòng đó để làm chỉ số cũ kỳ này.
  - `RecordAsync`: validate số mới >= số cũ; insert vào DB. Bắt duplicate `uq_room_month` chuyển thành `BusinessRuleException("Phòng này đã chốt điện nước tháng này.")` (BR-08).

- [ ] **Step 5: Chạy test kiểm chứng**
  - Lệnh: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj"`

- [ ] **Step 6: Commit**
  - Commit message: `feat: add contract and utility services (Task 7)`
