// ChuTro "Hóa đơn" tab — lập hóa đơn + thu tiền (BR-11).
// Dùng esc/fmtMoney/toast/openModal/confirmDialog (ui.js) và toBillingMonth (js/utils.js).
const invEl = id => document.getElementById(id);

async function refreshInvoiceRoomOptions() {
  const sel = invEl('inv-room');
  if (!sel) return;
  const keep = sel.value;
  try {
    const phong = await window.bridge.call('PHONG_LAY_TAT_CA', {}) || [];
    sel.innerHTML = `<option value="">Tất cả</option>` + phong.map(r =>
      `<option value="${esc(r.id)}">${esc(r.soPhong)}</option>`).join('');
    if (keep) sel.value = keep;
  } catch (err) {
    toast(err.message, 'err');
  }
}

async function loadInvoices() {
  const body = invEl('hoa_don-body');
  if (!body) return;
  await refreshInvoiceRoomOptions();
  const payload = { kyCuoc: toBillingMonth(invEl('inv-month')?.value) };
  if (invEl('inv-room')?.value) payload.phongId = Number(invEl('inv-room').value);
  try {
    const rows = await window.bridge.call('HOA_DON_LAY_TAT_CA', payload) || [];
    body.innerHTML = rows.map(i => {
      // BR-11: hóa đơn đã thu bất biến — nút Thu khóa.
      const paid = (i.trangThai ?? i.trang_thai) === 'DaThu';
      return `<tr>
        <td><b>${esc(i.soPhong ?? `#${i.phongId}`)}</b></td>
        <td class="num">${fmtMoney(i.tienPhong)}</td>
        <td class="num">${fmtMoney(i.tienDien)}</td>
        <td class="num">${fmtMoney(i.tienNuoc)}</td>
        <td class="num">${fmtMoney(i.phiKhac)}</td>
        <td class="num amount">${fmtMoney(i.tongTien)}</td>
        <td><span class="tag ${paid ? 'paid' : 'unpaid'}">${paid ? 'Đã thu' : 'Chưa thu'}</span></td>
        <td><button class="btn ${paid ? 'btn-outline' : 'btn-primary'} btn-sm"
            ${paid ? 'disabled' : `onclick="payInvoice(${i.id})"`}>
            <i class="fas fa-hand-holding-usd"></i> Thu</button></td>
      </tr>`;
    }).join('') || `<tr><td colspan="8">Chưa có hóa đơn nào</td></tr>`;

    const foot = document.querySelector('#tab-hoa_don .tblfoot span');
    if (foot) foot.textContent = `Tổng: ${rows.length} bản ghi`;
  } catch (err) {
    toast(err.message, 'err');
  }
}

async function payInvoice(id) {
  if (!await confirmDialog(`Thu hóa đơn #${id}?`)) return;
  try {
    await window.bridge.call('HOA_DON_THANH_TOAN', { hoaDonId: Number(id) });
    toast('Đã thu hóa đơn.', 'ok');
    await loadInvoices();
  } catch (err) {
    toast(err.message, 'err');
  }
}

async function createInvoice() {
  try {
    const phong = await window.bridge.call('PHONG_LAY_TAT_CA', {}) || [];
    if (!phong.length) { toast('Không có phòng nào.', 'err'); return; }
    openModal({
      title: 'Lập hóa đơn tháng',
      fields: [
        {
          name: 'phongId', label: 'Phòng', type: 'select', required: true,
          value: String(phong[0].id),
          options: phong.map(r => ({ value: String(r.id), label: r.soPhong }))
        },
        { name: 'kyCuoc', label: 'Tháng (yyyy-MM)', value: toBillingMonth(invEl('inv-month')?.value), required: true },
        { name: 'phiKhac', label: 'Phí khác', type: 'number', value: '0' }
      ],
      onSubmit: async v => {
        // Server tự tính mọi khoản từ hợp đồng + chỉ số; client chỉ gửi Phòng + Tháng + Phí khác.
        await window.bridge.call('HOA_DON_TAO', {
          phongId: Number(v.phongId),
          kyCuoc: String(v.kyCuoc || '').trim(),
          phiKhac: Number(v.phiKhac) || 0
        });
        toast('Đã lập hóa đơn.', 'ok');
        await loadInvoices();
      }
    });
  } catch (err) {
    toast(err.message, 'err');
  }
}