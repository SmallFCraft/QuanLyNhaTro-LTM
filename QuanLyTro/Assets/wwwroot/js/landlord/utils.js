// Landlord "Điện nước" tab — chốt chỉ số (BR-07). Helpers esc/toast from ui.js.
// Tháng server yêu cầu `yyyy-MM`; <option> trên index.html có thể là `MM/yyyy` → chuẩn hoá 1 chỗ.
function toBillingMonth(value) {
  const s = String(value ?? '').trim();
  const m = s.match(/^(\d{1,2})\/(\d{4})$/);
  return m ? `${m[2]}-${m[1].padStart(2, '0')}` : s;
}

const utilEl = id => document.getElementById(id);
const num = el => Number(String(el?.value ?? '').replace(/\./g, '').replace(',', '.')) || 0;

async function loadUtils() {
  const sel = utilEl('util-room');
  if (!sel) return;
  const keep = sel.value;
  try {
    const rooms = await window.bridge.call('ROOM_GET_ALL', {}) || [];
    sel.innerHTML = rooms.map(r => `<option value="${esc(r.id)}">${esc(r.roomNumber)}</option>`).join('');
    if (keep && rooms.some(r => String(r.id) === String(keep))) sel.value = keep;
    if (!sel.value && rooms.length) sel.value = String(rooms[0].id);
    await fillPreviousReading();
  } catch (err) {
    toast(err.message, 'err');
  }
}

// Chỉ số cũ = bản ghi gần nhất TRƯỚC tháng đang chốt (server chặn dưới bằng SQL).
async function fillPreviousReading() {
  const sel = utilEl('util-room');
  const month = toBillingMonth(utilEl('util-month')?.value);
  if (!sel?.value || !month) return;
  try {
    const prev = await window.bridge.call('UTILITY_GET_PREVIOUS', {
      roomId: Number(sel.value),
      billingMonth: month
    });
    const oldElec = prev?.newElectricity ?? 0;
    const oldWater = prev?.newWater ?? 0;
    utilEl('util-elec-old').value = oldElec;
    utilEl('util-water-old').value = oldWater;
    // Đơn giá theo kỳ trước; chỉ số mới khởi đầu bằng chỉ số cũ.
    utilEl('util-elec-rate').value = prev?.electricityRate ?? '';
    utilEl('util-water-rate').value = prev?.waterRate ?? '';
    utilEl('util-elec-new').value = oldElec;
    utilEl('util-water-new').value = oldWater;
    recalcUtils();
  } catch (err) {
    toast(err.message, 'err');
  }
}

// Preview chỉ để xem — server tự tính khi lập hóa đơn (US-12/13).
function recalcUtils() {
  const totals = document.querySelectorAll('#tab-utils .calc .row .tot');
  const money = n => `${n.toLocaleString('vi-VN')} đ`;
  const elec = (num(utilEl('util-elec-new')) - num(utilEl('util-elec-old'))) * num(utilEl('util-elec-rate'));
  const water = (num(utilEl('util-water-new')) - num(utilEl('util-water-old'))) * num(utilEl('util-water-rate'));
  if (totals[0]) totals[0].textContent = money(elec);
  if (totals[1]) totals[1].textContent = money(water);
}

async function saveUtility() {
  const sel = utilEl('util-room');
  const month = toBillingMonth(utilEl('util-month')?.value);
  if (!sel?.value) { toast('Vui lòng chọn phòng.', 'err'); return; }

  const reading = {
    id: 0,
    roomId: Number(sel.value),
    billingMonth: month,
    oldElectricity: num(utilEl('util-elec-old')),
    newElectricity: num(utilEl('util-elec-new')),
    electricityRate: num(utilEl('util-elec-rate')),
    oldWater: num(utilEl('util-water-old')),
    newWater: num(utilEl('util-water-new')),
    waterRate: num(utilEl('util-water-rate'))
  };

  // BR-07 chặn ở server; chặn sớm ở client để khỏi vòng lượt rác.
  if (reading.newElectricity < reading.oldElectricity) { toast('Chỉ số điện mới phải lớn hơn hoặc bằng chỉ số cũ.', 'err'); return; }
  if (reading.newWater < reading.oldWater) { toast('Chỉ số nước mới phải lớn hơn hoặc bằng chỉ số cũ.', 'err'); return; }
  if (reading.electricityRate <= 0 || reading.waterRate <= 0) { toast('Đơn giá điện và nước phải lớn hơn 0.', 'err'); return; }

  try {
    await window.bridge.call('UTILITY_RECORD', reading);
    toast(`Đã chốt chỉ số phòng ${sel.options[sel.selectedIndex].text} tháng ${month}.`, 'ok');
  } catch (err) {
    toast(err.message, 'err');
  }
}