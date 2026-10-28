// Tab Tổng quan: 4 thẻ KPI + 3 biểu đồ + bảng còn nợ + bảng HĐ sắp hết hạn.
async function loadDash() {
  const month = new Date().toISOString().slice(0, 7); // yyyy-MM
  try {
    const [summary, hoaDon, hopDong] = await Promise.all([
      window.bridge.call('BAO_CAO_TONG_QUAN', { kyCuoc: month }),
      window.bridge.call('HOA_DON_LAY_TAT_CA', { kyCuoc: month }),
      window.bridge.call('HOP_DONG_LAY_TAT_CA', {})
    ]);

    if (summary) {
      const totalRooms = summary.tongSoPhong ?? summary.totalRooms;
      const availableRooms = summary.phongTrong ?? summary.availableRooms;
      const currentTenants = summary.khachHienTai ?? summary.currentTenants;
      const unpaidAmount = summary.soTienChuaThu ?? summary.unpaidAmount;
      const rentedRooms = summary.phongDaThue ?? summary.rentedRooms ?? 0;

      setKpi('kpi-total-phong', totalRooms);
      setKpi('kpi-available', availableRooms);
      setKpi('kpi-khach_thue', currentTenants);
      setKpi('kpi-unpaid', fmtMoney(unpaidAmount));
      const el = document.getElementById('kpi-available-sub');
      if (el) el.textContent = `Thuê ${rentedRooms} · Trống ${availableRooms ?? 0}`;
      // Cập nhật nhãn đếm trên tab "Phòng" và "Người dùng" ngay khi mở Dashboard
      const cntBadge = document.getElementById('cnt-phong');
      if (cntBadge && totalRooms !== undefined && totalRooms !== null) cntBadge.textContent = totalRooms;
      const cntTenants = document.getElementById('cnt-khach_thue');
      if (cntTenants && currentTenants !== undefined && currentTenants !== null) cntTenants.textContent = currentTenants;

      renderThieuHoaDon(summary);
    }

    const overdue = (Array.isArray(hoaDon) ? hoaDon : []).filter(i => (i.trangThai ?? i.trang_thai) !== 'DaThu');
    const ob = document.getElementById('dash-overdue-rows');
    if (ob) ob.innerHTML = overdue.map(i => {
      const room = i.soPhong ?? ('P' + i.phongId);
      const rep = i.tenNguoiDaiDien ?? i.representativeName ?? '—';
      const tong = i.tongTien ?? i.totalAmount ?? 0;
      return `
        <tr><td><b>${esc(room)}</b></td>
        <td class="mono">${esc(i.kyCuoc)}</td>
        <td class="num due-red">${fmtMoney(tong)}</td>
        <td>${esc(rep)}</td></tr>`;
    }).join('') || '<tr><td colspan="4" style="color:var(--dim)">Không có phòng nào còn nợ.</td></tr>';

    const soon = (Array.isArray(hopDong) ? hopDong : []).map(item => {
      const inner = item.contract || item.hopDong || item.HopDong || item;
      return {
        ...inner,
        soPhong: item.soPhong ?? item.roomNumber ?? inner.soPhong,
        representativeName: item.representativeName ?? item.tenNguoiDaiDien ?? inner.representativeName ?? inner.tenNguoiDaiDien ?? '—',
        trangThai: inner.trangThai ?? inner.trang_thai,
        ngayKetThuc: inner.ngayKetThuc ?? inner.endDate
      };
    }).filter(c => {
      const st = c.trangThai;
      if (st !== 'HieuLuc') return false;
      const end = c.ngayKetThuc;
      const days = (new Date(end) - new Date()) / 86400000;
      return days >= 0 && days <= 30;
    });
    const sb = document.getElementById('dash-expiring-rows');
    if (sb) sb.innerHTML = soon.map(c => {
      const end = c.ngayKetThuc ?? c.endDate;
      const days = Math.ceil((new Date(end) - new Date()) / 86400000);
      const room = c.soPhong ?? ('P' + c.phongId);
      const rep = c.tenNguoiDaiDien ?? c.representativeName ?? '—';
      return `<tr><td><b>${esc(room)}</b></td>
        <td>${esc(rep)}</td>
        <td class="mono">${fmtDateOnly(end)}</td>
        <td class="num due-red">${days} ngày</td></tr>`;
    }).join('') || '<tr><td colspan="4" style="color:var(--dim)">Không có hợp đồng nào sắp hết hạn.</td></tr>';

    await renderDashCharts(summary, month);
  } catch (err) {
    toast(err.message, 'err');
  }
}

/**
 * 3 biểu đồ Tổng quan: doanh thu 6 kỳ, tình trạng phòng, thu/còn nợ kỳ này.
 * Tách try/catch riêng: lỗi vẽ biểu đồ không được làm hỏng 2 bảng số liệu phía trên.
 * ponytail: server chỉ trả 1 kỳ → phải hỏi 5 kỳ trước đó (N+1 theo kỳ, đều là truy vấn tổng hợp).
 * Thêm mảng lưu theo kỳ ở server nếu số lượng kỳ phình to.
 */
async function renderDashCharts(summary, month) {
  if (!summary) return;
  try {
    const rev = document.getElementById('dash-rev-chart');
    if (rev) {
      const months = lastMonths(month, 6);
      const past = await Promise.all(months.slice(0, -1).map(m =>
        window.bridge.call('BAO_CAO_TONG_QUAN', { kyCuoc: m }).catch(() => null)));
      const paids = [
        ...past.map(s => Number(s?.soTienDaThu ?? s?.paidAmount) || 0),
        Number(summary.soTienDaThu ?? summary.paidAmount) || 0
      ];
      const max = Math.max(...paids, 1);
      // Cột 0% biến mất khỏi trục; và cột 100% đẩy nhãn giá vượt khỏi card.
      rev.innerHTML = months.map((m, i) => `
        <div class="b ${i === months.length - 1 ? 'on' : ''}" style="height:${Math.max(3, Math.round(paids[i] / max * 78))}%"
             title="Kỳ ${esc(m)} — đã thu ${fmtMoney(paids[i])} đ">
          <i>${fmtMoney(paids[i])}</i><b>${esc(m.slice(5))}</b>
        </div>`).join('');
    }

    const tong = Number(summary.tongSoPhong ?? summary.totalRooms) || 0;
    const daThue = Number(summary.phongDaThue ?? summary.rentedRooms) || 0;
    const trong = Number(summary.phongTrong ?? summary.availableRooms) || 0;
    const daThu = Number(summary.soTienDaThu ?? summary.paidAmount) || 0;
    const chuaThu = Number(summary.soTienChuaThu ?? summary.unpaidAmount) || 0;

    renderPie('dash-room-pie', [
      { label: 'Đang thuê', value: daThue, color: 'var(--sage)' },
      { label: 'Trống', value: trong, color: 'var(--amber)' },
      { label: 'Bảo trì / khác', value: Math.max(0, tong - daThue - trong), color: 'var(--dim)' }
    ], 'Chưa có phòng nào.');

    renderPie('dash-money-pie', [
      { label: 'Đã thu', value: daThu, display: `${fmtMoney(daThu)} đ`, color: 'var(--sage)' },
      { label: 'Còn nợ', value: chuaThu, display: `${fmtMoney(chuaThu)} đ`, color: 'var(--terracotta)' }
    ], 'Chưa phát sinh kỳ cước nào.');
  } catch (err) {
    toast(err.message, 'err');
  }
}

// Donut bằng SVG <circle> (không thư viện): chu vi 100 → dasharray = phần trăm, mỗi múi là
// một phần tử nên hover/<title> được từng phần. Mảng rỗng → text, không vẽ vòng đầy màu.
function renderPie(id, items, emptyText) {
  const el = document.getElementById(id);
  if (!el) return;
  const total = items.reduce((a, s) => a + Math.max(0, s.value), 0);
  if (total <= 0) { el.innerHTML = `<div class="pie-empty">${esc(emptyText)}</div>`; return; }

  let acc = 0;
  const segs = items.filter(i => i.value > 0).map((i, k) => {
    const pct = i.value / total * 100;
    const from = acc;
    acc += pct;
    const label = `${i.label}: ${i.display ?? i.value} (${pct.toFixed(0)}%)`;
    return `<circle class="pie-seg" cx="21" cy="21" r="15.9155" fill="none"
              stroke="${i.color}" stroke-width="6" stroke-dasharray="${pct.toFixed(2)} ${(100 - pct).toFixed(2)}"
              stroke-dashoffset="${(-from).toFixed(2)}" style="--i:${k}"><title>${esc(label)}</title></circle>`;
  }).join('');
  const label = items.map(i => `${i.label} ${i.display ?? i.value}`).join(', ');

  el.innerHTML = `
    <svg class="pie" viewBox="0 0 42 42" role="img" aria-label="${esc(label)}">${segs}</svg>
    <div class="pie-legend">${items.map(i => `
      <div class="it"><span class="sw" style="background:${i.color}"></span>
      <span class="nm">${esc(i.label)}</span><span class="vl">${esc(i.display ?? i.value)}</span></div>`).join('')}
    </div>`;
}

// Phòng có HĐ hiệu lực nhưng kỳ này chưa lập hóa đơn: nếu chỉ nhìn KPI "Còn nợ" thì
// chúng vô hình (chưa có hóa đơn để đếm), chủ trọ tưởng đã xong tháng.
function renderThieuHoaDon(summary) {
  const card = document.getElementById('dash-thieu-hd');
  if (!card) return;

  const so = Number(summary?.soPhongThieuHoaDon ?? 0) || 0;
  const danhSach = Array.isArray(summary?.phongThieuHoaDon) ? summary.phongThieuHoaDon : [];
  card.hidden = so === 0;

  const text = document.getElementById('dash-thieu-hd-text');
  if (text && so > 0) {
    const phong = danhSach.length ? esc(danhSach.join(', ')) : `${so} phòng`;
    text.innerHTML = `<b>${phong}</b> đã có hợp đồng hiệu lực nhưng chưa có hóa đơn kỳ này.
      Chốt chỉ số điện nước rồi lập hóa đơn để khách thanh toán được.`;
  }
}

function setKpi(id, value) {
  const el = document.getElementById(id);
  if (el) el.textContent = value ?? '—';
}
