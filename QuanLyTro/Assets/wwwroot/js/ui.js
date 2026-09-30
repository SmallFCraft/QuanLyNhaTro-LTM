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
