// Tab Biến động (PM-05): lịch sử theo khoảng ngày #p-from → #p-to + xuất file.
function historyRange() {
  return {
    from: document.getElementById('p-from')?.value || '',
    to: document.getElementById('p-to')?.value || ''
  };
}

async function loadHistory() {
  const { from, to } = historyRange();
  const rows = await window.bridge.call('RESIDENCE_HISTORY_GET', { from, to });
  const body = document.querySelector('#ptab-history tbody');
  if (!body) return;
  body.innerHTML = (rows || []).map(h => `
    <tr>
      <td><b>${esc(h.fullName)}</b></td>
      <td><b>${esc(h.roomNumber)}</b></td>
      <td><span class="tag rent">${esc(h.eventType)}</span></td>
      <td class="mono">${fmtDateOnly(h.eventDate)}</td>
      <td class="mono">—</td>
      <td>${esc(h.notes) || '—'}</td>
    </tr>`).join('') || `<tr class="empty"><td colspan="6">Không có biến động nào</td></tr>`;

  const foot = document.querySelector('#ptab-history .tblfoot span');
  if (foot) foot.textContent = `Tổng: ${(rows || []).length} bản ghi`;
}

async function exportHistory() {
  const { from, to } = historyRange();
  const payload = {
    fromDate: from,
    toDate: to,
    format: 'CSV',
    roomNumber: null,
    eventType: null
  };
  try {
    const res = await window.bridge.call('EXPORT_RESIDENCE_HISTORY', payload);
    alert(`Đã xuất ${res.rowCount} bản ghi tới:\n${res.filePath}`);
  } catch (err) {
    alert(err.message);
  }
}
