// Landlord "Người thuê" tab — full CRUD. Helpers esc/toast/openModal/confirmDialog from ui.js.

let tenantCache = [];

function selectedTenantId() {
  const sel = document.querySelector('#tenants-body tr.sel');
  return sel ? Number(sel.dataset.id) : null;
}

async function loadTenants() {
  const sel = document.getElementById('tenant-room');
  const body = document.getElementById('tenants-body');
  try {
    const rooms = await window.bridge.call('ROOM_GET_ALL', {}) || [];
    // ponytail: TENANT_GET_BY_ROOM là action đọc duy nhất (BR-16) nên phải hỏi từng phòng (N+1).
    // Thêm TENANT_GET_ALL vào ma trận quyền khi số phòng lớn.
    const all = [];
    for (const r of rooms) {
      const tenants = await window.bridge.call('TENANT_GET_BY_ROOM', { roomId: r.id }) || [];
      for (const t of tenants) all.push({ ...t, roomNumber: r.roomNumber });
    }
    tenantCache = all;
    // Rebuild room filter dropdown, keep selection.
    if (sel) {
      const keep = sel.value;
      sel.innerHTML = rooms.map(r =>
        `<option value="${esc(r.id)}">${esc(r.roomNumber)} (${r.currentOccupants ?? 0}/${r.maxOccupants})</option>`).join('');
      if (keep) sel.value = keep;
      if (!sel.value && rooms.length) sel.value = String(rooms[0].id);
    }
    renderTenants();
  } catch (err) {
    toast(err.message, 'err');
  }
  function renderTenants() {
    if (!body) return;
    const roomId = sel && sel.value ? String(sel.value) : '';
    const rows = roomId
      ? tenantCache.filter(t => String(t.roomId) === roomId)
      : tenantCache;
    body.innerHTML = rows.map(t => `
      <tr data-id="${t.id}" onclick="this.parentNode.querySelectorAll('tr').forEach(r=>r.classList.remove('sel'));this.classList.add('sel')">
        <td><b>${esc(t.fullName)}</b></td>
        <td class="mono">${esc(t.idCard)}</td>
        <td class="mono">${fmtDateOnly(t.dateOfBirth)}</td>
        <td class="mono">${esc(t.phone)}</td>
        <td>${esc(t.hometown)}</td>
        <td><span class="tag ${t.isTemporaryRegistered ? 'done' : 'warn'}">${t.isTemporaryRegistered ? 'Đã nộp' : 'Chưa nộp'}</span></td>
        <td><button class="btn btn-outline btn-sm" onclick="event.stopPropagation();resetTenantPassword(${t.id})"><i class="fas fa-key"></i></button></td>
      </tr>`).join('') || `<tr><td colspan="7">Không có người thuê nào</td></tr>`;
    updateTenantFoot(rows);
  }
}

function updateTenantFoot(rows) {
  const foot = document.querySelector('#tab-tenants .tblfoot span');
  if (!foot) return;
  const sel = tenantCache.find(t => t.id === selectedTenantId());
  foot.innerHTML = `Tổng: ${rows.length} bản ghi <span class="sep">|</span> <span class="sel-txt">Chọn: ${sel ? esc(sel.fullName) : '—'}</span>`;
}

const tenantFields = (t, rooms) => [
  { name: 'fullName', label: 'Họ tên', value: t?.fullName ?? '', required: true },
  { name: 'dateOfBirth', label: 'Ngày sinh', type: 'date', value: (t?.dateOfBirth ?? '').slice(0, 10), required: true },
  { name: 'idCard', label: 'CCCD (12 số)', value: t?.idCard ?? '', required: true },
  { name: 'phone', label: 'SĐT', value: t?.phone ?? '', required: true },
  { name: 'hometown', label: 'Quê quán', value: t?.hometown ?? '', required: true },
  { name: 'workplace', label: 'Nơi học/làm', value: t?.workplace ?? '' },
  {
    name: 'roomId', label: 'Phòng', type: 'select', value: String(t?.roomId ?? rooms[0]?.value ?? ''),
    options: rooms.map(r => ({ value: r.value, label: r.label }))
  },
  {
    name: 'isTemporaryRegistered', label: 'Tạm trú', type: 'select',
    value: t?.isTemporaryRegistered ? 'true' : 'false',
    options: [{ value: 'false', label: 'Chưa nộp' }, { value: 'true', label: 'Đã nộp' }]
  }
];

function tenantPayload(values, id) {
  return {
    id: id ?? 0,
    roomId: Number(values.roomId),
    fullName: values.fullName,
    dateOfBirth: values.dateOfBirth,
    idCard: values.idCard,
    phone: values.phone,
    hometown: values.hometown,
    workplace: values.workplace || null,
    isTemporaryRegistered: values.isTemporaryRegistered === 'true'
  };
}

async function freeRooms() {
  const rooms = await window.bridge.call('ROOM_GET_ALL', {}) || [];
  return rooms
    .filter(r => (r.currentOccupants ?? 0) < r.maxOccupants)
    .map(r => ({ value: String(r.id), label: `${r.roomNumber} (${r.currentOccupants ?? 0}/${r.maxOccupants})` }));
}

async function addTenant() {
  try {
    const rooms = await freeRooms();
    if (!rooms.length) { toast('Không còn phòng trống.', 'err'); return; }
    openModal({
      title: 'Thêm người thuê',
      fields: tenantFields(null, rooms),
      onSubmit: async v => {
        await window.bridge.call('TENANT_ADD', { tenant: tenantPayload(v), plainPassword: null });
        toast('Đã thêm người thuê.', 'ok');
        await loadTenants();
      }
    });
  } catch (err) {
    toast(err.message, 'err');
  }
}

function findTenant(id) {
  return tenantCache.find(t => t.id === Number(id));
}

function editTenant(id) {
  const t = findTenant(id);
  if (!t) { toast('Vui lòng chọn người thuê cần sửa.', 'err'); return; }
  openModal({
    title: `Sửa — ${t.fullName}`,
    fields: tenantFields(t, [{ value: String(t.roomId), label: t.roomNumber }]),
    onSubmit: async v => {
      await window.bridge.call('TENANT_UPDATE', { tenant: tenantPayload(v, t.id), plainPassword: null });
      toast('Đã lưu hồ sơ.', 'ok');
      await loadTenants();
    }
  });
}

async function moveTenant(id) {
  const t = findTenant(id);
  if (!t) { toast('Vui lòng chọn người thuê cần chuyển phòng.', 'err'); return; }
  try {
    const rooms = await freeRooms();
    const target = rooms.filter(r => r.value !== String(t.roomId));
    if (!target.length) { toast('Không còn phòng trống để chuyển.', 'err'); return; }
    openModal({
      title: `Chuyển phòng — ${t.fullName}`,
      fields: [{ name: 'roomId', label: 'Phòng mới', type: 'select', value: target[0].value, options: target }],
      onSubmit: async v => {
        await window.bridge.call('TENANT_UPDATE', { tenant: { ...t, roomId: Number(v.roomId) }, plainPassword: null });
        toast(`Đã chuyển ${t.fullName}.`, 'ok');
        await loadTenants();
      }
    });
  } catch (err) {
    toast(err.message, 'err');
  }
}

async function checkoutTenant(id) {
  const t = findTenant(id);
  if (!t) { toast('Vui lòng chọn người thuê cần trả phòng.', 'err'); return; }
  if (!await confirmDialog(`Cho ${t.fullName} trả phòng?`)) return;
  try {
    await window.bridge.call('TENANT_CHECKOUT', { tenantId: t.id });
    toast(`Đã cho ${t.fullName} trả phòng.`, 'ok');
    await loadTenants();
  } catch (err) {
    toast(err.message, 'err');
  }
}

async function deleteTenant(id) {
  const t = findTenant(id);
  if (!t) { toast('Vui lòng chọn hồ sơ cần xóa.', 'err'); return; }
  if (!await confirmDialog(`Xóa hồ sơ của ${t.fullName}? Chỉ xóa được hồ sơ đã trả phòng.`)) return;
  try {
    await window.bridge.call('TENANT_DELETE', { tenantId: t.id });
    toast(`Đã xóa hồ sơ ${t.fullName}.`, 'ok');
    await loadTenants();
  } catch (err) {
    toast(err.message, 'err');
  }
}

async function resetTenantPassword(id) {
  const t = findTenant(id);
  if (!t) return;
  const digits = String(t.idCard || '').replace(/\D/g, '');
  const pwd = digits.length > 6 ? digits.slice(-6) : digits;
  try {
    await window.bridge.call('TENANT_UPDATE', { tenant: t, plainPassword: pwd });
    toast(`Đã đặt lại mật khẩu mặc định cho ${t.fullName}.`, 'ok');
  } catch (err) {
    toast(err.message, 'err');
  }
}
