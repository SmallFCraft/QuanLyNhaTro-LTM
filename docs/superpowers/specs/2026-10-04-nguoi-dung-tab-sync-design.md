# Spec: Đồng bộ Tab "Người dùng" & Quản lý Khách tự đăng ký trong Giao diện Chủ trọ

**Ngày tạo:** 2026-10-04  
**Tác giả:** Claude Code (Architect & Senior Dev)  
**Trạng thái:** Chờ phê duyệt  
**Mục tiêu:** Cải tiến tab "Người thuê" thành "Người dùng" trên giao diện Chủ trọ (`chutro/index.html`), gộp cả khách đang thuê và khách tự đăng ký chưa có phòng (`phong_id IS NULL`), hiển thị rõ ràng cột "Trạng thái thuê" và phân biệt tài khoản.

---

## 1. Bối cảnh & Vấn đề hiện tại

1. **Khách tự đăng ký bị vô hình:**
   - Khách thuê tự đăng ký tài khoản qua form `auth.js` (`DANG_KY`) sẽ tạo bản ghi `khach_thue` với `phong_id = NULL`.
   - Tab "Người thuê" của Chủ trọ (`khach_thue.js:loadTenants`) hiện chỉ lặp qua danh sách phòng đã có (`PHONG_LAY_TAT_CA`) rồi gọi `KHACH_THUE_THEO_PHONG` theo từng `phongId`.
   - Hậu quả: Không hề gọi `KHACH_THUE_THEO_PHONG` với `phongId: 0`, khiến khách tự đăng ký không xuất hiện trong bảng, Chủ trọ không biết có ai đã đăng ký để gán phòng.

2. **Tên tab và phân loại chưa chuẩn xác:**
   - Tab mang tên "Người thuê", nhưng hệ thống bao gồm cả những người đã đăng ký tài khoản người dùng nhưng **chưa thuê phòng nào** (chờ nhận phòng/quét QR).
   - Tên tab phù hợp hơn: **"Người dùng"** (hoặc "Khách thuê & Người dùng").

3. **Thiếu cột "Trạng thái thuê":**
   - Hiện tại bảng chỉ có: Phòng | Họ tên | CCCD | Ngày sinh | SĐT | Quê quán | Tạm trú | Nút đổi MK.
   - Trạng thái "Tạm trú" là trạng thái hành chính (Đã nộp / Chưa nộp), không thể hiện trạng thái ở (Đang ở phòng nào, hay chưa nhận phòng).
   - Cần bổ sung cột **"Trạng thái"** rõ ràng:
     - `Chờ nhận phòng` (badge vàng/cam) khi `phongId == null`
     - `Đang thuê` (badge xanh lá) khi `phongId != null`

---

## 2. Giải pháp kỹ thuật

### 2.1 Backend / Giao thức TCP
- **Không cần sửa schema DB hay router:**
  - `DieuPhoiYeuCau.cs` (dòng 74-77) và `KhachThueRepository.cs` (dòng 27-33) đã hỗ trợ sẵn: `phongId <= 0` trả về tất cả khách `phong_id IS NULL` cho ChuTro/QuanLy.
  - Tận dụng 100% backend sẵn có (nguyên tắc Lazy Senior Developer & YAGNI).

### 2.2 Frontend Chủ trọ (`QuanLyTro/Assets/wwwroot/chutro/`)

1. **`index.html`:**
   - Đổi nhãn tab: `<div class="tab" data-tab="khach_thue" ...><i class="fas fa-users"></i> Người dùng <span class="cnt" id="cnt-khach_thue">0</span></div>` (thêm badge đếm động).
   - Bảng `#tab-khach_thue`: Thêm cột header `<th>Trạng thái</th>` cạnh cột `<th>Phòng</th>`.
   - Cập nhật tooltip và toolbar: lọc phòng có thêm mục `-- Khách tự đăng ký / Chờ nhận phòng (N) --`.

2. **`js/khach_thue.js`:**
   - Trong `loadTenants()`:
     - Gọi song song:
       1. `PHONG_LAY_TAT_CA`
       2. `KHACH_THUE_THEO_PHONG { phongId: 0 }` (danh sách khách chưa có phòng)
       3. `KHACH_THUE_THEO_PHONG { phongId: r.id }` cho từng phòng
     - Gộp tất cả vào `tenantCache`.
     - Cập nhật badge `#cnt-khach_thue` = tổng số người dùng trong cache.
     - Dropdown `#tenant-room`:
       - Option 1: `-- Tất cả (${tenantCache.length} người) --`
       - Option 2: `-- Chờ nhận phòng (${pending.length}) --` (value = `pending`)
       - Option 3..N: Các phòng cụ thể kèm sĩ số hiện tại.
   - Trong `renderTenants()`:
     - Filter hỗ trợ:
       - Rỗng `""` -> tất cả
       - `"pending"` -> `t.phongId == null`
       - ID số -> `t.phongId == id`
     - Cột `Phòng`: nếu `t.phongId == null` hiển thị `<span class="tag warn">Chưa gán</span>`, ngược lại `<span class="tag rent">${r.soPhong}</span>`.
     - Cột `Trạng thái`:
       - `t.phongId == null`: `<span class="tag warn"><i class="fas fa-clock"></i> Chờ nhận phòng</span>`
       - `t.phongId != null`: `<span class="tag done"><i class="fas fa-check-circle"></i> Đang thuê</span>`
   - Cột nút hành động:
     - Khách chưa gán phòng (`phongId == null`): nút "Chuyển phòng" và "Trả phòng" bị vô hiệu hóa (disabled), nút "Sửa" và "Xóa hồ sơ" hoạt động bình thường.
     - Thêm action gán phòng nhanh hoặc dùng trực tiếp qua màn Lập hợp đồng (đã có sẵn).

3. **`js/main.js` & `js/dash.js`:**
   - Cập nhật `LANDLORD_TITLES.khach_thue` = `'Quản lý người dùng & khách thuê'`.
   - Đếm badge `#cnt-khach_thue` tự động.

---

## 3. Ranh giới & Rủi ro (Boundaries & Edge Cases)

1. **Tính tương thích ngược:**
   - `data-tab="khach_thue"` và `id="tab-khach_thue"` giữ nguyên định danh để không làm hỏng `LANDLORD_LOADERS['khach_thue']` và các bài test contract (`LandlordTabContractTests.cs`).
2. **Vai trò Công an:**
   - Công an chỉ được xem khách có phòng thật (server đã chặn `phongId <= 0` cho CongAn tại `DieuPhoiYeuCau.cs:74-77`). Giao diện công an giữ nguyên không ảnh hưởng.
3. **Hiệu năng:**
   - Số lượng khách chưa gán phòng được lấy bằng đúng 1 request TCP nhẹ `KHACH_THUE_THEO_PHONG { phongId: 0 }`.

---

## 4. Kế hoạch kiểm thử

1. **Unit & Contract Test C#:**
   - Thêm test contract xác nhận tab `khach_thue` có tiêu đề "Người dùng", có cột "Trạng thái", và có logic gọi `phongId: 0`.
2. **Chạy toàn bộ test suite:**
   - `dotnet test QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj` phải pass 100% (240+ tests).
3. **Browser Node Driver Test:**
   - Test tự động trên DOM giả lập xác minh dropdown chứa option chờ nhận phòng, bảng hiển thị đúng tag `Chờ nhận phòng` và `Đang thuê`.
