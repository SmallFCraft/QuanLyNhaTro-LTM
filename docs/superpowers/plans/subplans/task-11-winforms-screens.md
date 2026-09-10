# Sub-Plan F: Màn Hình Nghiệp Vụ WinForms (Task 11)

> **Agent Worker:** Subagent F (Chạy SAU Sub-Plan E — cần `TcpClientService` và shell `Form1`).
> **Phối hợp:** Wave 4, tầng UI. Không chứa quyết định nghiệp vụ — mọi rule do Server quyết.
> **Master Plan:** `docs/superpowers/plans/2026-09-09-quanly-phongtro-net8.md`
> **Design system:** `DESIGN.md` · wireframe `docs/superpowers/mockups/wireframe-quanly-phongtro.html`

**Goal:** Dựng 7 tab cho chủ trọ và 1 tab cho người thuê, mỗi màn là `UserControl` nhúng vào `TabControl` của `Form1`.

**Files:**
- Create: `QuanLyTro/Forms/DashboardForm.cs` (+ `.Designer.cs`)
- Create: `QuanLyTro/Forms/RoomsForm.cs` (+ `.Designer.cs`)
- Create: `QuanLyTro/Forms/TenantsForm.cs` (+ `.Designer.cs`)
- Create: `QuanLyTro/Forms/ContractsForm.cs` (+ `.Designer.cs`)
- Create: `QuanLyTro/Forms/UtilitiesForm.cs` (+ `.Designer.cs`)
- Create: `QuanLyTro/Forms/InvoicesForm.cs` (+ `.Designer.cs`)
- Create: `QuanLyTro/Forms/ReportsForm.cs` (+ `.Designer.cs`)
- Create: `QuanLyTro/Forms/MyInvoicesForm.cs` (+ `.Designer.cs`)
- Modify: `QuanLyTro/Form1.cs`, `QuanLyTro/Form1.Designer.cs`

**Interfaces:**
- Consumes: singleton `TcpClientService` từ `Form1`, `ActionNames`, các DTO trong `QuanLyTro.Shared.Models`.
- Produces: UI cho US-01..US-24; không chứa business-rule decisions.

## Các bước thực hiện

- [ ] **Step 0: Tạo tab `DashboardForm` (Tổng quan)**
  - 4 thẻ KPI: Tổng số phòng · Phòng trống (kèm tỷ lệ lấp đầy) · Người đang ở · Còn nợ tháng.
  - Bảng "Còn nợ — đôn đốc" (US-17) và "HĐ sắp hết hạn" (US-11), cột `Còn` tô `#D95D39` khi < 30 ngày.
  - Gọi `REPORT_SUMMARY` + `CONTRACT_GET_ALL` + `INVOICE_GET_ALL` một lần khi mở tab. Click dòng → điều hướng sang tab tương ứng.

- [ ] **Step 1: Tạo `RoomsForm`**
  - `DataGridView` readonly: số phòng, giá, sức chứa, số người, trạng thái. Toolbar Thêm/Sửa/Xóa/F5.
  - Nút Xóa có `MessageBox` xác nhận. Mọi thao tác gọi `ROOM_*`, sau success reload grid.

- [ ] **Step 2: Tạo `TenantsForm`**
  - ComboBox chọn phòng; grid người thuê; input bắt buộc + ô mật khẩu (ghi chú "để trống = 6 số cuối CCCD").
  - Add/Update/Checkout/Delete gọi `TENANT_*`. Client chỉ kiểm tra trường rỗng/định dạng ngày; Server quyết định sức chứa và CCCD duy nhất.

- [ ] **Step 3: Tạo `ContractsForm`**
  - Chọn phòng, đại diện, ngày bắt đầu/kết thúc, giá thuê, tiền cọc. Nút Tạo/Chấm dứt/Gia hạn.
  - Cột "Còn" hiển thị số ngày còn lại, tô `#D95D39` khi < 30 ngày. Hiển thị thông báo lỗi Server nguyên văn.

- [ ] **Step 4: Tạo `UtilitiesForm`**
  - Chọn phòng/tháng; gọi `UTILITY_GET_PREVIOUS` điền chỉ số cũ; `NumericUpDown` cho chỉ số mới và đơn giá.
  - Preview tiền chỉ để UX (US-13); kết quả chính thức lấy từ Server khi `UTILITY_RECORD`.

- [ ] **Step 5: Tạo `InvoicesForm`**
  - Chọn tháng, list hóa đơn + chi tiết breakdown, filter chưa thu. Nút Lập hóa đơn và Thu.
  - Dòng `Paid` khóa nút Thu (BR-11). Server tính toàn bộ amount ngoại trừ `OtherFees`.

- [ ] **Step 6: Tạo `ReportsForm` và xuất CSV bằng stdlib**
  - Gọi `REPORT_SUMMARY`; hiển thị KPI và bảng nhiều tháng (US-19: client gọi từng tháng rồi cộng dòng tổng).
  - Gọi `EXPORT_RESIDENCE`; dùng `StreamWriter` UTF-8 BOM, escape CSV bằng cách bọc `"` và nhân đôi `"` bên trong. KHÔNG thêm package Excel.

- [ ] **Step 7: Tạo `MyInvoicesForm` (người thuê)**
  - 1 tab duy nhất: thẻ hồ sơ (tên, phòng, hạn HĐ, nhãn `ReadOnly`), banner quá hạn, bảng quyết toán có dòng phụ chỉ số điện/nước, khối tổng tiền, bảng lịch sử.
  - Gọi `INVOICE_GET_MINE`. **Không có nút thao tác ghi.**

- [ ] **Step 8: Gắn forms vào tab shell, áp design system, kiểm tra accessibility**
  - Áp token `DESIGN.md`: nền `#101417`, card `#181C1F`, header grid `#1A2025` chữ `#767E88` in hoa, dòng chọn nền `#21262B` + dải trái 3px `#D95D39`, cột số canh phải, tag sage/terracotta/amber theo bảng ngữ nghĩa, nút chính `#D95D39`, bo góc 4px/8px, không pill.
  - Đặt `AccessibleName`, tab order hợp lý, labels liên kết controls, font `Segoe UI 9pt` (số liệu `Consolas 9.5pt`), keyboard navigation hoạt động, lỗi hiển thị bằng `MessageBox` và giữ input.

- [ ] **Step 9: Build và chạy luồng thủ công**
  - Lệnh: `dotnet build "QuanLyTro/QuanLyTro.slnx"`
  - Luồng: phòng trống → thêm tenant → tạo contract → chốt utility → tạo invoice → pay → report.
  - Expected: trạng thái đúng từng bước, Client không treo.

- [ ] **Step 10: Commit**
  - Commit message: `feat: add WinForms business screens (Task 11)`
