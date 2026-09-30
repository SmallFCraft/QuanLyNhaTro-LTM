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
