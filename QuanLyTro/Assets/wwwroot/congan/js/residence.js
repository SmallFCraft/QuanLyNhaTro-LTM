// Tab Tạm trú (PM-04): mặc định chỉ người CHƯA đăng ký, đổi được qua dropdown trạng thái.
function residenceRows(rows) {
  const room = document.getElementById('p-res-room-filter')?.value || '';
  const all = document.getElementById('p-res-status-filter')?.value === 'all';
  return (rows || []).filter(t =>
    (all || !t.daDangKyTamTru) &&
    (!room || String(t.soPhong ?? '') === room));
}

function renderResidence(rows) {
  const body = document.querySelector('#ptab-residence tbody');
  if (!body) return;
  const pending = residenceRows(rows);
  body.innerHTML = pending.map(t => `
    <tr>
      <td><b>${esc(t.hoTen)}</b></td>
      <td class="mono">${fmtDateOnly(t.ngaySinh)}</td>
      <td class="mono">${esc(t.cccd)}</td>
      <td>${esc(t.queQuan ?? t.que_quan)}</td>
      <td><b>${esc(t.soPhong)}</b></td>
      <td class="mono">—</td>
      <td><span class="tag ${t.daDangKyTamTru ? 'done' : 'warn'}">${t.daDangKyTamTru ? 'Đã đăng ký' : 'Chưa đăng ký'}</span></td>
    </tr>`).join('') || `<tr class="empty"><td colspan="7">Không có bản ghi nào</td></tr>`;

  const foot = document.querySelector('#ptab-residence .tblfoot span');
  if (foot) foot.textContent = `Tổng: ${pending.length} bản ghi`;
}

async function exportResidence() {
  const pending = residenceRows(policeCache);
  if (pending.length === 0) {
    toast('Không có bản ghi chưa đăng ký tạm trú nào để xuất.', 'info');
    return;
  }
  const header = ['Họ tên', 'Ngày sinh', 'CCCD', 'Quê quán', 'Số phòng', 'Trạng thái'];
  const rows = pending.map(t => [
    t.hoTen || '',
    fmtDateOnly(t.ngaySinh),
    t.cccd || '',
    t.queQuan ?? t.que_quan ?? '',
    t.soPhong || '',
    'Chưa đăng ký'
  ]);
  const csv = '﻿' + [header, ...rows]
    .map(r => r.map(c => `"${String(c).replace(/"/g, '""')}"`).join(','))
    .join('\r\n');
  const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
  const a = document.createElement('a');
  a.href = URL.createObjectURL(blob);
  a.download = `ChuaDangKyTamTru_${new Date().toISOString().slice(0, 10)}.csv`;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(a.href);
  toast(`Đã xuất ${pending.length} công dân chưa đăng ký ra file CSV`, 'ok');
}
