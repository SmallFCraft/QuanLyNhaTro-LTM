# Quản Lý Phòng Trọ .NET 8 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement plan task-by-task.

**Goal:** Nâng solution WinForms hiện có lên .NET 8 và xây hệ thống quản lý phòng trọ Client–Server TCP/IP theo SRS.

**Architecture:** Ba project: `QuanLyTro.Shared` chứa DTO và hợp đồng giao thức; `QuanLyTro.Server` sở hữu nghiệp vụ, TCP và MySQL; `QuanLyTro` là WinForms Client chỉ gửi yêu cầu rồi hiển thị kết quả. JSON UTF-8 phân khung bằng một dòng cho mỗi packet; transaction và unique constraint bảo vệ dữ liệu khi nhiều Client ghi đồng thời.

**Tech Stack:** C# 12, .NET 8, WinForms, `TcpClient`/`TcpListener`, `System.Text.Json`, ADO.NET, MySqlConnector, MySQL 8/InnoDB, MSTest.

**Spec:** `docs/superpowers/specs/2026-09-09-quanly-phongtro-srs-design.md`

## Global Constraints
- Target `net8.0` cho Shared/Server/Tests; target `net8.0-windows` cho WinForms Client.
- Client không được tham chiếu `MySqlConnector` hoặc mở kết nối DB.
- Mọi quy tắc BR-01 đến BR-13 chạy tại Server; ràng buộc DB bảo vệ uniqueness và foreign keys.
- Mỗi packet JSON UTF-8 kết thúc bằng `\n`; giới hạn một dòng packet là 1 MiB.
- Mọi SQL nhận dữ liệu người dùng phải dùng parameter; không nối chuỗi SQL.
- Tiền dùng `decimal`; tháng hóa đơn dùng chuỗi chuẩn `yyyy-MM`; ngày dùng `DateOnly` trong DTO.
- Mật khẩu dùng PBKDF2 (`Rfc2898DeriveBytes.Pbkdf2`) với salt riêng; token phiên sinh bằng `RandomNumberGenerator`.
- Không thêm abstraction ngoài interfaces tại biên Repository/Network cần cho kiểm thử.

---

## Bản đồ file

```text
QuanLyTro/
├── QuanLyTro.slnx
├── QuanLyTro.csproj                    # WinForms Client, nâng lên SDK-style .NET 8
├── Program.cs
├── Form1.cs / Form1.Designer.cs        # đổi thành shell chính, chưa đổi tên để giữ designer
├── Network/TcpClientService.cs
├── Protocol/ClientRequestException.cs
├── Forms/RoomsForm.cs (+ Designer)
├── Forms/TenantsForm.cs (+ Designer)
├── Forms/ContractsForm.cs (+ Designer)
├── Forms/UtilitiesForm.cs (+ Designer)
├── Forms/InvoicesForm.cs (+ Designer)
├── Forms/ReportsForm.cs (+ Designer)
├── QuanLyTro.Shared/
│   ├── QuanLyTro.Shared.csproj
│   ├── Models/*.cs
│   └── Protocol/{ActionNames,RequestPacket,ResponsePacket,JsonDefaults}.cs
├── QuanLyTro.Server/
│   ├── QuanLyTro.Server.csproj
│   ├── Program.cs
│   ├── appsettings.json
│   ├── Config/ServerOptions.cs
│   ├── Data/{Database,SchemaInitializer}.cs
│   ├── Security/{PasswordHasher,SessionStore}.cs
│   ├── Repositories/*.cs
│   ├── Services/*.cs
│   └── Network/{TcpServer,ClientHandler,RequestRouter}.cs
├── QuanLyTro.Tests/
│   ├── QuanLyTro.Tests.csproj
│   ├── ProtocolTests.cs
│   ├── BusinessRuleTests.cs
│   ├── PasswordHasherTests.cs
│   └── TcpRoundTripTests.cs
└── database/schema.sql
```

### Task 1: Nâng WinForms hiện có và tạo solution ba project

**Files:**
- Modify: `QuanLyTro/QuanLyTro.csproj`
- Modify: `QuanLyTro/Program.cs`
- Modify: `QuanLyTro/QuanLyTro.slnx`
- Create: `QuanLyTro/QuanLyTro.Shared/QuanLyTro.Shared.csproj`
- Create: `QuanLyTro/QuanLyTro.Server/QuanLyTro.Server.csproj`
- Create: `QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj`
- Delete: `QuanLyTro/App.config`
- Delete: `QuanLyTro/Properties/AssemblyInfo.cs`
- Delete: `QuanLyTro/Properties/Settings.settings`
- Delete: `QuanLyTro/Properties/Settings.Designer.cs`

**Interfaces:**
- Produces: buildable solution; Client references Shared, Server references Shared, Tests reference Shared and Server.

- [x] **Step 1: Thay project Client bằng SDK-style**
- [x] **Step 2: Hiện đại hóa entry point WinForms**
- [x] **Step 3: Tạo Shared, Server và Tests project**
- [x] **Step 4: Cập nhật `.slnx` và xóa metadata .NET Framework cũ**
- [x] **Step 5: Restore và build**
- [x] **Step 6: Commit**

### Task 2: Định nghĩa DTO và giao thức dùng chung

**Files:**
- Create: `QuanLyTro/QuanLyTro.Shared/Models/RoomDto.cs`
- Create: `QuanLyTro/QuanLyTro.Shared/Models/TenantDto.cs`
- Create: `QuanLyTro/QuanLyTro.Shared/Models/ContractDto.cs`
- Create: `QuanLyTro/QuanLyTro.Shared/Models/UtilityReadingDto.cs`
- Create: `QuanLyTro/QuanLyTro.Shared/Models/InvoiceDto.cs`
- Create: `QuanLyTro/QuanLyTro.Shared/Models/AuthAndReportDtos.cs`
- Create: `QuanLyTro/QuanLyTro.Shared/Protocol/ActionNames.cs`
- Create: `QuanLyTro/QuanLyTro.Shared/Protocol/RequestPacket.cs`
- Create: `QuanLyTro/QuanLyTro.Shared/Protocol/ResponsePacket.cs`
- Create: `QuanLyTro/QuanLyTro.Shared/Protocol/JsonDefaults.cs`
- Test: `QuanLyTro/QuanLyTro.Tests/ProtocolTests.cs`

**Interfaces:**
- Produces: `RequestPacket.Create<T>(string action, string? token, T data)`, `T GetData<T>()`, `ResponsePacket.Ok<T>()`, `ResponsePacket.Fail()`, `JsonDefaults.Options`.

- [x] **Step 1: Viết test protocol thất bại**
- [x] **Step 2: Chạy test để xác nhận fail**
- [x] **Step 3: Tạo DTO bất biến và enum**
- [x] **Step 4: Tạo packet giữ `JsonElement` thay vì JSON lồng dạng string**
- [x] **Step 5: Khai báo đủ 18 Action và các payload nhỏ**
- [x] **Step 6: Chạy test và toàn solution**
- [x] **Step 7: Commit**
```

### Task 3: Tạo schema MySQL và cấu hình Server

**Files:**
- Create: `QuanLyTro/database/schema.sql`
- Create: `QuanLyTro/QuanLyTro.Server/appsettings.json`
- Create: `QuanLyTro/QuanLyTro.Server/Config/ServerOptions.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Data/Database.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Data/SchemaInitializer.cs`
- Modify: `QuanLyTro/QuanLyTro.Server/QuanLyTro.Server.csproj`

**Interfaces:**
- Produces: `ServerOptions.Load(string path)`, `Database.OpenAsync(CancellationToken)`, `SchemaInitializer.InitializeAsync()`.

- [ ] **Step 1: Viết test cấu hình thất bại**

```csharp
[TestMethod]
public void ServerOptions_RejectsInvalidPort()
{
    Assert.ThrowsException<InvalidDataException>(() =>
        ServerOptions.Validate(new ServerOptions("server=localhost", 0)));
}
```

- [ ] **Step 2: Chạy test xác nhận fail**

```bash
dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter ServerOptions_RejectsInvalidPort
```

Expected: FAIL vì `ServerOptions` chưa tồn tại.

- [ ] **Step 3: Tạo cấu hình JSON và copy khi build**

```json
{
  "ConnectionString": "Server=127.0.0.1;Port=3306;Database=quanly_phongtro_nhs;User ID=root;Password=;SslMode=None;",
  "Port": 8888
}
```

```xml
<ItemGroup>
  <None Update="appsettings.json"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></None>
</ItemGroup>
```

- [ ] **Step 4: Tạo schema đúng BR-01, BR-03, BR-08 và FK**

Dùng SQL trong mục 3 của SRS; thêm `CHECK (price > 0)`, `CHECK (max_occupants > 0)`, `CHECK (end_date > start_date)`, `CHECK (new_electricity >= old_electricity)`, `CHECK (new_water >= old_water)`, index `(room_id,status)` cho hợp đồng và index `(status,billing_month)` cho hóa đơn. `SchemaInitializer` đọc embedded `schema.sql`, tách bằng marker `-- statement`, chạy tuần tự.

- [ ] **Step 5: Tạo connection factory**

```csharp
public sealed class Database(string connectionString)
{
    public async Task<MySqlConnection> OpenAsync(CancellationToken ct = default)
    {
        var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync(ct);
        return connection;
    }
}
```

- [ ] **Step 6: Chạy test và build Server**

```bash
dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter ServerOptions_RejectsInvalidPort
```

```bash
dotnet build "QuanLyTro/QuanLyTro.Server/QuanLyTro.Server.csproj"
```

Expected: PASS; build 0 errors.

- [ ] **Step 7: Smoke-test schema với Laragon đang chạy**

```bash
dotnet run --project "QuanLyTro/QuanLyTro.Server" -- --initialize-only
```

Expected: `Database initialized.`; chạy lần hai vẫn thành công.

- [ ] **Step 8: Commit**

```bash
git add QuanLyTro/database QuanLyTro/QuanLyTro.Server QuanLyTro/QuanLyTro.Tests
```

```bash
git commit -m "feat: add MySQL schema and server configuration"
```

### Task 4: Password, đăng nhập và session

**Files:**
- Create: `QuanLyTro/QuanLyTro.Server/Security/PasswordHasher.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Security/SessionStore.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Repositories/UserRepository.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Services/AuthService.cs`
- Test: `QuanLyTro/QuanLyTro.Tests/PasswordHasherTests.cs`

**Interfaces:**
- Produces: `PasswordHasher.Hash(string)`, `Verify(string,string)`, `SessionStore.Create(int)`, `TryGetUser(string,out int)`, `AuthService.LoginAsync(LoginRequest,CancellationToken)`.

- [ ] **Step 1: Viết test hash thất bại**

```csharp
[TestMethod]
public void PasswordHash_VerifiesCorrectPasswordOnly()
{
    var hash = PasswordHasher.Hash("MatKhau!123");
    Assert.IsTrue(PasswordHasher.Verify("MatKhau!123", hash));
    Assert.IsFalse(PasswordHasher.Verify("sai", hash));
    Assert.AreNotEqual(hash, PasswordHasher.Hash("MatKhau!123"));
}
```

- [ ] **Step 2: Chạy test xác nhận fail**

```bash
dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter PasswordHash_VerifiesCorrectPasswordOnly
```

- [ ] **Step 3: Implement PBKDF2 và so sánh constant-time**

```csharp
private const int Iterations = 210_000;
public static string Hash(string password)
{
    ArgumentException.ThrowIfNullOrWhiteSpace(password);
    var salt = RandomNumberGenerator.GetBytes(16);
    var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, 32);
    return $"pbkdf2-sha256${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
}
```

`Verify` parse bốn phần, giới hạn iteration hợp lệ, derive lại 32 byte, dùng `CryptographicOperations.FixedTimeEquals`.

- [ ] **Step 4: Tạo session in-memory hết hạn sau 8 giờ**

```csharp
public string Create(int userId)
{
    var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    sessions[token] = (userId, DateTimeOffset.UtcNow.AddHours(8));
    return token;
}
```

Dùng `ConcurrentDictionary`; xóa token hết hạn khi truy cập.

- [ ] **Step 5: Tạo UserRepository và AuthService**

SQL: `SELECT id, password_hash, full_name FROM users WHERE username=@username LIMIT 1`. Nếu sai thông tin, trả cùng thông báo `Tên đăng nhập hoặc mật khẩu không đúng.`; không tiết lộ tài khoản tồn tại.

- [ ] **Step 6: Chạy test**

```bash
dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter PasswordHash
```

Expected: PASS.

- [ ] **Step 7: Commit**

```bash
git add QuanLyTro/QuanLyTro.Server/Security QuanLyTro/QuanLyTro.Server/Repositories/UserRepository.cs QuanLyTro/QuanLyTro.Server/Services/AuthService.cs QuanLyTro/QuanLyTro.Tests/PasswordHasherTests.cs
```

```bash
git commit -m "feat: add secure server authentication"
```

### Task 5: Quản lý phòng và người thuê

**Files:**
- Create: `QuanLyTro/QuanLyTro.Server/Repositories/RoomRepository.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Repositories/TenantRepository.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Services/RoomService.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Services/TenantService.cs`
- Test: `QuanLyTro/QuanLyTro.Tests/BusinessRuleTests.cs`

**Interfaces:**
- Produces: CRUD phòng; list/add/update/checkout người thuê; `BusinessRuleException` chứa thông báo thân thiện.

- [ ] **Step 1: Viết test validation thuần thất bại**

```csharp
[DataTestMethod]
[DataRow("", 1000000, 2)]
[DataRow("P1", 0, 2)]
[DataRow("P1", 1000000, 0)]
public void RoomService_RejectsInvalidRoom(string number, double price, int capacity)
{
    Assert.ThrowsException<BusinessRuleException>(() =>
        RoomService.Validate(new RoomDto(0, number, (decimal)price, capacity, RoomStatus.Available, null, 0)));
}
```

- [ ] **Step 2: Chạy test xác nhận fail**

```bash
dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter RoomService_RejectsInvalidRoom
```

- [ ] **Step 3: Implement RoomRepository parameter hóa**

Cung cấp `GetAllAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`, `CanDeleteAsync`. `GetAllAsync` dùng `LEFT JOIN tenants t ON t.room_id=r.id`, `COUNT(t.id)`, `GROUP BY r.id`. Bắt MySQL duplicate key `1062`, chuyển thành `BusinessRuleException("Số phòng đã tồn tại.")`.

- [ ] **Step 4: Implement RoomService BR-01, BR-12**

Validation: số phòng trim/non-empty/max 20; giá > 0; sức chứa > 0. Xóa chỉ khi repository xác nhận không tenant và không active contract.

- [ ] **Step 5: Implement TenantRepository/Service BR-02, BR-03**

`TenantService.AddAsync` mở transaction, khóa room bằng `SELECT ... FOR UPDATE`, đếm tenant, từ chối nếu đủ; insert tenant parameter hóa; cập nhật room thành `Rented`; commit. `CheckoutAsync` set `room_id=NULL`; nếu count còn lại bằng 0 và không active contract, set room `Available`.

- [ ] **Step 6: Chạy unit tests và DB integration scenario**

```bash
dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj"
```

Chạy Server với DB test, gửi hai lệnh thêm cùng `RoomNumber`; Expected: một success, một `Số phòng đã tồn tại.`.

- [ ] **Step 7: Commit**

```bash
git add QuanLyTro/QuanLyTro.Server/Repositories QuanLyTro/QuanLyTro.Server/Services QuanLyTro/QuanLyTro.Tests/BusinessRuleTests.cs
```

```bash
git commit -m "feat: add room and tenant business logic"
```

### Task 6: Hợp đồng và điện nước

**Files:**
- Create: `QuanLyTro/QuanLyTro.Server/Repositories/ContractRepository.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Repositories/UtilityRepository.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Services/ContractService.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Services/UtilityService.cs`
- Modify: `QuanLyTro/QuanLyTro.Tests/BusinessRuleTests.cs`

**Interfaces:**
- Produces: create/terminate contract; get previous/record utility; calculation helpers.

- [ ] **Step 1: Viết test BR-06/BR-07 và công thức điện nước**

```csharp
[TestMethod]
public void Contract_RequiresEndAfterStart() =>
    Assert.ThrowsException<BusinessRuleException>(() => ContractService.Validate(
        new ContractDto(0, 1, 1, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 1), 2_000_000, 0, ContractStatus.Active, null)));

[TestMethod]
public void Utility_CalculatesAmounts()
{
    var reading = new UtilityReadingDto(0, 1, "2026-09", 100, 125, 3500, 10, 13, 10000);
    Assert.AreEqual(87_500m, UtilityService.ElectricityAmount(reading));
    Assert.AreEqual(30_000m, UtilityService.WaterAmount(reading));
}
```

- [ ] **Step 2: Chạy test xác nhận fail**

```bash
dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter "Contract|Utility"
```

- [ ] **Step 3: Implement ContractService BR-04, BR-05, BR-06, BR-13**

Trong transaction: khóa room; kiểm tra representative có `room_id` đúng; kiểm tra không active contract; insert contract; cập nhật room `Rented`; commit. Terminate chỉ active contract, lưu lý do bắt buộc, set `Terminated`.

- [ ] **Step 4: Implement UtilityService BR-07, BR-08**

Validate tháng bằng `DateOnly.TryParseExact(value + "-01", "yyyy-MM-dd", ...)`; chỉ số/rate không âm; new >= old. `GetPreviousAsync` lấy record tháng mới nhất nhỏ hơn tháng yêu cầu. Duplicate `(room_id,billing_month)` map thành thông báo rõ.

- [ ] **Step 5: Chạy tests**

```bash
dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj"
```

Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add QuanLyTro/QuanLyTro.Server/Repositories/ContractRepository.cs QuanLyTro/QuanLyTro.Server/Repositories/UtilityRepository.cs QuanLyTro/QuanLyTro.Server/Services/ContractService.cs QuanLyTro/QuanLyTro.Server/Services/UtilityService.cs QuanLyTro/QuanLyTro.Tests/BusinessRuleTests.cs
```

```bash
git commit -m "feat: add contracts and utility readings"
```

### Task 7: Hóa đơn, thanh toán và báo cáo

**Files:**
- Create: `QuanLyTro/QuanLyTro.Server/Repositories/InvoiceRepository.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Repositories/ReportRepository.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Services/InvoiceService.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Services/ReportService.cs`
- Modify: `QuanLyTro/QuanLyTro.Tests/BusinessRuleTests.cs`

**Interfaces:**
- Produces: create/list/pay invoices; summary and temporary residence rows.

- [ ] **Step 1: Viết test BR-10 thất bại**

```csharp
[TestMethod]
public void Invoice_TotalEqualsAllComponents()
{
    Assert.AreEqual(2_167_500m, InvoiceService.CalculateTotal(2_000_000m, 87_500m, 30_000m, 50_000m));
    Assert.ThrowsException<BusinessRuleException>(() => InvoiceService.CalculateTotal(1, 0, 0, -2));
}
```

- [ ] **Step 2: Chạy test xác nhận fail**

```bash
dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter Invoice_TotalEqualsAllComponents
```

- [ ] **Step 3: Implement tạo hóa đơn BR-08 đến BR-10, BR-13**

Trong transaction: khóa room và utility record; lấy active contract; từ chối thiếu contract/reading; tính từng khoản server-side; insert invoice với unique room/month; commit. Không nhận amount điện/nước/tổng từ Client.

- [ ] **Step 4: Implement thanh toán BR-11**

SQL atomic: `UPDATE invoices SET status='Paid', paid_at=UTC_TIMESTAMP() WHERE id=@id AND status='Unpaid'`; affected rows phải là 1. Không cung cấp endpoint update/delete hóa đơn.

- [ ] **Step 5: Implement báo cáo**

`SummaryReportDto`: total rooms, available/rented rooms, current tenants, paid/unpaid amount theo tháng. `ResidenceExportDto`: full name, DOB, ID card, hometown, room number; query chỉ tenant có room.

- [ ] **Step 6: Chạy tests**

```bash
dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj"
```

Expected: PASS.

- [ ] **Step 7: Commit**

```bash
git add QuanLyTro/QuanLyTro.Server/Repositories/InvoiceRepository.cs QuanLyTro/QuanLyTro.Server/Repositories/ReportRepository.cs QuanLyTro/QuanLyTro.Server/Services/InvoiceService.cs QuanLyTro/QuanLyTro.Server/Services/ReportService.cs QuanLyTro/QuanLyTro.Tests/BusinessRuleTests.cs
```

```bash
git commit -m "feat: add invoices payments and reports"
```

### Task 8: TCP Server, router và xử lý nhiều Client

**Files:**
- Create: `QuanLyTro/QuanLyTro.Server/Network/TcpServer.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Network/ClientHandler.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Network/RequestRouter.cs`
- Modify: `QuanLyTro/QuanLyTro.Server/Program.cs`
- Test: `QuanLyTro/QuanLyTro.Tests/TcpRoundTripTests.cs`

**Interfaces:**
- Produces: `TcpServer.RunAsync(CancellationToken)`, `RequestRouter.RouteAsync(RequestPacket,CancellationToken)`.

- [ ] **Step 1: Viết TCP round-trip test thất bại**

```csharp
[TestMethod]
public async Task Server_ReturnsOneJsonResponsePerRequestLine()
{
    var router = new StubRouter(ResponsePacket.Ok(new { Pong = true }));
    await using var server = await TestTcpServer.StartAsync(router);
    using var client = new TcpClient();
    await client.ConnectAsync(IPAddress.Loopback, server.Port);
    using var writer = new StreamWriter(client.GetStream(), new UTF8Encoding(false)) { AutoFlush = true };
    using var reader = new StreamReader(client.GetStream(), Encoding.UTF8);
    await writer.WriteLineAsync("{\"Action\":\"PING\",\"Token\":null,\"Data\":{}}");
    var response = JsonSerializer.Deserialize<ResponsePacket>(await reader.ReadLineAsync(), JsonDefaults.Options)!;
    Assert.IsTrue(response.Success);
}
```

- [ ] **Step 2: Chạy test xác nhận fail**

```bash
dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter Server_ReturnsOneJsonResponsePerRequestLine
```

- [ ] **Step 3: Implement accept loop không tạo raw Thread**

```csharp
while (!ct.IsCancellationRequested)
{
    var client = await listener.AcceptTcpClientAsync(ct);
    _ = HandleSafelyAsync(client, ct);
}
```

Theo yêu cầu đa Client, mỗi connection có một async Task độc lập. Không dùng `new Thread`; async I/O nhẹ và vẫn đáp ứng độc lập.

- [ ] **Step 4: Implement ClientHandler an toàn**

Dùng UTF-8 strict; `ReadLineAsync(ct)`; nếu dòng > 1 MiB, phản hồi lỗi rồi đóng; deserialize lỗi trả `JSON không hợp lệ.`; mọi request nhận đúng một response line; log exception server-side, không gửi stack trace cho Client.

- [ ] **Step 5: Implement RequestRouter đủ 18 action**

`AUTH_LOGIN` không cần token. Mọi action khác kiểm tra `SessionStore.TryGetUser`; switch Action gọi đúng service; action lạ trả `Hành động không được hỗ trợ.`; `BusinessRuleException` thành `Success=false` và message; exception DB thành thông báo chung.

- [ ] **Step 6: Wire Program và Ctrl+C cancellation**

Load config; `--initialize-only` chạy schema rồi exit; mode thường initialize schema, tạo repositories/services/router/server, lắng nghe `IPAddress.Any:8888`; Ctrl+C cancel token và dừng sạch.

- [ ] **Step 7: Chạy TCP tests và tải đồng thời**

```bash
dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter Tcp
```

Thêm test mở 20 Client đồng thời gửi `ROOM_GET_ALL`; Expected: 20 response hợp lệ, không treo trong 5 giây.

- [ ] **Step 8: Commit**

```bash
git add QuanLyTro/QuanLyTro.Server/Network QuanLyTro/QuanLyTro.Server/Program.cs QuanLyTro/QuanLyTro.Tests/TcpRoundTripTests.cs
```

```bash
git commit -m "feat: add concurrent TCP server and request routing"
```

### Task 9: TCP Client service và kết nối UI

**Files:**
- Create: `QuanLyTro/Network/TcpClientService.cs`
- Create: `QuanLyTro/Protocol/ClientRequestException.cs`
- Modify: `QuanLyTro/Form1.cs`
- Modify: `QuanLyTro/Form1.Designer.cs`
- Test: `QuanLyTro/QuanLyTro.Tests/TcpRoundTripTests.cs`

**Interfaces:**
- Produces: `ConnectAsync(host,port,ct)`, `SendAsync<TRequest,TResponse>(action,data,ct)`, `Disconnected` event, `Token` property.

- [ ] **Step 1: Viết integration test client-server thất bại**

```csharp
[TestMethod]
public async Task TcpClientService_DeserializesTypedResponse()
{
    await using var server = await TestTcpServer.StartAsync(new StubRouter(
        ResponsePacket.Ok(new[] { new RoomDto(1, "P1", 1_000_000, 2, RoomStatus.Available, null, 0) })));
    await using var client = new TcpClientService();
    await client.ConnectAsync("127.0.0.1", server.Port, CancellationToken.None);
    var rooms = await client.SendAsync<object, List<RoomDto>>(ActionNames.RoomGetAll, new { }, CancellationToken.None);
    Assert.AreEqual("P1", rooms[0].RoomNumber);
}
```

- [ ] **Step 2: Chạy test xác nhận fail**

```bash
dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter TcpClientService_DeserializesTypedResponse
```

- [ ] **Step 3: Implement Client service với một request tại một thời điểm**

Dùng một connection lâu dài, `SemaphoreSlim(1,1)` quanh write/read pair để response không lệch request; timeout 10 giây bằng linked `CancellationTokenSource`; EOF phát `Disconnected`; `Success=false` ném `ClientRequestException(Message)`.

- [ ] **Step 4: Biến Form1 thành connection shell**

Thêm input IP mặc định `127.0.0.1`, port `8888`, nút `Kết nối`, label trạng thái, `TabControl` bị disable khi chưa kết nối. Event handler async, bắt exception, không block UI thread.

- [ ] **Step 5: Chạy tests và mở Designer**

```bash
dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj"
```

Mở `Form1.cs [Design]` trong Visual Studio; Expected: designer tải không lỗi. Chạy Server + Client; Expected: trạng thái đổi `Đã kết nối`.

- [ ] **Step 6: Commit**

```bash
git add QuanLyTro/Network QuanLyTro/Protocol QuanLyTro/Form1.cs QuanLyTro/Form1.Designer.cs QuanLyTro/QuanLyTro.Tests/TcpRoundTripTests.cs
```

```bash
git commit -m "feat: connect WinForms client over TCP"
```

### Task 10: Các màn hình nghiệp vụ WinForms

**Files:**
- Create: `QuanLyTro/Forms/RoomsForm.cs` and `.Designer.cs`
- Create: `QuanLyTro/Forms/TenantsForm.cs` and `.Designer.cs`
- Create: `QuanLyTro/Forms/ContractsForm.cs` and `.Designer.cs`
- Create: `QuanLyTro/Forms/UtilitiesForm.cs` and `.Designer.cs`
- Create: `QuanLyTro/Forms/InvoicesForm.cs` and `.Designer.cs`
- Create: `QuanLyTro/Forms/ReportsForm.cs` and `.Designer.cs`
- Modify: `QuanLyTro/Form1.cs`
- Modify: `QuanLyTro/Form1.Designer.cs`

**Interfaces:**
- Consumes: singleton `TcpClientService` từ Form1.
- Produces: UI cho US-01 đến US-22; không chứa business-rule decisions.

- [ ] **Step 1: Tạo `RoomsForm`**

DataGridView readonly hiển thị số phòng, giá, sức chứa, số người, trạng thái; controls add/update; nút delete có `MessageBox` xác nhận; mọi thao tác gọi `ROOM_*`, sau success reload grid.

- [ ] **Step 2: Tạo `TenantsForm`**

ComboBox phòng; grid tenant; inputs bắt buộc; Add/Update/Checkout gọi `TENANT_*`; Client chỉ kiểm tra trường rỗng/định dạng ngày, Server quyết định capacity và CCCD uniqueness.

- [ ] **Step 3: Tạo `ContractsForm`**

Chọn room, representative, dates, rental/deposit; Create/Terminate; hiển thị thông báo lỗi Server. Thêm tab `Sắp hết hạn` lọc client-side từ dữ liệu Server trong 30 ngày; nếu protocol thiếu list contract, bổ sung action `CONTRACT_GET_ALL` vào spec/protocol trước khi code.

- [ ] **Step 4: Tạo `UtilitiesForm`**

Chọn room/month; gọi `UTILITY_GET_PREVIOUS` điền chỉ số cũ; NumericUpDown cho chỉ số mới và rates; preview tiền chỉ để UX, kết quả chính thức lấy từ Server khi `UTILITY_RECORD`.

- [ ] **Step 5: Tạo `InvoicesForm`**

Chọn tháng, list invoices và detail breakdown; nút Create và Pay; paid rows khóa action; filter unpaid. Server tính toàn bộ amount ngoại trừ `OtherFees`.

- [ ] **Step 6: Tạo `ReportsForm` và CSV bằng stdlib**

Gọi `REPORT_SUMMARY`; hiển thị KPI và bảng tháng. Gọi `EXPORT_RESIDENCE`; dùng `StreamWriter` UTF-8 BOM, escape CSV bằng cách bọc dấu `"` và nhân đôi dấu `"` bên trong. Không thêm Excel package.

- [ ] **Step 7: Gắn forms vào tab shell, kiểm tra accessibility cơ bản**

Đặt `AccessibleName`, tab order hợp lý, labels liên kết controls, font Segoe UI 9+, button có text rõ; keyboard navigation hoạt động; lỗi hiển thị bằng MessageBox và giữ input.

- [ ] **Step 8: Build và manual flow**

```bash
dotnet build "QuanLyTro/QuanLyTro.slnx"
```

Chạy flow: phòng trống → thêm tenant → tạo contract → chốt utility → tạo invoice → pay → report; Expected: trạng thái đúng từng bước, Client không treo.

- [ ] **Step 9: Commit**

```bash
git add QuanLyTro/Forms QuanLyTro/Form1.cs QuanLyTro/Form1.Designer.cs
```

```bash
git commit -m "feat: add WinForms management screens"
```

### Task 11: Hoàn thiện kiểm thử SRS và tài liệu chạy

**Files:**
- Create: `QuanLyTro/README.md`
- Create: `QuanLyTro/docs/test-cases.md`
- Modify: `QuanLyTro/QuanLyTro.Tests/BusinessRuleTests.cs`
- Modify: `QuanLyTro/QuanLyTro.Tests/TcpRoundTripTests.cs`

**Interfaces:**
- Produces: hướng dẫn setup/reproducible acceptance checklist; bằng chứng US/BR coverage.

- [ ] **Step 1: Thêm test matrix BR-01 đến BR-13**

Mỗi BR có ít nhất một happy path và một rejection path. Với BR cần DB, dùng database riêng `quanly_phongtro_nhs_test`, reset tables trong test setup; không chạy vào DB dev.

- [ ] **Step 2: Thêm test mất kết nối và malformed input**

Test Server đóng connection khi Client đang chờ; Expected: `IOException`/`ClientRequestException`, event `Disconnected` một lần. Test JSON lỗi, action lạ, packet >1 MiB; Server sống và Client kế tiếp vẫn kết nối được.

- [ ] **Step 3: Thêm concurrent uniqueness test US-22**

Hai Client gửi `ROOM_ADD` cùng `RoomNumber` qua `Task.WhenAll`; assert đúng một `Success=true`, một `Success=false`; DB có đúng một row.

- [ ] **Step 4: Viết README**

Bao gồm prerequisites: Visual Studio 2022 17.8+ với `.NET desktop development`, .NET 8 SDK, Laragon/MySQL; cách đổi connection string; initialize schema; seed admin bằng utility CLI có hash PBKDF2; startup multiple projects; port/firewall LAN; command build/test.

- [ ] **Step 5: Viết acceptance checklist ánh xạ US-01–US-22 và BR-01–BR-13**

Mỗi dòng gồm mã, bước chuẩn bị, thao tác, kết quả mong đợi, trạng thái. Không thêm US-23 vì báo cáo đánh dấu Could; ghi rõ deferred.

- [ ] **Step 6: Chạy full verification**

```bash
dotnet format "QuanLyTro/QuanLyTro.slnx" --verify-no-changes
```

```bash
dotnet build "QuanLyTro/QuanLyTro.slnx" -c Release --no-restore
```

```bash
dotnet test "QuanLyTro/QuanLyTro.slnx" -c Release --no-build
```

Expected: format sạch, build 0 warnings/errors, tất cả tests PASS.

- [ ] **Step 7: Chạy acceptance flow LAN**

Server máy chính; hai Client từ hai tiến trình/máy; hoàn thành luồng tháng và concurrent duplicate room; đo request LAN <500 ms trong điều kiện lab.

- [ ] **Step 8: Commit**

```bash
git add QuanLyTro/README.md QuanLyTro/docs QuanLyTro/QuanLyTro.Tests
```

```bash
git commit -m "test: verify SRS acceptance scenarios"
```
