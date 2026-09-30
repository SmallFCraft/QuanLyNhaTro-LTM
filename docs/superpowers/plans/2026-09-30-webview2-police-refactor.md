# WebView2 + Police Refactor Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Chuyển đổi toàn bộ tầng UI của ứng dụng Client từ WinForms UserControls truyền thống sang giao diện HTML/CSS phong cách *Terracotta & Slate* thông qua `Microsoft.Web.WebView2`, đồng thời hỗ trợ đầy đủ tác nhân thứ ba: **Công an phường (Police)** với phân quyền chỉ đọc (Server-enforced), tra cứu công dân, tạm trú và xuất lịch sử biến động.

**Architecture:** Client vẫn là WinForms .NET 8 desktop nhưng dùng duy nhất `Form1` làm shell chứa control native `WebView2`. WebView2 nạp `index.html` (đóng gói từ `docs/superpowers/mockups/giaodien.html`). Giao tiếp 2 chiều C# ↔ JS qua `WebMessage`: JS gửi lệnh `{ requestId, action, data }`, C# gọi `TcpClientService` gửi gói tin TCP JSON 1 dòng tới Server, sau đó trả phản hồi JSON về JS để render bảng/KPI. Token phiên do C# giữ bí mật. Mọi phân quyền (BR-14, BR-16) do Server kiểm soát.

**Tech Stack:** C# .NET 8 (`net8.0-windows` & `net8.0`), `Microsoft.Web.WebView2` NuGet (1.0.2903.40+), HTML5 / CSS3 / Vanilla JS (không framework, dùng token từ `DESIGN.md`), FontAwesome 6.5.1 qua CDN hoặc local, TCP Socket JSON (`TcpClientService`), MySQL 8.0 (Server).

**Spec:** [`docs/superpowers/specs/2026-09-30-police-webview2-design.md`](../specs/2026-09-30-police-webview2-design.md) (kèm theo [`2026-09-10-quanly-phongtro-multi-actor-design.md`](../specs/2026-09-10-quanly-phongtro-multi-actor-design.md) và [`DESIGN.md`](../../../DESIGN.md)).

## Global Constraints

- **Platform:** Client là `net8.0-windows` (.NET 8 Windows Forms); Server/Shared/Tests là `net8.0`.
- **Runtime:** `Microsoft.Web.WebView2` là dependency duy nhất được thêm vào `QuanLyTro.csproj`. Không cài thêm package UI nặng hay web framework (React/Vue/Blazor).
- **Core mạng:** Không dùng HTTP/REST cho nghiệp vụ. 100% I/O qua `TcpClientService` gửi JSON một dòng kết thúc `\n` tới Server cổng 8888.
- **Bảo mật token:** Token phiên được giữ trong C# `TcpClientService.Token`, tuyệt đối không lưu vào `localStorage`, cookie hoặc lộ ra DOM toàn cục.
- **Server là nguồn chân lý:** Mọi thao tác ghi từ vai `Police` đều bị Server từ chối kể cả khi Client bị can thiệp JS (BR-16).
- **Thiết kế giao diện:** 100% màu sắc và component tuân thủ `DESIGN.md` (Terracotta `#D95D39`, Canvas `#0B0F12`, Base `#101417`, Card `#181C1F`, Sage `#8BD7A3`, Amber `#E0AF68`). Ngoại lệ duy nhất là màu vai Police `#70A9FF`.

## Review Focus

- **RF-1: Đăng nhập vai Police:** Tài khoản police đăng nhập thành công phải nhận được `Role = UserRole.Police`, JS chuyển đúng sang shell `#police` và không lộ các tab của Landlord.
- **RF-2: Chặn ghi vai Police (BR-16):** Nếu JS hoặc người dùng cố gửi `ROOM_ADD`, `TENANT_ADD`, `INVOICE_PAY` dưới token Police, Server phải trả về `Success = false` và thông báo `"Vai Công an phường chỉ được đọc."`.
- **RF-3: Cầu nối bất đồng bộ WebMessage:** Khi người dùng gửi nhiều thao tác liên tiếp, các phản hồi C# trả về qua `PostWebMessageAsJson` phải đúng `requestId` để JS không hiển thị nhầm dữ liệu giữa các tab.
- **RF-4: Mất kết nối TCP:** Khi Server ngắt kết nối đột ngột, C# bắt sự kiện `Disconnected`, gửi event `SERVER_DISCONNECTED` vào WebView2; UI hiển thị thanh cảnh báo đỏ và khóa form mà không sập app.
- **RF-5: Xuất file biến động lưu trú (PM-05.2):** Kiểm tra ngày kết thúc $\ge$ ngày bắt đầu phía Server; ghi file thành công trả về đường dẫn file và số lượng bản ghi.

---

## File Structure

```
QuanLyTro/
├── Form1.cs                                   [Shell WebView2, lắng nghe WebMessage]
├── Form1.Designer.cs                          [Chứa WebView2 control dock Fill]
├── QuanLyTro.csproj                           [Thêm NuGet Microsoft.Web.WebView2]
├── Assets/
│   └── wwwroot/
│       ├── index.html                         [4 cửa sổ: login, landlord, police, tenant]
│       ├── css/style.css                      [CSS trích xuất từ giaodien.html]
│       └── js/
│           ├── bridge.js                      [Cầu nối WebMessage, không logic màn hình]
│           ├── login.js                       [Đăng nhập + định tuyến theo vai]
│           ├── landlord.js                    [7 tab chủ trọ + render từ Server]
│           ├── police.js                      [3 tab công an, chỉ đọc + xuất file]
│           └── tenant.js                      [1 tab hóa đơn của tôi]
├── Network/
│   ├── TcpClientService.cs                    [Giữ nguyên core socket]
│   └── WebMessageBridge.cs                    [Bộ điều phối thông điệp JS ↔ C#]
QuanLyTro.Shared/
├── Models/
│   ├── AuthAndReportDtos.cs                   [Thêm UserRole.Police, DTO lịch sử lưu trú]
│   └── ResidenceDtos.cs                       [DTO biến động: ResidenceHistoryDto, ExportRequest]
├── Protocol/
│   └── ActionNames.cs                         [Thêm RESIDENCE_HISTORY_GET, EXPORT_RESIDENCE_HISTORY]
QuanLyTro.Server/
├── Network/
│   ├── PermissionMatrix.cs                    [Thêm Police vào ma trận quyền, chặn toàn bộ lệnh ghi]
│   └── RequestRouter.cs                       [Route 2 action lưu trú mới]
├── Services/
│   └── ResidenceService.cs                    [Nghiệp vụ tra cứu lưu trú và xuất file]
└── Repositories/
    └── ResidenceRepository.cs                 [Truy vấn bảng tenants, rooms, residence_history]
QuanLyTro.Tests/
├── AssetPackagingTests.cs                     [Task 4a — wwwroot được copy khi build]
├── LandlordTabContractTests.cs                [Task 4b — 7 tab khớp HTML/JS]
├── PoliceShellContractTests.cs                [Task 4c — 3 tab, không nút ghi]
├── TenantShellContractTests.cs                [Task 4d — 1 tab, không nút ghi]
├── PermissionMatrixTests.cs                   [Test ma trận quyền cho vai Police]
├── WebMessageBridgeTests.cs                   [Test bộ điều phối thông điệp JSON]
├── ResidenceServiceTests.cs                   [Test nghiệp vụ tra cứu lưu trú]
└── PoliceIntegrationTests.cs                  [Task 7 — kiểm tra chặn ghi E2E]
```

---

### Task 1: Mở rộng Shared Protocol & Models cho vai Công an phường

**Files:**
- Modify: `QuanLyTro/QuanLyTro.Shared/Models/AuthAndReportDtos.cs:4-8`
- Create: `QuanLyTro/QuanLyTro.Shared/Models/ResidenceDtos.cs`
- Modify: `QuanLyTro/QuanLyTro.Shared/Protocol/ActionNames.cs:33-47`
- Test: `QuanLyTro/QuanLyTro.Tests/ProtocolTests.cs`

**Interfaces:**
- Consumes: `JsonDefaults.Options`
- Produces: `UserRole.Police`, `ActionNames.ResidenceHistoryGet`, `ActionNames.ExportResidenceHistory`, `ResidenceHistoryDto`, `ExportHistoryRequest`, `ExportResult`

- [ ] **Step 1: Viết test kiểm tra ActionNames và UserRole có Police**

Sửa `QuanLyTro/QuanLyTro.Tests/ProtocolTests.cs`, thêm test method:

```csharp
[TestMethod]
public void ActionNames_ContainsPoliceActions_AndUserRoleHasPolice()
{
    Assert.IsTrue(ActionNames.All.Contains(ActionNames.ResidenceHistoryGet));
    Assert.IsTrue(ActionNames.All.Contains(ActionNames.ExportResidenceHistory));
    Assert.IsTrue(Enum.IsDefined(typeof(UserRole), UserRole.Police));
}
```

- [ ] **Step 2: Chạy test để xác nhận test FAIL**

Run: `dotnet test QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj --filter "FullyQualifiedName~ActionNames_ContainsPoliceActions" -v m`
Expected: FAIL với lỗi compile (chưa có `UserRole.Police` và các hằng số action).

- [ ] **Step 3: Cập nhật Models và Protocol**

Trong `QuanLyTro/QuanLyTro.Shared/Models/AuthAndReportDtos.cs`:
```csharp
public enum UserRole
{
    Landlord,
    Tenant,
    Police,
}
```

Tạo file mới `QuanLyTro/QuanLyTro.Shared/Models/ResidenceDtos.cs`:
```csharp
namespace QuanLyTro.Shared.Models;

public sealed record ResidenceHistoryDto(
    int Id,
    string FullName,
    string IdCard,
    string RoomNumber,
    string EventType, // "Vào", "Ra", "Chuyển phòng"
    DateTime EventDate,
    string? Notes);

public sealed record ExportHistoryRequest(
    DateTime FromDate,
    DateTime ToDate,
    string Format, // "CSV", "Excel", "PDF"
    string? RoomNumber,
    string? EventType);

public sealed record ExportResult(
    string FilePath,
    int RowCount);
```

Trong `QuanLyTro/QuanLyTro.Shared/Protocol/ActionNames.cs`:
Thêm 2 hằng số vào class và mảng `All`:
```csharp
public const string ResidenceHistoryGet = "RESIDENCE_HISTORY_GET";
public const string ExportResidenceHistory = "EXPORT_RESIDENCE_HISTORY";
```

- [ ] **Step 4: Chạy test để xác nhận PASS**

Run: `dotnet test QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj --filter "FullyQualifiedName~ActionNames_ContainsPoliceActions" -v m`
Expected: PASS 1/1 test.

- [ ] **Step 5: Commit**

```bash
git add QuanLyTro/QuanLyTro.Shared/ QuanLyTro/QuanLyTro.Tests/ProtocolTests.cs
git commit -m "feat(protocol): add Police role and residence history actions"
```

---

### Task 2: Server PermissionMatrix & Chặn ghi cho vai Police (BR-16)

**Files:**
- Modify: `QuanLyTro/QuanLyTro.Server/Network/PermissionMatrix.cs`
- Modify: `QuanLyTro/QuanLyTro.Tests/PermissionMatrixTests.cs`

**Interfaces:**
- Consumes: `UserRole.Police`, `ActionNames`
- Produces: `PermissionMatrix.IsAllowed(action, UserRole.Police)`

- [ ] **Step 1: Viết test kiểm tra quyền của Police trong PermissionMatrixTests**

Sửa `QuanLyTro/QuanLyTro.Tests/PermissionMatrixTests.cs`:

```csharp
[TestMethod]
public void Police_HasReadOnlyAccessToAllowedActions_AndRejectedOnAllWrites()
{
    // Được phép đọc
    Assert.IsTrue(PermissionMatrix.IsAllowed(ActionNames.AuthLogin, UserRole.Police));
    Assert.IsTrue(PermissionMatrix.IsAllowed(ActionNames.RoomGetAll, UserRole.Police));
    Assert.IsTrue(PermissionMatrix.IsAllowed(ActionNames.TenantGetByRoom, UserRole.Police));
    Assert.IsTrue(PermissionMatrix.IsAllowed(ActionNames.ExportResidence, UserRole.Police));
    Assert.IsTrue(PermissionMatrix.IsAllowed(ActionNames.ResidenceHistoryGet, UserRole.Police));
    Assert.IsTrue(PermissionMatrix.IsAllowed(ActionNames.ExportResidenceHistory, UserRole.Police));

    // Bị cấm toàn bộ thao tác ghi (BR-16)
    Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.RoomAdd, UserRole.Police));
    Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.RoomUpdate, UserRole.Police));
    Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.RoomDelete, UserRole.Police));
    Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.TenantAdd, UserRole.Police));
    Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.TenantUpdate, UserRole.Police));
    Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.TenantCheckout, UserRole.Police));
    Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.TenantDelete, UserRole.Police));
    Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.ContractCreate, UserRole.Police));
    Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.ContractTerminate, UserRole.Police));
    Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.UtilityRecord, UserRole.Police));
    Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.InvoiceCreate, UserRole.Police));
    Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.InvoicePay, UserRole.Police));
    Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.InvoiceGetMine, UserRole.Police));
}
```

- [ ] **Step 2: Chạy test để xác nhận FAIL**

Run: `dotnet test QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj --filter "FullyQualifiedName~Police_HasReadOnlyAccess" -v m`
Expected: FAIL vì `PermissionMatrix` chưa cấu hình vai `Police`.

- [ ] **Step 3: Cập nhật PermissionMatrix.cs**

Mở `QuanLyTro/QuanLyTro.Server/Network/PermissionMatrix.cs`:
Cập nhật bảng `Allowed`:
```csharp
private static readonly HashSet<UserRole> LandlordAndPolice = [UserRole.Landlord, UserRole.Police];

// Trong từ điển Allowed:
[ActionNames.AuthLogin] = Roles(UserRole.Landlord, UserRole.Tenant, UserRole.Police),
[ActionNames.RoomGetAll] = LandlordAndPolice,
[ActionNames.TenantGetByRoom] = LandlordAndPolice,
[ActionNames.ExportResidence] = LandlordAndPolice,
[ActionNames.ResidenceHistoryGet] = LandlordAndPolice,
[ActionNames.ExportResidenceHistory] = LandlordAndPolice,
```
Đồng thời đảm bảo `AllActions_AreRegisteredInMatrix` vẫn đúng với mọi action mới.

- [ ] **Step 4: Chạy test để xác nhận PASS**

Run: `dotnet test QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj --filter "FullyQualifiedName~PermissionMatrixTests" -v m`
Expected: PASS toàn bộ test ma trận quyền.

- [ ] **Step 5: Commit**

```bash
git add QuanLyTro/QuanLyTro.Server/Network/PermissionMatrix.cs QuanLyTro/QuanLyTro.Tests/PermissionMatrixTests.cs
git commit -m "feat(security): enforce read-only permission matrix for Police (BR-16)"
```

---

### Task 3: Cài đặt WebView2 NuGet & Cầu nối WebMessageBridge

**Files:**
- Modify: `QuanLyTro/QuanLyTro.csproj`
- Create: `QuanLyTro/Network/WebMessageBridge.cs`
- Create: `QuanLyTro.Tests/WebMessageBridgeTests.cs`

**Interfaces:**
- Consumes: `TcpClientService`, `JsonSerializer`, `WebMessageBridge.IncomingMessage`
- Produces: `WebMessageBridge.DispatchAsync(string rawJson, Func<string, Task> postBack)`

- [ ] **Step 1: Viết test cho WebMessageBridge**

Tạo `QuanLyTro/QuanLyTro.Tests/WebMessageBridgeTests.cs`:
Test việc giải mã JSON từ client JS và đóng gói trả về đúng `requestId`:

```csharp
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Network;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class WebMessageBridgeTests
{
    [TestMethod]
    public async Task ParseAndDispatch_InvalidJson_ReturnsErrorEnvelope()
    {
        string? returnedJson = null;
        var bridge = new WebMessageBridge(client: null!);

        await bridge.DispatchAsync("invalid json", msg =>
        {
            returnedJson = msg;
            return Task.CompletedTask;
        });

        Assert.IsNotNull(returnedJson);
        using var doc = JsonDocument.Parse(returnedJson);
        Assert.IsFalse(doc.RootElement.GetProperty("success").GetBoolean());
        Assert.IsTrue(doc.RootElement.GetProperty("error").GetString()!.Contains("JSON"));
    }
}
```

- [ ] **Step 2: Thêm NuGet Microsoft.Web.WebView2 vào QuanLyTro.csproj**

Thêm `PackageReference` vào `QuanLyTro/QuanLyTro.csproj` (thêm vào ItemGroup đã có, KHÔNG tạo ItemGroup trùng):
```xml
  <ItemGroup>
    <PackageReference Include="Microsoft.Web.WebView2" Version="1.0.2903.40" />
    <ProjectReference Include="QuanLyTro.Shared\QuanLyTro.Shared.csproj" />
  </ItemGroup>
```

**Đồng thời** thêm khối `Content` copy `wwwroot` và khối link sang project test (task 4a KHÔNG sửa csproj nữa — mọi thay đổi csproj nằm ở task này để tránh tranh chấp file):
```xml
  <ItemGroup>
    <Content Include="Assets\wwwroot\**">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>
```

Và trong `QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj` — `Content` không tự chảy qua `ProjectReference`, nên phải link tường minh để test đọc được file web:
```xml
  <ItemGroup>
    <Content Include="..\Assets\wwwroot\**" Link="Assets\wwwroot\%(RecursiveDir)%(Filename)%(Extension)">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>
```

- [ ] **Step 3: Triển khai WebMessageBridge.cs**

Tạo `QuanLyTro/Network/WebMessageBridge.cs`:
```csharp
using System;
using System.Text.Json;
using System.Threading.Tasks;
using QuanLyTro.Protocol;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Network;

/// <summary>
/// Cầu nối tiếp nhận thông điệp JSON từ JavaScript (WebView2) và định tuyến tới TcpClientService.
/// Định dạng JS gửi: { "requestId": "r1", "action": "AUTH_LOGIN", "data": { ... } }
/// Định dạng C# trả về: { "requestId": "r1", "success": true/false, "data": ..., "error": "..." }
/// </summary>
public sealed class WebMessageBridge
{
    private readonly TcpClientService _client;

    public WebMessageBridge(TcpClientService client)
    {
        _client = client;
    }

    public sealed record ClientEnvelope(string? RequestId, string? Action, JsonElement Data);

    public async Task DispatchAsync(string rawJson, Func<string, Task> postBack)
    {
        string? reqId = null;
        try
        {
            var env = JsonSerializer.Deserialize<ClientEnvelope>(rawJson, JsonDefaults.Options);
            if (env is null || string.IsNullOrWhiteSpace(env.Action))
            {
                await PostErrorAsync(postBack, null, "Gói tin JSON không hợp lệ hoặc thiếu Action.");
                return;
            }

            reqId = env.RequestId;
            var response = await _client.SendAsync<JsonElement, JsonElement>(env.Action, env.Data);
            
            var okPayload = JsonSerializer.Serialize(new
            {
                requestId = reqId,
                success = true,
                data = response,
                error = (string?)null
            }, JsonDefaults.Options);

            await postBack(okPayload);
        }
        catch (ClientRequestException ex)
        {
            await PostErrorAsync(postBack, reqId, ex.Message);
        }
        catch (Exception ex)
        {
            await PostErrorAsync(postBack, reqId, "Lỗi kết nối hoặc hệ thống: " + ex.Message);
        }
    }

    private static Task PostErrorAsync(Func<string, Task> postBack, string? requestId, string message)
    {
        var errPayload = JsonSerializer.Serialize(new
        {
            requestId,
            success = false,
            data = (object?)null,
            error = message
        }, JsonDefaults.Options);
        return postBack(errPayload);
    }
}
```

- [ ] **Step 4: Chạy test để xác nhận PASS**

Run: `dotnet test QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj --filter "FullyQualifiedName~WebMessageBridgeTests" -v m`
Expected: PASS 1/1 test.

- [ ] **Step 5: Commit**

```bash
git add QuanLyTro/QuanLyTro.csproj QuanLyTro/Network/WebMessageBridge.cs QuanLyTro/QuanLyTro.Tests/WebMessageBridgeTests.cs
git commit -m "feat(ui): add WebView2 dependency and WebMessageBridge"
```

---

### Task 4a: Khung Assets, CSS và cầu nối JS (login + shell)

**Files:**
- Create: `QuanLyTro/Assets/wwwroot/index.html`
- Create: `QuanLyTro/Assets/wwwroot/css/style.css`
- Create: `QuanLyTro/Assets/wwwroot/js/bridge.js`
- Create: `QuanLyTro/Assets/wwwroot/js/login.js`
- Modify: `QuanLyTro/QuanLyTro.csproj` (CopyToOutputDirectory cho wwwroot)
- Test: `QuanLyTro/QuanLyTro.Tests/AssetPackagingTests.cs`

**Interfaces:**
- Consumes: `docs/superpowers/mockups/giaodien.html`
- Produces: `window.bridge.call(action, data) -> Promise`, `window.bridge.onMessage(raw)`, `enterWorkspace(user)`, `showShell(roleKey)`

- [ ] **Step 1: Viết test kiểm tra file asset được đóng gói**

Tạo `QuanLyTro/QuanLyTro.Tests/AssetPackagingTests.cs`:
```csharp
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class AssetPackagingTests
{
    private static string Wwwroot =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "wwwroot");

    [TestMethod]
    public void Wwwroot_ContainsEntryPointAndBridge()
    {
        Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "index.html")), "Thiếu index.html");
        Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "js", "bridge.js")), "Thiếu bridge.js");
        Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "css", "style.css")), "Thiếu style.css");
    }
}
```

- [ ] **Step 2: Chạy test để xác nhận FAIL**

Run: `dotnet test QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj --filter "FullyQualifiedName~AssetPackagingTests" -v m`
Expected: FAIL — `Thiếu index.html`.

- [ ] **Step 3: Tách CSS từ giaodien.html thành style.css**

Tạo `QuanLyTro/Assets/wwwroot/css/style.css`, chuyển nguyên thẻ `<style>` của `docs/superpowers/mockups/giaodien.html` sang. Giữ nguyên toàn bộ biến màu `:root`, `.titlebar`, `.userbar`, `.headline`, `.tabs`, `.tbl`, `.kpi`, `.tag`, `.badge-module`, `.statusstrip`.

- [ ] **Step 4: Viết bridge.js — chỉ thư viện cầu nối, không logic màn hình**

Tạo `QuanLyTro/Assets/wwwroot/js/bridge.js`:
```javascript
// Cầu nối WebMessage: JS gọi C#, C# trả về theo requestId.
window.bridge = {
  _handlers: {},
  _seq: 0,

  call: function (action, data) {
    return new Promise((resolve, reject) => {
      const requestId = 'req_' + (++this._seq);
      this._handlers[requestId] = { resolve, reject };
      window.chrome.webview.postMessage(JSON.stringify({
        requestId: requestId,
        action: action,
        data: data || {}
      }));
    });
  },

  onMessage: function (raw) {
    let msg;
    try { msg = typeof raw === 'string' ? JSON.parse(raw) : raw; } catch (e) { return; }
    if (!msg || !msg.requestId) return;
    const h = this._handlers[msg.requestId];
    if (!h) return;
    delete this._handlers[msg.requestId];
    if (msg.success) h.resolve(msg.data);
    else h.reject(new Error(msg.error || 'Thao tác thất bại'));
  }
};

window.chrome.webview.addEventListener('message', ev => window.bridge.onMessage(ev.data));

// Chuyển shell theo vai. roleKey: 'landlord' | 'police' | 'tenant'
function showShell(roleKey) {
  ['login', 'landlord', 'police', 'tenant'].forEach(id => {
    const el = document.getElementById(id);
    if (el) el.hidden = (id !== roleKey);
  });
}
```

- [ ] **Step 5: Viết login.js — đăng nhập và định tuyến theo vai**

Tạo `QuanLyTro/Assets/wwwroot/js/login.js`:
```javascript
// Ánh xạ Role số từ server (Landlord=0, Tenant=1, Police=2) sang shell.
const ROLE_TO_SHELL = { 0: 'landlord', 1: 'tenant', 2: 'police' };

async function doLogin() {
  const username = document.getElementById('lu').value.trim();
  const password = document.getElementById('lp').value;
  if (!username || !password) {
    alert('Vui lòng nhập tài khoản và mật khẩu.');
    return;
  }
  try {
    const user = await window.bridge.call('AUTH_LOGIN', { Username: username, Password: password });
    enterWorkspace(user);
  } catch (err) {
    alert(err.message);
  }
}

function enterWorkspace(user) {
  const shell = ROLE_TO_SHELL[user.Role] || 'landlord';
  showShell(shell);
  document.getElementById('uname').textContent = user.FullName;
  document.getElementById('pname').textContent = user.FullName;
  if (shell === 'landlord') loadLandlordTab('dash');
  if (shell === 'police') loadPoliceTab('citizens');
  if (shell === 'tenant') loadTenantInvoices();
}
```

- [ ] **Step 6: Tạo index.html nạp đủ script theo thứ tự phụ thuộc**

Tạo `QuanLyTro/Assets/wwwroot/index.html` từ 4 cửa sổ của `giaodien.html`, với:
```html
<link rel="stylesheet" href="css/style.css">
...
<script src="js/bridge.js"></script>
<script src="js/login.js"></script>
<script src="js/landlord.js"></script>
<script src="js/police.js"></script>
<script src="js/tenant.js"></script>
```
Ở task này chỉ cần 4 cửa sổ rỗng có `id` đúng: `login`, `landlord`, `police`, `tenant`; các file `landlord.js`/`police.js`/`tenant.js` tạo stub rỗng `// TODO: Task 4b/4c/4d` để trang không lỗi 404.

- [ ] **Step 7: (đã chuyển sang Task 3)** — cấu hình `Content`/`Link` cho `wwwroot` nằm ở Task 3 Step 2 để mọi thay đổi `.csproj` tập trung một chỗ, tránh hai task cùng sửa một file.

- [ ] **Step 8: Chạy test để xác nhận PASS**

Run: `dotnet test QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj --filter "FullyQualifiedName~AssetPackagingTests" -v m`
Expected: PASS 1/1 test.

- [ ] **Step 9: Commit**

```bash
git add QuanLyTro/Assets/ QuanLyTro/QuanLyTro.Tests/AssetPackagingTests.cs
git commit -m "feat(ui): add WebView2 asset skeleton, CSS and JS bridge"
```

---

### Task 4b: Shell Chủ trọ — 7 tab HTML + render từ Server

**Files:**
- Create: `QuanLyTro/Assets/wwwroot/js/landlord.js`
- Modify: `QuanLyTro/Assets/wwwroot/index.html` (điền nội dung 7 tab)
- Test: `QuanLyTro/QuanLyTro.Tests/LandlordTabContractTests.cs`

**Interfaces:**
- Consumes: `window.bridge.call`, `showShell`
- Produces: `loadLandlordTab(tabKey)`, `renderRooms(rows)`, `renderTenants(rows)`, `renderInvoices(rows)`

- [ ] **Step 1: Viết test kiểm tra tab key khớp giữa HTML và JS**

Tạo `QuanLyTro/QuanLyTro.Tests/LandlordTabContractTests.cs`:
```csharp
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class LandlordTabContractTests
{
    private static string Wwwroot =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "wwwroot");

    [TestMethod]
    public void LandlordHtml_HasSevenTabContainers_AndJsReferencesEachTabKey()
    {
        var html = File.ReadAllText(Path.Combine(Wwwroot, "index.html"));
        var js = File.ReadAllText(Path.Combine(Wwwroot, "js", "landlord.js"));

        string[] tabKeys = { "dash", "rooms", "tenants", "contracts", "utils", "invoices", "reports" };
        foreach (var key in tabKeys)
        {
            Assert.IsTrue(html.Contains($"id=\"tab-{key}\""), $"index.html thiếu tab-{key}");
            Assert.IsTrue(js.Contains($"'{key}'"), $"landlord.js chưa xử lý tab {key}");
        }
    }
}
```

- [ ] **Step 2: Chạy test để xác nhận FAIL**

Run: `dotnet test QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj --filter "FullyQualifiedName~LandlordTabContractTests" -v m`
Expected: FAIL — `index.html thiếu tab-dash`.

- [ ] **Step 3: Điền 7 tab vào index.html**

Chuyển nội dung 7 khối `#tab-dash` … `#tab-reports` từ `giaodien.html` vào cửa sổ `#landlord` của `index.html`, giữ nguyên id.

- [ ] **Step 4: Viết landlord.js render từ dữ liệu Server**

Tạo `QuanLyTro/Assets/wwwroot/js/landlord.js`:
```javascript
const LANDLORD_TITLES = {
  dash: 'Tổng quan vận hành cơ sở',
  rooms: 'Danh sách phòng trọ',
  tenants: 'Hồ sơ khách thuê',
  contracts: 'Hợp đồng & khách thuê',
  utils: 'Chốt chỉ số điện nước',
  invoices: 'Hóa đơn & thu tiền',
  reports: 'Thống kê doanh thu'
};

async function loadLandlordTab(tabKey) {
  document.getElementById('modtitle').textContent = LANDLORD_TITLES[tabKey] || '';
  Object.keys(LANDLORD_TITLES).forEach(k => {
    const el = document.getElementById('tab-' + k);
    if (el) el.hidden = (k !== tabKey);
  });
  document.querySelectorAll('#landlord .tabs .tab').forEach(t => {
    t.classList.toggle('on', t.dataset.tab === tabKey);
  });

  try {
    if (tabKey === 'rooms') renderRooms(await window.bridge.call('ROOM_GET_ALL', {}));
    if (tabKey === 'invoices') renderInvoices(await window.bridge.call('INVOICE_GET_ALL', {}));
    if (tabKey === 'reports') await loadReports();
  } catch (err) {
    alert(err.message);
  }
}

function renderRooms(rows) {
  const body = document.querySelector('#tab-rooms tbody');
  if (!body) return;
  body.innerHTML = (rows || []).map(r => `
    <tr>
      <td><b>${r.roomNumber}</b></td>
      <td class="num amount">${(r.price || 0).toLocaleString('vi-VN')}</td>
      <td class="num">${r.maxOccupants}</td>
      <td class="num">${r.currentOccupants ?? 0}</td>
      <td><span class="tag ${r.status === 'Rented' ? 'rent' : 'avail'}">${r.status}</span></td>
    </tr>`).join('');
}

function renderInvoices(rows) {
  const body = document.querySelector('#tab-invoices tbody');
  if (!body) return;
  body.innerHTML = (rows || []).map(i => `
    <tr>
      <td><b>${i.roomNumber}</b></td>
      <td class="mono">${i.billingMonth}</td>
      <td class="num amount">${(i.totalAmount || 0).toLocaleString('vi-VN')}</td>
      <td><span class="tag ${i.isPaid ? 'paid' : 'unpaid'}">${i.isPaid ? 'Đã thanh toán' : 'Chưa thanh toán'}</span></td>
    </tr>`).join('');
}

async function loadReports() {
  const summary = await window.bridge.call('REPORT_SUMMARY', {});
  // Đổ số liệu KPI vào #tab-reports; trường hợp null thì để trống
  document.querySelectorAll('#tab-reports [data-kpi]').forEach(el => {
    const key = el.dataset.kpi;
    if (summary && summary[key] != null) el.textContent = summary[key].toLocaleString('vi-VN');
  });
}
```

- [ ] **Step 5: Chạy test để xác nhận PASS**

Run: `dotnet test QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj --filter "FullyQualifiedName~LandlordTabContractTests" -v m`
Expected: PASS 1/1 test.

- [ ] **Step 6: Commit**

```bash
git add QuanLyTro/Assets/wwwroot/ QuanLyTro/QuanLyTro.Tests/LandlordTabContractTests.cs
git commit -m "feat(ui): landlord 7-tab shell rendered from server data"
```

---

### Task 4c: Shell Công an phường — 3 tab chỉ đọc + xuất file

**Files:**
- Create: `QuanLyTro/Assets/wwwroot/js/police.js`
- Modify: `QuanLyTro/Assets/wwwroot/index.html` (điền 3 tab Police)
- Test: `QuanLyTro/QuanLyTro.Tests/PoliceShellContractTests.cs`

**Interfaces:**
- Consumes: `window.bridge.call`
- Produces: `loadPoliceTab(tabKey)`, `renderCitizens(rows)`, `exportHistory()`

- [ ] **Step 1: Viết test kiểm tra shell Police không có nút ghi**

Tạo `QuanLyTro/QuanLyTro.Tests/PoliceShellContractTests.cs`:
```csharp
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class PoliceShellContractTests
{
    private static string Wwwroot =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "wwwroot");

    [TestMethod]
    public void PoliceShell_HasThreeTabs_AndNoWriteButtons()
    {
        var html = File.ReadAllText(Path.Combine(Wwwroot, "index.html"));
        var start = html.IndexOf("id=\"police\"", System.StringComparison.Ordinal);
        Assert.IsTrue(start >= 0, "index.html thiếu cửa sổ #police");

        var policeBlock = html.Substring(start, html.Length - start);
        string[] tabKeys = { "citizens", "residence", "history" };
        foreach (var key in tabKeys)
        {
            Assert.IsTrue(policeBlock.Contains($"id=\"ptab-{key}\""), $"Thiếu tab công an: ptab-{key}");
        }

        // BR-16: không được có nút ghi trong shell công an
        foreach (var verb in new[] { "Thêm", "Sửa", "Xóa", "Lập hợp đồng", "Thu tiền" })
        {
            Assert.IsFalse(Regex.IsMatch(policeBlock, $">{verb}<"), $"Shell công an có nút ghi: {verb}");
        }
    }
}
```

- [ ] **Step 2: Chạy test để xác nhận FAIL**

Run: `dotnet test QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj --filter "FullyQualifiedName~PoliceShellContractTests" -v m`
Expected: FAIL — `index.html thiếu cửa sổ #police`.

- [ ] **Step 3: Điền 3 tab Police vào index.html**

Chuyển 3 khối `#ptab-citizens`, `#ptab-residence`, `#ptab-history` từ `giaodien.html` vào cửa sổ `#police`, giữ nguyên id và các nhãn `ReadOnly`.

- [ ] **Step 4: Viết police.js**

Tạo `QuanLyTro/Assets/wwwroot/js/police.js`:
```javascript
const POLICE_TITLES = {
  citizens: 'Tra cứu công dân',
  residence: 'Báo cáo lưu trú & tạm trú',
  history: 'Lịch sử biến động lưu trú'
};

async function loadPoliceTab(tabKey) {
  document.getElementById('ptitle').textContent = POLICE_TITLES[tabKey] || '';
  Object.keys(POLICE_TITLES).forEach(k => {
    const el = document.getElementById('ptab-' + k);
    if (el) el.hidden = (k !== tabKey);
  });
  document.querySelectorAll('#police .tabs .tab').forEach(t => {
    t.classList.toggle('on', t.dataset.ptab === tabKey);
  });

  try {
    if (tabKey === 'citizens' || tabKey === 'residence') {
      // Tải phòng, sau đó gom người thuê theo từng phòng
      const rooms = await window.bridge.call('ROOM_GET_ALL', {}) || [];
      const all = [];
      for (const r of rooms) {
        const tenants = await window.bridge.call('TENANT_GET_BY_ROOM', { roomId: r.id ?? r.Id }) || [];
        for (const t of tenants) {
          all.push({ ...t, roomNumber: r.roomNumber ?? r.RoomNumber });
        }
      }
      const q = (document.getElementById('p-search')?.value || '').toLowerCase().trim();
      const filtered = q ? all.filter(t =>
        (t.fullName || '').toLowerCase().includes(q) ||
        (t.idCard || '').includes(q) ||
        (t.phone || '').includes(q)) : all;
      renderCitizens(filtered);
    }
    if (tabKey === 'history') await loadHistory();
  } catch (err) {
    alert(err.message);
  }
}

function renderCitizens(rows) {
  const body = document.querySelector('#ptab-citizens tbody');
  if (!body) return;
  body.innerHTML = (rows || []).map(t => `
    <tr>
      <td><b>${t.fullName}</b></td>
      <td class="mono">${t.idCard}</td>
      <td class="mono">${t.phone}</td>
      <td>${t.roomNumber}</td>
      <td><span class="tag ${t.isTemporaryRegistered ? 'done' : 'warn'}">
        ${t.isTemporaryRegistered ? 'Đã đăng ký' : 'Chưa đăng ký'}</span></td>
    </tr>`).join('');
}

async function loadHistory() {
  const from = document.getElementById('p-from')?.value;
  const to = document.getElementById('p-to')?.value;
  const rows = await window.bridge.call('RESIDENCE_HISTORY_GET', { FromDate: from, ToDate: to });
  const body = document.querySelector('#ptab-history tbody');
  if (!body) return;
  body.innerHTML = (rows || []).map(h => `
    <tr>
      <td><b>${h.fullName}</b></td>
      <td><b>${h.roomNumber}</b></td>
      <td>${h.eventType}</td>
      <td class="mono">${h.eventDate}</td>
      <td>${h.notes || '—'}</td>
    </tr>`).join('');
}

async function exportHistory() {
  const payload = {
    FromDate: document.getElementById('p-from').value,
    ToDate: document.getElementById('p-to').value,
    Format: 'CSV',
    RoomNumber: null,
    EventType: null
  };
  try {
    const res = await window.bridge.call('EXPORT_RESIDENCE_HISTORY', payload);
    alert(`Đã xuất ${res.RowCount} bản ghi tới:\n${res.FilePath}`);
  } catch (err) {
    alert(err.message);
  }
}
```

- [ ] **Step 5: Chạy test để xác nhận PASS**

Run: `dotnet test QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj --filter "FullyQualifiedName~PoliceShellContractTests" -v m`
Expected: PASS 1/1 test.

- [ ] **Step 6: Commit**

```bash
git add QuanLyTro/Assets/wwwroot/ QuanLyTro/QuanLyTro.Tests/PoliceShellContractTests.cs
git commit -m "feat(ui): police read-only shell with 3 tabs and export"
```

---

### Task 4d: Shell Khách thuê — 1 tab hóa đơn của tôi

**Files:**
- Create: `QuanLyTro/Assets/wwwroot/js/tenant.js`
- Modify: `QuanLyTro/Assets/wwwroot/index.html` (điền tab Tenant)
- Test: `QuanLyTro/QuanLyTro.Tests/TenantShellContractTests.cs`

**Interfaces:**
- Consumes: `window.bridge.call`
- Produces: `loadTenantInvoices()`, `renderTenantInvoices(rows)`

- [ ] **Step 1: Viết test kiểm tra shell Tenant chỉ đọc và có đúng 1 tab**

Tạo `QuanLyTro/QuanLyTro.Tests/TenantShellContractTests.cs`:
```csharp
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class TenantShellContractTests
{
    private static string Wwwroot =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "wwwroot");

    [TestMethod]
    public void TenantShell_HasSingleTab_AndNoWriteButtons()
    {
        var html = File.ReadAllText(Path.Combine(Wwwroot, "index.html"));
        var start = html.IndexOf("id=\"tenant\"", System.StringComparison.Ordinal);
        Assert.IsTrue(start >= 0, "index.html thiếu cửa sổ #tenant");

        var tenantBlock = html.Substring(start, html.Length - start);
        Assert.AreEqual(1, Regex.Matches(tenantBlock, "class=\"tab on\"").Count,
            "Shell khách thuê phải có đúng 1 tab");

        foreach (var verb in new[] { "Thêm", "Sửa", "Xóa" })
        {
            Assert.IsFalse(Regex.IsMatch(tenantBlock, $">{verb}<"), $"Shell khách thuê có nút ghi: {verb}");
        }
    }
}
```

- [ ] **Step 2: Chạy test để xác nhận FAIL**

Run: `dotnet test QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj --filter "FullyQualifiedName~TenantShellContractTests" -v m`
Expected: FAIL — `index.html thiếu cửa sổ #tenant`.

- [ ] **Step 3: Điền tab Tenant vào index.html**

Chuyển nội dung cửa sổ `#tenant` từ `giaodien.html` sang, giữ nguyên khối hồ sơ, banner quá hạn, bảng quyết toán, bảng lịch sử thanh toán.

- [ ] **Step 4: Viết tenant.js**

Tạo `QuanLyTro/Assets/wwwroot/js/tenant.js`:
```javascript
async function loadTenantInvoices() {
  try {
    const rows = await window.bridge.call('INVOICE_GET_MINE', {});
    renderTenantInvoices(rows);
  } catch (err) {
    alert(err.message);
  }
}

function renderTenantInvoices(rows) {
  const body = document.querySelector('#tenant .tbl tbody');
  if (!body) return;
  body.innerHTML = (rows || []).map(i => `
    <tr>
      <td class="mono">${i.billingMonth}</td>
      <td class="num amount">${(i.totalAmount || 0).toLocaleString('vi-VN')}</td>
      <td class="mono">${i.paidAt || '—'}</td>
      <td class="num"><span class="tag ${i.isPaid ? 'paid' : 'unpaid'}">
        ${i.isPaid ? 'Đã thanh toán' : 'Chưa thanh toán'}</span></td>
    </tr>`).join('');
}
```

- [ ] **Step 5: Chạy test để xác nhận PASS**

Run: `dotnet test QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj --filter "FullyQualifiedName~TenantShellContractTests" -v m`
Expected: PASS 1/1 test.

- [ ] **Step 6: Commit**

```bash
git add QuanLyTro/Assets/wwwroot/ QuanLyTro/QuanLyTro.Tests/TenantShellContractTests.cs
git commit -m "feat(ui): tenant read-only shell rendered from server data"
```

---

### Task 5: Refactor Form1 sang WebView2 Shell

**Files:**
- Modify: `QuanLyTro/Form1.cs`
- Modify: `QuanLyTro/Form1.Designer.cs`
- Clean: Đánh dấu obsolete / loại bỏ các UserControl WinForms cũ (`QuanLyTro/Forms/*Form.cs`)

**Interfaces:**
- Consumes: `Microsoft.Web.WebView2.WinForms.WebView2`, `WebMessageBridge`, `TcpClientService`
- Produces: Form1 nạp `file:///.../Assets/wwwroot/index.html`, lắng nghe `WebMessageReceived`

- [ ] **Step 1: Cập nhật Form1.Designer.cs**

Dọn dẹp các control MenuStrip, TabControl, UserControls cũ trong `Form1.Designer.cs`. Thêm control duy nhất:
```csharp
this.webView = new Microsoft.Web.WebView2.WinForms.WebView2();
((System.ComponentModel.ISupportInitialize)(this.webView)).BeginInit();
this.SuspendLayout();

this.webView.Dock = System.Windows.Forms.DockStyle.Fill;
this.webView.Location = new System.Drawing.Point(0, 0);
this.webView.Name = "webView";
this.webView.Size = new System.Drawing.Size(1024, 768);
this.webView.TabIndex = 0;

this.ClientSize = new System.Drawing.Size(1024, 768);
this.Controls.Add(this.webView);
this.Name = "Form1";
this.Text = "Quản Lý Nhà Trọ .NET 8";
((System.ComponentModel.ISupportInitialize)(this.webView)).EndInit();
this.ResumeLayout(false);
```

- [ ] **Step 2: Cập nhật Form1.cs khởi tạo WebView2 và gắn Bridge**

Trong `QuanLyTro/Form1.cs`:
```csharp
using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using QuanLyTro.Network;

namespace QuanLyTro;

public partial class Form1 : Form
{
    public static TcpClientService Client { get; } = new();
    private readonly WebMessageBridge _bridge;

    public Form1()
    {
        InitializeComponent();
        _bridge = new WebMessageBridge(Client);
        Load += Form1_Load;
        Client.Disconnected += OnClientDisconnected;
    }

    private async void Form1_Load(object? sender, EventArgs e)
    {
        var (host, port) = ReadServerEndpoint();
        try
        {
            if (!Client.IsConnected)
            {
                await Client.ConnectAsync(host, port);
            }
        }
        catch
        {
            // Cho phép mở UI, JS sẽ báo khi bấm login
        }

        await InitializeWebViewAsync();
    }

    private async Task InitializeWebViewAsync()
    {
        await webView.EnsureCoreWebView2Async();
        webView.CoreWebView2.WebMessageReceived += async (_, args) =>
        {
            var rawJson = args.TryGetWebMessageAsString();
            if (!string.IsNullOrEmpty(rawJson))
            {
                await _bridge.DispatchAsync(rawJson, async msg =>
                {
                    await webView.InvokeAsync(() => webView.CoreWebView2.PostWebMessageAsString(msg));
                });
            }
        };

        var htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "wwwroot", "index.html");
        webView.CoreWebView2.Navigate(htmlPath);
    }

    private void OnClientDisconnected(object? sender, EventArgs e)
    {
        if (IsDisposed) return;
        BeginInvoke(() =>
        {
            webView.CoreWebView2?.ExecuteScriptAsync("alert('Mất kết nối máy chủ TCP. Vui lòng thử lại.');");
        });
    }

    private static (string Host, int Port) ReadServerEndpoint()
    {
        var host = ConfigurationManager.AppSettings["ServerHost"] ?? "127.0.0.1";
        var port = int.TryParse(ConfigurationManager.AppSettings["ServerPort"], out var p) ? p : 8888;
        return (host, port);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        Client.Disconnected -= OnClientDisconnected;
        Client.Dispose();
        base.OnFormClosed(e);
    }
}
```

- [ ] **Step 3: Biên dịch dự án Client**

Run: `dotnet build QuanLyTro/QuanLyTro.csproj`
Expected: Build thành công (không có lỗi tham chiếu WinForms UserControls).

- [ ] **Step 4: Commit**

```bash
git add QuanLyTro/Form1.cs QuanLyTro/Form1.Designer.cs
git commit -m "refactor(ui): convert Form1 into clean WebView2 container shell"
```

---

### Task 6: Server Backend Service & Repository cho Lưu trú Công an (PM-04, PM-05)

**Files:**
- Create: `QuanLyTro/QuanLyTro.Server/Repositories/ResidenceRepository.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Services/ResidenceService.cs`
- Modify: `QuanLyTro/QuanLyTro.Server/Network/RequestRouter.cs`
- Test: `QuanLyTro.Tests/ResidenceServiceTests.cs`

**Interfaces:**
- Consumes: `Database.OpenAsync`, `ResidenceHistoryDto`, `ExportHistoryRequest`
- Produces: `ResidenceService.GetHistoryAsync()`, `ResidenceService.ExportHistoryAsync()`

- [ ] **Step 1: Viết unit test cho ResidenceService**

Tạo `QuanLyTro/QuanLyTro.Tests/ResidenceServiceTests.cs`:
Kiểm tra validation logic: ngày kết thúc phải $\ge$ ngày bắt đầu.

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Server.Services;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class ResidenceServiceTests
{
    [TestMethod]
    [ExpectedException(typeof(BusinessRuleException))]
    public async Task ExportHistory_EndDateBeforeStartDate_ThrowsException()
    {
        var service = new ResidenceService(repo: null!);
        var req = new ExportHistoryRequest(
            FromDate: new DateTime(2026, 10, 1),
            ToDate: new DateTime(2026, 9, 1),
            Format: "CSV",
            RoomNumber: null,
            EventType: null);

        await service.ExportHistoryAsync(req, CancellationToken.None);
    }
}
```

- [ ] **Step 2: Chạy test để xác nhận FAIL**

Run: `dotnet test QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj --filter "FullyQualifiedName~ResidenceServiceTests" -v m`
Expected: FAIL vì chưa có `ResidenceService`.

- [ ] **Step 3: Triển khai ResidenceRepository và ResidenceService**

Tạo `QuanLyTro/QuanLyTro.Server/Services/ResidenceService.cs`:
```csharp
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QuanLyTro.Server.Repositories;
using QuanLyTro.Shared.Models;

namespace QuanLyTro.Server.Services;

public sealed class ResidenceService
{
    private readonly ResidenceRepository _repo;

    public ResidenceService(ResidenceRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<ResidenceHistoryDto>> GetHistoryAsync(DateTime from, DateTime to, string? room, CancellationToken ct)
    {
        if (to < from) throw new BusinessRuleException("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");
        return await _repo.GetHistoryAsync(from, to, room, ct);
    }

    public async Task<ExportResult> ExportHistoryAsync(ExportHistoryRequest req, CancellationToken ct)
    {
        if (req.ToDate < req.FromDate) throw new BusinessRuleException("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");
        var data = await _repo.GetHistoryAsync(req.FromDate, req.ToDate, req.RoomNumber, ct);
        // Ghi file CSV tạm vào thư mục xuất
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"LuuTru_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        // ponytail: xuất CSV đơn giản cho môn học, không cần thư viện ngoài
        using var sw = new System.IO.StreamWriter(path, false, System.Text.Encoding.UTF8);
        await sw.WriteLineAsync("Họ tên,CCCD,Số phòng,Sự kiện,Ngày,Ghi chú");
        foreach (var r in data)
        {
            await sw.WriteLineAsync($"{r.FullName},{r.IdCard},{r.RoomNumber},{r.EventType},{r.EventDate:dd/MM/yyyy},{r.Notes}");
        }
        return new ExportResult(path, data.Count);
    }
}
```

Tạo stub `ResidenceRepository.cs` truy vấn cơ sở dữ liệu.
Đăng ký handler vào `QuanLyTro/QuanLyTro.Server/Network/RequestRouter.cs`:
```csharp
[ActionNames.ResidenceHistoryGet] = (r, _, ct) => Ok(_residence.GetHistoryAsync(...)),
[ActionNames.ExportResidenceHistory] = (r, _, ct) => Ok(_residence.ExportHistoryAsync(...)),
```

- [ ] **Step 4: Chạy test để xác nhận PASS**

Run: `dotnet test QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj --filter "FullyQualifiedName~ResidenceServiceTests" -v m`
Expected: PASS 1/1 test.

- [ ] **Step 5: Commit**

```bash
git add QuanLyTro/QuanLyTro.Server/ QuanLyTro/QuanLyTro.Tests/ResidenceServiceTests.cs
git commit -m "feat(server): implement residence service and export for Police (PM-05)"
```

---

### Task 7: Tích hợp E2E Round-trip & Kiểm tra toàn diện 3 vai trò

**Files:**
- Create: `QuanLyTro/QuanLyTro.Tests/PoliceIntegrationTests.cs`
- Modify: `docs/superpowers/plans/2026-09-30-webview2-police-refactor.md` (đánh dấu hoàn tất)

**Interfaces:**
- Consumes: `TcpClientService`, `RequestRouter`, `UserRole.Police`
- Produces: Kết quả kiểm thử E2E: Landlord CRUD, Police ReadOnly, Tenant Hóa đơn

- [x] **Step 1: Viết test kịch bản E2E cho Police trong PoliceIntegrationTests.cs**

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuanLyTro.Server.Network;
using QuanLyTro.Shared.Models;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class PoliceIntegrationTests
{
    [TestMethod]
    public void VerifyPoliceEnforcement_CannotWrite_CanRead()
    {
        // Kiểm tra phân quyền tĩnh bảo vệ toàn bộ nghiệp vụ ghi
        Assert.IsTrue(PermissionMatrix.IsAllowed(ActionNames.ResidenceHistoryGet, UserRole.Police));
        Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.RoomDelete, UserRole.Police));
        Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.TenantCheckout, UserRole.Police));
    }
}
```

- [x] **Step 2: Chạy toàn bộ test suites độc lập**

Run: `dotnet test QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj --filter "FullyQualifiedName~ProtocolTests|FullyQualifiedName~PermissionMatrixTests|FullyQualifiedName~WebMessageBridgeTests|FullyQualifiedName~ResidenceServiceTests|FullyQualifiedName~PoliceIntegrationTests"`
Expected: PASS toàn bộ các test không phụ thuộc MySQL ngoài.

- [x] **Step 3: Biên dịch toàn bộ Solution**

Run: `dotnet build QuanLyTro/QuanLyTro.csproj`
Expected: Build thành công 0 warning, 0 error.

- [x] **Step 4: Commit**

```bash
git add QuanLyTro/QuanLyTro.Tests/PoliceIntegrationTests.cs
git commit -m "test(e2e): add police permission verification test"
```

---

## Self-Review Check

1. **Spec coverage:** Kế hoạch bao phủ đủ:
   - WebView2 Shell & WebMessage Bridge (Spec §3).
   - Tác nhân Police 3 tab & chặn ghi BR-16 (Spec §2).
   - 2 action mới: `RESIDENCE_HISTORY_GET`, `EXPORT_RESIDENCE_HISTORY` (Spec §2.2).
   - Tách CSS/JS đóng gói Assets wwwroot (Spec §3).
2. **Placeholder scan:** Không có "TBD", "TODO", code block đầy đủ cho từng bước.
3. **Type consistency:** Các tên DTO (`ResidenceHistoryDto`, `ExportHistoryRequest`, `ExportResult`) và enum (`UserRole.Police`) đồng nhất từ Task 1 đến Task 6.
4. **Review Focus:** 5 điểm nguy cơ (RF-1 đến RF-5) đều có test case tương ứng ở Task 1, 2, 3, 6, 7.
