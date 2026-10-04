// Tab Phòng: danh sách + thêm/sửa/xóa. BR-12: chỉ xóa phòng trống.
let _roomsCache = [];
let _activeContractsByRoom = new Set();

async function loadRooms() {
  try {
    const [rooms, contracts] = await Promise.all([
      window.bridge.call('PHONG_LAY_TAT_CA', {}) || [],
      window.bridge.call('HOP_DONG_LAY_TAT_CA', {}).catch(() => []) || []
    ]);
    _roomsCache = rooms || [];
    _activeContractsByRoom = new Set(
      (contracts || [])
        .map(x => x.contract || x.hopDong || x)
        .filter(c => (c.trangThai ?? c.trang_thai) === 'HieuLuc')
        .map(c => Number(c.phongId ?? c.phong_id))
    );
    renderRooms(_roomsCache);
  } catch (err) {
    toast(err.message, 'err');
  }
}

function selectedRoomId() {
  const sel = document.querySelector('#phong-body tr.sel');
  return sel ? Number(sel.dataset.id) : null;
}

function refreshRoomToolbar() {
  const id = selectedRoomId();
  const r = id == null ? null : _roomsCache.find(x => x.id === id) ?? null;
  const editBtn = document.getElementById('btn-edit-room');
  const delBtn = document.getElementById('btn-del-room');
  const full = r != null && (r.soNguoiHienTai ?? 0) > 0;
  if (editBtn) editBtn.disabled = r == null;
  if (delBtn) {
    delBtn.disabled = r == null || full;
    delBtn.title = r == null
      ? 'Chọn một phòng trong bảng để xóa'
      : full ? 'Chỉ xóa được phòng trống (BR-12)' : 'Xóa phòng đang chọn';
  }
}

function renderRooms(rows) {
  const body = document.querySelector('#tab-phong tbody');
  if (!body) return;
  const keep = selectedRoomId();
  const q = (document.getElementById('room-search')?.value || '').toLowerCase().trim();
  const trangThaiFilter = document.getElementById('room-trang_thai')?.value || '';
  const list = (rows || []).filter(r => {
    const st = r.trangThai ?? r.trang_thai;
    return (!q || String(r.soPhong).toLowerCase().includes(q)) &&
           (!trangThaiFilter || st === trangThaiFilter);
  });

  body.innerHTML = list.map(r => {
    const count = r.soNguoiHienTai ?? 0;
    const full = count > 0;
    const hasActiveContract = _activeContractsByRoom.has(Number(r.id));
    // Cảnh báo: phòng có người ở nhưng không còn hợp đồng hiệu lực (vừa bị chấm dứt hoặc chưa ký)
    const treoPhapLy = full && !hasActiveContract;

    const st = r.trangThai ?? r.trang_thai;
    const gia = r.giaThue ?? r.gia_thue;
    const moTa = r.moTa ?? r.mo_ta ?? '';
    const stTag = treoPhapLy
      ? `<span class="tag warn" title="Phòng có người nhưng không có hợp đồng hiệu lực — cần làm thủ tục trả phòng hoặc ký lại hợp đồng."><i class="fas fa-exclamation-triangle"></i> Treo HĐ (${count} người)</span>`
      : `<span class="tag ${st === 'DaThue' ? 'rent' : st === 'BaoTri' ? 'warn' : 'avail'}">
        ${st === 'DaThue' ? 'Đang thuê' : st === 'BaoTri' ? 'Bảo trì' : 'Phòng trống'}</span>`;

    return `<tr data-id="${r.id}"${r.id === keep ? ' class="sel"' : ''} onclick="this.parentNode.querySelectorAll('tr').forEach(x=>x.classList.remove('sel'));this.classList.add('sel');refreshRoomToolbar()">
      <td><b>${esc(r.soPhong)}</b></td>
      <td class="num">${fmtMoney(gia)}</td>
      <td class="num">${r.soNguoiToiDa}</td>
      <td class="num ${full ? 'due-red' : ''}">${count}/${r.soNguoiToiDa}</td>
      <td>${stTag}</td>
      <td>${esc(moTa)}</td>
      <td>
        <button class="btn btn-outline btn-sm" onclick="event.stopPropagation();editRoom(${r.id})"><i class="fas fa-edit"></i></button>
        <button class="btn btn-danger btn-sm" onclick="event.stopPropagation();deleteRoom(${r.id})"${full ? ' disabled title="Chỉ xóa được phòng trống (BR-12)"' : ''}><i class="fas fa-trash"></i></button>
      </td></tr>`;
  }).join('') || '<tr><td colspan="7" style="color:var(--dim)">Chưa có phòng nào.</td></tr>';

  const foot = document.querySelector('#tab-phong .tblfoot span');
  if (foot) foot.textContent = `Tổng: ${list.length} bản ghi`;
  refreshRoomToolbar();
}

function editSelectedRoom() {
  const id = selectedRoomId();
  if (id == null) { toast('Vui lòng chọn một phòng trong bảng.', 'info'); return; }
  editRoom(id);
}

function deleteSelectedRoom() {
  const id = selectedRoomId();
  if (id == null) { toast('Vui lòng chọn một phòng trong bảng.', 'info'); return; }
  deleteRoom(id);
}

document.addEventListener('keydown', e => {
  const pane = document.getElementById('tab-phong');
  if (!pane || pane.hidden) return;
  if (document.querySelector('#modal-root .modal-back, #modal-root .modal')) return;
  const active = document.activeElement;
  if (active && (active.tagName === 'INPUT' || active.tagName === 'TEXTAREA' || active.tagName === 'SELECT')) return;
  if (e.key === 'F1') { e.preventDefault(); addRoom(); }
  if (e.key === 'F2') { e.preventDefault(); editSelectedRoom(); }
});

function addRoom() {
  openModal({
    title: 'Thêm phòng mới',
    fields: [
      { name: 'soPhong', label: 'Số phòng', type: 'text', required: true },
      { name: 'giaThue', label: 'Giá thuê (đ)', type: 'number', required: true },
      { name: 'soNguoiToiDa', label: 'Sức chứa', type: 'number', value: '2', required: true },
      { name: 'moTa', label: 'Mô tả', type: 'text' }
    ],
    onSubmit: async v => {
      await window.bridge.call('PHONG_THEM', {
        soPhong: v.soPhong,
        giaThue: Number(v.giaThue),
        soNguoiToiDa: Number(v.soNguoiToiDa),
        trangThai: 'Trong',
        moTa: v.moTa || null
      });
      toast('Đã thêm phòng ' + v.soPhong, 'ok');
      await loadRooms();
    }
  });
}

function editRoom(id) {
  const r = _roomsCache.find(x => x.id === id);
  if (!r) return;
  const st = r.trangThai ?? r.trang_thai;
  const gia = r.giaThue ?? r.gia_thue;
  const moTa = r.moTa ?? r.mo_ta ?? '';
  openModal({
    title: 'Sửa phòng ' + r.soPhong,
    fields: [
      { name: 'soPhong', label: 'Số phòng', type: 'text', value: r.soPhong, required: true },
      { name: 'giaThue', label: 'Giá thuê (đ)', type: 'number', value: gia, required: true },
      { name: 'soNguoiToiDa', label: 'Sức chứa', type: 'number', value: r.soNguoiToiDa, required: true },
      { name: 'trangThai', label: 'Trạng thái', type: 'select', value: st, options: [
        { value: 'Trong', label: 'Phòng trống' },
        { value: 'DaThue', label: 'Đang thuê' },
        { value: 'BaoTri', label: 'Bảo trì' }] },
      { name: 'moTa', label: 'Mô tả', type: 'text', value: moTa }
    ],
    onSubmit: async v => {
      await window.bridge.call('PHONG_CAP_NHAT', {
        id: r.id,
        soPhong: v.soPhong,
        giaThue: Number(v.giaThue),
        soNguoiToiDa: Number(v.soNguoiToiDa),
        trangThai: v.trangThai,
        moTa: v.moTa || null
      });
      toast('Đã cập nhật phòng ' + v.soPhong, 'ok');
      await loadRooms();
    }
  });
}

async function deleteRoom(id) {
  const r = _roomsCache.find(x => x.id === id);
  if (!r) return;
  if (!await confirmDialog(`Xóa phòng ${r.soPhong}? Thao tác không thể hoàn tác.`)) return;
  try {
    await window.bridge.call('PHONG_XOA', { phongId: r.id });
    toast('Đã xóa phòng ' + r.soPhong, 'ok');
    await loadRooms();
  } catch (err) {
    toast(err.message, 'err');
  }
}
