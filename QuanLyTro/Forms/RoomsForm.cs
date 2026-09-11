using System.Globalization;
using QuanLyTro.Shared.Models;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Forms;

/// <summary>
/// Màn hình quản lý phòng — bố cục theo template .toolbar / .tblwrap / .tblfoot / .note.
/// DataGridView readonly: số phòng, giá, sức chứa, đang ở, trạng thái, mô tả + cột nút Sửa/Xóa.
/// Thêm/Sửa/Xóa/F5 gọi ROOM_*, client chỉ kiểm tra empty/malformed, Server quyết BR-01/BR-12.
/// </summary>
public partial class RoomsForm : UserControl
{
    private const string RowActionEdit = "Sửa";
    private const string RowActionDelete = "Xóa";

    public sealed record StatusFilterItem(RoomStatus? Status, string DisplayText);

    private static readonly (string Fore, string Back, string Border) TagAvailable =
        (ScreenTheme.Neutral, ScreenTheme.EmptyBg, ScreenTheme.TagEmptyBorder);
    private static readonly (string Fore, string Back, string Border) TagRented =
        (ScreenTheme.Sage, ScreenTheme.SageBg, ScreenTheme.SageBorder);
    private static readonly (string Fore, string Back, string Border) TagMaintenance =
        (ScreenTheme.Amber, ScreenTheme.AmberBg, ScreenTheme.AmberBorder);

    private readonly List<RoomDto> _rooms = [];
    private RoomDto? _editingRoom;
    private bool _suppressFilterReload;

    public RoomsForm()
    {
        InitializeComponent();
        ConfigureGrid();
        PopulateStatusFilter();
        WireEvents();

        pnlNote.Controls.Add(NoteBar.Create(
            "Chỉ xóa được phòng trống (BR-12). Số người = đếm trực tiếp từ tenants."));
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
        dgvRooms.BackgroundColor = ScreenTheme.From(ScreenTheme.GridBg);
        dgvRooms.GridColor = ScreenTheme.From(ScreenTheme.Subtle);
        dgvRooms.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        dgvRooms.RowTemplate.Height = 40;
        dgvRooms.ColumnHeadersHeight = 34;
        dgvRooms.EnableHeadersVisualStyles = false;
        dgvRooms.ScrollBars = ScrollBars.Vertical;
        dgvRooms.DefaultCellStyle.SelectionBackColor = ScreenTheme.From(ScreenTheme.Active);
        dgvRooms.AlternatingRowsDefaultCellStyle.SelectionBackColor = ScreenTheme.From(ScreenTheme.Active);

        dgvRooms.ColumnHeadersDefaultCellStyle.BackColor = ScreenTheme.From(ScreenTheme.HeaderBg);
        dgvRooms.ColumnHeadersDefaultCellStyle.ForeColor = ScreenTheme.From(ScreenTheme.Dim);
        dgvRooms.ColumnHeadersDefaultCellStyle.Font = ScreenTheme.HeaderFont;
        dgvRooms.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        dgvRooms.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

        dgvRooms.DefaultCellStyle.BackColor = ScreenTheme.From(ScreenTheme.Panel);
        dgvRooms.DefaultCellStyle.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
        dgvRooms.DefaultCellStyle.Font = ScreenTheme.Body;
        dgvRooms.DefaultCellStyle.SelectionForeColor = ScreenTheme.From(ScreenTheme.CreamLight);
        dgvRooms.AlternatingRowsDefaultCellStyle.BackColor = ScreenTheme.From(ScreenTheme.RowAlt2);
        dgvRooms.AlternatingRowsDefaultCellStyle.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
        dgvRooms.AlternatingRowsDefaultCellStyle.SelectionForeColor = ScreenTheme.From(ScreenTheme.CreamLight);

        dgvRooms.CellPainting += DgvRooms_CellPainting;
        dgvRooms.CellMouseClick += DgvRooms_CellMouseClick;
        dgvRooms.CellMouseMove += DgvRooms_CellMouseMove;
        dgvRooms.SelectionChanged += (_, _) => UpdateSelectionSummary();
    }

    private void PopulateStatusFilter()
    {
        _suppressFilterReload = true;
        cboStatusFilter.Items.Clear();
        cboStatusFilter.Items.Add(new StatusFilterItem(null, "Tất cả trạng thái"));
        cboStatusFilter.Items.Add(new StatusFilterItem(RoomStatus.Available, "Trống"));
        cboStatusFilter.Items.Add(new StatusFilterItem(RoomStatus.Rented, "Đang thuê"));
        cboStatusFilter.Items.Add(new StatusFilterItem(RoomStatus.Maintenance, "Bảo trì"));
        cboStatusFilter.DisplayMember = nameof(StatusFilterItem.DisplayText);
        cboStatusFilter.SelectedIndex = 0;
        _suppressFilterReload = false;
    }

    private void WireEvents()
    {
        btnAdd.Click += (_, _) => ShowAddForm();
        btnEdit.Click += (_, _) => ShowEditForm();
        btnDelete.Click += async (_, _) => await DeleteSelectedAsync();
        btnRefresh.Click += async (_, _) => await ReloadAsync();
        btnSave.Click += async (_, _) => await SaveRoomAsync();
        btnCancel.Click += (_, _) => HideForm();
        txtSearch.TextChanged += (_, _) => ApplyFilter();
        cboStatusFilter.SelectedIndexChanged += (_, _) =>
        {
            if (!_suppressFilterReload)
            {
                ApplyFilter();
            }
        };
        dgvRooms.DoubleClick += (_, _) => ShowEditForm();
    }

    /// <summary>F1 thêm · F2 sửa · F5 tải lại — điều hướng bàn phím như template.</summary>
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.F5)
        {
            _ = ReloadAsync();
            return true;
        }

        if (keyData == Keys.F1)
        {
            ShowAddForm();
            return true;
        }

        if (keyData == Keys.F2)
        {
            ShowEditForm();
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
            ApplyFilter();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi tải danh sách phòng",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void ApplyFilter()
    {
        var keyword = txtSearch.Text.Trim();
        var status = (cboStatusFilter.SelectedItem as StatusFilterItem)?.Status;

        var selectedId = SelectedRoom?.Id;
        dgvRooms.Rows.Clear();

        foreach (var r in _rooms)
        {
            if (status is { } wanted && r.Status != wanted)
            {
                continue;
            }

            if (keyword.Length > 0
                && !r.RoomNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                && !(r.Description ?? string.Empty).Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var index = dgvRooms.Rows.Add(
                r.RoomNumber,
                r.Price.ToString("N0", CultureInfo.InvariantCulture),
                r.MaxOccupants.ToString(CultureInfo.InvariantCulture),
                $"{r.CurrentOccupants}/{r.MaxOccupants}",
                FormatStatus(r.Status),
                r.Description ?? string.Empty,
                string.Empty);
            dgvRooms.Rows[index].Tag = r;
        }

        if (selectedId is { } id)
        {
            SelectRoomById(id);
        }

        tblFoot.SetTotal(dgvRooms.Rows.Count);
        UpdateSelectionSummary();
    }

    private void SelectRoomById(int id)
    {
        foreach (DataGridViewRow row in dgvRooms.Rows)
        {
            if (row.Tag is RoomDto room && room.Id == id)
            {
                row.Selected = true;
                return;
            }
        }
    }

    private RoomDto? SelectedRoom => dgvRooms.CurrentRow?.Tag as RoomDto;

    private void UpdateSelectionSummary()
    {
        // Template .sel-txt in tên phòng đang chọn; chưa chọn thì để trống.
        tblFoot.SetSelected(SelectedRoom?.RoomNumber);
        btnEdit.Enabled = SelectedRoom is not null;
        btnDelete.Enabled = SelectedRoom is not null;
    }

    private static string FormatStatus(RoomStatus status) => status switch
    {
        RoomStatus.Available => "Phòng trống",
        RoomStatus.Rented => "Đang thuê",
        RoomStatus.Maintenance => "Bảo trì",
        _ => status.ToString(),
    };

    private static (string Fore, string Back, string Border) TagColors(RoomStatus status) => status switch
    {
        RoomStatus.Rented => TagRented,
        RoomStatus.Maintenance => TagMaintenance,
        _ => TagAvailable,
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
        if (SelectedRoom is not { } room)
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
            if (cboStatus.Items[i] is StatusFilterItem item && item.Status == room.Status)
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
        var status = (cboStatus.SelectedItem as StatusFilterItem)?.Status ?? RoomStatus.Available;
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
        if (SelectedRoom is not { } room)
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

    /// <summary>Vị trí ô nút Sửa / Xóa trong cột hành động — dùng chung cho vẽ và hit-test.</summary>
    private static (Rectangle Edit, Rectangle Delete) ActionBounds(Rectangle cell)
    {
        const int w = 62;
        const int gap = 6;
        var del = new Rectangle(cell.Right - gap - w, cell.Y, w, cell.Height);
        var edit = new Rectangle(del.X - gap - w, cell.Y, w, cell.Height);
        return (edit, del);
    }

    private bool IsActionCell(int rowIndex, int columnIndex) =>
        rowIndex >= 0 && rowIndex < dgvRooms.Rows.Count
        && columnIndex >= 0 && columnIndex < dgvRooms.Columns.Count
        && dgvRooms.Columns[columnIndex].Name == colActions.Name;

    private void DgvRooms_CellMouseMove(object? sender, DataGridViewCellMouseEventArgs e)
    {
        if (!IsActionCell(e.RowIndex, e.ColumnIndex))
        {
            dgvRooms.Cursor = Cursors.Default;
            return;
        }

        var (edit, delete) = ActionBounds(dgvRooms.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false));
        dgvRooms.Cursor = edit.Contains(e.Location) || delete.Contains(e.Location)
            ? Cursors.Hand
            : Cursors.Default;
    }

    private void DgvRooms_CellMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
    {
        if (!IsActionCell(e.RowIndex, e.ColumnIndex))
        {
            return;
        }

        var (edit, delete) = ActionBounds(dgvRooms.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false));
        if (edit.Contains(e.Location))
        {
            ShowEditForm();
        }
        else if (delete.Contains(e.Location))
        {
            _ = DeleteSelectedAsync();
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
            using var brush = new SolidBrush(ScreenTheme.From(ScreenTheme.Terracotta));
            e.Graphics.FillRectangle(brush, e.CellBounds.X, e.CellBounds.Y, 3, e.CellBounds.Height);
            e.Handled = true;
            return;
        }

        // Cột hành động: 2 nút outline Sửa · danger Xóa như template
        if (dgvRooms.Columns[e.ColumnIndex].Name == colActions.Name
            && dgvRooms.Rows[e.RowIndex].Tag is RoomDto actionRow)
        {
            e.PaintBackground(e.CellBounds, isSelected);
            var (edit, delete) = ActionBounds(e.CellBounds);
            var canDelete = actionRow.Status == RoomStatus.Available;

            ScreenTheme.PaintButton(e.Graphics, edit, RowActionEdit,
                ScreenTheme.MenuBtnBg, ScreenTheme.Cream, ScreenTheme.OutlineBtn);

            if (canDelete)
            {
                ScreenTheme.PaintButton(e.Graphics, delete, RowActionDelete,
                    ScreenTheme.HoverBg, ScreenTheme.Error, ScreenTheme.DangerBtn);
            }
            else
            {
                ScreenTheme.PaintButton(e.Graphics, delete, RowActionDelete,
                    ScreenTheme.Panel, ScreenTheme.Placeholder, ScreenTheme.OutlineBtn);
            }

            e.Handled = true;
            return;
        }

        // Tag trạng thái (DESIGN.md §2.4)
        if (dgvRooms.Columns[e.ColumnIndex].Name == colStatus.Name && !isSelected
            && dgvRooms.Rows[e.RowIndex].Tag is RoomDto r)
        {
            e.PaintBackground(e.CellBounds, false);
            var (fore, back, border) = TagColors(r.Status);
            ScreenTheme.PaintTag(e.Graphics, e.CellBounds, FormatStatus(r.Status), fore, back, border);
            e.Handled = true;
        }
    }
}
