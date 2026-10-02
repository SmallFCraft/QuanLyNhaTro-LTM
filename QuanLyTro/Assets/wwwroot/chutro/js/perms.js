// Màn hình Phân quyền động (chỉ Chủ trọ được phép truy cập).
let _permsData = null;
let _currentRole = 'QuanLy';
let _permsDraft = new Set();

const ROLE_LABELS = {
  QuanLy: { name: 'Quản lý khu trọ', icon: 'fa-user-tie', desc: 'Nhân viên phụ trách vận hành' },
  CongAn:  { name: 'Công an phường', icon: 'fa-user-shield', desc: 'Cán bộ kiểm tra cư trú' },
  KhachThue:  { name: 'Khách thuê phòng', icon: 'fa-user', desc: 'Người thuê xem hóa đơn' }
};

async function loadPerms() {
  try {
    const res = await window.bridge.call('PHAN_QUYEN_LAY_MA_TRAN', {});
    if (!res) return;
    _permsData = res;
    renderPermsRoleSelector();
    selectPermsRole(_currentRole);
  } catch (err) {
    toast(err.message, 'err');
  }
}

function renderPermsRoleSelector() {
  const c = document.getElementById('permsRoles');
  if (!c || !_permsData) return;
  c.innerHTML = Object.entries(ROLE_LABELS).map(([vai_tro, meta]) => {
    const on = vai_tro === _currentRole ? ' active' : '';
    return `<button type="button" class="perms-vai_tro-btn${on}" onclick="selectPermsRole('${vai_tro}')">
      <i class="fas ${meta.icon}"></i>
      <span class="vai_tro-title">${meta.name}</span>
      <span class="vai_tro-desc">${meta.desc}</span>
    </button>`;
  }).join('');
}

function actionsForRole(vai_tro) {
  const map = (_permsData && _permsData.quyenTheoVaiTro) || {};
  if (map[vai_tro]) return map[vai_tro];
  // Khóa JSON có thể lệch hoa/thường so với ROLE_LABELS ('POLICE' vs 'CongAn') — so khớp không phân biệt.
  const key = Object.keys(map).find(k => k.toLowerCase() === String(vai_tro).toLowerCase());
  return (key && map[key]) || [];
}

function selectPermsRole(vai_tro) {
  _currentRole = vai_tro;
  renderPermsRoleSelector();
  _permsDraft = new Set(actionsForRole(vai_tro));
  renderPermsCheckboxes();
}

function togglePermAction(hanh_dong, checked) {
  if (checked) {
    _permsDraft.add(hanh_dong);
  } else {
    _permsDraft.delete(hanh_dong);
  }
}

function renderPermsCheckboxes() {
  const container = document.getElementById('permsGroups');
  if (!container || !_permsData) return;

  // Gom nhóm hanh_dong theo Nhom
  const groups = {};
  (_permsData.danhSachHanhDong || []).forEach(item => {
    if (!groups[item.group]) groups[item.group] = [];
    groups[item.group].push(item);
  });

  const html = Object.entries(groups).map(([groupName, items]) => {
    const itemsHtml = items.map(item => {
      const isChecked = _permsDraft.has(item.hanh_dong) ? 'checked' : '';
      return `
        <label class="perm-item">
          <input type="checkbox" ${isChecked} onchange="togglePermAction('${item.hanh_dong}', this.checked)">
          <div class="perm-info">
            <span class="perm-desc">${esc(item.mo_ta)}</span>
            <code class="perm-hanh_dong">${esc(item.hanh_dong)}</code>
          </div>
        </label>
      `;
    }).join('');

    return `
      <div class="perm-group-card">
        <div class="perm-group-header">
          <i class="fas fa-folder-open"></i> ${esc(groupName)}
          <span class="perm-group-count">${items.length} quyền</span>
        </div>
        <div class="perm-items-grid">
          ${itemsHtml}
        </div>
      </div>
    `;
  }).join('');

  container.innerHTML = html;
}

function resetPermsDraft() {
  if (!_permsData) return;
  selectPermsRole(_currentRole);
  toast('Đã khôi phục trạng thái ban đầu.', 'info');
}

async function saveRolePermissions() {
  if (!_permsData) return;
  const hanh_dong = Array.from(_permsDraft);
  try {
    const res = await window.bridge.call('PHAN_QUYEN_CAP_NHAT_VAI_TRO', {
      VaiTro: _currentRole,
      Actions: hanh_dong
    });
    // Cập nhật bộ đệm cục bộ
    if (!_permsData.quyenTheoVaiTro) _permsData.quyenTheoVaiTro = {};
    _permsData.quyenTheoVaiTro[_currentRole] = hanh_dong;
    toast(res?.message || `Đã cập nhật phân quyền cho vai trò ${ROLE_LABELS[_currentRole]?.name || _currentRole}!`, 'ok');
  } catch (err) {
    toast(err.message, 'err');
  }
}
