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
  const root = document.getElementById('modal-root');
  if (!root) { alert(msg); return; }
  const el = document.createElement('div');
  el.className = `toast ${kind || 'info'}`;
  el.textContent = msg;
  root.appendChild(el);
  setTimeout(() => el.remove(), 4000);
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
        value="${esc(f.value ?? '')}"${f.disabled ? ' disabled' : ''}${f.required ? ' required' : ''}>`;
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

function confirmDialog(msg) {
  return Promise.resolve(window.confirm(msg));
}

// ===== Thanh menu =====
const MENU_ITEMS = {
  landlord: [
    { icon: 'fa-door-open', label: 'Phòng', tab: 'rooms' },
    { icon: 'fa-users', label: 'Người thuê', tab: 'tenants' },
    { icon: 'fa-file-signature', label: 'Hợp đồng', tab: 'contracts' },
    { icon: 'fa-tachometer-alt', label: 'Điện nước', tab: 'utils' },
    { icon: 'fa-receipt', label: 'Hóa đơn', tab: 'invoices' }
  ]
};

function toggleMenu(el) {
  const open = el.querySelector('.dropdown-menu');
  if (open) { open.remove(); return; }

  const shell = el.closest('.win')?.id || 'landlord';
  const items = MENU_ITEMS[shell] || [];
  if (items.length === 0) return;

  const menu = document.createElement('div');
  menu.className = 'dropdown-menu';
  menu.innerHTML = items.map(i =>
    `<div class="dropdown-item" onclick="loadLandlordTab('${i.tab}');this.closest('.dropdown-menu').remove()">
       <i class="fas ${i.icon}"></i>${esc(i.label)}</div>`).join('');

  // Gắn vào body với position:fixed — .hscroll của menustrip có overflow nên
  // dropdown gắn trong đó sẽ bị cắt cụt.
  const r = el.getBoundingClientRect();
  menu.style.top = (r.bottom + 4) + 'px';
  menu.style.left = r.left + 'px';
  document.body.appendChild(menu);

  const close = e => {
    if (!menu.contains(e.target) && !el.contains(e.target)) {
      menu.remove();
      document.removeEventListener('click', close);
    }
  };
  setTimeout(() => document.addEventListener('click', close), 0);
}

function showInfoDialog() {
  openModal({
    title: 'Thông tin phần mềm',
    fields: [
      { name: 'app', label: 'Ứng dụng', type: 'text',
        value: 'Hệ Thống Quản Lý Phòng Trọ Phường Ngũ Hành Sơn', disabled: true },
      { name: 'rt', label: 'Nền tảng', type: 'text',
        value: '.NET 8.0 — WinForms + WebView2 Client', disabled: true },
      { name: 'proto', label: 'Giao thức', type: 'text',
        value: 'TCP Socket, gói tin JSON kết thúc bằng ký tự xuống dòng', disabled: true }
    ],
    onSubmit: async () => { /* chỉ để xem */ }
  });
}

function showHelp() {
  openModal({
    title: 'Trợ giúp nhanh',
    fields: [
      { name: 'h1', label: 'Đăng nhập', type: 'text',
        value: 'Chủ trọ dùng tài khoản admin; công an phường và người thuê dùng tài khoản được cấp.', disabled: true },
      { name: 'h2', label: 'Dữ liệu', type: 'text',
        value: 'Mọi thao tác đọc/ghi đều gửi qua máy chủ TCP — không truy cập cơ sở dữ liệu trực tiếp.', disabled: true },
      { name: 'h3', label: 'Mất kết nối', type: 'text',
        value: 'Nếu mất kết nối tới máy chủ, ứng dụng thông báo và giữ nguyên dữ liệu đã nhập.', disabled: true }
    ],
    onSubmit: async () => { /* chỉ để xem */ }
  });
}
