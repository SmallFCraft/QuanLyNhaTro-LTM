// Tab Phòng: danh sách + thêm/sửa/xóa. BR-12: chỉ xóa phòng trống.
let _roomsCache = [];

async function loadRooms() {
  try {
    _roomsCache = await window.bridge.call('PHONG_LAY_TAT_CA', {}) || [];
    renderRooms(_roomsCache);
  } catch (err) {
    toast(err.message, 'err');
  }
}

function renderRooms(rows) {
  const body = document.querySelector('#tab-phong tbody');
  if (!body) return;
  const q = (document.getElementById('room-search')?.value || '').toLowerCase().trim();
  const trang_thai = document.getElementById('room-trang_thai')?.value || '';
  const list = (rows || []).filter(r =>
    (!q || String(r.soPhong).toLowerCase().includes(q)) &&
    (!trang_thai || r.trang_thai === trang_thai));

  body.innerHTML = list.map(r => {
    const full = (r.soNguoiHienTai ?? 0) > 0;
    return `<tr>
      <td><b>${esc(r.soPhong)}</b></td>
      <td class="num">${fmtMoney(r.gia_thue)}</td>
      <td class="num">${r.soNguoiToiDa}</td>
      <td class="num ${full ? 'due-red' : ''}">${r.soNguoiHienTai ?? 0}/${r.soNguoiToiDa}</td>
      <td><span class="tag ${r.trang_thai === 'DaThue' ? 'rent' : r.trang_thai === 'BaoTri' ? 'warn' : 'avail'}">
        ${r.trang_thai === 'DaThue' ? 'Đang thuê' : r.trang_thai === 'BaoTri' ? 'Bảo trì' : 'Phòng trống'}</span></td>
      <td>${esc(r.mo_ta ?? '')}</td>
      <td>
        <button class="btn btn-outline btn-sm" onclick="editRoom(${r.id})"><i class="fas fa-edit"></i></button>
        <button class="btn btn-danger btn-sm" onclick="deleteRoom(${r.id})"${full ? ' disabled title="Chỉ xóa được phòng trống (BR-12)"' : ''}><i class="fas fa-trash"></i></button>
      </td></tr>`;
  }).join('') || '<tr><td colspan="7" style="color:var(--dim)">Chưa có phòng nào.</td></tr>';

  const foot = document.querySelector('#tab-phong .tblfoot span');
  if (foot) foot.textContent = `Tổng: ${list.length} bản ghi`;
}

function addRoom() {
  openModal({
    title: 'Thêm phòng mới',
    fields: [
      { name: 'soPhong', label: 'Số phòng', type: 'text', required: true },
      { name: 'gia_thue', label: 'Giá thuê (đ)', type: 'number', required: true },
      { name: 'soNguoiToiDa', label: 'Sức chứa', type: 'number', value: '2', required: true },
      { name: 'mo_ta', label: 'Mô tả', type: 'text' }
    ],
    onSubmit: async v => {
      await window.bridge.call('PHONG_THEM', {
        soPhong: v.soPhong,
        gia_thue: Number(v.gia_thue),
        soNguoiToiDa: Number(v.soNguoiToiDa),
        trang_thai: 'Trong',
        mo_ta: v.mo_ta || null
      });
      toast('Đã thêm phòng ' + v.soPhong, 'ok');
      await loadRooms();
    }
  });
}

function editRoom(id) {
  const r = _roomsCache.find(x => x.id === id);
  if (!r) return;
  openModal({
    title: 'Sửa phòng ' + r.soPhong,
    fields: [
      { name: 'soPhong', label: 'Số phòng', type: 'text', value: r.soPhong, required: true },
      { name: 'gia_thue', label: 'Giá thuê (đ)', type: 'number', value: r.gia_thue, required: true },
      { name: 'soNguoiToiDa', label: 'Sức chứa', type: 'number', value: r.soNguoiToiDa, required: true },
      { name: 'trang_thai', label: 'Trạng thái', type: 'select', value: r.trang_thai, options: [
        { value: 'Trong', label: 'Phòng trống' },
        { value: 'DaThue', label: 'Đang thuê' },
        { value: 'BaoTri', label: 'Bảo trì' }] },
      { name: 'mo_ta', label: 'Mô tả', type: 'text', value: r.mo_ta ?? '' }
    ],
    onSubmit: async v => {
      await window.bridge.call('PHONG_CAP_NHAT', {
        id: r.id,
        soPhong: v.soPhong,
        gia_thue: Number(v.gia_thue),
        soNguoiToiDa: Number(v.soNguoiToiDa),
        trang_thai: v.trang_thai,
        mo_ta: v.mo_ta || null
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
