# Sub-Plan G: Kiểm Thử Cuối & Tài Liệu (Task 12)

> **Agent Worker:** Subagent G (Chạy CUỐI CÙNG, sau Wave 4).
> **Phối hợp:** Wave 5, chốt sổ — verify toàn hệ thống khớp SRS trước khi demo.
> **Master Plan:** `docs/superpowers/plans/2026-09-09-quanly-phongtro-net8.md`

**Goal:** Dựng bộ test acceptance phủ các user story SRS + BR, đo coverage, và viết README hướng dẫn chạy demo.

**Files:**
- Create: `QuanLyTro/QuanLyTro.Tests/AcceptanceTests.cs` ✅
- Create: `QuanLyTro/README.md` ✅
- Modify: `docs/superpowers/plans/2026-09-09-quanly-phongtro-net8.md` (đánh dấu hoàn thành) ✅

**Interfaces:**
- Consumes: toàn bộ Server service + Client service đã xong.
- Produces: bằng chứng kiểm thử cho hội đồng; README có lệnh chạy chính xác.

## Các bước thực hiện

- [x] **Step 1: Viết test acceptance cho luồng tháng (SRS §7)**
  - Luồng: tạo phòng → thêm tenant → tạo contract → chốt utility → tạo invoice → pay → report → export.
  - Mỗi bước assert đúng trạng thái DB sau khi gọi service. ✅ `MonthlyFlow_CreateToPaymentAndReport` — 8 bước, MySQL thật, assert DB sau từng bước (kể cả paid_at, tổng tiền BR-10 4.325.000).

- [x] **Step 2: Viết test ma trận phân quyền (BR-14, US-24)**
  - Tenant gọi mọi action ngoài `AUTH_LOGIN` + `INVOICE_GET_MINE` → `Success=false`, `"Không có quyền."`.
  - `INVOICE_GET_MINE` chỉ trả hóa đơn phòng tenant đang ở. ✅ `PermissionMatrix_TenantRejectedOnEveryLandlordAction` (lặp đủ 22 action qua `RequestRouter` thật), `PermissionMatrix_LandlordRejectedOnInvoiceGetMine`, `InvoiceGetMine_ReturnsOnlyInvoicesOfTenantsCurrentRoom` (2 phòng 2 hóa đơn).

- [x] **Step 3: Viết test lockout (US-23)**
  - Sai 5 lần liên tiếp → khóa 1 phút; dùng clock injectable để không chờ thật.
  - Đăng nhập thành công reset bộ đếm. ✅ `Lockout_FiveWrongAttemptsBlockSixthUntilClockAdvancesThenSuccessResets` — `FakeClock` advance 1 phút, không sleep.

- [x] **Step 4: Viết test BR còn thiếu**
  - BR-04 (1 HĐ Active/phòng), BR-05 (đại diện là thành viên phòng), BR-06 (EndDate > StartDate), BR-09 (điều kiện lập hóa đơn), BR-10 (công thức tính tiền), BR-11 (hóa đơn Paid bất biến). ✅ 6 test `Br04..Br11_*` chạy service thật + MySQL thật.

- [x] **Step 5: Chạy toàn bộ test và đo coverage**
  - Lệnh: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --collect:"XPlat Code Coverage"`
  - Expected: 0 failed; ghi lại số test và % coverage. ✅ 127 PASS / 0 FAIL. `XPlat Code Coverage` không có collector → fallback `--collect:"Code Coverage"` (Visual Studio collector): **27.3% dòng** (5.522/20.221), artifact `TestResults/<guid>/*.coverage` (gitignored, đo bằng `dotnet-coverage merge -f cobertura`).

- [x] **Step 6: Viết `README.md` hướng dẫn chạy demo**
  - Yêu cầu: .NET 8 SDK, Laragon + MySQL 3306.
  - Lệnh: bật Laragon → `--initialize-only` → chạy Server → chạy Client.
  - Tài khoản demo: chủ trọ `admin`, người thuê = CCCD.
  - Bảng 22 action TCP và ma trận phân quyền. ✅ `QuanLyTro/README.md` — thêm: kiến trúc ASCII, protocol detail (1 MiB / 10s / SemaphoreSlim), hạn chế, bằng chứng 127 test + coverage.
  - **Sự thật seed admin:** `schema.sql` KHÔNG seed user; `admin/admin-pass` trong DB Laragon do đặt tay lúc E2E. README ghi thủ tục an toàn: sinh hash bằng `PasswordHasher.Hash` (không có lệnh seed chính thức trong repo — không tự thêm code production ngoài phạm vi Task 12) rồi `INSERT ... ON DUPLICATE KEY UPDATE` vào `users`; đã verify đăng nhập được sau khi chạy đúng thủ tục.

- [x] **Step 7: Đánh dấu plan hoàn thành**
  - Cập nhật checkbox trong master plan; ghi chú task nào verify trên MySQL thật. ✅ Bảng sub-plan G ✅ (commit placeholder `pending`), tổng test 127, coverage 27.3%, ghi chú seed admin. Checkbox §8 chỉ tick khi đã commit.

- [ ] **Step 8: Commit**
  - Commit message: `test: verify SRS acceptance scenarios docs (Task 12)` — thực hiện sau lần verify cuối.
