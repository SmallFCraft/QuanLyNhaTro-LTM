# THIẾT KẾ: CÂY THƯ MỤC OUTLINE WEBVIEW2 CHO CÁC TÁC NHÂN (PHASE A)

**Ngày lập:** 2026-10-01
**Cơ sở:** Quyết định người dùng 2026-10-01 — Phase A tái cơ cấu UI Multi-page cho Auth + Landlord + Police + Tenant; tác nhân Manager (Quản lý trọ) chuyển sang Phase B kèm backend C# và DB.

---

## 1. MỤC TIÊU & BỐI CẢNH

### 1.1. Hiện trạng
- `Assets/wwwroot/index.html` gom 4 shell (`#login`, `#landlord`, `#police`, `#tenant`) trong cùng 1 DOM.
- Các script JS nạp chung vào `window` → biến global `_roomsCache`, `policeCache`, `currentTenantInvoices` có thể đè nhau.
- Cần phân chia thành cây thư mục outline rõ ràng: mỗi tác nhân một thư mục độc lập gồm HTML, CSS, JS riêng.

### 1.2. Quyết định phạm vi Phase A
- **Làm:** `auth/`, `landlord/`, `police/`, `tenant/`, `shared/`.
- **Tạm hoãn sang Phase B:** `manager/` (vì `UserRole` và `schema.sql` chưa có role `Manager`, chưa có bảng `areas` hay phân khu — làm trước sẽ thành trang chết).
- **Ràng buộc:** 183 tests hiện tại phải xanh nguyên vẹn. Sửa đường dẫn kiểm thử trong các test file trực tiếp thay vì giữ lại file trùng lặp (không tạo "hai nguồn sự thật").
- **Bảo mật:** Không dùng `localStorage` hay `sessionStorage` (spec §3.2 C2 cấm). Tên hiển thị người dùng truyền qua query string URL khi chuyển trang: `landlord/index.html?u=Nguy%E1%BB%85n%20V%C4%83n%20A`. Token phiên 100% ở lại C# `TcpClientService.Token`.

---

## 2. KIẾN TRÚC THƯ MỤC MỚI

```
QuanLyTro/Assets/wwwroot/
│
├── shared/                         # DÙNG CHUNG — nạp trước ở mọi trang
│   ├── css/
│   │   ├── tokens.css             # Biến CSS: màu sắc, typography, spacing
│   │   └── base.css               # Reset, typography, nút, ô nhập, bảng, modal, toast
│   └── js/
│       ├── bridge.js              # window.bridge.call() giao tiếp C# WebView2
│       └── ui.js                  # esc(), fmtMoney(), fmtDate(), toast(), openModal()
│
├── auth/                           # MÀN HÌNH ĐĂNG NHẬP (entrypoint khởi động)
│   ├── index.html                 # Form đăng nhập (Chủ trọ / Công an / Khách thuê)
│   ├── auth.css                   # Layout card đăng nhập
│   └── auth.js                    # doLogin() -> gọi AUTH_LOGIN -> chuyển trang theo vai
│
├── landlord/                       # TÁC NHÂN 1: CHỦ TRỌ
│   ├── index.html                 # Shell chủ trọ: menu strip, userbar, 7 tabs
│   ├── landlord.css               # Theme Terracotta & Slate
│   └── js/
│       ├── main.js                # Quản lý active tab, nạp tên từ query string, đăng xuất
│       ├── dash.js                # Tổng quan, KPI, cảnh báo nợ/hết hạn
│       ├── rooms.js               # CRUD phòng trọ
│       ├── tenants.js             # CRUD khách thuê, cấp tài khoản
│       ├── contracts.js           # Lập, gia hạn, chấm dứt hợp đồng
│       ├── utils.js               # Chốt chỉ số điện nước
│       ├── invoices.js            # Tạo hóa đơn, thu tiền
│       └── reports.js             # Báo cáo doanh thu, xuất CSV tạm trú
│
├── police/                         # TÁC NHÂN 2: CÔNG AN PHƯỜNG (Chỉ đọc lưu trú)
│   ├── index.html                 # Shell công an: badge ReadOnly, 3 tabs
│   ├── police.css                 # Theme Xanh công an (#70A9FF)
│   └── js/
│       ├── main.js                # Quản lý tab, nạp tên từ query string, đăng xuất
│       ├── citizens.js            # Tra cứu công dân toàn phường, xuất CSV
│       ├── residence.js           # Báo cáo tạm trú chưa nộp (PM-04)
│       └── history.js             # Lịch sử biến động lưu trú + export CSV (PM-05)
│
└── tenant/                         # TÁC NHÂN 3: KHÁCH THUÊ PHÒNG (Cá nhân)
    ├── index.html                 # Shell khách: chi tiết quyết toán, lịch sử hóa đơn
    ├── tenant.css                 # Giao diện thẻ thanh toán
    └── js/
        └── tenant.js              # Gọi INVOICE_GET_MINE, sao chép nội dung chuyển khoản
```

*Xóa bỏ hoàn toàn file `index.html` đơn khối cũ (605 dòng), `css/style.css`, `js/login.js`, `js/landlord.js` sau khi di chuyển xong.*

---

## 3. CƠ CHẾ ĐIỀU HƯỚNG MULTI-PAGE QUA WEBVIEW2

### 3.1. Khởi động ứng dụng
`Form1.cs:54-55` khởi động thẳng vào trang đăng nhập:
```csharp
var htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "wwwroot", "auth", "index.html");
webView.CoreWebView2.Navigate(new Uri(htmlPath).AbsoluteUri);
```

### 3.2. Đăng nhập thành công
Trong `auth/auth.js`:
```javascript
const roleRoutes = {
  0: '../landlord/index.html',
  1: '../tenant/index.html',
  2: '../police/index.html',
  'Landlord': '../landlord/index.html',
  'Tenant': '../tenant/index.html',
  'Police': '../police/index.html'
};
const route = roleRoutes[user.role ?? user.Role] || '../landlord/index.html';
const name = encodeURIComponent(user.fullName ?? user.FullName ?? '');
window.location.href = `${route}?u=${name}`;
```

### 3.3. Đăng xuất
Mỗi tác nhân có nút Đăng xuất trên `userbar`. Khi bấm, gọi hàm `logout()`:
```javascript
async function logout() {
  await window.bridge.call('UI_LOGOUT', {});
  window.location.href = '../auth/index.html';
}
```
Tại C# `WebMessageBridge.cs`:
- Bắt action cục bộ `UI_LOGOUT`:
  - `_client.Token = null;` (hủy token phiên)
  - Trả về `{ requestId, success: true, data: null, error: null }`
  - Không gửi gói tin rác lên TCP Server.

---

## 4. CHIẾN LƯỢC CẬP NHẬT KIỂM THỬ (TEST REFACTOR)

Các test contract cũ đọc `Path.Combine(Wwwroot, "index.html")` được cập nhật đường dẫn chính xác tới file của module tương ứng:
- `SanityTests.cs`: kiểm tra `#room-status` trong `landlord/index.html`, kiểm tra `landlord/js/rooms.js`.
- `LandlordTabContractTests.cs`: kiểm tra 7 tab trong `landlord/index.html`, kiểm tra các file `landlord/js/*.js`.
- `PoliceShellContractTests.cs`: kiểm tra `#police` và 3 tab trong `police/index.html`, kiểm tra các file `police/js/*.js`.
- `TenantShellContractTests.cs`: kiểm tra `#tenant` trong `tenant/index.html`, kiểm tra `tenant/js/tenant.js`.
- `UiHelpersContractTests.cs`: kiểm tra `shared/js/ui.js`, kiểm tra các id màn hình trên từng trang HTML con, kiểm tra thứ tự nạp thẻ `<script>` ở mỗi trang.
- `AssetPackagingTests.cs`: kiểm tra sự tồn tại của cấu trúc thư mục mới `auth/`, `landlord/`, `police/`, `tenant/`, `shared/`.
- `NoDeadButtonsTests.cs`: quét mọi file HTML tìm thấy trong `Assets/wwwroot/**/*.html` để đảm bảo không trang nào có nút chết.
- `PoliceShellContractTests.FrontEnd_NeverReadsSessionToken`: quét đệ quy mọi file `.js` và `.html` trong `Assets/wwwroot/` bảo đảm không có `token` hay `localStorage`/`sessionStorage`.

Không có file mồ côi hay shim rác. Một sự thật duy nhất.
