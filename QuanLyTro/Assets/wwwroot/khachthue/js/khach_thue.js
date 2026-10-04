// ponytail: shell khách thuê — server tự suy ra phòng từ token. Phân trang phía server (page/soLuongMoiTrang).
let currentTenantInvoices = [];
let tenantHistoryPage = 1;
let tenantHistoryPageSize = 10;
let tenantHistoryTotal = 0;
let tenantRoomId = null;
let tenantRoomName = null;
let camStream = null;
let camInterval = null;
let camStarting = false;

async function loadTenantInvoices() {
  try {
    await loadTenantHistoryPage(tenantHistoryPage, tenantHistoryPageSize);
    // BR-19: chỉ hiện màn nhận phòng khi server xác nhận khach_thue.phong_id IS NULL.
    // KHÔNG suy từ số hóa đơn — khách vừa nhận phòng chưa phát sinh kỳ cước nào vẫn phải vào app.
    if (tenantRoomId === null) {
      showCheckinScreen(true);
      return;
    }
    showCheckinScreen(false);
  } catch (err) {
    toast(err.message, 'err');
    renderTenantEmpty();
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
  if (!show) {
    stopCamera();
    // KHÔNG ép switchTenantTab('overview') ở đây: caller tự quyết (load ban đầu mới về overview,
    // còn auto-refresh 30s giữ nguyên tab khách đang xem).
  }
}

function switchCheckinTab(tabName) {
  stopCamera();
  document.querySelectorAll('#checkinTabs .tab').forEach(t => {
    t.classList.toggle('on', t.dataset.tab === tabName);
  });
  document.querySelectorAll('.checkin-pane').forEach(p => {
    const on = p.id === `checkin-${tabName}`;
    p.classList.toggle('active', on);
    p.style.display = on ? '' : 'none';
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
    // Reload lại giao diện để hiển thị dashboard phòng và cước
    window.location.reload();
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
  if (tenantRoomName) {
    const badge = document.getElementById('troom-badge');
    if (badge) badge.textContent = tenantRoomName.startsWith('P') ? tenantRoomName : `P.${tenantRoomName}`;
  }
  renderTenantHistory(currentTenantInvoices);
  renderTenantPagination();
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
      phongId: res.phongId ?? res.PhongId ?? null,
      soPhong: res.soPhong ?? res.SoPhong ?? null
    };
  }
  return null;
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
      <tr class="clickable-row" onclick="showInvoiceDetail(${Number(i.id)})" title="Bấm để xem chi tiết kỳ ${esc(i.kyCuoc)}">
        <td class="mono">${esc(i.kyCuoc)}</td>
        <td class="num amount">${fmtMoney(i.tongTien)}</td>
        <td class="mono">${isPaidInvoice(i) ? fmtDate(i.ngayDong) : '—'}</td>
        <td class="num"><span class="tag ${isPaidInvoice(i) ? 'paid' : 'unpaid'}">
          ${isPaidInvoice(i) ? 'Đã thanh toán' : 'Chưa thanh toán'}</span></td>
      </tr>`).join('') || `<tr><td colspan="4" style="text-align:center;color:var(--dim)">Không có hóa đơn nào</td></tr>`;
  }
  const info = document.getElementById('tenant-page-info');
  if (info) info.textContent = `Tổng: ${tenantHistoryTotal} bản ghi`;
}

function renderTenantPagination() {
  const tongSoTrang = Math.max(1, Math.ceil(tenantHistoryTotal / tenantHistoryPageSize));
  const indicator = document.getElementById('tenant-page-indicator');
  if (indicator) indicator.textContent = `Trang ${tenantHistoryPage} / ${tongSoTrang}`;
  const prev = document.getElementById('btn-prev-page');
  if (prev) prev.disabled = tenantHistoryPage <= 1;
  const next = document.getElementById('btn-next-page');
  if (next) next.disabled = tenantHistoryPage >= tongSoTrang;
}

function renderTenantEmpty() {
  const body = document.getElementById('tenant-history-body');
  if (body) body.innerHTML = `<tr><td colspan="4" style="text-align:center;color:var(--dim)">Không tải được dữ liệu</td></tr>`;
  tenantHistoryTotal = 0;
  renderTenantPagination();
}

// Tóm tắt Tổng quan + chi tiết Kỳ này: khi đang ở trang 1, currentTenantInvoices[0] đã là
// hóa đơn mới nhất (server ORDER BY ky_cuoc DESC) nên render ngay, tiết kiệm 1 round-trip.
// Chỉ gọi request riêng { page: 1, soLuongMoiTrang: 1 } khi khách đang đứng ở trang > 1.
async function refreshTenantSummary() {
  if (tenantHistoryPage === 1 && currentTenantInvoices.length > 0) {
    renderTenantSummary(currentTenantInvoices[0]);
    return;
  }
  try {
    const res = await window.bridge.call('HOA_DON_CUA_TOI', { page: 1, soLuongMoiTrang: 1 });
    const dto = normalizeHistoryDto(res);
    const latest = (dto && dto.items && dto.items[0]) || currentTenantInvoices[0];
    if (latest) renderTenantSummary(latest);
  } catch (err) {
    const fallback = currentTenantInvoices[0];
    if (fallback) renderTenantSummary(fallback);
  }
}

function renderTenantSummary(unpaid) {
  const setVal = (id, val) => {
    const el = document.getElementById(id);
    if (el) el.textContent = val;
  };

  // Khối Kỳ này
  setVal('rc-room', fmtMoney(unpaid.tienPhong) + ' đ');
  setVal('rc-elec', fmtMoney(unpaid.tienDien) + ' đ');
  setVal('rc-water', fmtMoney(unpaid.tienNuoc) + ' đ');
  setVal('rc-other', fmtMoney(unpaid.phiKhac) + ' đ');
  setVal('rc-total', fmtMoney(unpaid.tongTien));
  const paid = isPaidInvoice(unpaid);
  setVal('rc-code', `MÃ SỐ BẢNG KÊ: HD-${String(unpaid.kyCuoc || '').replace('-', '')}-${unpaid.phongId || unpaid.id}`);
  setVal('rc-title', `Chi tiết quyết toán cước tháng ${String(unpaid.kyCuoc || '').replace('-', '/')}`);
  setTag('rc-trang_thai-tag', paid, 'Đã thanh toán', 'Chưa thanh toán');
  setTag('rc-due-tag', !paid, 'Chưa thu', 'Đã thu');

  // Khối Tổng quan
  setVal('ov-month', String(unpaid.kyCuoc || '').replace('-', '/'));
  setVal('ov-total', fmtMoney(unpaid.tongTien) + ' đ');
  setVal('ov-trang_thai', paid ? 'Đã thanh toán' : 'Chưa nộp');
  setVal('troom-badge', `P.${unpaid.phongId ?? ''}`);
  setVal('tcontract-due', paid ? 'Hợp đồng: Đang hiệu lực' : 'Hợp đồng: Còn cước chưa nộp');
  setVal('due-alert-text', paid
    ? `Kỳ cước ${unpaid.kyCuoc} đã được thanh toán. Cảm ơn bạn!`
    : `Kỳ cước ${unpaid.kyCuoc} chưa hoàn tất thanh toán. Tổng cần nộp: ${fmtMoney(unpaid.tongTien)} đ.`);
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
      title: `Chi tiết kỳ cước ${target.kyCuoc}`,
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
  document.querySelectorAll('#tenantTabs .tab').forEach(t => {
    t.classList.toggle('on', t.dataset.tab === name);
  });
  document.querySelectorAll('.tab-pane').forEach(p => {
    p.classList.toggle('active', p.id === `tab-${name}`);
  });
}

async function copyTransfer() {
  const target = (currentTenantInvoices || []).find(i => !isPaidInvoice(i)) || (currentTenantInvoices || [])[0];
  if (!target) {
    if (typeof toast === 'function') toast('Không có hóa đơn để sao chép', 'warn');
    return;
  }
  const text = `HD-${String(target.kyCuoc || '').replace('-', '')}-${target.phongId || target.id} ${fmtMoney(target.tongTien)} đ`;
  try {
    if (navigator.clipboard && navigator.clipboard.writeText) {
      await navigator.clipboard.writeText(text);
      if (typeof toast === 'function') toast(`Đã sao chép: ${text}`, 'ok');
    } else {
      if (typeof toast === 'function') toast(`Nội dung chuyển khoản: ${text}`, 'info');
    }
  } catch (err) {
    if (typeof toast === 'function') toast(`Nội dung: ${text}`, 'info');
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
  loadTenantInvoices();
  // ponytail: tự động làm mới toàn trang mỗi 30s + khi quay lại cửa sổ — thay nút sync tay.
  startAutoRefresh(loadTenantInvoices, 30000);
});
