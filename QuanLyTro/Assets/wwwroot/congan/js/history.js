// Tab Biến động (PM-05): lịch sử theo khoảng ngày #p-from → #p-to + xuất file.
function historyRange() {
  return {
    from: document.getElementById('p-from')?.value || '',
    to: document.getElementById('p-to')?.value || ''
  };
}

async function loadHistory() {
  const { from, to } = historyRange();
  const rows = await window.bridge.call('LICH_SU_CU_TRU_LAY', { from, to });
  const body = document.querySelector('#ptab-history tbody');
  if (!body) return;
  body.innerHTML = (rows || []).map(h => {
    const loai = h.loaiBienDong ?? h.eventType ?? 'Vào';
    const ngay = h.ngayBienDong ?? h.eventDate;
    const ghiChu = h.ghiChu ?? h.ghi_chu ?? '—';
    return `
      <tr>
        <td><b>${esc(h.hoTen)}</b></td>
        <td><b>${esc(h.soPhong)}</b></td>
        <td><span class="tag rent">${esc(loai)}</span></td>
        <td class="mono">${fmtDateOnly(ngay)}</td>
        <td class="mono">—</td>
        <td>${esc(ghiChu)}</td>
      </tr>`;
  }).join('') || `<tr class="empty"><td colspan="6">Không có biến động nào</td></tr>`;

  const foot = document.querySelector('#ptab-history .tblfoot span');
  if (foot) foot.textContent = `Tổng: ${(rows || []).length} bản ghi`;
}

async function exportHistory() {
  const { from, to } = historyRange();
  const payload = {
    tuNgay: from || null,
    denNgay: to || null,
    dinhDang: 'CSV',
    soPhong: null,
    loaiBienDong: null
  };
  try {
    const res = await window.bridge.call('XUAT_LICH_SU_CU_TRU', payload);
    const count = res.soDong ?? res.rowCount ?? 0;
    const path = res.duongDanFile ?? res.filePath ?? '';
    toast(`Đã xuất ${count} bản ghi tới: ${path}`, 'ok');
  } catch (err) {
    alertDialog(err.message, 'Xuất thất bại');
  }
}
