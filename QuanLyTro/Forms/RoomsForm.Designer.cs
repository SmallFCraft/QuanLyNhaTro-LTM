#nullable enable

namespace QuanLyTro.Forms;

partial class RoomsForm
{
    private System.ComponentModel.IContainer? components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.pnlToolbar = new QuanLyTro.Forms.ToolbarPanel();
        this.txtSearch = new System.Windows.Forms.TextBox();
        this.cboStatusFilter = new System.Windows.Forms.ComboBox();
        this.btnRefresh = new System.Windows.Forms.Button();
        this.pnlDivider = new System.Windows.Forms.Panel();
        this.btnDelete = new System.Windows.Forms.Button();
        this.btnEdit = new System.Windows.Forms.Button();
        this.btnAdd = new System.Windows.Forms.Button();
        this.pnlGridHost = new System.Windows.Forms.Panel();
        this.tblFoot = new QuanLyTro.Forms.TblFootPanel();
        this.pnlNote = new System.Windows.Forms.Panel();
        this.dgvRooms = new System.Windows.Forms.DataGridView();
        this.colRoomNumber = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colPrice = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colMaxOccupants = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colCurrentOccupants = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colActions = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.pnlForm = new System.Windows.Forms.Panel();
        this.lblFormTitle = new System.Windows.Forms.Label();
        this.lblRoomNumber = new System.Windows.Forms.Label();
        this.txtRoomNumber = new System.Windows.Forms.TextBox();
        this.lblPrice = new System.Windows.Forms.Label();
        this.numPrice = new System.Windows.Forms.NumericUpDown();
        this.lblMaxOccupants = new System.Windows.Forms.Label();
        this.numMaxOccupants = new System.Windows.Forms.NumericUpDown();
        this.lblStatus = new System.Windows.Forms.Label();
        this.cboStatus = new System.Windows.Forms.ComboBox();
        this.lblDescription = new System.Windows.Forms.Label();
        this.txtDescription = new System.Windows.Forms.TextBox();
        this.lblError = new System.Windows.Forms.Label();
        this.btnCancel = new System.Windows.Forms.Button();
        this.btnSave = new System.Windows.Forms.Button();
        this.pnlToolbar.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.numPrice)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numMaxOccupants)).BeginInit();
        this.pnlGridHost.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvRooms)).BeginInit();
        this.pnlForm.SuspendLayout();
        this.SuspendLayout();

        //
        // pnlToolbar
        //
        this.pnlToolbar.AccessibleName = "Thanh công cụ quản lý phòng";
        this.pnlToolbar.Controls.Add(this.txtSearch);
        this.pnlToolbar.Controls.Add(this.cboStatusFilter);
        this.pnlToolbar.Controls.Add(this.btnRefresh);
        this.pnlToolbar.Controls.Add(this.pnlDivider);
        this.pnlToolbar.Controls.Add(this.btnDelete);
        this.pnlToolbar.Controls.Add(this.btnEdit);
        this.pnlToolbar.Controls.Add(this.btnAdd);
        this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
        this.pnlToolbar.Location = new System.Drawing.Point(14, 14);
        this.pnlToolbar.Name = "pnlToolbar";
        this.pnlToolbar.Size = new System.Drawing.Size(972, 44);
        this.pnlToolbar.TabIndex = 0;

        //
        // txtSearch
        //
        this.txtSearch.AccessibleName = "Tìm phòng theo số phòng hoặc mô tả";
        this.txtSearch.BackColor = ScreenTheme.From(ScreenTheme.Field);
        this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.txtSearch.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
        this.txtSearch.Location = new System.Drawing.Point(12, 9);
        this.txtSearch.MaxLength = 100;
        this.txtSearch.Name = "txtSearch";
        this.txtSearch.PlaceholderText = "Tìm phòng...";
        this.txtSearch.Size = new System.Drawing.Size(170, 23);
        this.txtSearch.TabIndex = 0;

        //
        // cboStatusFilter
        //
        this.cboStatusFilter.AccessibleName = "Lọc theo trạng thái phòng";
        this.cboStatusFilter.BackColor = ScreenTheme.From(ScreenTheme.Field);
        this.cboStatusFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboStatusFilter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.cboStatusFilter.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
        this.cboStatusFilter.Location = new System.Drawing.Point(190, 9);
        this.cboStatusFilter.Name = "cboStatusFilter";
        this.cboStatusFilter.Size = new System.Drawing.Size(150, 23);
        this.cboStatusFilter.TabIndex = 1;

        //
        // btnAdd
        //
        this.btnAdd.AccessibleName = "Thêm phòng mới (F1)";
        this.btnAdd.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
        this.btnAdd.BackColor = ScreenTheme.From(ScreenTheme.MenuBtnBg);
        this.btnAdd.FlatAppearance.BorderColor = ScreenTheme.From(ScreenTheme.OutlineBtn);
        this.btnAdd.FlatAppearance.MouseOverBackColor = ScreenTheme.From(ScreenTheme.HoverBg);
        this.btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnAdd.Font = ScreenTheme.Body;
        this.btnAdd.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
        this.btnAdd.Location = new System.Drawing.Point(612, 6);
        this.btnAdd.Name = "btnAdd";
        this.btnAdd.Size = new System.Drawing.Size(96, 32);
        this.btnAdd.TabIndex = 2;
        this.btnAdd.Text = "+ Thêm (F1)";
        this.btnAdd.UseVisualStyleBackColor = false;

        //
        // btnEdit
        //
        this.btnEdit.AccessibleName = "Sửa phòng đã chọn (F2)";
        this.btnEdit.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
        this.btnEdit.BackColor = ScreenTheme.From(ScreenTheme.MenuBtnBg);
        this.btnEdit.FlatAppearance.BorderColor = ScreenTheme.From(ScreenTheme.OutlineBtn);
        this.btnEdit.FlatAppearance.MouseOverBackColor = ScreenTheme.From(ScreenTheme.HoverBg);
        this.btnEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnEdit.Font = ScreenTheme.Body;
        this.btnEdit.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
        this.btnEdit.Location = new System.Drawing.Point(714, 6);
        this.btnEdit.Name = "btnEdit";
        this.btnEdit.Size = new System.Drawing.Size(84, 32);
        this.btnEdit.TabIndex = 3;
        this.btnEdit.Text = "Sửa (F2)";
        this.btnEdit.UseVisualStyleBackColor = false;

        //
        // btnDelete
        //
        this.btnDelete.AccessibleName = "Xóa phòng đã chọn";
        this.btnDelete.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
        this.btnDelete.BackColor = ScreenTheme.From(ScreenTheme.HoverBg);
        this.btnDelete.FlatAppearance.BorderColor = ScreenTheme.From(ScreenTheme.DangerBtn);
        this.btnDelete.FlatAppearance.MouseOverBackColor = ScreenTheme.From(ScreenTheme.ErrorBgSoft);
        this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnDelete.Font = ScreenTheme.Body;
        this.btnDelete.ForeColor = ScreenTheme.From(ScreenTheme.Error);
        this.btnDelete.Location = new System.Drawing.Point(804, 6);
        this.btnDelete.Name = "btnDelete";
        this.btnDelete.Size = new System.Drawing.Size(84, 32);
        this.btnDelete.TabIndex = 4;
        this.btnDelete.Text = "Xóa";
        this.btnDelete.UseVisualStyleBackColor = false;

        //
        // pnlDivider
        //
        this.pnlDivider.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
        this.pnlDivider.BackColor = ScreenTheme.From(ScreenTheme.Subtle);
        this.pnlDivider.Location = new System.Drawing.Point(897, 14);
        this.pnlDivider.Name = "pnlDivider";
        this.pnlDivider.Size = new System.Drawing.Size(1, 16);
        this.pnlDivider.TabIndex = 5;

        //
        // btnRefresh
        //
        this.btnRefresh.AccessibleName = "Tải lại danh sách phòng (F5)";
        this.btnRefresh.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
        this.btnRefresh.BackColor = ScreenTheme.From(ScreenTheme.Base);
        this.btnRefresh.FlatAppearance.BorderColor = ScreenTheme.From(ScreenTheme.Base);
        this.btnRefresh.FlatAppearance.MouseOverBackColor = ScreenTheme.From(ScreenTheme.HoverBg);
        this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnRefresh.Font = ScreenTheme.Body;
        this.btnRefresh.ForeColor = ScreenTheme.From(ScreenTheme.Dim);
        this.btnRefresh.Location = new System.Drawing.Point(906, 6);
        this.btnRefresh.Name = "btnRefresh";
        this.btnRefresh.Size = new System.Drawing.Size(60, 32);
        this.btnRefresh.TabIndex = 6;
        this.btnRefresh.Text = "F5";
        this.btnRefresh.UseVisualStyleBackColor = false;

        //
        // pnlNote — .note (nội dung do NoteBar.Create gắn trong RoomsForm.cs)
        //

        //
        // pnlGridHost
        //
        this.pnlGridHost.AccessibleName = "Khu vực bảng danh sách phòng";
        this.pnlGridHost.BackColor = ScreenTheme.From(ScreenTheme.Base);
        this.pnlGridHost.Controls.Add(this.dgvRooms);
        this.pnlGridHost.Controls.Add(this.tblFoot);
        this.pnlGridHost.Controls.Add(this.pnlNote);
        this.pnlGridHost.Dock = System.Windows.Forms.DockStyle.Fill;
        this.pnlGridHost.Location = new System.Drawing.Point(14, 58);
        this.pnlGridHost.Name = "pnlGridHost";
        this.pnlGridHost.Padding = new System.Windows.Forms.Padding(0, 12, 0, 0);
        this.pnlGridHost.Size = new System.Drawing.Size(632, 514);
        this.pnlGridHost.TabIndex = 1;
        this.pnlNote.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.pnlNote.Location = new System.Drawing.Point(0, 480);
        this.pnlNote.Name = "pnlNote";
        this.pnlNote.Size = new System.Drawing.Size(632, 34);
        this.pnlNote.TabIndex = 2;

        //
        // dgvRooms
        //
        this.dgvRooms.AccessibleName = "Bảng danh sách phòng";
        this.dgvRooms.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        this.dgvRooms.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colRoomNumber,
            this.colPrice,
            this.colMaxOccupants,
            this.colCurrentOccupants,
            this.colStatus,
            this.colDescription,
            this.colActions});
        this.dgvRooms.Dock = System.Windows.Forms.DockStyle.Fill;
        this.dgvRooms.Location = new System.Drawing.Point(0, 12);
        this.dgvRooms.Name = "dgvRooms";
        this.dgvRooms.Size = new System.Drawing.Size(632, 440);
        this.dgvRooms.TabIndex = 0;

        //
        // colRoomNumber
        //
        this.colRoomNumber.HeaderText = "SỐ PHÒNG";
        this.colRoomNumber.Name = "colRoomNumber";
        this.colRoomNumber.ReadOnly = true;
        this.colRoomNumber.Width = 110;

        //
        // colPrice
        //
        this.colPrice.DefaultCellStyle = new System.Windows.Forms.DataGridViewCellStyle
        {
            Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight,
            Font = ScreenTheme.Mono,
        };
        this.colPrice.HeaderText = "GIÁ/THÁNG";
        this.colPrice.Name = "colPrice";
        this.colPrice.ReadOnly = true;
        this.colPrice.Width = 130;

        //
        // colMaxOccupants
        //
        this.colMaxOccupants.DefaultCellStyle = new System.Windows.Forms.DataGridViewCellStyle
        {
            Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight,
            Font = ScreenTheme.Mono,
        };
        this.colMaxOccupants.HeaderText = "SỨC CHỨA";
        this.colMaxOccupants.Name = "colMaxOccupants";
        this.colMaxOccupants.ReadOnly = true;
        this.colMaxOccupants.Width = 90;

        //
        // colCurrentOccupants
        //
        this.colCurrentOccupants.DefaultCellStyle = new System.Windows.Forms.DataGridViewCellStyle
        {
            Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight,
            Font = ScreenTheme.Mono,
        };
        this.colCurrentOccupants.HeaderText = "ĐANG Ở";
        this.colCurrentOccupants.Name = "colCurrentOccupants";
        this.colCurrentOccupants.ReadOnly = true;
        this.colCurrentOccupants.Width = 90;

        //
        // colStatus
        //
        this.colStatus.HeaderText = "TRẠNG THÁI";
        this.colStatus.Name = "colStatus";
        this.colStatus.ReadOnly = true;
        this.colStatus.Width = 130;

        //
        // colDescription
        //
        this.colDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
        this.colDescription.HeaderText = "MÔ TẢ";
        this.colDescription.Name = "colDescription";
        this.colDescription.ReadOnly = true;

        //
        // colActions
        //
        this.colActions.HeaderText = "";
        this.colActions.Name = "colActions";
        this.colActions.ReadOnly = true;
        this.colActions.Resizable = System.Windows.Forms.DataGridViewTriState.False;
        this.colActions.Width = 150;

        //
        // tblFoot
        //
        this.tblFoot.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.tblFoot.Location = new System.Drawing.Point(0, 452);
        this.tblFoot.Name = "tblFoot";
        this.tblFoot.Size = new System.Drawing.Size(632, 28);
        this.tblFoot.TabIndex = 1;

        //
        // pnlForm
        //
        this.pnlForm.BackColor = ScreenTheme.From(ScreenTheme.Card);
        this.pnlForm.Controls.Add(this.lblFormTitle);
        this.pnlForm.Controls.Add(this.lblRoomNumber);
        this.pnlForm.Controls.Add(this.txtRoomNumber);
        this.pnlForm.Controls.Add(this.lblPrice);
        this.pnlForm.Controls.Add(this.numPrice);
        this.pnlForm.Controls.Add(this.lblMaxOccupants);
        this.pnlForm.Controls.Add(this.numMaxOccupants);
        this.pnlForm.Controls.Add(this.lblStatus);
        this.pnlForm.Controls.Add(this.cboStatus);
        this.pnlForm.Controls.Add(this.lblDescription);
        this.pnlForm.Controls.Add(this.txtDescription);
        this.pnlForm.Controls.Add(this.lblError);
        this.pnlForm.Controls.Add(this.btnCancel);
        this.pnlForm.Controls.Add(this.btnSave);
        this.pnlForm.Dock = System.Windows.Forms.DockStyle.Right;
        this.pnlForm.Location = new System.Drawing.Point(646, 58);
        this.pnlForm.Name = "pnlForm";
        this.pnlForm.Padding = new System.Windows.Forms.Padding(14);
        this.pnlForm.Size = new System.Drawing.Size(340, 528);
        this.pnlForm.TabIndex = 2;
        this.pnlForm.Visible = false;

        //
        // lblFormTitle
        //
        this.lblFormTitle.AutoSize = true;
        this.lblFormTitle.BackColor = System.Drawing.Color.Transparent;
        this.lblFormTitle.Font = ScreenTheme.SectionTitle;
        this.lblFormTitle.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
        this.lblFormTitle.Location = new System.Drawing.Point(14, 14);
        this.lblFormTitle.Name = "lblFormTitle";
        this.lblFormTitle.Size = new System.Drawing.Size(107, 17);
        this.lblFormTitle.TabIndex = 0;
        this.lblFormTitle.Text = "THÔNG TIN PHÒNG";

        //
        // lblRoomNumber
        //
        this.lblRoomNumber.AutoSize = true;
        this.lblRoomNumber.BackColor = System.Drawing.Color.Transparent;
        this.lblRoomNumber.Font = ScreenTheme.Body;
        this.lblRoomNumber.ForeColor = ScreenTheme.From(ScreenTheme.Muted);
        this.lblRoomNumber.Location = new System.Drawing.Point(14, 52);
        this.lblRoomNumber.Name = "lblRoomNumber";
        this.lblRoomNumber.Size = new System.Drawing.Size(59, 15);
        this.lblRoomNumber.TabIndex = 1;
        this.lblRoomNumber.Text = "Số phòng";

        //
        // txtRoomNumber
        //
        this.txtRoomNumber.AccessibleName = "Số phòng";
        this.txtRoomNumber.BackColor = ScreenTheme.From(ScreenTheme.Field);
        this.txtRoomNumber.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.txtRoomNumber.Font = ScreenTheme.Body;
        this.txtRoomNumber.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
        this.txtRoomNumber.Location = new System.Drawing.Point(14, 72);
        this.txtRoomNumber.MaxLength = 20;
        this.txtRoomNumber.Name = "txtRoomNumber";
        this.txtRoomNumber.Size = new System.Drawing.Size(312, 23);
        this.txtRoomNumber.TabIndex = 2;

        //
        // lblPrice
        //
        this.lblPrice.AutoSize = true;
        this.lblPrice.BackColor = System.Drawing.Color.Transparent;
        this.lblPrice.Font = ScreenTheme.Body;
        this.lblPrice.ForeColor = ScreenTheme.From(ScreenTheme.Muted);
        this.lblPrice.Location = new System.Drawing.Point(14, 112);
        this.lblPrice.Name = "lblPrice";
        this.lblPrice.Size = new System.Drawing.Size(96, 15);
        this.lblPrice.TabIndex = 3;
        this.lblPrice.Text = "Giá thuê (VNĐ)";

        //
        // numPrice
        //
        this.numPrice.AccessibleName = "Giá thuê phòng";
        this.numPrice.BackColor = ScreenTheme.From(ScreenTheme.Field);
        this.numPrice.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.numPrice.Font = ScreenTheme.Mono;
        this.numPrice.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
        this.numPrice.Increment = new decimal(new int[] { 100000, 0, 0, 0 });
        this.numPrice.Location = new System.Drawing.Point(14, 132);
        this.numPrice.Maximum = new decimal(new int[] { 1000000000, 0, 0, 0 });
        this.numPrice.Name = "numPrice";
        this.numPrice.Size = new System.Drawing.Size(312, 22);
        this.numPrice.TabIndex = 4;
        this.numPrice.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.numPrice.ThousandsSeparator = true;

        //
        // lblMaxOccupants
        //
        this.lblMaxOccupants.AutoSize = true;
        this.lblMaxOccupants.BackColor = System.Drawing.Color.Transparent;
        this.lblMaxOccupants.Font = ScreenTheme.Body;
        this.lblMaxOccupants.ForeColor = ScreenTheme.From(ScreenTheme.Muted);
        this.lblMaxOccupants.Location = new System.Drawing.Point(14, 172);
        this.lblMaxOccupants.Name = "lblMaxOccupants";
        this.lblMaxOccupants.Size = new System.Drawing.Size(62, 15);
        this.lblMaxOccupants.TabIndex = 5;
        this.lblMaxOccupants.Text = "Sức chứa";

        //
        // numMaxOccupants
        //
        this.numMaxOccupants.AccessibleName = "Sức chứa tối đa";
        this.numMaxOccupants.BackColor = ScreenTheme.From(ScreenTheme.Field);
        this.numMaxOccupants.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.numMaxOccupants.Font = ScreenTheme.Mono;
        this.numMaxOccupants.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
        this.numMaxOccupants.Location = new System.Drawing.Point(14, 192);
        this.numMaxOccupants.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
        this.numMaxOccupants.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        this.numMaxOccupants.Name = "numMaxOccupants";
        this.numMaxOccupants.Size = new System.Drawing.Size(312, 22);
        this.numMaxOccupants.TabIndex = 6;
        this.numMaxOccupants.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.numMaxOccupants.Value = new decimal(new int[] { 1, 0, 0, 0 });

        //
        // lblStatus
        //
        this.lblStatus.AutoSize = true;
        this.lblStatus.BackColor = System.Drawing.Color.Transparent;
        this.lblStatus.Font = ScreenTheme.Body;
        this.lblStatus.ForeColor = ScreenTheme.From(ScreenTheme.Muted);
        this.lblStatus.Location = new System.Drawing.Point(14, 232);
        this.lblStatus.Name = "lblStatus";
        this.lblStatus.Size = new System.Drawing.Size(60, 15);
        this.lblStatus.TabIndex = 7;
        this.lblStatus.Text = "Trạng thái";

        //
        // cboStatus
        //
        this.cboStatus.AccessibleName = "Trạng thái phòng";
        this.cboStatus.BackColor = ScreenTheme.From(ScreenTheme.Field);
        this.cboStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboStatus.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.cboStatus.Font = ScreenTheme.Body;
        this.cboStatus.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
        this.cboStatus.Location = new System.Drawing.Point(14, 252);
        this.cboStatus.Name = "cboStatus";
        this.cboStatus.Size = new System.Drawing.Size(312, 23);
        this.cboStatus.TabIndex = 8;

        //
        // lblDescription
        //
        this.lblDescription.AutoSize = true;
        this.lblDescription.BackColor = System.Drawing.Color.Transparent;
        this.lblDescription.Font = ScreenTheme.Body;
        this.lblDescription.ForeColor = ScreenTheme.From(ScreenTheme.Muted);
        this.lblDescription.Location = new System.Drawing.Point(14, 292);
        this.lblDescription.Name = "lblDescription";
        this.lblDescription.Size = new System.Drawing.Size(48, 15);
        this.lblDescription.TabIndex = 9;
        this.lblDescription.Text = "Mô tả";

        //
        // txtDescription
        //
        this.txtDescription.AccessibleName = "Mô tả phòng";
        this.txtDescription.BackColor = ScreenTheme.From(ScreenTheme.Field);
        this.txtDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.txtDescription.Font = ScreenTheme.Body;
        this.txtDescription.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
        this.txtDescription.Location = new System.Drawing.Point(14, 312);
        this.txtDescription.MaxLength = 255;
        this.txtDescription.Multiline = true;
        this.txtDescription.Name = "txtDescription";
        this.txtDescription.Size = new System.Drawing.Size(312, 62);
        this.txtDescription.TabIndex = 10;

        //
        // lblError
        //
        this.lblError.BackColor = System.Drawing.Color.Transparent;
        this.lblError.Font = new System.Drawing.Font("Segoe UI", 8.5F);
        this.lblError.ForeColor = ScreenTheme.From(ScreenTheme.Error);
        this.lblError.Location = new System.Drawing.Point(14, 384);
        this.lblError.Name = "lblError";
        this.lblError.Size = new System.Drawing.Size(312, 40);
        this.lblError.TabIndex = 11;

        //
        // btnSave
        //
        this.btnSave.AccessibleName = "Lưu phòng";
        this.btnSave.BackColor = ScreenTheme.From(ScreenTheme.Terracotta);
        this.btnSave.FlatAppearance.BorderColor = ScreenTheme.From(ScreenTheme.Terracotta);
        this.btnSave.FlatAppearance.MouseOverBackColor = ScreenTheme.From(ScreenTheme.TerracottaHover);
        this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnSave.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.btnSave.ForeColor = ScreenTheme.From(ScreenTheme.OnFill);
        this.btnSave.Location = new System.Drawing.Point(14, 432);
        this.btnSave.Name = "btnSave";
        this.btnSave.Size = new System.Drawing.Size(150, 34);
        this.btnSave.TabIndex = 12;
        this.btnSave.Text = "Lưu";
        this.btnSave.UseVisualStyleBackColor = false;

        //
        // btnCancel
        //
        this.btnCancel.AccessibleName = "Hủy nhập phòng";
        this.btnCancel.BackColor = ScreenTheme.From(ScreenTheme.MenuBtnBg);
        this.btnCancel.FlatAppearance.BorderColor = ScreenTheme.From(ScreenTheme.OutlineBtn);
        this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnCancel.Font = ScreenTheme.Body;
        this.btnCancel.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
        this.btnCancel.Location = new System.Drawing.Point(176, 432);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(150, 34);
        this.btnCancel.TabIndex = 13;
        this.btnCancel.Text = "Hủy";
        this.btnCancel.UseVisualStyleBackColor = false;

        //
        // RoomsForm
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.AccessibleName = "Trang quản lý phòng";
        this.BackColor = ScreenTheme.From(ScreenTheme.Base);
        this.Controls.Add(this.pnlGridHost);
        this.Controls.Add(this.pnlForm);
        this.Controls.Add(this.pnlToolbar);
        this.Font = ScreenTheme.Body;
        this.Name = "RoomsForm";
        this.Padding = new System.Windows.Forms.Padding(14);
        this.Size = new System.Drawing.Size(1000, 600);

        this.pnlToolbar.ResumeLayout(false);
        this.pnlToolbar.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.numPrice)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numMaxOccupants)).EndInit();
        this.pnlGridHost.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.dgvRooms)).EndInit();
        this.pnlForm.ResumeLayout(false);
        this.pnlForm.PerformLayout();
        this.ResumeLayout(false);
    }

    private ToolbarPanel pnlToolbar;
    private System.Windows.Forms.TextBox txtSearch;
    private System.Windows.Forms.ComboBox cboStatusFilter;
    private System.Windows.Forms.Button btnRefresh;
    private System.Windows.Forms.Panel pnlDivider;
    private System.Windows.Forms.Button btnDelete;
    private System.Windows.Forms.Button btnEdit;
    private System.Windows.Forms.Button btnAdd;
    private System.Windows.Forms.Panel pnlGridHost;
    private TblFootPanel tblFoot;
    private System.Windows.Forms.DataGridView dgvRooms;
    private System.Windows.Forms.DataGridViewTextBoxColumn colRoomNumber;
    private System.Windows.Forms.DataGridViewTextBoxColumn colPrice;
    private System.Windows.Forms.DataGridViewTextBoxColumn colMaxOccupants;
    private System.Windows.Forms.DataGridViewTextBoxColumn colCurrentOccupants;
    private System.Windows.Forms.DataGridViewTextBoxColumn colStatus;
    private System.Windows.Forms.DataGridViewTextBoxColumn colDescription;
    private System.Windows.Forms.DataGridViewTextBoxColumn colActions;
    private System.Windows.Forms.Panel pnlNote;
    private System.Windows.Forms.Panel pnlForm;
    private System.Windows.Forms.Label lblFormTitle;
    private System.Windows.Forms.Label lblRoomNumber;
    private System.Windows.Forms.TextBox txtRoomNumber;
    private System.Windows.Forms.Label lblPrice;
    private System.Windows.Forms.NumericUpDown numPrice;
    private System.Windows.Forms.Label lblMaxOccupants;
    private System.Windows.Forms.NumericUpDown numMaxOccupants;
    private System.Windows.Forms.Label lblStatus;
    private System.Windows.Forms.ComboBox cboStatus;
    private System.Windows.Forms.Label lblDescription;
    private System.Windows.Forms.TextBox txtDescription;
    private System.Windows.Forms.Label lblError;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnSave;
}
