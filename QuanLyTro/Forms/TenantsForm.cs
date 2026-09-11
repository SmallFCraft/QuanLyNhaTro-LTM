using System.Globalization;
using QuanLyTro.Shared.Models;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Forms;

/// <summary>
/// Màn hình quản lý người thuê — bố cục theo template .toolbar / .tblwrap / .rowbn / .note.
/// ComboBox chọn phòng; grid người thuê; hàng nút Sửa · Chuyển phòng · Trả phòng · Xóa hồ sơ;
/// form nhập có ô mật khẩu (trống = 6 số cuối CCCD).
/// Client chỉ kiểm tra rỗng/định dạng; Server quyết BR-02 (sức chứa) và BR-03 (CCCD duy nhất).
/// </summary>
public partial class TenantsForm : UserControl
{
    private sealed record RoomComboItem(int Id, string DisplayText);

    private readonly List<TenantDto> _tenants = [];
    private TenantDto? _editingTenant;
    private bool _suppressRoomReload;

    public TenantsForm()
    {
        InitializeComponent();
        ConfigureGrid();
        WireEvents();

        pnlNote.Controls.Add(NoteBar.Create("Xóa hồ sơ chỉ khả dụng khi đã trả phòng (US-06)."));
    }

    private void ConfigureGrid()
    {
        dgvTenants.AutoGenerateColumns = false;
        dgvTenants.AllowUserToAddRows = false;
        dgvTenants.AllowUserToDeleteRows = false;
        dgvTenants.AllowUserToResizeRows = false;
        dgvTenants.ReadOnly = true;
        dgvTenants.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvTenants.MultiSelect = false;
        dgvTenants.RowHeadersVisible = false;
        dgvTenants.BorderStyle = BorderStyle.None;
        dgvTenants.BackgroundColor = ScreenTheme.From(ScreenTheme.GridBg);
        dgvTenants.GridColor = ScreenTheme.From(ScreenTheme.Subtle);
        dgvTenants.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        dgvTenants.RowTemplate.Height = 40;
        dgvTenants.ColumnHeadersHeight = 34;
        dgvTenants.EnableHeadersVisualStyles = false;
        dgvTenants.ScrollBars = ScrollBars.Vertical;

        dgvTenants.ColumnHeadersDefaultCellStyle.BackColor = ScreenTheme.From(ScreenTheme.HeaderBg);
        dgvTenants.ColumnHeadersDefaultCellStyle.ForeColor = ScreenTheme.From(ScreenTheme.Dim);
        dgvTenants.ColumnHeadersDefaultCellStyle.Font = ScreenTheme.HeaderFont;
        dgvTenants.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        dgvTenants.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

        dgvTenants.DefaultCellStyle.BackColor = ScreenTheme.From(ScreenTheme.Panel);
        dgvTenants.DefaultCellStyle.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
        dgvTenants.DefaultCellStyle.Font = ScreenTheme.Body;
        dgvTenants.DefaultCellStyle.SelectionBackColor = ScreenTheme.From(ScreenTheme.Active);
        dgvTenants.DefaultCellStyle.SelectionForeColor = ScreenTheme.From(ScreenTheme.CreamLight);
        dgvTenants.AlternatingRowsDefaultCellStyle.BackColor = ScreenTheme.From(ScreenTheme.RowAlt2);
        dgvTenants.AlternatingRowsDefaultCellStyle.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
        dgvTenants.AlternatingRowsDefaultCellStyle.SelectionBackColor = ScreenTheme.From(ScreenTheme.Active);
        dgvTenants.AlternatingRowsDefaultCellStyle.SelectionForeColor = ScreenTheme.From(ScreenTheme.CreamLight);

        dgvTenants.CellPainting += DgvTenants_CellPainting;
        dgvTenants.CellMouseClick += DgvTenants_CellMouseClick;
        dgvTenants.CellMouseMove += DgvTenants_CellMouseMove;
        dgvTenants.SelectionChanged += (_, _) => UpdateSelectionSummary();
    }

    private void WireEvents()
    {
        cboRoom.SelectedIndexChanged += async (_, _) => await OnRoomChangedAsync();
        btnAdd.Click += (_, _) => ShowAddForm();
        btnEdit.Click += (_, _) => ShowEditForm();
        btnMove.Click += (_, _) => ShowMoveDialog();
        btnCheckout.Click += async (_, _) => await CheckoutSelectedAsync();
        btnDelete.Click += async (_, _) => await DeleteSelectedAsync();
        btnSave.Click += async (_, _) => await SaveTenantAsync();
        btnCancel.Click += (_, _) => HideForm();
        dgvTenants.DoubleClick += (_, _) => ShowEditForm();
    }

    /// <summary>F5 tải lại · Esc đóng form nhập — giữ hành vi cũ.</summary>
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.F5)
        {
            _ = LoadRoomsAsync();
            return true;
        }

        if (keyData == Keys.Escape && pnlForm.Visible)
        {
            HideForm();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignMode)
        {
            return;
        }

        await LoadRoomsAsync();
    }

    public async Task LoadRoomsAsync()
    {
        try
        {
            var rooms = await Form1.Client.SendAsync<object, List<RoomDto>>(
                ActionNames.RoomGetAll, new { }, CancellationToken.None);

            _suppressRoomReload = true;
            cboRoom.Items.Clear();
            foreach (var r in rooms)
            {
                cboRoom.Items.Add(new RoomComboItem(r.Id, $"{r.RoomNumber} ({r.CurrentOccupants}/{r.MaxOccupants})"));
            }

            cboRoom.DisplayMember = nameof(RoomComboItem.DisplayText);
            cboRoom.ValueMember = nameof(RoomComboItem.Id);
            _suppressRoomReload = false;

            if (cboRoom.Items.Count > 0)
            {
                cboRoom.SelectedIndex = 0;
            }
            else
            {
                dgvTenants.Rows.Clear();
                _tenants.Clear();
                UpdateSelectionSummary();
            }

            tblFoot.SetTotal(dgvTenants.Rows.Count);
        }
        catch (Exception ex)
        {
            _suppressRoomReload = false;
            MessageBox.Show(this, ex.Message, "Lỗi tải danh sách phòng",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async Task OnRoomChangedAsync()
    {
        if (_suppressRoomReload)
        {
            return;
        }

        await ReloadAsync();
    }

    public async Task ReloadAsync()
    {
        if (cboRoom.SelectedItem is not RoomComboItem room)
        {
            dgvTenants.Rows.Clear();
            _tenants.Clear();
            tblFoot.SetTotal(0);
            UpdateSelectionSummary();
            return;
        }

        try
        {
            var tenants = await Form1.Client.SendAsync<object, List<TenantDto>>(
                ActionNames.TenantGetByRoom, new { RoomId = room.Id }, CancellationToken.None);

            _tenants.Clear();
            _tenants.AddRange(tenants);

            dgvTenants.Rows.Clear();
            foreach (var t in _tenants)
            {
                var index = dgvTenants.Rows.Add(
                    t.FullName,
                    t.IdCard,
                    t.DateOfBirth.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                    t.Phone,
                    t.Hometown,
                    t.IsTemporaryRegistered ? "Đã nộp" : "Chưa nộp",
                    string.Empty);
                dgvTenants.Rows[index].Tag = t;
            }

            tblFoot.SetTotal(dgvTenants.Rows.Count);
            if (dgvTenants.Rows.Count > 0)
            {
                dgvTenants.Rows[0].Selected = true;
            }

            UpdateSelectionSummary();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi tải danh sách người thuê",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private TenantDto? SelectedTenant => dgvTenants.CurrentRow?.Tag as TenantDto;

    /// <summary>Nút dòng theo selection: Xóa chỉ bật khi đã trả phòng (US-06 — room_id == null).</summary>
    private void UpdateSelectionSummary()
    {
        var tenant = SelectedTenant;
        tblFoot.SetSelected(tenant?.FullName);
        btnEdit.Enabled = tenant is not null && tenant.RoomId is not null;
        btnMove.Enabled = tenant is not null && tenant.RoomId is not null;
        btnCheckout.Enabled = tenant is not null && tenant.RoomId is not null;
        btnDelete.Enabled = tenant is not null && tenant.RoomId is null;
    }

    private void ShowAddForm()
    {
        if (cboRoom.SelectedItem is not RoomComboItem)
        {
            MessageBox.Show(this, "Vui lòng chọn phòng trước khi thêm người thuê.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        _editingTenant = null;
        lblFormTitle.Text = "THÊM NGƯỜI THUÊ";
        txtFullName.Clear();
        txtIdCard.Clear();
        dtpDateOfBirth.Value = new DateTime(2000, 1, 1);
        txtPhone.Clear();
        txtHometown.Clear();
        txtWorkplace.Clear();
        chkTemporary.Checked = false;
        txtPassword.Clear();
        lblError.Text = string.Empty;
        pnlForm.Visible = true;
        txtFullName.Focus();
    }

    private void ShowEditForm()
    {
        if (dgvTenants.CurrentRow?.Tag is not TenantDto tenant)
        {
            MessageBox.Show(this, "Vui lòng chọn người thuê cần sửa.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        _editingTenant = tenant;
        lblFormTitle.Text = $"SỬA — {tenant.FullName}";
        txtFullName.Text = tenant.FullName;
        txtIdCard.Text = tenant.IdCard;
        dtpDateOfBirth.Value = tenant.DateOfBirth.ToDateTime(TimeOnly.MinValue);
        txtPhone.Text = tenant.Phone;
        txtHometown.Text = tenant.Hometown;
        txtWorkplace.Text = tenant.Workplace ?? string.Empty;
        chkTemporary.Checked = tenant.IsTemporaryRegistered;
        txtPassword.Clear();
        lblError.Text = string.Empty;
        pnlForm.Visible = true;
        txtFullName.Focus();
    }

    /// <summary>Chuyển phòng: dialog chọn phòng mới rồi gửi UpdateAsync (giữ nguyên dữ liệu khác).</summary>
    private void ShowMoveDialog()
    {
        if (dgvTenants.CurrentRow?.Tag is not TenantDto tenant)
        {
            MessageBox.Show(this, "Vui lòng chọn người thuê cần chuyển phòng.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (tenant.RoomId is null)
        {
            MessageBox.Show(this, "Người này đã trả phòng, không thể chuyển phòng.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new Form
        {
            Text = $"Chuyển phòng — {tenant.FullName}",
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            StartPosition = FormStartPosition.CenterParent,
            Size = new Size(360, 170),
            BackColor = ScreenTheme.From(ScreenTheme.Card),
            ForeColor = ScreenTheme.From(ScreenTheme.Cream),
            Font = ScreenTheme.Body,
        };

        var lblPrompt = new Label
        {
            Text = "Chọn phòng mới:",
            Location = new Point(20, 16),
            AutoSize = true,
            ForeColor = ScreenTheme.From(ScreenTheme.Muted),
        };

        var cboNewRoom = new ComboBox
        {
            AccessibleName = "Phòng mới cho người thuê",
            Location = new Point(20, 40),
            Size = new Size(300, 23),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = ScreenTheme.From(ScreenTheme.Field),
            ForeColor = ScreenTheme.From(ScreenTheme.Cream),
            FlatStyle = FlatStyle.Flat,
        };

        var btnOk = new Button
        {
            AccessibleName = "Xác nhận chuyển phòng",
            Text = "Chuyển",
            DialogResult = DialogResult.OK,
            Location = new Point(148, 84),
            Size = new Size(88, 32),
            BackColor = ScreenTheme.From(ScreenTheme.Terracotta),
            ForeColor = ScreenTheme.From(ScreenTheme.OnFill),
            FlatStyle = FlatStyle.Flat,
        };
        btnOk.FlatAppearance.BorderSize = 0;

        var btnCancel = new Button
        {
            AccessibleName = "Hủy chuyển phòng",
            Text = "Hủy",
            DialogResult = DialogResult.Cancel,
            Location = new Point(244, 84),
            Size = new Size(76, 32),
            BackColor = ScreenTheme.From(ScreenTheme.MenuBtnBg),
            ForeColor = ScreenTheme.From(ScreenTheme.Cream),
            FlatStyle = FlatStyle.Flat,
        };
        btnCancel.FlatAppearance.BorderColor = ScreenTheme.From(ScreenTheme.OutlineBtn);

        foreach (var item in cboRoom.Items)
        {
            cboNewRoom.Items.Add(item);
        }

        cboNewRoom.DisplayMember = nameof(RoomComboItem.DisplayText);
        cboNewRoom.ValueMember = nameof(RoomComboItem.Id);

        var currentIndex = cboRoom.SelectedIndex;
        if (cboNewRoom.Items.Count > 0)
        {
            cboNewRoom.SelectedIndex = Math.Max(0, Math.Min(currentIndex, cboNewRoom.Items.Count - 1));
        }

        dialog.Controls.AddRange([lblPrompt, cboNewRoom, btnOk, btnCancel]);
        dialog.AcceptButton = btnOk;
        dialog.CancelButton = btnCancel;

        if (dialog.ShowDialog(this) != DialogResult.OK
            || cboNewRoom.SelectedItem is not RoomComboItem newRoom
            || newRoom.Id == tenant.RoomId)
        {
            return;
        }

        _ = MoveTenantAsync(tenant, newRoom);
    }

    private async Task MoveTenantAsync(TenantDto tenant, RoomComboItem newRoom)
    {
        try
        {
            var payload = new TenantDto(
                tenant.Id, newRoom.Id, tenant.FullName, tenant.DateOfBirth, tenant.IdCard,
                tenant.Phone, tenant.Hometown, tenant.Workplace, tenant.IsTemporaryRegistered);

            await Form1.Client.SendAsync<object, bool>(
                ActionNames.TenantUpdate, new { tenant = payload, plainPassword = (string?)null },
                CancellationToken.None);

            MessageBox.Show(this,
                $"Đã chuyển {tenant.FullName} sang phòng {newRoom.DisplayText.Split(' ')[0]}.",
                "Chuyển phòng", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            // Server message verbatim — BR-02 sức chứa phòng mới.
            MessageBox.Show(this, ex.Message, "Không thể chuyển phòng",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            await LoadRoomsAsync();
        }
    }

    private void HideForm()
    {
        pnlForm.Visible = false;
        _editingTenant = null;
        lblError.Text = string.Empty;
    }

    private async Task SaveTenantAsync()
    {
        lblError.Text = string.Empty;

        if (cboRoom.SelectedItem is not RoomComboItem room)
        {
            lblError.Text = "Vui lòng chọn phòng.";
            return;
        }

        var fullName = txtFullName.Text.Trim();
        if (fullName.Length == 0)
        {
            lblError.Text = "Vui lòng nhập họ tên.";
            txtFullName.Focus();
            return;
        }

        var idCard = txtIdCard.Text.Trim();
        if (idCard.Length != 12 || !idCard.All(char.IsAsciiDigit))
        {
            lblError.Text = "Số CCCD phải gồm đúng 12 chữ số.";
            txtIdCard.Focus();
            return;
        }

        var phone = txtPhone.Text.Trim();
        if (phone.Length == 0)
        {
            lblError.Text = "Vui lòng nhập số điện thoại.";
            txtPhone.Focus();
            return;
        }

        var hometown = txtHometown.Text.Trim();
        if (hometown.Length == 0)
        {
            lblError.Text = "Vui lòng nhập quê quán.";
            txtHometown.Focus();
            return;
        }

        var workplace = string.IsNullOrWhiteSpace(txtWorkplace.Text) ? null : txtWorkplace.Text.Trim();
        var password = string.IsNullOrWhiteSpace(txtPassword.Text) ? null : txtPassword.Text;

        var payload = new TenantDto(
            Id: _editingTenant?.Id ?? 0,
            RoomId: room.Id,
            FullName: fullName,
            DateOfBirth: DateOnly.FromDateTime(dtpDateOfBirth.Value),
            IdCard: idCard,
            Phone: phone,
            Hometown: hometown,
            Workplace: workplace,
            IsTemporaryRegistered: chkTemporary.Checked);

        btnSave.Enabled = false;
        try
        {
            var envelope = new { tenant = payload, plainPassword = password };

            if (_editingTenant is null)
            {
                await Form1.Client.SendAsync<object, TenantDto>(
                    ActionNames.TenantAdd, envelope, CancellationToken.None);
            }
            else
            {
                await Form1.Client.SendAsync<object, bool>(
                    ActionNames.TenantUpdate, envelope, CancellationToken.None);
            }

            HideForm();
            await LoadRoomsAsync();
        }
        catch (Exception ex)
        {
            // Server message verbatim (BR-02/BR-03), inputs preserved in the form.
            lblError.Text = ex.Message;
            MessageBox.Show(this, ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnSave.Enabled = true;
        }
    }

    private async Task CheckoutSelectedAsync()
    {
        if (dgvTenants.CurrentRow?.Tag is not TenantDto tenant)
        {
            MessageBox.Show(this, "Vui lòng chọn người thuê cần trả phòng.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var confirm = MessageBox.Show(this,
            $"Cho {tenant.FullName} trả phòng?", "Xác nhận trả phòng",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes)
        {
            return;
        }

        try
        {
            await Form1.Client.SendAsync<object, bool>(
                ActionNames.TenantCheckout, new { TenantId = tenant.Id }, CancellationToken.None);

            MessageBox.Show(this, $"Đã cho {tenant.FullName} trả phòng.", "Thành công",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadRoomsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Không thể trả phòng",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task DeleteSelectedAsync()
    {
        if (dgvTenants.CurrentRow?.Tag is not TenantDto tenant)
        {
            MessageBox.Show(this, "Vui lòng chọn hồ sơ cần xóa.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var confirm = MessageBox.Show(this,
            $"Xóa hồ sơ của {tenant.FullName}? Chỉ xóa được hồ sơ đã trả phòng.",
            "Xác nhận xóa hồ sơ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes)
        {
            return;
        }

        try
        {
            await Form1.Client.SendAsync<object, bool>(
                ActionNames.TenantDelete, new { TenantId = tenant.Id }, CancellationToken.None);

            MessageBox.Show(this, $"Đã xóa hồ sơ {tenant.FullName}.", "Thành công",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadRoomsAsync();
        }
        catch (Exception ex)
        {
            // Server message verbatim — US-06: chỉ xóa hồ sơ đã trả phòng.
            MessageBox.Show(this, ex.Message, "Không thể xóa hồ sơ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>Vị trí ô nút key (template fa-key) trong cột cuối — dùng chung cho vẽ và hit-test.</summary>
    private static Rectangle KeyBounds(Rectangle cell) => new(cell.X + 8, cell.Y + 6, cell.Width - 16, cell.Height - 12);

    private bool IsKeyCell(int rowIndex, int columnIndex) =>
        rowIndex >= 0 && rowIndex < dgvTenants.Rows.Count
        && columnIndex >= 0 && columnIndex < dgvTenants.Columns.Count
        && dgvTenants.Columns[columnIndex].Name == colKey.Name;

    private void DgvTenants_CellMouseMove(object? sender, DataGridViewCellMouseEventArgs e)
    {
        if (!IsKeyCell(e.RowIndex, e.ColumnIndex))
        {
            dgvTenants.Cursor = Cursors.Default;
            return;
        }

        dgvTenants.Cursor = KeyBounds(dgvTenants.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false))
            .Contains(e.Location)
            ? Cursors.Hand
            : Cursors.Default;
    }

    private void DgvTenants_CellMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
    {
        if (!IsKeyCell(e.RowIndex, e.ColumnIndex)
            || !KeyBounds(dgvTenants.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false)).Contains(e.Location)
            || dgvTenants.Rows[e.RowIndex].Tag is not TenantDto tenant)
        {
            return;
        }

        if (tenant.RoomId is null)
        {
            MessageBox.Show(this, "Người này đã trả phòng.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        _ = ResetPasswordAsync(tenant);
    }

    /// <summary>Nút key: đặt lại mật khẩu mặc định = 6 số cuối CCCD (đúng quy tắc ghi trên toolbar).</summary>
    private async Task ResetPasswordAsync(TenantDto tenant)
    {
        try
        {
            // ponytail: lặp lại quy tắc 6 số cuối CCCD ở client để bấm nút là đổi thật;
            // đổi sang action TENANT_RESET_PASSWORD nếu Server bổ sung.
            var digits = tenant.IdCard.Trim();
            var defaultPassword = digits.Length > 6 ? digits[^6..] : digits;

            await Form1.Client.SendAsync<object, bool>(
                ActionNames.TenantUpdate,
                new { tenant, plainPassword = defaultPassword },
                CancellationToken.None);

            MessageBox.Show(this,
                $"Đã đặt lại mật khẩu mặc định (6 số cuối CCCD) cho {tenant.FullName}.",
                "Đặt lại mật khẩu", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Không thể đặt lại mật khẩu",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DgvTenants_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.Graphics == null)
        {
            return;
        }

        var isSelected = (e.State & DataGridViewElementStates.Selected) != 0;

        // Dòng đang chọn: dải trái 3px Terracotta (DESIGN.md §3.2)
        if (isSelected && e.ColumnIndex == 0)
        {
            e.Paint(e.CellBounds, DataGridViewPaintParts.All);
            using var brush = new SolidBrush(ScreenTheme.From(ScreenTheme.Terracotta));
            e.Graphics.FillRectangle(brush, e.CellBounds.X, e.CellBounds.Y, 3, e.CellBounds.Height);
            e.Handled = true;
            return;
        }

        // Tag tạm trú (DESIGN.md §2.4)
        if (dgvTenants.Columns[e.ColumnIndex].Name == colTemporary.Name && !isSelected)
        {
            if (dgvTenants.Rows[e.RowIndex].Tag is TenantDto t)
            {
                e.PaintBackground(e.CellBounds, false);
                var (fore, back, border) = t.IsTemporaryRegistered
                    ? (ScreenTheme.Sage, ScreenTheme.SageBg, ScreenTheme.SageBorder)
                    : (ScreenTheme.Amber, ScreenTheme.AmberBg, ScreenTheme.AmberBorder);
                var text = t.IsTemporaryRegistered ? "Đã nộp" : "Chưa nộp";
                ScreenTheme.PaintTag(e.Graphics, e.CellBounds, text, fore, back, border);
                e.Handled = true;
            }

            return;
        }

        // Nút key (template fa-key): đặt lại mật khẩu mặc định
        if (dgvTenants.Columns[e.ColumnIndex].Name == colKey.Name
            && dgvTenants.Rows[e.RowIndex].Tag is TenantDto keyRow)
        {
            e.PaintBackground(e.CellBounds, isSelected);
            var enabled = keyRow.RoomId is not null;
            ScreenTheme.PaintButton(e.Graphics, KeyBounds(e.CellBounds), "Đặt lại MK",
                enabled ? ScreenTheme.MenuBtnBg : ScreenTheme.Panel,
                enabled ? ScreenTheme.Cream : ScreenTheme.Placeholder,
                ScreenTheme.OutlineBtn);
            e.Handled = true;
        }
    }
}
