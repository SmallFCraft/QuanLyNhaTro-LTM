// Ánh xạ VaiTro từ server (ChuTro=0, KhachThue=1, CongAn=2, QuanLy=3) sang trang tác nhân.
// Chủ trọ và Quản lý dùng CHUNG shell `chutro/` — shell tự ẩn/hiện tab theo `?vai_tro=`.
const ROLE_ROUTES = {
  0: '../chutro/index.html?vai_tro=ChuTro',
  1: '../khachthue/index.html',
  2: '../congan/index.html',
  3: '../chutro/index.html?vai_tro=QuanLy',
  'ChuTro': '../chutro/index.html?vai_tro=ChuTro',
  'KhachThue': '../khachthue/index.html',
  'CongAn': '../congan/index.html',
  'QuanLy': '../chutro/index.html?vai_tro=QuanLy'
};

async function doLogin() {
  const tenDangNhap = document.getElementById('lu').value.trim();
  const matKhau = document.getElementById('lp').value;
  if (!tenDangNhap || !matKhau) {
    alertDialog('Vui lòng nhập tài khoản và mật khẩu.', 'Thiếu thông tin', 'warn');
    return;
  }
  try {
    const user = await window.bridge.call('DANG_NHAP', { TenDangNhap: tenDangNhap, MatKhau: matKhau });
    if (!user) return;
    const vaiTro = user.vaiTro;
    const target = ROLE_ROUTES[vaiTro];
    if (!target) {
      alertDialog('Tài khoản chưa được gán vai trò hợp lệ. Vui lòng liên hệ Chủ trọ.', 'Không thể truy cập', 'err');
      return;
    }
    const name = encodeURIComponent(user.hoTen ?? user.HoTen ?? '');
    const sep = target.includes('?') ? '&' : '?';
    window.location.href = `${target}${sep}u=${name}`;
  } catch (err) {
    alertDialog(err.message, 'Đăng nhập không thành công', 'err');
  }
}
