// Cầu nối WebMessage: JS gọi C#, C# trả về theo requestId.
window.bridge = {
  _handlers: {},
  _seq: 0,

  call: function (hanh_dong, data) {
    return new Promise((resolve, reject) => {
      const requestId = 'req_' + (++this._seq);
      this._handlers[requestId] = { resolve, reject };
      window.chrome.webview.postMessage(JSON.stringify({
        requestId: requestId,
        hanh_dong: hanh_dong,
        data: data || {}
      }));
    });
  },

  onMessage: function (raw) {
    let msg;
    try { msg = typeof raw === 'string' ? JSON.parse(raw) : raw; } catch (e) { return; }
    if (!msg || !msg.requestId) return;
    const h = this._handlers[msg.requestId];
    if (!h) return;
    delete this._handlers[msg.requestId];
    if (msg.success) h.resolve(msg.data);
    else h.reject(new Error(msg.error || 'Thao tác thất bại'));
  }
};

// ponytail: mock chỉ chạy khi KHÔNG có WebView2 (mở file trực tiếp / preview trong browser).
// Trong app thật window.chrome.webview luôn tồn tại nên nhánh này không bao giờ chạy.
if (window.chrome && window.chrome.webview) {
  window.chrome.webview.addEventListener('message', ev => window.bridge.onMessage(ev.data));
} else if (window.location.protocol.startsWith('http')) {
  const SAMPLE = [
    { id: 3, phongId: 102, hopDongId: 1, kyCuoc: '2026-09', tienPhong: 2000000, tienDien: 175000, tienNuoc: 20000, phiKhac: 50000, tongTien: 2245000, trang_thai: 'ChuaThu', ngayDong: null },
    { id: 2, phongId: 102, hopDongId: 1, kyCuoc: '2026-08', tienPhong: 2000000, tienDien: 172000, tienNuoc: 20000, phiKhac: 50000, tongTien: 2242000, trang_thai: 'DaThu', ngayDong: '2026-09-05T10:14:00' },
    { id: 1, phongId: 102, hopDongId: 1, kyCuoc: '2026-07', tienPhong: 2000000, tienDien: 155000, tienNuoc: 20000, phiKhac: 50000, tongTien: 2225000, trang_thai: 'DaThu', ngayDong: '2026-08-04T15:22:00' }
  ];
  window.bridge.call = function (hanh_dong, data) {
    if (hanh_dong === 'HOA_DON_CUA_TOI') {
      const page = Number((data && data.page) || 1);
      const size = Number((data && data.soLuongMoiTrang) || 10);
      const start = (page - 1) * size;
      return Promise.resolve({ items: SAMPLE.slice(start, start + size), tongSo: SAMPLE.length, page, soLuongMoiTrang: size });
    }
    return Promise.resolve({});
  };
}
