# DESIGN DELTA: TÁC NHÂN CÔNG AN PHƯỜNG + TẦNG GIAO DIỆN WEBVIEW2

**Ngày:** 2026-09-30
**Cơ sở:** Bổ sung cho `2026-09-10-quanly-phongtro-multi-actor-design.md` — **đảo ngược một quyết định của spec đó** (§8 nêu rõ "Vai công an phường" nằm ngoài phạm vi).
**Trạng thái:** Tài liệu thiết kế đã cập nhật. **Code chưa triển khai** — xem §5.

---

## 1. BỐI CẢNH & QUYẾT ĐỊNH MỚI

Bản mẫu trực quan [`docs/superpowers/mockups/giaodien.html`](../../mockups/giaodien.html) đã bổ sung màn hình cho cán bộ công an phường (shell `#police`, 3 tab). Để báo cáo môn học khớp với bản mẫu, tác nhân này được đưa vào phạm vi đề tài.

Hai quyết định cũ bị đảo, ghi lại rõ để không ai hiểu nhầm:

| Quyết định cũ | Nguồn | Trạng thái mới |
|---|---|---|
| "Không làm vai công an phường" | spec 2026-09-10 §1.1, §8 | **Đảo** — POLICE là tác nhân thứ 3, có màn hình riêng |
| "HTML/CSS chỉ tồn tại ở wireframe, không phải bề mặt người dùng cuối" | `PRODUCT.md` §Platform | **Đảo** — HTML/CSS trở thành tầng giao diện thật, nạp qua WebView2 |

Vai **Quản lý trọ (Manager)** trong `GIOI_THIEU_DE_TAI.md` vẫn là phạm vi thiết kế nhưng chưa có màn hình trong bản mẫu hiện tại — không nằm trong delta này.

---

## 2. TÁC NHÂN CÔNG AN PHƯỜNG (POLICE)

### 2.1. Vai trò và phạm vi

| Thuộc tính | Giá trị |
|---|---|
| Tên vai | `Police` (thêm vào `enum UserRole` cạnh `Landlord`, `Tenant`) |
| Đăng nhập | Bảng `users`, `role = 'Police'`; cán bộ được cấp tài khoản, không dùng CCCD |
| Quyền đọc | `RESIDENCE_GET_ALL`, `RESIDENCE_HISTORY_GET`, `EXPORT_RESIDENCE_HISTORY` |
| Quyền ghi | **Không có.** Mọi action ghi bị `RequestRouter` từ chối với thông báo `"Vai Công an phường chỉ được đọc."` |
| Phạm vi dữ liệu | Toàn phường Ngũ Hành Sơn — không giới hạn theo khu như Manager |

Cờ `is_temporary_registered` trên bảng `tenants` là nguồn sự thật duy nhất cho trạng thái tạm trú; công an chỉ đọc cờ này, không có đường ghi nào chạm tới nó.

### 2.2. Action mới (2)

| Action | Payload vào | Trả ra |
|---|---|---|
| `RESIDENCE_HISTORY_GET` | `{ FromDate, ToDate, RoomNumber? }` | `List<ResidenceHistoryDto>` — biến động vào/ra/chuyển phòng trong khoảng ngày |
| `EXPORT_RESIDENCE_HISTORY` | `{ FromDate, ToDate, Format, Columns[] }` | `{ FilePath, RowCount }` — CSV / Excel / PDF |

`EXPORT_RESIDENCE` (đã có, chủ trọ dùng) giữ nguyên cho danh sách khai báo tạm trú. Action mới phục vụ PM-05.2 (xuất lịch sử biến động) theo phân rã chức năng.

### 2.3. Màn hình (khớp bản mẫu `giaodien.html`)

| Mã | Tab | Nội dung |
|---|---|---|
| PM-02, PM-03 | **Công dân** | 4 thẻ KPI; lọc theo CCCD / họ tên (không phân biệt hoa thường) / SĐT / phòng; bảng họ tên, ngày sinh, CCCD, quê quán, phòng, trạng thái tạm trú |
| PM-04 | **Tạm trú** | Lọc danh sách chưa đăng ký tạm trú theo phòng; nút xuất CSV / Excel |
| PM-05 | **Biến động** | Chọn khoảng ngày vào → ra, lọc loại biến động; xem trước 10 dòng đầu trước khi xuất; ghi log xuất file về Server |

Badge `ReadOnly` hiển thị vĩnh viễn trên đầu màn hình. Shell dùng màu nhận diện riêng `#70A9FF` (ngoài bảng token của `DESIGN.md` — ghi nhận là ngoại lệ chỉ dùng cho vai POLICE).

---

## 3. TẦNG GIAO DIỆN WEBVIEW2

### 3.1. Quyết định

Client vẫn là ứng dụng desktop WinForms `.NET 8`, nhưng phần trình bày render bằng HTML/CSS nạp qua `Microsoft.Web.WebView2`. Mọi logic nghiệp vụ và toàn bộ I/O mạng vẫn nằm ở C#.

```
Form1 (WinForms)
├── titlebar / menustrip / userbar / statusstrip   (Control native, sơn theo token)
└── WebView2
    └── index.html  ←  assets của bản mẫu giaodien.html
          ⇅ WebMessage (JSON)
    ClientService (C#)  ──TCP JSON──▶  Server
```

### 3.2. Cầu nối C# ↔ JavaScript

- JS → C#: `window.chrome.webview.postMessage({ requestId, action, data })`
- C# → JS: `webview.CoreWebView2.PostWebMessageAsJson(...)` theo `requestId` để khớp phản hồi.
- Token phiên do C# giữ, **không** đưa vào DOM. JS không tự gọi action nào ngoài `postMessage`.

### 3.3. Ràng buộc bắt buộc

- Nghiệp vụ vẫn 100% ở Server. WebView2 chỉ đổi bề mặt, không đổi kiến trúc 3 tầng.
- Phân quyền vẫn kiểm ở `RequestRouter`. Client sửa HTML/JS không vượt được quyền.
- Ẩn nút ghi ở UI là tiện ích trải nghiệm, **không phải** cơ chế bảo mật.

---

## 4. ĐỒNG BỘ TÀI LIỆU

1. `XAC_DINH_CHUC_NANG_MENU_GIAO_DIEN.md` — thêm §2.12 (màn hình lưu trú của công an) và cập nhật bảng phân quyền §3.
2. `PRODUCT.md` — nguồn UI là `giaodien.html` (file `wireframe-quanly-phongtro.html` đã bị xoá, các tham chiếu cũ đã đổi), WebView2 nêu ở §Platform và §Stack.
3. `2026-09-10-quanly-phongtro-multi-actor-design.md` — thêm ghi chú đầu file trỏ sang delta này; giữ nguyên phần còn lại.
4. `GIOI_THIEU_DE_TAI.md` — **không cần sửa**: đã có sẵn vai Công an phường, FR-28, FR-29, BR-16 và ma trận chức năng tương ứng.

---

## 5. KHOẢNG CÁCH GIỮA TÀI LIỆU VÀ CODE

Docs đi trước code. Trước khi demo bảo vệ phải đóng lại các mục sau:

| Việc | File code |
|---|---|
| Thêm `UserRole.Police` | `QuanLyTro/QuanLyTro.Shared/Models/AuthAndReportDtos.cs` |
| 2 action mới + permission matrix chặn ghi | `QuanLyTro/QuanLyTro.Server/Network/RequestRouter.cs` |
| Cột `role` nhận giá trị `Police` | `database/schema.sql` |
| Giao diện: thay `TabControl` bằng WebView2 | `QuanLyTro/Form1.cs`, `QuanLyTro/Forms/*` |
| Test: Police đọc được, ghi bị từ chối | `QuanLyTro/QuanLyTro.Tests/` |

`XAC_DINH_CHUC_NANG_MENU_GIAO_DIEN.md` §2.5–2.7 còn mô tả vai "Quản lý trọ" chưa có màn hình — giữ nguyên trong tài liệu, đánh dấu deferred.
