// Kiểm tra đồng bộ FE: handler inline trong index.html phải có hàm JS, mọi getElementById phải có id trong HTML.
// Chạy: node tools/xcheck.js (thoát khác 0 nếu lệch). Đọc-only, không sửa file.
const fs = require('fs');
const path = require('path');

const ROOT = path.join(__dirname, '..', 'QuanLyTro', 'Assets', 'wwwroot');
const ROLES = ['auth', 'chutro', 'congan', 'khachthue'];

let hasError = false;

for (const role of ROLES) {
  const dir = path.join(ROOT, role);
  const htmlFile = path.join(dir, 'index.html');
  if (!fs.existsSync(htmlFile)) continue;
  const html = fs.readFileSync(htmlFile, 'utf8').replace(/\r/g, '');

  // Gom script theo đúng thứ tự load trong HTML.
  const scripts = [];
  const scriptRegex = /<script src="([^"]+)"><\/script>/g;
  let m;
  while ((m = scriptRegex.exec(html)) !== null) {
    scripts.push(path.resolve(dir, m[1]));
  }
  const jsContent = scripts
    .filter((p) => fs.existsSync(p))
    .map((p) => fs.readFileSync(p, 'utf8').replace(/\r/g, ''))
    .join('\n');

  // 1. Mọi handler inline on*="fn(" phải có hàm tương ứng trong JS.
  const handlerRegex = /on[a-z]+="([A-Za-z_$][A-Za-z0-9_$]*)\(/g;
  const handlers = new Set();
  while ((m = handlerRegex.exec(html)) !== null) handlers.add(m[1]);

  const missingFns = [];
  for (const fn of handlers) {
    if (!new RegExp(`\\bfunction\\s+${fn}\\b|(?:const|let|var)\\s+${fn}\\s*=`).test(jsContent)) {
      missingFns.push(fn);
    }
  }

  // 2. Mọi getElementById('x') phải có id="x" trong HTML.
  const idRegex = /getElementById\(['"]([A-Za-z0-9_-]+)['"]\)/g;
  const jsIds = new Set();
  while ((m = idRegex.exec(jsContent)) !== null) jsIds.add(m[1]);

  const missingIds = [];
  for (const id of jsIds) {
    if (!new RegExp(`id=["']${id}["']`).test(html)) missingIds.push(id);
  }

  console.log(`[${role}] handler: ${handlers.size} thiếu hàm: ${missingFns.join(', ') || '(không)'}`);
  console.log(`[${role}] id JS đọc: ${jsIds.size} HTML không có: ${missingIds.join(', ') || '(không)'}`);
  if (missingFns.length || missingIds.length) hasError = true;
}

console.log(hasError ? '\nLỆCH ĐỒNG BỘ PHÁT HIỆN!' : '\nĐỒNG BỘ: không phát hiện lệch');
process.exit(hasError ? 1 : 0);
