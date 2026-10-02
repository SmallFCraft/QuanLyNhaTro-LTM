// Cầu nối WebMessage: JS gọi C#, C# trả về theo requestId.
window.bridge = {
  _handlers: {},
  _seq: 0,

  call: function (action, data) {
    return new Promise((resolve, reject) => {
      const requestId = 'req_' + (++this._seq);
      this._handlers[requestId] = { resolve, reject };
      window.chrome.webview.postMessage(JSON.stringify({
        requestId: requestId,
        action: action,
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
    { id: 3, roomId: 102, contractId: 1, billingMonth: '2026-09', roomAmount: 2000000, electricityAmount: 175000, waterAmount: 20000, otherFees: 50000, totalAmount: 2245000, status: 'Unpaid', paidAt: null },
    { id: 2, roomId: 102, contractId: 1, billingMonth: '2026-08', roomAmount: 2000000, electricityAmount: 172000, waterAmount: 20000, otherFees: 50000, totalAmount: 2242000, status: 'Paid', paidAt: '2026-09-05T10:14:00' },
    { id: 1, roomId: 102, contractId: 1, billingMonth: '2026-07', roomAmount: 2000000, electricityAmount: 155000, waterAmount: 20000, otherFees: 50000, totalAmount: 2225000, status: 'Paid', paidAt: '2026-08-04T15:22:00' }
  ];
  window.bridge.call = function (action, data) {
    if (action === 'INVOICE_GET_MINE') {
      const page = Number((data && data.page) || 1);
      const size = Number((data && data.pageSize) || 10);
      const start = (page - 1) * size;
      return Promise.resolve({ items: SAMPLE.slice(start, start + size), totalCount: SAMPLE.length, page, pageSize: size });
    }
    return Promise.resolve({});
  };
}
