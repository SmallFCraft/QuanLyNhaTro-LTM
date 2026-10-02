// ponytail: tenant read-only shell — server derives room from token. Pagination is server-side (page/pageSize).
let currentTenantInvoices = [];
let tenantHistoryPage = 1;
let tenantHistoryPageSize = 10;
let tenantHistoryTotal = 0;

async function loadTenantInvoices() {
  try {
    await loadTenantHistoryPage(tenantHistoryPage, tenantHistoryPageSize);
  } catch (err) {
    if (typeof toast === 'function') {
      toast(err.message, 'err');
    } else {
      alert(err.message);
    }
    renderTenantEmpty();
  }
  await refreshTenantSummary();
}

async function loadTenantHistoryPage(page, pageSize) {
  const res = await window.bridge.call('INVOICE_GET_MINE', { page, pageSize });
  const dto = normalizeHistoryDto(res);
  currentTenantInvoices = (dto && dto.items) || [];
  tenantHistoryPage = (dto && dto.page) || 1;
  tenantHistoryPageSize = (dto && dto.pageSize) || pageSize;
  tenantHistoryTotal = (dto && dto.totalCount) || currentTenantInvoices.length;
  renderTenantHistory(currentTenantInvoices);
  renderTenantPagination();
}

async function changeTenantPage(delta) {
  const totalPages = Math.max(1, Math.ceil(tenantHistoryTotal / tenantHistoryPageSize));
  const next = tenantHistoryPage + delta;
  if (next < 1 || next > totalPages) return;
  try {
    await loadTenantHistoryPage(next, tenantHistoryPageSize);
  } catch (err) {
    if (typeof toast === 'function') toast(err.message, 'err');
  }
}

function normalizeHistoryDto(res) {
  if (!res) return null;
  if (Array.isArray(res)) {
    return { items: res, totalCount: res.length, page: 1, pageSize: res.length || 10 };
  }
  if (res.items || res.Items) {
    const items = res.items || res.Items || [];
    const i = n => (n === undefined || n === null) ? undefined : Number(n);
    return {
      items,
      totalCount: i(res.totalCount ?? res.TotalCount) ?? items.length,
      page: i(res.page ?? res.Page) ?? 1,
      pageSize: i(res.pageSize ?? res.PageSize) ?? 10
    };
  }
  return null;
}

function isPaidInvoice(i) {
  return i.isPaid === true || i.status === 'Paid' || i.status === 'paid' || i.status === 1;
}

function renderTenantHistory(rows) {
  const list = rows || [];
  const body = document.getElementById('tenant-history-body');
  if (body) {
    body.innerHTML = list.map(i => `
      <tr class="clickable-row" onclick="showInvoiceDetail(${Number(i.id)})" title="Bấm để xem chi tiết kỳ ${esc(i.billingMonth)}">
        <td class="mono">${esc(i.billingMonth)}</td>
        <td class="num amount">${fmtMoney(i.totalAmount)}</td>
        <td class="mono">${isPaidInvoice(i) ? fmtDate(i.paidAt) : '—'}</td>
        <td class="num"><span class="tag ${isPaidInvoice(i) ? 'paid' : 'unpaid'}">
          ${isPaidInvoice(i) ? 'Đã thanh toán' : 'Chưa thanh toán'}</span></td>
      </tr>`).join('') || `<tr><td colspan="4" style="text-align:center;color:var(--dim)">Không có hóa đơn nào</td></tr>`;
  }
  const info = document.getElementById('tenant-page-info');
  if (info) info.textContent = `Tổng: ${tenantHistoryTotal} bản ghi`;
}

function renderTenantPagination() {
  const totalPages = Math.max(1, Math.ceil(tenantHistoryTotal / tenantHistoryPageSize));
  const indicator = document.getElementById('tenant-page-indicator');
  if (indicator) indicator.textContent = `Trang ${tenantHistoryPage} / ${totalPages}`;
  const prev = document.getElementById('btn-prev-page');
  if (prev) prev.disabled = tenantHistoryPage <= 1;
  const next = document.getElementById('btn-next-page');
  if (next) next.disabled = tenantHistoryPage >= totalPages;
}

function renderTenantEmpty() {
  const body = document.getElementById('tenant-history-body');
  if (body) body.innerHTML = `<tr><td colspan="4" style="text-align:center;color:var(--dim)">Không tải được dữ liệu</td></tr>`;
  tenantHistoryTotal = 0;
  renderTenantPagination();
}

// Tóm tắt Tổng quan + chi tiết Kỳ này: đọc hóa đơn MỚI NHẤT (một request riêng page=1, pageSize=1
// theo billing_month DESC) thay vì phụ thuộc trang lịch sử.
async function refreshTenantSummary() {
  try {
    const res = await window.bridge.call('INVOICE_GET_MINE', { page: 1, pageSize: 1 });
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
  setVal('rc-room', fmtMoney(unpaid.roomAmount) + ' đ');
  setVal('rc-elec', fmtMoney(unpaid.electricityAmount) + ' đ');
  setVal('rc-water', fmtMoney(unpaid.waterAmount) + ' đ');
  setVal('rc-other', fmtMoney(unpaid.otherFees) + ' đ');
  setVal('rc-total', fmtMoney(unpaid.totalAmount));
  const paid = isPaidInvoice(unpaid);
  setVal('rc-code', `MÃ SỐ BẢNG KÊ: HD-${String(unpaid.billingMonth || '').replace('-', '')}-${unpaid.roomId || unpaid.id}`);
  setVal('rc-title', `Chi tiết quyết toán cước tháng ${String(unpaid.billingMonth || '').replace('-', '/')}`);
  setTag('rc-status-tag', paid, 'Đã thanh toán', 'Chưa thanh toán');
  setTag('rc-due-tag', !paid, 'Chưa thu', 'Đã thu');

  // Khối Tổng quan
  setVal('ov-month', String(unpaid.billingMonth || '').replace('-', '/'));
  setVal('ov-total', fmtMoney(unpaid.totalAmount) + ' đ');
  setVal('ov-status', paid ? 'Đã thanh toán' : 'Chưa nộp');
  setVal('troom-badge', `P.${unpaid.roomId ?? ''}`);
  setVal('tcontract-due', paid ? 'Hợp đồng: Đang hiệu lực' : 'Hợp đồng: Còn cước chưa nộp');
  setVal('due-alert-text', paid
    ? `Kỳ cước ${unpaid.billingMonth} đã được thanh toán. Cảm ơn bạn!`
    : `Kỳ cước ${unpaid.billingMonth} chưa hoàn tất thanh toán. Tổng cần nộp: ${fmtMoney(unpaid.totalAmount)} đ.`);
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
      title: `Chi tiết kỳ cước ${target.billingMonth}`,
      fields: [
        { name: 'room', label: 'Tiền phòng', type: 'text', value: fmtMoney(target.roomAmount) + ' đ', disabled: true },
        { name: 'elec', label: 'Tiền điện', type: 'text', value: fmtMoney(target.electricityAmount) + ' đ', disabled: true },
        { name: 'water', label: 'Tiền nước', type: 'text', value: fmtMoney(target.waterAmount) + ' đ', disabled: true },
        { name: 'other', label: 'Dịch vụ khác', type: 'text', value: fmtMoney(target.otherFees) + ' đ', disabled: true },
        { name: 'total', label: 'Tổng cộng', type: 'text', value: fmtMoney(target.totalAmount) + ' đ', disabled: true },
        { name: 'status', label: 'Trạng thái', type: 'text', value: paid ? `Đã thanh toán (${fmtDate(target.paidAt)})` : 'Chưa thanh toán', disabled: true }
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
  const isPaid = i => i.isPaid === true || i.status === 'Paid' || i.status === 'paid' || i.status === 1;
  const target = (currentTenantInvoices || []).find(i => !isPaid(i)) || (currentTenantInvoices || [])[0];
  if (!target) {
    if (typeof toast === 'function') toast('Không có hóa đơn để sao chép', 'warn');
    return;
  }
  const text = `HD-${String(target.billingMonth || '').replace('-', '')}-${target.roomId || target.id} ${fmtMoney(target.totalAmount)} đ`;
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
