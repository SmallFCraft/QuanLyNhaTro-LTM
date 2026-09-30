// ponytail: tenant read-only shell — server derives room from token. Add pagination if history grows.
let currentTenantInvoices = [];

async function loadTenantInvoices() {
  try {
    const rows = await window.bridge.call('INVOICE_GET_MINE', {});
    currentTenantInvoices = rows || [];
    renderTenantInvoices(currentTenantInvoices);
  } catch (err) {
    if (typeof toast === 'function') {
      toast(err.message, 'err');
    } else {
      alert(err.message);
    }
  }
}

function renderTenantInvoices(rows) {
  const list = rows || [];
  const isPaid = i => i.isPaid === true || i.status === 'Paid' || i.status === 'paid' || i.status === 1;

  // Render lịch sử bảng thanh toán
  const body = document.getElementById('tenant-history-body') || document.querySelector('#tenant .tbl tbody');
  if (body) {
    body.innerHTML = list.map(i => `
      <tr>
        <td class="mono">${esc(i.billingMonth)}</td>
        <td class="num amount">${fmtMoney(i.totalAmount)}</td>
        <td class="mono">${isPaid(i) ? fmtDate(i.paidAt) : '—'}</td>
        <td class="num"><span class="tag ${isPaid(i) ? 'paid' : 'unpaid'}">
          ${isPaid(i) ? 'Đã thanh toán' : 'Chưa thanh toán'}</span></td>
      </tr>`).join('') || `<tr><td colspan="4">Không có hóa đơn nào</td></tr>`;
  }

  // Cập nhật khối "Chi tiết quyết toán" bằng hóa đơn mới nhất chưa thanh toán (hoặc hóa đơn mới nhất)
  const unpaid = list.find(i => !isPaid(i)) || list[0];
  if (unpaid) {
    const setVal = (id, val) => {
      const el = document.getElementById(id);
      if (el) el.textContent = val;
    };
    setVal('rc-room', fmtMoney(unpaid.roomAmount) + ' đ');
    setVal('rc-elec', fmtMoney(unpaid.electricityAmount) + ' đ');
    setVal('rc-water', fmtMoney(unpaid.waterAmount) + ' đ');
    setVal('rc-other', fmtMoney(unpaid.otherFees) + ' đ');
    setVal('rc-total', fmtMoney(unpaid.totalAmount));
  }
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
  const name = new URLSearchParams(window.location.search).get('u');
  if (name) document.getElementById('tname').textContent = name;
  loadTenantInvoices();
});
