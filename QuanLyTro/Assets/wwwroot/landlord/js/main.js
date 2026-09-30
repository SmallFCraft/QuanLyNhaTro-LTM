// Màn hình Chủ trọ: khung 7 tab + điều hướng. Tiện ích dùng chung ở ../shared/js/ui.js.
const LANDLORD_TITLES = {
  dash: 'Tổng quan vận hành cơ sở',
  rooms: 'Danh sách phòng trọ',
  tenants: 'Hồ sơ khách thuê',
  contracts: 'Hợp đồng & khách thuê',
  utils: 'Chốt chỉ số điện nước',
  invoices: 'Hóa đơn & thu tiền',
  reports: 'Thống kê doanh thu'
};

const LANDLORD_LOADERS = {
  dash: () => loadDash(),
  rooms: () => loadRooms(),
  tenants: () => loadTenants(),
  contracts: () => loadContracts(),
  utils: () => loadUtils(),
  invoices: () => loadInvoices(),
  reports: () => loadReports()
};

async function loadLandlordTab(tabKey) {
  document.getElementById('modtitle').textContent = LANDLORD_TITLES[tabKey] || '';
  Object.keys(LANDLORD_TITLES).forEach(k => {
    const el = document.getElementById('tab-' + k);
    if (el) el.hidden = (k !== tabKey);
  });
  document.querySelectorAll('#landlord .tabs .tab').forEach(t => {
    t.classList.toggle('on', t.dataset.tab === tabKey);
  });
  try {
    const loader = LANDLORD_LOADERS[tabKey];
    if (loader) await loader();
  } catch (err) {
    toast(err.message, 'err');
  }
}

async function logout() {
  await window.bridge.call('UI_LOGOUT', {});
  window.location.href = '../auth/index.html';
}

document.addEventListener('DOMContentLoaded', () => {
  // auth.js điều hướng sang đây kèm ?u=<tên đã encode>.
  const name = new URLSearchParams(window.location.search).get('u');
  if (name) document.getElementById('uname').textContent = name;
  loadLandlordTab('dash');
});
