// Tab Phòng: danh sách + thêm/sửa/xóa. BR-12: chỉ xóa phòng trống.
let _roomsCache = [];

async function loadRooms() {
  try {
    _roomsCache = await window.bridge.call('ROOM_GET_ALL', {}) || [];
    renderRooms(_roomsCache);
  } catch (err) {
    toast(err.message, 'err');
  }
}

function renderRooms(rows) {
  const body = document.querySelector('#tab-rooms tbody');
  if (!body) return;
  const q = (document.getElementById('room-search')?.value || '').toLowerCase().trim();
  const status = document.getElementById('room-status')?.value || '';
  const list = (rows || []).filter(r =>
    (!q || String(r.roomNumber).toLowerCase().includes(q)) &&
    (!status || r.status === status));

  body.innerHTML = list.map(r => {
    const full = (r.currentOccupants ?? 0) > 0;
    return `<tr>
      <td><b>${esc(r.roomNumber)}</b></td>
      <td class="num">${fmtMoney(r.price)}</td>
      <td class="num">${r.maxOccupants}</td>
      <td class="num ${full ? 'due-red' : ''}">${r.currentOccupants ?? 0}/${r.maxOccupants}</td>
      <td><span class="tag ${r.status === 'Rented' ? 'rent' : r.status === 'Maintenance' ? 'warn' : 'avail'}">
        ${r.status === 'Rented' ? 'Đang thuê' : r.status === 'Maintenance' ? 'Bảo trì' : 'Phòng trống'}</span></td>
      <td>${esc(r.description ?? '')}</td>
      <td>
        <button class="btn btn-outline btn-sm" onclick="editRoom(${r.id})"><i class="fas fa-edit"></i></button>
        <button class="btn btn-danger btn-sm" onclick="deleteRoom(${r.id})"${full ? ' disabled title="Chỉ xóa được phòng trống (BR-12)"' : ''}><i class="fas fa-trash"></i></button>
      </td></tr>`;
  }).join('') || '<tr><td colspan="7" style="color:var(--dim)">Chưa có phòng nào.</td></tr>';

  const foot = document.querySelector('#tab-rooms .tblfoot span');
  if (foot) foot.textContent = `Tổng: ${list.length} bản ghi`;
}

function addRoom() {
  openModal({
    title: 'Thêm phòng mới',
    fields: [
      { name: 'roomNumber', label: 'Số phòng', type: 'text', required: true },
      { name: 'price', label: 'Giá thuê (đ)', type: 'number', required: true },
      { name: 'maxOccupants', label: 'Sức chứa', type: 'number', value: '2', required: true },
      { name: 'description', label: 'Mô tả', type: 'text' }
    ],
    onSubmit: async v => {
      await window.bridge.call('ROOM_ADD', {
        roomNumber: v.roomNumber,
        price: Number(v.price),
        maxOccupants: Number(v.maxOccupants),
        status: 'Available',
        description: v.description || null
      });
      toast('Đã thêm phòng ' + v.roomNumber, 'ok');
      await loadRooms();
    }
  });
}

function editRoom(id) {
  const r = _roomsCache.find(x => x.id === id);
  if (!r) return;
  openModal({
    title: 'Sửa phòng ' + r.roomNumber,
    fields: [
      { name: 'roomNumber', label: 'Số phòng', type: 'text', value: r.roomNumber, required: true },
      { name: 'price', label: 'Giá thuê (đ)', type: 'number', value: r.price, required: true },
      { name: 'maxOccupants', label: 'Sức chứa', type: 'number', value: r.maxOccupants, required: true },
      { name: 'status', label: 'Trạng thái', type: 'select', value: r.status, options: [
        { value: 'Available', label: 'Phòng trống' },
        { value: 'Rented', label: 'Đang thuê' },
        { value: 'Maintenance', label: 'Bảo trì' }] },
      { name: 'description', label: 'Mô tả', type: 'text', value: r.description ?? '' }
    ],
    onSubmit: async v => {
      await window.bridge.call('ROOM_UPDATE', {
        id: r.id,
        roomNumber: v.roomNumber,
        price: Number(v.price),
        maxOccupants: Number(v.maxOccupants),
        status: v.status,
        description: v.description || null
      });
      toast('Đã cập nhật phòng ' + v.roomNumber, 'ok');
      await loadRooms();
    }
  });
}

async function deleteRoom(id) {
  const r = _roomsCache.find(x => x.id === id);
  if (!r) return;
  if (!await confirmDialog(`Xóa phòng ${r.roomNumber}? Thao tác không thể hoàn tác.`)) return;
  try {
    await window.bridge.call('ROOM_DELETE', { roomId: r.id });
    toast('Đã xóa phòng ' + r.roomNumber, 'ok');
    await loadRooms();
  } catch (err) {
    toast(err.message, 'err');
  }
}
