#nullable enable

namespace QuanLyTro.Forms;

partial class TenantsForm
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
        this.lblRoom = new System.Windows.Forms.Label();
        this.cboRoom = new System.Windows.Forms.ComboBox();
        this.btnAdd = new System.Windows.Forms.Button();
        this.lblHint = new System.Windows.Forms.Label();
        this.pnlMain = new System.Windows.Forms.Panel();
        this.dgvTenants = new System.Windows.Forms.DataGridView();
        this.colFullName = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colIdCard = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colDateOfBirth = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colPhone = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colHometown = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colTemporary = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.tblFoot = new QuanLyTro.Forms.TblFootPanel();
        this.pnlRowActions = new System.Windows.Forms.Panel();
        this.btnDelete = new System.Windows.Forms.Button();
        this.btnCheckout = new System.Windows.Forms.Button();
        this.btnMove = new System.Windows.Forms.Button();
        this.btnEdit = new System.Windows.Forms.Button();
        this.pnlNote = new System.Windows.Forms.Panel();
        this.pnlForm = new System.Windows.Forms.Panel();
        this.lblFormTitle = new System.Windows.Forms.Label();
        this.lblFullName = new System.Windows.Forms.Label();
        this.txtFullName = new System.Windows.Forms.TextBox();
        this.lblIdCard = new System.Windows.Forms.Label();
        this.txtIdCard = new System.Windows.Forms.TextBox();
        this.lblDateOfBirth = new System.Windows.Forms.Label();
        this.dtpDateOfBirth = new System.Windows.Forms.DateTimePicker();
        this.lblPhone = new System.Windows.Forms.Label();
        this.txtPhone = new System.Windows.Forms.TextBox();
        this.lblHometown = new System.Windows.Forms.Label();
        this.txtHometown = new System.Windows.Forms.TextBox();
        this.lblWorkplace = new System.Windows.Forms.Label();
        this.txtWorkplace = new System.Windows.Forms.TextBox();
        this.chkTemporary = new System.Windows.Forms.CheckBox();
        this.lblPassword = new System.Windows.Forms.Label();
        this.txtPassword = new System.Windows.Forms.TextBox();
        this.lblPasswordHint = new System.Windows.Forms.Label();
        this.lblError = new System.Windows.Forms.Label();
        this.btnCancel = new System.Windows.Forms.Button();
        this.btnSave = new System.Windows.Forms.Button();
        this.pnlToolbar.SuspendLayout();
        this.pnlMain.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvTenants)).BeginInit();
        this.pnlRowActions.SuspendLayout();
        this.pnlForm.SuspendLayout();
        this.SuspendLayout();

        //
        // pnlToolbar — .toolbar: chọn phòng + nút Thêm + ghi chú mật khẩu mặc định
        //
        this.pnlToolbar.AccessibleName = "Thanh công cụ quản lý người thuê";
        this.pnlToolbar.Controls.Add(this.lblRoom);
        this.pnlToolbar.Controls.Add(this.cboRoom);
        this.pnlToolbar.Controls.Add(this.btnAdd);
        this.pnlToolbar.Controls.Add(this.lblHint);
        this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
        this.pnlToolbar.Location = new System.Drawing.Point(14, 14);
        this.pnlToolbar.Name = "pnlToolbar";
        this.pnlToolbar.Size = new System.Drawing.Size(972, 44);
        this.pnlToolbar.TabIndex = 0;

        //
        // lblRoom
        //
        this.lblRoom.AutoSize = true;
        this.lblRoom.BackColor = System.Drawing.Color.Transparent;
        this.lblRoom.Font = ScreenTheme.Body;
        this.lblRoom.ForeColor = ScreenTheme.From(ScreenTheme.Muted);
        this.lblRoom.Location = new System.Drawing.Point(14, 16);
        this.lblRoom.Name = "lblRoom";
        this.lblRoom.Size = new System.Drawing.Size(41, 15);
        this.lblRoom.TabIndex = 0;
        this.lblRoom.Text = "Phòng";

        //
        // cboRoom
        //
        this.cboRoom.AccessibleName = "Chọn phòng để xem người thuê";
        this.cboRoom.BackColor = ScreenTheme.From(ScreenTheme.Field);
        this.cboRoom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboRoom.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.cboRoom.Font = ScreenTheme.Body;
        this.cboRoom.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
        this.cboRoom.Location = new System.Drawing.Point(63, 12);
        this.cboRoom.Name = "cboRoom";
        this.cboRoom.Size = new System.Drawing.Size(150, 23);
        this.cboRoom.TabIndex = 1;

        //
        // btnAdd
        //
        this.btnAdd.AccessibleName = "Thêm người thuê vào phòng";
        this.btnAdd.BackColor = ScreenTheme.From(ScreenTheme.Terracotta);
        this.btnAdd.FlatAppearance.BorderColor = ScreenTheme.From(ScreenTheme.Terracotta);
        this.btnAdd.FlatAppearance.MouseOverBackColor = ScreenTheme.From(ScreenTheme.TerracottaHover);
        this.btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnAdd.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.btnAdd.ForeColor = ScreenTheme.From(ScreenTheme.OnFill);
        this.btnAdd.Location = new System.Drawing.Point(225, 8);
        this.btnAdd.Name = "btnAdd";
        this.btnAdd.Size = new System.Drawing.Size(88, 30);
        this.btnAdd.TabIndex = 2;
        this.btnAdd.Text = "+ Thêm";
        this.btnAdd.UseVisualStyleBackColor = false;

        //
        // lblHint — .toolbar ghi chú mật khẩu mặc định (canh phải)
        //
        this.lblHint.AccessibleName = "Mật khẩu mặc định là 6 số cuối CCCD";
        this.lblHint.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
        this.lblHint.AutoSize = true;
        this.lblHint.BackColor = System.Drawing.Color.Transparent;
        this.lblHint.Font = new System.Drawing.Font("Segoe UI", 8.25F);
        this.lblHint.ForeColor = ScreenTheme.From(ScreenTheme.Dim);
        this.lblHint.Location = new System.Drawing.Point(722, 17);
        this.lblHint.Name = "lblHint";
        this.lblHint.Size = new System.Drawing.Size(232, 13);
        this.lblHint.TabIndex = 3;
        this.lblHint.Text = "Mật khẩu mặc định = 6 số cuối CCCD";

        //
        // pnlMain — vùng bảng + chân bảng + hàng nút + dải ghi chú
        //
        this.pnlMain.BackColor = ScreenTheme.From(ScreenTheme.Base);
        this.pnlMain.Controls.Add(this.dgvTenants);
        this.pnlMain.Controls.Add(this.tblFoot);
        this.pnlMain.Controls.Add(this.pnlRowActions);
        this.pnlMain.Controls.Add(this.pnlNote);
        this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
        this.pnlMain.Location = new System.Drawing.Point(14, 58);
        this.pnlMain.Name = "pnlMain";
        this.pnlMain.Padding = new System.Windows.Forms.Padding(0, 12, 0, 0);
        this.pnlMain.Size = new System.Drawing.Size(632, 528);
        this.pnlMain.TabIndex = 1;

        //
        // dgvTenants
        //
        this.dgvTenants.AccessibleName = "Bảng danh sách người thuê";
        this.dgvTenants.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        this.dgvTenants.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colFullName,
            this.colIdCard,
            this.colDateOfBirth,
            this.colPhone,
            this.colHometown,
            this.colTemporary,
            this.colKey});
        this.dgvTenants.Dock = System.Windows.Forms.DockStyle.Fill;
        this.dgvTenants.Location = new System.Drawing.Point(0, 12);
        this.dgvTenants.Name = "dgvTenants";
        this.dgvTenants.Size = new System.Drawing.Size(632, 402);
        this.dgvTenants.TabIndex = 0;

        //
        // colFullName
        //
        this.colFullName.HeaderText = "HỌ TÊN";
        this.colFullName.Name = "colFullName";
        this.colFullName.ReadOnly = true;
        this.colFullName.Width = 170;

        //
        // colIdCard
        //
        this.colIdCard.DefaultCellStyle = new System.Windows.Forms.DataGridViewCellStyle
        {
            Font = ScreenTheme.Mono,
        };
        this.colIdCard.HeaderText = "CCCD";
        this.colIdCard.Name = "colIdCard";
        this.colIdCard.ReadOnly = true;
        this.colIdCard.Width = 130;

        //
        // colDateOfBirth
        //
        this.colDateOfBirth.DefaultCellStyle = new System.Windows.Forms.DataGridViewCellStyle
        {
            Font = ScreenTheme.Mono,
        };
        this.colDateOfBirth.HeaderText = "NGÀY SINH";
        this.colDateOfBirth.Name = "colDateOfBirth";
        this.colDateOfBirth.ReadOnly = true;
        this.colDateOfBirth.Width = 105;

        //
        // colPhone
        //
        this.colPhone.DefaultCellStyle = new System.Windows.Forms.DataGridViewCellStyle
        {
            Font = ScreenTheme.Mono,
        };
        this.colPhone.HeaderText = "SĐT";
        this.colPhone.Name = "colPhone";
        this.colPhone.ReadOnly = true;
        this.colPhone.Width = 125;

        //
        // colHometown
        //
        this.colHometown.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
        this.colHometown.HeaderText = "QUÊ QUÁN";
        this.colHometown.Name = "colHometown";
        this.colHometown.ReadOnly = true;

        //
        // colTemporary
        //
        this.colTemporary.HeaderText = "TẠM TRÚ";
        this.colTemporary.Name = "colTemporary";
        this.colTemporary.ReadOnly = true;
        this.colTemporary.Width = 110;

        //
        // colKey — nút đặt lại mật khẩu mặc định (template fa-key)
        //
        this.colKey.HeaderText = "";
        this.colKey.Name = "colKey";
        this.colKey.ReadOnly = true;
        this.colKey.Resizable = System.Windows.Forms.DataGridViewTriState.False;
        this.colKey.Width = 110;

        //
        // tblFoot — .tblfoot
        //
        this.tblFoot.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.tblFoot.Location = new System.Drawing.Point(0, 414);
        this.tblFoot.Name = "tblFoot";
        this.tblFoot.Size = new System.Drawing.Size(632, 28);
        this.tblFoot.TabIndex = 1;

        //
        // pnlRowActions — .rowbn dưới bảng
        //
        this.pnlRowActions.AccessibleName = "Hàng thao tác hồ sơ người thuê";
        this.pnlRowActions.BackColor = System.Drawing.Color.Transparent;
        this.pnlRowActions.Controls.Add(this.btnDelete);
        this.pnlRowActions.Controls.Add(this.btnCheckout);
        this.pnlRowActions.Controls.Add(this.btnMove);
        this.pnlRowActions.Controls.Add(this.btnEdit);
        this.pnlRowActions.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.pnlRowActions.Location = new System.Drawing.Point(0, 442);
        this.pnlRowActions.Name = "pnlRowActions";
        this.pnlRowActions.Padding = new System.Windows.Forms.Padding(0, 12, 0, 0);
        this.pnlRowActions.Size = new System.Drawing.Size(632, 52);
        this.pnlRowActions.TabIndex = 2;

        //
        // btnEdit
        //
        this.btnEdit.AccessibleName = "Sửa hồ sơ người thuê đã chọn";
        this.btnEdit.BackColor = ScreenTheme.From(ScreenTheme.MenuBtnBg);
        this.btnEdit.FlatAppearance.BorderColor = ScreenTheme.From(ScreenTheme.OutlineBtn);
        this.btnEdit.FlatAppearance.MouseOverBackColor = ScreenTheme.From(ScreenTheme.HoverBg);
        this.btnEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnEdit.Font = ScreenTheme.Body;
        this.btnEdit.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
        this.btnEdit.Location = new System.Drawing.Point(0, 12);
        this.btnEdit.Name = "btnEdit";
        this.btnEdit.Size = new System.Drawing.Size(78, 30);
        this.btnEdit.TabIndex = 0;
        this.btnEdit.Text = "Sửa";
        this.btnEdit.UseVisualStyleBackColor = false;

        //
        // btnMove
        //
        this.btnMove.AccessibleName = "Chuyển người thuê sang phòng khác";
        this.btnMove.BackColor = ScreenTheme.From(ScreenTheme.MenuBtnBg);
        this.btnMove.FlatAppearance.BorderColor = ScreenTheme.From(ScreenTheme.OutlineBtn);
        this.btnMove.FlatAppearance.MouseOverBackColor = ScreenTheme.From(ScreenTheme.HoverBg);
        this.btnMove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnMove.Font = ScreenTheme.Body;
        this.btnMove.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
        this.btnMove.Location = new System.Drawing.Point(84, 12);
        this.btnMove.Name = "btnMove";
        this.btnMove.Size = new System.Drawing.Size(118, 30);
        this.btnMove.TabIndex = 1;
        this.btnMove.Text = "Chuyển phòng";
        this.btnMove.UseVisualStyleBackColor = false;

        //
        // btnCheckout
        //
        this.btnCheckout.AccessibleName = "Cho người thuê trả phòng";
        this.btnCheckout.BackColor = ScreenTheme.From(ScreenTheme.MenuBtnBg);
        this.btnCheckout.FlatAppearance.BorderColor = ScreenTheme.From(ScreenTheme.OutlineBtn);
        this.btnCheckout.FlatAppearance.MouseOverBackColor = ScreenTheme.From(ScreenTheme.HoverBg);
        this.btnCheckout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnCheckout.Font = ScreenTheme.Body;
        this.btnCheckout.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
        this.btnCheckout.Location = new System.Drawing.Point(208, 12);
        this.btnCheckout.Name = "btnCheckout";
        this.btnCheckout.Size = new System.Drawing.Size(100, 30);
        this.btnCheckout.TabIndex = 2;
        this.btnCheckout.Text = "Trả phòng";
        this.btnCheckout.UseVisualStyleBackColor = false;

        //
        // btnDelete
        //
        this.btnDelete.AccessibleName = "Xóa hồ sơ người thuê đã trả phòng";
        this.btnDelete.BackColor = ScreenTheme.From(ScreenTheme.HoverBg);
        this.btnDelete.FlatAppearance.BorderColor = ScreenTheme.From(ScreenTheme.DangerBtn);
        this.btnDelete.FlatAppearance.MouseOverBackColor = ScreenTheme.From(ScreenTheme.ErrorBgSoft);
        this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnDelete.Font = ScreenTheme.Body;
        this.btnDelete.ForeColor = ScreenTheme.From(ScreenTheme.Error);
        this.btnDelete.Location = new System.Drawing.Point(314, 12);
        this.btnDelete.Name = "btnDelete";
        this.btnDelete.Size = new System.Drawing.Size(112, 30);
        this.btnDelete.TabIndex = 3;
        this.btnDelete.Text = "Xóa hồ sơ";
        this.btnDelete.UseVisualStyleBackColor = false;

        //
        // pnlNote — .note (nội dung do NoteBar.Create gắn trong TenantsForm.cs)
        //
        this.pnlNote.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.pnlNote.Location = new System.Drawing.Point(0, 494);
        this.pnlNote.Name = "pnlNote";
        this.pnlNote.Size = new System.Drawing.Size(632, 34);
        this.pnlNote.TabIndex = 3;

        //
        // pnlForm
        //
        this.pnlForm.BackColor = ScreenTheme.From(ScreenTheme.Card);
        this.pnlForm.Controls.Add(this.lblFormTitle);
        this.pnlForm.Controls.Add(this.lblFullName);
        this.pnlForm.Controls.Add(this.txtFullName);
        this.pnlForm.Controls.Add(this.lblIdCard);
        this.pnlForm.Controls.Add(this.txtIdCard);
        this.pnlForm.Controls.Add(this.lblDateOfBirth);
        this.pnlForm.Controls.Add(this.dtpDateOfBirth);
        this.pnlForm.Controls.Add(this.lblPhone);
        this.pnlForm.Controls.Add(this.txtPhone);
        this.pnlForm.Controls.Add(this.lblHometown);
        this.pnlForm.Controls.Add(this.txtHometown);
        this.pnlForm.Controls.Add(this.lblWorkplace);
        this.pnlForm.Controls.Add(this.txtWorkplace);
        this.pnlForm.Controls.Add(this.chkTemporary);
        this.pnlForm.Controls.Add(this.lblPassword);
        this.pnlForm.Controls.Add(this.txtPassword);
        this.pnlForm.Controls.Add(this.lblPasswordHint);
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
        this.lblFormTitle.Size = new System.Drawing.Size(145, 17);
        this.lblFormTitle.TabIndex = 0;
        this.lblFormTitle.Text = "THÊM NGƯỜI THUÊ";

        //
        // lblFullName
        //
        this.lblFullName.AutoSize = true;
        this.lblFullName.BackColor = System.Drawing.Color.Transparent;
        this.lblFullName.Font = ScreenTheme.Body;
        this.lblFullName.ForeColor = ScreenTheme.From(ScreenTheme.Muted);
        this.lblFullName.Location = new System.Drawing.Point(14, 48);
        this.lblFullName.Name = "lblFullName";
        this.lblFullName.Size = new System.Drawing.Size(45, 15);
        this.lblFullName.TabIndex = 1;
        this.lblFullName.Text = "Họ tên";

        //
        // txtFullName
        //
        this.txtFullName.AccessibleName = "Họ tên người thuê";
        this.txtFullName.BackColor = ScreenTheme.From(ScreenTheme.Field);
        this.txtFullName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.txtFullName.Font = ScreenTheme.Body;
        this.txtFullName.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
        this.txtFullName.Location = new System.Drawing.Point(14, 68);
        this.txtFullName.MaxLength = 100;
        this.txtFullName.Name = "txtFullName";
        this.txtFullName.Size = new System.Drawing.Size(150, 23);
        this.txtFullName.TabIndex = 2;

        //
        // lblIdCard
        //
        this.lblIdCard.AutoSize = true;
        this.lblIdCard.BackColor = System.Drawing.Color.Transparent;
        this.lblIdCard.Font = ScreenTheme.Body;
        this.lblIdCard.ForeColor = ScreenTheme.From(ScreenTheme.Muted);
        this.lblIdCard.Location = new System.Drawing.Point(176, 48);
        this.lblIdCard.Name = "lblIdCard";
        this.lblIdCard.Size = new System.Drawing.Size(35, 15);
        this.lblIdCard.TabIndex = 3;
        this.lblIdCard.Text = "CCCD";

        //
        // txtIdCard
        //
        this.txtIdCard.AccessibleName = "Số CCCD người thuê, 12 chữ số";
        this.txtIdCard.BackColor = ScreenTheme.From(ScreenTheme.Field);
        this.txtIdCard.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.txtIdCard.Font = ScreenTheme.Mono;
        this.txtIdCard.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
        this.txtIdCard.Location = new System.Drawing.Point(176, 68);
        this.txtIdCard.MaxLength = 12;
        this.txtIdCard.Name = "txtIdCard";
        this.txtIdCard.Size = new System.Drawing.Size(150, 22);
        this.txtIdCard.TabIndex = 4;

        //
        // lblDateOfBirth
        //
        this.lblDateOfBirth.AutoSize = true;
        this.lblDateOfBirth.BackColor = System.Drawing.Color.Transparent;
        this.lblDateOfBirth.Font = ScreenTheme.Body;
        this.lblDateOfBirth.ForeColor = ScreenTheme.From(ScreenTheme.Muted);
        this.lblDateOfBirth.Location = new System.Drawing.Point(14, 104);
        this.lblDateOfBirth.Name = "lblDateOfBirth";
        this.lblDateOfBirth.Size = new System.Drawing.Size(65, 15);
        this.lblDateOfBirth.TabIndex = 5;
        this.lblDateOfBirth.Text = "Ngày sinh";

        //
        // dtpDateOfBirth
        //
        this.dtpDateOfBirth.AccessibleName = "Ngày sinh người thuê";
        this.dtpDateOfBirth.CalendarForeColor = ScreenTheme.From(ScreenTheme.Cream);
        this.dtpDateOfBirth.CalendarMonthBackground = ScreenTheme.From(ScreenTheme.Card);
        this.dtpDateOfBirth.CustomFormat = "dd/MM/yyyy";
        this.dtpDateOfBirth.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
        this.dtpDateOfBirth.Location = new System.Drawing.Point(14, 124);
        this.dtpDateOfBirth.Name = "dtpDateOfBirth";
        this.dtpDateOfBirth.Size = new System.Drawing.Size(150, 23);
        this.dtpDateOfBirth.TabIndex = 6;
        this.dtpDateOfBirth.Value = new System.DateTime(2000, 1, 1, 0, 0, 0, 0);

        //
        // lblPhone
        //
        this.lblPhone.AutoSize = true;
        this.lblPhone.BackColor = System.Drawing.Color.Transparent;
        this.lblPhone.Font = ScreenTheme.Body;
        this.lblPhone.ForeColor = ScreenTheme.From(ScreenTheme.Muted);
        this.lblPhone.Location = new System.Drawing.Point(176, 104);
        this.lblPhone.Name = "lblPhone";
        this.lblPhone.Size = new System.Drawing.Size(77, 15);
        this.lblPhone.TabIndex = 7;
        this.lblPhone.Text = "Số điện thoại";

        //
        // txtPhone
        //
        this.txtPhone.AccessibleName = "Số điện thoại người thuê";
        this.txtPhone.BackColor = ScreenTheme.From(ScreenTheme.Field);
        this.txtPhone.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.txtPhone.Font = ScreenTheme.Mono;
        this.txtPhone.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
        this.txtPhone.Location = new System.Drawing.Point(176, 124);
        this.txtPhone.MaxLength = 20;
        this.txtPhone.Name = "txtPhone";
        this.txtPhone.Size = new System.Drawing.Size(150, 22);
        this.txtPhone.TabIndex = 8;

        //
        // lblHometown
        //
        this.lblHometown.AutoSize = true;
        this.lblHometown.BackColor = System.Drawing.Color.Transparent;
        this.lblHometown.Font = ScreenTheme.Body;
        this.lblHometown.ForeColor = ScreenTheme.From(ScreenTheme.Muted);
        this.lblHometown.Location = new System.Drawing.Point(14, 160);
        this.lblHometown.Name = "lblHometown";
        this.lblHometown.Size = new System.Drawing.Size(55, 15);
        this.lblHometown.TabIndex = 9;
        this.lblHometown.Text = "Quê quán";

        //
        // txtHometown
        //
        this.txtHometown.AccessibleName = "Quê quán người thuê";
        this.txtHometown.BackColor = ScreenTheme.From(ScreenTheme.Field);
        this.txtHometown.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.txtHometown.Font = ScreenTheme.Body;
        this.txtHometown.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
        this.txtHometown.Location = new System.Drawing.Point(14, 180);
        this.txtHometown.MaxLength = 150;
        this.txtHometown.Name = "txtHometown";
        this.txtHometown.Size = new System.Drawing.Size(150, 23);
        this.txtHometown.TabIndex = 10;

        //
        // lblWorkplace
        //
        this.lblWorkplace.AutoSize = true;
        this.lblWorkplace.BackColor = System.Drawing.Color.Transparent;
        this.lblWorkplace.Font = ScreenTheme.Body;
        this.lblWorkplace.ForeColor = ScreenTheme.From(ScreenTheme.Muted);
        this.lblWorkplace.Location = new System.Drawing.Point(176, 160);
        this.lblWorkplace.Name = "lblWorkplace";
        this.lblWorkplace.Size = new System.Drawing.Size(78, 15);
        this.lblWorkplace.TabIndex = 11;
        this.lblWorkplace.Text = "Nơi làm việc";

        //
        // txtWorkplace
        //
        this.txtWorkplace.AccessibleName = "Nơi làm việc người thuê";
        this.txtWorkplace.BackColor = ScreenTheme.From(ScreenTheme.Field);
        this.txtWorkplace.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.txtWorkplace.Font = ScreenTheme.Body;
        this.txtWorkplace.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
        this.txtWorkplace.Location = new System.Drawing.Point(176, 180);
        this.txtWorkplace.MaxLength = 150;
        this.txtWorkplace.Name = "txtWorkplace";
        this.txtWorkplace.Size = new System.Drawing.Size(150, 23);
        this.txtWorkplace.TabIndex = 12;

        //
        // chkTemporary
        //
        this.chkTemporary.AccessibleName = "Đã đăng ký tạm trú";
        this.chkTemporary.AutoSize = true;
        this.chkTemporary.Font = ScreenTheme.Body;
        this.chkTemporary.ForeColor = ScreenTheme.From(ScreenTheme.Neutral);
        this.chkTemporary.Location = new System.Drawing.Point(14, 220);
        this.chkTemporary.Name = "chkTemporary";
        this.chkTemporary.Size = new System.Drawing.Size(131, 19);
        this.chkTemporary.TabIndex = 13;
        this.chkTemporary.Text = "Đã đăng ký tạm trú";
        this.chkTemporary.UseVisualStyleBackColor = true;

        //
        // lblPassword
        //
        this.lblPassword.AutoSize = true;
        this.lblPassword.BackColor = System.Drawing.Color.Transparent;
        this.lblPassword.Font = ScreenTheme.Body;
        this.lblPassword.ForeColor = ScreenTheme.From(ScreenTheme.Muted);
        this.lblPassword.Location = new System.Drawing.Point(14, 254);
        this.lblPassword.Name = "lblPassword";
        this.lblPassword.Size = new System.Drawing.Size(61, 15);
        this.lblPassword.TabIndex = 14;
        this.lblPassword.Text = "Mật khẩu";

        //
        // txtPassword
        //
        this.txtPassword.AccessibleName = "Mật khẩu người thuê, không bắt buộc";
        this.txtPassword.BackColor = ScreenTheme.From(ScreenTheme.Field);
        this.txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.txtPassword.Font = ScreenTheme.Body;
        this.txtPassword.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
        this.txtPassword.Location = new System.Drawing.Point(14, 274);
        this.txtPassword.MaxLength = 100;
        this.txtPassword.Name = "txtPassword";
        this.txtPassword.PasswordChar = '●';
        this.txtPassword.Size = new System.Drawing.Size(312, 23);
        this.txtPassword.TabIndex = 15;

        //
        // lblPasswordHint
        //
        this.lblPasswordHint.AutoSize = true;
        this.lblPasswordHint.BackColor = System.Drawing.Color.Transparent;
        this.lblPasswordHint.Font = new System.Drawing.Font("Segoe UI", 8.25F);
        this.lblPasswordHint.ForeColor = ScreenTheme.From(ScreenTheme.Dim);
        this.lblPasswordHint.Location = new System.Drawing.Point(14, 302);
        this.lblPasswordHint.Name = "lblPasswordHint";
        this.lblPasswordHint.Size = new System.Drawing.Size(254, 13);
        this.lblPasswordHint.TabIndex = 16;
        this.lblPasswordHint.Text = "Để trống = mật khẩu mặc định 6 số cuối CCCD";

        //
        // lblError
        //
        this.lblError.BackColor = System.Drawing.Color.Transparent;
        this.lblError.Font = new System.Drawing.Font("Segoe UI", 8.5F);
        this.lblError.ForeColor = ScreenTheme.From(ScreenTheme.Error);
        this.lblError.Location = new System.Drawing.Point(14, 326);
        this.lblError.Name = "lblError";
        this.lblError.Size = new System.Drawing.Size(312, 38);
        this.lblError.TabIndex = 17;

        //
        // btnSave
        //
        this.btnSave.AccessibleName = "Lưu người thuê";
        this.btnSave.BackColor = ScreenTheme.From(ScreenTheme.Terracotta);
        this.btnSave.FlatAppearance.BorderColor = ScreenTheme.From(ScreenTheme.Terracotta);
        this.btnSave.FlatAppearance.MouseOverBackColor = ScreenTheme.From(ScreenTheme.TerracottaHover);
        this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnSave.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.btnSave.ForeColor = ScreenTheme.From(ScreenTheme.OnFill);
        this.btnSave.Location = new System.Drawing.Point(14, 374);
        this.btnSave.Name = "btnSave";
        this.btnSave.Size = new System.Drawing.Size(150, 34);
        this.btnSave.TabIndex = 18;
        this.btnSave.Text = "Lưu";
        this.btnSave.UseVisualStyleBackColor = false;

        //
        // btnCancel
        //
        this.btnCancel.AccessibleName = "Hủy nhập người thuê";
        this.btnCancel.BackColor = ScreenTheme.From(ScreenTheme.MenuBtnBg);
        this.btnCancel.FlatAppearance.BorderColor = ScreenTheme.From(ScreenTheme.OutlineBtn);
        this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnCancel.Font = ScreenTheme.Body;
        this.btnCancel.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
        this.btnCancel.Location = new System.Drawing.Point(176, 374);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(150, 34);
        this.btnCancel.TabIndex = 19;
        this.btnCancel.Text = "Hủy";
        this.btnCancel.UseVisualStyleBackColor = false;

        //
        // TenantsForm
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.AccessibleName = "Trang quản lý người thuê";
        this.BackColor = ScreenTheme.From(ScreenTheme.Base);
        this.Controls.Add(this.pnlMain);
        this.Controls.Add(this.pnlForm);
        this.Controls.Add(this.pnlToolbar);
        this.Font = ScreenTheme.Body;
        this.Name = "TenantsForm";
        this.Padding = new System.Windows.Forms.Padding(14);
        this.Size = new System.Drawing.Size(1000, 600);

        this.pnlToolbar.ResumeLayout(false);
        this.pnlToolbar.PerformLayout();
        this.pnlMain.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.dgvTenants)).EndInit();
        this.pnlRowActions.ResumeLayout(false);
        this.pnlForm.ResumeLayout(false);
        this.pnlForm.PerformLayout();
        this.ResumeLayout(false);
    }

    private QuanLyTro.Forms.ToolbarPanel pnlToolbar;
    private System.Windows.Forms.Label lblRoom;
    private System.Windows.Forms.ComboBox cboRoom;
    private System.Windows.Forms.Button btnAdd;
    private System.Windows.Forms.Label lblHint;
    private System.Windows.Forms.Panel pnlMain;
    private System.Windows.Forms.DataGridView dgvTenants;
    private System.Windows.Forms.DataGridViewTextBoxColumn colFullName;
    private System.Windows.Forms.DataGridViewTextBoxColumn colIdCard;
    private System.Windows.Forms.DataGridViewTextBoxColumn colDateOfBirth;
    private System.Windows.Forms.DataGridViewTextBoxColumn colPhone;
    private System.Windows.Forms.DataGridViewTextBoxColumn colHometown;
    private System.Windows.Forms.DataGridViewTextBoxColumn colTemporary;
    private System.Windows.Forms.DataGridViewTextBoxColumn colKey;
    private QuanLyTro.Forms.TblFootPanel tblFoot;
    private System.Windows.Forms.Panel pnlRowActions;
    private System.Windows.Forms.Button btnDelete;
    private System.Windows.Forms.Button btnCheckout;
    private System.Windows.Forms.Button btnMove;
    private System.Windows.Forms.Button btnEdit;
    private System.Windows.Forms.Panel pnlNote;
    private System.Windows.Forms.Panel pnlForm;
    private System.Windows.Forms.Label lblFormTitle;
    private System.Windows.Forms.Label lblFullName;
    private System.Windows.Forms.TextBox txtFullName;
    private System.Windows.Forms.Label lblIdCard;
    private System.Windows.Forms.TextBox txtIdCard;
    private System.Windows.Forms.Label lblDateOfBirth;
    private System.Windows.Forms.DateTimePicker dtpDateOfBirth;
    private System.Windows.Forms.Label lblPhone;
    private System.Windows.Forms.TextBox txtPhone;
    private System.Windows.Forms.Label lblHometown;
    private System.Windows.Forms.TextBox txtHometown;
    private System.Windows.Forms.Label lblWorkplace;
    private System.Windows.Forms.TextBox txtWorkplace;
    private System.Windows.Forms.CheckBox chkTemporary;
    private System.Windows.Forms.Label lblPassword;
    private System.Windows.Forms.TextBox txtPassword;
    private System.Windows.Forms.Label lblPasswordHint;
    private System.Windows.Forms.Label lblError;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnSave;
}
