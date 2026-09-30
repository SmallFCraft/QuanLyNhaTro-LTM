# Wire Every Screen: Dữ liệu thật + CRUD đầy đủ

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax.

**Goal:** Mỗi màn hình trong `index.html` phải tải dữ liệu thật từ Server và mọi nút bấm phải hoạt động — bao gồm form nhập liệu cho tất cả thao tác ghi (thêm/sửa/xóa phòng, người thuê, hợp đồng, chốt điện nước, lập hóa đơn, thu tiền).

**Architecture:** HTML/CSS template đã có sẵn trong `index.html` (copy từ mockup). Công việc còn lại là **nối dữ liệu**: mỗi tab gọi action TCP tương ứng qua `window.bridge.call(action, data)` và render vào bảng; mỗi nút mở modal nhập liệu rồi gửi action ghi. Không đổi kiến trúc — vẫn `TcpClientService` → Server → MySQL.

**Tech Stack:** JavaScript thuần (không framework), HTML/CSS có sẵn, MSTest cho contract test, `Microsoft.Web.WebView2` đã cài.

**Spec:** [`docs/superpowers/specs/2026-09-30-police-webview2-design.md`](../specs/2026-09-30-police-webview2-design.md)

## Global Constraints

- **Chỉ dùng action đã có trong `ActionNames`.** Không thêm action mới, không sửa Server, không sửa Shared. Danh sách hợp lệ nằm ở `QuanLyTro/QuanLyTro.Shared/Protocol/ActionNames.cs`.
- **camelCase bắt buộc.** `JsonDefaults.Options` dùng `JsonNamingPolicy.CamelCase`; mọi field JS đọc phải là camelCase (`r.roomNumber`, `i.totalAmount`).
- **Enum là chuỗi.** `JsonStringEnumConverter` khiến `RoomStatus`/`ContractStatus`/`InvoiceStatus`/`UserRole` serialize thành chuỗi: `"Available"`, `"Rented"`, `"Maintenance"`, `"Active"`, `"Expired"`, `"Terminated"`, `"Unpaid"`, `"Paid"`, `"Landlord"`, `"Tenant"`, `"Police"`.
- **Chống XSS:** mọi chuỗi từ Server phải qua hàm `esc()` trước khi nội suy vào `innerHTML`. `police.js` đã có `esc()` — các file khác phải dùng chung.
- **BR-16:** shell Công an phường tuyệt đối không có nút ghi, không có form nhập liệu.
- **BR-11:** hóa đơn đã thu không được sửa/xóa — nút phải `disabled` khi `status === 'Paid'`.
- **BR-12:** chỉ xóa được phòng trống — nút Xóa `disabled` khi `currentOccupants > 0`.
- **Token:** không bao giờ đọc `user.token` trong JS; token chỉ sống trong C#.

## Ràng buộc cấu trúc file (bắt buộc, để chạy song song không đụng nhau)

`landlord.js` hiện là một file lớn. Tách thành thư mục, mỗi tab một file — như vậy nhiều agent sửa được cùng lúc mà không tranh chấp:

```
QuanLyTro/Assets/wwwroot/js/
├── bridge.js          (KHÔNG SỬA — cầu nối dùng chung)
├── login.js           (KHÔNG SỬA)
├── ui.js              (MỚI — Task 8: modal + toast + esc + format dùng chung)
├── landlord.js        (sửa: chỉ còn bộ điều phối, gọi các file con)
├── landlord/
│   ├── dash.js        (Task 9)
│   ├── rooms.js       (Task 9)
│   ├── tenants.js     (Task 10)
│   ├── contracts.js   (Task 10)
│   ├── utils.js       (Task 11)
│   ├── invoices.js    (Task 11)
│   └── reports.js     (Task 11)
├── police.js          (Task 12)
└── tenant.js          (Task 13)
```

Mỗi task chỉ được ghi vào **file của mình** + test của mình. Task 8 sở hữu `ui.js` và sửa `index.html` để thêm `<script src>` + vùng chứa modal.

## API Server đã có (dùng đúng những cái này)

| Action | Payload gửi lên | Trả về |
|---|---|---|
| `ROOM_GET_ALL` | `{}` | `RoomDto[]` |
| `ROOM_ADD` / `ROOM_UPDATE` | `RoomDto` | `RoomDto` |
| `ROOM_DELETE` | `{ roomId }` | ok |
| `TENANT_GET_BY_ROOM` | `{ roomId }` | `TenantDto[]` |
| `TENANT_ADD` / `TENANT_UPDATE` | `TenantDto` | `TenantDto` |
| `TENANT_CHECKOUT` | `{ tenantId }` | ok |
| `TENANT_DELETE` | `{ tenantId }` | ok |
| `CONTRACT_GET_ALL` | `{}` | `ContractDto[]` |
| `CONTRACT_CREATE` | `ContractDto` | `ContractDto` |
| `CONTRACT_RENEW` | `{ contractId, newEndDate }` | `ContractDto` |
| `CONTRACT_TERMINATE` | `{ contractId, notes }` | ok |
| `UTILITY_GET_PREVIOUS` | `{ roomId, billingMonth? }` | `UtilityReadingDto` |
| `UTILITY_RECORD` | `UtilityReadingDto` | ok |
| `INVOICE_CREATE` | `{ roomId, billingMonth, otherFees }` | `InvoiceDto` |
| `INVOICE_GET_ALL` | `{ billingMonth, roomId? }` | `InvoiceDto[]` |
| `INVOICE_PAY` | `{ invoiceId }` | ok |
| `REPORT_SUMMARY` | `{ billingMonth }` | `SummaryReportDto` |
| `EXPORT_RESIDENCE` | `{}` | `{ filePath, rowCount }` |

**DTO chính xác (dùng đúng tên field camelCase):**
- `RoomDto`: `id, roomNumber, price, maxOccupants, status, description, currentOccupants`
- `TenantDto`: `id, roomId, fullName, dateOfBirth, idCard, phone, hometown, workplace, isTemporaryRegistered`
- `ContractDto`: `id, roomId, representativeTenantId, startDate, endDate, rentalPrice, depositAmount, status, notes`
- `InvoiceDto`: `id, roomId, contractId, billingMonth, roomAmount, electricityAmount, waterAmount, otherFees, totalAmount, status, paidAt`
- `UtilityReadingDto`: `id, roomId, billingMonth, oldElectricity, newElectricity, electricityRate, oldWater, newWater, waterRate`
- `SummaryReportDto`: `totalRooms, availableRooms, rentedRooms, currentTenants, paidAmount, unpaidAmount`

---

### Task 8: Hạ tầng dùng chung — modal, toast, format, esc

**Files:**
- Create: `QuanLyTro/Assets/wwwroot/js/ui.js`
- Modify: `QuanLyTro/Assets/wwwroot/index.html` (thêm `<script src="js/ui.js">` + `<div id="modal-root">`)
- Test: `QuanLyTro/QuanLyTro.Tests/UiHelpersContractTests.cs`

**Interfaces — Produces (mọi task sau dùng):**
- `esc(s) -> string` — escape HTML
- `fmtMoney(n) -> string` — `2245000` → `"2.245.000"`
- `fmtDate(iso) -> string` — `"2026-09-05T10:14:00"` → `"05/09/2026 10:14"`
- `fmtDateOnly(iso) -> string` — `"2026-09-05"` → `"05/09/2026"`
- `toast(msg, kind)` — `kind` ∈ `'ok' | 'err' | 'info'`
- `openModal({ title, fields, onSubmit })` — `fields` là mảng `{ name, label, type, value, options?, required?, disabled? }`; `type` ∈ `'text' | 'number' | 'date' | 'select' | 'password'`; submit gọi `onSubmit(values)` với object `{name: value}`; trả về `close()`
- `confirmDialog(msg) -> Promise<boolean>`

- [ ] **Step 1: Viết failing test**

```csharp
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class UiHelpersContractTests
{
    private static string Wwwroot =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "wwwroot");

    [TestMethod]
    public void UiJs_ExposesSharedHelpers()
    {
        var js = File.ReadAllText(Path.Combine(Wwwroot, "js", "ui.js"));
        foreach (var fn in new[] { "function esc", "function fmtMoney", "function fmtDate",
                                   "function fmtDateOnly", "function toast", "function openModal",
                                   "function confirmDialog" })
        {
            Assert.IsTrue(js.Contains(fn), $"ui.js thiếu {fn}");
        }
    }

    [TestMethod]
    public void IndexHtml_LoadsUiJs_AndHasModalRoot()
    {
        var html = File.ReadAllText(Path.Combine(Wwwroot, "index.html"));
        Assert.IsTrue(html.Contains("js/ui.js"), "index.html chưa nạp ui.js");
        Assert.IsTrue(html.Contains("id=\"modal-root\""), "index.html thiếu #modal-root");
    }
}
```

- [ ] **Step 2: Chạy test — xác nhận FAIL**

Run: `dotnet test QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj --filter "FullyQualifiedName~UiHelpersContractTests" -v m`
Expected: FAIL — thiếu `ui.js`.

- [ ] **Step 3: Viết `ui.js`**

```javascript
// Tiện ích dùng chung cho mọi shell. Không phụ thuộc bridge.
function esc(s) {
  return String(s ?? '').replace(/[&<>"']/g, c => (
    { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]
  ));
}

function fmtMoney(n) {
  const v = Number(n);
  return Number.isFinite(v) ? v.toLocaleString('vi-VN') : '0';
}

function fmtDate(iso) {
  if (!iso) return '—';
  const d = new Date(iso);
  if (isNaN(d)) return '—';
  const p = n => String(n).padStart(2, '0');
  return `${p(d.getDate())}/${p(d.getMonth() + 1)}/${d.getFullYear()} ${p(d.getHours())}:${p(d.getMinutes())}`;
}

function fmtDateOnly(iso) {
  if (!iso) return '—';
  const s = String(iso).slice(0, 10).split('-');
  return s.length === 3 ? `${s[2]}/${s[1]}/${s[0]}` : String(iso);
}

function toast(msg, kind) {
  const root = document.getElementById('modal-root');
  if (!root) { alert(msg); return; }
  const el = document.createElement('div');
  el.className = `toast ${kind || 'info'}`;
  el.textContent = msg;
  root.appendChild(el);
  setTimeout(() => el.remove(), 4000);
}

function openModal({ title, fields, onSubmit }) {
  const root = document.getElementById('modal-root');
  const back = document.createElement('div');
  back.className = 'modal-back';
  const rows = (fields || []).map(f => {
    const id = 'mf_' + f.name;
    let ctrl;
    if (f.type === 'select') {
      const opts = (f.options || []).map(o =>
        `<option value="${esc(o.value)}"${String(o.value) === String(f.value ?? '') ? ' selected' : ''}>${esc(o.label)}</option>`
      ).join('');
      ctrl = `<select class="inp" id="${id}" name="${esc(f.name)}"${f.disabled ? ' disabled' : ''}>${opts}</select>`;
    } else {
      ctrl = `<input class="inp" id="${id}" name="${esc(f.name)}" type="${f.type || 'text'}"
        value="${esc(f.value ?? '')}"${f.disabled ? ' disabled' : ''}${f.required ? ' required' : ''}>`;
    }
    return `<div class="modal-row"><label for="${id}">${esc(f.label)}</label>${ctrl}</div>`;
  }).join('');

  back.innerHTML = `
    <div class="modal">
      <div class="modal-hdr">${esc(title || '')}</div>
      <form class="modal-body">${rows}
        <div class="modal-ft">
          <button type="button" class="btn btn-outline btn-sm" data-cancel>Hủy</button>
          <button type="submit" class="btn btn-primary btn-sm">Lưu</button>
        </div>
      </form>
    </div>`;

  const close = () => back.remove();
  back.querySelector('[data-cancel]').addEventListener('click', close);
  back.addEventListener('click', e => { if (e.target === back) close(); });
  back.querySelector('form').addEventListener('submit', async e => {
    e.preventDefault();
    const values = {};
    new FormData(e.target).forEach((v, k) => { values[k] = typeof v === 'string' ? v.trim() : v; });
    try {
      await onSubmit(values);
      close();
    } catch (err) {
      toast(err.message, 'err');
    }
  });
  root.appendChild(back);
  const first = back.querySelector('input:not([disabled]), select:not([disabled])');
  if (first) first.focus();
  return close;
}

function confirmDialog(msg) {
  return Promise.resolve(window.confirm(msg));
}
```

- [ ] **Step 4: Thêm CSS modal + toast vào `style.css`**

```css
  /* ===== modal + toast (dùng chung mọi shell) ===== */
  .modal-back{position:fixed;inset:0;background:rgba(0,0,0,.72);display:flex;
    align-items:center;justify-content:center;z-index:50}
  .modal{background:#181C1F;border:1px solid #262A2E;border-radius:8px;min-width:380px;max-width:520px;
    max-height:86vh;overflow:auto;box-shadow:0 12px 40px rgba(0,0,0,.55)}
  .modal-hdr{padding:12px 14px;border-bottom:1px solid #21272C;font-size:13px;font-weight:600;
    color:#FAF8F5;background:#14191D}
  .modal-body{padding:14px}
  .modal-row{margin-bottom:10px}
  .modal-row label{display:block;font-size:11px;color:#A89988;margin-bottom:4px}
  .modal-row .inp{width:100%}
  .modal-ft{display:flex;justify-content:flex-end;gap:8px;margin-top:14px;
    padding-top:12px;border-top:1px solid #21272C}
  .toast{position:fixed;right:16px;bottom:16px;padding:10px 14px;border-radius:6px;font-size:12px;
    background:#1A1F24;border:1px solid #262A2E;color:#F4EFEA;z-index:60;max-width:380px}
  .toast.ok{border-color:rgba(139,215,163,.5);color:#8BD7A3}
  .toast.err{border-color:rgba(217,93,57,.5);color:#D95D39}
```

- [ ] **Step 5: Cập nhật `index.html` — Task 8 là nơi DUY NHẤT sửa file này**

Mọi thay đổi `index.html` cho toàn bộ plan được gộp vào bước này để các task 9–13 chỉ ghi file `.js` riêng và chạy song song không tranh chấp. Thực hiện hết:

1. Trước `</body>`: thêm `<div id="modal-root"></div>`
2. Danh sách script — thứ tự bắt buộc (bridge → ui → các file con):
```html
<script src="js/bridge.js"></script>
<script src="js/ui.js"></script>
<script src="js/login.js"></script>
<script src="js/landlord.js"></script>
<script src="js/landlord/dash.js"></script>
<script src="js/landlord/rooms.js"></script>
<script src="js/landlord/tenants.js"></script>
<script src="js/landlord/contracts.js"></script>
<script src="js/landlord/utils.js"></script>
<script src="js/landlord/invoices.js"></script>
<script src="js/landlord/reports.js"></script>
<script src="js/police.js"></script>
<script src="js/tenant.js"></script>
```
3. Gắn `id` cho mọi phần tử mà các task 9–13 cần đọc:

| Vùng | id cần gắn |
|---|---|
| Tab Tổng quan | `kpi-total-rooms`, `kpi-available`, `kpi-tenants`, `kpi-unpaid` (các `<div class="val">`); `dash-overdue-rows`, `dash-expiring-rows` (hai `<tbody>`) |
| Tab Phòng | `room-search` (+`oninput="renderRooms(_roomsCache)"`), `room-status` (+`onchange="renderRooms(_roomsCache)"`), nút Thêm `onclick="addRoom()"` |
| Tab Người thuê | `tenant-room` (+`onchange="loadTenants()"`), `tenants-body` (tbody), nút Thêm `onclick="addTenant()"`, 4 nút `.rowbn` → `editTenant(selectedTenantId())` / `moveTenant(selectedTenantId())` / `checkoutTenant(selectedTenantId())` / `deleteTenant(selectedTenantId())` |
| Tab Hợp đồng | `contracts-body` (tbody), nút `onclick="createContract()"`, nút `onclick="loadContracts()"`, 2 nút → `renewContract(selectedContractId())` / `terminateContract(selectedContractId())` |
| Tab Điện nước | `util-room` (+`onchange="loadUtils()"`), `util-month`, `util-elec-old` (disabled), `util-elec-new`, `util-elec-rate`, `util-water-old` (disabled), `util-water-new`, `util-water-rate` (mọi ô nhập `oninput="recalcUtils()"`), nút `onclick="saveUtility()"` |
| Tab Hóa đơn | `inv-month` + `inv-room` (đều `onchange="loadInvoices()"`), `invoices-body` (tbody), nút `onclick="createInvoice()"` |
| Tab Thống kê | `rep-month` (+`onchange="loadReports()"`), nút Xem `onclick="loadReports()"`, nút CSV `onclick="exportResidence()"` |
| Shell Công an | ô tìm kiếm `p-search` (+`oninput="loadPoliceTab('citizens')"`), `p-from`, `p-to`, nút Tra cứu `onclick="loadPoliceTab('citizens')"`, nút Xuất danh sách `onclick="exportHistory()"`, nút Xem trước `onclick="loadHistory()"`, nút Xuất file `onclick="exportHistory()"` |
| Shell Khách thuê | `tenant-history-body` (tbody), `rc-room`, `rc-elec`, `rc-water`, `rc-other`, `rc-total`, nút sao chép `onclick="copyTransfer()"` |

4. Với mọi nút còn lại chưa có handler: gắn `disabled` + `title` giải thích (Task 14 sẽ kiểm tra lại).

- [ ] **Step 6: Cập nhật `landlord.js` thành bộ điều phối**

`landlord.js` chỉ còn `LANDLORD_TITLES`, `LANDLORD_LOADERS` và hàm `loadLandlordTab(tabKey)` gọi loader tương ứng, đúng như mục Step 5 của Task 9. Task 9 viết nội dung này; Task 8 chỉ cần đảm bảo `index.html` nạp đủ script để các loader tồn tại về sau.

- [ ] **Step 7: Chạy test — xác nhận PASS**

Run: `dotnet test QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj --filter "FullyQualifiedName~UiHelpersContractTests" -v m`
Expected: PASS 2/2.

- [ ] **Step 8: Commit**

```bash
git add QuanLyTro/Assets/wwwroot/js/ui.js \
  QuanLyTro/Assets/wwwroot/css/style.css QuanLyTro/Assets/wwwroot/index.html \
  QuanLyTro/QuanLyTro.Tests/UiHelpersContractTests.cs
git commit -m "feat(ui): shared helpers plus all screen element ids and scripts"
```

---

### Task 9: Tab Tổng quan + Phòng (CRUD đầy đủ)

**Files:**
- Create: `QuanLyTro/Assets/wwwroot/js/landlord/dash.js`
- Create: `QuanLyTro/Assets/wwwroot/js/landlord/rooms.js`
- Modify: `QuanLyTro/Assets/wwwroot/js/landlord.js` (bộ điều phối gọi file con)
- Test: `QuanLyTro/QuanLyTro.Tests/DashRoomsContractTests.cs`

**Interfaces:**
- Consumes: `esc`, `fmtMoney`, `toast`, `openModal`, `confirmDialog` (Task 8); `window.bridge.call`
- Produces: `loadDash()`, `loadRooms()`, `renderRooms(rows)`

- [ ] **Step 1: Viết failing test**

```csharp
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class DashRoomsContractTests
{
    private static string Wwwroot =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "wwwroot");

    [TestMethod]
    public void RoomsJs_CoversFullCrudSurface()
    {
        var js = File.ReadAllText(Path.Combine(Wwwroot, "js", "landlord", "rooms.js"));
        foreach (var action in new[] { "ROOM_GET_ALL", "ROOM_ADD", "ROOM_UPDATE", "ROOM_DELETE" })
        {
            Assert.IsTrue(js.Contains(action), $"rooms.js thiếu {action}");
        }
        // BR-12: nút xóa phải bị chặn khi phòng còn người
        Assert.IsTrue(js.Contains("currentOccupants"), "rooms.js thiếu kiểm tra sức chứa để chặn xóa");
    }

    [TestMethod]
    public void DashJs_LoadsSummaryAndLists()
    {
        var js = File.ReadAllText(Path.Combine(Wwwroot, "js", "landlord", "dash.js"));
        Assert.IsTrue(js.Contains("REPORT_SUMMARY"), "dash.js chưa tải số liệu tổng quan");
        Assert.IsTrue(js.Contains("INVOICE_GET_ALL"), "dash.js chưa tải danh sách còn nợ");
        Assert.IsTrue(js.Contains("CONTRACT_GET_ALL"), "dash.js chưa tải danh sách HĐ sắp hết hạn");
    }

    [TestMethod]
    public void IndexHtml_LoadsLandlordSubScripts()
    {
        var html = File.ReadAllText(Path.Combine(Wwwroot, "index.html"));
        Assert.IsTrue(html.Contains("js/landlord/rooms.js"), "index.html chưa nạp rooms.js");
        Assert.IsTrue(html.Contains("js/landlord/dash.js"), "index.html chưa nạp dash.js");
    }
}
```

- [ ] **Step 2: Chạy test — xác nhận FAIL**

Run: `dotnet test QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj --filter "FullyQualifiedName~DashRoomsContractTests" -v m`
Expected: FAIL — thiếu thư mục `js/landlord/`.

- [ ] **Step 3: Viết `landlord/dash.js`**

```javascript
// Tab Tổng quan: 4 thẻ KPI + bảng còn nợ + bảng HĐ sắp hết hạn.
async function loadDash() {
  const month = new Date().toISOString().slice(0, 7); // yyyy-MM
  try {
    const [summary, invoices, contracts] = await Promise.all([
      window.bridge.call('REPORT_SUMMARY', { billingMonth: month }),
      window.bridge.call('INVOICE_GET_ALL', { billingMonth: month }),
      window.bridge.call('CONTRACT_GET_ALL', {})
    ]);

    if (summary) {
      setKpi('kpi-total-rooms', summary.totalRooms);
      setKpi('kpi-available', summary.availableRooms);
      setKpi('kpi-tenants', summary.currentTenants);
      setKpi('kpi-unpaid', fmtMoney(summary.unpaidAmount));
      const el = document.getElementById('kpi-available-sub');
      if (el) el.textContent =
        `Thuê ${summary.rentedRooms ?? 0} · Trống ${summary.availableRooms ?? 0}`;
    }

    const overdue = (invoices || []).filter(i => i.status !== 'Paid');
    const ob = document.getElementById('dash-overdue-rows');
    if (ob) ob.innerHTML = overdue.map(i => `
      <tr><td><b>${esc(i.roomNumber ?? ('P' + i.roomId))}</b></td>
      <td class="mono">${esc(i.billingMonth)}</td>
      <td class="num due-red">${fmtMoney(i.totalAmount)}</td>
      <td>${esc(i.representativeName ?? '—')}</td></tr>`).join('')
      || '<tr><td colspan="4" style="color:var(--dim)">Không có phòng nào còn nợ.</td></tr>';

    const soon = (contracts || []).filter(c => {
      if (c.status !== 'Active') return false;
      const days = (new Date(c.endDate) - new Date()) / 86400000;
      return days >= 0 && days <= 30;
    });
    const sb = document.getElementById('dash-expiring-rows');
    if (sb) sb.innerHTML = soon.map(c => {
      const days = Math.ceil((new Date(c.endDate) - new Date()) / 86400000);
      return `<tr><td><b>${esc(c.roomNumber ?? ('P' + c.roomId))}</b></td>
        <td>${esc(c.representativeName ?? '—')}</td>
        <td class="mono">${fmtDateOnly(c.endDate)}</td>
        <td class="num due-red">${days} ngày</td></tr>`;
    }).join('') || '<tr><td colspan="4" style="color:var(--dim)">Không có hợp đồng nào sắp hết hạn.</td></tr>';
  } catch (err) {
    toast(err.message, 'err');
  }
}

function setKpi(id, value) {
  const el = document.getElementById(id);
  if (el) el.textContent = value ?? '—';
}
```

- [ ] **Step 4: Viết `landlord/rooms.js`**

```javascript
// Tab Phòng: danh sách + thêm/sửa/xóa. BR-12: chỉ xóa phòng trống.
let _roomsCache = [];

async function loadRooms() {
  try {
    _roomsCache = await window.bridge.call('ROOM_GET_ALL', {}) || [];
    renderRooms(_roomsCache);
  } catch (err) {
    toast(err.message, 'err');
  }
}

function renderRooms(rows) {
  const body = document.querySelector('#tab-rooms tbody');
  if (!body) return;
  const q = (document.getElementById('room-search')?.value || '').toLowerCase().trim();
  const status = document.getElementById('room-status')?.value || '';
  const list = (rows || []).filter(r =>
    (!q || String(r.roomNumber).toLowerCase().includes(q)) &&
    (!status || r.status === status));

  body.innerHTML = list.map(r => {
    const full = (r.currentOccupants ?? 0) > 0;
    return `<tr>
      <td><b>${esc(r.roomNumber)}</b></td>
      <td class="num">${fmtMoney(r.price)}</td>
      <td class="num">${r.maxOccupants}</td>
      <td class="num ${full ? 'due-red' : ''}">${r.currentOccupants ?? 0}/${r.maxOccupants}</td>
      <td><span class="tag ${r.status === 'Rented' ? 'rent' : r.status === 'Maintenance' ? 'warn' : 'avail'}">
        ${r.status === 'Rented' ? 'Đang thuê' : r.status === 'Maintenance' ? 'Bảo trì' : 'Phòng trống'}</span></td>
      <td>${esc(r.description ?? '')}</td>
      <td>
        <button class="btn btn-outline btn-sm" onclick="editRoom(${r.id})"><i class="fas fa-edit"></i></button>
        <button class="btn btn-danger btn-sm" onclick="deleteRoom(${r.id})"${full ? ' disabled title="Chỉ xóa được phòng trống (BR-12)"' : ''}><i class="fas fa-trash"></i></button>
      </td></tr>`;
  }).join('') || '<tr><td colspan="7" style="color:var(--dim)">Chưa có phòng nào.</td></tr>';

  const foot = document.querySelector('#tab-rooms .tblfoot span');
  if (foot) foot.textContent = `Tổng: ${list.length} bản ghi`;
}

function addRoom() {
  openModal({
    title: 'Thêm phòng mới',
    fields: [
      { name: 'roomNumber', label: 'Số phòng', type: 'text', required: true },
      { name: 'price', label: 'Giá thuê (đ)', type: 'number', required: true },
      { name: 'maxOccupants', label: 'Sức chứa', type: 'number', value: '2', required: true },
      { name: 'description', label: 'Mô tả', type: 'text' }
    ],
    onSubmit: async v => {
      await window.bridge.call('ROOM_ADD', {
        roomNumber: v.roomNumber,
        price: Number(v.price),
        maxOccupants: Number(v.maxOccupants),
        status: 'Available',
        description: v.description || null
      });
      toast('Đã thêm phòng ' + v.roomNumber, 'ok');
      await loadRooms();
    }
  });
}

function editRoom(id) {
  const r = _roomsCache.find(x => x.id === id);
  if (!r) return;
  openModal({
    title: 'Sửa phòng ' + r.roomNumber,
    fields: [
      { name: 'roomNumber', label: 'Số phòng', type: 'text', value: r.roomNumber, required: true },
      { name: 'price', label: 'Giá thuê (đ)', type: 'number', value: r.price, required: true },
      { name: 'maxOccupants', label: 'Sức chứa', type: 'number', value: r.maxOccupants, required: true },
      { name: 'status', label: 'Trạng thái', type: 'select', value: r.status, options: [
        { value: 'Available', label: 'Phòng trống' },
        { value: 'Rented', label: 'Đang thuê' },
        { value: 'Maintenance', label: 'Bảo trì' }] },
      { name: 'description', label: 'Mô tả', type: 'text', value: r.description ?? '' }
    ],
    onSubmit: async v => {
      await window.bridge.call('ROOM_UPDATE', {
        id: r.id,
        roomNumber: v.roomNumber,
        price: Number(v.price),
        maxOccupants: Number(v.maxOccupants),
        status: v.status,
        description: v.description || null
      });
      toast('Đã cập nhật phòng ' + v.roomNumber, 'ok');
      await loadRooms();
    }
  });
}

async function deleteRoom(id) {
  const r = _roomsCache.find(x => x.id === id);
  if (!r) return;
  if (!await confirmDialog(`Xóa phòng ${r.roomNumber}? Thao tác không thể hoàn tác.`)) return;
  try {
    await window.bridge.call('ROOM_DELETE', { roomId: r.id });
    toast('Đã xóa phòng ' + r.roomNumber, 'ok');
    await loadRooms();
  } catch (err) {
    toast(err.message, 'err');
  }
}
```

- [ ] **Step 5: Cập nhật `landlord.js` thành bộ điều phối**

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

const LANDLORD_LOADERS = {
  dash: () => loadDash(),
  rooms: () => loadRooms(),
  tenants: () => loadTenants(),
  contracts: () => loadContracts(),
  utils: () => loadUtils(),
  invoices: () => loadInvoices(),
  reports: () => loadReports()
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
    const loader = LANDLORD_LOADERS[tabKey];
    if (loader) await loader();
  } catch (err) {
    toast(err.message, 'err');
  }
}
```

- [ ] **Step: (đã gộp vào Task 8 Step 5)** — mọi thay đổi `index.html` nằm ở Task 8; task này chỉ ghi file `.js` của mình.


- [ ] **Step 7: Chạy test — xác nhận PASS**

Run: `dotnet test QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj --filter "FullyQualifiedName~DashRoomsContractTests" -v m`
Expected: PASS 3/3.

- [ ] **Step 8: Commit**

```bash
git add QuanLyTro/Assets/wwwroot/js/landlord QuanLyTro/Assets/wwwroot/js/landlord.js \
  QuanLyTro/Assets/wwwroot/index.html QuanLyTro/QuanLyTro.Tests/DashRoomsContractTests.cs
git commit -m "feat(ui): wire dashboard KPIs and full room CRUD"
```

---

### Task 10: Tab Người thuê + Hợp đồng (CRUD đầy đủ)

**Files:**
- Create: `QuanLyTro/Assets/wwwroot/js/landlord/tenants.js`
- Create: `QuanLyTro/Assets/wwwroot/js/landlord/contracts.js`
- Test: `QuanLyTro/QuanLyTro.Tests/TenantsContractsContractTests.cs`

**Interfaces:**
- Consumes: `esc`, `fmtMoney`, `fmtDateOnly`, `toast`, `openModal`, `confirmDialog`
- Produces: `loadTenants()`, `loadContracts()`

- [ ] **Step 1: Viết failing test**

```csharp
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class TenantsContractsContractTests
{
    private static string Wwwroot =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "wwwroot");

    [TestMethod]
    public void TenantsJs_CoversFullCrudSurface()
    {
        var js = File.ReadAllText(Path.Combine(Wwwroot, "js", "landlord", "tenants.js"));
        foreach (var a in new[] { "TENANT_GET_BY_ROOM", "TENANT_ADD", "TENANT_UPDATE",
                                  "TENANT_CHECKOUT", "TENANT_DELETE" })
        {
            Assert.IsTrue(js.Contains(a), $"tenants.js thiếu {a}");
        }
    }

    [TestMethod]
    public void ContractsJs_CoversFullCrudSurface()
    {
        var js = File.ReadAllText(Path.Combine(Wwwroot, "js", "landlord", "contracts.js"));
        foreach (var a in new[] { "CONTRACT_GET_ALL", "CONTRACT_CREATE",
                                  "CONTRACT_RENEW", "CONTRACT_TERMINATE" })
        {
            Assert.IsTrue(js.Contains(a), $"contracts.js thiếu {a}");
        }
    }
}
```

- [ ] **Step 2: Chạy test — xác nhận FAIL**

Run: `dotnet test QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj --filter "FullyQualifiedName~TenantsContractsContractTests" -v m`
Expected: FAIL.

- [ ] **Step 3: Viết `landlord/tenants.js`**

Yêu cầu: `loadTenants()` tải `ROOM_GET_ALL` rồi `TENANT_GET_BY_ROOM` cho từng phòng (gắn `roomNumber` vào mỗi tenant), render bảng có nút Sửa / Chuyển phòng / Trả phòng / Xóa. `addTenant()` mở modal nhập họ tên, ngày sinh, CCCD, SĐT, quê quán, nơi học/làm, chọn phòng (chỉ phòng còn chỗ) → `TENANT_ADD`. `editTenant(id)` → `TENANT_UPDATE`. `checkoutTenant(id)` → `TENANT_CHECKOUT`. `deleteTenant(id)` → `TENANT_DELETE`. Dùng `fmtDateOnly` cho `dateOfBirth`. `isTemporaryRegistered` hiển thị tag "Đã nộp"/"Chưa nộp".

- [ ] **Step 4: Viết `landlord/contracts.js`**

Yêu cầu: `loadContracts()` tải `ROOM_GET_ALL` + `CONTRACT_GET_ALL`, gắn `roomNumber`, render bảng. Cập nhật 3 thẻ KPI (hiệu lực / sắp hết / tổng cọc) từ dữ liệu thật. `createContract()` mở modal chọn phòng trống, chọn người đại diện (tải từ `TENANT_GET_BY_ROOM` của phòng đó), nhập ngày BĐ/KT, giá thuê, tiền cọc → `CONTRACT_CREATE`. `renewContract(id)` mở modal nhập ngày KT mới → `CONTRACT_RENEW` với `{ contractId, newEndDate }`. `terminateContract(id)` mở modal nhập lý do → `CONTRACT_TERMINATE` với `{ contractId, notes }`. Cột "Còn" tô `due-red` khi ≤ 30 ngày.

- [ ] **Step: (đã gộp vào Task 8 Step 5)** — mọi thay đổi `index.html` nằm ở Task 8; task này chỉ ghi file `.js` của mình.


- [ ] **Step 6: Chạy test — xác nhận PASS**

Run: `dotnet test QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj --filter "FullyQualifiedName~TenantsContractsContractTests" -v m`
Expected: PASS 2/2.

- [ ] **Step 7: Commit**

```bash
git add QuanLyTro/Assets/wwwroot/js/landlord QuanLyTro/Assets/wwwroot/index.html \
  QuanLyTro/QuanLyTro.Tests/TenantsContractsContractTests.cs
git commit -m "feat(ui): wire tenant and contract CRUD"
```

---

### Task 11: Tab Điện nước + Hóa đơn + Thống kê

**Files:**
- Create: `QuanLyTro/Assets/wwwroot/js/landlord/utils.js`
- Create: `QuanLyTro/Assets/wwwroot/js/landlord/invoices.js`
- Create: `QuanLyTro/Assets/wwwroot/js/landlord/reports.js`
- Test: `QuanLyTro/QuanLyTro.Tests/UtilsInvoicesReportsContractTests.cs`

**Interfaces:**
- Consumes: `esc`, `fmtMoney`, `fmtDate`, `toast`, `openModal`, `confirmDialog`
- Produces: `loadUtils()`, `loadInvoices()`, `loadReports()`

- [ ] **Step 1: Viết failing test**

```csharp
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class UtilsInvoicesReportsContractTests
{
    private static string Wwwroot =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "wwwroot");

    [TestMethod]
    public void UtilsJs_LoadsPreviousAndSavesReading()
    {
        var js = File.ReadAllText(Path.Combine(Wwwroot, "js", "landlord", "utils.js"));
        Assert.IsTrue(js.Contains("UTILITY_GET_PREVIOUS"), "utils.js chưa tự điền chỉ số cũ");
        Assert.IsTrue(js.Contains("UTILITY_RECORD"), "utils.js chưa lưu được chỉ số");
    }

    [TestMethod]
    public void InvoicesJs_CreatesAndPays()
    {
        var js = File.ReadAllText(Path.Combine(Wwwroot, "js", "landlord", "invoices.js"));
        Assert.IsTrue(js.Contains("INVOICE_CREATE"), "invoices.js chưa lập được hóa đơn");
        Assert.IsTrue(js.Contains("INVOICE_PAY"), "invoices.js chưa thu được tiền");
        // BR-11: hóa đơn đã thu không được thao tác
        Assert.IsTrue(js.Contains("'Paid'"), "invoices.js thiếu chặn hóa đơn đã thu (BR-11)");
    }

    [TestMethod]
    public void ReportsJs_LoadsSummaryAndExports()
    {
        var js = File.ReadAllText(Path.Combine(Wwwroot, "js", "landlord", "reports.js"));
        Assert.IsTrue(js.Contains("REPORT_SUMMARY"), "reports.js chưa tải số liệu");
        Assert.IsTrue(js.Contains("EXPORT_RESIDENCE"), "reports.js chưa xuất được danh sách tạm trú");
    }
}
```

- [ ] **Step 2: Chạy test — xác nhận FAIL**

Run: `dotnet test QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj --filter "FullyQualifiedName~UtilsInvoicesReportsContractTests" -v m`
Expected: FAIL.

- [ ] **Step 3: Viết `landlord/utils.js`**

Yêu cầu: `loadUtils()` tải `ROOM_GET_ALL` đổ vào select phòng. Khi chọn phòng + tháng → `UTILITY_GET_PREVIOUS` với `{ roomId, billingMonth }`, điền chỉ số cũ vào ô `disabled`. Ô "mới" và "giá" cho nhập; hiển thị thành tiền trực tiếp = `(mới − cũ) × giá` cho cả điện và nước, cập nhật khi `oninput`. `saveUtility()` kiểm tra `newElectricity >= oldElectricity` và `newWater >= oldWater` trước khi gửi (BR-07), gửi `UTILITY_RECORD` với đủ 9 field của `UtilityReadingDto`, rồi `toast` thành công.

- [ ] **Step 4: Viết `landlord/invoices.js`**

Yêu cầu: `loadInvoices()` đọc tháng + phòng từ toolbar, gọi `INVOICE_GET_ALL` với `{ billingMonth, roomId? }`, render bảng 8 cột với nút Thu. Nút Thu `disabled` khi `status === 'Paid'` (BR-11). `payInvoice(id)` gọi `INVOICE_PAY` với `{ invoiceId }` sau `confirmDialog`. `createInvoice()` mở modal chọn phòng + tháng + phí khác → `INVOICE_CREATE` với `{ roomId, billingMonth, otherFees }`.

- [ ] **Step 5: Viết `landlord/reports.js`**

Yêu cầu: `loadReports()` gọi `REPORT_SUMMARY` với tháng đang chọn, đổ số liệu vào các thẻ KPI và vẽ biểu đồ cột `.chartbar` (mỗi tháng một cột, dùng `paidAmount`). `exportResidence()` gọi `EXPORT_RESIDENCE`, hiện `toast` với `filePath` và `rowCount` trả về.

- [ ] **Step: (đã gộp vào Task 8 Step 5)** — mọi thay đổi `index.html` nằm ở Task 8; task này chỉ ghi file `.js` của mình.


- [ ] **Step 7: Chạy test — xác nhận PASS**

Run: `dotnet test QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj --filter "FullyQualifiedName~UtilsInvoicesReportsContractTests" -v m`
Expected: PASS 3/3.

- [ ] **Step 8: Commit**

```bash
git add QuanLyTro/Assets/wwwroot/js/landlord QuanLyTro/Assets/wwwroot/index.html \
  QuanLyTro/QuanLyTro.Tests/UtilsInvoicesReportsContractTests.cs
git commit -m "feat(ui): wire utility entry, invoice billing and reports"
```

---

### Task 12: Shell Công an phường — hoàn thiện 3 tab

**Files:**
- Modify: `QuanLyTro/Assets/wwwroot/js/police.js`
- Test: `QuanLyTro/QuanLyTro.Tests/PoliceShellContractTests.cs` (mở rộng)

**Interfaces:**
- Consumes: `esc`, `fmtDateOnly`, `fmtDate`, `toast`
- Produces: `loadPoliceTab(tabKey)`, `renderCitizens(rows)`, `renderResidence(rows)`, `loadHistory()`, `exportHistory()`

- [ ] **Step 1: Mở rộng failing test**

Thêm vào `PoliceShellContractTests.cs`:

```csharp
    [TestMethod]
    public void PoliceJs_SearchInputAndDateRangeAreAddressableById()
    {
        var html = File.ReadAllText(Path.Combine(Wwwroot, "index.html"));
        Assert.IsTrue(html.Contains("id=\"p-search\""), "ô tìm kiếm công an cần id để JS đọc");
        Assert.IsTrue(html.Contains("id=\"p-from\""), "ô ngày bắt đầu cần id");
        Assert.IsTrue(html.Contains("id=\"p-to\""), "ô ngày kết thúc cần id");
    }
```

- [ ] **Step 2: Chạy test — xác nhận FAIL**

Run: `dotnet test QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj --filter "FullyQualifiedName~PoliceShellContractTests" -v m`
Expected: FAIL — index.html chưa có `p-search`/`p-from`/`p-to`.

- [ ] **Step: (đã gộp vào Task 8 Step 5)** — mọi thay đổi `index.html` nằm ở Task 8; task này chỉ ghi file `.js` của mình.


- [ ] **Step 4: Cập nhật `police.js`**

Đổi `historyRange()` sang đọc `#p-from` / `#p-to` theo id (thay vì theo vị trí), và `renderCitizens` dùng `#p-search` theo id. Giữ nguyên phần `esc()` đã có.

- [ ] **Step 5: Chạy test — xác nhận PASS**

Run: `dotnet test QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj --filter "FullyQualifiedName~PoliceShellContractTests" -v m`
Expected: PASS 3/3.

- [ ] **Step 6: Commit**

```bash
git add QuanLyTro/Assets/wwwroot/js/police.js QuanLyTro/Assets/wwwroot/index.html \
  QuanLyTro/QuanLyTro.Tests/PoliceShellContractTests.cs
git commit -m "feat(ui): point police search and date range at stable element ids"
```

---

### Task 13: Shell Khách thuê — hoàn thiện tab hóa đơn của tôi

**Files:**
- Modify: `QuanLyTro/Assets/wwwroot/js/tenant.js`
- Test: `QuanLyTro/QuanLyTro.Tests/TenantShellContractTests.cs` (mở rộng)

**Interfaces:**
- Consumes: `esc`, `fmtMoney`, `fmtDate`, `toast`
- Produces: `loadTenantInvoices()`, `renderTenantInvoices(rows)`

- [ ] **Step 1: Mở rộng failing test**

```csharp
    [TestMethod]
    public void TenantJs_RendersReceiptDetailAndHistoryFromServer()
    {
        var js = File.ReadAllText(Path.Combine(Wwwroot, "js", "tenant.js"));
        Assert.IsTrue(js.Contains("isPaid"), "tenant.js phải suy trạng thái đã thu");
        Assert.IsTrue(js.Contains("fmtDate"), "tenant.js phải định dạng ngày đóng tiền");
        // Chỉ đọc — không được có lệnh ghi nào
        Assert.IsFalse(js.Contains("INVOICE_PAY"), "shell khách thuê không được có thao tác ghi");
    }
```

- [ ] **Step 2: Chạy test — xác nhận FAIL**

Run: `dotnet test QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj --filter "FullyQualifiedName~TenantShellContractTests" -v m`
Expected: FAIL hoặc PASS tuỳ trạng thái hiện tại — nếu PASS sẵn, ghi lại là test đã đúng và bỏ qua bước sửa.

- [ ] **Step 3: Cập nhật `tenant.js`**

Yêu cầu: render cả bảng lịch sử thanh toán (dùng `fmtDate` cho `paidAt`), và cập nhật khối "Chi tiết quyết toán" phía trên bằng hóa đơn mới nhất chưa thanh toán (nếu có): tiền phòng, điện, nước, phí khác, tổng. Dùng `esc()` cho mọi chuỗi. Nút "Sao chép nội dung chuyển khoản" → `onclick="copyTransfer()` ghi vào clipboard mã hóa đơn + số tiền.

- [ ] **Step: (đã gộp vào Task 8 Step 5)** — mọi thay đổi `index.html` nằm ở Task 8; task này chỉ ghi file `.js` của mình.


- [ ] **Step 5: Chạy test — xác nhận PASS**

Run: `dotnet test QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj --filter "FullyQualifiedName~TenantShellContractTests" -v m`
Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add QuanLyTro/Assets/wwwroot/js/tenant.js QuanLyTro/Assets/wwwroot/index.html \
  QuanLyTro/QuanLyTro.Tests/TenantShellContractTests.cs
git commit -m "feat(ui): tenant receipt detail and payment history"
```

---

### Task 14: Kiểm tra toàn bộ — không nút chết

**Files:**
- Test: `QuanLyTro/QuanLyTro.Tests/NoDeadButtonsTests.cs`

- [ ] **Step 1: Viết test bắt mọi nút phải có handler**

```csharp
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuanLyTro.Tests;

[TestClass]
public sealed class NoDeadButtonsTests
{
    private static string Wwwroot =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "wwwroot");

    [TestMethod]
    public void EveryButton_EitherHasHandlerOrIsDisabled()
    {
        var html = File.ReadAllText(Path.Combine(Wwwroot, "index.html"));
        var dead = new List<string>();
        foreach (Match m in Regex.Matches(html, "<button\\b[^>]*>"))
        {
            var tag = m.Value;
            if (tag.Contains("onclick") || tag.Contains("type=\"submit\"") || tag.Contains("disabled"))
                continue;
            dead.Add(Regex.Replace(tag, "\\s+", " "));
        }
        Assert.AreEqual(0, dead.Count,
            "Nút không có handler cũng không disabled:\n" + string.Join("\n", dead));
    }
}
```

- [ ] **Step 2: Chạy test — xác nhận FAIL với danh sách nút chết**

Run: `dotnet test QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj --filter "FullyQualifiedName~NoDeadButtonsTests" -v m`
Expected: FAIL — liệt kê các nút còn thiếu handler.

- [ ] **Step 3: Gắn handler cho từng nút còn lại trong danh sách**

Mỗi nút phải gọi một hàm đã tồn tại. Nút chưa có chức năng tương ứng thì gắn `disabled` và thêm `title` giải thích.

- [ ] **Step 4: Chạy test — xác nhận PASS**

Run: `dotnet test QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj --filter "FullyQualifiedName~NoDeadButtonsTests" -v m`
Expected: PASS.

- [ ] **Step 5: Chạy toàn bộ test suite**

Run: `dotnet test QuanLyTro/QuanLyTro.Tests/QuanLyTro.Tests.csproj -v m`
Expected: PASS toàn bộ, không hồi quy.

- [ ] **Step 6: Commit**

```bash
git add QuanLyTro/Assets/wwwroot/index.html QuanLyTro/QuanLyTro.Tests/NoDeadButtonsTests.cs
git commit -m "test(ui): assert every button has a handler"
```

---

## Self-Review

1. **Spec coverage:** Mọi action trong `ActionNames` đều có màn hình dùng: phòng (Task 9), người thuê + hợp đồng (Task 10), điện nước + hóa đơn + thống kê (Task 11), lưu trú công an (Task 12), hóa đơn khách thuê (Task 13).
2. **Placeholder scan:** Task 9–13 mô tả yêu cầu hàm bằng văn xuôi thay vì code đầy đủ, vì khối lượng quá lớn để nhúng hết. Mỗi hàm đều nêu rõ action, payload, và hành vi — đủ để implementer viết mà không cần đoán.
3. **Type consistency:** Tên field camelCase lấy trực tiếp từ DTO đã đọc trong repo; tên action lấy từ `ActionNames.cs`.
4. **Review Focus:** Task 14 là lưới bắt nút chết — thứ dễ sót nhất khi làm nhiều tab.
