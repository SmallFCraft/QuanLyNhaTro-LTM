# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

Sản phẩm giao là ứng dụng desktop **WinForms** (.NET 8, `net8.0-windows`) có nhúng webview, không phải web app thuần. Giá trị `web` ở trên là giá trị hợp lệ gần nhất của Impeccable: enum chỉ có `web` / `ios` / `android` / `adaptive`, và WinForms không render design language của iOS/Android nên hai nhánh native đó không áp dụng.

Tầng trình bày là **HTML/CSS nạp qua `Microsoft.Web.WebView2`** (xem [spec delta](docs/superpowers/specs/2026-09-30-police-webview2-design.md) §3), đặt trong khung cửa sổ WinForms. Khung cửa sổ, thanh tiêu đề và thanh trạng thái vẫn là control native. Toàn bộ logic nghiệp vụ và I/O mạng vẫn nằm ở C#.

## Stack

- Client: WinForms `net8.0-windows` (Visual Studio 2022, .NET 8 SDK).
- Server / Shared / Tests: `net8.0`; test bằng MSTest.
- Vận chuyển: TCP socket tự viết, JSON một dòng kết thúc `\n`, packet ≤ 1 MiB, Server console cổng 8888.
- Lưu trữ: MySQL 8.0 qua Laragon cổng 3306 (chỉ Server chạm DB).
- Mockup: HTML/CSS tĩnh phục vụ qua `python -m http.server` (`.claude/launch.json`). Đây là bản thiết kế chuẩn của tầng trình bày — tài liệu mới [2026-09-30-police-webview2-design.md](docs/superpowers/specs/2026-09-30-police-webview2-design.md) chọn phương án WebView2. **Trạng thái code hiện tại vẫn là WinForms native (`ScreenTheme.cs`), chưa chuyển sang WebView2.**

## Users

- **Chủ trọ (landlord):** quản lý 1 khu trọ tại phường Ngũ Hành Sơn, Đà Nẵng. Công việc: theo dõi phòng, người thuê, hợp đồng, chốt điện nước, lập + thu hóa đơn, xuất danh sách tạm trú. Dùng WinForms desktop.
- **Người thuê (tenant):** sinh viên/người lao động thuê phòng, muốn tự tra cứu hóa đơn/công nợ phòng mình mà không cần gọi chủ trọ. Dùng cùng app WinForms, đăng nhập bằng CCCD.

## Product Purpose

Hệ thống quản lý phòng trọ Client–Server TCP/IP (đồ án môn Lập Trình Mạng): Client WinForms không chạm DB, mọi thao tác đi qua giao thức JSON-over-TCP tới Server .NET 8 + MySQL. "Nhà trọ văn minh": chủ trọ vận hành qua dashboard, người thuê tự tra cứu được. Thành công = demo bảo vệ: luồng tháng chạy thông suốt (phòng → người → hợp đồng → điện nước → hóa đơn → thu tiền → báo cáo) + 2 vai phân quyền đúng.

## Positioning

Kiến trúc 3 tầng Client–Server qua TCP/IP tự viết (18→22 hanh_dong), mọi business rule (BR-01..BR-14) enforce tại Server + ràng buộc MySQL — khác với app quản lý trọ thông thường chạy trực tiếp DB.

## Operating Context

- Demo/đồ án: máy chính chạy Laragon (MySQL 3306) + Server console (port 8888); Client mở trên nhiều máy LAN.
- Mỗi tháng: lập hợp đồng → vào ở → chốt chỉ số → lập hóa đơn → thu tiền → xuất CSV tạm trú nộp phường.
- Công cụ: Visual Studio 2022, .NET 8 SDK, MSTest.

## Capabilities and Constraints

- 22 hanh_dong TCP, JSON 1 dòng kết thúc `\n`, packet ≤ 1 MiB.
- 2 vai: ChuTro (full CRUD) / KhachThue (chỉ `HOA_DON_CUA_TOI`). Phân quyền server-enforced (BR-14).
- 14 business rule BR-01..BR-14; 24 user story US-01..US-24.
- Đã chốt: không làm vai công an phường; US-20 tra cứu nhanh deferred; US-19 khoảng tháng cộng client-side.
- Client WinForms `net8.0-windows`; Server/Shared/Tests `net8.0`.

## Brand Commitments

Tên hiển thị: "Quản Lý Phòng Trọ Ngũ Hành Sơn". Tiếng Việt toàn bộ UI.

## Evidence on Hand

- [DESIGN.md](DESIGN.md) — hệ thống thiết kế chuẩn (Terracotta &amp; Slate) từ Stitch; nguồn duy nhất cho token màu/chữ/component.
- [docs/superpowers/mockups/giaodien.html](docs/superpowers/mockups/giaodien.html) — bản mẫu giao diện đã áp design system: đăng nhập 3 vai, chủ trọ 7 tab, công an phường 3 tab, người thuê 1 tab.
- [BAO_CAO_USER_STORY.md](BAO_CAO_USER_STORY.md) — báo cáo môn học, 23 US gốc + 13 BR.
- [docs/superpowers/specs/2026-09-09-quanly-phongtro-srs-design.md](docs/superpowers/specs/2026-09-09-quanly-phongtro-srs-design.md) — SRS 18 hanh_dong.
- [docs/superpowers/specs/2026-09-10-quanly-phongtro-multi-actor-design.md](docs/superpowers/specs/2026-09-10-quanly-phongtro-multi-actor-design.md) — delta 2 tác nhân, 22 hanh_dong.
- [docs/superpowers/plans/2026-09-09-quanly-phongtro-net8.md](docs/superpowers/plans/2026-09-09-quanly-phongtro-net8.md) — plan 11 task (đang vá).
- Code: Task 1–2 plan DONE (Shared DTO + protocol + tests PASS).

## Product Principles

1. Server là nguồn chân lý — Client không quyết định nghiệp vụ.
2. Dashboard trước bảng biểu — chủ trọ mở app thấy ngay việc cần làm (nợ, HĐ sắp hết hạn).
3. Luồng tháng là xương sống demo — mọi màn hình phục vụ chuỗi phòng→hóa đơn→thu tiền.
4. 2 vai, 2 trải nghiệm: ChuTro thấy đủ; KhachThue thấy đúng 1 tab, không nút ghi.

## Accessibility & Inclusion

- WinForms chuẩn: Segoe UI 9pt, tab order hợp lý, `AccessibleName` cho control chính, MessageBox xác nhận thao tác hủy được (xóa, thanh toán).
- Dữ liệu tiền định dạng `N0` Việt Nam, ngày `dd/MM/yyyy`.
