// ChuTro "Hợp đồng" tab — full CRUD. Helpers esc/fmtMoney/fmtDateOnly/toast/openModal/confirmDialog from ui.js.

const CONTRACT_STATUS = {
  ChoNhanPhong: { label: 'Chờ nhận phòng', tag: 'warn' },
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

const plusDaysIso = (days = 30) => {
  const d = new Date();
  d.setDate(d.getDate() + Number(days));
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
};

const daysLeft = c => Math.round(
  (new Date(String(c.ngayKetThuc ?? c.endDate).slice(0, 10)) - new Date(todayIso())) / 86400000);

const isExpiring = c => (c.trangThai ?? c.trang_thai) === 'HieuLuc' && daysLeft(c) >= 0 && daysLeft(c) <= EXPIRING_DAYS;

// MucHopDongItem: { contract/hopDong, soPhong/roomNumber, representativeName/tenNguoiDaiDien }
const flatten = item => {
  const inner = item.contract || item.hopDong || item.HopDong || item;
  return {
    ...inner,
    soPhong: item.soPhong ?? item.roomNumber ?? inner.soPhong,
    representativeName: item.representativeName ?? item.tenNguoiDaiDien ?? inner.representativeName ?? inner.tenNguoiDaiDien ?? '—',
    trangThai: inner.trangThai ?? inner.trang_thai,
    giaThue: inner.giaThue ?? inner.gia_thue,
    tienCoc: inner.tienCoc ?? inner.tien_coc,
    ghiChu: inner.ghiChu ?? inner.ghi_chu
  };
};

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
    ChoNhanPhong: contractCache.filter(c => (c.trangThai ?? c.trang_thai) === 'ChoNhanPhong').length,
    HieuLuc: contractCache.filter(c => (c.trangThai ?? c.trang_thai) === 'HieuLuc').length,
    Expiring: contractCache.filter(isExpiring).length,
    Closed: contractCache.filter(c => (c.trangThai ?? c.trang_thai) === 'ChamDut').length
  };
  sel.innerHTML = [
    ['All', `Tất cả hợp đồng (${count.All})`],
    ['ChoNhanPhong', `Chờ nhận phòng (${count.ChoNhanPhong})`],
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
  if (contractFilter === 'ChoNhanPhong') rows = rows.filter(c => (c.trangThai ?? c.trang_thai) === 'ChoNhanPhong');
  else if (contractFilter === 'HieuLuc') rows = rows.filter(c => (c.trangThai ?? c.trang_thai) === 'HieuLuc');
  else if (contractFilter === 'Expiring') rows = rows.filter(isExpiring);
  else if (contractFilter === 'Closed') rows = rows.filter(c => (c.trangThai ?? c.trang_thai) === 'ChamDut');
  if (q) rows = rows.filter(c =>
    (c.soPhong || '').toLowerCase().includes(q) ||
    (c.representativeName || '').toLowerCase().includes(q) ||
    (c.ghiChu || c.ghi_chu || '').toLowerCase().includes(q));
  return rows;
}

function updateContractKpis() {
  const vals = document.querySelectorAll('#tab-hop_dong .kpis .kpi .val');
  if (vals.length < 3) return;
  const active = contractCache.filter(c => (c.trangThai ?? c.trang_thai) === 'HieuLuc');
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
    const stVal = c.trangThai ?? c.trang_thai;
    const left = stVal === 'HieuLuc' ? daysLeft(c) : null;
    const leftText = left === null ? '—' : left >= 0 ? `${left} ngày` : 'Quá hạn';
    const st = CONTRACT_STATUS[stVal] || { label: stVal, tag: 'warn' };
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
    const busy = new Set(contractCache.filter(c => {
      const st = c.trangThai ?? c.trang_thai;
      return st === 'HieuLuc' || st === 'ChoNhanPhong';
    }).map(c => c.phongId));
    const free = phong.filter(r => !busy.has(r.id));
    if (!free.length) { toast('Không có phòng nào còn trống để lập hợp đồng.', 'err'); return; }

    openModal({
      title: 'Lập hợp đồng — chọn phòng',
      fields: [{
        name: 'phongId', label: 'Phòng', type: 'select', value: String(free[0].id),
        options: free.map(r => ({ value: String(r.id), label: `${r.soPhong} (${fmtMoney(r.giaThue ?? r.gia_thue)} đ)` }))
      }],
      onSubmit: async v => openContractForm(Number(v.phongId), phong.find(r => r.id === Number(v.phongId)))
    });
  } catch (err) {
    toast(err.message, 'err');
  }
}

async function openContractForm(phongId, room) {
  // Khách đang ở phòng này + khách tự đăng ký chưa có phòng (chờ nhận phòng bằng QR).
  const inRoom = await window.bridge.call('KHACH_THUE_THEO_PHONG', { phongId }) || [];
  const pending = await window.bridge.call('KHACH_THUE_THEO_PHONG', { phongId: 0 }) || [];
  const khach_thue = [...inRoom, ...pending];
  if (!khach_thue.length) { toast('Chưa có người thuê nào để lập hợp đồng.', 'err'); return; }

  const roomPrice = room.giaThue ?? room.gia_thue ?? '';
  openModal({
    title: `Hợp đồng — ${room.soPhong}`,
    fields: [
      {
        name: 'nguoiDaiDienId', label: 'Người đại diện', type: 'select', value: String(khach_thue[0].id),
        options: khach_thue.map(t => ({ value: String(t.id), label: `${t.hoTen} (${t.cccd})` }))
      },
      { name: 'ngayBatDau', label: 'Ngày bắt đầu', type: 'date', value: todayIso(), required: true },
      { name: 'ngayKetThuc', label: 'Ngày kết thúc', type: 'date', value: plusYearIso(), required: true },
      { name: 'giaThue', label: 'Giá thuê', type: 'number', value: String(roomPrice), required: true },
      { name: 'tienCoc', label: 'Tiền cọc', type: 'number', value: '0' },
      { name: 'choQr', label: 'Chờ khách quét QR nhận phòng', type: 'select', value: '1',
        options: [{ value: '1', label: 'Có — sinh mã QR / PIN' }, { value: '0', label: 'Không — bàn giao trực tiếp' }] },
      { name: 'ghiChu', label: 'Ghi chú', value: '' }
    ],
    onSubmit: async v => {
      const choQr = v.choQr === '1';
      await window.bridge.call('HOP_DONG_TAO', {
        id: 0,
        phongId,
        nguoiDaiDienId: Number(v.nguoiDaiDienId),
        ngayBatDau: v.ngayBatDau,
        ngayKetThuc: v.ngayKetThuc,
        giaThue: Number(v.giaThue),
        tienCoc: Number(v.tienCoc || 0),
        trangThai: choQr ? 'ChoNhanPhong' : 'HieuLuc',
        ghiChu: v.ghiChu || null
      });
      toast(choQr
        ? `Đã lập hợp đồng chờ nhận phòng cho ${room.soPhong}. Bấm "Xem QR / PIN" để gửi mã cho khách.`
        : `Đã lập hợp đồng cho phòng ${room.soPhong}.`, 'ok');
      await loadContracts();
    }
  });
}

/** BR-18: hiện QR + PIN dùng một lần cho hợp đồng đang chờ nhận phòng. */
async function showContractQr(id) {
  if (!id) { toast('Hãy chọn một hợp đồng trước.', 'warn'); return; }
  try {
    const qr = await window.bridge.call('HOP_DONG_SINH_QR', { hopDongId: Number(id) });
    const c = findContract(id);
    // findContract trả về item đã flatten() nên dùng trường representativeName (không phải tenNguoiDaiDien).
    const tenDaiDien = c ? (c.representativeName || c.tenNguoiDaiDien || '') : '';
    const title = c ? `Mã nhận phòng — ${c.soPhong || ''} ${tenDaiDien}`.trim() : 'Mã nhận phòng';

    const root = document.getElementById('modal-root');
    const back = document.createElement('div');
    back.className = 'modal-back';
    back.innerHTML = `
      <div class="modal">
        <div class="modal-hdr">${esc(title)}</div>
        <div class="modal-body" style="text-align:center">
          <div id="qrBox" style="min-height:220px;display:flex;align-items:center;justify-content:center"></div>
          <div style="margin-top:10px">
            <div class="muted" style="font-size:12px">Mã PIN dự phòng (nhập tay nếu không quét được)</div>
            <div class="mono" id="qrPin" style="font-size:24px;letter-spacing:6px;font-weight:700"></div>
          </div>
          <div class="muted" style="font-size:12px;margin-top:8px">
            Mã dùng một lần — khách đăng nhập bằng CCCD người đại diện rồi quét hoặc nhập PIN.
          </div>
          <div class="modal-ft">
            <button type="button" class="btn btn-outline btn-sm" data-cancel>Đóng</button>
            <button type="button" class="btn btn-outline btn-sm" data-download><i class="fas fa-download"></i> Tải ảnh QR</button>
            <button type="button" class="btn btn-primary btn-sm" data-print><i class="fas fa-print"></i> In phiếu</button>
          </div>
        </div>
      </div>`;

    const close = () => back.remove();
    back.querySelector('[data-cancel]').addEventListener('click', close);
    back.addEventListener('click', e => { if (e.target === back) close(); });
    root.appendChild(back);

    const img = document.createElement('img');
    img.alt = 'Mã QR nhận phòng';
    img.width = 220;
    img.height = 220;
    buildQrImage(img, qr.qrDataString);
    back.querySelector('#qrBox').appendChild(img);
    back.querySelector('#qrPin').textContent = qr.maPin;

    back.querySelector('[data-download]').addEventListener('click', () => {
      const a = document.createElement('a');
      a.href = img.src;
      a.download = `QR-${qr.hopDongId}.png`;
      a.click();
    });
    back.querySelector('[data-print]').addEventListener('click', () => printQrCard(title, img.src, qr.maPin));
  } catch (err) {
    toast(err.message, 'err');
  }
}

/** Vẽ QR từ thư viện qrcode-generator (qrcode(0,'M')) lên <img>. */
function buildQrImage(img, payload) {
  const qr = qrcode(0, 'M');
  qr.addData(payload);
  qr.make();
  img.src = qr.createDataURL(6, 8);
}

let qrPrintFrame = null;
/** In phiếu QR + PIN bằng iframe ẩn trong trang (hoạt động ổn định trên WebView2 không bị chặn popup). */
function printQrCard(title, imgSrc, pin) {
  if (!qrPrintFrame || !qrPrintFrame.isConnected) {
    qrPrintFrame = document.createElement('iframe');
    qrPrintFrame.style.cssText = 'position:fixed;right:0;bottom:0;width:0;height:0;border:0;visibility:hidden';
    document.body.appendChild(qrPrintFrame);
  }
  const frame = qrPrintFrame;
  const doc = frame.contentWindow?.document;
  if (!doc) {
    toast('Không mở được khung in.', 'err');
    return;
  }
  doc.open();
  doc.write(`
    <!DOCTYPE html><html><head><meta charset="utf-8"><title>${esc(title)}</title></head>
    <body style="font-family:system-ui,-apple-system,sans-serif;text-align:center;padding:24px">
      <h3>${esc(title)}</h3>
      <img src="${imgSrc}" width="240" height="240" style="display:block;margin:12px auto">
      <p style="font-size:12px;color:#666;margin:8px 0 4px">Mã PIN nhận phòng</p>
      <p style="font-size:28px;letter-spacing:6px;font-weight:700;margin:0">${esc(pin)}</p>
    </body></html>`);
  doc.close();
  setTimeout(() => {
    try {
      frame.contentWindow.focus();
      frame.contentWindow.print();
    } catch {
      toast('Không thể kích hoạt hộp thoại in.', 'err');
    }
  }, 200);
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
  const conHan = (c.trangThai ?? c.trang_thai) === 'ChoNhanPhong' || daysLeft(c) > 0;

  const fields = [];
  if (conHan) {
    fields.push({
      name: 'ngayBanGiao',
      label: 'Ngày bàn giao dự kiến (quy định báo trước ≥ 30 ngày)',
      type: 'date',
      value: plusDaysIso(30),
      required: true
    });
  }
  fields.push({
    name: 'ghiChu',
    label: conHan
      ? 'Lý do chấm dứt trước hạn (bắt buộc: nợ tiền 3 tháng, vi phạm nội quy...)'
      : 'Lý do chấm dứt (tùy chọn)',
    value: c.ghiChu ?? c.ghi_chu ?? '',
    required: conHan,
    placeholder: conHan ? 'Nhập căn cứ chấm dứt hợp đồng...' : 'Để trống nếu không có ghi chú'
  });

  openModal({
    title: `Chấm dứt — phòng ${c.soPhong ?? c.phongId}${conHan ? ' (trước hạn)' : ''}`,
    fields,
    onSubmit: async v => {
      let ghiChuCombined = (v.ghiChu || '').trim();
      if (conHan && v.ngayBanGiao) {
        ghiChuCombined = `[Bàn giao dự kiến: ${v.ngayBanGiao}] ${ghiChuCombined}`.trim();
      }
      await window.bridge.call('HOP_DONG_CHAM_DUT', { hopDongId: c.id, ghi_chu: ghiChuCombined || null });
      toast('Đã chấm dứt hợp đồng.', 'ok');
      await loadContracts();
    }
  });
}

document.addEventListener('DOMContentLoaded', () => {
  document.querySelector('#tab-hop_dong .toolbar select')
    ?.addEventListener('change', e => { contractFilter = e.target.value; renderContracts(); });
  document.querySelector('#tab-hop_dong .toolbar input.inp')
    ?.addEventListener('input', renderContracts);
});
