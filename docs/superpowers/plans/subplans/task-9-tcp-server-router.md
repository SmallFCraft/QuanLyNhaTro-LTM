# Sub-Plan D: TCP Server & Router (Task 9)

> **Agent Worker:** Subagent D (Chạy SAU Wave 2 — cần toàn bộ Service nghiệp vụ đã xong để route).
> **Phối hợp:** Wave 3, hạ tầng mạng — không đụng file Service, chỉ đọc interface.
> **Master Plan:** `docs/superpowers/plans/2026-09-09-quanly-phongtro-net8.md`

**Goal:** Dựng TCP listener đa kết nối, bộ định tuyến action, và enforce PermissionMatrix + session trước khi gọi service.

**Files:**
- Create: `QuanLyTro/QuanLyTro.Server/Network/TcpListenerServer.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Network/ClientHandler.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Network/RequestRouter.cs`
- Modify: `QuanLyTro/QuanLyTro.Server/Program.cs`
- Test: `QuanLyTro/QuanLyTro.Tests/TcpRoundTripTests.cs`

**Interfaces:**
- Consumes: `IAuthService`, `RoomService`, `TenantService`, `ContractService`, `UtilityService`, `InvoiceService`, `ReportService`, `SessionStore`, `PermissionMatrix`.
- Produces: `TcpListenerServer.StartAsync(port, ct)`, `RequestRouter.HandleAsync(RequestPacket)` → `ResponsePacket`.

## Các bước thực hiện

- [ ] **Step 1: Viết integration test round-trip client-server thất bại**
  - Test khởi động server trên cổng ngẫu nhiên, gửi `AUTH_LOGIN` thô qua `TcpClient`, đọc response JSON.

- [ ] **Step 2: Chạy test xác nhận fail**
  - Lệnh: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter TcpRoundTrip`

- [ ] **Step 3: Cài đặt `RequestRouter`**
  - Tra bảng action → handler. Mỗi request (trừ `AUTH_LOGIN`) phải:
    1. `SessionStore.TryGet(token)` → lấy `(userId, role)`.
    2. `PermissionMatrix.IsAllowed(action, role)` → nếu không, trả `ResponsePacket.Fail("Không có quyền.")`.
    3. Gọi service, bọc `BusinessRuleException` → `ResponsePacket.Fail(message)`, các exception khác → message chung.

- [ ] **Step 4: Cài đặt `ClientHandler`**
  - Mỗi kết nối một `async Task` xử lý độc lập (KHÔNG dùng `new Thread`).
  - Đọc `StreamReader.ReadLineAsync()`, giới hạn 1 MiB/packet, ghi `StreamWriter.WriteLineAsync()`.
  - Bắt exception ở tầng ngoài để một client hỏng không sập server.

- [ ] **Step 5: Cài đặt `TcpListenerServer` và nối vào `Program.cs`**
  - `TcpListener` bind `IPAddress.Any` cổng 8888, vòng lặp accept, giao `TcpClient` cho `ClientHandler`.
  - `Program.cs`: `--initialize-only` chạy schema rồi thoát; không có cờ thì chạy server.

- [ ] **Step 6: Chạy test và smoke-test thủ công**
  - Lệnh: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj"`
  - Chạy `dotnet run --project "QuanLyTro/QuanLyTro.Server"`, dùng `telnet`/script gửi 1 request, xác nhận response JSON.

- [ ] **Step 7: Commit**
  - Commit message: `feat: add concurrent TCP server and request routing (Task 9)`
