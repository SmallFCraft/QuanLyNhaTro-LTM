const POLICE_TITLES = {
  citizens: 'Tra cứu công dân',
  residence: 'Báo cáo lưu trú & tạm trú',
  history: 'Lịch sử biến động lưu trú'
};

const searchBox = () => document.getElementById('p-search');
const searchKey = () => (searchBox()?.value || '').toLowerCase().trim();

// Bộ lọc chạy client-side trên cache.
let policeCache = null;

async function loadPoliceTab(tabKey) {
  document.getElementById('ptitle').textContent = POLICE_TITLES[tabKey] || '';
  Object.keys(POLICE_TITLES).forEach(k => {
    const el = document.getElementById('ptab-' + k);
    if (el) el.hidden = (k !== tabKey);
  });
  document.querySelectorAll('#police .tabs .tab').forEach(t => {
    t.classList.toggle('on', t.dataset.ptab === tabKey);
  });

  try {
    if (tabKey === 'citizens' || tabKey === 'residence') {
      // ponytail: TENANT_GET_BY_ROOM là action đọc duy nhất cho Công an (BR-16) nên phải hỏi
      // từng phòng (N+1). Thêm TENANT_GET_ALL vào ma trận quyền khi số phòng lớn.
      if (!policeCache) {
        const rooms = await window.bridge.call('ROOM_GET_ALL', {}) || [];
        const all = [];
        for (const r of rooms) {
          const tenants = await window.bridge.call('TENANT_GET_BY_ROOM', { roomId: r.id }) || [];
          // TenantDto không có roomNumber -> gắn từ phòng đang lặp.
          for (const t of tenants) all.push({ ...t, roomNumber: r.roomNumber });
        }
        policeCache = all;
      }
      renderCitizens(policeCache);
      renderResidence(policeCache);
    }
    if (tabKey === 'history') await loadHistory();
  } catch (err) {
    alert(err.message);
  }
}

function renderCitizens(rows) {
  const body = document.querySelector('#ptab-citizens tbody');
  if (!body) return;
  const q = searchKey();
  const filtered = q ? (rows || []).filter(t =>
    (t.fullName || '').toLowerCase().includes(q) ||
    (t.idCard || '').toLowerCase().includes(q) ||
    (t.phone || '').includes(q)) : (rows || []);
  body.innerHTML = filtered.map(t => `
    <tr>
      <td><b>${esc(t.fullName)}</b></td>
      <td class="mono">${fmtDateOnly(t.dateOfBirth)}</td>
      <td class="mono">${esc(t.idCard)}</td>
      <td>${esc(t.hometown)}</td>
      <td><b>${esc(t.roomNumber)}</b></td>
      <td class="num"><span class="tag ${t.isTemporaryRegistered ? 'done' : 'warn'}">
        ${t.isTemporaryRegistered ? 'Đã đăng ký' : 'Chưa đăng ký'}</span></td>
    </tr>`).join('') || `<tr><td colspan="6">Không có công dân nào</td></tr>`;
}

function renderResidence(rows) {
  const body = document.querySelector('#ptab-residence tbody');
  if (!body) return;
  // Tab tạm trú: chỉ những người CHƯA đăng ký tạm trú (PM-04).
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
    </tr>`).join('') || `<tr><td colspan="7">Tất cả đã đăng ký tạm trú</td></tr>`;
}

// Toolbar #ptab-history đọc khoảng ngày theo id #p-from, #p-to.
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
    </tr>`).join('') || `<tr><td colspan="6">Không có biến động nào</td></tr>`;
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

// Gắn lắng nghe tìm kiếm công dân khi DOM sẵn sàng.
document.addEventListener('DOMContentLoaded', () => {
  const search = searchBox();
  if (search) search.addEventListener('input', () => renderCitizens(policeCache));
});
