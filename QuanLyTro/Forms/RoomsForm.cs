using System.Globalization;
using QuanLyTro.Shared.Models;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Forms;

/// <summary>
/// Màn hình quản lý phòng (Sub-Plan F — Step 1).
/// DataGridView readonly: số phòng, giá, sức chứa, số người, trạng thái, mô tả.
/// Thêm/Sửa/Xóa/F5 gọi ROOM_*, client chỉ kiểm tra empty/malformed, Server quyết BR-01/BR-12.
/// </summary>
public partial class RoomsForm : UserControl
{
    private sealed record StatusComboItem(RoomStatus Status, string DisplayText);

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

    private readonly List<RoomDto> _rooms = [];
    private RoomDto? _editingRoom;

    public RoomsForm()
    {
        InitializeComponent();
        ConfigureGrid();
        PopulateStatusCombo();
        WireEvents();
    }

    private void ConfigureGrid()
    {
        dgvRooms.AutoGenerateColumns = false;
        dgvRooms.AllowUserToAddRows = false;
        dgvRooms.AllowUserToDeleteRows = false;
        dgvRooms.AllowUserToResizeRows = false;
        dgvRooms.ReadOnly = true;
        dgvRooms.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvRooms.MultiSelect = false;
        dgvRooms.RowHeadersVisible = false;
        dgvRooms.BorderStyle = BorderStyle.None;
        dgvRooms.BackgroundColor = GridBg;
        dgvRooms.GridColor = BorderSubtle;
        dgvRooms.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        dgvRooms.RowTemplate.Height = 40;
        dgvRooms.ColumnHeadersHeight = 34;
        dgvRooms.EnableHeadersVisualStyles = false;
        dgvRooms.ScrollBars = ScrollBars.Vertical;

        dgvRooms.ColumnHeadersDefaultCellStyle.BackColor = HeaderBg;
        dgvRooms.ColumnHeadersDefaultCellStyle.ForeColor = TextDim;
        dgvRooms.ColumnHeadersDefaultCellStyle.Font = HeaderFont;
        dgvRooms.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        dgvRooms.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

        dgvRooms.DefaultCellStyle.BackColor = SurfacePanel;
        dgvRooms.DefaultCellStyle.ForeColor = TextCream;
        dgvRooms.DefaultCellStyle.Font = BodyFont;
        dgvRooms.DefaultCellStyle.SelectionBackColor = SurfaceActive;
        dgvRooms.DefaultCellStyle.SelectionForeColor = TextCreamLight;
        dgvRooms.AlternatingRowsDefaultCellStyle.BackColor = RowAlt;
        dgvRooms.AlternatingRowsDefaultCellStyle.ForeColor = TextCream;
        dgvRooms.AlternatingRowsDefaultCellStyle.SelectionBackColor = SurfaceActive;
        dgvRooms.AlternatingRowsDefaultCellStyle.SelectionForeColor = TextCreamLight;

        dgvRooms.CellPainting += DgvRooms_CellPainting;
    }

    private void PopulateStatusCombo()
    {
        cboStatus.Items.Clear();
        cboStatus.Items.Add(new StatusComboItem(RoomStatus.Available, "Trống (Available)"));
        cboStatus.Items.Add(new StatusComboItem(RoomStatus.Rented, "Đang thuê (Rented)"));
        cboStatus.Items.Add(new StatusComboItem(RoomStatus.Maintenance, "Bảo trì (Maintenance)"));
        cboStatus.DisplayMember = nameof(StatusComboItem.DisplayText);
        cboStatus.ValueMember = nameof(StatusComboItem.Status);
        cboStatus.SelectedIndex = 0;
    }

    private void WireEvents()
    {
        btnAdd.Click += (_, _) => ShowAddForm();
        btnEdit.Click += (_, _) => ShowEditForm();
        btnDelete.Click += async (_, _) => await DeleteSelectedAsync();
        btnRefresh.Click += async (_, _) => await ReloadAsync();
        btnSave.Click += async (_, _) => await SaveRoomAsync();
        btnCancel.Click += (_, _) => HideForm();
        dgvRooms.DoubleClick += (_, _) => ShowEditForm();
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
        if (!DesignMode)
        {
            await ReloadAsync();
        }
    }

    public async Task ReloadAsync()
    {
        try
        {
            var rooms = await Form1.Client.SendAsync<object, List<RoomDto>>(
                ActionNames.RoomGetAll, new { }, CancellationToken.None);

            _rooms.Clear();
            _rooms.AddRange(rooms);

            dgvRooms.Rows.Clear();
            foreach (var r in _rooms)
            {
                var index = dgvRooms.Rows.Add(
                    r.RoomNumber,
                    r.Price.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " đ",
                    r.MaxOccupants.ToString(CultureInfo.InvariantCulture),
                    $"{r.CurrentOccupants}/{r.MaxOccupants}",
                    FormatStatus(r.Status),
                    r.Description ?? string.Empty);
                dgvRooms.Rows[index].Tag = r;
            }

            lblCount.Text = $"{_rooms.Count} bản ghi";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi tải danh sách phòng",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private static string FormatStatus(RoomStatus status) => status switch
    {
        RoomStatus.Available => "Phòng trống",
        RoomStatus.Rented => "Đang thuê",
        RoomStatus.Maintenance => "Bảo trì",
        _ => status.ToString(),
    };

    private void ShowAddForm()
    {
        _editingRoom = null;
        lblFormTitle.Text = "THÊM PHÒNG MỚI";
        txtRoomNumber.Clear();
        numPrice.Value = 2000000m;
        numMaxOccupants.Value = 2;
        cboStatus.SelectedIndex = 0;
        txtDescription.Clear();
        lblError.Text = string.Empty;
        pnlForm.Visible = true;
        txtRoomNumber.Focus();
    }

    private void ShowEditForm()
    {
        if (dgvRooms.CurrentRow?.Tag is not RoomDto room)
        {
            MessageBox.Show(this, "Vui lòng chọn phòng cần sửa.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        _editingRoom = room;
        lblFormTitle.Text = $"SỬA PHÒNG {room.RoomNumber}";
        txtRoomNumber.Text = room.RoomNumber;
        numPrice.Value = room.Price > 0 && room.Price <= numPrice.Maximum ? room.Price : numPrice.Minimum;
        numMaxOccupants.Value = room.MaxOccupants >= numMaxOccupants.Minimum && room.MaxOccupants <= numMaxOccupants.Maximum
            ? room.MaxOccupants
            : 2;

        for (var i = 0; i < cboStatus.Items.Count; i++)
        {
            if (cboStatus.Items[i] is StatusComboItem item && item.Status == room.Status)
            {
                cboStatus.SelectedIndex = i;
                break;
            }
        }

        txtDescription.Text = room.Description ?? string.Empty;
        lblError.Text = string.Empty;
        pnlForm.Visible = true;
        txtRoomNumber.Focus();
    }

    private void HideForm()
    {
        pnlForm.Visible = false;
        _editingRoom = null;
        lblError.Text = string.Empty;
    }

    private async Task SaveRoomAsync()
    {
        lblError.Text = string.Empty;
        var roomNumber = txtRoomNumber.Text.Trim();
        if (roomNumber.Length == 0)
        {
            lblError.Text = "Vui lòng nhập số phòng.";
            txtRoomNumber.Focus();
            return;
        }

        var price = numPrice.Value;
        if (price <= 0)
        {
            lblError.Text = "Giá thuê phải lớn hơn 0.";
            numPrice.Focus();
            return;
        }

        var max = (int)numMaxOccupants.Value;
        var status = (cboStatus.SelectedItem as StatusComboItem)?.Status ?? RoomStatus.Available;
        var desc = string.IsNullOrWhiteSpace(txtDescription.Text) ? null : txtDescription.Text.Trim();

        btnSave.Enabled = false;
        try
        {
            if (_editingRoom is null)
            {
                var payload = new RoomDto(0, roomNumber, price, max, status, desc, 0);
                await Form1.Client.SendAsync<RoomDto, RoomDto>(
                    ActionNames.RoomAdd, payload, CancellationToken.None);
            }
            else
            {
                var payload = new RoomDto(_editingRoom.Id, roomNumber, price, max, status, desc, _editingRoom.CurrentOccupants);
                await Form1.Client.SendAsync<RoomDto, bool>(
                    ActionNames.RoomUpdate, payload, CancellationToken.None);
            }

            HideForm();
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            // Server error message verbatim, preserve user's inputs.
            lblError.Text = ex.Message;
            MessageBox.Show(this, ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnSave.Enabled = true;
        }
    }

    private async Task DeleteSelectedAsync()
    {
        if (dgvRooms.CurrentRow?.Tag is not RoomDto room)
        {
            MessageBox.Show(this, "Vui lòng chọn phòng cần xóa.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var confirm = MessageBox.Show(
            this,
            $"Bạn có chắc chắn muốn xóa phòng {room.RoomNumber}?",
            "Xác nhận xóa phòng",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes)
        {
            return;
        }

        try
        {
            await Form1.Client.SendAsync<object, bool>(
                ActionNames.RoomDelete, new { RoomId = room.Id }, CancellationToken.None);

            MessageBox.Show(this, $"Đã xóa phòng {room.RoomNumber}.", "Thành công",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            // Message tiếng Việt từ Server (BR-12: chỉ xóa phòng trống).
            MessageBox.Show(this, ex.Message, "Không thể xóa phòng",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DgvRooms_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
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

        // Tag trạng thái (DESIGN.md §2.4)
        if (dgvRooms.Columns[e.ColumnIndex].Name == colStatus.Name && !isSelected)
        {
            if (dgvRooms.Rows[e.RowIndex].Tag is RoomDto r)
            {
                e.PaintBackground(e.CellBounds, false);
                var (fore, back, border) = r.Status switch
                {
                    RoomStatus.Available => ("#CAC6C1", "#1C2227", "#313A42"),
                    RoomStatus.Rented => ("#8BD7A3", "#29322F", "#2E4A35"),
                    RoomStatus.Maintenance => ("#E0AF68", "#2A2319", "#483A24"),
                    _ => ("#CAC6C1", "#1C2227", "#313A42"),
                };
                ScreenTheme.PaintTag(e.Graphics, e.CellBounds, FormatStatus(r.Status), fore, back, border);
                e.Handled = true;
            }
        }
    }
}
