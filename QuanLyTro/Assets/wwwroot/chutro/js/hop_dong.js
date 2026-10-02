// ChuTro "Hợp đồng" tab — full CRUD. Helpers esc/fmtMoney/fmtDateOnly/toast/openModal/confirmDialog from ui.js.

const CONTRACT_STATUS = {
  HieuLuc: { label: 'Đang hiệu lực', tag: 'rent' },
  HetHan: { label: 'Hết hạn', tag: 'bad' },
  ChamDut: { label: 'Đã thanh lý', tag: 'bad' }
};
const EXPIRING_DAYS = 30;

let contractCache = [];
let contractFilter = 'All';

function selectedContractId() {
  const sel = document.querySelector('#hop_dong-body tr.sel');
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
  (new Date(String(c.ngayKetThuc).slice(0, 10)) - new Date(todayIso())) / 86400000);

// One rule drives both the due-red cell and the "Sắp hết ≤ 30 ngày" KPI (brief: ≤ 30).
const isExpiring = c => c.trang_thai === 'HieuLuc' && daysLeft(c) >= 0 && daysLeft(c) <= EXPIRING_DAYS;

// HOP_DONG_LAY_TAT_CA trả về MucHopDongItem { contract, soPhong, representativeName } (Server
// gộp tên phòng + tên đại diện). Phẳng hóa để không phụ thuộc vị trí dữ liệu từng hanh_dong.
const flatten = item => ({ ...(item.contract || item), soPhong: item.soPhong });

async function loadContracts() {
  try {
    contractCache = (await window.bridge.call('HOP_DONG_LAY_TAT_CA', {}) || []).map(flatten);
    buildContractFilter();
    updateContractKpis();
    renderContracts();
  } catch (err) {
    toast(err.message, 'err');
  }
}

function buildContractFilter() {
  const sel = document.querySelector('#tab-hop_dong .toolbar select');
  if (!sel) return;
  const count = {
    All: contractCache.length,
    HieuLuc: contractCache.filter(c => c.trang_thai === 'HieuLuc').length,
    Expiring: contractCache.filter(isExpiring).length,
    Closed: contractCache.filter(c => c.trang_thai === 'ChamDut').length
  };
  sel.innerHTML = [
    ['All', `Tất cả hợp đồng (${count.All})`],
    ['HieuLuc', `Đang hiệu lực (${count.HieuLuc})`],
    ['Expiring', `Sắp hết hạn ≤ ${EXPIRING_DAYS} ngày (${count.Expiring})`],
    ['Closed', `Đã thanh lý (${count.Closed})`]
  ].map(([v, label]) => `<option value="${esc(v)}">${esc(label)}</option>`).join('');
  contractFilter = 'All';
  sel.value = 'All';
}

function filteredContracts() {
  const q = (document.querySelector('#tab-hop_dong .toolbar input.inp')?.value || '').trim().toLowerCase();
  let rows = contractCache;
  if (contractFilter === 'HieuLuc') rows = rows.filter(c => c.trang_thai === 'HieuLuc');
  else if (contractFilter === 'Expiring') rows = rows.filter(isExpiring);
  else if (contractFilter === 'Closed') rows = rows.filter(c => c.trang_thai === 'ChamDut');
  if (q) rows = rows.filter(c =>
    (c.soPhong || '').toLowerCase().includes(q) ||
    (c.representativeName || '').toLowerCase().includes(q) ||
    (c.ghi_chu || '').toLowerCase().includes(q));
  return rows;
}

function updateContractKpis() {
  const vals = document.querySelectorAll('#tab-hop_dong .kpis .kpi .val');
  if (vals.length < 3) return;
  const active = contractCache.filter(c => c.trang_thai === 'HieuLuc');
  const deposit = active.reduce((sum, c) => sum + Number(c.tienCoc || 0), 0);
  vals[0].innerHTML = `${active.length} <span style="font-size:11px;font-weight:400;color:var(--muted)">/ ${contractCache.length} HĐ</span>`;
  vals[1].innerHTML = `${String(contractCache.filter(isExpiring).length).padStart(2, '0')} <span style="font-size:11px;font-weight:400;color:var(--muted)">≤ ${EXPIRING_DAYS} ngày</span>`;
  vals[2].innerHTML = `${esc(fmtMoney(deposit))} <span style="font-size:11px;font-weight:400;color:var(--muted)">đ</span>`;
}

function renderContracts() {
  const body = document.getElementById('hop_dong-body');
  if (!body) return;
  const rows = filteredContracts();
  body.innerHTML = rows.map(c => {
    const left = c.trang_thai === 'HieuLuc' ? daysLeft(c) : null;
    const leftText = left === null ? '—' : left >= 0 ? `${left} ngày` : 'Quá hạn';
    const st = CONTRACT_STATUS[c.trang_thai] || { label: c.trang_thai, tag: 'warn' };
    const urgent = isExpiring(c);
    const tag = urgent ? 'warn' : st.tag;
    const label = urgent ? 'Sắp hết hạn' : st.label;
    return `
      <tr data-id="${c.id}" onclick="this.parentNode.querySelectorAll('tr').forEach(r=>r.classList.remove('sel'));this.classList.add('sel');updateContractFoot()">
        <td><b>${esc(c.soPhong ?? c.phongId)}</b></td>
        <td>${esc(c.representativeName ?? '—')}</td>
        <td class="mono">${fmtDateOnly(c.ngayBatDau)}</td>
        <td class="mono">${fmtDateOnly(c.ngayKetThuc)}</td>
        <td class="num ${urgent ? 'due-red' : ''}">${esc(leftText)}</td>
        <td class="num">${esc(fmtMoney(c.giaThue))}</td>
        <td class="num">${esc(fmtMoney(c.tienCoc))}</td>
        <td><span class="tag ${tag}">${esc(label)}</span></td>
      </tr>`;
  }).join('') || `<tr><td colspan="8">Không có hợp đồng nào</td></tr>`;
  updateContractFoot();
}

function updateContractFoot() {
  const foot = document.querySelector('#tab-hop_dong .tblfoot span');
  if (!foot) return;
  const id = selectedContractId();
  const c = contractCache.find(x => x.id === id);
  foot.innerHTML = `Tổng: ${filteredContracts().length} bản ghi <span class="sep">|</span> <span class="sel-txt">Chọn: ${c ? esc(c.soPhong ?? c.phongId) : '—'}</span>`;
}

function findContract(id) {
  return contractCache.find(c => c.id === Number(id));
}

async function createContract() {
  try {
    const phong = await window.bridge.call('PHONG_LAY_TAT_CA', {}) || [];
    const busy = new Set(contractCache.filter(c => c.trang_thai === 'HieuLuc').map(c => c.phongId));
    const free = phong.filter(r => !busy.has(r.id));
    if (!free.length) { toast('Không có phòng nào còn trống để lập hợp đồng.', 'err'); return; }

    // openModal không hỗ trợ select phụ thuộc -> 2 bước: chọn phòng, rồi mới ra danh sách người đại diện.
    openModal({
      title: 'Lập hợp đồng — chọn phòng',
      fields: [{
        name: 'phongId', label: 'Phòng', type: 'select', value: String(free[0].id),
        options: free.map(r => ({ value: String(r.id), label: `${r.soPhong} (${fmtMoney(r.gia_thue)} đ)` }))
      }],
      onSubmit: async v => openContractForm(Number(v.phongId), phong.find(r => r.id === Number(v.phongId)))
    });
  } catch (err) {
    toast(err.message, 'err');
  }
}

async function openContractForm(phongId, room) {
  const khach_thue = await window.bridge.call('KHACH_THUE_THEO_PHONG', { phongId }) || [];
  if (!khach_thue.length) { toast('Phòng này chưa có người thuê.', 'err'); return; }
  openModal({
    title: `Hợp đồng — ${room.soPhong}`,
    fields: [
      {
        name: 'nguoiDaiDienId', label: 'Người đại diện', type: 'select', value: String(khach_thue[0].id),
        options: khach_thue.map(t => ({ value: String(t.id), label: `${t.hoTen} (${t.cccd})` }))
      },
      { name: 'ngayBatDau', label: 'Ngày bắt đầu', type: 'date', value: todayIso(), required: true },
      { name: 'ngayKetThuc', label: 'Ngày kết thúc', type: 'date', value: plusYearIso(), required: true },
      { name: 'giaThue', label: 'Giá thuê', type: 'number', value: String(room.gia_thue ?? ''), required: true },
      { name: 'tienCoc', label: 'Tiền cọc', type: 'number', value: '0' },
      { name: 'ghi_chu', label: 'Ghi chú', value: '' }
    ],
    onSubmit: async v => {
      await window.bridge.call('HOP_DONG_TAO', {
        id: 0,
        phongId,
        nguoiDaiDienId: Number(v.nguoiDaiDienId),
        ngayBatDau: v.ngayBatDau,
        ngayKetThuc: v.ngayKetThuc,
        giaThue: Number(v.giaThue),
        tienCoc: Number(v.tienCoc || 0),
        trang_thai: 'HieuLuc',
        ghi_chu: v.ghi_chu || null
      });
      toast(`Đã lập hợp đồng cho phòng ${room.soPhong}.`, 'ok');
      await loadContracts();
    }
  });
}

function renewContract(id) {
  const c = findContract(id);
  if (!c) { toast('Vui lòng chọn hợp đồng cần gia hạn.', 'err'); return; }
  openModal({
    title: `Gia hạn — phòng ${c.soPhong ?? c.phongId}`,
    fields: [{ name: 'newEndDate', label: 'Ngày kết thúc mới', type: 'date', value: plusYearIso(), required: true }],
    onSubmit: async v => {
      await window.bridge.call('HOP_DONG_GIA_HAN', { hopDongId: c.id, newEndDate: v.newEndDate });
      toast('Đã gia hạn hợp đồng.', 'ok');
      await loadContracts();
    }
  });
}

function terminateContract(id) {
  const c = findContract(id);
  if (!c) { toast('Vui lòng chọn hợp đồng cần chấm dứt.', 'err'); return; }
  openModal({
    title: `Chấm dứt — phòng ${c.soPhong ?? c.phongId}`,
    fields: [{ name: 'ghi_chu', label: 'Lý do chấm dứt', value: c.ghi_chu ?? '' }],
    onSubmit: async v => {
      await window.bridge.call('HOP_DONG_CHAM_DUT', { hopDongId: c.id, ghi_chu: v.ghi_chu || null });
      toast('Đã chấm dứt hợp đồng.', 'ok');
      await loadContracts();
    }
  });
}

// Toolbar #tab-hop_dong không có id -> bám theo vị trí, không sửa index.html.
document.addEventListener('DOMContentLoaded', () => {
  document.querySelector('#tab-hop_dong .toolbar select')
    ?.addEventListener('change', e => { contractFilter = e.target.value; renderContracts(); });
  document.querySelector('#tab-hop_dong .toolbar input.inp')
    ?.addEventListener('input', renderContracts);
});
