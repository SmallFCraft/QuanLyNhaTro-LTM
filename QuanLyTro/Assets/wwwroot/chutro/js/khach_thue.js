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
    // BR-17/BR-19: khách tự đăng ký có phong_id IS NULL — phải hỏi riêng (phongId: 0),
    // nếu không họ vô hình trong tab này dù màn lập hợp đồng vẫn thấy.
    const [phong, pending] = await Promise.all([
      (async () => (await window.bridge.call('PHONG_LAY_TAT_CA', {})) || [])(),
      (async () => (await window.bridge.call('KHACH_THUE_THEO_PHONG', { phongId: 0 })) || [])()
    ]);
    // ponytail: KHACH_THUE_THEO_PHONG là hanh_dong đọc duy nhất (BR-16) nên phải hỏi từng phòng (N+1).
    // Thêm TENANT_GET_ALL vào ma trận quyền khi số phòng lớn.
    const lists = await Promise.all(phong.map(async r => {
      const khach_thue = (await window.bridge.call('KHACH_THUE_THEO_PHONG', { phongId: r.id })) || [];
      // Gán phongId từ phòng đang hỏi: ta biết chắc họ thuộc phòng này, không phụ thuộc field server trả.
      return khach_thue.map(t => ({ ...t, phongId: t.phongId ?? r.id, soPhong: r.soPhong }));
    }));
    // Khách chờ gán phòng đứng trước danh sách: chủ trọ thấy họ ngay khi mở tab.
    tenantCache = [
      ...pending.map(t => ({ ...t, phongId: null, soPhong: null })),
      ...lists.flat()
    ];
    const cntBadge = document.getElementById('cnt-khach_thue');
    if (cntBadge) cntBadge.textContent = tenantCache.length;
    // Rebuild room filter dropdown, keep selection, có option "Tất cả" + "Chờ nhận phòng".
    if (sel) {
      const keep = sel.value;
      const opts = [
        `<option value="">-- Tất cả (${tenantCache.length} người) --</option>`,
        `<option value="pending">-- Chờ nhận phòng (${pending.length}) --</option>`,
        ...phong.map(r =>
          `<option value="${esc(r.id)}">${esc(r.soPhong)} (${r.soNguoiHienTai ?? 0}/${r.soNguoiToiDa})</option>`)
      ];
      sel.innerHTML = opts.join('');
      if (keep !== undefined && keep !== null && keep !== '') {
        sel.value = keep;
      } else {
        sel.value = '';
      }
    }
    renderTenants();
  } catch (err) {
    toast(err.message, 'err');
  }
  function renderTenants() {
    if (!body) return;
    const filterVal = sel && sel.value ? String(sel.value) : '';
    const rows = filterVal === 'pending'
      ? tenantCache.filter(t => t.phongId == null)
      : (filterVal ? tenantCache.filter(t => String(t.phongId) === filterVal) : tenantCache);

    body.innerHTML = rows.map(t => {
      const hasRoom = t.phongId != null;
      const roomTag = hasRoom
        ? `<span class="tag rent">${esc(t.soPhong || ('P.' + t.phongId))}</span>`
        : `<span class="tag warn">Chưa gán</span>`;
      const statusTag = hasRoom
        ? `<span class="tag done"><i class="fas fa-check-circle"></i> Đang thuê</span>`
        : `<span class="tag warn"><i class="fas fa-clock"></i> Chờ nhận phòng</span>`;

      return `
        <tr data-id="${t.id}" onclick="this.parentNode.querySelectorAll('tr').forEach(r=>r.classList.remove('sel'));this.classList.add('sel')">
          <td>${roomTag}</td>
          <td>${statusTag}</td>
          <td><b>${esc(t.hoTen)}</b></td>
          <td class="mono">${esc(t.cccd)}</td>
          <td class="mono">${fmtDateOnly(t.ngaySinh)}</td>
          <td class="mono">${esc(t.soDienThoai)}</td>
          <td>${esc(t.queQuan)}</td>
          <td><span class="tag ${t.daDangKyTamTru ? 'done' : 'warn'}">${t.daDangKyTamTru ? 'Đã nộp' : 'Chưa nộp'}</span></td>
          <td><button class="btn btn-outline btn-sm" onclick="event.stopPropagation();resetTenantPassword(${t.id})" title="Đặt lại mật khẩu mặc định (6 số cuối CCCD)"><i class="fas fa-key"></i></button></td>
        </tr>`;
    }).join('') || `<tr><td colspan="9" style="text-align:center;color:var(--dim)">Không có người dùng nào</td></tr>`;
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
  { name: 'matKhau', label: 'Mật khẩu', type: 'text', value: '', placeholder: t ? 'Để trống = giữ nguyên' : 'Để trống = 6 số cuối CCCD' },
  { name: 'soDienThoai', label: 'SĐT', value: t?.soDienThoai ?? t?.so_dien_thoai ?? '', required: true },
  { name: 'queQuan', label: 'Quê quán', value: t?.queQuan ?? t?.que_quan ?? '', required: true },
  { name: 'noiLamViec', label: 'Nơi học/làm', value: t?.noiLamViec ?? t?.noi_lam_viec ?? '' },
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
  const pId = values.phongId !== undefined && values.phongId !== null && String(values.phongId).trim() !== ''
    ? Number(values.phongId)
    : null;
  return {
    id: id ?? 0,
    phongId: Number.isFinite(pId) && pId > 0 ? pId : null,
    hoTen: values.hoTen,
    ngaySinh: values.ngaySinh,
    cccd: values.cccd,
    soDienThoai: values.soDienThoai ?? values.so_dien_thoai,
    queQuan: values.queQuan ?? values.que_quan,
    noiLamViec: (values.noiLamViec ?? values.noi_lam_viec) || null,
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
        const pwd = (v.matKhau || '').trim() || null;
        await window.bridge.call('KHACH_THUE_THEM', { tenant: tenantPayload(v), plainPassword: pwd });
        const defaultPwd = String(v.cccd || '').trim().slice(-6);
        toast(`Đã thêm ${v.hoTen}. Đăng nhập: CCCD ${v.cccd} | MK: ${pwd || defaultPwd}`, 'ok');
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

async function editTenant(id) {
  const t = findTenant(id);
  if (!t) { toast('Vui lòng chọn người thuê cần sửa.', 'err'); return; }
  // Khách chờ gán phòng không có id phòng thật — không được build option value "null" (submit ra NaN).
  const phongOptions = t.phongId == null
    ? [{ value: '', label: '— Chưa gán phòng —' }, ...await freeRooms()]
    : [{ value: String(t.phongId), label: t.soPhong || ('P.' + t.phongId) }];
  openModal({
    title: `Sửa — ${t.hoTen}`,
    fields: tenantFields(t, phongOptions),
    onSubmit: async v => {
      const pwd = (v.matKhau || '').trim() || null;
      await window.bridge.call('KHACH_THUE_CAP_NHAT', { tenant: tenantPayload(v, t.id), plainPassword: pwd });
      toast('Đã lưu hồ sơ.', 'ok');
      await loadTenants();
    }
  });
}

async function moveTenant(id) {
  const t = findTenant(id);
  if (!t) { toast('Vui lòng chọn người thuê cần chuyển phòng.', 'err'); return; }
  // Khách chờ nhận phòng chưa có phongId — moveTenant vẫn dùng được (gán phòng đầu tiên),
  // nhưng phải chặn trả phòng để không báo thành công giả.
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
  // Khách chưa được gán phòng thì không có gì để trả — server trả 0 rows và UI sẽ báo "thành công" giả.
  if (t.phongId == null) { toast(`${t.hoTen} chưa được gán vào phòng nào.`, 'warn'); return; }
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
