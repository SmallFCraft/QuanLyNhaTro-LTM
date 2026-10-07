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

function switchAuthTab(mode) {
  const dangKy = mode === 'register';
  document.getElementById('formLogin').style.display = dangKy ? 'none' : '';
  document.getElementById('formRegister').style.display = dangKy ? '' : 'none';
  document.getElementById('tabLogin').classList.toggle('on', !dangKy);
  document.getElementById('tabRegister').classList.toggle('on', dangKy);
}

/** BR-17: khách tự đăng ký tài khoản; thành công thì điền sẵn CCCD vào ô đăng nhập. */
let regSubmitting = false;
async function doRegister() {
  if (regSubmitting) return;
  const cccd = document.getElementById('regCccd').value.trim();
  const matKhau = document.getElementById('regMatKhau').value;
  const ngaySinh = document.getElementById('regNgaySinh').value;

  if (!/^\d{12}$/.test(cccd)) {
    alertDialog('Số CCCD phải gồm đúng 12 chữ số.', 'Dữ liệu chưa hợp lệ', 'warn');
    return;
  }
  if (!ngaySinh) {
    alertDialog('Vui lòng chọn ngày sinh.', 'Dữ liệu chưa hợp lệ', 'warn');
    return;
  }
  if (!matKhau || matKhau.length < 6) {
    alertDialog('Mật khẩu phải từ 6 ký tự trở lên.', 'Dữ liệu chưa hợp lệ', 'warn');
    return;
  }

  const btn = document.querySelector('#formRegister button[type="submit"]');
  regSubmitting = true;
  if (btn) btn.disabled = true;
  try {
    await window.bridge.call('DANG_KY', {
      HoTen: document.getElementById('regHoTen').value.trim(),
      NgaySinh: ngaySinh,
      Cccd: cccd,
      SoDienThoai: document.getElementById('regSdt').value.trim(),
      QueQuan: document.getElementById('regQueQuan').value.trim(),
      NoiLamViec: document.getElementById('regNoiLamViec').value.trim() || null,
      matKhau: matKhau
    });

    document.getElementById('lu').value = cccd;
    document.getElementById('lp').value = '';
    document.getElementById('regMatKhau').value = '';
    switchAuthTab('login');
    await alertDialog('Đăng ký thành công. Hãy đăng nhập và quét mã QR của phòng để nhận phòng.', 'Thành công', 'ok');
  } catch (err) {
    alertDialog(err.message, 'Đăng ký không thành công', 'err');
  } finally {
    regSubmitting = false;
    if (btn) btn.disabled = false;
  }
}

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

// Indicator kết nối Server: UI_SERVER_STATUS đọc socket ngay trong host WebView (không gửi gói TCP),
// nên polling không tốn request và vẫn trả lời được khi Server đã chết.
// Không dùng startAutoRefresh: hàm đó bỏ qua lúc activeElement là INPUT — trang đăng nhập gõ gần như luôn.
async function pollServerStatus() {
  if (document.hidden) return;
  const el = document.getElementById('srv-status');
  if (!el) return;
  let connected = false;
  try {
    const st = await window.bridge.call('UI_SERVER_STATUS', {});
    connected = !!(st && st.connected);
  } catch (e) { /* probe hỏng = coi như mất kết nối, để lần poll sau thử lại */ }
  el.dataset.state = connected ? 'on' : 'off';
  el.querySelector('.txt').textContent = connected
    ? 'Đã kết nối máy chủ'
    : 'Mất kết nối máy chủ — hãy mở Server TCP';
}

function startServerStatusPolling(ms = 5000) {
  pollServerStatus();
  setInterval(pollServerStatus, ms);
  document.addEventListener('visibilitychange', () => { if (!document.hidden) pollServerStatus(); });
}
startServerStatusPolling();
