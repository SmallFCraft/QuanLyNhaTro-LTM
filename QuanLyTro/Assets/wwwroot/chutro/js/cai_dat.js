// Tab Cài đặt: tài khoản nhận tiền → ảnh QR SePay (https://qr.sepay.vn/img, public, không cần token).

// Mã ngắn SePay chấp nhận cho tham số `bank`. Không phải lựa chọn sẵn thì cho phép nhập tay.
const NGAN_HANG_SEPAY = [
  'Vietcombank', 'MBBank', 'Techcombank', 'ACB', 'BIDV', 'VPBank', 'TPBank',
  'Sacombank', 'VietinBank', 'Agribank', 'LPBank', 'SHB', 'HDBank', 'VIB',
  'SeABank', 'MSB', 'OCB', 'Eximbank', 'KienLongBank', 'NCB'
];

// sepayQrUrl()/boDauMemo() nằm ở shared/js/ui.js — dùng chung với shell khách thuê.

let _cauHinh = null;

async function loadCaiDat() {
  const sel = document.getElementById('cfg-ngan_hang');
  if (sel && sel.options.length === 0) {
    sel.innerHTML = '<option value="">— Chọn ngân hàng —</option>' +
      NGAN_HANG_SEPAY.map(b => `<option value="${esc(b)}">${esc(b)}</option>`).join('');
  }
  try {
    // Bản đầy đủ có api_token — chỉ Chủ trọ gọi được. Chưa có token thì form vẫn dùng bản thường.
    _cauHinh = (await window.bridge.call('CAU_HINH_DAY_DU', {})) || null;
  } catch {
    try {
      _cauHinh = (await window.bridge.call('CAU_HINH_LAY', {})) || null;
    } catch (err) {
      toast(err.message, 'err');
      return;
    }
  }
  const token = document.getElementById('cfg-api_token');
  if (token) token.value = _cauHinh?.apiToken || '';
  const clear = document.getElementById('cfg-token-clear');
  if (clear) clear.disabled = !_cauHinh?.coApiToken;
  const state = document.getElementById('cfg-token-state');
  if (state) {
    state.textContent = _cauHinh?.coApiToken
      ? 'Đã lưu token — để trống ô trên nếu không đổi.'
      : 'Chưa có token. QR tĩnh vẫn dùng được; token chỉ cần cho SePay API.';
  }
  if (_cauHinh) {
    if (sel && _cauHinh.nganHang && !NGAN_HANG_SEPAY.includes(_cauHinh.nganHang)) {
      sel.insertAdjacentHTML('beforeend', `<option value="${esc(_cauHinh.nganHang)}">${esc(_cauHinh.nganHang)}</option>`);
    }
    if (sel) sel.value = _cauHinh.nganHang || '';
    const stk = document.getElementById('cfg-so_tai_khoan');
    if (stk) stk.value = _cauHinh.soTaiKhoan || '';
    const chu = document.getElementById('cfg-chu_tai_khoan');
    if (chu) chu.value = _cauHinh.chuTaiKhoan || '';
    const tpl = document.getElementById('cfg-template_qr');
    if (tpl) tpl.value = _cauHinh.templateQr ?? 'compact';
  }
  renderCauHinhPreview();
}

function docFormCauHinh() {
  return {
    nganHang: document.getElementById('cfg-ngan_hang')?.value || '',
    soTaiKhoan: (document.getElementById('cfg-so_tai_khoan')?.value || '').trim(),
    chuTaiKhoan: (document.getElementById('cfg-chu_tai_khoan')?.value || '').trim(),
    templateQr: document.getElementById('cfg-template_qr')?.value ?? 'compact'
  };
}

function renderCauHinhPreview() {
  const box = document.getElementById('cfg-qr-preview');
  if (!box) return;
  const url = sepayQrUrl(docFormCauHinh());
  box.innerHTML = url
    ? `<img class="qr-img" src="${esc(url)}" alt="Mã QR nhận tiền"
         onerror="this.replaceWith(Object.assign(document.createElement('div'),{className:'pie-empty',textContent:'Không tải được ảnh QR — kiểm tra ngân hàng và số tài khoản.'}))">`
    : '<div class="pie-empty">Nhập ngân hàng và số tài khoản để xem trước.</div>';
}

async function saveCauHinh() {
  const form = docFormCauHinh();
  const note = document.getElementById('cfg-note');
  const tokenInput = document.getElementById('cfg-api_token');
  // Ô token để trống = KHÔNG gửi apiToken → server giữ token cũ.
  const payload = {
    nganHang: form.nganHang,
    soTaiKhoan: form.soTaiKhoan,
    chuTaiKhoan: form.chuTaiKhoan,
    templateQr: form.templateQr
  };
  const token = tokenInput?.value.trim();
  if (token) payload.apiToken = token;

  try {
    const saved = await window.bridge.call('CAU_HINH_CAP_NHAT', payload);
    _cauHinh = saved || form;
    // Server chuẩn hoá chủ tài khoản (bỏ dấu, in hoa) — hiện lại giá trị đã lưu, không phải giá trị vừa gõ.
    const chu = document.getElementById('cfg-chu_tai_khoan');
    if (chu && _cauHinh.chuTaiKhoan !== undefined) chu.value = _cauHinh.chuTaiKhoan;
    if (note) note.textContent = `Đã lưu lúc ${new Date().toLocaleTimeString('vi-VN')}`;
    renderCauHinhPreview();
    await loadCaiDat(); // nạp lại để cập nhật trạng thái token
    toast('Đã lưu cài đặt nhận tiền.', 'ok');
  } catch (err) {
    toast(err.message, 'err');
  }
}

async function chayDoiSoat() {
  const note = document.getElementById('cfg-doi-soat-note');
  if (note) note.textContent = 'Đang đối soát...';
  try {
    const kq = await window.bridge.call('DOI_SOAT_CHAY', {});
    const soMoi = kq?.soGiaoDichMoi ?? 0;
    const soKhop = kq?.soKhop ?? 0;
    const soBoQua = kq?.soBoQua ?? 0;
    if (note) {
      note.textContent = soMoi === 0
        ? 'Không có giao dịch mới.'
        : `${soMoi} giao dịch mới: ${soKhop} khớp, ${soBoQua} bỏ qua.`;
    }
    if (soKhop > 0) toast(`Đã tự thu ${soKhop} hóa đơn từ chuyển khoản.`, 'ok');
  } catch (err) {
    if (note) note.textContent = 'Đối soát lỗi.';
    toast(err.message, 'err');
  }
}

function toggleTokenVisibility() {
  const input = document.getElementById('cfg-api_token');
  const eye = document.getElementById('cfg-token-eye');
  if (!input) return;
  const hidden = input.type === 'password';
  input.type = hidden ? 'text' : 'password';
  if (eye) eye.className = hidden ? 'fas fa-eye-slash' : 'fas fa-eye';
}

async function xoaApiToken() {
  if (!await confirmDialog('Xoá API token SePay? QR tĩnh vẫn hoạt động, chỉ mất quyền gọi SePay API.')) return;
  const form = docFormCauHinh();
  try {
    await window.bridge.call('CAU_HINH_CAP_NHAT', {
      nganHang: form.nganHang,
      soTaiKhoan: form.soTaiKhoan,
      chuTaiKhoan: form.chuTaiKhoan,
      templateQr: form.templateQr,
      apiToken: ''
    });
    const input = document.getElementById('cfg-api_token');
    if (input) input.value = '';
    await loadCaiDat();
    toast('Đã xoá API token.', 'ok');
  } catch (err) {
    toast(err.message, 'err');
  }
}

document.addEventListener('DOMContentLoaded', () => {
  for (const id of ['cfg-ngan_hang', 'cfg-so_tai_khoan', 'cfg-chu_tai_khoan', 'cfg-template_qr']) {
    document.getElementById(id)?.addEventListener('input', renderCauHinhPreview);
    document.getElementById(id)?.addEventListener('change', renderCauHinhPreview);
  }
});

// ponytail: mỗi keystroke dựng lại <img> → SePay nhận 1 request/ảnh. Đổi sang debounce 300ms nếu log thấy quá tải.
