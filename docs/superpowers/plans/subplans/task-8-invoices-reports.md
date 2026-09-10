# Sub-Plan C: Hóa Đơn, Thanh Toán & Báo Cáo (Task 8)

> **Agent Worker:** Subagent C (Chạy SAU Task 6 & 7 — phụ thuộc dữ liệu phòng + HĐ + điện nước).
> **Phối hợp:** Wave 2, cuối chuỗi Server-side nghiệp vụ.
> **Master Plan:** `docs/superpowers/plans/2026-09-09-quanly-phongtro-net8.md`

**Goal:** Triển khai Repository và Service cho Hóa đơn hàng tháng (BR-09, BR-10, BR-11, BR-13) và báo cáo thống kê + xuất CSV tạm trú (US-17, US-18, US-19, US-08, US-22).

**Files:**
- Create: `QuanLyTro/QuanLyTro.Server/Repositories/InvoiceRepository.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Repositories/ReportRepository.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Services/InvoiceService.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Services/ReportService.cs`
- Modify: `QuanLyTro/QuanLyTro.Tests/BusinessRuleTests.cs`

**Interfaces:**
- Produces:
  - `InvoiceService`: `CreateAsync(CreateInvoiceRequest)`, `GetAllAsync(string billingMonth, int? roomId)`, `PayAsync(int invoiceId)`, `GetMineAsync(int tenantId)`.
  - `ReportService`: `GetSummaryAsync(string billingMonth)`, `ExportResidenceAsync()`.

## Các bước thực hiện

- [ ] **Step 1: Viết test nghiệp vụ hóa đơn**
  - `CreateAsync` chỉ được khi phòng có HĐ Active và đã chốt điện nước tháng đó (BR-09).
  - Tổng tiền server tự tính theo công thức BR-10, Client chỉ gửi `OtherFees`.
  - Trả tiền: chỉ hóa đơn `Unpaid` được thanh toán; hóa đơn `Paid` là bất biến (BR-11).

- [ ] **Step 2: Chạy test xác nhận fail**
  - Lệnh: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter InvoiceService`

- [ ] **Step 3: Cài đặt `InvoiceRepository` & `InvoiceService`**
  - `CreateAsync` trong transaction (BR-13): đọc HĐ Active, đọc chỉ số điện nước của tháng, tính tiền theo BR-10, insert hóa đơn.
  - Bắt duplicate key `uq_invoice_room_month` → `BusinessRuleException("Tháng này đã lập hóa đơn cho phòng.")`.
  - `PayAsync`: chỉ UPDATE hóa đơn `Unpaid` → `Paid`, set `paid_at = NOW()`; nếu đã `Paid` → ném lỗi (BR-11).
  - `GetMineAsync`: dùng room của tenant đang ở (server suy từ session, BR-14).

- [ ] **Step 4: Cài đặt `ReportRepository` & `ReportService`**
  - `GetSummaryAsync`: tổng số phòng, trống, đang thuê, số người, đã thu, còn nợ (US-04, US-18).
  - `ExportResidenceAsync`: trả danh sách người đang ở kèm phòng (US-08); CSV do CLIENT tự dựng bằng `StreamWriter` UTF-8 BOM.

- [ ] **Step 5: Chạy test kiểm chứng**
  - Lệnh: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj"`

- [ ] **Step 6: Commit**
  - Commit message: `feat: add invoice, payment and reporting services (Task 8)`
