// ponytail: shell khách thuê — server tự suy ra phòng từ token. Phân trang phía server (page/soLuongMoiTrang).
// ponytail: chưa có hồ sơ nhận tiền trên server → hằng tĩnh rỗng. Chuyển sang bảng cấu hình server khi có.
const BANK_INFO = '';
let currentTenantInvoices = [];
let tenantHistoryPage = 1;
let tenantHistoryPageSize = 10;
let tenantHistoryTotal = 0;
let tenantRoomId = null;
let tenantRoomName = null;
let tenantHopDong = null;
let tenantChiSo = null;
// Skeleton chỉ hiện khi CHƯA từng nạp được gì — auto-refresh 30s không được làm nhấp nháy bảng đang xem.
let tenantLoaded = false;
let camStream = null;
let camInterval = null;
let camStarting = false;
// Lần nạp gần nhất có lỗi không — khi đúng thì tóm tắt KHÔNG được khẳng định "chưa phát sinh".
let tenantLoadError = false;

async function loadTenantInvoices() {
  if (!tenantLoaded) renderTenantLoading();
  try {
    await loadTenantHistoryPage(tenantHistoryPage, tenantHistoryPageSize);
    tenantLoaded = true;
    tenantLoadError = false;
    // BR-19: chỉ hiện màn nhận phòng khi server xác nhận khach_thue.phong_id IS NULL.
    // KHÔNG suy từ số hóa đơn — khách vừa nhận phòng chưa phát sinh kỳ cước nào vẫn phải vào app.
    if (tenantRoomId === null) {
      showCheckinScreen(true);
      return;
    }
    showCheckinScreen(false);
    announceTenant(`Đã nạp xong ${currentTenantInvoices.length} kỳ cước, tổng ${tenantHistoryTotal} bản ghi`);
  } catch (err) {
    toast(err.message, 'err');
    announceTenant('Không tải được dữ liệu: ' + err.message);
    renderTenantEmpty();
    syncTenantRoomHeader();
    // Cho phép skeleton chạy lại ở lần nạp kế tiếp (thủ công hoặc auto-refresh 30s).
    tenantLoaded = false;
    tenantLoadError = true;
  }
  await refreshTenantSummary();
}

function showCheckinScreen(show) {
  const checkin = document.getElementById('tab-nhan-phong');
  const navTabs = document.getElementById('tenantTabs');
  const content = checkin ? checkin.parentElement : null;
  if (checkin) {
    checkin.classList.toggle('active', show);
    checkin.style.display = show ? '' : 'none';
  }
  // Khi hiện màn nhận phòng: ẩn sạch các pane app (overview/current/history) để không xếp chồng.
  if (content) {
    content.querySelectorAll(':scope > .tab-pane').forEach(p => {
      if (p === checkin) return;
      if (show) {
        p.classList.remove('active');
        p.style.display = 'none';
      } else {
        p.style.display = '';
      }
    });
  }
  if (navTabs) navTabs.style.display = show ? 'none' : '';
  syncTenantRoomHeader();
  if (!show) {
    stopCamera();
    // KHÔNG ép switchTenantTab('overview') ở đây: caller tự quyết (load ban đầu mới về overview,
    // còn auto-refresh 30s giữ nguyên tab khách đang xem).
  }
}

function setText(id, text) {
  const el = document.getElementById(id);
  if (el) el.textContent = text;
}

// BR-19: đồng bộ userbar + headline + statusstrip theo trạng thái nhận phòng.
// tenantRoomId === null → khách chưa có phòng, mọi cụm header phải nói "chưa nhận phòng",
// KHÔNG được để lại số phòng / tiêu đề hóa đơn của shell tĩnh.
function syncTenantRoomHeader() {
  const hasRoom = tenantRoomId !== null;
  const nameStr = tenantRoomName !== null && tenantRoomName !== undefined ? String(tenantRoomName).trim() : '';
  const roomLabel = nameStr
    ? (nameStr.startsWith('P') ? nameStr : `P.${nameStr}`)
    : (hasRoom ? `P.${tenantRoomId}` : '');

  setText('troom-userbar-text', hasRoom ? `Phòng ${roomLabel}` : 'Chưa nhận phòng');
  setText('thead-badge', hasRoom ? 'Chứng từ thu cước' : 'Nhận phòng');
  setText('thead-title', hasRoom ? 'Hóa Đơn Của Tôi' : 'Nhận Phòng Trọ');
  setText('tstatus-text', hasRoom ? `Người thuê · ${roomLabel} (KhachThue)` : 'Người thuê (KhachThue)');
  if (hasRoom) setText('troom-badge', roomLabel);
}

// Giữ aria-selected + tabindex khớp class="on" để screen reader + bàn phím thấy đúng tab.
function syncTabBar(barId, name) {
  document.querySelectorAll(`#${barId} .tab`).forEach(t => {
    const on = t.dataset.tab === name;
    t.classList.toggle('on', on);
    t.setAttribute('aria-selected', on ? 'true' : 'false');
    t.tabIndex = on ? 0 : -1;
  });
}

// Bàn phím cho tab bar: Arrow chuyển tab kế/trước, Home/End về đầu/cuối, focus tab được chọn.
function wireTabKeys(barId, onPick) {
  const bar = document.getElementById(barId);
  if (!bar) return;
  bar.addEventListener('keydown', e => {
    const tabs = [...bar.querySelectorAll('.tab')];
    const i = tabs.indexOf(document.activeElement);
    if (i < 0) return;
    let next = -1;
    if (e.key === 'ArrowRight') next = (i + 1) % tabs.length;
    else if (e.key === 'ArrowLeft') next = (i - 1 + tabs.length) % tabs.length;
    else if (e.key === 'Home') next = 0;
    else if (e.key === 'End') next = tabs.length - 1;
    else return;
    e.preventDefault();
    onPick(tabs[next].dataset.tab);
    tabs[next].focus();
  });
}

function announceTenant(msg) {
  const live = document.getElementById('tenant-live');
  if (live) live.textContent = msg;
}

function switchCheckinTab(tabName) {
  stopCamera();
  syncTabBar('checkinTabs', tabName);
  // Chỉ đổi class — .checkin-pane{display:none} / .active{display:block} trong khachthue.css
  // là nguồn hiển thị duy nhất; ghi style.display sẽ đè rules và làm mất animation.
  document.querySelectorAll('.checkin-pane').forEach(p => {
    p.classList.toggle('active', p.id === `checkin-${tabName}`);
  });
}

function checkinQr() {
  loadTenantInvoices();
}

/** Tab 1: đọc file ảnh QR từ người dùng qua <canvas> + jsQR */
function decodeQrFile(input) {
  const file = input.files && input.files[0];
  if (!file) return;
  const reader = new FileReader();
  reader.onerror = () => alertDialog('Không thể đọc file ảnh từ thiết bị.', 'Lỗi đọc file', 'err');
  reader.onload = e => {
    const img = new Image();
    img.onerror = () => alertDialog('Định dạng ảnh không được hỗ trợ hoặc file bị hỏng.', 'Lỗi ảnh', 'err');
    img.onload = () => {
      if (!img.width || !img.height) {
        alertDialog('Ảnh không có kích thước hợp lệ (0x0).', 'Ảnh rỗng', 'warn');
        return;
      }
      try {
        const canvas = document.getElementById('qrCanvas');
        // Canvas khai báo `hidden` trong index.html — chưa bỏ thì ctx.drawImage vẽ vào 0×0.
        canvas.hidden = false;
        const ctx = canvas.getContext('2d');
        canvas.width = img.width;
        canvas.height = img.height;
        ctx.drawImage(img, 0, 0);
        const imgData = ctx.getImageData(0, 0, img.width, img.height);
        const code = jsQR(imgData.data, imgData.width, imgData.height);
        if (code && code.data) {
          submitCheckinCode(code.data);
        } else {
          alertDialog('Không tìm thấy mã QR hợp lệ trong ảnh vừa chọn.', 'Không nhận diện được', 'warn');
        }
      } catch (err) {
        alertDialog('Lỗi giải mã ảnh QR: ' + (err.message || err), 'Lỗi xử lý', 'err');
      }
    };
    img.src = e.target.result;
  };
  reader.readAsDataURL(file);
}

/** Tab 2: mở webcam loop quét thời gian thực */
async function toggleCamera() {
  if (camStarting) return;
  if (camStream) {
    stopCamera();
    return;
  }
  const video = document.getElementById('qrVideo');
  const btn = document.getElementById('btn-cam');
  camStarting = true;
  if (btn) btn.disabled = true;
  try {
    try {
      camStream = await navigator.mediaDevices.getUserMedia({ video: { facingMode: 'environment' } });
    } catch (err) {
      // Máy tính/WebView2 có thể không có camera hướng sau; retry camera mặc định.
      if (err.name === 'NotAllowedError' || err.name === 'PermissionDeniedError') throw err;
      camStream = await navigator.mediaDevices.getUserMedia({ video: true });
    }
    video.srcObject = camStream;

    // Đợi video sẵn sàng metadata trước khi play (chặn race condition)
    if (video.readyState < video.HAVE_METADATA) {
      await new Promise(resolve => {
        video.onloadedmetadata = () => { video.onloadedmetadata = null; resolve(); };
        setTimeout(resolve, 800);
      });
    }

    try {
      await video.play();
    } catch (playErr) {
      // Chromium reject play() bằng AbortError khi có load request mới xen vào (autoplay/srcObject đổi).
      // Video autoplay muted vẫn hiển thị stream bình thường, không coi là lỗi chết.
      if (playErr.name !== 'AbortError') throw playErr;
    }

    if (btn) btn.innerHTML = '<i class="fas fa-stop"></i> Tắt camera';

    const canvas = document.createElement('canvas');
    const ctx = canvas.getContext('2d');

    camInterval = setInterval(() => {
      if (video.readyState === video.HAVE_ENOUGH_DATA) {
        canvas.width = video.videoWidth;
        canvas.height = video.videoHeight;
        ctx.drawImage(video, 0, 0, canvas.width, canvas.height);
        const imgData = ctx.getImageData(0, 0, canvas.width, canvas.height);
        const code = jsQR(imgData.data, imgData.width, imgData.height);
        if (code && code.data) {
          stopCamera();
          submitCheckinCode(code.data);
        }
      }
    }, 300);
  } catch (err) {
    stopCamera();
    const detail = err.message === 'Could not start video source'
      ? 'Không khởi động được camera. Hãy đóng ứng dụng khác đang dùng camera rồi thử lại, hoặc tải ảnh QR / nhập mã PIN.'
      : 'Không thể truy cập camera: ' + err.message;
    alertDialog(detail, 'Lỗi thiết bị', 'err');
  } finally {
    camStarting = false;
    if (btn) btn.disabled = false;
  }
}


function stopCamera() {
  if (camInterval) { clearInterval(camInterval); camInterval = null; }
  if (camStream) {
    camStream.getTracks().forEach(t => t.stop());
    camStream = null;
  }
  const btn = document.getElementById('btn-cam');
  if (btn) btn.innerHTML = '<i class="fas fa-camera"></i> Bật camera';
}

/** Tab 3: nhập mã PIN 8 số */
function submitPin() {
  const pin = (document.getElementById('qrPin')?.value || '').trim();
  if (!/^\d{8}$/.test(pin)) {
    alertDialog('Mã PIN nhận phòng phải gồm đúng 8 chữ số.', 'Mã không đúng định dạng', 'warn');
    return;
  }
  submitCheckinCode(pin);
}

/** Gửi mã lên server qua action KHACH_THUE_NHAN_PHONG_QR (BR-19) */
async function submitCheckinCode(rawCode) {
  try {
    const res = await window.bridge.call('KHACH_THUE_NHAN_PHONG_QR', { tokenOrPin: rawCode });
    stopCamera();
    await alertDialog(
      `Chào mừng bạn đến phòng ${res.soPhong || ''}! Hợp đồng đã có hiệu lực từ ${res.ngayBatDau || ''}.`,
      'Nhận phòng thành công', 'ok');
    tenantHistoryPage = 1;
    tenantLoaded = false;
    // Nạp lại dữ liệu mà không mất tab — khác window.location.reload().
    await loadTenantInvoices();
    switchTenantTab('overview');
  } catch (err) {
    alertDialog(err.message, 'Nhận phòng thất bại', 'err');
  }
}

async function loadTenantHistoryPage(page, soLuongMoiTrang) {
  const res = await window.bridge.call('HOA_DON_CUA_TOI', { page, soLuongMoiTrang });
  const dto = normalizeHistoryDto(res);
  currentTenantInvoices = (dto && dto.items) || [];
  tenantHistoryPage = (dto && dto.page) || 1;
  tenantHistoryPageSize = (dto && dto.soLuongMoiTrang) || soLuongMoiTrang;
  tenantHistoryTotal = (dto && dto.tongSo) || currentTenantInvoices.length;
  // phongId rõ ràng (kể cả null = server xác nhận chưa có phòng) thì tin server.
  // Chỉ khi field vắng mặt (bridge mock / server cũ) mới suy từ hóa đơn sẵn có để khỏi đẩy nhầm khách có phòng vào màn QR.
  tenantRoomId = dto && dto.phongId !== undefined
    ? dto.phongId
    : (currentTenantInvoices.length > 0 ? tenantRoomId : null);
  tenantRoomName = (dto && dto.soPhong) || null;
  tenantHopDong = (dto && dto.hopDong) || null;
  tenantChiSo = (dto && dto.chiSoKyNay) || null;
  syncTenantRoomHeader();
  renderTenantHistory(currentTenantInvoices);
  renderTenantPagination();
  renderTenantUtilities();
}

async function changeTenantPage(delta) {
  const tongSoTrang = Math.max(1, Math.ceil(tenantHistoryTotal / tenantHistoryPageSize));
  const next = tenantHistoryPage + delta;
  if (next < 1 || next > tongSoTrang) return;
  try {
    await loadTenantHistoryPage(next, tenantHistoryPageSize);
  } catch (err) {
    if (typeof toast === 'function') toast(err.message, 'err');
  }
}

function normalizeHistoryDto(res) {
  if (!res) return null;
  if (Array.isArray(res)) {
    return { items: res, tongSo: res.length, page: 1, soLuongMoiTrang: res.length || 10 };
  }
  const list = res.danhSach || res.items || res.DanhSach || res.Items;
  if (list) {
    const items = list || [];
    const i = n => (n === undefined || n === null) ? undefined : Number(n);
    return {
      items,
      tongSo: i(res.tongSo ?? res.TongSo ?? res.totalCount ?? res.TotalCount) ?? items.length,
      page: i(res.trang ?? res.Trang ?? res.page ?? res.Page) ?? 1,
      soLuongMoiTrang: i(res.soLuongMoiTrang ?? res.SoLuongMoiTrang ?? res.pageSize ?? res.PageSize) ?? 10,
      // Giữ undefined khi field VẮNG MẶT khác null THẬT: loadTenantHistoryPage dùng '!== undefined'
      // để giữ phòng cũ cho server/mock cũ không trả phongId, không ép về "chưa nhận phòng".
      phongId: 'phongId' in res ? res.phongId : ('PhongId' in res ? res.PhongId : undefined),
      soPhong: res.soPhong ?? res.SoPhong ?? null,
      hopDong: normalizeHopDong(res.hopDong ?? res.HopDong),
      chiSoKyNay: normalizeChiSo(res.chiSoKyNay ?? res.ChiSoKyNay)
    };
  }
  return null;
}

// hopDong / chiSoKyNay: server serialize camelCase (JsonNamingPolicy.CamelCase), vẫn nhận PascalCase cho mock & server cũ.
function subField(o, camel, pascal) {
  return o[camel] !== undefined ? o[camel] : o[pascal];
}

function subNum(o, camel, pascal) {
  const v = Number(subField(o, camel, pascal));
  return Number.isFinite(v) ? v : 0;
}

function normalizeHopDong(hd) {
  if (!hd) return null;
  return {
    id: subField(hd, 'id', 'Id'),
    soPhong: subField(hd, 'soPhong', 'SoPhong') ?? null,
    ngayBatDau: subField(hd, 'ngayBatDau', 'NgayBatDau') ?? null,
    ngayKetThuc: subField(hd, 'ngayKetThuc', 'NgayKetThuc') ?? null,
    giaThue: subNum(hd, 'giaThue', 'GiaThue'),
    tienCoc: subNum(hd, 'tienCoc', 'TienCoc')
  };
}

function normalizeChiSo(cs) {
  if (!cs) return null;
  return {
    kyCuoc: subField(cs, 'kyCuoc', 'KyCuoc') ?? null,
    dienCu: subNum(cs, 'dienCu', 'DienCu'),
    dienMoi: subNum(cs, 'dienMoi', 'DienMoi'),
    giaDien: subNum(cs, 'giaDien', 'GiaDien'),
    nuocCu: subNum(cs, 'nuocCu', 'NuocCu'),
    nuocMoi: subNum(cs, 'nuocMoi', 'NuocMoi'),
    giaNuoc: subNum(cs, 'giaNuoc', 'GiaNuoc'),
    hinhThucNuoc: subField(cs, 'hinhThucNuoc', 'HinhThucNuoc') || 'Khoi',
    soNguoiNuoc: subNum(cs, 'soNguoiNuoc', 'SoNguoiNuoc')
  };
}

function isPaidInvoice(i) {
  const st = i.trangThai ?? i.trang_thai;
  return i.isPaid === true || st === 'DaThu' || st === 'paid' || st === 1;
}

function renderTenantHistory(rows) {
  const list = rows || [];
  const body = document.getElementById('tenant-history-body');
  if (body) {
    body.innerHTML = list.map(i => `
      <tr class="clickable-row" onclick="showInvoiceDetail(${Number(i.id)})" title="Bấm để xem chi tiết kỳ ${esc(fmtKyCuoc(i.kyCuoc))}" aria-label="Kỳ ${esc(fmtKyCuoc(i.kyCuoc))}">
        <td class="mono" data-label="Kỳ cước">${esc(fmtKyCuoc(i.kyCuoc))}</td>
        <td class="num amount" data-label="Tổng tiền">${fmtMoney(i.tongTien)}</td>
        <td class="mono" data-label="Ngày đóng">${isPaidInvoice(i) ? fmtDate(i.ngayDong) : '—'}</td>
        <td class="num" data-label="Trạng thái"><span class="tag ${isPaidInvoice(i) ? 'paid' : 'unpaid'}">
          ${isPaidInvoice(i) ? 'Đã thanh toán' : 'Chưa thanh toán'}</span></td>
      </tr>`).join('') || `<tr class="empty"><td colspan="4" data-label="Kỳ cước">Chưa phát sinh kỳ cước nào</td></tr>`;
  }
  const info = document.getElementById('tenant-page-info');
  if (info) info.textContent = `Tổng: ${tenantHistoryTotal} bản ghi`;
}

function renderTenantPagination() {
  // Không có bản ghi thì "Trang 1 / 1" vô nghĩa — ẩn cụm phân trang.
  // (Viết `total < 1` chứ không so `=== 0`: contract BR-19 cấm chuỗi đó trong file này,
  // vì nó là dấu hiệu suy 'chưa có phòng' từ số hóa đơn.)
  const cluster = document.getElementById('tenant-pagination');
  if (cluster) cluster.hidden = tenantHistoryTotal < 1;
  const tongSoTrang = Math.max(1, Math.ceil(tenantHistoryTotal / tenantHistoryPageSize));
  const indicator = document.getElementById('tenant-page-indicator');
  if (indicator) indicator.textContent = `Trang ${tenantHistoryPage} / ${tongSoTrang}`;
  const prev = document.getElementById('btn-prev-page');
  if (prev) prev.disabled = tenantHistoryPage <= 1;
  const next = document.getElementById('btn-next-page');
  if (next) next.disabled = tenantHistoryPage >= tongSoTrang;
}

function renderTenantLoading() {
  const body = document.getElementById('tenant-history-body');
  if (body) {
    const skel = `<div class="skel"></div>`;
    body.innerHTML = `<tr><td colspan="4">${skel}${skel}${skel}</td></tr>`;
  }
}

function renderTenantEmpty() {
  const body = document.getElementById('tenant-history-body');
  if (body) {
    body.innerHTML = `<tr class="empty"><td colspan="4">
      <div class="empty-state">Không tải được dữ liệu
        <div><button type="button" class="btn btn-outline btn-sm" onclick="loadTenantInvoices()">Thử lại</button></div>
      </div></td></tr>`;
  }
  const info = document.getElementById('tenant-page-info');
  if (info) info.textContent = '';
  tenantHistoryTotal = 0;
  renderTenantPagination();
  // Không gọi renderTenantUtilities ở đây: currentTenantInvoices vẫn là số cũ của lần nạp
  // thành công trước — xóa KPI khi lỗi mạng chợt chớn sẽ nói "0 kỳ" oan.
}

// Tóm tắt Tổng quan + chi tiết Kỳ này: khi đang ở trang 1, currentTenantInvoices[0] đã là
// hóa đơn mới nhất (server ORDER BY ky_cuoc DESC) nên render ngay, tiết kiệm 1 round-trip.
// Chỉ gọi request riêng { page: 1, soLuongMoiTrang: 1 } khi khách đang đứng ở trang > 1.
async function refreshTenantSummary() {
  // Lỗi nạp: KHÔNG được khẳng định "chưa phát sinh kỳ cước nào" — đó là sự thật app chưa biết.
  // (Ruling A: số không tính được thì không được trình bày như số.)
  if (tenantLoadError) { renderTenantSummaryError(); return; }
  if (tenantHistoryPage === 1 && currentTenantInvoices.length > 0) {
    renderTenantSummary(currentTenantInvoices[0]);
    return;
  }
  try {
    const res = await window.bridge.call('HOA_DON_CUA_TOI', { page: 1, soLuongMoiTrang: 1 });
    const dto = normalizeHistoryDto(res);
    // Đồng bộ chỉ số/hợp đồng của ĐÚNG trang 1 trước khi vẽ: nếu không, tổng tiền kỳ mới nhất
    // bị ghép với chỉ số của kỳ trang cũ → khách tự kiểm tra phép tính sẽ ra số sai.
    if (dto && dto.hopDong) tenantHopDong = dto.hopDong;
    if (dto && dto.chiSoKyNay) tenantChiSo = dto.chiSoKyNay;
    const latest = (dto && dto.items && dto.items[0]) || currentTenantInvoices[0];
    if (latest) { renderTenantSummary(latest); return; }
  } catch (err) {
    renderTenantSummaryError();
    return;
  }
  renderTenantNoInvoice();
}

// Không có dữ liệu vì lỗi tải (khác hẳn "khách chưa phát sinh kỳ cước nào").
function renderTenantSummaryError() {
  setText('due-alert-text', 'Không tải được dữ liệu cước.');
  setTag('due-alert-tag', false, 'Đã nộp', 'Chưa nộp');
}

// Khách chưa phát sinh kỳ cước nào: số 0 thật thay cho dấu — và due-alert-text không giữ câu placeholder.
function renderTenantNoInvoice() {
  const recv = tenantHopDong && tenantHopDong.ngayBatDau
    ? ` · Nhận phòng từ ${fmtDateOnly(tenantHopDong.ngayBatDau)}`
    : '';
  setText('due-alert-text', 'Chưa phát sinh kỳ cước nào' + recv + '.');
  setTag('due-alert-tag', false, 'Đã nộp', 'Chưa nộp');
  setText('rc-title', 'Chi tiết quyết toán cước');
  setText('rc-code', 'MÃ SỐ BẢNG KÊ: —');
  ['rc-room', 'rc-elec', 'rc-water', 'rc-other'].forEach(id => setText(id, '0 đ'));
  setText('rc-total', '0');
  setTag('rc-trang_thai-tag', false, 'Đã thanh toán', 'Chưa thanh toán');
  setTag('rc-due-tag', false, 'Chưa thu', 'Đã thu');
  setText('ov-month', '—');
  setText('ov-total', '0 đ');
  setText('ov-trang_thai', 'Chưa phát sinh kỳ cước');
  setText('ov-elec', '0');
  setText('ov-nuoc', '0');
  setText('ov-elec-sub', 'Chưa có chỉ số kỳ này');
  setText('ov-nuoc-sub', 'Chưa có chỉ số kỳ này');
  setText('rc-elec-sub', 'Chưa có chỉ số kỳ này');
  setText('rc-water-sub', 'Chưa có chỉ số kỳ này');
  setText('tcontract-due', tenantHopDong
    ? `HĐ đến ${fmtDateOnly(tenantHopDong.ngayKetThuc)} · ${fmtMoney(tenantHopDong.giaThue)} đ/tháng`
    : 'Chưa có hợp đồng hiệu lực');
}

function renderTenantSummary(unpaid) {
  // Khối Kỳ này
  setText('rc-room', fmtMoney(unpaid.tienPhong) + ' đ');
  setText('rc-elec', fmtMoney(unpaid.tienDien) + ' đ');
  setText('rc-water', fmtMoney(unpaid.tienNuoc) + ' đ');
  setText('rc-other', fmtMoney(unpaid.phiKhac) + ' đ');
  setText('rc-total', fmtMoney(unpaid.tongTien));
  const paid = isPaidInvoice(unpaid);
  setText('rc-code', `MÃ SỐ BẢNG KÊ: HD-${String(unpaid.kyCuoc || '').replace('-', '')}-${unpaid.phongId || unpaid.id}`);
  setText('rc-title', `Chi tiết quyết toán cước tháng ${fmtKyCuoc(unpaid.kyCuoc)}`);
  setTag('rc-trang_thai-tag', paid, 'Đã thanh toán', 'Chưa thanh toán');
  setTag('rc-due-tag', !paid, 'Chưa thu', 'Đã thu');

  // Khối Tổng quan — tiêu thụ điện, nước, hạn hợp đồng, và chỉ số kỳ này
  const cs = tenantChiSo;
  setText('ov-elec', cs ? fmtMoney(cs.dienMoi - cs.dienCu) : '0');
  // Nước đổi đơn vị theo hình thức: theo khối (m³) hoặc theo đầu người.
  if (cs) {
    const per = String(cs.hinhThucNuoc || '').toLowerCase() === 'nguoi';
    setText('ov-nuoc', fmtMoney(per ? cs.soNguoiNuoc : cs.nuocMoi - cs.nuocCu));
    setText('ov-nuoc-unit', per ? 'người' : 'khối');
    setText('ov-nuoc-sub', `${per ? `${fmtMoney(cs.soNguoiNuoc)} người × ${fmtMoney(cs.giaNuoc)} đ` : `${fmtMoney(cs.nuocCu)} → ${fmtMoney(cs.nuocMoi)} khối × ${fmtMoney(cs.giaNuoc)} đ`}`);
    setText('ov-elec-sub', `${fmtMoney(cs.dienCu)} → ${fmtMoney(cs.dienMoi)} kWh × ${fmtMoney(cs.giaDien)} đ`);
    setText('rc-elec-sub', `${fmtMoney(cs.dienCu)} → ${fmtMoney(cs.dienMoi)} kWh × ${fmtMoney(cs.giaDien)} đ`);
    // rc-water-sub dùng định dạng khác: theo hinhThucNuoc (đầu người hoặc khối)
    setText('rc-water-sub', per ? `${fmtMoney(cs.soNguoiNuoc)} người × ${fmtMoney(cs.giaNuoc)} đ` : `${fmtMoney(cs.nuocCu)} → ${fmtMoney(cs.nuocMoi)} khối × ${fmtMoney(cs.giaNuoc)} đ`);
  } else {
    setText('ov-nuoc', '0');
    setText('ov-nuoc-unit', 'khối');
    setText('ov-elec-sub', 'Chưa có chỉ số kỳ này');
    setText('ov-nuoc-sub', 'Chưa có chỉ số kỳ này');
    setText('rc-elec-sub', 'Chưa có chỉ số kỳ này');
    setText('rc-water-sub', 'Chưa có chỉ số kỳ này');
  }

  // Khối Tổng quan — hợp đồng & hạn
  if (tenantHopDong) {
    setText('tcontract-due', `HĐ đến ${fmtDateOnly(tenantHopDong.ngayKetThuc)} · ${fmtMoney(tenantHopDong.giaThue)} đ/tháng`);
  } else {
    setText('tcontract-due', 'Chưa có hợp đồng hiệu lực');
  }
  setText('ov-month', fmtKyCuoc(unpaid.kyCuoc));
  setText('ov-total', fmtMoney(unpaid.tongTien) + ' đ');
  setText('ov-trang_thai', paid ? 'Đã thanh toán' : 'Chưa nộp');
  setText('due-alert-text', paid
    ? `Kỳ cước ${fmtKyCuoc(unpaid.kyCuoc)} đã được thanh toán. Cảm ơn bạn!`
    : `Kỳ cước ${fmtKyCuoc(unpaid.kyCuoc)} chưa hoàn tất thanh toán. Tổng cần nộp: ${fmtMoney(unpaid.tongTien)} đ.`);
  setTag('due-alert-tag', paid, 'Đã nộp', 'Chưa nộp');
}

function setTag(id, ok, okText, badText) {
  const el = document.getElementById(id);
  if (!el) return;
  el.textContent = ok ? okText : badText;
  el.className = `tag ${ok ? 'paid' : 'unpaid'}`;
}

function showInvoiceDetail(id) {
  const target = (currentTenantInvoices || []).find(i => Number(i.id) === Number(id));
  if (!target) return;
  const paid = isPaidInvoice(target);
  if (typeof openModal === 'function') {
    openModal({
      title: `Chi tiết kỳ cước ${fmtKyCuoc(target.kyCuoc)}`,
      fields: [
        { name: 'room', label: 'Tiền phòng', type: 'text', value: fmtMoney(target.tienPhong) + ' đ', disabled: true },
        { name: 'elec', label: 'Tiền điện', type: 'text', value: fmtMoney(target.tienDien) + ' đ', disabled: true },
        { name: 'water', label: 'Tiền nước', type: 'text', value: fmtMoney(target.tienNuoc) + ' đ', disabled: true },
        { name: 'other', label: 'Dịch vụ khác', type: 'text', value: fmtMoney(target.phiKhac) + ' đ', disabled: true },
        { name: 'total', label: 'Tổng cộng', type: 'text', value: fmtMoney(target.tongTien) + ' đ', disabled: true },
        { name: 'trang_thai', label: 'Trạng thái', type: 'text', value: paid ? `Đã thanh toán (${fmtDate(target.ngayDong)})` : 'Chưa thanh toán', disabled: true }
      ],
      onSubmit: async () => { /* chỉ để xem */ }
    });
  }
}

function switchTenantTab(name) {
  syncTabBar('tenantTabs', name);
  document.querySelectorAll('.tab-pane').forEach(p => {
    p.classList.toggle('active', p.id === `tab-${name}`);
  });
}

/**
 * Tab "Tiện ích": 4 KPI + biểu đồ SVG cột đôi tính từ dữ liệu đã nạp trong currentTenantInvoices.
 * Ruling A — MỌI KPI đều gắn sub ghi phạm vi, vì mảng chỉ là 1 trang: số kỳ → `trên N trang`,
 * còn lại → `tính trên M kỳ trang này`.
 */
// Kỳ cước 'yyyy-MM' → 'MM/yyyy'; giá trị rỗng/không khớp → '—'.
// Dùng chung cho KPI Tiện ích và nhãn trục biểu đồ — một nguồn định dạng, không nhân bản.
function fmtKyCuoc(v) {
  const s = String(v || '').trim();
  return /^\d{4}-\d{2}$/.test(s) ? s.slice(5) + '/' + s.slice(0, 4) : '—';
}

function renderTenantUtilities() {
  const kpis = document.getElementById('util-kpis');
  const charts = document.getElementById('util-charts');
  if (!kpis || !charts) return;
  const rows = currentTenantInvoices || [];
  const n = rows.length;
  const totalPages = Math.max(1, Math.ceil(tenantHistoryTotal / tenantHistoryPageSize));
  if (!n) {
    // Ruling A: sub phạm vi vẫn đúng khi 0 kỳ — không bơm số giả.
    kpis.innerHTML = [
      `<div class="kpi"><div class="lbl">Số kỳ đã phát sinh</div><div class="val">${tenantHistoryTotal}</div><div class="sub">trên ${totalPages} trang</div></div>`,
      `<div class="kpi"><div class="lbl">Trung bình mỗi kỳ</div><div class="val">0 đ</div><div class="sub">tính trên 0 kỳ trang này</div></div>`,
      `<div class="kpi"><div class="lbl">Kỳ cao nhất</div><div class="val">0 đ</div><div class="sub">chưa có kỳ · 0 kỳ trang này</div></div>`,
      `<div class="kpi green"><div class="lbl">Tổng đã nộp</div><div class="val">0 đ</div><div class="sub">chỉ kỳ đã thu · 0 kỳ trang này</div></div>`,
    ].join('');
    charts.innerHTML = '<div class="chart-empty">Chưa phát sinh kỳ cước nào — biểu đồ sẽ hiện khi có dữ liệu.</div>';
    const ovEmpty = document.getElementById('ov-chart');
    if (ovEmpty) ovEmpty.innerHTML = '<div class="chart-empty">Chưa phát sinh kỳ cước nào.</div>';
    return;
  }
  const sum = rows.reduce((a, h) => a + Number(h.tongTien || 0), 0);
  const avg = Math.round(sum / n);
  const top = rows.reduce((a, b) => Number(b.tongTien || 0) > Number(a.tongTien || 0) ? b : a, rows[0]);
  const paidSum = rows.filter(isPaidInvoice).reduce((a, h) => a + Number(h.tongTien || 0), 0);
  kpis.innerHTML = `
    <div class="kpi"><div class="lbl">Số kỳ đã phát sinh</div><div class="val">${tenantHistoryTotal}</div><div class="sub">trên ${totalPages} trang</div></div>
    <div class="kpi"><div class="lbl">Trung bình mỗi kỳ</div><div class="val">${fmtMoney(avg)} đ</div><div class="sub">tính trên ${n} kỳ trang này</div></div>
    <div class="kpi"><div class="lbl">Kỳ cao nhất</div><div class="val">${fmtMoney(top.tongTien)} đ</div><div class="sub">${esc(fmtKyCuoc(top.kyCuoc))} · tính trên ${n} kỳ trang này</div></div>
    <div class="kpi green"><div class="lbl">Tổng đã nộp</div><div class="val">${fmtMoney(paidSum)} đ</div><div class="sub">chỉ kỳ đã thu · ${n} kỳ trang này</div></div>`;
  charts.innerHTML = '';
  // Bọc trong .card chuẩn của shell (cùng vocab với .duo/.kpis) — SVG co giãn theo bề rộng card.
  const chartCard = document.createElement('div');
  chartCard.className = 'card';
  const chartHdr = document.createElement('div');
  chartHdr.className = 'hdr';
  chartHdr.innerHTML = '<i class="fas fa-chart-column"></i> Điện (kWh) &amp; nước theo kỳ';
  chartCard.appendChild(chartHdr);
  chartCard.appendChild(buildTenantChart(rows));
  charts.appendChild(chartCard);
  // Khe "Xu hướng cước" ở Tổng quan — cùng dữ liệu, dựng lại SVG (không di chuyển node).
  const ov = document.getElementById('ov-chart');
  if (ov) { ov.innerHTML = ''; ov.appendChild(buildTenantChart(rows)); }
}

/**
 * Biểu đồ SVG cột đôi (điện amber #E0AF68: kWh, nước sage #8BD7A3: khối/người) theo kyCuoc tăng dần.
 * Vẽ bằng document.createElementNS — không thư viện (test TenantJs_BuildsChartWithoutLibrary).
 * Controller ruling: vẽ THEO SẢN LƯỢNG (kWh và khối), không vẽ theo tiền — tránh việc tiền nước
 * (~20k) bị tiền điện (~175k) đè bẹp khiến cột nước gần như tàng hình trên cùng một thang đo.
 * Quy đổi: tiền / đơn giá (lấy từ chiSoKyNay hiện có, hoặc đơn giá chuẩn 3.500 đ/kWh, 12.000 đ/khối).
 * Thang đo riêng cho từng chuỗi (chuẩn hoá theo max của chuỗi đó) để cả hai luôn nhìn thấy được.
 * ponytail: sản lượng suy từ tiền nên sai lệch nếu đơn giá đổi giữa các kỳ — chuyển sang đọc
 * chỉ số thật theo từng dòng khi server trả kèm chi_so trong danh sách hóa đơn.
 */
function buildTenantChart(rows) {
  const NS = 'http://www.w3.org/2000/svg';
  const data = [...rows].sort((a, b) => String(a.kyCuoc).localeCompare(String(b.kyCuoc))).slice(-12);
  const svg = document.createElementNS(NS, 'svg');
  svg.setAttribute('class', 'bar-chart');
  svg.setAttribute('role', 'img');

  const cs = tenantChiSo;
  const giaD = (cs && Number(cs.giaDien)) || 3500;
  const giaN = (cs && Number(cs.giaNuoc)) || 12000;
  const perNguoi = cs && String(cs.hinhThucNuoc || '').toLowerCase() === 'nguoi';
  const donViNuoc = perNguoi ? 'người' : 'khối';
  // Sản lượng ước tính từ hóa đơn — làm tròn về số nguyên
  const series = data.map(h => ({
    kyCuoc: h.kyCuoc,
    elecQty: Math.max(0, Math.round(Number(h.tienDien || 0) / giaD)),
    waterQty: Math.max(0, Math.round(Number(h.tienNuoc || 0) / giaN))
  }));

  const totE = series.reduce((a, s) => a + s.elecQty, 0);
  const totW = series.reduce((a, s) => a + s.waterQty, 0);
  const title = document.createElementNS(NS, 'title');
  title.textContent = `${data.length} kỳ từ ${fmtKyCuoc(data[0].kyCuoc)} đến ${fmtKyCuoc(data[data.length - 1].kyCuoc)}: tổng ${fmtMoney(totE)} kWh điện, ${fmtMoney(totW)} ${donViNuoc} nước`;
  svg.appendChild(title);

  const W = 320, H = 120, pad = 18;
  svg.setAttribute('viewBox', `0 0 ${W} ${H}`);
  const maxE = Math.max(1, ...series.map(s => s.elecQty));
  const maxW = Math.max(1, ...series.map(s => s.waterQty));
  const gw = (W - pad * 2) / series.length;

  series.forEach((s, i) => {
    // Kỳ không tiêu thụ vẫn để 1px — cột 0px biến mất khỏi trục, đọc thành "thiếu dữ liệu".
    const eH = Math.max(1, Math.round(s.elecQty / maxE * (H - pad * 2)));
    const wH = Math.max(1, Math.round(s.waterQty / maxW * (H - pad * 2)));
    const x = pad + i * gw;

    const e = document.createElementNS(NS, 'rect');
    e.setAttribute('x', x + 1); e.setAttribute('y', H - pad - eH);
    e.setAttribute('width', Math.max(1, gw / 2 - 2)); e.setAttribute('height', eH);
    e.setAttribute('fill', '#E0AF68');
    svg.appendChild(e);

    const w = document.createElementNS(NS, 'rect');
    w.setAttribute('x', x + gw / 2); w.setAttribute('y', H - pad - wH);
    w.setAttribute('width', Math.max(1, gw / 2 - 2)); w.setAttribute('height', wH);
    w.setAttribute('fill', '#8BD7A3');
    svg.appendChild(w);

    const t = document.createElementNS(NS, 'text');
    t.setAttribute('x', x + gw / 2); t.setAttribute('y', H - 5);
    t.setAttribute('text-anchor', 'middle'); t.setAttribute('font-size', '8'); t.setAttribute('fill', '#767E88');
    t.textContent = fmtKyCuoc(s.kyCuoc);
    svg.appendChild(t);
  });
  return svg;
}

/**
 * Ruling B — copy thông tin chuyển khoản: <soPhong> <kyCuoc>: <tongTien> đ.
 * BANK_INFO còn '' (chưa có hồ sơ nhận tiền trên server) → copy dòng kỳ cước hôm nay
 * kèm ghi chú chưa cấu hình STK; KHÔNG render placeholder STK nào lên màn hình.
 */
async function copyTransfer() {
  const target = (currentTenantInvoices || []).find(i => !isPaidInvoice(i)) || (currentTenantInvoices || [])[0];
  if (!target) {
    if (typeof toast === 'function') toast('Không có hóa đơn để sao chép', 'warn');
    return;
  }
  const room = String(tenantRoomName || target.soPhong || '').trim();
  const line = `${room ? room + ' ' : ''}${target.kyCuoc || ''}: ${fmtMoney(target.tongTien)} đ`;
  const text = BANK_INFO ? `${BANK_INFO}\n${line}` : line;
  const hint = BANK_INFO ? '' : ' (thông tin nhận tiền chưa cấu hình)';
  try {
    if (navigator.clipboard && navigator.clipboard.writeText) {
      await navigator.clipboard.writeText(text);
      if (typeof toast === 'function') toast(`Đã sao chép: ${line}${hint}`, 'ok');
    } else {
      if (typeof toast === 'function') toast(`Nội dung chuyển khoản: ${text}${hint}`, 'info');
    }
  } catch (err) {
    if (typeof toast === 'function') toast(`Nội dung: ${text}${hint}`, 'info');
  }
}

// logout() khai báo ở shared/js/ui.js, tải trước file này.
document.addEventListener('DOMContentLoaded', () => {
  // auth.js điều hướng sang đây kèm ?u=<tên đã encode>.
  const params = new URLSearchParams(window.location.search);
  const name = params.get('u');
  if (name) {
    const card = document.getElementById('tname-card');
    if (card) card.textContent = name;
    const legacy = document.getElementById('tname');
    if (legacy) legacy.textContent = name;
  }
  switchTenantTab(params.get('tab') === 'history' ? 'history' : 'overview');
  wireTabKeys('tenantTabs', switchTenantTab);
  wireTabKeys('checkinTabs', switchCheckinTab);
  loadTenantInvoices();
  // ponytail: tự động làm mới toàn trang mỗi 30s + khi quay lại cửa sổ — thay nút sync tay.
  startAutoRefresh(loadTenantInvoices, 30000);
});
