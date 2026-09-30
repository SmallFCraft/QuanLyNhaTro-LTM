// Landlord "Hóa đơn" tab — lập hóa đơn + thu tiền (BR-11).
// Dùng esc/fmtMoney/toast/openModal/confirmDialog (ui.js) và toBillingMonth (landlord/utils.js).
const invEl = id => document.getElementById(id);

async function refreshInvoiceRoomOptions() {
  const sel = invEl('inv-room');
  if (!sel) return;
  const keep = sel.value;
  try {
    const rooms = await window.bridge.call('ROOM_GET_ALL', {}) || [];
    sel.innerHTML = `<option value="">Tất cả</option>` + rooms.map(r =>
      `<option value="${esc(r.id)}">${esc(r.roomNumber)}</option>`).join('');
    if (keep) sel.value = keep;
  } catch (err) {
    toast(err.message, 'err');
  }
}

async function loadInvoices() {
  const body = invEl('invoices-body');
  if (!body) return;
  await refreshInvoiceRoomOptions();
  const payload = { billingMonth: toBillingMonth(invEl('inv-month')?.value) };
  if (invEl('inv-room')?.value) payload.roomId = Number(invEl('inv-room').value);
  try {
    const rows = await window.bridge.call('INVOICE_GET_ALL', payload) || [];
    body.innerHTML = rows.map(i => {
      // BR-11: hóa đơn đã thu bất biến — nút Thu khóa.
      const paid = i.status === 'Paid';
      return `<tr>
        <td><b>${esc(i.roomNumber ?? `#${i.roomId}`)}</b></td>
        <td class="num">${fmtMoney(i.roomAmount)}</td>
        <td class="num">${fmtMoney(i.electricityAmount)}</td>
        <td class="num">${fmtMoney(i.waterAmount)}</td>
        <td class="num">${fmtMoney(i.otherFees)}</td>
        <td class="num amount">${fmtMoney(i.totalAmount)}</td>
        <td><span class="tag ${paid ? 'paid' : 'unpaid'}">${paid ? 'Đã thu' : 'Chưa thu'}</span></td>
        <td><button class="btn ${paid ? 'btn-outline' : 'btn-primary'} btn-sm"
            ${paid ? 'disabled' : `onclick="payInvoice(${i.id})"`}>
            <i class="fas fa-hand-holding-usd"></i> Thu</button></td>
      </tr>`;
    }).join('') || `<tr><td colspan="8">Chưa có hóa đơn nào</td></tr>`;
  } catch (err) {
    toast(err.message, 'err');
  }
}

async function payInvoice(id) {
  if (!await confirmDialog(`Thu hóa đơn #${id}?`)) return;
  try {
    await window.bridge.call('INVOICE_PAY', { invoiceId: Number(id) });
    toast('Đã thu hóa đơn.', 'ok');
    await loadInvoices();
  } catch (err) {
    toast(err.message, 'err');
  }
}

async function createInvoice() {
  try {
    const rooms = await window.bridge.call('ROOM_GET_ALL', {}) || [];
    if (!rooms.length) { toast('Không có phòng nào.', 'err'); return; }
    openModal({
      title: 'Lập hóa đơn tháng',
      fields: [
        {
          name: 'roomId', label: 'Phòng', type: 'select', required: true,
          value: String(rooms[0].id),
          options: rooms.map(r => ({ value: String(r.id), label: r.roomNumber }))
        },
        { name: 'billingMonth', label: 'Tháng (yyyy-MM)', value: toBillingMonth(invEl('inv-month')?.value), required: true },
        { name: 'otherFees', label: 'Phí khác', type: 'number', value: '0' }
      ],
      onSubmit: async v => {
        // Server tự tính mọi khoản từ hợp đồng + chỉ số; client chỉ gửi Phòng + Tháng + Phí khác.
        await window.bridge.call('INVOICE_CREATE', {
          roomId: Number(v.roomId),
          billingMonth: String(v.billingMonth || '').trim(),
          otherFees: Number(v.otherFees) || 0
        });
        toast('Đã lập hóa đơn.', 'ok');
        await loadInvoices();
      }
    });
  } catch (err) {
    toast(err.message, 'err');
  }
}