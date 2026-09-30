// Tab Công dân (PM-02/03): tra cứu theo CCCD / họ tên / SĐT, lọc client-side trên cache.
const searchBox = () => document.getElementById('p-search');
const searchKey = () => (searchBox()?.value || '').toLowerCase().trim();

function filterCitizens(rows) {
  const q = searchKey();
  if (!q) return rows || [];
  return (rows || []).filter(t =>
    (t.fullName || '').toLowerCase().includes(q) ||
    (t.idCard || '').toLowerCase().includes(q) ||
    (t.phone || '').includes(q));
}

function renderCitizens(rows) {
  const body = document.querySelector('#ptab-citizens tbody');
  if (!body) return;
  body.innerHTML = filterCitizens(rows).map(t => `
    <tr>
      <td><b>${esc(t.fullName)}</b></td>
      <td class="mono">${fmtDateOnly(t.dateOfBirth)}</td>
      <td class="mono">${esc(t.idCard)}</td>
      <td>${esc(t.hometown)}</td>
      <td><b>${esc(t.roomNumber)}</b></td>
      <td class="num"><span class="tag ${t.isTemporaryRegistered ? 'done' : 'warn'}">
        ${t.isTemporaryRegistered ? 'Đã đăng ký' : 'Chưa đăng ký'}</span></td>
    </tr>`).join('') || `<tr class="empty"><td colspan="6">Không có công dân nào</td></tr>`;
}

async function exportCitizens() {
  const list = filterCitizens(policeCache);

  if (list.length === 0) {
    toast('Không có bản ghi nào để xuất.', 'info');
    return;
  }

  const header = ['Họ tên', 'Ngày sinh', 'CCCD', 'Quê quán', 'Số phòng', 'Trạng thái'];
  const rows = list.map(t => [
    t.fullName || '',
    fmtDateOnly(t.dateOfBirth),
    t.idCard || '',
    t.hometown || '',
    t.roomNumber || '',
    t.isTemporaryRegistered ? 'Đã đăng ký' : 'Chưa đăng ký'
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
