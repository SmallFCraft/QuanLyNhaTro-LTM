// Ánh xạ Role số và chữ từ server (Landlord=0, Tenant=1, Police=2) sang trang tác nhân.
const ROLE_ROUTES = {
  0: '../landlord/index.html',
  1: '../tenant/index.html',
  2: '../police/index.html',
  '0': '../landlord/index.html',
  '1': '../tenant/index.html',
  '2': '../police/index.html',
  'Landlord': '../landlord/index.html',
  'Tenant': '../tenant/index.html',
  'Police': '../police/index.html'
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
    if (!user) return;
    const roleVal = user.role ?? user.Role;
    const target = ROLE_ROUTES[roleVal] || '../landlord/index.html';
    const name = encodeURIComponent(user.fullName ?? user.FullName ?? '');
    window.location.href = `${target}?u=${name}`;
  } catch (err) {
    alert(err.message);
  }
}
