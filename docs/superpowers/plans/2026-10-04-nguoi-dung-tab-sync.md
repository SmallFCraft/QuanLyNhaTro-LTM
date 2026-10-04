# [Người Dùng Tab Sync] Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Chuyển đổi tab "Người thuê" thành tab "Người dùng" trên giao diện Chủ trọ, hiển thị toàn bộ người dùng bao gồm cả khách đã đăng ký tài khoản nhưng chưa nhận phòng (`phong_id IS NULL`), bổ sung cột trạng thái thuê và bộ lọc trực quan.

**Architecture:** Mở rộng client JavaScript của tab `khach_thue` để gọi song song `KHACH_THUE_THEO_PHONG` với `phongId: 0` kết hợp dữ liệu từng phòng; mở rộng bảng HTML thêm cột `Trạng thái` và dropdown lọc; bảo toàn tuyệt đối ma trận phân quyền backend và hợp đồng API TCP sẵn có.

**Tech Stack:** HTML5 / CSS3 / Vanilla JS (ES6+), Microsoft.Web.WebView2, C# .NET 8.0 MSTest.

**Spec:** `docs/superpowers/specs/2026-10-04-nguoi-dung-tab-sync-design.md`

## Global Constraints

- Không đổi tên ID `tab-khach_thue` hoặc attribute `data-tab="khach_thue"` để tránh phá vỡ giao thức router và các bài test contract hiện hữu.
- Sử dụng chuẩn `KHACH_THUE_THEO_PHONG` với `phongId: 0` của server cho khách chưa gán phòng; không tự ý thêm action mới vào ActionNames nếu không cần thiết.
- Mọi kiểm thử `dotnet test` phải chạy xanh tuyệt đối (0 fail).
- Tuyệt đối không sinh file rác, file ảnh chụp màn hình không kiểm soát.

## Review Focus

1. **Khách chưa có phòng (`phongId == null`):** Hiển thị badge trạng thái `Chờ nhận phòng`, phòng `Chưa gán`, nút Chuyển phòng / Trả phòng phải xử lý an toàn không gây lỗi NaN.
2. **Khách đang thuê (`phongId != null`):** Hiển thị badge trạng thái `Đang thuê`, phòng `P...`, các thao tác chuyển/trả phòng bình thường.
3. **Bộ lọc phòng:** Lọc đúng khách chờ nhận phòng khi chọn option `-- Chờ nhận phòng --`.
4. **Đếm động:** Badge đếm số lượng người dùng trên tab `cnt-khach_thue` cập nhật đúng tổng số khách.
5. **Contract Test:** Các assertion trong `LandlordTabContractTests` phản ánh đúng các thẻ mới.

---

### Task 1: Bổ sung Contract Test cho Tab "Người dùng" và Cột Trạng Thái

**Files:**
- Modify: `QuanLyTro/QuanLyTro.Tests/LandlordTabContractTests.cs`

**Interfaces:**
- Consumes: `QuanLyTro/Assets/wwwroot/chutro/index.html`, `QuanLyTro/Assets/wwwroot/chutro/js/khach_thue.js`
- Produces: Test methods `LandlordTenantTab_IncludesUnassignedQuery_AndStatusColumn`

- [ ] **Step 1: Viết failing test kiểm tra tab Người dùng và cột Trạng thái**

Thêm test method vào `LandlordTabContractTests.cs`:
```csharp
[TestMethod]
public void LandlordTenantTab_IncludesUnassignedQuery_AndStatusColumn()
{
    var html = File.ReadAllText(Path.Combine(Wwwroot, "chutro", "index.html"));
    var js = File.ReadAllText(Path.Combine(Wwwroot, "chutro", "js", "khach_thue.js"));

    // 1. Phải đổi tên tab thành "Người dùng"
    Assert.IsTrue(html.Contains("Người dùng"), "Tab khach_thue phải mang tên 'Người dùng'");
    Assert.IsTrue(html.Contains("id=\"cnt-khach_thue\""), "Tab khach_thue phải có badge đếm id='cnt-khach_thue'");

    // 2. Bảng phải có cột Trạng thái
    Assert.IsTrue(html.Contains("<th>Trạng thái</th>"), "Bảng người dùng phải có cột header '<th>Trạng thái</th>'");

    // 3. JS phải gọi lấy khách chưa có phòng (phongId: 0)
    Assert.IsTrue(js.Contains("phongId: 0") || js.Contains("phongId:0"),
        "khach_thue.js phải gọi KHACH_THUE_THEO_PHONG với phongId: 0 để lấy khách chưa gán phòng");

    // 4. JS phải render trạng thái thuê
    Assert.IsTrue(js.Contains("Chờ nhận phòng") && js.Contains("Đang thuê"),
        "khach_thue.js phải render nhãn trạng thái 'Chờ nhận phòng' và 'Đang thuê'");
}
```

- [ ] **Step 2: Chạy test để xác nhận test FAIL đúng như mong đợi**

Run: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter LandlordTenantTab_IncludesUnassignedQuery_AndStatusColumn --nologo -v q`
Expected: FAIL với thông báo thiếu nhãn `Người dùng` hoặc cột `Trạng thái`.

---

### Task 2: Cập Nhật Giao Diện `chutro/index.html` và `chutro/js/main.js`

**Files:**
- Modify: `QuanLyTro/Assets/wwwroot/chutro/index.html:40, 114-145`
- Modify: `QuanLyTro/Assets/wwwroot/chutro/js/main.js:5`

**Interfaces:**
- Consumes: Cấu trúc HTML của tab Chủ trọ
- Produces: HTML tab Người dùng với badge `#cnt-khach_thue` và cột `<th>Trạng thái</th>`

- [ ] **Step 1: Đổi nhãn tab thành "Người dùng" và thêm badge đếm**

Trong `chutro/index.html` dòng 40:
```html
<div class="tab" data-tab="khach_thue" onclick="loadLandlordTab('khach_thue')"><i class="fas fa-users"></i> Người dùng <span class="cnt" id="cnt-khach_thue">0</span></div>
```

- [ ] **Step 2: Thêm cột `<th>Trạng thái</th>` vào bảng `#tab-khach_thue`**

Trong `chutro/index.html` bảng người dùng:
```html
<thead><tr><th>Phòng</th><th>Trạng thái</th><th>Họ tên</th><th>CCCD</th><th>Ngày sinh</th><th>SĐT</th><th>Quê quán</th><th>Tạm trú</th><th></th></tr></thead>
```
Cập nhật cả colspan của các dòng rỗng / mẫu thành `colspan="9"`.

- [ ] **Step 3: Cập nhật tiêu đề module trong `chutro/js/main.js`**

```javascript
khach_thue: 'Quản lý người dùng & khách thuê',
```

---

### Task 3: Cập Nhật `chutro/js/khach_thue.js` để Nạp Khách Chờ Gán Phòng & Render Trạng Thái

**Files:**
- Modify: `QuanLyTro/Assets/wwwroot/chutro/js/khach_thue.js`

**Interfaces:**
- Consumes: `KHACH_THUE_THEO_PHONG` với `{ phongId: 0 }` và `{ phongId: r.id }`
- Produces: Bảng người dùng đầy đủ trạng thái, dropdown lọc đa năng

- [ ] **Step 1: Mở rộng `loadTenants()` để truy vấn cả khách chưa có phòng**

```javascript
const [phong, pending] = await Promise.all([
  window.bridge.call('PHONG_LAY_TAT_CA', {}) || [],
  window.bridge.call('KHACH_THUE_THEO_PHONG', { phongId: 0 }) || []
]);

const lists = await Promise.all(phong.map(async r => {
  const khach = await window.bridge.call('KHACH_THUE_THEO_PHONG', { phongId: r.id }) || [];
  return khach.map(t => ({ ...t, soPhong: r.soPhong }));
}));

// Gộp khách chưa có phòng (phongId = null, soPhong = null) và khách trong phòng
tenantCache = [
  ...pending.map(t => ({ ...t, soPhong: null, phongId: null })),
  ...lists.flat()
];

// Cập nhật badge đếm trên tab
const cntBadge = document.getElementById('cnt-khach_thue');
if (cntBadge) cntBadge.textContent = tenantCache.length;
```

- [ ] **Step 2: Cập nhật dropdown bộ lọc `#tenant-room`**

```javascript
if (sel) {
  const keep = sel.value;
  const opts = [
    `<option value="">-- Tất cả (${tenantCache.length} người) --</option>`,
    `<option value="pending">-- Chờ nhận phòng (${pending.length}) --</option>`,
    ...phong.map(r =>
      `<option value="${esc(r.id)}">${esc(r.soPhong)} (${r.soNguoiHienTai ?? 0}/${r.soNguoiToiDa})</option>`)
  ];
  sel.innerHTML = opts.join('');
  if (keep !== undefined && keep !== null && keep !== '') sel.value = keep;
  else sel.value = '';
}
```

- [ ] **Step 3: Cập nhật hàm lọc và render hàng trong `renderTenants()`**

Hỗ trợ lọc theo phòng và theo trạng thái `pending`:
```javascript
const filterVal = sel && sel.value ? String(sel.value) : '';
const rows = filterVal === 'pending'
  ? tenantCache.filter(t => t.phongId == null)
  : (filterVal ? tenantCache.filter(t => String(t.phongId) === filterVal) : tenantCache);

body.innerHTML = rows.map(t => {
  const hasRoom = t.phongId != null;
  const roomTag = hasRoom
    ? `<span class="tag rent">${esc(t.soPhong || ('P.' + t.phongId))}</span>`
    : `<span class="tag warn">Chưa gán</span>`;
  const statusTag = hasRoom
    ? `<span class="tag done"><i class="fas fa-check-circle"></i> Đang thuê</span>`
    : `<span class="tag warn"><i class="fas fa-clock"></i> Chờ nhận phòng</span>`;

  return `
    <tr data-id="${t.id}" onclick="this.parentNode.querySelectorAll('tr').forEach(r=>r.classList.remove('sel'));this.classList.add('sel')">
      <td>${roomTag}</td>
      <td>${statusTag}</td>
      <td><b>${esc(t.hoTen)}</b></td>
      <td class="mono">${esc(t.cccd)}</td>
      <td class="mono">${fmtDateOnly(t.ngaySinh)}</td>
      <td class="mono">${esc(t.soDienThoai)}</td>
      <td>${esc(t.queQuan)}</td>
      <td><span class="tag ${t.daDangKyTamTru ? 'done' : 'warn'}">${t.daDangKyTamTru ? 'Đã nộp' : 'Chưa nộp'}</span></td>
      <td><button class="btn btn-outline btn-sm" onclick="event.stopPropagation();resetTenantPassword(${t.id})" title="Đặt lại mật khẩu"><i class="fas fa-key"></i></button></td>
    </tr>`;
}).join('') || `<tr><td colspan="9" style="text-align:center;color:var(--dim)">Không có người dùng nào</td></tr>`;
```

- [ ] **Step 4: Bảo vệ an toàn các nút hành động (Chuyển phòng, Trả phòng)**

Trong `moveTenant` và `checkoutTenant`: kiểm tra nếu `!t.phongId` thì báo thông báo thân thiện: "Người dùng chưa được gán vào phòng nào." thay vì ném lỗi gọi API.

---

### Task 4: Kiểm Thử Tự Động & Thẩm Định Hồi Quy Toàn Bộ

**Files:**
- Verify: Toàn bộ solution `QuanLyTro.slnx`

- [ ] **Step 1: Chạy test contract mới**

Run: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --filter LandlordTenantTab_IncludesUnassignedQuery_AndStatusColumn --nologo -v q`
Expected: PASS 1/1.

- [ ] **Step 2: Chạy kiểm thử tự động Node.js mô phỏng logic render**

Chạy kịch bản test trên terminal kiểm tra cả 3 luồng: Khách có phòng, Khách tự đăng ký, Bộ lọc pending.
Expected: PASS.

- [ ] **Step 3: Chạy toàn bộ test suite dự án**

Run: `dotnet test "QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj" --nologo -v q`
Expected: 241/241 test cases PASS.
