# Sub-Plan G: Kiểm Thử Cuối & Tài Liệu (Task 12)

> **Agent Worker:** Subagent G (Chạy CUỐI CÙNG, sau Wave 4).
> **Phối hợp:** Wave 5, chốt sổ — verify toàn hệ thống khớp SRS trước khi demo.
> **Master Plan:** `docs/superpowers/plans/2026-09-09-quanly-phongtro-net8.md`

**Goal:** Dựng bộ test acceptance phủ các user story SRS + BR, đo coverage, và viết README hướng dẫn chạy demo.

**Files:**
- Create: `QuanLyTro/QuanLyTro.Tests/AcceptanceTests.cs`
- Create: `QuanLyTro/README.md`
- Modify: `docs/superpowers/plans/2026-09-09-quanly-phongtro-net8.md` (đánh dấu hoàn thành)

**Interfaces:**
- Consumes: toàn bộ Server service + Client service đã xong.
- Produces: bằng chứng kiểm thử cho hội đồng; README có lệnh chạy chính xác.

## Các bước thực hiện

- [ ] **Step 1: Viết test acceptance cho luồng tháng (SRS §7)**
  - Luồng: tạo phòng → thêm tenant → tạo contract → chốt utility → tạo invoice → pay → report.
  - Mỗi bước assert đúng trạng thái DB sau khi gọi service.

- [ ] **Step 2: Viết test ma trận phân quyền (BR-14, US-24)**
  - Tenant gọi mọi action ngoài `AUTH_LOGIN` + `INVOICE_GET_MINE` → `Success=false`, `"Không có quyền."`.
  - `INVOICE_GET_MINE` chỉ trả hóa đơn phòng tenant đang ở.

- [ ] **Step 3: Viết test lockout (US-23)**
  - Sai 5 lần liên tiếp → khóa 1 phút; dùng clock injectable để không chờ thật.
  - Đăng nhập thành công reset bộ đếm.

- [ ] **Step 4: Viết test BR còn thiếu**
  - BR-04 (1 HĐ Active/phòng), BR-05 (đại diện là thành viên phòng), BR-06 (EndDate > StartDate), BR-09 (điều kiện lập hóa đơn), BR-10 (công thức tính tiền), BR-11 (hóa đơn Paid bất biến).

- [ ] **Step 5: Chạy toàn bộ test và đo coverage**
  - Lệnh: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --collect:"XPlat Code Coverage"`
  - Expected: 0 failed; ghi lại số test và % coverage.

- [ ] **Step 6: Viết `README.md` hướng dẫn chạy demo**
  - Yêu cầu: .NET 8 SDK, Laragon + MySQL 3306.
  - Lệnh: bật Laragon → `dotnet run --project "QuanLyTro/QuanLyTro.Server" -- --initialize-only` → chạy Server → chạy Client.
  - Tài khoản demo: chủ trọ `admin`, người thuê = CCCD.
  - Bảng 22 action TCP và ma trận phân quyền.

- [ ] **Step 7: Đánh dấu plan hoàn thành**
  - Cập nhật checkbox trong master plan; ghi chú task nào verify trên MySQL thật.

- [ ] **Step 8: Commit**
  - Commit message: `test: verify SRS acceptance scenarios and docs (Task 12)`
