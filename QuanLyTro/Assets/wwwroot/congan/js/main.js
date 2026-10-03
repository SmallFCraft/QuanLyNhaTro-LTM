// Màn hình Công an phường (BR-16): khung 3 tab + điều hướng.
// Tiện ích dùng chung ở ../shared/js/ui.js.
const POLICE_TITLES = {
  citizens: 'Tra cứu công dân',
  residence: 'Báo cáo lưu trú & tạm trú',
  history: 'Lịch sử biến động lưu trú'
};

// Cache client-side dùng chung cho tab Công dân và Tạm trú.
let policeCache = null;
let _currentPTab = 'citizens';

async function loadPoliceTab(tabKey) {
  _currentPTab = tabKey || _currentPTab;
  document.getElementById('ptitle').textContent = POLICE_TITLES[tabKey] || '';
  Object.keys(POLICE_TITLES).forEach(k => {
    const el = document.getElementById('ptab-' + k);
    if (el) el.hidden = (k !== tabKey);
  });
  document.querySelectorAll('#congan .tabs .tab').forEach(t => {
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
  // ponytail: KHACH_THUE_THEO_PHONG là hanh_dong đọc duy nhất cho Công an (BR-16) nên phải hỏi
  // từng phòng (N+1). Thêm TENANT_GET_ALL vào ma trận quyền khi số phòng lớn.
  const phong = await window.bridge.call('PHONG_LAY_TAT_CA', {}) || [];
  const all = [];
  for (const r of phong) {
    const khach_thue = await window.bridge.call('KHACH_THUE_THEO_PHONG', { phongId: r.id }) || [];
    // KhachThueDto không có soPhong -> gắn từ phòng đang lặp.
    for (const t of khach_thue) all.push({ ...t, soPhong: r.soPhong });
  }
  policeCache = all;
}

document.addEventListener('DOMContentLoaded', () => {
  // auth.js điều hướng sang đây kèm ?u=<tên đã encode>.
  const name = new URLSearchParams(window.location.search).get('u');
  if (name) document.getElementById('pname').textContent = name;
  loadPoliceTab('citizens');

  // ponytail: tự động làm mới toàn trang — thay nút sync tay. Reset cache buộc nạp lại từ server.
  startAutoRefresh(() => { policeCache = null; return loadPoliceTab(_currentPTab); }, 30000);
});
