using System.Globalization;
using QuanLyTro.Shared.Models;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Forms;

/// <summary>
/// Màn hình quản lý người thuê (Sub-Plan F — Step 2).
/// ComboBox chọn phòng; grid người thuê; form nhập có ô mật khẩu (trống = 6 số cuối CCCD).
/// Client chỉ kiểm tra rỗng/định dạng; Server quyết BR-02 (sức chứa) và BR-03 (CCCD duy nhất).
/// </summary>
public partial class TenantsForm : UserControl
{
    private sealed record RoomComboItem(int Id, string DisplayText);

    private static readonly Color SurfacePanel = ColorTranslator.FromHtml("#1A1F24");
    private static readonly Color SurfaceActive = ColorTranslator.FromHtml("#21262B");
    private static readonly Color HeaderBg = ColorTranslator.FromHtml("#1A2025");
    private static readonly Color GridBg = ColorTranslator.FromHtml("#14181C");
    private static readonly Color RowAlt = ColorTranslator.FromHtml("#161B1F");
    private static readonly Color BorderSubtle = ColorTranslator.FromHtml("#21272C");
    private static readonly Color TextDim = ColorTranslator.FromHtml("#767E88");
    private static readonly Color TextCream = ColorTranslator.FromHtml("#F4EFEA");
    private static readonly Color TextCreamLight = ColorTranslator.FromHtml("#FAF8F5");
    private static readonly Color Terracotta = ColorTranslator.FromHtml("#D95D39");

    private static readonly Font HeaderFont = new("Segoe UI", 8.25f, FontStyle.Bold);
    private static readonly Font BodyFont = new("Segoe UI", 9f);
    private static readonly Font MonoFont = new("Consolas", 9.5f);

    private readonly List<TenantDto> _tenants = [];
    private TenantDto? _editingTenant;
    private bool _suppressRoomReload;

    public TenantsForm()
    {
        InitializeComponent();
        ConfigureGrid();
        WireEvents();
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
        dgvTenants.BackgroundColor = GridBg;
        dgvTenants.GridColor = BorderSubtle;
        dgvTenants.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        dgvTenants.RowTemplate.Height = 40;
        dgvTenants.ColumnHeadersHeight = 34;
        dgvTenants.EnableHeadersVisualStyles = false;
        dgvTenants.ScrollBars = ScrollBars.Vertical;

        dgvTenants.ColumnHeadersDefaultCellStyle.BackColor = HeaderBg;
        dgvTenants.ColumnHeadersDefaultCellStyle.ForeColor = TextDim;
        dgvTenants.ColumnHeadersDefaultCellStyle.Font = HeaderFont;
        dgvTenants.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        dgvTenants.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

        dgvTenants.DefaultCellStyle.BackColor = SurfacePanel;
        dgvTenants.DefaultCellStyle.ForeColor = TextCream;
        dgvTenants.DefaultCellStyle.Font = BodyFont;
        dgvTenants.DefaultCellStyle.SelectionBackColor = SurfaceActive;
        dgvTenants.DefaultCellStyle.SelectionForeColor = TextCreamLight;
        dgvTenants.AlternatingRowsDefaultCellStyle.BackColor = RowAlt;
        dgvTenants.AlternatingRowsDefaultCellStyle.ForeColor = TextCream;
        dgvTenants.AlternatingRowsDefaultCellStyle.SelectionBackColor = SurfaceActive;
        dgvTenants.AlternatingRowsDefaultCellStyle.SelectionForeColor = TextCreamLight;

        dgvTenants.CellPainting += DgvTenants_CellPainting;
    }

    private void WireEvents()
    {
        cboRoom.SelectedIndexChanged += async (_, _) => await OnRoomChangedAsync();
        btnAdd.Click += (_, _) => ShowAddForm();
        btnEdit.Click += (_, _) => ShowEditForm();
        btnCheckout.Click += async (_, _) => await CheckoutSelectedAsync();
        btnDelete.Click += async (_, _) => await DeleteSelectedAsync();
        btnRefresh.Click += async (_, _) => await ReloadAsync();
        btnSave.Click += async (_, _) => await SaveTenantAsync();
        btnCancel.Click += (_, _) => HideForm();
        dgvTenants.DoubleClick += (_, _) => ShowEditForm();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.F5)
        {
            _ = ReloadAsync();
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
            }
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
                    t.IsTemporaryRegistered ? "Đã nộp" : "Chưa nộp");
                dgvTenants.Rows[index].Tag = t;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi tải danh sách người thuê",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
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
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            // Server message verbatim — US-06: chỉ xóa hồ sơ đã trả phòng.
            MessageBox.Show(this, ex.Message, "Không thể xóa hồ sơ",
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
            using var brush = new SolidBrush(Terracotta);
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
                    ? ("#8BD7A3", "#29322F", "#2E4A35")
                    : ("#E0AF68", "#2A2319", "#483A24");
                var text = t.IsTemporaryRegistered ? "Đã nộp" : "Chưa nộp";
                ScreenTheme.PaintTag(e.Graphics, e.CellBounds, text, fore, back, border);
                e.Handled = true;
            }
        }
    }
}
