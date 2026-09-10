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

    private readonly List<ContractGridRow> _contractRows = [];

    public ContractsForm()
    {
        InitializeComponent();
        WireEvents();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignMode) return;
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

            dgvContracts.DataSource = null;
            dgvContracts.DataSource = _contractRows;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi tải danh sách hợp đồng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
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
            BackColor = ColorTranslator.FromHtml("#181C1F"),
            ForeColor = ColorTranslator.FromHtml("#F4EFEA")
        };

        var lblPrompt = new Label
        {
            Text = $"Ngày kết thúc hiện tại: {selected.EndDateText}\nChọn ngày kết thúc mới:",
            Location = new Point(20, 16),
            Size = new Size(300, 36),
            Font = new Font("Segoe UI", 9F),
            ForeColor = ColorTranslator.FromHtml("#CAC6C1")
        };

        var dtpNewEnd = new DateTimePicker
        {
            Location = new Point(20, 60),
            Size = new Size(300, 24),
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "dd/MM/yyyy",
            Value = selected.EndDate.ToDateTime(TimeOnly.MinValue).AddMonths(6),
            CalendarForeColor = ColorTranslator.FromHtml("#F4EFEA"),
            CalendarMonthBackground = ColorTranslator.FromHtml("#181C1F")
        };

        var btnOk = new Button
        {
            Text = "Gia Hạn",
            DialogResult = DialogResult.OK,
            Location = new Point(140, 100),
            Size = new Size(88, 32),
            BackColor = ColorTranslator.FromHtml("#D95D39"),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnOk.FlatAppearance.BorderSize = 0;

        var btnCancel = new Button
        {
            Text = "Hủy",
            DialogResult = DialogResult.Cancel,
            Location = new Point(236, 100),
            Size = new Size(84, 32),
            BackColor = ColorTranslator.FromHtml("#1C2126"),
            ForeColor = ColorTranslator.FromHtml("#F4EFEA"),
            FlatStyle = FlatStyle.Flat
        };
        btnCancel.FlatAppearance.BorderColor = ColorTranslator.FromHtml("#2E373F");

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
        if (e.RowIndex < 0 || e.RowIndex >= _contractRows.Count) return;

        var row = _contractRows[e.RowIndex];

        // Format Còn column: paint #D95D39 when < 30 days
        if (dgvContracts.Columns[e.ColumnIndex].Name == colRemainingDays.Name)
        {
            if (row.Status == ContractStatus.Active)
            {
                if (row.RemainingDays < 30)
                {
                    e.CellStyle!.ForeColor = ColorTranslator.FromHtml("#D95D39");
                }
                else
                {
                    e.CellStyle!.ForeColor = ColorTranslator.FromHtml("#8BD7A3");
                }
            }
            else
            {
                e.CellStyle!.ForeColor = ColorTranslator.FromHtml("#767E88");
            }
        }
        // Semantic tag coloring for Status column
        else if (dgvContracts.Columns[e.ColumnIndex].Name == colStatus.Name)
        {
            switch (row.Status)
            {
                case ContractStatus.Active:
                    e.CellStyle!.ForeColor = ColorTranslator.FromHtml("#8BD7A3");
                    break;
                case ContractStatus.Expired:
                    e.CellStyle!.ForeColor = ColorTranslator.FromHtml("#E0AF68");
                    break;
                case ContractStatus.Terminated:
                    e.CellStyle!.ForeColor = ColorTranslator.FromHtml("#CAC6C1");
                    break;
            }
        }
    }

    private void DgvContracts_RowPostPaint(object? sender, DataGridViewRowPostPaintEventArgs e)
    {
        // Dòng đang chọn: dải trái 3px #D95D39 (DESIGN.md "Selected Signature").
        if ((e.State & DataGridViewElementStates.Selected) != 0)
        {
            using var brush = new SolidBrush(ColorTranslator.FromHtml("#D95D39"));
            e.Graphics.FillRectangle(brush, e.RowBounds.Left, e.RowBounds.Top, 3, e.RowBounds.Height);
        }
    }
}
