// Ánh xạ Role số và chữ từ server (Landlord=0, Tenant=1, Police=2) sang shell.
const ROLE_TO_SHELL = {
  0: 'landlord',
  1: 'tenant',
  2: 'police',
  '0': 'landlord',
  '1': 'tenant',
  '2': 'police',
  'Landlord': 'landlord',
  'Tenant': 'tenant',
  'Police': 'police',
  'landlord': 'landlord',
  'tenant': 'tenant',
  'police': 'police'
};

async function doLogin() {
  const username = document.getElementById('lu').value.trim();
  const password = document.getElementById('lp').value;
  if (!username || !password) {
    alert('Vui lòng nhập tài khoản và mật khẩu.');
    return;
  }
  try {
    const user = await window.bridge.call('AUTH_LOGIN', { Username: username, Password: password });
    enterWorkspace(user);
  } catch (err) {
    alert(err.message);
  }
}

function enterWorkspace(user) {
  if (!user) return;
  const roleVal = user.role ?? user.Role;
  const shell = ROLE_TO_SHELL[roleVal] || 'landlord';
  showShell(shell);
  const name = user.fullName ?? user.FullName ?? '';
  const uname = document.getElementById('uname');
  if (uname) uname.textContent = name;
  const pname = document.getElementById('pname');
  if (pname) pname.textContent = name;
  if (shell === 'landlord' && typeof loadLandlordTab === 'function') loadLandlordTab('dash');
  if (shell === 'police' && typeof loadPoliceTab === 'function') loadPoliceTab('citizens');
  if (shell === 'tenant' && typeof loadTenantInvoices === 'function') loadTenantInvoices();
}

function logout() {
  showShell('login');
}
