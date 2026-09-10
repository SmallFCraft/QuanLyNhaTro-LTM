# Sub-Plan E: TCP Client Service & Shell (Task 10)

> **Agent Worker:** Subagent E (Chạy song song được với Sub-Plan D SAU khi chốt hợp đồng protocol).
> **Phối hợp:** Wave 3, phía Client. Chỉ phụ thuộc `QuanLyTro.Shared` (protocol đã xong), KHÔNG phụ thuộc Server.
> **Master Plan:** `docs/superpowers/plans/2026-09-09-quanly-phongtro-net8.md`

**Goal:** Dịch vụ TCP phía client (một request một thời điểm, có timeout) và dựng `Form1` thành shell: menu strip, vùng đăng nhập, TabControl theo vai, status strip.

**Files:**
- Create: `QuanLyTro/Network/TcpClientService.cs`
- Create: `QuanLyTro/Protocol/ClientRequestException.cs`
- Modify: `QuanLyTro/Form1.cs`, `QuanLyTro/Form1.Designer.cs`
- Modify: `QuanLyTro/App.config`
- Test: `QuanLyTro/QuanLyTro.Tests/TcpRoundTripTests.cs`

**Interfaces:**
- Consumes: `RequestPacket`, `ResponsePacket`, `ActionNames`, `LoginResult`, `UserRole`.
- Produces: `TcpClientService.ConnectAsync(host, port, ct)`, `SendAsync<TReq, TResp>(action, data, ct)`, event `Disconnected`, property `Token`.

## Các bước thực hiện

- [ ] **Step 1: Viết integration test client-server thất bại**
  - Server stub trả `ResponsePacket.Ok(List<RoomDto>)`; client phải deserialize đúng kiểu.

- [ ] **Step 2: Chạy test xác nhận fail**
  - Lệnh: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter TcpClientService_DeserializesTypedResponse`

- [ ] **Step 3: Cài đặt `TcpClientService`**
  - Một kết nối lâu dài; `SemaphoreSlim(1,1)` quanh cặp write/read để response không lệch request.
  - Timeout 10 giây bằng linked `CancellationTokenSource`.
  - EOF → phát event `Disconnected`.
  - `Success=false` → ném `ClientRequestException(Message)` với message tiếng Việt từ server.

- [ ] **Step 4: Đọc cấu hình server từ `App.config`**
  - Thêm `ServerHost` (`127.0.0.1`) và `ServerPort` (`8888`). KHÔNG có ô nhập IP/Port trên UI — người dùng cuối không cấu hình hạ tầng.

- [ ] **Step 5: Dựng `Form1` thành shell chính**
  - Áp token từ `DESIGN.md` (`ColorTranslator.FromHtml`), font `Segoe UI 9pt`.
  - Title bar 32px nền `#0B0F12`, icon `#D95D39`.
  - Menu strip 36px: Hệ thống · Quản lý · Báo cáo · Trợ giúp.
  - Vùng đăng nhập: segmented 2 vai (Chủ trọ / Khách thuê), ô tài khoản + mật khẩu, nút `Đăng nhập`.
  - Sau login: `TabControl` dựng tab theo `Role` (Landlord 7 tab, Tenant 1 tab).
  - Status strip 22px: vai hiện tại + `.NET 8.0 CLR`. Không hiện IP/latency.

- [ ] **Step 6: Mở Designer kiểm tra và chạy thử**
  - Lệnh: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj"`
  - Mở `Form1.cs [Design]` trong Visual Studio; Expected: designer tải không lỗi.
  - Chạy Server + Client rồi đăng nhập; Expected: `TabControl` hiện đúng số tab theo vai.

- [ ] **Step 7: Commit**
  - Commit message: `feat: connect WinForms client over TCP (Task 10)`
