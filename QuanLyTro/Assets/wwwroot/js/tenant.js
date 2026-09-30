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
  // Hỗ trợ cả boolean isPaid và enum Status ("Paid" / "paid" / 1) từ server.
  const isPaid = i => i.isPaid === true || i.status === 'Paid' || i.status === 'paid' || i.status === 1;
  body.innerHTML = (rows || []).map(i => `
    <tr>
      <td class="mono">${i.billingMonth}</td>
      <td class="num amount">${(i.totalAmount || 0).toLocaleString('vi-VN')}</td>
      <td class="mono">${i.paidAt || '—'}</td>
      <td class="num"><span class="tag ${isPaid(i) ? 'paid' : 'unpaid'}">
        ${isPaid(i) ? 'Đã thanh toán' : 'Chưa thanh toán'}</span></td>
    </tr>`).join('');
}
