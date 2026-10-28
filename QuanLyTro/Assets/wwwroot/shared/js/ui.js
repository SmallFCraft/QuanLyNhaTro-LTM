// Tiện ích dùng chung cho mọi shell. Không phụ thuộc bridge.
function esc(s) {
  return String(s ?? '').replace(/[&<>"']/g, c => (
    { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]
  ));
}

function fmtMoney(n) {
  const v = Number(n);
  return Number.isFinite(v) ? v.toLocaleString('vi-VN') : '0';
}

function fmtDate(iso) {
  if (!iso) return '—';
  const d = new Date(iso);
  if (isNaN(d)) return '—';
  const p = n => String(n).padStart(2, '0');
  return `${p(d.getDate())}/${p(d.getMonth() + 1)}/${d.getFullYear()} ${p(d.getHours())}:${p(d.getMinutes())}`;
}

function fmtDateOnly(iso) {
  if (!iso) return '—';
  const s = String(iso).slice(0, 10).split('-');
  return s.length === 3 ? `${s[2]}/${s[1]}/${s[0]}` : String(iso);
}

function toast(msg, kind) {
  let root = document.getElementById('modal-root');
  if (!root) {
    root = document.createElement('div');
    root.id = 'modal-root';
    document.body.appendChild(root);
  }
  const el = document.createElement('div');
  el.className = `toast ${kind || 'info'}`;
  el.textContent = msg;
  root.appendChild(el);
  setTimeout(() => el.remove(), 4000);
}

/**
 * Tự động làm mới toàn trang: định kỳ + khi quay lại tab. Thay các nút "F5 / Đồng bộ / Tải lại".
 * Bỏ qua khi modal đang mở (tránh nuốt thao tác nhập liệu) và khi lượt trước còn chạy (tránh chồng request).
 */
function startAutoRefresh(fn, ms = 30000) {
  let running = false;
  const tick = async () => {
    if (running || document.hidden) return;
    // Bỏ qua khi đang mở modal nhập liệu hoặc người dùng đang gõ trong input/textarea/select.
    if (document.querySelector('#modal-root .modal-back, #modal-root .modal')) return;
    const active = document.activeElement;
    if (active && (active.tagName === 'INPUT' || active.tagName === 'TEXTAREA' || active.tagName === 'SELECT')) return;
    running = true;
    try { await fn(); } finally { running = false; }
  };
  setInterval(tick, ms);
  document.addEventListener('visibilitychange', () => { if (!document.hidden) tick(); });
}

function openModal({ title, fields, onSubmit }) {
  const root = document.getElementById('modal-root');
  const back = document.createElement('div');
  back.className = 'modal-back';
  const rows = (fields || []).map(f => {
    const id = 'mf_' + f.name;
    let ctrl;
    if (f.type === 'select') {
      const opts = (f.options || []).map(o =>
        `<option value="${esc(o.value)}"${String(o.value) === String(f.value ?? '') ? ' selected' : ''}>${esc(o.label)}</option>`
      ).join('');
      ctrl = `<select class="inp" id="${id}" name="${esc(f.name)}"${f.disabled ? ' disabled' : ''}>${opts}</select>`;
    } else {
      ctrl = `<input class="inp" id="${id}" name="${esc(f.name)}" type="${f.type || 'text'}"
        value="${esc(f.value ?? '')}"${f.placeholder ? ` placeholder="${esc(f.placeholder)}"` : ''}${f.disabled ? ' disabled' : ''}${f.required ? ' required' : ''}>`;
    }
    return `<div class="modal-row"><label for="${id}">${esc(f.label)}</label>${ctrl}</div>`;
  }).join('');

  back.innerHTML = `
    <div class="modal">
      <div class="modal-hdr">${esc(title || '')}</div>
      <form class="modal-body">${rows}
        <div class="modal-ft">
          <button type="button" class="btn btn-outline btn-sm" data-cancel>Hủy</button>
          <button type="submit" class="btn btn-primary btn-sm">Lưu</button>
        </div>
      </form>
    </div>`;

  const close = () => back.remove();
  back.querySelector('[data-cancel]').addEventListener('click', close);
  back.addEventListener('click', e => { if (e.target === back) close(); });
  back.querySelector('form').addEventListener('submit', async e => {
    e.preventDefault();
    const values = {};
    new FormData(e.target).forEach((v, k) => { values[k] = typeof v === 'string' ? v.trim() : v; });
    try {
      await onSubmit(values);
      close();
    } catch (err) {
      toast(err.message, 'err');
    }
  });
  root.appendChild(back);
  const first = back.querySelector('input:not([disabled]), select:not([disabled])');
  if (first) first.focus();
  return close;
}

/**
 * Hộp thoại thông báo nội bộ — thay window.alert (Chromium chèn tiêu đề tên miền).
 * kind: 'info' (mặc định) | 'ok' | 'warn' | 'err' — đổi màu icon + tiêu đề theo theme.
 */
function alertDialog(msg, title, kind) {
  const root = document.getElementById('modal-root');
  if (!root) { toast(msg, kind === 'ok' ? 'ok' : 'err'); return Promise.resolve(); }
  const tone = kind || 'info';
  const icons = { info: 'fa-circle-info', ok: 'fa-circle-check', warn: 'fa-triangle-exclamation', err: 'fa-circle-exclamation' };
  const titles = { info: 'Thông báo', ok: 'Thành công', warn: 'Cảnh báo', err: 'Lỗi' };
  const back = document.createElement('div');
  back.className = 'modal-back';
  back.innerHTML = `
    <div class="modal dlg dlg-${esc(tone)}" role="alertdialog" aria-modal="true">
      <div class="dlg-body">
        <div class="dlg-icon"><i class="fas ${icons[tone] || icons.info}"></i></div>
        <div class="dlg-text">
          <div class="dlg-title">${esc(title || titles[tone] || titles.info)}</div>
          <div class="dlg-msg">${esc(msg)}</div>
        </div>
      </div>
      <div class="dlg-ft">
        <button type="button" class="btn btn-primary btn-sm" data-ok>Đã hiểu</button>
      </div>
    </div>`;
  return new Promise(resolve => {
    const close = () => { back.remove(); document.removeEventListener('keydown', onKey); resolve(); };
    const onKey = e => { if (e.key === 'Escape' || e.key === 'Enter') close(); };
    back.querySelector('[data-ok]').addEventListener('click', close);
    back.addEventListener('click', e => { if (e.target === back) close(); });
    document.addEventListener('keydown', onKey);
    root.appendChild(back);
    back.querySelector('[data-ok]').focus();
  });
}

/** Hộp thoại xác nhận nội bộ — thay window.confirm. Trả về Promise<boolean>. */
function confirmDialog(msg, title, kind) {
  const root = document.getElementById('modal-root');
  if (!root) return Promise.resolve(false);
  const tone = kind || 'warn';
  const icons = { info: 'fa-circle-info', warn: 'fa-triangle-exclamation', err: 'fa-circle-exclamation' };
  const back = document.createElement('div');
  back.className = 'modal-back';
  back.innerHTML = `
    <div class="modal dlg dlg-${esc(tone)}" role="alertdialog" aria-modal="true">
      <div class="dlg-body">
        <div class="dlg-icon"><i class="fas ${icons[tone] || icons.warn}"></i></div>
        <div class="dlg-text">
          <div class="dlg-title">${esc(title || 'Xác nhận thao tác')}</div>
          <div class="dlg-msg">${esc(msg)}</div>
        </div>
      </div>
      <div class="dlg-ft">
        <button type="button" class="btn btn-outline btn-sm" data-cancel>Hủy</button>
        <button type="button" class="btn btn-danger btn-sm" data-ok>Đồng ý</button>
      </div>
    </div>`;
  return new Promise(resolve => {
    const done = v => { back.remove(); document.removeEventListener('keydown', onKey); resolve(v); };
    const onKey = e => {
      if (e.key === 'Escape') done(false);
      if (e.key === 'Enter') done(true);
    };
    back.querySelector('[data-cancel]').addEventListener('click', () => done(false));
    back.querySelector('[data-ok]').addEventListener('click', () => done(true));
    back.addEventListener('click', e => { if (e.target === back) done(false); });
    document.addEventListener('keydown', onKey);
    root.appendChild(back);
    back.querySelector('[data-ok]').focus();
  });
}

/**
 * Trạng thái hợp đồng của từng phòng — một sự thật dùng chung cho các tab
 * Phòng / Người thuê (BR-18/BR-19): HieuLuc > ChoNhanPhong > ChamDut.
 * Input: items thô từ HOP_DONG_LAY_TAT_CA (MucHopDongItem hoặc HopDongDto).
 * Trả về Map(phongId → trạng thái mạnh nhất).
 */
function contractStatusByRoom(items) {
  const rank = s => (s === 'HieuLuc' ? 3 : s === 'ChoNhanPhong' ? 2 : s === 'ChamDut' ? 1 : 0);
  const byRoom = new Map();
  for (const item of items || []) {
    const c = item.contract || item.hopDong || item;
    const st = c.trangThai ?? c.trang_thai;
    const pid = Number(c.phongId ?? c.phong_id);
    if (!Number.isFinite(pid)) continue;
    if (rank(st) > rank(byRoom.get(pid))) byRoom.set(pid, st);
  }
  return byRoom;
}

// Suy trạng thái hiển thị từ trạng thái HĐ của phòng:
// 'HieuLuc' → Đang thuê, 'ChoNhanPhong' (kể cả ham muốn) → Chờ nhận phòng,
// còn lại (HetHan/ChamDut/mất HĐ) → Treo HĐ, null (chưa gán phòng) → Chờ nhận phòng.
function tenantStatusTag(contractState, hasRoom) {
  if (!hasRoom || contractState === 'ChoNhanPhong')
    return `<span class="tag warn"><i class="fas fa-clock"></i> Chờ nhận phòng</span>`;
  if (contractState === 'HieuLuc')
    return `<span class="tag done"><i class="fas fa-check-circle"></i> Đang thuê</span>`;
  return `<span class="tag warn" title="Còn gán phòng nhưng không có hợp đồng hiệu lực — cần trả phòng hoặc ký lại."><i class="fas fa-exclamation-triangle"></i> Treo HĐ</span>`;
}

// Dựng URL ảnh QR SePay (https://qr.sepay.vn/img — public, không cần token).
// Dùng chung cho tab Cài đặt của Chủ trọ và tab Kỳ này của Khách thuê.
function sepayQrUrl(cfg, amount, des) {
  const s = cfg || {};
  const bank = s.nganHang || s.bank;
  const acc = s.soTaiKhoan || s.acc;
  if (!bank || !acc) return '';
  const p = new URLSearchParams({
    bank,
    acc,
    template: s.templateQr || s.template || 'compact'
  });
  if (Number.isFinite(amount) && amount >= 1000 && amount <= 499000000) {
    p.set('amount', String(Math.round(amount)));
  }
  const memo = boDauMemo(des);
  if (memo) p.set('des', memo);
  return `https://qr.sepay.vn/img?${p.toString()}`;
}

// `des` của SePay chỉ nhận chữ/số không dấu — bỏ dấu rồi mới đưa vào URL.
function boDauMemo(text) {
  return String(text || '')
    .normalize('NFD').replace(/[̀-ͯ]/g, '')
    .replace(/đ/g, 'd').replace(/Đ/g, 'D')
    .replace(/[^A-Za-z0-9 ]/g, ' ')
    .replace(/\s+/g, ' ')
    .trim();
}

window.alert = msg => toast(msg, 'err');

async function logout() {
  await window.bridge.call('UI_LOGOUT', {});
  window.location.href = '../auth/index.html';
}
