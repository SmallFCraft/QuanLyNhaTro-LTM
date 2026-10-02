// Ánh xạ Role từ server (Landlord=0, Tenant=1, Police=2, Manager=3) sang trang tác nhân.
// Chủ trọ và Quản lý dùng CHUNG shell `landlord/` — shell tự ẩn/hiện tab theo `?role=`.
const ROLE_ROUTES = {
  0: '../landlord/index.html?role=Landlord',
  1: '../tenant/index.html',
  2: '../police/index.html',
  3: '../landlord/index.html?role=Manager',
  'Landlord': '../landlord/index.html?role=Landlord',
  'Tenant': '../tenant/index.html',
  'Police': '../police/index.html',
  'Manager': '../landlord/index.html?role=Manager'
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
    const target = ROLE_ROUTES[roleVal];
    if (!target) {
      alert('Tài khoản chưa được gán vai trò hợp lệ. Liên hệ Chủ trọ.');
      return;
    }
    const name = encodeURIComponent(user.fullName ?? user.FullName ?? '');
    const sep = target.includes('?') ? '&' : '?';
    window.location.href = `${target}${sep}u=${name}`;
  } catch (err) {
    alert(err.message);
  }
}
