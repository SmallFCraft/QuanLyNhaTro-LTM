const LANDLORD_TITLES = {
  'dash': 'Tổng quan vận hành cơ sở',
  'rooms': 'Danh sách phòng trọ',
  'tenants': 'Hồ sơ khách thuê',
  'contracts': 'Hợp đồng & khách thuê',
  'utils': 'Chốt chỉ số điện nước',
  'invoices': 'Hóa đơn & thu tiền',
  'reports': 'Thống kê doanh thu'
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
    if (tabKey === 'rooms') renderRooms(await window.bridge.call('ROOM_GET_ALL', {}));
    if (tabKey === 'invoices') renderInvoices(await window.bridge.call('INVOICE_GET_ALL', {}));
    if (tabKey === 'reports') await loadReports();
  } catch (err) {
    alert(err.message);
  }
}

function renderRooms(rows) {
  const body = document.querySelector('#tab-rooms tbody');
  if (!body) return;
  body.innerHTML = (rows || []).map(r => `
    <tr>
      <td><b>${r.roomNumber}</b></td>
      <td class="num amount">${(r.price || 0).toLocaleString('vi-VN')}</td>
      <td class="num">${r.maxOccupants}</td>
      <td class="num">${r.currentOccupants ?? 0}</td>
      <td><span class="tag ${r.status === 'Rented' ? 'rent' : 'avail'}">${r.status}</span></td>
    </tr>`).join('');
}

function renderInvoices(rows) {
  const body = document.querySelector('#tab-invoices tbody');
  if (!body) return;
  body.innerHTML = (rows || []).map(i => `
    <tr>
      <td><b>${i.roomNumber}</b></td>
      <td class="mono">${i.billingMonth}</td>
      <td class="num amount">${(i.totalAmount || 0).toLocaleString('vi-VN')}</td>
      <td><span class="tag ${i.isPaid ? 'paid' : 'unpaid'}">${i.isPaid ? 'Đã thanh toán' : 'Chưa thanh toán'}</span></td>
    </tr>`).join('');
}

async function loadReports() {
  const summary = await window.bridge.call('REPORT_SUMMARY', {});
  // Đổ số liệu KPI vào #tab-reports; trường hợp null thì để trống
  document.querySelectorAll('#tab-reports [data-kpi]').forEach(el => {
    const key = el.dataset.kpi;
    if (summary && summary[key] != null) el.textContent = summary[key].toLocaleString('vi-VN');
  });
}
