// ChuTro "Điện nước" tab — chốt chỉ số (BR-07). Helpers esc/toast from ui.js.
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
    const phong = await window.bridge.call('PHONG_LAY_TAT_CA', {}) || [];
    sel.innerHTML = phong.map(r => `<option value="${esc(r.id)}">${esc(r.soPhong)}</option>`).join('');
    if (keep && phong.some(r => String(r.id) === String(keep))) sel.value = keep;
    if (!sel.value && phong.length) sel.value = String(phong[0].id);
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
    const prev = await window.bridge.call('DIEN_NUOC_LAY_KY_TRUOC', {
      phongId: Number(sel.value),
      kyCuoc: month
    });
    const oldElec = prev?.dienMoi ?? 0;
    const nuocCu = prev?.nuocMoi ?? 0;
    utilEl('util-elec-old').value = oldElec;
    utilEl('util-water-old').value = nuocCu;
    // Có kỳ trước → chỉ số cũ do hệ thống chốt, khóa lại. Chưa từng chốt → cho nhập
    // (đồng hồ đã quay từ trước khi vào phần mềm, số 0 làm chữ điện sai).
    utilEl('util-elec-old').disabled = !!prev;
    utilEl('util-water-old').disabled = !!prev;
    // Đơn giá theo kỳ trước; chỉ số mới khởi đầu bằng chỉ số cũ.
    utilEl('util-elec-rate').value = prev?.giaDien ?? '';
    utilEl('util-water-rate').value = prev?.giaNuoc ?? '';
    utilEl('util-elec-new').value = oldElec;
    utilEl('util-water-new').value = nuocCu;

    // Phục hồi hình thức nước kỳ trước nếu có
    const mode = prev?.hinhThucNuoc || 'Khoi';
    if (utilEl('util-water-mode')) utilEl('util-water-mode').value = mode;
    if (utilEl('util-water-people')) utilEl('util-water-people').value = prev?.soNguoiNuoc || 1;
    onWaterModeChange();
    recalcUtils();
  } catch (err) {
    toast(err.message, 'err');
  }
}

function onWaterModeChange() {
  const mode = utilEl('util-water-mode')?.value || 'Khoi';
  const isNguoi = mode === 'Nguoi';
  const blockKhoi = utilEl('util-water-block-khoi');
  const blockNguoi = utilEl('util-water-block-nguoi');
  const rateLbl = utilEl('util-water-rate-lbl');
  if (blockKhoi) blockKhoi.style.display = isNguoi ? 'none' : 'inline-flex';
  if (blockNguoi) blockNguoi.style.display = isNguoi ? 'inline-flex' : 'none';
  if (rateLbl) rateLbl.textContent = isNguoi ? '× giá/người' : '× giá/khối';
  recalcUtils();
}

// Preview chỉ để xem — server tự tính khi lập hóa đơn (US-12/13).
function recalcUtils() {
  const totals = document.querySelectorAll('#tab-utils .calc .row .tot');
  const money = n => `${Math.max(0, n).toLocaleString('vi-VN')} đ`;

  const oldElec = num(utilEl('util-elec-old'));
  const newElec = num(utilEl('util-elec-new'));
  const elecRate = num(utilEl('util-elec-rate'));
  const chuDien = Math.max(0, newElec - oldElec);
  const chuDienEl = utilEl('util-elec-kwh');
  if (chuDienEl) chuDienEl.textContent = `${chuDien} kWh`;

  const elec = (newElec - oldElec) * elecRate;
  if (totals[0]) {
    totals[0].textContent = money(elec);
    totals[0].style.color = (newElec < oldElec) ? 'var(--terracotta)' : '';
  }

  const mode = utilEl('util-water-mode')?.value || 'Khoi';
  let water = 0;
  if (mode === 'Nguoi') {
    const people = num(utilEl('util-water-people'));
    const waterRate = num(utilEl('util-water-rate'));
    water = people * waterRate;
    if (totals[1]) totals[1].style.color = '';
  } else {
    const oldWater = num(utilEl('util-water-old'));
    const newWater = num(utilEl('util-water-new'));
    const waterRate = num(utilEl('util-water-rate'));
    const soKhoi = Math.max(0, newWater - oldWater);
    const soKhoiEl = utilEl('util-water-m3');
    if (soKhoiEl) soKhoiEl.textContent = `${soKhoi} m³`;
    water = (newWater - oldWater) * waterRate;
    if (totals[1]) totals[1].style.color = (newWater < oldWater) ? 'var(--terracotta)' : '';
  }

  if (totals[1]) totals[1].textContent = money(water);
}

async function saveUtility() {
  const sel = utilEl('util-room');
  const month = toBillingMonth(utilEl('util-month')?.value);
  if (!sel?.value) { toast('Vui lòng chọn phòng.', 'err'); return; }

  const mode = utilEl('util-water-mode')?.value || 'Khoi';
  const isNguoi = mode === 'Nguoi';

  const oldWater = num(utilEl('util-water-old'));
  const newWater = isNguoi ? oldWater : num(utilEl('util-water-new'));
  const people = isNguoi ? num(utilEl('util-water-people')) : 0;

  const reading = {
    id: 0,
    phongId: Number(sel.value),
    kyCuoc: month,
    dienCu: num(utilEl('util-elec-old')),
    dienMoi: num(utilEl('util-elec-new')),
    giaDien: num(utilEl('util-elec-rate')),
    nuocCu: oldWater,
    nuocMoi: newWater,
    giaNuoc: num(utilEl('util-water-rate')),
    hinhThucNuoc: mode,
    soNguoiNuoc: people
  };

  // BR-07 chặn ở server; chặn sớm ở client để khỏi vòng lượt rác.
  if (reading.dienMoi < reading.dienCu) { toast('Chỉ số điện mới phải lớn hơn hoặc bằng chỉ số cũ.', 'err'); return; }
  if (!isNguoi && reading.nuocMoi < reading.nuocCu) { toast('Chỉ số nước mới phải lớn hơn hoặc bằng chỉ số cũ.', 'err'); return; }
  if (isNguoi && reading.soNguoiNuoc <= 0) { toast('Số người dùng nước phải lớn hơn 0.', 'err'); return; }
  if (reading.giaDien <= 0 || reading.giaNuoc <= 0) { toast('Đơn giá điện và nước phải lớn hơn 0.', 'err'); return; }

  try {
    await window.bridge.call('DIEN_NUOC_GHI_SO', reading);
    toast(`Đã chốt chỉ số phòng ${sel.options[sel.selectedIndex].text} tháng ${month}.`, 'ok');
  } catch (err) {
    toast(err.message, 'err');
  }
}