// Tab Tạm trú (PM-04): chỉ những người CHƯA đăng ký tạm trú.
function renderResidence(rows) {
  const body = document.querySelector('#ptab-residence tbody');
  if (!body) return;
  const pending = (rows || []).filter(t => !t.isTemporaryRegistered);
  body.innerHTML = pending.map(t => `
    <tr>
      <td><b>${esc(t.fullName)}</b></td>
      <td class="mono">${fmtDateOnly(t.dateOfBirth)}</td>
      <td class="mono">${esc(t.idCard)}</td>
      <td>${esc(t.hometown)}</td>
      <td><b>${esc(t.roomNumber)}</b></td>
      <td class="mono">—</td>
      <td><span class="tag warn">Chưa đăng ký</span></td>
    </tr>`).join('') || `<tr class="empty"><td colspan="7">Tất cả đã đăng ký tạm trú</td></tr>`;

  const foot = document.querySelector('#ptab-residence .tblfoot span');
  if (foot) foot.textContent = `Tổng: ${pending.length} chưa đăng ký`;
}
