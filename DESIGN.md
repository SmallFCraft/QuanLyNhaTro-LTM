---
name: Terracotta & Slate Institutional - Quản Lý Phòng Trọ
source: stitch_property_management_winforms_client
platform: WinForms (.NET 8) + Web Mockup Reference
colors:
  surface-canvas: '#0B0F12'
  surface-base: '#101417'
  surface-card: '#181C1F'
  surface-panel: '#1A1F24'
  surface-active: '#21262B'
  surface-field: '#15191D'
  surface-overlay: '#262A2E'
  surface-hover: '#1F252A'

  border-hairline: '#262A2E'
  border-subtle: '#21272C'
  border-strong: '#3A444E'
  border-input: '#2C3237'
  border-outline-btn: '#2E373F'
  border-tag-empty: '#313A42'
  border-emphasis-top: '#2B333E'
  border-avatar: '#303945'

  text-cream: '#F4EFEA'
  text-cream-light: '#FAF8F5'
  text-neutral: '#CAC6C1'
  text-on-fill: '#FFFFFF'
  text-muted: '#A89988'
  text-dim: '#767E88'
  text-placeholder: '#555D66'

  primary-terracotta: '#D95D39'
  primary-terracotta-hover: '#EA6944'
  primary-terracotta-active: '#B84524'
  primary-tint: '#FFB5A0'
  primary-bg-subtle: 'rgba(217, 93, 57, 0.12)'
  primary-border-subtle: 'rgba(217, 93, 57, 0.35)'

  trang_thai-sage: '#8BD7A3'
  trang_thai-sage-border: '#2E4A35'
  trang_thai-sage-bg: 'rgba(139, 215, 163, 0.12)'

  trang_thai-amber: '#E0AF68'
  trang_thai-amber-border: '#483A24'
  trang_thai-amber-bg: '#2A2319'

  trang_thai-error: '#FFB4AB'
  trang_thai-error-bg: '#32191B'
  trang_thai-error-border: '#93000A'
  trang_thai-error-surface: '#1F1715'
  trang_thai-error-panel: '#1D1715'
  trang_thai-error-panel-border: '#432C25'

  overlay-scrim: 'rgba(0, 0, 0, 0.72)'
typography:
  font-primary: 'Inter, "Segoe UI", system-ui, sans-serif'
  font-mono: '"JetBrains Mono", "SF Mono", Consolas, monospace'
  body-size: 12.5px
  body-line-height: 1.45
  title-size: 14px
  kpi-size: 22px
  label-size: 10px
  uppercase-tracking: '0.06em'
radii:
  component: 4px
  container: 6px
  card: 8px
  pill-allowed: false
density:
  cell-height: 40px
  control-height: 32px
  grid-unit: 4px
---

# Hệ Thống Thiết Kế: Terracotta & Slate Institutional

Nguồn chuẩn duy nhất từ thư mục `stitch_property_management_winforms_client`, áp dụng cho ứng dụng WinForms .NET 8 và wireframe HTML tham chiếu của **Hệ Thống Quản Lý Phòng Trọ Phường Ngũ Hành Sơn**.

## 1. Bản Chất & Thẩm Mỹ

- Phong cách: **Swiss Modernism kết hợp Warm Neo-Brutalism**.
- Đặc trưng:
  - Nền tối đa tầng: Canvas sâu (`#0B0F12`), thân ứng dụng (`#101417`), thẻ/khối thao tác (`#181C1F` / `#1A1F24`).
  - Viền hairline 1px rõ nét (`#262A2E`, `#3A444E`), không dùng đổ bóng nhòe hay hiệu ứng neon phát sáng.
  - Màu nhấn Terracotta nung (`#D95D39`) cho hành động chính, viền tiêu điểm và trạng thái nợ/cảnh báo.
  - Màu Sage thảo mộc (`#8BD7A3`) cho trạng thái hoạt động tốt, phòng đang thuê, hóa đơn đã thanh toán.
  - Góc bo nghiêm ngặt: 4px cho nút/input/tag, 8px cho card. **Tuyệt đối không dùng pill tròn 9999px**.

## 2. Bảng Token Màu Chi Tiết

### 2.1. Phân Tầng Nền (Surfaces)
| Token | Mã Màu | Mục Đích Sử Dụng |
|---|---|---|
| `surface-canvas` | `#0B0F12` | Nền ngoài cùng, viền khung cửa sổ, thanh titlebar WinForms |
| `surface-base` | `#101417` | Nền chính của viewport, thanh điều hướng |
| `surface-card` | `#181C1F` | Thẻ dữ liệu, khối KPI, khung form đăng nhập |
| `surface-panel` | `#1A1F24` | Bảng dữ liệu (DataGrid), modal panel |
| `surface-active` | `#21262B` | Dòng được chọn trong bảng (`border-left: 3px solid #D95D39`) |
| `surface-field` | `#15191D` | Ô nhập liệu TextBox, ComboBox, vùng đọc chỉ số |
| `surface-header` | `#14191D` | Menu strip, dải tiêu đề phân đoạn |

### 2.2. Đường Kẻ Cấu Trúc (Borders)
| Token | Mã Màu | Mục Đích |
|---|---|---|
| `border-hairline` | `#262A2E` | Viền chuẩn 1px cho mọi card, ô nhập, phân cách hàng bảng |
| `border-subtle` | `#21272C` | Đường chia nhỏ nội bộ trong bảng hoặc menu |
| `border-strong` | `#3A444E` | Viền nút thứ cấp, viền container quan trọng |
| `border-outline-btn` | `#2E373F` | Viền nút Outline mặc định (hover sáng lên `border-strong`) |
| `border-tag-empty` | `#313A42` | Viền tag trạng thái "trống / chờ" |
| `border-emphasis-top` | `#2B333E` | Đường viền trên dày 2px của khối tổng tiền |
| `border-avatar` | `#303945` | Viền ô chữ cái đại diện người thuê |
| `border-focus` | `#D95D39` | Viền trạng thái focus của TextBox/ComboBox |

### 2.3. Màu Chữ (Typography Contrast)
| Token | Mã Màu | Mục Đích |
|---|---|---|
| `text-cream` | `#F4EFEA` | Tiêu đề, nhãn quan trọng, giá tiền chính |
| `text-cream-light` | `#FAF8F5` | Chữ nhấn mạnh nhất, dòng đang chọn, tổng tiền |
| `text-neutral` | `#CAC6C1` | Chữ trên tag trạng thái trung tính (phòng trống, chờ xử lý) |
| `text-on-fill` | `#FFFFFF` | Chữ trên nền đặc Terracotta (nút Primary, tag Quá hạn) |
| `text-muted` | `#A89988` | Nhãn phụ, mô tả ngắn, header của DataGrid |
| `text-dim` | `#767E88` | Tiêu đề cột, trạng thái hệ thống, phiên bản |
| `text-placeholder` | `#555D66` | Chữ gợi ý trong ô nhập liệu |

### 2.4. Màu Ngữ Nghĩa (Semantic Roles)
| Trạng Thái | Màu Chữ | Nền Tag | Viền Tag | Áp Dụng Trong Nghiệp Vụ |
|---|---|---|---|---|
| **Đang thuê / Đã thu / OK** | `#8BD7A3` | `rgba(139, 215, 163, 0.12)` | `#2E4A35` | Phòng có người ở, HĐ HieuLuc, Hóa đơn đã thu, Đã tạm trú |
| **Nợ / Cảnh báo khẩn** | `#D95D39` | `rgba(217, 93, 57, 0.12)` | `rgba(217, 93, 57, 0.35)` | Phòng nợ tiền, Hóa đơn chưa thu/quá hạn, lỗi kết nối TCP |
| **Sắp hết hạn / Bảo trì** | `#E0AF68` | `#2A2319` | `#483A24` | Hợp đồng ≤ 30 ngày, phòng đang sửa chữa, chưa chốt điện nước |
| **Phòng trống / Chờ** | `#CAC6C1` | `#1C2227` | `#313A42` | Phòng Trong, chưa nộp tạm trú, người thuê đã trả phòng |
| **Nền banner quá hạn** | `#FFB4AB` trên `#1F1715` | viền `rgba(217,93,57,.40)` | — | Dải cảnh báo nợ đầu màn người thuê |

## 3. Quy Tắc Thành Phần (Components)

### 3.1. Cửa Sổ Desktop (WinForms Chrome)
- **Title Bar:** Cao 32px, nền `#0B0F12`, chữ `#F4EFEA`, icon `#D95D39`. Nút thu nhỏ/phóng to `#767E88`, nút đóng đỏ dịu `#FFB4AB` khi hover.
- **Menu Strip:** Cao 36px, nền `#14191D`, nút bấm dạng tab phẳng với viền mảnh `#2B3239`, hover sáng dần.
- **TrangThai Strip (Đáy):** Cao 28px, nền `#0B0E11`, hiển thị:
  - Chấm trạng thái TCP Socket xanh (`#8BD7A3`) hoặc đỏ (`#D95D39`).
  - IP + Port: `127.0.0.1:8888`.
  - Độ trễ phản hồi: `12ms`.
  - Vai trò hiện tại: `Chủ trọ (ChuTro)` hoặc `Người thuê: Phòng P.101 (KhachThue)`.

### 3.2. DataGrid (Bảng Dữ Liệu)
- Header: Cao 34px, nền `#1A2025`, chữ in hoa cỡ 10px `letter-spacing: 0.06em`, màu `#767E88`.
- Hàng: Cao 40px, viền đáy `#1F252B`, nền xen kẽ `#14181C` / `#161B1F`, hover `#1A2026`.
- **Dòng Đang Chọn (Selected Signature):** Nền `#21262B`, mép trái có dải màu Terracotta rộng 3px (`border-left: 3px solid #D95D39`).
- Cột số (tiền, chỉ số, ngày): Canh phải, dùng font mono hoặc `font-variant-numeric: tabular-nums`.

### 3.3. Nút Bấm (Buttons)
- **Nút Chính (Primary):** Nền `#D95D39`, chữ trắng `#FFFFFF`, viền `#D95D39`, góc bo 4px, cao 32px hoặc 36px. Hover: `#EA6944`. HieuLuc: `#B84524`.
- **Nút Thứ Cấp (Outline):** Nền trong suốt hoặc `#1C2126`, chữ `#F4EFEA`, viền `#2E373F`. Hover: nền `#272F36`, viền `#3A444E`.
- **Nút Xóa / Nguy Hiểm (Danger):** Nền `#1F252A`, chữ `#FFB4AB`, viền `#3E2925`. Hover: `#32191B`.

### 3.4. Thẻ KPI (Operations Metric Blocks)
- Khối chữ nhật bo 6px, nền `#181C1F`, viền `#262A2E`.
- Dải viền đỉnh 2px thể hiện ngữ nghĩa:
  - Sage 2px: Đã thu, phòng đã lấp đầy.
  - Terracotta 2px: Còn nợ, hợp đồng sắp hết hạn.
  - Muted 2px: Tổng số phòng, tổng cọc.

### 3.5. Form Nhập Liệu (Inputs & Selectors)
- TextBox / ComboBox: Cao 32px, nền `#15191D`, viền `#2C3237`, chữ `#F4EFEA`, góc bo 4px. Focus: viền `#D95D39`, vòng sáng nhẹ `#D95D39/20`.
- Radio / Switch chọn vai trò: Thiết kế dạng segmented control 2 ô liền khối, ô được chọn có nền `#262A2E` và viền `#3D4349`.

## 4. Ánh Xạ Vào 2 Vai Trò Ứng Dụng

| Thành Phần | Giao Diện Chủ Trọ (ChuTro) | Giao Diện Người Thuê (KhachThue) |
|---|---|---|
| Số tab | 7 tab (Tổng quan, Phòng, Người thuê, Hợp đồng, Điện nước, Hóa đơn, Thống kê) | 1 tab duy nhất: Hóa Đơn Của Tôi |
| Hành động | Đầy đủ nút Thêm, Sửa, Xóa, Lập HĐ, Chốt số, Thu tiền, Xuất CSV | Chỉ xem (ReadOnly), nút Tra cứu, nút Sao chép nội dung chuyển khoản |
| Nhận diện | Badge vương miện `#D95D39` "Chủ trọ / Quản trị viên" | Badge người thuê "Phòng P.101 • Khách thuê" |
| Đăng nhập | Tài khoản quản trị `admin` + mật khẩu | Số CCCD + mật khẩu (mặc định 6 số cuối CCCD) |

## 5. Ánh Xạ Mã Nguồn WinForms (.NET 8)

Khi triển khai code WinForms C# (Task 9 & 10):
- `Form.BackColor = ColorTranslator.FromHtml("#101417")`
- `DataGridView.BackgroundColor = ColorTranslator.FromHtml("#14181C")`
- `DataGridView.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#21262B")`
- `ButtonPrimary.BackColor = ColorTranslator.FromHtml("#D95D39")`
- `Font`: Segoe UI 9pt (WinForms native) cho text thường, Consolas 9.5pt cho số liệu/tiền tệ.
