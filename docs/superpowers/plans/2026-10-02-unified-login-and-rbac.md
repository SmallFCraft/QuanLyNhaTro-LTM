# Kế hoạch triển khai: Form Đăng nhập Thống nhất & Phân quyền Động 4 Tác nhân (RBAC)

> **Dành cho agent thực thi:** BẮT BUỘC DÙNG KỸ NĂNG: Dùng `superpowers:subagent-driven-development` hoặc `superpowers:executing-plans` để thực thi từng task. Mỗi task theo cú pháp checkbox (`- [ ]`).

**Mục tiêu:** 
1. Thay thế giao diện đăng nhập hiện tại bằng 1 form duy nhất (chỉ gồm Tên đăng nhập & Mật khẩu), tự động phát hiện vai trò người dùng để chuyển hướng.
2. Thiết lập 4 vai trò rõ ràng: Chủ trọ (`Landlord`), Quản lý (`Manager`), Công an (`Police`), Người thuê (`Tenant`).
3. Triển khai cơ chế phân quyền động (Role-Based Access Control - RBAC) lưu tại MySQL, cache trên RAM TCP Server, cho phép Chủ trọ cấp/thu hồi quyền của Quản lý và các vai trò khác thông qua giao diện trực quan.

**Kiến trúc:** 
- Shared: Mở rộng `UserRole` enum có `Manager = 3`. Thêm action TCP `PERMISSION_GET_MATRIX` và `PERMISSION_UPDATE_ROLE`.
- Database: Bổ sung bảng `role_permissions` và cập nhật ENUM `users.role`.
- Server: Nâng cấp `PermissionMatrix` thành Hybrid In-Memory Cache (Landlord bypass cứng trong code; Manager/Police/Tenant tra cứu từ CSDL và cache đồng bộ). `RequestRouter` định tuyến action phân quyền.
- Client: Tinh giản `auth/index.html` thành 1 form. Mở rộng `landlord/index.html` thành workspace thích ứng cho cả Chủ trọ (kèm tab Phân quyền) và Quản lý (tự động ẩn các tính năng bị thu hồi quyền).

**Tech Stack:** .NET 8.0, C#, WinForms WebView2, Vanilla JS, MySQL 8.0, MSTest.

**Spec:** `docs/superpowers/specs/2026-10-02-unified-login-and-rbac-design.md`

## Ràng buộc chung (Global Constraints)
- Tương thích ngược: Giữ vững toàn bộ 193 unit/integration test hiện có không bị regression.
- Hiệu năng phân quyền: Việc kiểm tra quyền trên mỗi packet TCP phải tra cứu qua cache `O(1)` (HashSet in-memory), tuyệt đối không truy vấn DB trên mỗi request.
- An toàn hệ thống: Chủ trọ (`Landlord`) luôn có toàn quyền quản trị trong code, không thể bị tước quyền bởi bất kỳ cấu hình CSDL nào.
- Trải nghiệm đăng nhập: Một form duy nhất, không có radio hay dropdown chọn vai trò.

## Trọng tâm kiểm thử & rà soát (Review Focus)
1. Thử đăng nhập bằng tài khoản không tồn tại hoặc sai pass -> Vẫn trả về cùng thông báo lỗi bảo mật, không lộ role hay bảng dữ liệu.
2. Manager gọi action không được cấp quyền -> Server trả về `ResponsePacket.Fail("Không có quyền.")`.
3. Chủ trọ cập nhật quyền của Manager runtime -> Cache trên server nạp lại ngay lập tức, request kế tiếp của Manager chịu tác động ngay không cần khởi động lại server.
4. Tài khoản `users` cũ trong DB chưa có quyền trong `role_permissions` -> Server tự động nạp quyền mặc định (fallback an toàn).
5. Manager cố tình gọi `PERMISSION_UPDATE_ROLE` -> Bị Server chặn ngay lập tức vì không phải Landlord.

---

### Task 1: Mở rộng Shared Protocol & Enums

**Files:**
- Modify: `QuanLyTro/QuanLyTro.Shared/Models/AuthAndReportDtos.cs`
- Modify: `QuanLyTro/QuanLyTro.Shared/Protocol/ActionNames.cs`
- Test: `QuanLyTro/QuanLyTro.Tests/ProtocolTests.cs`

**Interfaces:**
- Produces: `UserRole.Manager = 3`, `RolePermissionItemDto`, `RolePermissionsMatrixDto`, `UpdateRolePermissionsRequest`, `ActionNames.PermissionGetMatrix`, `ActionNames.PermissionUpdateRole`.

- [ ] **Step 1: Viết test cho ActionNames và DTOs mới**

Thêm test case vào `QuanLyTro/QuanLyTro.Tests/ProtocolTests.cs`:
```csharp
[TestMethod]
public void ActionNames_ContainsNewPermissionActions()
{
    CollectionAssert.Contains((System.Collections.ICollection)ActionNames.All, ActionNames.PermissionGetMatrix);
    CollectionAssert.Contains((System.Collections.ICollection)ActionNames.All, ActionNames.PermissionUpdateRole);
}

[TestMethod]
public void UserRole_ContainsManager()
{
    Assert.AreEqual(3, (int)UserRole.Manager);
}
```

- [ ] **Step 2: Chạy test để xác nhận test FAIL**

Run: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter "ActionNames_ContainsNewPermissionActions|UserRole_ContainsManager"`
Expected: FAIL do chưa khai báo `PermissionGetMatrix`, `PermissionUpdateRole`, `UserRole.Manager`.

- [ ] **Step 3: Triển khai code trong Shared**

Trong `QuanLyTro/QuanLyTro.Shared/Models/AuthAndReportDtos.cs`:
```csharp
public enum UserRole
{
    Landlord = 0,
    Tenant = 1,
    Police = 2,
    Manager = 3,
}

public sealed record RolePermissionItemDto(string Action, string Description, string Group);
public sealed record RolePermissionsMatrixDto(
    Dictionary<string, List<string>> RoleActions,
    List<RolePermissionItemDto> AvailableActions
);
public sealed record UpdateRolePermissionsRequest(string Role, List<string> Actions);
```

Trong `QuanLyTro/QuanLyTro.Shared/Protocol/ActionNames.cs`:
```csharp
public const string PermissionGetMatrix = "PERMISSION_GET_MATRIX";
public const string PermissionUpdateRole = "PERMISSION_UPDATE_ROLE";

// Cập nhật ActionNames.All thêm 2 action này (tổng cộng 24 actions).
```

- [ ] **Step 4: Chạy test để xác nhận test PASS**

Run: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter "ActionNames_ContainsNewPermissionActions|UserRole_ContainsManager"`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add QuanLyTro/QuanLyTro.Shared/ QuanLyTro/QuanLyTro.Tests/ProtocolTests.cs
git commit -m "feat(protocol): add Manager role and permission management actions"
```

---

### Task 2: CSDL - Migration Schema & Seed Demo Account

**Files:**
- Modify: `QuanLyTro/database/schema.sql`
- Modify: `QuanLyTro/QuanLyTro.Server/Data/DemoSeeder.cs`
- Test: `QuanLyTro/QuanLyTro.Tests/DemoSeederTests.cs`

**Interfaces:**
- Consumes: `UserRole.Manager`
- Produces: Bảng `role_permissions`, migration thêm `Manager` vào `users.role`, seed tài khoản `manager` / `manager` và các quyền mặc định.

- [ ] **Step 1: Viết test cho DemoSeeder có tài khoản Manager và quyền mặc định**

Trong `QuanLyTro/QuanLyTro.Tests/DemoSeederTests.cs`:
```csharp
[TestMethod]
public void DemoSeeder_HasManagerCredentials()
{
    Assert.AreEqual("manager", DemoSeeder.ManagerUsername);
}
```

- [ ] **Step 2: Cập nhật `database/schema.sql`**

Bổ sung bảng `role_permissions` và migration cập nhật enum:
```sql
CREATE TABLE IF NOT EXISTS role_permissions (
    role VARCHAR(20) NOT NULL,
    action VARCHAR(50) NOT NULL,
    PRIMARY KEY (role, action)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
-- statement

-- Cập nhật enum role cho users nếu thiếu Manager
ALTER TABLE users MODIFY COLUMN role ENUM('Landlord', 'Manager', 'Police', 'Tenant') NOT NULL DEFAULT 'Landlord';
-- statement
```

- [ ] **Step 3: Cập nhật `DemoSeeder.cs`**

Thêm `ManagerUsername = "manager"`, thêm seeding tài khoản `manager`/`manager` vào `users` và seed danh sách quyền mặc định vào `role_permissions`:
- Manager mặc định được cấp: Toàn bộ quyền phòng trọ, khách thuê, hợp đồng, điện nước, hóa đơn, báo cáo doanh thu (`ActionNames.Room*`, `ActionNames.Tenant*`, `ActionNames.Contract*`, `ActionNames.Utility*`, `ActionNames.Invoice*`, `ActionNames.ReportSummary`, `ActionNames.ExportResidence`).
- Police mặc định được cấp: Các quyền đọc cư trú (`ActionNames.RoomGetAll`, `ActionNames.TenantGetByRoom`, `ActionNames.ExportResidence`, `ActionNames.ResidenceHistoryGet`, `ActionNames.ExportResidenceHistory`).
- Tenant mặc định: `ActionNames.InvoiceGetMine`.

- [ ] **Step 4: Chạy test xác nhận DemoSeeder**

Run: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter "DemoSeeder"`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add QuanLyTro/database/schema.sql QuanLyTro/QuanLyTro.Server/Data/DemoSeeder.cs QuanLyTro/QuanLyTro.Tests/DemoSeederTests.cs
git commit -m "feat(db): add role_permissions table and seed manager demo account"
```

---

### Task 3: Phân quyền Động - PermissionRepository & Hybrid PermissionMatrix

**Files:**
- Create: `QuanLyTro/QuanLyTro.Server/Repositories/IPermissionRepository.cs`
- Create: `QuanLyTro/QuanLyTro.Server/Repositories/PermissionRepository.cs`
- Modify: `QuanLyTro/QuanLyTro.Server/Network/PermissionMatrix.cs`
- Test: `QuanLyTro/QuanLyTro.Tests/PermissionMatrixTests.cs`

**Interfaces:**
- Produces: `IPermissionRepository` (GetMatrixAsync, UpdateRoleActionsAsync), `PermissionMatrix.InitializeAsync()`, `PermissionMatrix.ReloadAsync()`, `PermissionMatrix.IsAllowed(action, role)`.

- [ ] **Step 1: Viết test cho PermissionMatrix hỗ trợ Manager và Dynamic Reload**

Trong `QuanLyTro/QuanLyTro.Tests/PermissionMatrixTests.cs`:
```csharp
[TestMethod]
public void Manager_HasOperationalPermissions_ButNotPermissionAdmin()
{
    Assert.IsTrue(PermissionMatrix.IsAllowed(ActionNames.RoomGetAll, UserRole.Manager));
    Assert.IsTrue(PermissionMatrix.IsAllowed(ActionNames.RoomAdd, UserRole.Manager));
    Assert.IsTrue(PermissionMatrix.IsAllowed(ActionNames.InvoicePay, UserRole.Manager));
    
    // Quản lý KHÔNG được phép quản lý quyền
    Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.PermissionUpdateRole, UserRole.Manager));
    Assert.IsFalse(PermissionMatrix.IsAllowed(ActionNames.PermissionGetMatrix, UserRole.Manager));
}

[TestMethod]
public void Landlord_HasAllPermissions_IncludingPermissionAdmin()
{
    Assert.IsTrue(PermissionMatrix.IsAllowed(ActionNames.PermissionGetMatrix, UserRole.Landlord));
    Assert.IsTrue(PermissionMatrix.IsAllowed(ActionNames.PermissionUpdateRole, UserRole.Landlord));
}
```

- [ ] **Step 2: Chạy test để thấy FAIL**

Run: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter "Manager_HasOperationalPermissions|Landlord_HasAllPermissions"`
Expected: FAIL.

- [ ] **Step 3: Triển khai `IPermissionRepository` và `PermissionRepository`**

Đọc và lưu dữ liệu từ bảng `role_permissions`. Khi đọc trả về `Dictionary<string, HashSet<string>>`. Khi lưu dùng Transaction để thay thế bộ action của một role.

- [ ] **Step 4: Nâng cấp `PermissionMatrix.cs`**

- `Landlord`: Hardcoded return `action != ActionNames.InvoiceGetMine`.
- Dynamic matrix: Nạp từ `IPermissionRepository`, lưu trữ trong `ConcurrentDictionary<string, HashSet<UserRole>>`.
- Hàm `SetPermissions(Dictionary<string, HashSet<UserRole>> dynamicMap)` cho phép reload tức thì.

- [ ] **Step 5: Chạy test xác nhận PASS**

Run: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter "PermissionMatrixTests"`
Expected: PASS toàn bộ.

- [ ] **Step 6: Commit**

```bash
git add QuanLyTro/QuanLyTro.Server/Repositories/ QuanLyTro/QuanLyTro.Server/Network/PermissionMatrix.cs QuanLyTro/QuanLyTro.Tests/PermissionMatrixTests.cs
git commit -m "feat(auth): implement dynamic PermissionMatrix with in-memory caching and Landlord bypass"
```

---

### Task 4: Server Router & Permission Handlers

**Files:**
- Modify: `QuanLyTro/QuanLyTro.Server/Network/RequestRouter.cs`
- Modify: `QuanLyTro/QuanLyTro.Server/Program.cs`
- Test: `QuanLyTro/QuanLyTro.Tests/TcpRoundTripTests.cs`

**Interfaces:**
- Consumes: `IPermissionRepository`, `PermissionMatrix`
- Produces: Router handles `ActionNames.PermissionGetMatrix`, `ActionNames.PermissionUpdateRole`.

- [ ] **Step 1: Viết test RoundTrip kiểm tra API phân quyền qua TCP**

Trong `QuanLyTro/QuanLyTro.Tests/TcpRoundTripTests.cs`:
```csharp
[TestMethod]
public async Task TcpRoundTrip_LandlordCanGetAndSetPermissions_ManagerCannot()
{
    // Test đăng nhập Landlord lấy permission matrix -> thành công
    // Test đăng nhập Manager gọi PERMISSION_UPDATE_ROLE -> bị từ chối "Không có quyền."
}
```

- [ ] **Step 2: Thêm handler vào `RequestRouter.cs`**

```csharp
[ActionNames.PermissionGetMatrix] = (_, _, ct) => Ok(_permissions.GetMatrixAsync(ct)),
[ActionNames.PermissionUpdateRole] = (r, s, ct) => UpdatePermissionsAsync(r, s, ct),
```
Kiểm tra `s.Role == UserRole.Landlord` trong `UpdatePermissionsAsync`, cập nhật CSDL và gọi `PermissionMatrix.ReloadAsync()`.

- [ ] **Step 3: Cập nhật dependency injection trong `Program.cs`**

Đăng ký `PermissionRepository`, khởi tạo `PermissionMatrix` lúc nạp Server.

- [ ] **Step 4: Chạy test xác nhận PASS**

Run: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter "TcpRoundTrip_LandlordCanGetAndSetPermissions"`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add QuanLyTro/QuanLyTro.Server/Network/RequestRouter.cs QuanLyTro/QuanLyTro.Server/Program.cs QuanLyTro/QuanLyTro.Tests/TcpRoundTripTests.cs
git commit -m "feat(server): wire permission matrix endpoints into TCP router"
```

---

### Task 5: Giao diện Form Đăng nhập Thống nhất

**Files:**
- Modify: `QuanLyTro/Assets/wwwroot/auth/index.html`
- Modify: `QuanLyTro/Assets/wwwroot/auth/auth.css`
- Modify: `QuanLyTro/Assets/wwwroot/auth/auth.js`
- Test: `QuanLyTro/QuanLyTro.Tests/WebMessageBridgeTests.cs`

**Interfaces:**
- Form đăng nhập chỉ có Username (`#lu`), Password (`#lp`), Nút Submit (`doLogin()`).
- Tự động điều hướng:
  - `Landlord` -> `../landlord/index.html?role=Landlord&u=...`
  - `Manager` -> `../landlord/index.html?role=Manager&u=...`
  - `Police` -> `../police/index.html?u=...`
  - `Tenant` -> `../tenant/index.html?u=...`

- [ ] **Step 1: Cập nhật `auth/index.html`**

Xóa bỏ hoàn toàn thẻ `<div class="roleseg">...</div>`.
Chỉnh sửa placeholder của ô `#lu`: `placeholder="Tài khoản hoặc số CCCD"`.

- [ ] **Step 2: Cập nhật `auth/auth.css`**

Loại bỏ css thừa liên quan đến `.roleseg`.

- [ ] **Step 3: Cập nhật `auth/auth.js`**

Cập nhật `ROLE_ROUTES`:
```javascript
const ROLE_ROUTES = {
  0: '../landlord/index.html?role=Landlord',
  1: '../tenant/index.html',
  2: '../police/index.html',
  3: '../landlord/index.html?role=Manager',
  'Landlord': '../landlord/index.html?role=Landlord',
  'Tenant': '../tenant/index.html',
  'Police': '../police/index.html',
  'Manager': '../landlord/index.html?role=Manager'
};
```
Khi gọi `AUTH_LOGIN`, nhận role trả về từ server và redirect kèm thông tin username và role.

- [ ] **Step 4: Chạy test WebMessageBridge và Auth**

Run: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter "WebMessageBridgeTests|AuthTests"`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add QuanLyTro/Assets/wwwroot/auth/
git commit -m "feat(ui): unify login form to single username/password form with auto-routing"
```

---

### Task 6: Màn hình Phân quyền & Thích ứng Giao diện Chủ trọ / Quản lý

**Files:**
- Modify: `QuanLyTro/Assets/wwwroot/landlord/index.html`
- Create: `QuanLyTro/Assets/wwwroot/landlord/js/perms.js`
- Modify: `QuanLyTro/Assets/wwwroot/landlord/js/main.js`
- Modify: `QuanLyTro/Assets/wwwroot/landlord/landlord.css`

**Interfaces:**
- Tab `perms`: "Phân quyền" (chỉ hiển thị khi `role === 'Landlord'`).
- Script `perms.js`: Hàm `loadPerms()`, hiển thị danh sách quyền theo role, nút `savePerms()`.
- Menu thích ứng: Khi `role === 'Manager'`, ẩn tab `perms`, hiển thị huy hiệu `Quản lý cơ sở`.

- [ ] **Step 1: Cập nhật `landlord/index.html`**

- Thêm tab Phân quyền trong thanh tab:
  `<div class="tab" id="tabbtn-perms" data-tab="perms" onclick="loadLandlordTab('perms')"><i class="fas fa-user-shield"></i> Phân quyền</div>`
- Thêm container nội dung `<div id="tab-perms" hidden>...</div>` chứa selector chọn role cần gán (Quản lý, Công an, Người thuê) và danh sách checkbox các quyền.
- Include file `js/perms.js` ở cuối trang.

- [ ] **Step 2: Viết script `landlord/js/perms.js`**

- Hàm `loadPerms()`: Gửi action `PERMISSION_GET_MATRIX` qua `window.bridge.call`.
- Render bảng quyền phân nhóm trực quan (Phòng trọ, Khách thuê, Hợp đồng, Điện nước, Hóa đơn, Báo cáo).
- Hàm `saveRolePermissions()`: Thu thập checkbox được tick của role hiện tại, gửi `PERMISSION_UPDATE_ROLE`, hiển thị `toast('Đã cập nhật phân quyền thành công!', 'ok')`.

- [ ] **Step 3: Cập nhật `landlord/js/main.js`**

- Đọc param `role` từ URL:
  - Nếu `role === 'Manager'`:
    - Đổi badge thành `<i class="fas fa-user-tie"></i> Quản lý cơ sở`.
    - Ẩn tab `#tabbtn-perms`.
  - Nếu `role === 'Landlord'`:
    - Đổi badge thành `<i class="fas fa-crown"></i> Chủ trọ (Toàn quyền)`.
    - Hiện tab `#tabbtn-perms`.
- Tích hợp loader cho `perms`: `perms: () => loadPerms()`.

- [ ] **Step 4: Chạy build & test kiểm tra file assets**

Run: `dotnet build QuanLyTro/QuanLyTro.slnx`
Expected: Build thành công không có lỗi.

- [ ] **Step 5: Commit**

```bash
git add QuanLyTro/Assets/wwwroot/landlord/
git commit -m "feat(ui): add role permissions management view for Landlord and adapt Manager view"
```

---

### Task 7: Kiểm thử Tổng thể & Nghiệm thu (Verification)

**Files:**
- Run: Toàn bộ test suite trong `QuanLyTro.Tests`

- [ ] **Step 1: Đóng các tiến trình đang chạy (nếu có lock file) và chạy toàn bộ unit/integration test**

Run: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --nologo -v normal`
Expected: 100% tests PASS (>= 195 tests, 0 failed).

- [ ] **Step 2: Commit và tổng kết**

```bash
git status
git commit -m "test: verify all unit and round-trip tests pass with unified login and dynamic RBAC"
```
