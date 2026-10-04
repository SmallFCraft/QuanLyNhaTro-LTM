# Kế Hoạch Triển Khai: Tự Đăng Ký & Nhận Phòng Bằng Mã QR / PIN (Khách Thuê)

> **Dành cho kỹ sư triển khai:** Bắt buộc tuân thủ TDD: viết test trước, chạy fail, viết code, chạy pass, commit từng task.

**Mục tiêu:** Cho phép khách thuê tự đăng ký tài khoản từ client; chủ trọ lập hợp đồng sinh mã QR/PIN 1 lần; khách quét QR hoặc nhập PIN trên client để tự động kích hoạt nhận phòng.

**Kiến trúc:** 
- Database MySQL: Mở rộng `trang_thai` của `hop_dong` thêm `ChoNhanPhong`, bổ sung `ma_qr_token` và `ma_pin`.
- Server .NET 8 TCP: Thêm 3 Action TCP `DANG_KY`, `HOP_DONG_SINH_QR`, `KHACH_THUE_NHAN_PHONG_QR` với transaction an toàn và kiểm tra quyền RBAC.
- Frontend WebView2: Bổ sung form đăng ký ở `auth/`, modal xem/in QR ở `chutro/`, và giao diện 3 phương thức quét QR (Upload ảnh, Webcam, Nhập PIN) ở `khachthue/`.

**Tech Stack:** .NET 8.0, C#, MySQL 8.0, WebView2, HTML5 Canvas, jsQR, qrcode.js.  
**Spec tham chiếu:** [2026-10-03-tenant-qr-checkin-design.md](../specs/2026-10-03-tenant-qr-checkin-design.md)

---

## Ràng Buộc Toàn Cục (Global Constraints)
- Toàn bộ tên action giao thức TCP viết bằng tiếng Việt không dấu: `DANG_KY`, `HOP_DONG_SINH_QR`, `KHACH_THUE_NHAN_PHONG_QR`.
- Mã QR token dùng `RandomNumberGenerator.GetHexString(32)` (128-bit entropy), PIN gồm 8 chữ số `00000000`..`99999999`.
- Sau khi nhận phòng thành công, mã QR token và PIN phải bị xóa hoặc vô hiệu hóa ngay lập tức (dùng 1 lần duy nhất).
- Toàn bộ thư viện JS client (`qrcode.min.js`, `jsqr.min.js`) phải lưu offline nội bộ trong `QuanLyTro/Assets/wwwroot/shared/js/vendor/`, không dùng CDN ngoài.
- Tuyệt đối không làm gãy bất kỳ bài test nào trong số 207 test hiện tại.

---

## Trọng Tâm Rà Soát (Review Focus)
1. **Khách A quét mã của Khách B:** Phải bị server chặn với thông báo "Mã nhận phòng này được cấp cho người đại diện khác."
2. **Quét lại mã đã dùng:** Phải bị từ chối với thông báo "Mã nhận phòng không hợp lệ hoặc đã được sử dụng."
3. **Phòng đã bị đầy hoặc có HĐ hiệu lực khác trong lúc chờ:** Server phải rollback transaction và báo lỗi rõ ràng.
4. **CCCD đã tồn tại khi đăng ký:** Báo lỗi "Số CCCD này đã được đăng ký trong hệ thống."
5. **Định dạng ảnh upload không hợp lệ hoặc không có QR:** Client phải báo lỗi thân thiện thay vì crash.

---

## Chi Tiết Các Task Triển Khai

### Task 1: Cập Nhật CSDL & DTO Mô Hình Dữ Liệu
**Files:**
- Modify: `QuanLyTro/database/schema.sql`
- Modify: `QuanLyTro/QuanLyTro.Server/Data/KhoiTaoSchema.cs`
- Modify: `QuanLyTro/QuanLyTro.Shared/Models/Enums.cs`
- Modify: `QuanLyTro/QuanLyTro.Shared/Models/HopDongDto.cs`
- Modify: `QuanLyTro/QuanLyTro.Shared/Protocol/ActionNames.cs`
- Modify: `QuanLyTro/QuanLyTro.Shared/Protocol/QuyenMacDinhTheoVaiTro.cs`
- Test: `QuanLyTro/QuanLyTro.Tests/QrCheckinModelTests.cs`

- [ ] **Step 1: Viết test kiểm tra DTO, ActionNames và ENUM mới**
- [ ] **Step 2: Chạy test và xác nhận FAIL**
- [ ] **Step 3: Cập nhật Enum `TrangThaiHopDong` thêm `ChoNhanPhong`**
- [ ] **Step 4: Cập nhật `HopDongDto` thêm các trường `MaQrToken`, `MaPin`**
- [ ] **Step 5: Bổ sung 3 ActionNames vào `ActionNames.cs` và phân quyền mặc định**
- [ ] **Step 6: Thêm lệnh migration ALTER TABLE an toàn vào `KhoiTaoSchema.cs`**
- [ ] **Step 7: Chạy test và xác nhận PASS toàn bộ**

---

### Task 2: Nghiệp Vụ Tự Đăng Ký Tài Khoản (`DANG_KY`)
**Files:**
- Modify: `QuanLyTro/QuanLyTro.Server/Repositories/KhachThueRepository.cs`
- Modify: `QuanLyTro/QuanLyTro.Server/Services/KhachThueService.cs`
- Modify: `QuanLyTro/QuanLyTro.Server/Network/DieuPhoiYeuCau.cs`
- Test: `QuanLyTro/QuanLyTro.Tests/TenantRegistrationTests.cs`

- [ ] **Step 1: Viết test cho nghiệp vụ tự đăng ký (CCCD hợp lệ, trùng lặp, mật khẩu)**
- [ ] **Step 2: Chạy test và xác nhận FAIL**
- [ ] **Step 3: Cài đặt phương thức `RegisterAsync` trong `KhachThueService`**
- [ ] **Step 4: Bổ sung handler `DANG_KY` vào `DieuPhoiYeuCau` (không yêu cầu token)**
- [ ] **Step 5: Chạy test và xác nhận PASS**

---

### Task 3: Nghiệp Vụ Sinh QR/PIN & Kích Hoạt Nhận Phòng
**Files:**
- Modify: `QuanLyTro/QuanLyTro.Server/Repositories/HopDongRepository.cs`
- Modify: `QuanLyTro/QuanLyTro.Server/Services/HopDongService.cs`
- Modify: `QuanLyTro/QuanLyTro.Server/Network/DieuPhoiYeuCau.cs`
- Test: `QuanLyTro/QuanLyTro.Tests/QrCheckinWorkflowTests.cs`

- [ ] **Step 1: Viết test cho luồng sinh QR, kiểm tra token/pin, nhận phòng và chặn sai CCCD**
- [ ] **Step 2: Chạy test và xác nhận FAIL**
- [ ] **Step 3: Triển khai sinh token/PIN khi tạo hợp đồng `ChoNhanPhong`**
- [ ] **Step 4: Triển khai phương thức `CheckinByQrAsync` trong `HopDongService` có transaction an toàn**
- [ ] **Step 5: Bổ sung handler `HOP_DONG_SINH_QR` và `KHACH_THUE_NHAN_PHONG_QR` vào `DieuPhoiYeuCau`**
- [ ] **Step 6: Chạy test và xác nhận PASS**

---

### Task 4: Giao Diện Màn Hình Đăng Nhập & Đăng Ký (`auth/`)
**Files:**
- Modify: `QuanLyTro/Assets/wwwroot/auth/index.html`
- Modify: `QuanLyTro/Assets/wwwroot/auth/auth.js`
- Test: `QuanLyTro/QuanLyTro.Tests/AuthShellContractTests.cs`

- [ ] **Step 1: Bổ sung contract test kiểm tra nút đăng ký và form đăng ký trong auth shell**
- [ ] **Step 2: Thêm tab chuyển đổi "Đăng nhập" / "Đăng ký" trong `auth/index.html`**
- [ ] **Step 3: Viết logic submit form gọi action `DANG_KY` trong `auth.js`**
- [ ] **Step 4: Kiểm thử hiển thị trên Playwright**

---

### Task 5: Giao Diện Chủ Trọ — Xem & In Mã QR Hợp Đồng (`chutro/`)
**Files:**
- Create: `QuanLyTro/Assets/wwwroot/shared/js/vendor/qrcode.min.js`
- Modify: `QuanLyTro/Assets/wwwroot/chutro/index.html`
- Modify: `QuanLyTro/Assets/wwwroot/chutro/js/hop_dong.js`

- [ ] **Step 1: Thêm thư viện `qrcode.min.js` offline vào vendor**
- [ ] **Step 2: Bổ sung checkbox "Chờ khách quét QR nhận phòng" trong modal tạo hợp đồng**
- [ ] **Step 3: Thêm nút "Xem QR / PIN" cho các hợp đồng `ChoNhanPhong`**
- [ ] **Step 4: Mở modal hiển thị mã QR canvas + mã PIN 8 số + nút Tải ảnh/In**
- [ ] **Step 5: Kiểm tra bằng Playwright**

---

### Task 6: Giao Diện Khách Thuê — 3 Chế Độ Quét QR Nhận Phòng (`khachthue/`)
**Files:**
- Create: `QuanLyTro/Assets/wwwroot/shared/js/vendor/jsqr.min.js`
- Modify: `QuanLyTro/Assets/wwwroot/khachthue/index.html`
- Modify: `QuanLyTro/Assets/wwwroot/khachthue/js/khach_thue.js`

- [ ] **Step 1: Thêm thư viện `jsqr.min.js` offline vào vendor**
- [ ] **Step 2: Tạo màn hình "Chờ nhận phòng" hiển thị khi khách chưa có phòng**
- [ ] **Step 3: Xây dựng tab 1: Tải ảnh QR (upload file → render canvas → decode jsQR)**
- [ ] **Step 4: Xây dựng tab 2: Quét Webcam trực tiếp (getUserMedia → video/canvas loop)**
- [ ] **Step 5: Xây dựng tab 3: Nhập mã PIN 8 số thủ công**
- [ ] **Step 6: Gửi `KHACH_THUE_NHAN_PHONG_QR`, thông báo thành công và chuyển sang giao diện chính**

---

### Task 7: Kiểm Thử Tích Hợp Toàn Diện (End-to-End Test Suite)
**Files:**
- Create: `QuanLyTro/QuanLyTro.Tests/QrCheckinEndToEndTests.cs`

- [ ] **Step 1: Viết test kịch bản đầy đủ trên MySQL thật:**
  - Khách đăng ký tài khoản mới qua TCP.
  - Chủ trọ lập hợp đồng `ChoNhanPhong` cho CCCD đó.
  - Khách gửi mã PIN/QR xác nhận.
  - Kiểm tra hợp đồng chuyển `HieuLuc`, khách vào phòng, phòng thành `DaThue`.
  - Quét lại lần 2 bị từ chối.
- [ ] **Step 2: Chạy toàn bộ test suite và xác nhận tất cả PASS**
- [ ] **Step 3: Kiểm tra bằng Playwright trên cả 3 shell**
