# Kế Hoạch Triển Khai: Tách Multi-Page WebView2 Cho Các Tác Nhân (Phase A)

> **Dành cho agent thực thi:** BẮT BUỘC DÙNG SUB-SKILL: Dùng `superpowers:subagent-driven-development` (khuyến nghị) hoặc `superpowers:executing-plans` để thực thi từng task. Mỗi bước dùng cú pháp checkbox (`- [ ]`) để theo dõi tiến độ.

**Mục tiêu:** Tách giao diện đơn khối `index.html` trong WebView2 thành kiến trúc Multi-page với cây thư mục outline rõ ràng cho 3 tác nhân hiện có và Auth (`auth/`, `landlord/`, `police/`, `tenant/`, `shared/`), nâng cấp cầu điều hướng C# hỗ trợ `UI_LOGOUT`, cập nhật đường dẫn kiểm thử để toàn bộ 183 tests hiện tại giữ màu xanh tuyệt đối, không để lại file rác hay shim trùng lặp.

**Kiến trúc:** 
1. Mỗi tác nhân là một thư mục con hoàn chỉnh độc lập (`index.html`, css, js), loại bỏ hoàn toàn đè biến global giữa các vai.
2. Tài nguyên dùng chung tập trung ở `shared/css/` (tokens, base) và `shared/js/` (bridge, ui formatters).
3. `Form1.cs` khởi động thẳng vào `auth/index.html`.
4. `WebMessageBridge.cs` chặn action nội bộ `UI_LOGOUT` để xóa `_client.Token` an toàn.
5. Cập nhật các file test contract sang cấu trúc mới, cấm `localStorage`/`sessionStorage`, bảo vệ 100% không nút chết.

**Công nghệ:** C# .NET 8 WinForms, Microsoft.Web.WebView2 (1.0.2903.40), HTML5/CSS3/Vanilla JS (ES6+), MSTest.

**Tài liệu thiết kế (Spec):** [`docs/superpowers/specs/2026-10-01-multi-actor-multipage-webview2-design.md`](../specs/2026-10-01-multi-actor-multipage-webview2-design.md)

---

## Ràng Buộc Chung (Global Constraints)

- **Không dùng framework JS ngoài:** Chỉ dùng Vanilla JavaScript chuẩn, không webpack, không npm build, không babel.
- **Không lộ token xuống DOM:** Token phiên bắt buộc ở lại C# `TcpClientService.Token` (spec §3.2, C2).
- **Cấm hoàn toàn Web Storage:** Tuyệt đối không dùng `localStorage` hoặc `sessionStorage` trong bất kỳ file script hay HTML nào.
- **Không nhân đôi file:** Khi tách xong module nào, xóa file đơn khối tương ứng ở root (không giữ bản copy cũ).
- **Không phá vỡ test suite:** Sau mỗi task, lệnh kiểm thử `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --nologo -v q` phải chạy qua 183 tests hoặc nhiều hơn, không có lỗi biên dịch.

---

## Trọng Tâm Rà Soát (Review Focus)

1. **Điều hướng WebView2 bằng đường dẫn tương đối:** Trong môi trường nạp file local `file:///`, việc chuyển trang `window.location.href = '../landlord/index.html?u=...'` hoặc C# `Navigate()` phải trỏ đúng file vật lý trong `Assets/wwwroot/`.
2. **Biến dùng chung không bị undefined:** Các hàm tiện ích `esc()`, `fmtMoney()`, `fmtDate()`, `toast()` trong `shared/js/ui.js` phải được nạp trước các script nghiệp vụ trong mọi trang `index.html`.
3. **Bridge listener sẵn sàng:** `shared/js/bridge.js` phải lắng nghe `window.chrome.webview.addEventListener('message')` ngay khi trang vừa tải để không bỏ lỡ thông điệp TCP trả về từ C#.
4. **Không có nút chết (No dead buttons):** Mọi thẻ `<button>` trong toàn bộ các file `index.html` của các tác nhân phải có thuộc tính `onclick`, `type="submit"`, hoặc `disabled`.
5. **Cấm quyền ghi ở vai chỉ đọc:** Cả shell `police` và `tenant` tuyệt đối không được chứa bất kỳ nút bấm hay action nào liên quan đến Thêm/Sửa/Xóa dữ liệu.

---

## Chi Tiết Các Tác Vụ Triển Khai

### Task 1: Thiết lập thư mục `shared/` và Nâng Cấp `WebMessageBridge.cs`

**Mục tiêu:** Tạo `shared/css/` (tokens, base) và `shared/js/` (bridge, ui), đồng thời dạy `WebMessageBridge.cs` xử lý action cục bộ `UI_LOGOUT` mà không gửi lên TCP Server.

**Files:**
- Tạo mới: `QuanLyTro/Assets/wwwroot/shared/css/tokens.css`
- Tạo mới: `QuanLyTro/Assets/wwwroot/shared/css/base.css`
- Tạo mới: `QuanLyTro/Assets/wwwroot/shared/js/bridge.js`
- Tạo mới: `QuanLyTro/Assets/wwwroot/shared/js/ui.js`
- Sửa đổi: `QuanLyTro/Network/WebMessageBridge.cs`
- Sửa đổi: `QuanLyTro/QuanLyTro.Tests/AssetPackagingTests.cs`
- Sửa đổi: `QuanLyTro/QuanLyTro.Tests/WebMessageBridgeTests.cs`

**Interfaces:**
- Action `UI_LOGOUT`: xóa `_client.Token = null`, trả `success: true`.
- `window.bridge.call(action, data)` -> `Promise<object>`
- `esc(str)`, `fmtMoney(val)`, `fmtDate(iso)`, `fmtDateOnly(iso)`, `toast(msg, kind)`, `openModal(config)`

- [ ] **Bước 1: Viết test cho `Wwwroot_ContainsSharedAssets` và `DispatchAsync_UiLogout`**
  Thêm vào `AssetPackagingTests.cs`:
  ```csharp
  [TestMethod]
  public void Wwwroot_ContainsSharedAssets()
  {
      Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "shared", "css", "tokens.css")), "Thiếu tokens.css");
      Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "shared", "css", "base.css")), "Thiếu base.css");
      Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "shared", "js", "bridge.js")), "Thiếu shared bridge.js");
      Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "shared", "js", "ui.js")), "Thiếu shared ui.js");
  }
  ```
  Thêm vào `WebMessageBridgeTests.cs`:
  ```csharp
  [TestMethod]
  public async Task DispatchAsync_UiLogout_ClearsClientToken()
  {
      var client = new TcpClientService { Token = "test_token" };
      var bridge = new WebMessageBridge(client);
      string? responseJson = null;

      await bridge.DispatchAsync(
          """{"requestId":"r_out","action":"UI_LOGOUT","data":{}}""",
          msg => { responseJson = msg; return Task.CompletedTask; });

      Assert.IsNull(client.Token, "UI_LOGOUT phải xóa token");
      StringAssert.Contains(responseJson, "\"success\":true");
  }
  ```

- [ ] **Bước 2: Chạy test để xác nhận FAIL**
  Chạy: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter "FullyQualifiedName~Wwwroot_ContainsSharedAssets|FullyQualifiedName~DispatchAsync_UiLogout" --nologo -v q`
  Kết quả dự kiến: FAIL cả 2 test.

- [ ] **Bước 3: Tạo các asset trong `shared/` và cập nhật `WebMessageBridge.cs`**
  - Trích xuất biến `:root` từ `Assets/wwwroot/css/style.css` vào `shared/css/tokens.css`.
  - Trích xuất reset, typography, `.btn`, `.inp`, `.tbl`, modal, toast vào `shared/css/base.css`.
  - Sao chép `js/bridge.js` sang `shared/js/bridge.js` (bỏ hàm `showShell` vì không còn dùng mô hình ẩn hiện shell).
  - Sao chép `js/ui.js` sang `shared/js/ui.js`.
  - Trong `WebMessageBridge.cs`, thêm xử lý `env.Action == "UI_LOGOUT"` ngay trước khi gọi TCP:
    ```csharp
    if (env.Action == "UI_LOGOUT")
    {
        _client.Token = null;
        var ok = JsonSerializer.Serialize(new { requestId = reqId, success = true, data = (object?)null, error = (string?)null }, JsonDefaults.Options);
        await postBack(ok);
        return;
    }
    ```

- [ ] **Bước 4: Chạy test xác nhận PASS**
  Chạy: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter "FullyQualifiedName~Wwwroot_ContainsSharedAssets|FullyQualifiedName~DispatchAsync_UiLogout" --nologo -v q`
  Kết quả: PASS cả 2 test.

- [ ] **Bước 5: Commit Task 1**
  ```bash
  git add QuanLyTro/Assets/wwwroot/shared/ QuanLyTro/Network/WebMessageBridge.cs QuanLyTro/QuanLyTro.Tests/AssetPackagingTests.cs QuanLyTro/QuanLyTro.Tests/WebMessageBridgeTests.cs
  git commit -m "feat(ui): scaffold shared assets and implement UI_LOGOUT bridge action"
  ```

---

### Task 2: Tách Màn Hình Đăng Nhập (`auth/`)

**Mục tiêu:** Tạo module đăng nhập độc lập tại `Assets/wwwroot/auth/`, nhận phản hồi đăng nhập và điều hướng sang trang tác nhân kèm tên người dùng trong query string.

**Files:**
- Tạo mới: `QuanLyTro/Assets/wwwroot/auth/index.html`
- Tạo mới: `QuanLyTro/Assets/wwwroot/auth/auth.css`
- Tạo mới: `QuanLyTro/Assets/wwwroot/auth/auth.js`
- Sửa đổi: `QuanLyTro/QuanLyTro.Tests/AssetPackagingTests.cs`

**Interfaces:**
- Nhận role từ `AUTH_LOGIN`, điều hướng:
  - `Landlord` -> `../landlord/index.html?u=...`
  - `Police`   -> `../police/index.html?u=...`
  - `Tenant`   -> `../tenant/index.html?u=...`

- [ ] **Bước 1: Viết test kiểm tra asset màn hình auth**
  Thêm vào `AssetPackagingTests.cs`:
  ```csharp
  [TestMethod]
  public void Wwwroot_ContainsAuthModule()
  {
      Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "auth", "index.html")), "Thiếu auth/index.html");
      Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "auth", "auth.css")), "Thiếu auth/auth.css");
      Assert.IsTrue(File.Exists(Path.Combine(Wwwroot, "auth", "auth.js")), "Thiếu auth/auth.js");
  }
  ```

- [ ] **Bước 2: Chạy test xác nhận FAIL**
  Chạy: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter "FullyQualifiedName~Wwwroot_ContainsAuthModule" --nologo -v q`
  Kết quả: FAIL.

- [ ] **Bước 3: Dựng `auth/index.html`, `auth.css`, `auth.js`**
  - `auth/index.html`: Chứa form đăng nhập với 3 role radio, ô tài khoản `#lu`, mật khẩu `#lp`, nút submit `<button type="submit" ...>`.
  - Nạp: `../shared/css/tokens.css`, `../shared/css/base.css`, `auth.css`.
  - Nạp: `../shared/js/bridge.js`, `../shared/js/ui.js`, `auth.js`.
  - `auth.js`:
    ```javascript
    const ROLE_ROUTES = {
      0: '../landlord/index.html',
      1: '../tenant/index.html',
      2: '../police/index.html',
      '0': '../landlord/index.html',
      '1': '../tenant/index.html',
      '2': '../police/index.html',
      'Landlord': '../landlord/index.html',
      'Tenant': '../tenant/index.html',
      'Police': '../police/index.html'
    };

    async function doLogin() {
      const username = document.getElementById('lu').value.trim();
      const password = document.getElementById('lp').value;
      if (!username || !password) {
        alert('Vui lòng nhập tài khoản và mật khẩu.');
        return;
      }
      try {
        const user = await window.bridge.call('AUTH_LOGIN', { Username: username, Password: password });
        if (!user) return;
        const roleVal = user.role ?? user.Role;
        const target = ROLE_ROUTES[roleVal] || '../landlord/index.html';
        const name = encodeURIComponent(user.fullName ?? user.FullName ?? '');
        window.location.href = `${target}?u=${name}`;
      } catch (err) {
        alert(err.message);
      }
    }
    ```

- [ ] **Bước 4: Chạy test xác nhận PASS**
  Chạy: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter "FullyQualifiedName~Wwwroot_ContainsAuthModule" --nologo -v q`
  Kết quả: PASS.

- [ ] **Bước 5: Commit Task 2**
  ```bash
  git add QuanLyTro/Assets/wwwroot/auth/ QuanLyTro/QuanLyTro.Tests/AssetPackagingTests.cs
  git commit -m "feat(ui): create standalone auth module with query string routing"
  ```

---

### Task 3: Tách Màn Hình Chủ Trọ (`landlord/`)

**Mục tiêu:** Chuyển toàn bộ 7 tab của Chủ trọ sang `Assets/wwwroot/landlord/` và cấu trúc lại thư mục script.

**Files:**
- Tạo mới: `QuanLyTro/Assets/wwwroot/landlord/index.html`
- Tạo mới: `QuanLyTro/Assets/wwwroot/landlord/landlord.css`
- Tạo mới: `QuanLyTro/Assets/wwwroot/landlord/js/main.js`
- Chuyển: sao chép và cập nhật các file từ `Assets/wwwroot/js/landlord/*.js` sang `Assets/wwwroot/landlord/js/*.js`
- Sửa đổi: `QuanLyTro/QuanLyTro.Tests/AssetPackagingTests.cs`
- Sửa đổi: `QuanLyTro/QuanLyTro.Tests/LandlordTabContractTests.cs`
- Sửa đổi: `QuanLyTro/QuanLyTro.Tests/DashRoomsContractTests.cs`
- Sửa đổi: `QuanLyTro/QuanLyTro.Tests/TenantsContractsContractTests.cs`
- Sửa đổi: `QuanLyTro/QuanLyTro.Tests/UtilsInvoicesReportsContractTests.cs`

- [ ] **Bước 1: Cập nhật đường dẫn test của Landlord sang thư mục mới**
  - Trong `AssetPackagingTests.cs`, thêm `Wwwroot_ContainsLandlordModule` kiểm tra tồn tại `landlord/index.html`, `landlord.css`, `js/main.js` và 7 file tab.
  - Trong `LandlordTabContractTests.cs`, `DashRoomsContractTests.cs`, `TenantsContractsContractTests.cs`, `UtilsInvoicesReportsContractTests.cs`: đổi `Path.Combine(Wwwroot, "index.html")` thành `Path.Combine(Wwwroot, "landlord", "index.html")`; đổi `Path.Combine(Wwwroot, "js", "landlord", ...)` thành `Path.Combine(Wwwroot, "landlord", "js", ...)`.

- [ ] **Bước 2: Chạy test xác nhận FAIL**
  Chạy: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter "FullyQualifiedName~LandlordTabContractTests" --nologo -v q`
  Kết quả: FAIL (chưa có file tại vị trí mới).

- [ ] **Bước 3: Dựng module `landlord/`**
  - `landlord/index.html`: Chứa cấu trúc của shell `#landlord` (loại bỏ `hidden`, bỏ `#login`, `#police`, `#tenant`).
  - Nạp: `../shared/css/tokens.css`, `../shared/css/base.css`, `landlord.css`.
  - Nạp: `../shared/js/bridge.js`, `../shared/js/ui.js`, `js/main.js`, và 7 script `js/{dash,rooms,tenants,contracts,utils,invoices,reports}.js`.
  - `landlord/js/main.js`:
    - Khởi tạo `LANDLORD_TITLES` và `LANDLORD_LOADERS`.
    - Đọc query param `u` để gán vào `#uname`.
    - Hàm `logout()`:
      ```javascript
      async function logout() {
        await window.bridge.call('UI_LOGOUT', {});
        window.location.href = '../auth/index.html';
      }
      ```
    - Tự động gọi `loadLandlordTab('dash')` khi `DOMContentLoaded`.

- [ ] **Bước 4: Chạy lại toàn bộ test Landlord để xác nhận PASS**
  Chạy: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter "FullyQualifiedName~Landlord|FullyQualifiedName~DashRooms|FullyQualifiedName~TenantsContracts|FullyQualifiedName~UtilsInvoicesReports" --nologo -v q`
  Kết quả: PASS toàn bộ.

- [ ] **Bước 5: Commit Task 3**
  ```bash
  git add QuanLyTro/Assets/wwwroot/landlord/ QuanLyTro/QuanLyTro.Tests/
  git commit -m "feat(ui): migrate landlord module into standalone directory"
  ```

---

### Task 4: Tách Màn Hình Công An Phường (`police/`)

**Mục tiêu:** Chuyển giao diện Công an phường (3 tabs: Công dân, Tạm trú, Biến động) sang `Assets/wwwroot/police/`.

**Files:**
- Tạo mới: `QuanLyTro/Assets/wwwroot/police/index.html`
- Tạo mới: `QuanLyTro/Assets/wwwroot/police/police.css`
- Tạo mới: `QuanLyTro/Assets/wwwroot/police/js/main.js`
- Tạo mới: `QuanLyTro/Assets/wwwroot/police/js/citizens.js`
- Tạo mới: `QuanLyTro/Assets/wwwroot/police/js/residence.js`
- Tạo mới: `QuanLyTro/Assets/wwwroot/police/js/history.js`
- Sửa đổi: `QuanLyTro/QuanLyTro.Tests/PoliceShellContractTests.cs`
- Sửa đổi: `QuanLyTro/QuanLyTro.Tests/AssetPackagingTests.cs`

- [ ] **Bước 1: Cập nhật đường dẫn test của Police sang thư mục mới**
  - Trong `AssetPackagingTests.cs`, thêm `Wwwroot_ContainsPoliceModule`.
  - Trong `PoliceShellContractTests.cs`: đổi đường dẫn đọc `index.html` sang `police/index.html`.
  - Sửa `FrontEnd_NeverReadsSessionToken` để quét đệ quy mọi file `.js` và `.html` trong toàn bộ `Assets/wwwroot/`.

- [ ] **Bước 2: Chạy test xác nhận FAIL**
  Chạy: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter "FullyQualifiedName~PoliceShellContractTests" --nologo -v q`
  Kết quả: FAIL.

- [ ] **Bước 3: Dựng module `police/`**
  - `police/index.html`: Chứa shell `#police` không có `hidden`. Giữ nguyên mọi ID: `#p-search`, `#p-from`, `#p-to`, `#ptab-citizens`, `#ptab-residence`, `#ptab-history`.
  - Đảm bảo toàn bộ thẻ `<button>` đều có `disabled` hoặc `onclick`. Tuyệt đối không có nút ghi.
  - Phân tách logic từ `js/police.js` thành `js/citizens.js`, `js/residence.js`, `js/history.js`, và `js/main.js`.
  - Nút đăng xuất gọi: `UI_LOGOUT` và chuyển về `../auth/index.html`.

- [ ] **Bước 4: Chạy test xác nhận PASS**
  Chạy: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter "FullyQualifiedName~Police" --nologo -v q`
  Kết quả: PASS toàn bộ.

- [ ] **Bước 5: Commit Task 4**
  ```bash
  git add QuanLyTro/Assets/wwwroot/police/ QuanLyTro/QuanLyTro.Tests/PoliceShellContractTests.cs QuanLyTro/QuanLyTro.Tests/AssetPackagingTests.cs
  git commit -m "feat(ui): migrate police module into standalone directory"
  ```

---

### Task 5: Tách Màn Hình Khách Thuê (`tenant/`)

**Mục tiêu:** Chuyển giao diện Khách thuê sang `Assets/wwwroot/tenant/`.

**Files:**
- Tạo mới: `QuanLyTro/Assets/wwwroot/tenant/index.html`
- Tạo mới: `QuanLyTro/Assets/wwwroot/tenant/tenant.css`
- Tạo mới: `QuanLyTro/Assets/wwwroot/tenant/js/tenant.js`
- Sửa đổi: `QuanLyTro/QuanLyTro.Tests/TenantShellContractTests.cs`
- Sửa đổi: `QuanLyTro/QuanLyTro.Tests/AssetPackagingTests.cs`

- [ ] **Bước 1: Cập nhật đường dẫn test của Tenant sang thư mục mới**
  - Trong `AssetPackagingTests.cs`, thêm `Wwwroot_ContainsTenantModule`.
  - Trong `TenantShellContractTests.cs`: đổi đường dẫn đọc sang `tenant/index.html` và `tenant/js/tenant.js`.

- [ ] **Bước 2: Chạy test xác nhận FAIL**
  Chạy: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter "FullyQualifiedName~TenantShellContractTests" --nologo -v q`
  Kết quả: FAIL.

- [ ] **Bước 3: Dựng module `tenant/`**
  - `tenant/index.html`: Chứa shell `#tenant` không có `hidden`. Giữ đúng các ID: `#tenant-history-body`, `#rc-room`, `#rc-elec`, `#rc-water`, `#rc-other`, `#rc-total`.
  - Di chuyển `js/tenant.js` vào `tenant/js/tenant.js`, nạp tên từ query string `u`, hàm đăng xuất gọi `UI_LOGOUT` và chuyển về `../auth/index.html`.

- [ ] **Bước 4: Chạy test xác nhận PASS**
  Chạy: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter "FullyQualifiedName~TenantShellContractTests" --nologo -v q`
  Kết quả: PASS.

- [ ] **Bước 5: Commit Task 5**
  ```bash
  git add QuanLyTro/Assets/wwwroot/tenant/ QuanLyTro/QuanLyTro.Tests/TenantShellContractTests.cs QuanLyTro/QuanLyTro.Tests/AssetPackagingTests.cs
  git commit -m "feat(ui): migrate tenant module into standalone directory"
  ```

---

### Task 6: Cấu Hình Entry Point, Dọn Dẹp File Cũ & Toàn Bộ 183 Tests Xanh

**Mục tiêu:** Cập nhật `Form1.cs` trỏ vào `auth/index.html`, xóa các file cũ không còn dùng ở root (`index.html`, `css/style.css`, `js/`), cập nhật các test suite còn lại (`SanityTests.cs`, `UiHelpersContractTests.cs`, `NoDeadButtonsTests.cs`, `AssetPackagingTests.cs`), bảo đảm toàn bộ suite xanh 100%.

**Files:**
- Sửa đổi: `QuanLyTro/Form1.cs`
- Xóa bỏ: `QuanLyTro/Assets/wwwroot/index.html`
- Xóa bỏ: `QuanLyTro/Assets/wwwroot/css/`
- Xóa bỏ: `QuanLyTro/Assets/wwwroot/js/`
- Sửa đổi: `QuanLyTro/QuanLyTro.Tests/SanityTests.cs`
- Sửa đổi: `QuanLyTro/QuanLyTro.Tests/UiHelpersContractTests.cs`
- Sửa đổi: `QuanLyTro/QuanLyTro.Tests/NoDeadButtonsTests.cs`
- Sửa đổi: `QuanLyTro/QuanLyTro.Tests/AssetPackagingTests.cs`

- [ ] **Bước 1: Cập nhật `Form1.cs`**
  Tại [Form1.cs:54](QuanLyTro/Form1.cs:54):
  ```csharp
  var htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "wwwroot", "auth", "index.html");
  webView.CoreWebView2.Navigate(new Uri(htmlPath).AbsoluteUri);
  ```

- [ ] **Bước 2: Xóa các file đơn khối cũ tại root `Assets/wwwroot`**
  Xóa: `index.html`, thư mục `css/`, thư mục `js/`.

- [ ] **Bước 3: Cập nhật `SanityTests.cs`, `UiHelpersContractTests.cs`, `NoDeadButtonsTests.cs`, `AssetPackagingTests.cs`**
  - `SanityTests.cs`: trỏ đọc `landlord/js/rooms.js` và `landlord/index.html`.
  - `UiHelpersContractTests.cs`: trỏ đọc `shared/js/ui.js`; kiểm tra các ID trên từng file HTML của từng tác nhân; kiểm tra thứ tự nạp thẻ `<script>` trên từng file HTML.
  - `NoDeadButtonsTests.cs`: quét đệ quy mọi file `*.html` trong `Assets/wwwroot/` kiểm tra không nút nào chết.
  - `AssetPackagingTests.cs`: kiểm tra sự tồn tại của `auth/index.html`, `shared/js/bridge.js`, `shared/css/base.css`, v.v.

- [ ] **Bước 4: Chạy toàn bộ test suite dự án**
  Chạy: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --nologo -v q`
  Kết quả dự kiến: Toàn bộ 183+ tests đều PASS, không có lỗi biên dịch hay cảnh báo mới nào.

- [ ] **Bước 5: Commit Task 6**
  ```bash
  git add QuanLyTro/Form1.cs QuanLyTro/Assets/wwwroot/ QuanLyTro/QuanLyTro.Tests/
  git commit -m "refactor(ui): finalize multi-page structure, point Form1 to auth and cleanup legacy files"
  ```

---

## Bàn Giao & Handoff Thực Thi

Kế hoạch đã hoàn thành và lưu tại `docs/superpowers/plans/2026-10-01-multi-actor-multipage-webview2.md`.  
Người dùng đã chọn phương thức: **Subagent-driven** (chia các task cho subagent chạy, review độc lập từng bước).

Sẵn sàng chuyển sang skill `superpowers:subagent-driven-development` để bắt đầu Task 1.
