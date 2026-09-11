using System.Globalization;
using QuanLyTro.Shared.Models;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Forms;

/// <summary>
/// Màn hình "Hóa đơn của tôi" — vai người thuê, chỉ đọc (Sub-Plan F - Step 7).
/// - Thẻ hồ sơ (tên, phòng, hạn HĐ, nhãn ReadOnly) + banner quá hạn + bảng quyết toán + lịch sử.
/// - Dữ liệu từ INVOICE_GET_MINE (BR-14: Server suy phòng từ token). KHÔNG có nút ghi.
/// </summary>
public partial class MyInvoicesForm : UserControl
{
    private readonly List<InvoiceDto> _invoices = [];
    private readonly Dictionary<int, string> _roomNames = [];

    public MyInvoicesForm()
    {
        InitializeComponent();
        ApplyDesignTokens();
        ConfigureGrid();
    }

    private void ApplyDesignTokens()
    {
        pnlReceipt.FillHex = ScreenTheme.Card;
        pnlReceipt.BorderHex = ScreenTheme.Hairline;
        pnlReceipt.Radius = 8;

        pnlProfile.FillHex = ScreenTheme.Card;
        pnlProfile.BorderHex = ScreenTheme.Hairline;
        pnlProfile.Radius = 8;

        pnlEmptyState.FillHex = ScreenTheme.Card;
        pnlEmptyState.BorderHex = ScreenTheme.Hairline;
        pnlEmptyState.Radius = 8;

        // Nút hàng .rowbn: primary copy + outline đồng bộ (DESIGN.md §3.3)
        btnCopyTransfer.BackColor = ScreenTheme.From(ScreenTheme.Terracotta);
        btnCopyTransfer.ForeColor = ScreenTheme.From(ScreenTheme.OnFill);
        btnCopyTransfer.FlatAppearance.BorderColor = ScreenTheme.From(ScreenTheme.Terracotta);
        btnCopyTransfer.FlatAppearance.MouseOverBackColor = ScreenTheme.From(ScreenTheme.TerracottaHover);

        btnSync.BackColor = ScreenTheme.From(ScreenTheme.Panel);
        btnSync.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
        btnSync.FlatAppearance.BorderColor = ScreenTheme.From(ScreenTheme.OutlineBtn);
        btnSync.FlatAppearance.MouseOverBackColor = ScreenTheme.From(ScreenTheme.HoverBg);

        grid.BackColor = ScreenTheme.From(ScreenTheme.GridBg);
        grid.AlternatingRowsDefaultCellStyle.BackColor = ScreenTheme.From(ScreenTheme.RowAlt);
        grid.AlternatingRowsDefaultCellStyle.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
    }

    private void ConfigureGrid()
    {
        grid.AutoGenerateColumns = false;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToResizeRows = false;
        grid.ReadOnly = true;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.RowHeadersVisible = false;
        grid.BackgroundColor = ScreenTheme.From(ScreenTheme.GridBg);
        grid.BorderStyle = BorderStyle.None;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.GridColor = ScreenTheme.From(ScreenTheme.Subtle);
        grid.RowTemplate.Height = 40;
        grid.EnableHeadersVisualStyles = false;

        grid.ColumnHeadersHeight = 34;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.ColumnHeadersDefaultCellStyle.BackColor = ScreenTheme.From(ScreenTheme.HeaderBg);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = ScreenTheme.From(ScreenTheme.Dim);
        grid.ColumnHeadersDefaultCellStyle.Font = ScreenTheme.HeaderFont;
        grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

        grid.DefaultCellStyle.BackColor = ScreenTheme.From(ScreenTheme.RowAlt2);
        grid.DefaultCellStyle.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
        grid.DefaultCellStyle.Font = ScreenTheme.Body;
        grid.DefaultCellStyle.SelectionBackColor = ScreenTheme.From(ScreenTheme.Active);
        grid.DefaultCellStyle.SelectionForeColor = ScreenTheme.From(ScreenTheme.CreamLight);

        grid.AlternatingRowsDefaultCellStyle.BackColor = ScreenTheme.From(ScreenTheme.RowAlt);
        grid.AlternatingRowsDefaultCellStyle.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
        grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = ScreenTheme.From(ScreenTheme.Active);
        grid.AlternatingRowsDefaultCellStyle.SelectionForeColor = ScreenTheme.From(ScreenTheme.CreamLight);

        grid.Columns.Clear();
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "ColMonth",
            HeaderText = "KỲ CƯỚC",
            Width = 150,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft, Font = ScreenTheme.Mono },
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "ColTotal",
            HeaderText = "TỔNG TIỀN",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = ScreenTheme.Mono },
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "ColPaidAt",
            HeaderText = "THỜI ĐIỂM ĐÓNG",
            Width = 220,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft, Font = ScreenTheme.Mono },
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "ColStatus",
            HeaderText = "TRẠNG THÁI",
            Width = 160,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
        });

        grid.CellPainting += Grid_CellPainting;
        grid.SelectionChanged += Grid_SelectionChanged;
        btnCopyTransfer.Click += BtnCopyTransfer_Click;
        btnSync.Click += async (s, e) => await RefreshAsync();
    }

    /// <summary>F5 đồng bộ lại — template .rowbn "Đồng bộ F5".</summary>
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.F5)
        {
            _ = RefreshAsync();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (!DesignMode)
        {
            await RefreshAsync();
        }
    }

    /// <summary>BR-14: chỉ gọi INVOICE_GET_MINE, server suy phòng từ token.</summary>
    public async Task RefreshAsync()
    {
        try
        {
            var client = Form1.Client;
            if (client == null) return;

            var invoices = await client.SendAsync<object, List<InvoiceDto>>(
                ActionNames.InvoiceGetMine, new { }, CancellationToken.None);

            _invoices.Clear();
            _invoices.AddRange(invoices);
            _invoices.Sort((a, b) => string.CompareOrdinal(b.BillingMonth, a.BillingMonth));

            FillHistory();
            BindReceipt(_invoices.Count > 0 ? _invoices[0] : null);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi tải hóa đơn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void FillHistory()
    {
        grid.Rows.Clear();
        foreach (var inv in _invoices)
        {
            var status = inv.Status == InvoiceStatus.Paid ? "Đã thanh toán" : "Chưa thanh toán";
            var rowIndex = grid.Rows.Add(
                inv.BillingMonth,
                ScreenTheme.Money(inv.TotalAmount),
                inv.Status == InvoiceStatus.Paid ? ScreenTheme.Day(inv.PaidAt) : "—",
                status);
            grid.Rows[rowIndex].Tag = inv;
        }

        lblRowCount.Text = $"{_invoices.Count} bản ghi";
        tblFoot.SetTotal(_invoices.Count);
        tblFoot.SetSelected(grid.Rows.Count > 0
            ? Convert.ToString(grid.Rows[0].Cells[0].Value, CultureInfo.InvariantCulture)
            : null);

        if (grid.Rows.Count > 0)
        {
            grid.Rows[0].Selected = true;
        }

        // DataGridView tự chọn dòng 0 lúc Rows.Add (Tag chưa gán) nên event không bật được nút copy.
        UpdateCopyButton(grid.Rows.Count > 0 ? grid.Rows[0].Tag as InvoiceDto : null);
    }

    private void BindReceipt(InvoiceDto? inv)
    {
        if (inv == null)
        {
            pnlReceipt.Visible = false;
            pnlBanner.Visible = false;
            pnlEmptyState.Visible = true;
            return;
        }

        pnlReceipt.Visible = true;
        pnlEmptyState.Visible = false;

        var roomLabel = _roomNames.TryGetValue(inv.RoomId, out var rn) ? rn : $"P.{inv.RoomId}";

        lblReceiptTitle.Text = $"CHI TIẾT QUYẾT TOÁN CƯỚC THÁNG {inv.BillingMonth}";
        lblReceiptCode.Text = $"MÃ SỐ BẢNG KÊ: HD-{inv.BillingMonth.Replace("-", "")}-{inv.RoomId:000}";
        lblReceiptStatus.Text = inv.Status == InvoiceStatus.Paid ? "Đã thanh toán" : "Chưa thanh toán";
        lblReceiptStatus.SetRole(
            inv.Status == InvoiceStatus.Paid ? ScreenTheme.Sage : ScreenTheme.Terracotta,
            inv.Status == InvoiceStatus.Paid ? ScreenTheme.SageBg : ScreenTheme.ErrorBgSoft,
            inv.Status == InvoiceStatus.Paid ? ScreenTheme.SageBorder : ScreenTheme.ErrorBorderSoft);

        lblLineRoomValue.Text = ScreenTheme.Money(inv.RoomAmount);
        lblLineElecValue.Text = ScreenTheme.Money(inv.ElectricityAmount);
        lblLineWaterValue.Text = ScreenTheme.Money(inv.WaterAmount);
        lblLineOtherValue.Text = ScreenTheme.Money(inv.OtherFees);

        // Dòng phụ chỉ số điện/nước: DTO hóa đơn chỉ có số tiền, không có chỉ số công tơ.
        lblElecReading.Text = "Đã chốt công tơ theo kỳ · Server tính tiền (BR-10)";
        lblWaterReading.Text = "Đồng hồ nhánh · Server tính tiền (BR-10)";

        lblTotalValue.Text = ScreenTheme.Money(inv.TotalAmount);
        lblDueDate.Text = inv.PaidAt.HasValue
            ? $"Đã thanh toán: {ScreenTheme.Day(inv.PaidAt)}"
            : "Hạn chót: — (xem quy định hợp đồng)";

        // Banner quá hạn: chỉ hiện khi còn nợ (chưa thu) — DESIGN.md §2.4
        pnlBanner.Visible = inv.Status != InvoiceStatus.Paid;
        lblBannerText.Text = $"Kỳ cước {inv.BillingMonth} chưa hoàn tất thanh toán. Còn nợ {ScreenTheme.Money(inv.TotalAmount)}.";
        lblRoomTag.Text = roomLabel;
    }

    private void Grid_SelectionChanged(object? sender, EventArgs e)
    {
        if (grid.SelectedRows.Count > 0 && grid.SelectedRows[0].Tag is InvoiceDto inv)
        {
            BindReceipt(inv);
            tblFoot.SetSelected(inv.BillingMonth);
            UpdateCopyButton(inv);
        }
        else
        {
            tblFoot.SetSelected(null);
            UpdateCopyButton(null);
        }
    }

    /// <summary>Nút copy chỉ bật khi đã chọn một kỳ (template .rowbn, BR-14: chỉ đọc).</summary>
    private void UpdateCopyButton(InvoiceDto? inv)
    {
        btnCopyTransfer.Enabled = inv != null;
    }

    private void BtnCopyTransfer_Click(object? sender, EventArgs e)
    {
        if (grid.SelectedRows.Count == 0 || grid.SelectedRows[0].Tag is not InvoiceDto inv)
        {
            return;
        }

        var room = _roomNames.TryGetValue(inv.RoomId, out var roomNumber) ? roomNumber : inv.RoomId.ToString(CultureInfo.InvariantCulture);
        var amount = inv.TotalAmount.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"));
        Clipboard.SetText($"Chuyen tien P{room} ky {inv.BillingMonth} so tien {amount}đ - QuanLy Tro Ngu Hanh Son");
    }

    private void Grid_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.Graphics == null)
        {
            return;
        }

        if (grid.Rows[e.RowIndex].Tag is not InvoiceDto inv)
        {
            return;
        }

        var isSelected = (e.State & DataGridViewElementStates.Selected) != 0;

        // Signature dòng chọn: dải trái 3px Terracotta (DESIGN.md §3.2)
        if (isSelected && e.ColumnIndex == 0)
        {
            e.Paint(e.CellBounds, DataGridViewPaintParts.All);
            using var brush = new SolidBrush(ScreenTheme.From(ScreenTheme.Terracotta));
            e.Graphics.FillRectangle(brush, e.CellBounds.X, e.CellBounds.Y, 3, e.CellBounds.Height);
            e.Handled = true;
            return;
        }

        var colStatus = grid.Columns["ColStatus"];
        if (colStatus != null && e.ColumnIndex == colStatus.Index)
        {
            e.PaintBackground(e.CellBounds, isSelected);
            if (inv.Status == InvoiceStatus.Paid)
            {
                ScreenTheme.PaintTag(e.Graphics, e.CellBounds, "Đã thanh toán",
                    ScreenTheme.Sage, ScreenTheme.SageBg, ScreenTheme.SageBorder);
            }
            else
            {
                ScreenTheme.PaintTag(e.Graphics, e.CellBounds, "Chưa thanh toán",
                    ScreenTheme.Terracotta, ScreenTheme.DangerTagBg, ScreenTheme.DangerTagBorder);
            }
            e.Handled = true;
        }
    }

    /// <summary>
    /// Nạp thông tin hồ sơ người thuê (Leader gọi sau khi đăng nhập).
    /// Tên phòng cũng được ghi vào bảng tra để hiển thị đúng ở bảng kê (DTO hóa đơn chỉ có RoomId).
    /// </summary>
    public void SetProfile(string fullName, int roomId, string roomNumber, string? contractExpiry)
    {
        var name = string.IsNullOrWhiteSpace(fullName) ? "Người thuê" : fullName;
        _roomNames[roomId] = roomNumber;

        lblTenantName.Text = name;
        lblAvatarInitials.Text = Initials(name);
        lblContractExpiry.Text = $"Hạn HĐ: {(string.IsNullOrWhiteSpace(contractExpiry) ? "—" : contractExpiry)}";
        lblRoomTag.Text = roomNumber;
    }

    public static string Initials(string fullName)
    {
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return "?";
        }
        if (parts.Length == 1)
        {
            return parts[0][..1].ToUpper(CultureInfo.InvariantCulture);
        }
        return (parts[0][..1] + parts[^1][..1]).ToUpper(CultureInfo.InvariantCulture);
    }

    private void LineRule_Paint(object? sender, PaintEventArgs e)
    {
        if (sender is not Control line)
        {
            return;
        }

        using var pen = new Pen(ScreenTheme.From(ScreenTheme.Subtle));
        e.Graphics.DrawLine(pen, 10, line.Height - 1, line.Width - 10, line.Height - 1);
    }
}
