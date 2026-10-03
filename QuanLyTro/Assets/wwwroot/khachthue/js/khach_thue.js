// ponytail: shell khách thuê — server tự suy ra phòng từ token. Phân trang phía server (page/soLuongMoiTrang).
let currentTenantInvoices = [];
let tenantHistoryPage = 1;
let tenantHistoryPageSize = 10;
let tenantHistoryTotal = 0;

async function loadTenantInvoices() {
  try {
    await loadTenantHistoryPage(tenantHistoryPage, tenantHistoryPageSize);
  } catch (err) {
    toast(err.message, 'err');
    renderTenantEmpty();
  }
  await refreshTenantSummary();
}

async function loadTenantHistoryPage(page, soLuongMoiTrang) {
  const res = await window.bridge.call('HOA_DON_CUA_TOI', { page, soLuongMoiTrang });
  const dto = normalizeHistoryDto(res);
  currentTenantInvoices = (dto && dto.items) || [];
  tenantHistoryPage = (dto && dto.page) || 1;
  tenantHistoryPageSize = (dto && dto.soLuongMoiTrang) || soLuongMoiTrang;
  tenantHistoryTotal = (dto && dto.tongSo) || currentTenantInvoices.length;
  renderTenantHistory(currentTenantInvoices);
  renderTenantPagination();
}

async function changeTenantPage(delta) {
  const tongSoTrang = Math.max(1, Math.ceil(tenantHistoryTotal / tenantHistoryPageSize));
  const next = tenantHistoryPage + delta;
  if (next < 1 || next > tongSoTrang) return;
  try {
    await loadTenantHistoryPage(next, tenantHistoryPageSize);
  } catch (err) {
    if (typeof toast === 'function') toast(err.message, 'err');
  }
}

function normalizeHistoryDto(res) {
  if (!res) return null;
  if (Array.isArray(res)) {
    return { items: res, tongSo: res.length, page: 1, soLuongMoiTrang: res.length || 10 };
  }
  const list = res.danhSach || res.items || res.DanhSach || res.Items;
  if (list) {
    const items = list || [];
    const i = n => (n === undefined || n === null) ? undefined : Number(n);
    return {
      items,
      tongSo: i(res.tongSo ?? res.TongSo ?? res.totalCount ?? res.TotalCount) ?? items.length,
      page: i(res.trang ?? res.Trang ?? res.page ?? res.Page) ?? 1,
      soLuongMoiTrang: i(res.soLuongMoiTrang ?? res.SoLuongMoiTrang ?? res.pageSize ?? res.PageSize) ?? 10
    };
  }
  return null;
}

function isPaidInvoice(i) {
  const st = i.trangThai ?? i.trang_thai;
  return i.isPaid === true || st === 'DaThu' || st === 'paid' || st === 1;
}

function renderTenantHistory(rows) {
  const list = rows || [];
  const body = document.getElementById('tenant-history-body');
  if (body) {
    body.innerHTML = list.map(i => `
      <tr class="clickable-row" onclick="showInvoiceDetail(${Number(i.id)})" title="Bấm để xem chi tiết kỳ ${esc(i.kyCuoc)}">
        <td class="mono">${esc(i.kyCuoc)}</td>
        <td class="num amount">${fmtMoney(i.tongTien)}</td>
        <td class="mono">${isPaidInvoice(i) ? fmtDate(i.ngayDong) : '—'}</td>
        <td class="num"><span class="tag ${isPaidInvoice(i) ? 'paid' : 'unpaid'}">
          ${isPaidInvoice(i) ? 'Đã thanh toán' : 'Chưa thanh toán'}</span></td>
      </tr>`).join('') || `<tr><td colspan="4" style="text-align:center;color:var(--dim)">Không có hóa đơn nào</td></tr>`;
  }
  const info = document.getElementById('tenant-page-info');
  if (info) info.textContent = `Tổng: ${tenantHistoryTotal} bản ghi`;
}

function renderTenantPagination() {
  const tongSoTrang = Math.max(1, Math.ceil(tenantHistoryTotal / tenantHistoryPageSize));
  const indicator = document.getElementById('tenant-page-indicator');
  if (indicator) indicator.textContent = `Trang ${tenantHistoryPage} / ${tongSoTrang}`;
  const prev = document.getElementById('btn-prev-page');
  if (prev) prev.disabled = tenantHistoryPage <= 1;
  const next = document.getElementById('btn-next-page');
  if (next) next.disabled = tenantHistoryPage >= tongSoTrang;
}

function renderTenantEmpty() {
  const body = document.getElementById('tenant-history-body');
  if (body) body.innerHTML = `<tr><td colspan="4" style="text-align:center;color:var(--dim)">Không tải được dữ liệu</td></tr>`;
  tenantHistoryTotal = 0;
  renderTenantPagination();
}

// Tóm tắt Tổng quan + chi tiết Kỳ này: đọc hóa đơn MỚI NHẤT (một request riêng page=1, soLuongMoiTrang=1
// theo ky_cuoc DESC) thay vì phụ thuộc trang lịch sử.
async function refreshTenantSummary() {
  try {
    const res = await window.bridge.call('HOA_DON_CUA_TOI', { page: 1, soLuongMoiTrang: 1 });
    const dto = normalizeHistoryDto(res);
    const latest = (dto && dto.items && dto.items[0]) || currentTenantInvoices[0];
    if (latest) renderTenantSummary(latest);
  } catch (err) {
    const fallback = currentTenantInvoices[0];
    if (fallback) renderTenantSummary(fallback);
  }
}

function renderTenantSummary(unpaid) {
  const setVal = (id, val) => {
    const el = document.getElementById(id);
    if (el) el.textContent = val;
  };

  // Khối Kỳ này
  setVal('rc-room', fmtMoney(unpaid.tienPhong) + ' đ');
  setVal('rc-elec', fmtMoney(unpaid.tienDien) + ' đ');
  setVal('rc-water', fmtMoney(unpaid.tienNuoc) + ' đ');
  setVal('rc-other', fmtMoney(unpaid.phiKhac) + ' đ');
  setVal('rc-total', fmtMoney(unpaid.tongTien));
  const paid = isPaidInvoice(unpaid);
  setVal('rc-code', `MÃ SỐ BẢNG KÊ: HD-${String(unpaid.kyCuoc || '').replace('-', '')}-${unpaid.phongId || unpaid.id}`);
  setVal('rc-title', `Chi tiết quyết toán cước tháng ${String(unpaid.kyCuoc || '').replace('-', '/')}`);
  setTag('rc-trang_thai-tag', paid, 'Đã thanh toán', 'Chưa thanh toán');
  setTag('rc-due-tag', !paid, 'Chưa thu', 'Đã thu');

  // Khối Tổng quan
  setVal('ov-month', String(unpaid.kyCuoc || '').replace('-', '/'));
  setVal('ov-total', fmtMoney(unpaid.tongTien) + ' đ');
  setVal('ov-trang_thai', paid ? 'Đã thanh toán' : 'Chưa nộp');
  setVal('troom-badge', `P.${unpaid.phongId ?? ''}`);
  setVal('tcontract-due', paid ? 'Hợp đồng: Đang hiệu lực' : 'Hợp đồng: Còn cước chưa nộp');
  setVal('due-alert-text', paid
    ? `Kỳ cước ${unpaid.kyCuoc} đã được thanh toán. Cảm ơn bạn!`
    : `Kỳ cước ${unpaid.kyCuoc} chưa hoàn tất thanh toán. Tổng cần nộp: ${fmtMoney(unpaid.tongTien)} đ.`);
  setTag('due-alert-tag', paid, 'Đã nộp', 'Chưa nộp');
}

function setTag(id, ok, okText, badText) {
  const el = document.getElementById(id);
  if (!el) return;
  el.textContent = ok ? okText : badText;
  el.className = `tag ${ok ? 'paid' : 'unpaid'}`;
}

function showInvoiceDetail(id) {
  const target = (currentTenantInvoices || []).find(i => Number(i.id) === Number(id));
  if (!target) return;
  const paid = isPaidInvoice(target);
  if (typeof openModal === 'function') {
    openModal({
      title: `Chi tiết kỳ cước ${target.kyCuoc}`,
      fields: [
        { name: 'room', label: 'Tiền phòng', type: 'text', value: fmtMoney(target.tienPhong) + ' đ', disabled: true },
        { name: 'elec', label: 'Tiền điện', type: 'text', value: fmtMoney(target.tienDien) + ' đ', disabled: true },
        { name: 'water', label: 'Tiền nước', type: 'text', value: fmtMoney(target.tienNuoc) + ' đ', disabled: true },
        { name: 'other', label: 'Dịch vụ khác', type: 'text', value: fmtMoney(target.phiKhac) + ' đ', disabled: true },
        { name: 'total', label: 'Tổng cộng', type: 'text', value: fmtMoney(target.tongTien) + ' đ', disabled: true },
        { name: 'trang_thai', label: 'Trạng thái', type: 'text', value: paid ? `Đã thanh toán (${fmtDate(target.ngayDong)})` : 'Chưa thanh toán', disabled: true }
      ],
      onSubmit: async () => { /* chỉ để xem */ }
    });
  }
}

function switchTenantTab(name) {
  document.querySelectorAll('#tenantTabs .tab').forEach(t => {
    t.classList.toggle('on', t.dataset.tab === name);
  });
  document.querySelectorAll('.tab-pane').forEach(p => {
    p.classList.toggle('active', p.id === `tab-${name}`);
  });
}

async function copyTransfer() {
  const isPaid = i => i.isPaid === true || (i.trangThai ?? i.trang_thai) === 'DaThu' || (i.trangThai ?? i.trang_thai) === 'paid' || (i.trangThai ?? i.trang_thai) === 1;
  const target = (currentTenantInvoices || []).find(i => !isPaid(i)) || (currentTenantInvoices || [])[0];
  if (!target) {
    if (typeof toast === 'function') toast('Không có hóa đơn để sao chép', 'warn');
    return;
  }
  const text = `HD-${String(target.kyCuoc || '').replace('-', '')}-${target.phongId || target.id} ${fmtMoney(target.tongTien)} đ`;
  try {
    if (navigator.clipboard && navigator.clipboard.writeText) {
      await navigator.clipboard.writeText(text);
      if (typeof toast === 'function') toast(`Đã sao chép: ${text}`, 'ok');
    } else {
      if (typeof toast === 'function') toast(`Nội dung chuyển khoản: ${text}`, 'info');
    }
  } catch (err) {
    if (typeof toast === 'function') toast(`Nội dung: ${text}`, 'info');
  }
}

async function logout() {
  await window.bridge.call('UI_LOGOUT', {});
  window.location.href = '../auth/index.html';
}

document.addEventListener('DOMContentLoaded', () => {
  // auth.js điều hướng sang đây kèm ?u=<tên đã encode>.
  const params = new URLSearchParams(window.location.search);
  const name = params.get('u');
  if (name) {
    const card = document.getElementById('tname-card');
    if (card) card.textContent = name;
    const legacy = document.getElementById('tname');
    if (legacy) legacy.textContent = name;
  }
  switchTenantTab(params.get('tab') === 'history' ? 'history' : 'overview');
  loadTenantInvoices();
  // ponytail: tự động làm mới toàn trang mỗi 30s + khi quay lại cửa sổ — thay nút sync tay.
  startAutoRefresh(loadTenantInvoices, 30000);
});
