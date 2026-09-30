// Landlord "Hợp đồng" tab — full CRUD. Helpers esc/fmtMoney/fmtDateOnly/toast/openModal/confirmDialog from ui.js.

const CONTRACT_STATUS = {
  Active: { label: 'Đang hiệu lực', tag: 'rent' },
  Expired: { label: 'Hết hạn', tag: 'bad' },
  Terminated: { label: 'Đã thanh lý', tag: 'bad' }
};
const EXPIRING_DAYS = 30;

let contractCache = [];
let contractFilter = 'All';

function selectedContractId() {
  const sel = document.querySelector('#contracts-body tr.sel');
  return sel ? Number(sel.dataset.id) : null;
}

const todayIso = () => {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
};

const plusYearIso = () => {
  const d = new Date();
  d.setFullYear(d.getFullYear() + 1);
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
};

const daysLeft = c => Math.round(
  (new Date(String(c.endDate).slice(0, 10)) - new Date(todayIso())) / 86400000);

// One rule drives both the due-red cell and the "Sắp hết ≤ 30 ngày" KPI (brief: ≤ 30).
const isExpiring = c => c.status === 'Active' && daysLeft(c) >= 0 && daysLeft(c) <= EXPIRING_DAYS;

// CONTRACT_GET_ALL trả về ContractListItem { contract, roomNumber, representativeName } (Server
// gộp tên phòng + tên đại diện). Phẳng hóa để không phụ thuộc vị trí dữ liệu từng action.
const flatten = item => ({ ...(item.contract || item), roomNumber: item.roomNumber });

async function loadContracts() {
  try {
    contractCache = (await window.bridge.call('CONTRACT_GET_ALL', {}) || []).map(flatten);
    buildContractFilter();
    updateContractKpis();
    renderContracts();
  } catch (err) {
    toast(err.message, 'err');
  }
}

function buildContractFilter() {
  const sel = document.querySelector('#tab-contracts .toolbar select');
  if (!sel) return;
  const count = {
    All: contractCache.length,
    Active: contractCache.filter(c => c.status === 'Active').length,
    Expiring: contractCache.filter(isExpiring).length,
    Closed: contractCache.filter(c => c.status === 'Terminated').length
  };
  sel.innerHTML = [
    ['All', `Tất cả hợp đồng (${count.All})`],
    ['Active', `Đang hiệu lực (${count.Active})`],
    ['Expiring', `Sắp hết hạn ≤ ${EXPIRING_DAYS} ngày (${count.Expiring})`],
    ['Closed', `Đã thanh lý (${count.Closed})`]
  ].map(([v, label]) => `<option value="${v}">${esc(label)}</option>`).join('');
  contractFilter = 'All';
  sel.value = 'All';
}

function filteredContracts() {
  const q = (document.querySelector('#tab-contracts .toolbar input.inp')?.value || '').trim().toLowerCase();
  let rows = contractCache;
  if (contractFilter === 'Active') rows = rows.filter(c => c.status === 'Active');
  else if (contractFilter === 'Expiring') rows = rows.filter(isExpiring);
  else if (contractFilter === 'Closed') rows = rows.filter(c => c.status === 'Terminated');
  if (q) rows = rows.filter(c =>
    (c.roomNumber || '').toLowerCase().includes(q) ||
    (c.representativeName || '').toLowerCase().includes(q) ||
    (c.notes || '').toLowerCase().includes(q));
  return rows;
}

function updateContractKpis() {
  const vals = document.querySelectorAll('#tab-contracts .kpis .kpi .val');
  if (vals.length < 3) return;
  const active = contractCache.filter(c => c.status === 'Active');
  const deposit = active.reduce((sum, c) => sum + Number(c.depositAmount || 0), 0);
  vals[0].innerHTML = `${active.length} <span style="font-size:11px;font-weight:400;color:var(--muted)">/ ${contractCache.length} HĐ</span>`;
  vals[1].innerHTML = `${String(contractCache.filter(isExpiring).length).padStart(2, '0')} <span style="font-size:11px;font-weight:400;color:var(--muted)">≤ ${EXPIRING_DAYS} ngày</span>`;
  vals[2].innerHTML = `${esc(fmtMoney(deposit))} <span style="font-size:11px;font-weight:400;color:var(--muted)">đ</span>`;
}

function renderContracts() {
  const body = document.getElementById('contracts-body');
  if (!body) return;
  const rows = filteredContracts();
  body.innerHTML = rows.map(c => {
    const left = c.status === 'Active' ? daysLeft(c) : null;
    const leftText = left === null ? '—' : left >= 0 ? `${left} ngày` : 'Quá hạn';
    const st = CONTRACT_STATUS[c.status] || { label: c.status, tag: 'warn' };
    const urgent = isExpiring(c);
    const tag = urgent ? 'warn' : st.tag;
    const label = urgent ? 'Sắp hết hạn' : st.label;
    return `
      <tr data-id="${c.id}" onclick="this.parentNode.querySelectorAll('tr').forEach(r=>r.classList.remove('sel'));this.classList.add('sel');updateContractFoot()">
        <td><b>${esc(c.roomNumber ?? c.roomId)}</b></td>
        <td>${esc(c.representativeName ?? '—')}</td>
        <td class="mono">${fmtDateOnly(c.startDate)}</td>
        <td class="mono">${fmtDateOnly(c.endDate)}</td>
        <td class="num ${urgent ? 'due-red' : ''}">${esc(leftText)}</td>
        <td class="num">${esc(fmtMoney(c.rentalPrice))}</td>
        <td class="num">${esc(fmtMoney(c.depositAmount))}</td>
        <td><span class="tag ${tag}">${esc(label)}</span></td>
      </tr>`;
  }).join('') || `<tr><td colspan="8">Không có hợp đồng nào</td></tr>`;
  updateContractFoot();
}

function updateContractFoot() {
  const foot = document.querySelector('#tab-contracts .tblfoot span');
  if (!foot) return;
  const id = selectedContractId();
  const c = contractCache.find(x => x.id === id);
  foot.innerHTML = `Tổng: ${filteredContracts().length} bản ghi <span class="sep">|</span> <span class="sel-txt">Chọn: ${c ? esc(c.roomNumber ?? c.roomId) : '—'}</span>`;
}

function findContract(id) {
  return contractCache.find(c => c.id === Number(id));
}

async function createContract() {
  try {
    const rooms = await window.bridge.call('ROOM_GET_ALL', {}) || [];
    const busy = new Set(contractCache.filter(c => c.status === 'Active').map(c => c.roomId));
    const free = rooms.filter(r => !busy.has(r.id));
    if (!free.length) { toast('Không có phòng nào còn trống để lập hợp đồng.', 'err'); return; }

    // openModal không hỗ trợ select phụ thuộc -> 2 bước: chọn phòng, rồi mới ra danh sách người đại diện.
    openModal({
      title: 'Lập hợp đồng — chọn phòng',
      fields: [{
        name: 'roomId', label: 'Phòng', type: 'select', value: String(free[0].id),
        options: free.map(r => ({ value: String(r.id), label: `${r.roomNumber} (${fmtMoney(r.price)} đ)` }))
      }],
      onSubmit: async v => openContractForm(Number(v.roomId), rooms.find(r => r.id === Number(v.roomId)))
    });
  } catch (err) {
    toast(err.message, 'err');
  }
}

async function openContractForm(roomId, room) {
  const tenants = await window.bridge.call('TENANT_GET_BY_ROOM', { roomId }) || [];
  if (!tenants.length) { toast('Phòng này chưa có người thuê.', 'err'); return; }
  openModal({
    title: `Hợp đồng — ${room.roomNumber}`,
    fields: [
      {
        name: 'representativeTenantId', label: 'Người đại diện', type: 'select', value: String(tenants[0].id),
        options: tenants.map(t => ({ value: String(t.id), label: `${t.fullName} (${t.idCard})` }))
      },
      { name: 'startDate', label: 'Ngày bắt đầu', type: 'date', value: todayIso(), required: true },
      { name: 'endDate', label: 'Ngày kết thúc', type: 'date', value: plusYearIso(), required: true },
      { name: 'rentalPrice', label: 'Giá thuê', type: 'number', value: String(room.price ?? ''), required: true },
      { name: 'depositAmount', label: 'Tiền cọc', type: 'number', value: '0' },
      { name: 'notes', label: 'Ghi chú', value: '' }
    ],
    onSubmit: async v => {
      await window.bridge.call('CONTRACT_CREATE', {
        id: 0,
        roomId,
        representativeTenantId: Number(v.representativeTenantId),
        startDate: v.startDate,
        endDate: v.endDate,
        rentalPrice: Number(v.rentalPrice),
        depositAmount: Number(v.depositAmount || 0),
        status: 'Active',
        notes: v.notes || null
      });
      toast(`Đã lập hợp đồng cho phòng ${room.roomNumber}.`, 'ok');
      await loadContracts();
    }
  });
}

function renewContract(id) {
  const c = findContract(id);
  if (!c) { toast('Vui lòng chọn hợp đồng cần gia hạn.', 'err'); return; }
  openModal({
    title: `Gia hạn — phòng ${c.roomNumber ?? c.roomId}`,
    fields: [{ name: 'newEndDate', label: 'Ngày kết thúc mới', type: 'date', value: plusYearIso(), required: true }],
    onSubmit: async v => {
      await window.bridge.call('CONTRACT_RENEW', { contractId: c.id, newEndDate: v.newEndDate });
      toast('Đã gia hạn hợp đồng.', 'ok');
      await loadContracts();
    }
  });
}

function terminateContract(id) {
  const c = findContract(id);
  if (!c) { toast('Vui lòng chọn hợp đồng cần chấm dứt.', 'err'); return; }
  openModal({
    title: `Chấm dứt — phòng ${c.roomNumber ?? c.roomId}`,
    fields: [{ name: 'notes', label: 'Lý do chấm dứt', value: c.notes ?? '' }],
    onSubmit: async v => {
      await window.bridge.call('CONTRACT_TERMINATE', { contractId: c.id, notes: v.notes || null });
      toast('Đã chấm dứt hợp đồng.', 'ok');
      await loadContracts();
    }
  });
}

// Toolbar #tab-contracts không có id -> bám theo vị trí, không sửa index.html.
document.addEventListener('DOMContentLoaded', () => {
  document.querySelector('#tab-contracts .toolbar select')
    ?.addEventListener('change', e => { contractFilter = e.target.value; renderContracts(); });
  document.querySelector('#tab-contracts .toolbar input.inp')
    ?.addEventListener('input', renderContracts);
});
