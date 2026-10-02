// Màn hình Phân quyền động (chỉ Chủ trọ được phép truy cập).
let _permsData = null;
let _currentRole = 'Manager';
let _permsDraft = new Set();

const ROLE_LABELS = {
  Manager: { name: 'Quản lý khu trọ', icon: 'fa-user-tie', desc: 'Nhân viên phụ trách vận hành' },
  Police:  { name: 'Công an phường', icon: 'fa-user-shield', desc: 'Cán bộ kiểm tra cư trú' },
  Tenant:  { name: 'Khách thuê phòng', icon: 'fa-user', desc: 'Người thuê xem hóa đơn' }
};

async function loadPerms() {
  try {
    const res = await window.bridge.call('PERMISSION_GET_MATRIX', {});
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
  c.innerHTML = Object.entries(ROLE_LABELS).map(([role, meta]) => {
    const on = role === _currentRole ? ' active' : '';
    return `<button type="button" class="perms-role-btn${on}" onclick="selectPermsRole('${role}')">
      <i class="fas ${meta.icon}"></i>
      <span class="role-title">${meta.name}</span>
      <span class="role-desc">${meta.desc}</span>
    </button>`;
  }).join('');
}

function selectPermsRole(role) {
  _currentRole = role;
  renderPermsRoleSelector();
  const currentActions = (_permsData.roleActions && _permsData.roleActions[role]) || [];
  _permsDraft = new Set(currentActions);
  renderPermsCheckboxes();
}

function togglePermAction(action, checked) {
  if (checked) {
    _permsDraft.add(action);
  } else {
    _permsDraft.delete(action);
  }
}

function renderPermsCheckboxes() {
  const container = document.getElementById('permsGroups');
  if (!container || !_permsData) return;

  // Gom nhóm action theo Group
  const groups = {};
  (_permsData.availableActions || []).forEach(item => {
    if (!groups[item.group]) groups[item.group] = [];
    groups[item.group].push(item);
  });

  const html = Object.entries(groups).map(([groupName, items]) => {
    const itemsHtml = items.map(item => {
      const isChecked = _permsDraft.has(item.action) ? 'checked' : '';
      return `
        <label class="perm-item">
          <input type="checkbox" ${isChecked} onchange="togglePermAction('${item.action}', this.checked)">
          <div class="perm-info">
            <span class="perm-desc">${esc(item.description)}</span>
            <code class="perm-action">${esc(item.action)}</code>
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
  const actions = Array.from(_permsDraft);
  try {
    const res = await window.bridge.call('PERMISSION_UPDATE_ROLE', {
      Role: _currentRole,
      Actions: actions
    });
    // Cập nhật bộ đệm cục bộ
    if (!_permsData.roleActions) _permsData.roleActions = {};
    _permsData.roleActions[_currentRole] = actions;
    toast(res?.message || `Đã cập nhật phân quyền cho vai trò ${ROLE_LABELS[_currentRole]?.name || _currentRole}!`, 'ok');
  } catch (err) {
    toast(err.message, 'err');
  }
}
