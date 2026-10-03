# Thiết kế Tính năng Tự Đăng Ký & Nhận Phòng Bằng Mã QR / PIN (Khách Thuê)

- **Ngày tạo:** 2026-10-03
- **Tác giả:** Claude Developer & User
- **Trạng thái:** Chờ phê duyệt (Draft)

---

## 1. Bối cảnh & Mục tiêu

### Vấn đề hiện tại
- Khách thuê không thể tự đăng ký tài khoản từ client. Chủ trọ phải nhập tay toàn bộ hồ sơ (CCCD, Họ tên, SĐT, Quê quán) và gán phòng.
- Sau khi bàn giao phòng, không có cơ chế xác nhận bàn giao số (handshake) để chứng minh khách đã thực sự nhận phòng và chấp thuận hợp đồng.

### Mục tiêu giải pháp
1. Cho phép khách thuê **tự đăng ký tài khoản** trực tiếp từ màn hình đăng nhập (`auth/index.html`).
2. Chủ trọ lập hợp đồng dạng **Chờ nhận phòng** (`ChoNhanPhong`) gắn với CCCD người đại diện, tự động sinh mã bí mật (QR token) và mã PIN 8 số dùng một lần.
3. Khách thuê đăng nhập vào app, quét mã QR (bằng 1 trong 3 cách: **upload ảnh QR**, **webcam live**, hoặc **nhập mã PIN**) để nhận phòng.
4. Server đối chiếu CCCD của phiên đăng nhập với CCCD trên hợp đồng:
   - Nếu khớp: Kích hoạt hợp đồng sang `HieuLuc`, gán khách vào phòng, phòng chuyển sang `DaThue`.
   - Nếu không khớp hoặc mã sai/hết hạn: Từ chối, ghi nhật ký lỗi bảo mật.
   - Khi hợp đồng chấm dứt hoặc khách trả phòng: Mã QR/PIN vô hiệu hóa hoàn toàn, **chống người cũ bán lại cho người mới**.

---

## 2. Kiến trúc & Dữ liệu

### 2.1 CSDL MySQL (`quanly_phongtro_nhs`)

#### Bảng `hop_dong`
- Cập nhật ENUM cột `trang_thai`:
  ```sql
  ALTER TABLE hop_dong MODIFY COLUMN trang_thai ENUM('ChoNhanPhong', 'HieuLuc', 'HetHan', 'ChamDut') DEFAULT 'ChoNhanPhong';
  ```
- Thêm các cột phục vụ QR/PIN:
  ```sql
  ALTER TABLE hop_dong
    ADD COLUMN ma_qr_token VARCHAR(64) NULL UNIQUE AFTER ghi_chu,
    ADD COLUMN ma_pin VARCHAR(8) NULL AFTER ma_qr_token;
  ```

#### Bảng `khach_thue`
- Cho phép `phong_id` mang giá trị `NULL` khi khách tự đăng ký (đã hỗ trợ sẵn trong schema hiện tại).

### 2.2 Quy ước mã QR & PIN
- `ma_qr_token`: Sinh ngẫu nhiên bảo mật dạng hex 32 ký tự (128-bit entropy): `RandomNumberGenerator.GetHexString(32)`.
- `ma_pin`: 8 chữ số ngẫu nhiên dễ đọc: ví dụ `72948153`.
- Định dạng chuỗi nhúng vào QR Code:
  ```
  QUANLYTRO:NHANPHONG:<ma_qr_token>
  ```

---

## 3. Giao thức Mạng (TCP JSON Lines)

Bổ sung 3 hành động (`ActionNames`) mới:

| # | Action | Vai trò được phép | Payload gửi lên | Dữ liệu trả về | Mô tả |
|---|---|---|---|---|---|
| 27 | `DANG_KY` | Công khai (không token) | `{ cccd, hoTen, ngaySinh, soDienThoai, queQuan, noiLamViec?, matKhau }` | `KhachThueDto` | Đăng ký tài khoản người thuê mới (`phong_id = NULL`) |
| 28 | `HOP_DONG_SINH_QR` | `ChuTro`, `QuanLy` | `{ hopDongId }` | `{ hopDongId, maQrToken, maPin, qrDataString }` | Lấy dữ liệu sinh QR và mã PIN của HĐ đang chờ |
| 29 | `KHACH_THUE_NHAN_PHONG_QR` | `KhachThue` | `{ tokenOrPin }` | `{ hopDongId, soPhong, giaThue, ngayBatDau, ngayKetThuc }` | Khách quét QR/PIN để kích hoạt nhận phòng |

### Quy tắc nghiệp vụ mới (BR-17, BR-18, BR-19)
- **BR-17 (Tự đăng ký người thuê):** CCCD phải gồm đúng 12 chữ số, duy nhất toàn hệ thống. Mật khẩu mã hóa PBKDF2-SHA256 chuẩn. Hồ sơ mới tạo có `phong_id = NULL`.
- **BR-18 (Sinh QR hợp đồng):** Hợp đồng tạo mới có thể chọn trạng thái `ChoNhanPhong` (sinh kèm `ma_qr_token` và `ma_pin`) hoặc `HieuLuc` (bàn giao trực tiếp truyền thống).
- **BR-19 (Xác thực nhận phòng 1 lần):** Khách gọi `KHACH_THUE_NHAN_PHONG_QR` phải đăng nhập bằng đúng tài khoản của người đại diện (`nguoi_dai_dien_id`). Server thực hiện trong 1 Transaction có khóa dòng (`SELECT ... FOR UPDATE`):
  1. Kiểm tra mã tồn tại, hợp đồng đang `ChoNhanPhong`.
  2. Khách gửi request có ID trùng `nguoi_dai_dien_id`.
  3. Kiểm tra phòng vẫn còn sức chứa (BR-02) và phòng chưa có HĐ hiệu lực khác (BR-04).
  4. Cập nhật khách: `phong_id = hop_dong.phong_id`.
  5. Cập nhật hợp đồng: `trang_thai = 'HieuLuc'`, xóa `ma_qr_token` và `ma_pin` (đảm bảo dùng 1 lần duy nhất).
  6. Cập nhật phòng: `trang_thai = 'DaThue'`.

---

## 4. Giao diện Người Dùng (WebView2 / HTML5)

### 4.1 Màn hình Xác thực (`auth/index.html`)
- Thêm tab/nút **"Đăng ký thuê phòng"**.
- Form đăng ký: Họ tên, Ngày sinh (`<input type="date">`), CCCD 12 số, SĐT, Quê quán, Nơi làm việc, Mật khẩu.
- Validate client trước khi gửi: CCCD đủ 12 số, ngày sinh hợp lệ. Thành công → thông báo và tự điền CCCD vào ô đăng nhập.

### 4.2 Màn hình Chủ trọ (`chutro/index.html` & `chutro/js/hop_dong.js`)
- **Tạo hợp đồng:** Thêm checkbox "Tạo mã QR nhận phòng (Chờ khách quét)" (mặc định tích).
- **Danh sách hợp đồng:**
  - Badge trạng thái mới: `Chờ nhận phòng` (màu cam nhạt / viền đứt đoạn).
  - Cột thao tác có nút **"Xem QR / PIN"**: Mở modal hiển thị mã QR (vẽ bằng `qrcode.min.js`), mã PIN 8 số to rõ, nút In phiếu / Tải ảnh QR để gửi cho khách qua Zalo/Email.

### 4.3 Màn hình Khách thuê (`khachthue/index.html`)
- Khi khách đăng nhập mà chưa có phòng (`phong_id == null`):
  - Ẩn tab cước & hóa đơn hiện tại.
  - Hiển thị màn hình **"Nhận phòng trọ"** với 3 phương thức:
    1. **Tab 1 — Tải ảnh QR:** Nút bấm chọn file ảnh QR `.png/.jpg` → JS đọc qua `<canvas>` và giải mã bằng `jsQR`.
    2. **Tab 2 — Quét bằng Webcam:** Nút "Bật camera", stream `<video>` và quét frame thời gian thực.
    3. **Tab 3 — Nhập mã PIN:** Ô nhập 8 ký tự số đơn giản (fallback khi không có camera/ảnh).
  - Sau khi quét/nhập thành công: Hiện hiệu ứng chúc mừng thành công, tự động reload lại shell để hiển thị phòng và hóa đơn.

---

## 5. Thư viện tích hợp
- `qrcode.min.js` (~15KB, offline) đặt tại `QuanLyTro/Assets/wwwroot/shared/js/vendor/qrcode.min.js`.
- `jsqr.min.js` (~30KB, offline) đặt tại `QuanLyTro/Assets/wwwroot/shared/js/vendor/jsqr.min.js`.
- Không sử dụng CDN bên ngoài, đảm bảo ứng dụng chạy offline hoàn toàn trong mạng LAN.

---

## 6. Kế hoạch Kiểm thử & An toàn
1. **Unit & Contract Tests:**
   - Test sinh QR token và mã PIN duy nhất.
   - Test đăng ký tài khoản mới: CCCD trùng bị từ chối, sai định dạng bị từ chối.
   - Test nhận phòng: Người khác quét mã bị chặn, quét mã đã dùng bị chặn, quét thành công cập nhật đúng phòng + hợp đồng.
2. **Acceptance Test (MySQL thật):**
   - Luồng đầu-cuối: Khách tự đăng ký → Chủ trọ lập HĐ ChoNhanPhong → Khách nhận phòng bằng PIN/QR → HĐ chuyển HieuLuc → Phòng DaThue.
