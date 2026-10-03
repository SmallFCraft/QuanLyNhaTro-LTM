// Tab Tổng quan: 4 thẻ KPI + bảng còn nợ + bảng HĐ sắp hết hạn.
async function loadDash() {
  const month = new Date().toISOString().slice(0, 7); // yyyy-MM
  try {
    const [summary, hoaDon, hopDong] = await Promise.all([
      window.bridge.call('BAO_CAO_TONG_QUAN', { kyCuoc: month }),
      window.bridge.call('HOA_DON_LAY_TAT_CA', { kyCuoc: month }),
      window.bridge.call('HOP_DONG_LAY_TAT_CA', {})
    ]);

    if (summary) {
      const totalRooms = summary.tongSoPhong ?? summary.totalRooms;
      const availableRooms = summary.phongTrong ?? summary.availableRooms;
      const currentTenants = summary.khachHienTai ?? summary.currentTenants;
      const unpaidAmount = summary.soTienChuaThu ?? summary.unpaidAmount;
      const rentedRooms = summary.phongDaThue ?? summary.rentedRooms ?? 0;

      setKpi('kpi-total-phong', totalRooms);
      setKpi('kpi-available', availableRooms);
      setKpi('kpi-khach_thue', currentTenants);
      setKpi('kpi-unpaid', fmtMoney(unpaidAmount));
      const el = document.getElementById('kpi-available-sub');
      if (el) el.textContent = `Thuê ${rentedRooms} · Trống ${availableRooms ?? 0}`;
    }

    const overdue = (hoaDon || []).filter(i => (i.trangThai ?? i.trang_thai) !== 'DaThu');
    const ob = document.getElementById('dash-overdue-rows');
    if (ob) ob.innerHTML = overdue.map(i => {
      const room = i.soPhong ?? ('P' + i.phongId);
      const rep = i.tenNguoiDaiDien ?? i.representativeName ?? '—';
      const tong = i.tongTien ?? i.totalAmount ?? 0;
      return `
        <tr><td><b>${esc(room)}</b></td>
        <td class="mono">${esc(i.kyCuoc)}</td>
        <td class="num due-red">${fmtMoney(tong)}</td>
        <td>${esc(rep)}</td></tr>`;
    }).join('') || '<tr><td colspan="4" style="color:var(--dim)">Không có phòng nào còn nợ.</td></tr>';

    const soon = (hopDong || []).map(item => {
      const inner = item.contract || item.hopDong || item.HopDong || item;
      return {
        ...inner,
        soPhong: item.soPhong ?? item.roomNumber ?? inner.soPhong,
        representativeName: item.representativeName ?? item.tenNguoiDaiDien ?? inner.representativeName ?? inner.tenNguoiDaiDien ?? '—',
        trangThai: inner.trangThai ?? inner.trang_thai,
        ngayKetThuc: inner.ngayKetThuc ?? inner.endDate
      };
    }).filter(c => {
      const st = c.trangThai;
      if (st !== 'HieuLuc') return false;
      const end = c.ngayKetThuc;
      const days = (new Date(end) - new Date()) / 86400000;
      return days >= 0 && days <= 30;
    });
    const sb = document.getElementById('dash-expiring-rows');
    if (sb) sb.innerHTML = soon.map(c => {
      const end = c.ngayKetThuc ?? c.endDate;
      const days = Math.ceil((new Date(end) - new Date()) / 86400000);
      const room = c.soPhong ?? ('P' + c.phongId);
      const rep = c.tenNguoiDaiDien ?? c.representativeName ?? '—';
      return `<tr><td><b>${esc(room)}</b></td>
        <td>${esc(rep)}</td>
        <td class="mono">${fmtDateOnly(end)}</td>
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
