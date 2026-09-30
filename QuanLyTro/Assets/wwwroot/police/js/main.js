// Màn hình Công an phường (ReadOnly — BR-16): khung 3 tab + điều hướng.
// Tiện ích dùng chung ở ../shared/js/ui.js.
const POLICE_TITLES = {
  citizens: 'Tra cứu công dân',
  residence: 'Báo cáo lưu trú & tạm trú',
  history: 'Lịch sử biến động lưu trú'
};

// Cache client-side dùng chung cho tab Công dân và Tạm trú.
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
      await ensurePoliceCache();
      renderCitizens(policeCache);
      renderResidence(policeCache);
    }
    if (tabKey === 'history') await loadHistory();
  } catch (err) {
    toast(err.message, 'err');
  }
}

async function ensurePoliceCache() {
  if (policeCache) return;
  // ponytail: TENANT_GET_BY_ROOM là action đọc duy nhất cho Công an (BR-16) nên phải hỏi
  // từng phòng (N+1). Thêm TENANT_GET_ALL vào ma trận quyền khi số phòng lớn.
  const rooms = await window.bridge.call('ROOM_GET_ALL', {}) || [];
  const all = [];
  for (const r of rooms) {
    const tenants = await window.bridge.call('TENANT_GET_BY_ROOM', { roomId: r.id }) || [];
    // TenantDto không có roomNumber -> gắn từ phòng đang lặp.
    for (const t of tenants) all.push({ ...t, roomNumber: r.roomNumber });
  }
  policeCache = all;
}

async function logout() {
  await window.bridge.call('UI_LOGOUT', {});
  window.location.href = '../auth/index.html';
}

document.addEventListener('DOMContentLoaded', () => {
  // auth.js điều hướng sang đây kèm ?u=<tên đã encode>.
  const name = new URLSearchParams(window.location.search).get('u');
  if (name) document.getElementById('pname').textContent = name;
  loadPoliceTab('citizens');
});
