// ChuTro "Thống kê" tab — KPI + biểu đồ + xuất tạm trú (US-04/18/19, US-08).
// Dùng esc/fmtMoney/toast (ui.js) và toBillingMonth (js/utils.js).
const repEl = id => document.getElementById(id);

// KPI trong index.html không có id ổn định → lấy theo thứ tự thẻ (đã thu, còn nợ, lấp đầy).
function reportKpiVals() {
  return [...document.querySelectorAll('#tab-reports .kpis .kpi .val')];
}

async function loadReports() {
  const month = toBillingMonth(repEl('rep-month')?.value);
  try {
    const summary = await window.bridge.call('BAO_CAO_TONG_QUAN', { kyCuoc: month }) || {};
    fillReportKpis(summary);
    await renderChartbar(month);
  } catch (err) {
    toast(err.message, 'err');
  }
}

function fillReportKpis(s) {
  const vals = reportKpiVals();
  const phong = Number(s.tongSoPhong ?? s.totalRooms) || 0;
  const daThue = Number(s.phongDaThue ?? s.rentedRooms) || 0;
  const fill = phong > 0 ? Math.round(daThue / phong * 100) : 0;
  const texts = [
    `${fmtMoney(s.soTienDaThu ?? s.paidAmount)} đ`,
    `${fmtMoney(s.soTienChuaThu ?? s.unpaidAmount)} đ`,
    `${fill}%`
  ];
  vals.slice(0, 3).forEach((el, i) => { el.textContent = texts[i]; });
  const meter = document.querySelector('#tab-reports .kpi .meter i');
  if (meter) meter.style.width = `${fill}%`;
}

// US-19: server chỉ trả 1 kỳ → client hỏi 4 kỳ gần nhất và vẽ cột theo tỷ lệ đã thu lớn nhất.
async function renderChartbar(month) {
  const bar = document.querySelector('#tab-reports .chartbar');
  if (!bar) return;
  const months = lastMonths(month, 4);
  const sums = await Promise.all(months.map(m =>
    window.bridge.call('BAO_CAO_TONG_QUAN', { kyCuoc: m }).catch(() => null)));
  const paids = sums.map(s => Number(s?.soTienDaThu ?? s?.paidAmount) || 0);
  const max = Math.max(...paids, 1);
  bar.innerHTML = months.map((m, i) => `
    <div class="b ${m === month ? 'on' : ''}" style="height:${Math.round(paids[i] / max * 100)}%">
      <i>${fmtMoney(paids[i])}</i><b>${esc(m.slice(5))}</b>
    </div>`).join('');
}

// `yyyy-MM` lùi n tháng (mốc cuối là tháng đang chọn).
function lastMonths(month, count) {
  const m = /^(\d{4})-(\d{2})$/.exec(month || '');
  if (!m) return [month];
  let year = Number(m[1]);
  let mon = Number(m[2]);
  const out = [];
  for (let i = 0; i < count; i++) {
    out.unshift(`${year}-${String(mon).padStart(2, '0')}`);
    if (--mon === 0) { mon = 12; year--; }
  }
  return out;
}

// US-08: server trả về List<XuatHoSoTamTruDto> (không phải filePath) → tự dựng CSV UTF-8 BOM
// và tải xuống. Cột khớp ReportsForm.ExportResidenceCsvAsync, RFC 4180 escape.
async function exportResidence() {
  try {
    const rows = await window.bridge.call('XUAT_HO_SO_TAM_TRU', {}) || [];
    const cell = v => `"${String(v ?? '').replace(/"/g, '""')}"`;
    const csv = ['Họ tên,Ngày sinh,CCCD,Quê quán,Số phòng']
      .concat(rows.map(r => [r.hoTen, fmtDateOnly(r.ngaySinh), r.cccd, r.queQuan ?? r.que_quan, r.soPhong]
        .map(cell).join(',')))
      .join('\r\n');
    const url = URL.createObjectURL(new Blob(['﻿' + csv], { type: 'text/csv;charset=utf-8' }));
    const a = document.createElement('a');
    a.href = url;
    a.download = `danh-sach-tam-tru-${new Date().toISOString().slice(0, 10)}.csv`;
    a.click();
    URL.revokeObjectURL(url);
    toast(`Đã xuất ${rows.length} dòng tạm trú.`, 'ok');
  } catch (err) {
    toast(err.message, 'err');
  }
}