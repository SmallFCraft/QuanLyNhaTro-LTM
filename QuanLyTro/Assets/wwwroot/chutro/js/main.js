// Màn hình Chủ trọ: khung 7 tab + điều hướng. Tiện ích dùng chung ở ../shared/js/ui.js.
const LANDLORD_TITLES = {
  dash: 'Tổng quan vận hành cơ sở',
  phong: 'Danh sách phòng trọ',
  khach_thue: 'Quản lý người dùng & khách thuê',
  hop_dong: 'Hợp đồng & khách thuê',
  utils: 'Chốt chỉ số điện nước',
  hoa_don: 'Hóa đơn & thu tiền',
  reports: 'Thống kê doanh thu',
  perms: 'Quản lý phân quyền'
};

const LANDLORD_LOADERS = {
  dash: () => loadDash(),
  phong: () => loadRooms(),
  khach_thue: () => loadTenants(),
  hop_dong: () => loadContracts(),
  utils: () => loadUtils(),
  hoa_don: () => loadInvoices(),
  reports: () => loadReports(),
  perms: () => loadPerms()
};

let _currentTab = 'dash';

async function loadLandlordTab(tabKey) {
  _currentTab = tabKey || _currentTab;
  document.getElementById('modtitle').textContent = LANDLORD_TITLES[_currentTab] || '';
  Object.keys(LANDLORD_TITLES).forEach(k => {
    const el = document.getElementById('tab-' + k);
    if (el) el.hidden = (k !== _currentTab);
  });
  document.querySelectorAll('#chutro .tabs .tab').forEach(t => {
    t.classList.toggle('on', t.dataset.tab === _currentTab);
  });
  try {
    const loader = LANDLORD_LOADERS[_currentTab];
    if (loader) await loader();
  } catch (err) {
    toast(err.message, 'err');
  }
}

document.addEventListener('DOMContentLoaded', () => {
  // auth.js điều hướng sang đây kèm ?u=<tên đã encode> và ?vai_tro=ChuTro|QuanLy.
  const params = new URLSearchParams(window.location.search);
  const name = params.get('u');
  if (name) document.getElementById('uname').textContent = name;
  applyLandlordRoleView(params.get('vai_tro') || 'ChuTro');
  loadLandlordTab('dash');

  // ponytail: tự động làm mới tab hiện tại mỗi 30s + khi quay lại cửa sổ — thay nút F5 / Tải lại.
  startAutoRefresh(() => loadLandlordTab(_currentTab), 30000);
});

/**
 * Giao diện thích ứng theo vai trò. Server vẫn là nơi quyết định quyền cuối cùng ở mọi TCP hanh_dong —
 * đây chỉ là ẩn/hiện cho đúng trải nghiệm.
 * - ChuTro: hiện tab Phân quyền, huy hiệu Chủ trọ (Toàn quyền).
 * - QuanLy: ẩn tab Phân quyền, huy hiệu Quản lý cơ sở.
 */
function applyLandlordRoleView(vai_tro) {
  const isManager = vai_tro === 'QuanLy';
  const badge = document.getElementById('user-vai_tro-badge');
  const statusRole = document.getElementById('statusRole');
  const permsBtn = document.getElementById('tabbtn-perms');

  if (badge) {
    badge.innerHTML = isManager
      ? '<i class="fas fa-user-tie"></i> Quản lý cơ sở'
      : '<i class="fas fa-crown"></i> Chủ trọ (Toàn quyền)';
  }
  if (statusRole) statusRole.textContent = isManager ? 'Quản lý cơ sở' : 'Chủ trọ (ChuTro)';
  if (permsBtn) permsBtn.style.display = isManager ? 'none' : '';
  document.title = isManager ? 'Quản Lý Khu Trọ — Quản lý cơ sở' : 'Quản Lý Khu Trọ — Chủ trọ';
}
