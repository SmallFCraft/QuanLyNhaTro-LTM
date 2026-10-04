// Tab Công dân (PM-02/03): tra cứu theo CCCD / họ tên / SĐT, lọc client-side trên cache.
const searchBox = () => document.getElementById('p-search');
const searchKey = () => (searchBox()?.value || '').toLowerCase().trim();

function filterCitizens(rows) {
  const q = searchKey();
  const room = document.getElementById('p-room-filter')?.value || '';
  const status = document.getElementById('p-status-filter')?.value || '';
  return (rows || []).filter(t => {
    if (room && String(t.soPhong ?? '') !== room) return false;
    if (status === 'registered' && !t.daDangKyTamTru) return false;
    if (status === 'unregistered' && t.daDangKyTamTru) return false;
    if (!q) return true;
    return (t.hoTen || '').toLowerCase().includes(q) ||
      (t.cccd || '').toLowerCase().includes(q) ||
      ((t.soDienThoai ?? t.so_dien_thoai) || '').includes(q);
  });
}

function renderCitizens(rows) {
  const body = document.querySelector('#ptab-citizens tbody');
  if (!body) return;
  const list = filterCitizens(rows);
  body.innerHTML = list.map(t => `
    <tr>
      <td><b>${esc(t.hoTen)}</b></td>
      <td class="mono">${fmtDateOnly(t.ngaySinh)}</td>
      <td class="mono">${esc(t.cccd)}</td>
      <td>${esc(t.queQuan ?? t.que_quan)}</td>
      <td><b>${esc(t.soPhong)}</b></td>
      <td class="num"><span class="tag ${t.daDangKyTamTru ? 'done' : 'warn'}">
        ${t.daDangKyTamTru ? 'Đã đăng ký' : 'Chưa đăng ký'}</span></td>
    </tr>`).join('') || `<tr class="empty"><td colspan="6">Không có công dân nào</td></tr>`;

  const foot = document.querySelector('#ptab-citizens .tblfoot span');
  if (foot) foot.textContent = `Tổng: ${list.length} bản ghi`;
}

async function exportCitizens() {
  const list = filterCitizens(policeCache);

  if (list.length === 0) {
    toast('Không có bản ghi nào để xuất.', 'info');
    return;
  }

  const header = ['Họ tên', 'Ngày sinh', 'CCCD', 'Quê quán', 'Số phòng', 'Trạng thái'];
  const rows = list.map(t => [
    t.hoTen || '',
    fmtDateOnly(t.ngaySinh),
    t.cccd || '',
    t.queQuan ?? t.que_quan ?? '',
    t.soPhong || '',
    t.daDangKyTamTru ? 'Đã đăng ký' : 'Chưa đăng ký'
  ]);

  const csv = '﻿' + [header, ...rows]
    .map(r => r.map(c => `"${String(c).replace(/"/g, '""')}"`).join(','))
    .join('\r\n');

  const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
  const a = document.createElement('a');
  a.href = URL.createObjectURL(blob);
  a.download = `DanhSachCongDan_${new Date().toISOString().slice(0, 10)}.csv`;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(a.href);
  toast(`Đã xuất ${list.length} công dân ra file CSV`, 'ok');
}

// Gắn lắng nghe tìm kiếm công dân khi DOM sẵn sàng.
document.addEventListener('DOMContentLoaded', () => {
  const search = searchBox();
  if (search) search.addEventListener('input', () => renderCitizens(policeCache));
});
