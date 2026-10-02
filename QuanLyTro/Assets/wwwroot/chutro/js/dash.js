// Tab Tổng quan: 4 thẻ KPI + bảng còn nợ + bảng HĐ sắp hết hạn.
async function loadDash() {
  const month = new Date().toISOString().slice(0, 7); // yyyy-MM
  try {
    const [summary, hoa_don, hop_dong] = await Promise.all([
      window.bridge.call('BAO_CAO_TONG_QUAN', { kyCuoc: month }),
      window.bridge.call('HOA_DON_LAY_TAT_CA', { kyCuoc: month }),
      window.bridge.call('HOP_DONG_LAY_TAT_CA', {})
    ]);

    if (summary) {
      setKpi('kpi-total-phong', summary.totalRooms);
      setKpi('kpi-available', summary.availableRooms);
      setKpi('kpi-khach_thue', summary.currentTenants);
      setKpi('kpi-unpaid', fmtMoney(summary.unpaidAmount));
      const el = document.getElementById('kpi-available-sub');
      if (el) el.textContent =
        `Thuê ${summary.rentedRooms ?? 0} · Trống ${summary.availableRooms ?? 0}`;
    }

    const overdue = (hoa_don || []).filter(i => i.trang_thai !== 'DaThu');
    const ob = document.getElementById('dash-overdue-rows');
    if (ob) ob.innerHTML = overdue.map(i => `
      <tr><td><b>${esc(i.soPhong ?? ('P' + i.phongId))}</b></td>
      <td class="mono">${esc(i.kyCuoc)}</td>
      <td class="num due-red">${fmtMoney(i.tongTien)}</td>
      <td>${esc(i.representativeName ?? '—')}</td></tr>`).join('')
      || '<tr><td colspan="4" style="color:var(--dim)">Không có phòng nào còn nợ.</td></tr>';

    const soon = (hop_dong || []).filter(c => {
      if (c.trang_thai !== 'HieuLuc') return false;
      const days = (new Date(c.ngayKetThuc) - new Date()) / 86400000;
      return days >= 0 && days <= 30;
    });
    const sb = document.getElementById('dash-expiring-rows');
    if (sb) sb.innerHTML = soon.map(c => {
      const days = Math.ceil((new Date(c.ngayKetThuc) - new Date()) / 86400000);
      return `<tr><td><b>${esc(c.soPhong ?? ('P' + c.phongId))}</b></td>
        <td>${esc(c.representativeName ?? '—')}</td>
        <td class="mono">${fmtDateOnly(c.ngayKetThuc)}</td>
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
