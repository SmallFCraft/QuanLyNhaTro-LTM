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
  c.innerHTML = Object.entries(ROLE_LABELS).map(([vaiTro, meta]) => {
    const on = vaiTro === _currentRole ? ' active' : '';
    return `<button type="button" class="perms-vai_tro-btn${on}" onclick="selectPermsRole('${vaiTro}')">
      <i class="fas ${meta.icon}"></i>
      <span class="vai_tro-title">${meta.name}</span>
      <span class="vai_tro-desc">${meta.desc}</span>
    </button>`;
  }).join('');
}

function actionsForRole(vaiTro) {
  const map = (_permsData && (_permsData.quyenTheoVaiTro || _permsData.QuyenTheoVaiTro)) || {};
  if (map[vaiTro]) return map[vaiTro];
  const key = Object.keys(map).find(k => k.toLowerCase() === String(vaiTro).toLowerCase());
  return (key && map[key]) || [];
}

function selectPermsRole(vaiTro) {
  _currentRole = vaiTro;
  renderPermsRoleSelector();
  _permsDraft = new Set(actionsForRole(vaiTro));
  renderPermsCheckboxes();
}

function togglePermAction(hanhDong, checked) {
  if (checked) {
    _permsDraft.add(hanhDong);
  } else {
    _permsDraft.delete(hanhDong);
  }
}

function renderPermsCheckboxes() {
  const container = document.getElementById('permsGroups');
  if (!container || !_permsData) return;

  const catalog = _permsData.danhSachHanhDong || _permsData.DanhSachHanhDong || [];

  // Gom nhóm hanhDong theo Nhom
  const groups = {};
  catalog.forEach(item => {
    const nhom = item.nhom || item.Nhom || item.group || 'Khác';
    if (!groups[nhom]) groups[nhom] = [];
    groups[nhom].push(item);
  });

  const html = Object.entries(groups).map(([groupName, items]) => {
    const itemsHtml = items.map(item => {
      const code = item.hanhDong || item.HanhDong || item.action || '';
      const desc = item.moTa || item.MoTa || item.description || code;
      const isChecked = _permsDraft.has(code) ? 'checked' : '';
      return `
        <label class="perm-item">
          <input type="checkbox" ${isChecked} onchange="togglePermAction(this.dataset.code, this.checked)" data-code="${esc(code)}">
          <div class="perm-info">
            <span class="perm-desc">${esc(desc)}</span>
            <code class="perm-hanh_dong">${esc(code)}</code>
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
  const list = Array.from(_permsDraft);
  try {
    const res = await window.bridge.call('PHAN_QUYEN_CAP_NHAT_VAI_TRO', {
      VaiTro: _currentRole,
      DanhSachHanhDong: list
    });
    if (!_permsData.quyenTheoVaiTro) _permsData.quyenTheoVaiTro = {};
    _permsData.quyenTheoVaiTro[_currentRole] = list;
    toast(res?.message || `Đã cập nhật phân quyền cho vai trò ${ROLE_LABELS[_currentRole]?.name || _currentRole}!`, 'ok');
  } catch (err) {
    toast(err.message, 'err');
  }
}
