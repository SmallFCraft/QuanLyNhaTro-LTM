// ChuTro "Người thuê" tab — full CRUD. Helpers esc/toast/openModal/confirmDialog from ui.js.

let tenantCache = [];

function selectedTenantId() {
  const sel = document.querySelector('#khach_thue-body tr.sel');
  return sel ? Number(sel.dataset.id) : null;
}

async function loadTenants() {
  const sel = document.getElementById('tenant-room');
  const body = document.getElementById('khach_thue-body');
  try {
    const phong = await window.bridge.call('PHONG_LAY_TAT_CA', {}) || [];
    // ponytail: KHACH_THUE_THEO_PHONG là hanh_dong đọc duy nhất (BR-16) nên phải hỏi từng phòng (N+1).
    // Thêm TENANT_GET_ALL vào ma trận quyền khi số phòng lớn.
    const all = [];
    for (const r of phong) {
      const khach_thue = await window.bridge.call('KHACH_THUE_THEO_PHONG', { phongId: r.id }) || [];
      for (const t of khach_thue) all.push({ ...t, soPhong: r.soPhong });
    }
    tenantCache = all;
    // Rebuild room filter dropdown, keep selection.
    if (sel) {
      const keep = sel.value;
      sel.innerHTML = phong.map(r =>
        `<option value="${esc(r.id)}">${esc(r.soPhong)} (${r.soNguoiHienTai ?? 0}/${r.soNguoiToiDa})</option>`).join('');
      if (keep) sel.value = keep;
      if (!sel.value && phong.length) sel.value = String(phong[0].id);
    }
    renderTenants();
  } catch (err) {
    toast(err.message, 'err');
  }
  function renderTenants() {
    if (!body) return;
    const phongId = sel && sel.value ? String(sel.value) : '';
    const rows = phongId
      ? tenantCache.filter(t => String(t.phongId) === phongId)
      : tenantCache;
    body.innerHTML = rows.map(t => `
      <tr data-id="${t.id}" onclick="this.parentNode.querySelectorAll('tr').forEach(r=>r.classList.remove('sel'));this.classList.add('sel')">
        <td><b>${esc(t.hoTen)}</b></td>
        <td class="mono">${esc(t.cccd)}</td>
        <td class="mono">${fmtDateOnly(t.ngaySinh)}</td>
        <td class="mono">${esc(t.so_dien_thoai)}</td>
        <td>${esc(t.que_quan)}</td>
        <td><span class="tag ${t.daDangKyTamTru ? 'done' : 'warn'}">${t.daDangKyTamTru ? 'Đã nộp' : 'Chưa nộp'}</span></td>
        <td><button class="btn btn-outline btn-sm" onclick="event.stopPropagation();resetTenantPassword(${t.id})"><i class="fas fa-key"></i></button></td>
      </tr>`).join('') || `<tr><td colspan="7">Không có người thuê nào</td></tr>`;
    updateTenantFoot(rows);
  }
}

function updateTenantFoot(rows) {
  const foot = document.querySelector('#tab-khach_thue .tblfoot span');
  if (!foot) return;
  const sel = tenantCache.find(t => t.id === selectedTenantId());
  foot.innerHTML = `Tổng: ${rows.length} bản ghi <span class="sep">|</span> <span class="sel-txt">Chọn: ${sel ? esc(sel.hoTen) : '—'}</span>`;
}

const tenantFields = (t, phong) => [
  { name: 'hoTen', label: 'Họ tên', value: t?.hoTen ?? '', required: true },
  { name: 'ngaySinh', label: 'Ngày sinh', type: 'date', value: (t?.ngaySinh ?? '').slice(0, 10), required: true },
  { name: 'cccd', label: 'CCCD (12 số)', value: t?.cccd ?? '', required: true },
  { name: 'so_dien_thoai', label: 'SĐT', value: t?.so_dien_thoai ?? '', required: true },
  { name: 'que_quan', label: 'Quê quán', value: t?.que_quan ?? '', required: true },
  { name: 'noi_lam_viec', label: 'Nơi học/làm', value: t?.noi_lam_viec ?? '' },
  {
    name: 'phongId', label: 'Phòng', type: 'select', value: String(t?.phongId ?? phong[0]?.value ?? ''),
    options: phong.map(r => ({ value: r.value, label: r.label }))
  },
  {
    name: 'daDangKyTamTru', label: 'Tạm trú', type: 'select',
    value: t?.daDangKyTamTru ? 'true' : 'false',
    options: [{ value: 'false', label: 'Chưa nộp' }, { value: 'true', label: 'Đã nộp' }]
  }
];

function tenantPayload(values, id) {
  return {
    id: id ?? 0,
    phongId: Number(values.phongId),
    hoTen: values.hoTen,
    ngaySinh: values.ngaySinh,
    cccd: values.cccd,
    so_dien_thoai: values.so_dien_thoai,
    que_quan: values.que_quan,
    noi_lam_viec: values.noi_lam_viec || null,
    daDangKyTamTru: values.daDangKyTamTru === 'true'
  };
}

async function freeRooms() {
  const phong = await window.bridge.call('PHONG_LAY_TAT_CA', {}) || [];
  return phong
    .filter(r => (r.soNguoiHienTai ?? 0) < r.soNguoiToiDa)
    .map(r => ({ value: String(r.id), label: `${r.soPhong} (${r.soNguoiHienTai ?? 0}/${r.soNguoiToiDa})` }));
}

async function addTenant() {
  try {
    const phong = await freeRooms();
    if (!phong.length) { toast('Không còn phòng trống.', 'err'); return; }
    openModal({
      title: 'Thêm người thuê',
      fields: tenantFields(null, phong),
      onSubmit: async v => {
        await window.bridge.call('KHACH_THUE_THEM', { tenant: tenantPayload(v), plainPassword: null });
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
    title: `Sửa — ${t.hoTen}`,
    fields: tenantFields(t, [{ value: String(t.phongId), label: t.soPhong }]),
    onSubmit: async v => {
      await window.bridge.call('KHACH_THUE_CAP_NHAT', { tenant: tenantPayload(v, t.id), plainPassword: null });
      toast('Đã lưu hồ sơ.', 'ok');
      await loadTenants();
    }
  });
}

async function moveTenant(id) {
  const t = findTenant(id);
  if (!t) { toast('Vui lòng chọn người thuê cần chuyển phòng.', 'err'); return; }
  try {
    const phong = await freeRooms();
    const target = phong.filter(r => r.value !== String(t.phongId));
    if (!target.length) { toast('Không còn phòng trống để chuyển.', 'err'); return; }
    openModal({
      title: `Chuyển phòng — ${t.hoTen}`,
      fields: [{ name: 'phongId', label: 'Phòng mới', type: 'select', value: target[0].value, options: target }],
      onSubmit: async v => {
        await window.bridge.call('KHACH_THUE_CAP_NHAT', { tenant: { ...t, phongId: Number(v.phongId) }, plainPassword: null });
        toast(`Đã chuyển ${t.hoTen}.`, 'ok');
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
  if (!await confirmDialog(`Cho ${t.hoTen} trả phòng?`)) return;
  try {
    await window.bridge.call('KHACH_THUE_TRA_PHONG', { khachThueId: t.id });
    toast(`Đã cho ${t.hoTen} trả phòng.`, 'ok');
    await loadTenants();
  } catch (err) {
    toast(err.message, 'err');
  }
}

async function deleteTenant(id) {
  const t = findTenant(id);
  if (!t) { toast('Vui lòng chọn hồ sơ cần xóa.', 'err'); return; }
  if (!await confirmDialog(`Xóa hồ sơ của ${t.hoTen}? Chỉ xóa được hồ sơ đã trả phòng.`)) return;
  try {
    await window.bridge.call('KHACH_THUE_XOA', { khachThueId: t.id });
    toast(`Đã xóa hồ sơ ${t.hoTen}.`, 'ok');
    await loadTenants();
  } catch (err) {
    toast(err.message, 'err');
  }
}

async function resetTenantPassword(id) {
  const t = findTenant(id);
  if (!t) return;
  const digits = String(t.cccd || '').replace(/\D/g, '');
  const pwd = digits.length > 6 ? digits.slice(-6) : digits;
  try {
    await window.bridge.call('KHACH_THUE_CAP_NHAT', { tenant: t, plainPassword: pwd });
    toast(`Đã đặt lại mật khẩu mặc định cho ${t.hoTen}.`, 'ok');
  } catch (err) {
    toast(err.message, 'err');
  }
}
