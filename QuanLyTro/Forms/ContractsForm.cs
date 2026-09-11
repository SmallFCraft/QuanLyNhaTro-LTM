using System.Drawing.Drawing2D;
using System.Globalization;
using QuanLyTro.Shared.Models;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Forms;

public partial class ContractsForm : UserControl
{
    // Client-side representation matching the JSON payload of CONTRACT_GET_ALL
    public sealed record ContractGridRow(
        int Id,
        int RoomId,
        string RoomNumber,
        int RepresentativeTenantId,
        string RepresentativeName,
        DateOnly StartDate,
        DateOnly EndDate,
        decimal RentalPrice,
        decimal DepositAmount,
        ContractStatus Status,
        string? Notes
    )
    {
        public string StartDateText => StartDate.ToString("dd/MM/yyyy");
        public string EndDateText => EndDate.ToString("dd/MM/yyyy");
        public int RemainingDays => EndDate.DayNumber - DateOnly.FromDateTime(DateTime.Today).DayNumber;
        public string RemainingDaysText => Status == ContractStatus.Active
            ? (RemainingDays >= 0 ? $"{RemainingDays} ngày" : "Quá hạn")
            : "—";
        public string RentalPriceText => RentalPrice.ToString("N0", CultureInfo.InvariantCulture);
        public string DepositAmountText => DepositAmount.ToString("N0", CultureInfo.InvariantCulture);
        public string StatusText => Status switch
        {
            ContractStatus.Active => "Đang hiệu lực",
            ContractStatus.Expired => "Hết hạn",
            ContractStatus.Terminated => "Đã chấm dứt",
            _ => Status.ToString()
        };
    }

    /// <summary>Khớp JSON của <c>ContractListItem</c> phía Server (record không tham chiếu chéo project).</summary>
    public sealed record ContractGetAllItem(
        ContractDto? Contract,
        string? RoomNumber,
        string? RepresentativeName
    );

    public sealed record TerminateContractRequest(int ContractId, string? Notes);
    public sealed record RenewContractRequest(int ContractId, DateOnly NewEndDate);

    private sealed record RoomComboItem(int Id, string DisplayText);
    private sealed record TenantComboItem(int Id, string DisplayText);

    /// <summary>Bộ lọc toolbar — nhãn y hệt template #tab-contracts.</summary>
    private enum ContractFilter { All, Active, Expiring, Terminated }

    private sealed record FilterItem(ContractFilter Kind, string DisplayText);

    private const int ExpiringSoonDays = 30;

    private readonly List<ContractGridRow> _contractRows = [];
    private List<ContractGridRow> _viewRows = [];

    public ContractsForm()
    {
        InitializeComponent();
        WireEvents();

        noteContracts.Controls.Add(NoteBar.Create("Cột \"Còn\" < 30 ngày tô Terracotta: US-11."));
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignMode) return;
        // InitializeComponent chạy khi UserControl còn cỡ mặc định nên SplitterDistance bị kẹp;
        // đặt lại sau khi layout thật đã có (Panel1 là FixedPanel).
        splitMain.SplitterDistance = 320;
        _ = LoadRoomsAsync();
        _ = LoadContractsAsync();
    }

    private void WireEvents()
    {
        cboRoom.SelectedIndexChanged += async (_, _) => await OnRoomChangedAsync();
        btnCreate.Click += async (_, _) => await OnCreateContractAsync();
        btnTerminate.Click += async (_, _) => await OnTerminateContractAsync();
        btnRenew.Click += async (_, _) => await OnRenewContractAsync();
        btnRefresh.Click += async (_, _) => await LoadContractsAsync();

        txtSearch.TextChanged += (_, _) => ApplyFilter();
        cboFilter.SelectedIndexChanged += (_, _) => ApplyFilter();
        dgvContracts.SelectionChanged += (_, _) => UpdateFootSelection();

        dgvContracts.CellFormatting += DgvContracts_CellFormatting;
        // RowPostPaint: vẽ SAU khi cell tô nền, nếu không dải 3px bị SelectionBackColor phủ mất.
        dgvContracts.RowPostPaint += DgvContracts_RowPostPaint;
    }

    public async Task LoadRoomsAsync()
    {
        try
        {
            var rooms = await Form1.Client.SendAsync<object, List<RoomDto>>(
                ActionNames.RoomGetAll, new { }, CancellationToken.None);

            cboRoom.Items.Clear();
            foreach (var r in rooms)
            {
                cboRoom.Items.Add(new RoomComboItem(r.Id, $"{r.RoomNumber} ({r.Price:N0} đ)"));
            }

            cboRoom.DisplayMember = nameof(RoomComboItem.DisplayText);
            cboRoom.ValueMember = nameof(RoomComboItem.Id);

            if (cboRoom.Items.Count > 0)
            {
                cboRoom.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi tải phòng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async Task OnRoomChangedAsync()
    {
        if (cboRoom.SelectedItem is not RoomComboItem selectedRoom)
        {
            cboRepresentative.Items.Clear();
            return;
        }

        try
        {
            var tenants = await Form1.Client.SendAsync<object, List<TenantDto>>(
                ActionNames.TenantGetByRoom, new { RoomId = selectedRoom.Id }, CancellationToken.None);

            cboRepresentative.Items.Clear();
            foreach (var t in tenants)
            {
                cboRepresentative.Items.Add(new TenantComboItem(t.Id, $"{t.FullName} ({t.IdCard})"));
            }

            cboRepresentative.DisplayMember = nameof(TenantComboItem.DisplayText);
            cboRepresentative.ValueMember = nameof(TenantComboItem.Id);

            if (cboRepresentative.Items.Count > 0)
            {
                cboRepresentative.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi tải người thuê", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    public async Task LoadContractsAsync()
    {
        try
        {
            var items = await Form1.Client.SendAsync<object, List<ContractGetAllItem>>(
                ActionNames.ContractGetAll, new { }, CancellationToken.None);

            _contractRows.Clear();
            foreach (var item in items)
            {
                if (item.Contract is null) continue;
                var c = item.Contract;
                _contractRows.Add(new ContractGridRow(
                    c.Id,
                    c.RoomId,
                    item.RoomNumber ?? c.RoomId.ToString(),
                    c.RepresentativeTenantId,
                    item.RepresentativeName ?? c.RepresentativeTenantId.ToString(),
                    c.StartDate,
                    c.EndDate,
                    c.RentalPrice,
                    c.DepositAmount,
                    c.Status,
                    c.Notes
                ));
            }

            BuildFilterItems();
            UpdateKpis();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi tải danh sách hợp đồng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private static bool IsExpiringSoon(ContractGridRow r) =>
        r.Status == ContractStatus.Active && r.RemainingDays >= 0 && r.RemainingDays < ExpiringSoonDays;

    private int CountOf(ContractFilter kind) => _contractRows.Count(r => kind switch
    {
        ContractFilter.Active => r.Status == ContractStatus.Active,
        ContractFilter.Expiring => IsExpiringSoon(r),
        ContractFilter.Terminated => r.Status == ContractStatus.Terminated,
        _ => true
    });

    /// <summary>Dựng lại nhãn lọc kèm số đếm (template: "Tất cả hợp đồng (18)"), giữ nguyên dòng đang chọn.</summary>
    private void BuildFilterItems()
    {
        var keep = cboFilter.SelectedIndex;
        cboFilter.Items.Clear();
        foreach (var kind in new[] { ContractFilter.All, ContractFilter.Active, ContractFilter.Expiring, ContractFilter.Terminated })
        {
            var label = kind switch
            {
                ContractFilter.Active => "Đang hiệu lực",
                ContractFilter.Expiring => $"Sắp hết hạn ≤ {ExpiringSoonDays} ngày",
                ContractFilter.Terminated => "Đã chấm dứt",
                _ => "Tất cả hợp đồng"
            };
            cboFilter.Items.Add(new FilterItem(kind, $"{label} ({CountOf(kind)})"));
        }

        cboFilter.DisplayMember = nameof(FilterItem.DisplayText);
        cboFilter.SelectedIndex = keep is >= 0 and < 4 ? keep : 0;
    }

    private void ApplyFilter()
    {
        var kind = (cboFilter.SelectedItem as FilterItem)?.Kind ?? ContractFilter.All;
        var query = txtSearch.Text.Trim();

        IEnumerable<ContractGridRow> rows = kind switch
        {
            ContractFilter.Active => _contractRows.Where(r => r.Status == ContractStatus.Active),
            ContractFilter.Expiring => _contractRows.Where(IsExpiringSoon),
            ContractFilter.Terminated => _contractRows.Where(r => r.Status == ContractStatus.Terminated),
            _ => _contractRows
        };

        // Template gợi ý tìm theo CCCD/SĐT; payload CONTRACT_GET_ALL không có 2 trường đó.
        if (query.Length > 0)
        {
            rows = rows.Where(r =>
                Contains(r.RoomNumber, query)
                || Contains(r.RepresentativeName, query)
                || Contains(r.Notes ?? string.Empty, query));
        }

        _viewRows = rows.ToList();
        dgvContracts.DataSource = null;
        dgvContracts.DataSource = _viewRows;

        tblFoot.SetTotal(_viewRows.Count);
        if (_viewRows.Count > 0 && dgvContracts.Rows.Count > 0)
        {
            dgvContracts.Rows[0].Selected = true;
        }

        UpdateFootSelection();
    }

    private static bool Contains(string source, string query) =>
        source.Contains(query, StringComparison.CurrentCultureIgnoreCase);

    /// <summary>3 thẻ KPI template: Hiệu lực (x / tổng) · Sắp hết ≤ 30 ngày · Tổng cọc đang giữ.</summary>
    private void UpdateKpis()
    {
        var active = CountOf(ContractFilter.Active);
        var total = _contractRows.Count;
        // Tổng cọc = cọc của hợp đồng còn hiệu lực (HĐ đã chấm dứt/hết hạn đã trả cọc).
        var deposit = _contractRows.Where(r => r.Status == ContractStatus.Active).Sum(r => r.DepositAmount);

        lblKpiActive.Text = $"{active} / {total} HĐ";
        lblKpiExpiring.Text = CountOf(ContractFilter.Expiring).ToString("D2", CultureInfo.InvariantCulture);
        lblKpiDeposit.Text = ScreenTheme.Money(deposit);
    }

    private void UpdateFootSelection()
    {
        var label = dgvContracts.SelectedRows.Count > 0
            && dgvContracts.SelectedRows[0].DataBoundItem is ContractGridRow row
            ? row.RoomNumber
            : null;
        tblFoot.SetSelected(label);
    }

    private async Task OnCreateContractAsync()
    {
        // Client-side empty/format checks ONLY
        if (cboRoom.SelectedItem is not RoomComboItem selectedRoom)
        {
            MessageBox.Show("Vui lòng chọn phòng.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (cboRepresentative.SelectedItem is not TenantComboItem selectedTenant)
        {
            MessageBox.Show("Vui lòng chọn người đại diện.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var startDate = DateOnly.FromDateTime(dtpStartDate.Value);
        var endDate = DateOnly.FromDateTime(dtpEndDate.Value);

        if (endDate <= startDate)
        {
            MessageBox.Show("Ngày kết thúc phải sau ngày bắt đầu.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var contractToCreate = new ContractDto(
            Id: 0,
            RoomId: selectedRoom.Id,
            RepresentativeTenantId: selectedTenant.Id,
            StartDate: startDate,
            EndDate: endDate,
            RentalPrice: numRentalPrice.Value,
            DepositAmount: numDepositAmount.Value,
            Status: ContractStatus.Active,
            Notes: string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim()
        );

        try
        {
            await Form1.Client.SendAsync<ContractDto, ContractDto>(
                ActionNames.ContractCreate, contractToCreate, CancellationToken.None);

            MessageBox.Show("Tạo hợp đồng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadContractsAsync();
        }
        catch (Exception ex)
        {
            // Server error message verbatim, keep user inputs intact
            MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task OnTerminateContractAsync()
    {
        if (dgvContracts.SelectedRows.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn một hợp đồng để chấm dứt.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var selected = (ContractGridRow)dgvContracts.SelectedRows[0].DataBoundItem;
        if (selected.Status != ContractStatus.Active)
        {
            MessageBox.Show("Chỉ có thể chấm dứt hợp đồng đang hiệu lực.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirm = MessageBox.Show(
            $"Bạn có chắc muốn chấm dứt hợp đồng phòng {selected.RoomNumber} (Đại diện: {selected.RepresentativeName})?",
            "Xác nhận chấm dứt",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes) return;

        try
        {
            var req = new TerminateContractRequest(selected.Id, "Chấm dứt trước hạn");
            await Form1.Client.SendAsync<TerminateContractRequest, bool>(
                ActionNames.ContractTerminate, req, CancellationToken.None);

            MessageBox.Show("Chấm dứt hợp đồng thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadContractsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task OnRenewContractAsync()
    {
        if (dgvContracts.SelectedRows.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn một hợp đồng để gia hạn.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var selected = (ContractGridRow)dgvContracts.SelectedRows[0].DataBoundItem;
        if (selected.Status != ContractStatus.Active)
        {
            MessageBox.Show("Chỉ gia hạn được hợp đồng đang hiệu lực.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Show a simple modal dialog to choose new end date
        using var renewDialog = new Form
        {
            Text = $"Gia hạn hợp đồng — Phòng {selected.RoomNumber}",
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            StartPosition = FormStartPosition.CenterParent,
            Size = new Size(360, 190),
            BackColor = ScreenTheme.From(ScreenTheme.Card),
            ForeColor = ScreenTheme.From(ScreenTheme.Cream)
        };

        var lblPrompt = new Label
        {
            Text = $"Ngày kết thúc hiện tại: {selected.EndDateText}\nChọn ngày kết thúc mới:",
            Location = new Point(20, 16),
            Size = new Size(300, 36),
            Font = ScreenTheme.Body,
            ForeColor = ScreenTheme.From(ScreenTheme.Neutral)
        };

        var dtpNewEnd = new DateTimePicker
        {
            AccessibleName = "Ngày kết thúc mới của hợp đồng",
            Location = new Point(20, 60),
            Size = new Size(300, 24),
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "dd/MM/yyyy",
            Value = selected.EndDate.ToDateTime(TimeOnly.MinValue).AddMonths(6),
            CalendarForeColor = ScreenTheme.From(ScreenTheme.Cream),
            CalendarMonthBackground = ScreenTheme.From(ScreenTheme.Card)
        };

        var btnOk = new Button
        {
            AccessibleName = "Xác nhận gia hạn hợp đồng",
            Text = "Gia Hạn",
            DialogResult = DialogResult.OK,
            Location = new Point(140, 100),
            Size = new Size(88, 32),
            BackColor = ScreenTheme.From(ScreenTheme.Terracotta),
            ForeColor = ScreenTheme.From(ScreenTheme.OnFill),
            FlatStyle = FlatStyle.Flat
        };
        btnOk.FlatAppearance.BorderSize = 0;

        var btnCancel = new Button
        {
            AccessibleName = "Hủy gia hạn hợp đồng",
            Text = "Hủy",
            DialogResult = DialogResult.Cancel,
            Location = new Point(236, 100),
            Size = new Size(84, 32),
            BackColor = ScreenTheme.From(ScreenTheme.MenuBtnBg),
            ForeColor = ScreenTheme.From(ScreenTheme.Cream),
            FlatStyle = FlatStyle.Flat
        };
        btnCancel.FlatAppearance.BorderColor = ScreenTheme.From(ScreenTheme.OutlineBtn);

        renewDialog.Controls.AddRange([lblPrompt, dtpNewEnd, btnOk, btnCancel]);
        renewDialog.AcceptButton = btnOk;
        renewDialog.CancelButton = btnCancel;

        if (renewDialog.ShowDialog(this) != DialogResult.OK) return;

        var newEndDate = DateOnly.FromDateTime(dtpNewEnd.Value);
        if (newEndDate <= selected.EndDate)
        {
            MessageBox.Show("Ngày gia hạn phải sau ngày kết thúc hiện tại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            var req = new RenewContractRequest(selected.Id, newEndDate);
            await Form1.Client.SendAsync<RenewContractRequest, bool>(
                ActionNames.ContractRenew, req, CancellationToken.None);

            MessageBox.Show("Gia hạn hợp đồng thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadContractsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DgvContracts_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        // Lấy theo DataBoundItem, không theo chỉ số: bảng có thể đang bị lọc.
        if (e.RowIndex < 0 || dgvContracts.Rows[e.RowIndex].DataBoundItem is not ContractGridRow row) return;

        // Format Còn column: paint #D95D39 when < 30 days
        if (dgvContracts.Columns[e.ColumnIndex].Name == colRemainingDays.Name)
        {
            if (row.Status == ContractStatus.Active)
            {
                e.CellStyle!.ForeColor = ScreenTheme.From(
                    row.RemainingDays < ExpiringSoonDays ? ScreenTheme.Terracotta : ScreenTheme.Sage);
            }
            else
            {
                e.CellStyle!.ForeColor = ScreenTheme.From(ScreenTheme.Dim);
            }
        }
        // Semantic tag coloring for Status column
        else if (dgvContracts.Columns[e.ColumnIndex].Name == colStatus.Name)
        {
            switch (row.Status)
            {
                case ContractStatus.Active:
                    e.CellStyle!.ForeColor = ScreenTheme.From(ScreenTheme.Sage);
                    break;
                case ContractStatus.Expired:
                    e.CellStyle!.ForeColor = ScreenTheme.From(ScreenTheme.Amber);
                    break;
                case ContractStatus.Terminated:
                    e.CellStyle!.ForeColor = ScreenTheme.From(ScreenTheme.Neutral);
                    break;
            }
        }
    }

    private void DgvContracts_RowPostPaint(object? sender, DataGridViewRowPostPaintEventArgs e)
    {
        // Dòng đang chọn: dải trái 3px #D95D39 (DESIGN.md "Selected Signature").
        if ((e.State & DataGridViewElementStates.Selected) != 0)
        {
            using var brush = new SolidBrush(ScreenTheme.From(ScreenTheme.Terracotta));
            e.Graphics.FillRectangle(brush, e.RowBounds.Left, e.RowBounds.Top, 3, e.RowBounds.Height);
        }
    }

    /// <summary>
    /// Thẻ KPI template .kpi: nền card, viền hairline, bo 6px, dải đỉnh 2px mang ngữ nghĩa
    /// (Sage = hiệu lực, Terracotta = sắp hết hạn, border-emphasis-top = tổng cọc). DESIGN.md §3.4.
    /// </summary>
    internal sealed class KpiCard : Panel
    {
        public KpiCard()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer, true);
            BackColor = ScreenTheme.From(ScreenTheme.Base);
        }

        public string FillHex { get; set; } = ScreenTheme.Card;

        public string BorderHex { get; set; } = ScreenTheme.Hairline;

        public string TopHex { get; set; } = ScreenTheme.EmphasisTop;

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            ScreenTheme.PaintCard(e.Graphics, ClientRectangle, FillHex, BorderHex, 6);

            // Dải đỉnh 2px vẽ lọt trong 2 góc bo 6px.
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var pen = new Pen(ScreenTheme.From(TopHex), 2);
            e.Graphics.DrawLine(pen, 6, 1, Width - 7, 1);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Nền đã vẽ ở OnPaintBackground — không tô đè hình chữ nhật.
        }
    }
}
