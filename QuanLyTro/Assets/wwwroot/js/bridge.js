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

window.chrome.webview.addEventListener('message', ev => window.bridge.onMessage(ev.data));

// Chuyển shell theo vai. roleKey: 'landlord' | 'police' | 'tenant'
function showShell(roleKey) {
  ['login', 'landlord', 'police', 'tenant'].forEach(id => {
    const el = document.getElementById(id);
    if (el) el.hidden = (id !== roleKey);
  });
}
