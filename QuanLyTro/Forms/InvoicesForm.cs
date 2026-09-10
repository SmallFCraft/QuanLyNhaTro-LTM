using System.Globalization;
using QuanLyTro.Shared.Models;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Forms;

/// <summary>
/// Màn hình quản lý hóa đơn (Sub-Plan F - Step 5).
/// - Tháng (yyyy-MM), lọc chưa thu, list + detail breakdown.
/// - Lập hóa đơn: gửi CreateInvoiceRequest(RoomId, BillingMonth, OtherFees) — Server tính tiền (BR-10).
/// - Thu hóa đơn: gọi ActionNames.InvoicePay; khóa nút Thu trên dòng Paid (BR-11).
/// </summary>
public partial class InvoicesForm : UserControl
{
    private readonly List<InvoiceDto> _allInvoices = [];
    private readonly List<RoomDto> _rooms = [];
    private readonly Dictionary<int, string> _roomNames = [];
    private bool _showCreateForm;

    public InvoicesForm()
    {
        InitializeComponent();
        ApplyDesignTokens();
        ConfigureGrid();
        PopulateDefaultMonths();
        WireEvents();
    }

    private void ApplyDesignTokens()
    {
        btnReload.BackColor = ScreenTheme.From(ScreenTheme.Panel);
        btnReload.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
        btnReload.FlatStyle = FlatStyle.Flat;
        btnReload.FlatAppearance.BorderColor = ScreenTheme.From(ScreenTheme.OutlineBtn);

        btnToggleCreate.BackColor = ScreenTheme.From(ScreenTheme.Terracotta);
        btnToggleCreate.ForeColor = ScreenTheme.From(ScreenTheme.OnFill);
        btnToggleCreate.FlatStyle = FlatStyle.Flat;
        btnToggleCreate.FlatAppearance.BorderColor = ScreenTheme.From(ScreenTheme.Terracotta);

        btnCreateSubmit.BackColor = ScreenTheme.From(ScreenTheme.Terracotta);
        btnCreateSubmit.ForeColor = ScreenTheme.From(ScreenTheme.OnFill);
        btnCreateSubmit.FlatStyle = FlatStyle.Flat;
        btnCreateSubmit.FlatAppearance.BorderColor = ScreenTheme.From(ScreenTheme.Terracotta);

        cboMonth.BackColor = ScreenTheme.From(ScreenTheme.Field);
        cboMonth.ForeColor = ScreenTheme.From(ScreenTheme.Cream);

        cboRoom.BackColor = ScreenTheme.From(ScreenTheme.Field);
        cboRoom.ForeColor = ScreenTheme.From(ScreenTheme.Cream);

        numOtherFees.BackColor = ScreenTheme.From(ScreenTheme.Field);
        numOtherFees.ForeColor = ScreenTheme.From(ScreenTheme.Cream);

        pnlForm.Visible = false;
        pnlDetail.Visible = true;
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

        grid.DefaultCellStyle.BackColor = ScreenTheme.From(ScreenTheme.RowAlt);
        grid.DefaultCellStyle.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
        grid.DefaultCellStyle.Font = ScreenTheme.Body;
        grid.DefaultCellStyle.SelectionBackColor = ScreenTheme.From(ScreenTheme.Active);
        grid.DefaultCellStyle.SelectionForeColor = ScreenTheme.From(ScreenTheme.CreamLight);

        grid.AlternatingRowsDefaultCellStyle.BackColor = ScreenTheme.From(ScreenTheme.RowAlt2);
        grid.AlternatingRowsDefaultCellStyle.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
        grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = ScreenTheme.From(ScreenTheme.Active);
        grid.AlternatingRowsDefaultCellStyle.SelectionForeColor = ScreenTheme.From(ScreenTheme.CreamLight);

        grid.Columns.Clear();

        var colRoom = new DataGridViewTextBoxColumn
        {
            Name = "ColRoom",
            HeaderText = "PHÒNG",
            Width = 90,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft },
        };
        var colRoomFee = new DataGridViewTextBoxColumn
        {
            Name = "ColRoomFee",
            HeaderText = "TIỀN PHÒNG",
            Width = 110,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = ScreenTheme.Mono },
        };
        var colElec = new DataGridViewTextBoxColumn
        {
            Name = "ColElec",
            HeaderText = "ĐIỆN",
            Width = 95,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = ScreenTheme.Mono },
        };
        var colWater = new DataGridViewTextBoxColumn
        {
            Name = "ColWater",
            HeaderText = "NƯỚC",
            Width = 90,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = ScreenTheme.Mono },
        };
        var colOther = new DataGridViewTextBoxColumn
        {
            Name = "ColOther",
            HeaderText = "KHÁC",
            Width = 90,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = ScreenTheme.Mono },
        };
        var colTotal = new DataGridViewTextBoxColumn
        {
            Name = "ColTotal",
            HeaderText = "TỔNG TIỀN",
            Width = 120,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = ScreenTheme.Mono },
        };
        var colStatus = new DataGridViewTextBoxColumn
        {
            Name = "ColStatus",
            HeaderText = "TRẠNG THÁI",
            Width = 120,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
        };
        var colAction = new DataGridViewButtonColumn
        {
            Name = "ColAction",
            HeaderText = "THAO TÁC",
            Width = 85,
            UseColumnTextForButtonValue = false,
            FlatStyle = FlatStyle.Flat,
        };

        grid.Columns.AddRange(colRoom, colRoomFee, colElec, colWater, colOther, colTotal, colStatus, colAction);

        grid.CellPainting += Grid_CellPainting;
        grid.CellContentClick += Grid_CellContentClick;
        grid.SelectionChanged += Grid_SelectionChanged;
    }

    private void PopulateDefaultMonths()
    {
        var now = DateTime.Now;
        for (var i = 0; i < 12; i++)
        {
            cboMonth.Items.Add(now.AddMonths(-i).ToString("yyyy-MM", CultureInfo.InvariantCulture));
        }
        if (cboMonth.Items.Count > 0)
        {
            cboMonth.SelectedIndex = 0;
        }
    }

    private void WireEvents()
    {
        btnReload.Click += async (s, e) => await ReloadAsync();
        chkUnpaidOnly.CheckedChanged += (s, e) => ApplyFilter();
        cboMonth.SelectedIndexChanged += async (s, e) => await ReloadAsync();
        btnToggleCreate.Click += (s, e) => ToggleCreateForm();
        btnCreateSubmit.Click += async (s, e) => await SubmitCreateInvoiceAsync();
    }

    /// <summary>F5 tải lại — điều hướng bàn phím đầy đủ.</summary>
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.F5)
        {
            _ = ReloadAsync();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (!DesignMode)
        {
            await LoadRoomsAsync();
            await ReloadAsync();
        }
    }

    private void ToggleCreateForm()
    {
        _showCreateForm = !_showCreateForm;
        pnlForm.Visible = _showCreateForm;
        pnlDetail.Visible = !_showCreateForm;
        btnToggleCreate.Text = _showCreateForm ? "✕ Đóng form" : "+ Lập hóa đơn";
        lblError.Visible = false;
        lblError.Text = string.Empty;
    }

    public async Task LoadRoomsAsync()
    {
        try
        {
            var client = Form1.Client;
            if (client == null) return;
            var rooms = await client.SendAsync<object, List<RoomDto>>(
                ActionNames.RoomGetAll, new { }, CancellationToken.None);
            _rooms.Clear();
            _roomNames.Clear();
            cboRoom.Items.Clear();
            foreach (var r in rooms)
            {
                _rooms.Add(r);
                _roomNames[r.Id] = r.RoomNumber;
                cboRoom.Items.Add(new RoomComboItem(r.Id, r.RoomNumber));
            }
            if (cboRoom.Items.Count > 0)
            {
                cboRoom.SelectedIndex = 0;
            }
        }
        catch
        {
            // pony: phòng chưa load được -> sẽ reload khi user bấm Tải lại
        }
    }

    public async Task ReloadAsync()
    {
        var month = cboMonth.Text.Trim();
        if (string.IsNullOrWhiteSpace(month))
        {
            month = DateTime.Now.ToString("yyyy-MM", CultureInfo.InvariantCulture);
            cboMonth.Text = month;
        }

        try
        {
            var client = Form1.Client;
            if (client == null) return;

            var invoices = await client.SendAsync<object, List<InvoiceDto>>(
                ActionNames.InvoiceGetAll,
                new { BillingMonth = month },
                CancellationToken.None);

            _allInvoices.Clear();
            _allInvoices.AddRange(invoices);
            ApplyFilter();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi tải hóa đơn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void ApplyFilter()
    {
        grid.Rows.Clear();
        var filterUnpaid = chkUnpaidOnly.Checked;
        var count = 0;

        foreach (var inv in _allInvoices)
        {
            if (filterUnpaid && inv.Status != InvoiceStatus.Unpaid)
            {
                continue;
            }

            var roomLabel = _roomNames.TryGetValue(inv.RoomId, out var rn) ? rn : $"P.{inv.RoomId}";
            var rowIndex = grid.Rows.Add(
                roomLabel,
                ScreenTheme.Money(inv.RoomAmount),
                ScreenTheme.Money(inv.ElectricityAmount),
                ScreenTheme.Money(inv.WaterAmount),
                ScreenTheme.Money(inv.OtherFees),
                ScreenTheme.Money(inv.TotalAmount),
                inv.Status == InvoiceStatus.Paid ? "Đã thu" : "Chưa thu",
                inv.Status == InvoiceStatus.Paid ? "—" : "Thu");

            grid.Rows[rowIndex].Tag = inv;
            count++;
        }

        lblRowCount.Text = $"{count} bản ghi";
        if (grid.Rows.Count > 0)
        {
            grid.Rows[0].Selected = true;
            ShowDetail(grid.Rows[0].Tag as InvoiceDto);
        }
        else
        {
            ShowDetail(null);
        }
    }

    private void ShowDetail(InvoiceDto? inv)
    {
        if (inv == null)
        {
            lblEmptyDetail.Visible = true;
            lblDetailTitle.Text = "CHI TIẾT HÓA ĐƠN";
            lblDetailSubtitle.Text = "Chưa có hóa đơn nào";
            lblDetailStatus.Text = string.Empty;
            lblRoom.Visible = false;
            lblRoomValue.Visible = false;
            lblElectricity.Visible = false;
            lblElectricityValue.Visible = false;
            lblWater.Visible = false;
            lblWaterValue.Visible = false;
            lblOther.Visible = false;
            lblOtherValue.Visible = false;
            lblTotalValue.Text = "0 đ";
            return;
        }

        lblEmptyDetail.Visible = false;
        var roomLabel = _roomNames.TryGetValue(inv.RoomId, out var rn) ? rn : $"P.{inv.RoomId}";
        lblDetailTitle.Text = $"CHI TIẾT QUYẾT TOÁN CƯỚC THÁNG {inv.BillingMonth}";
        lblDetailSubtitle.Text = $"PHÒNG {roomLabel} · MÃ HĐ: HD-{inv.BillingMonth.Replace("-", "")}-{inv.RoomId}";

        lblDetailStatus.Text = inv.Status == InvoiceStatus.Paid ? "ĐÃ THANH TOÁN" : "CHƯA THANH TOÁN";
        lblDetailStatus.ForeColor = ScreenTheme.From(inv.Status == InvoiceStatus.Paid ? ScreenTheme.Sage : ScreenTheme.Terracotta);

        lblRoom.Visible = true;
        lblRoomValue.Visible = true;
        lblRoomValue.Text = ScreenTheme.Money(inv.RoomAmount);

        lblElectricity.Visible = true;
        lblElectricityValue.Visible = true;
        lblElectricityValue.Text = ScreenTheme.Money(inv.ElectricityAmount);

        lblWater.Visible = true;
        lblWaterValue.Visible = true;
        lblWaterValue.Text = ScreenTheme.Money(inv.WaterAmount);

        lblOther.Visible = true;
        lblOtherValue.Visible = true;
        lblOtherValue.Text = ScreenTheme.Money(inv.OtherFees);

        lblTotalValue.Text = ScreenTheme.Money(inv.TotalAmount);
    }

    private async Task SubmitCreateInvoiceAsync()
    {
        if (cboRoom.SelectedItem is not RoomComboItem item)
        {
            ShowError("Vui lòng chọn phòng cần lập hóa đơn.");
            return;
        }

        var month = cboMonth.Text.Trim();
        if (string.IsNullOrWhiteSpace(month))
        {
            ShowError("Vui lòng chọn tháng hóa đơn.");
            return;
        }

        var otherFees = numOtherFees.Value;

        try
        {
            lblError.Visible = false;
            var client = Form1.Client;
            if (client == null) return;

            var req = new CreateInvoiceRequest(item.RoomId, month, otherFees);
            await client.SendAsync<CreateInvoiceRequest, InvoiceDto>(
                ActionNames.InvoiceCreate, req, CancellationToken.None);

            MessageBox.Show(this,
                $"Đã lập hóa đơn thành công cho phòng {item.RoomNumber} tháng {month}.",
                "Lập hóa đơn",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            ToggleCreateForm();
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
            MessageBox.Show(this, ex.Message, "Không thể lập hóa đơn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void ShowError(string msg)
    {
        lblError.Text = msg;
        lblError.Visible = true;
    }

    private async void Grid_CellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        var colAction = grid.Columns["ColAction"];
        if (e.RowIndex < 0 || colAction == null || e.ColumnIndex != colAction.Index)
        {
            return;
        }

        if (grid.Rows[e.RowIndex].Tag is not InvoiceDto inv)
        {
            return;
        }

        // BR-11: đã thu thì bất biến, nút thu bị khóa
        if (inv.Status == InvoiceStatus.Paid)
        {
            return;
        }

        var roomLabel = _roomNames.TryGetValue(inv.RoomId, out var rn) ? rn : $"P.{inv.RoomId}";
        var confirm = MessageBox.Show(this,
            $"Xác nhận thu tiền hóa đơn phòng {roomLabel} tháng {inv.BillingMonth}?\nTổng số tiền: {ScreenTheme.Money(inv.TotalAmount)}",
            "Xác nhận thu tiền",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes)
        {
            return;
        }

        try
        {
            var client = Form1.Client;
            if (client == null) return;

            await client.SendAsync<object, bool>(
                ActionNames.InvoicePay,
                new { InvoiceId = inv.Id },
                CancellationToken.None);

            MessageBox.Show(this, "Đã ghi nhận thanh toán thành công.", "Thu tiền", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi thu tiền", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void Grid_SelectionChanged(object? sender, EventArgs e)
    {
        if (grid.SelectedRows.Count > 0 && grid.SelectedRows[0].Tag is InvoiceDto inv)
        {
            ShowDetail(inv);
        }
    }

    private void Grid_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.Graphics == null)
        {
            return;
        }

        var isSelected = (e.State & DataGridViewElementStates.Selected) != 0;
        var inv = grid.Rows[e.RowIndex].Tag as InvoiceDto;

        // Signature dòng chọn: mép trái có dải màu Terracotta 3px (DESIGN.md §3.2)
        if (isSelected && e.ColumnIndex == 0)
        {
            e.Paint(e.CellBounds, DataGridViewPaintParts.All);
            using var stripBrush = new SolidBrush(ScreenTheme.From(ScreenTheme.Terracotta));
            e.Graphics.FillRectangle(stripBrush, e.CellBounds.X, e.CellBounds.Y, 3, e.CellBounds.Height);
            e.Handled = true;
            return;
        }

        // Cột Trạng thái: vẽ tag ngữ nghĩa DESIGN.md §2.4
        var colStatus = grid.Columns["ColStatus"];
        if (colStatus != null && e.ColumnIndex == colStatus.Index && inv != null)
        {
            e.PaintBackground(e.CellBounds, isSelected);
            if (inv.Status == InvoiceStatus.Paid)
            {
                ScreenTheme.PaintTag(e.Graphics, e.CellBounds, "Đã thu",
                    ScreenTheme.Sage, ScreenTheme.SageBg, ScreenTheme.SageBorder);
            }
            else
            {
                ScreenTheme.PaintTag(e.Graphics, e.CellBounds, "Chưa thu",
                    ScreenTheme.Terracotta, ScreenTheme.DangerTagBg, ScreenTheme.DangerTagBorder);
            }
            e.Handled = true;
            return;
        }

        // Cột Thao tác (Thu): nút Terracotta khi Unpaid, disabled mờ khi Paid (BR-11)
        var colAction = grid.Columns["ColAction"];
        if (colAction != null && e.ColumnIndex == colAction.Index && inv != null)
        {
            e.PaintBackground(e.CellBounds, isSelected);
            if (inv.Status == InvoiceStatus.Paid)
            {
                ScreenTheme.PaintButton(e.Graphics, e.CellBounds, "Đã thu",
                    ScreenTheme.Panel, ScreenTheme.Dim, ScreenTheme.OutlineBtn);
            }
            else
            {
                ScreenTheme.PaintButton(e.Graphics, e.CellBounds, "Thu",
                    ScreenTheme.Terracotta, ScreenTheme.OnFill, ScreenTheme.Terracotta);
            }
            e.Handled = true;
        }
    }

    private sealed record RoomComboItem(int RoomId, string RoomNumber)
    {
        public override string ToString() => RoomNumber;
    }
}
