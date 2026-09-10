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
        this.pnlToolbar = new System.Windows.Forms.Panel();
        this.lblRoom = new System.Windows.Forms.Label();
        this.cboRoom = new System.Windows.Forms.ComboBox();
        this.btnAdd = new System.Windows.Forms.Button();
        this.btnRefresh = new System.Windows.Forms.Button();
        this.lblHint = new System.Windows.Forms.Label();
        this.dgvTenants = new System.Windows.Forms.DataGridView();
        this.colFullName = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colIdCard = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colDateOfBirth = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colPhone = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colHometown = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colTemporary = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.pnlActions = new System.Windows.Forms.Panel();
        this.btnDelete = new System.Windows.Forms.Button();
        this.btnCheckout = new System.Windows.Forms.Button();
        this.btnEdit = new System.Windows.Forms.Button();
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
        ((System.ComponentModel.ISupportInitialize)(this.dgvTenants)).BeginInit();
        this.pnlActions.SuspendLayout();
        this.pnlForm.SuspendLayout();
        this.SuspendLayout();

        //
        // pnlToolbar
        //
        this.pnlToolbar.BackColor = System.Drawing.ColorTranslator.FromHtml("#161B1F");
        this.pnlToolbar.Controls.Add(this.lblRoom);
        this.pnlToolbar.Controls.Add(this.cboRoom);
        this.pnlToolbar.Controls.Add(this.btnAdd);
        this.pnlToolbar.Controls.Add(this.btnRefresh);
        this.pnlToolbar.Controls.Add(this.lblHint);
        this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
        this.pnlToolbar.Location = new System.Drawing.Point(14, 14);
        this.pnlToolbar.Name = "pnlToolbar";
        this.pnlToolbar.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
        this.pnlToolbar.Size = new System.Drawing.Size(972, 50);
        this.pnlToolbar.TabIndex = 0;

        //
        // lblRoom
        //
        this.lblRoom.AutoSize = true;
        this.lblRoom.BackColor = System.Drawing.Color.Transparent;
        this.lblRoom.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblRoom.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblRoom.Location = new System.Drawing.Point(12, 17);
        this.lblRoom.Name = "lblRoom";
        this.lblRoom.Size = new System.Drawing.Size(37, 15);
        this.lblRoom.TabIndex = 0;
        this.lblRoom.Text = "Phòng";

        //
        // cboRoom
        //
        this.cboRoom.AccessibleName = "Chọn phòng để xem người thuê";
        this.cboRoom.BackColor = System.Drawing.ColorTranslator.FromHtml("#15191D");
        this.cboRoom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboRoom.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.cboRoom.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.cboRoom.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.cboRoom.Location = new System.Drawing.Point(56, 13);
        this.cboRoom.Name = "cboRoom";
        this.cboRoom.Size = new System.Drawing.Size(190, 23);
        this.cboRoom.TabIndex = 1;

        //
        // btnAdd
        //
        this.btnAdd.AccessibleName = "Thêm người thuê vào phòng";
        this.btnAdd.BackColor = System.Drawing.ColorTranslator.FromHtml("#D95D39");
        this.btnAdd.FlatAppearance.BorderColor = System.Drawing.ColorTranslator.FromHtml("#D95D39");
        this.btnAdd.FlatAppearance.MouseOverBackColor = System.Drawing.ColorTranslator.FromHtml("#EA6944");
        this.btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnAdd.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.btnAdd.ForeColor = System.Drawing.Color.White;
        this.btnAdd.Location = new System.Drawing.Point(258, 9);
        this.btnAdd.Name = "btnAdd";
        this.btnAdd.Size = new System.Drawing.Size(102, 32);
        this.btnAdd.TabIndex = 2;
        this.btnAdd.Text = "+ Thêm";
        this.btnAdd.UseVisualStyleBackColor = false;

        //
        // btnRefresh
        //
        this.btnRefresh.AccessibleName = "Tải lại người thuê (F5)";
        this.btnRefresh.BackColor = System.Drawing.ColorTranslator.FromHtml("#1C2126");
        this.btnRefresh.FlatAppearance.BorderColor = System.Drawing.ColorTranslator.FromHtml("#2E373F");
        this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnRefresh.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.btnRefresh.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.btnRefresh.Location = new System.Drawing.Point(366, 9);
        this.btnRefresh.Name = "btnRefresh";
        this.btnRefresh.Size = new System.Drawing.Size(110, 32);
        this.btnRefresh.TabIndex = 3;
        this.btnRefresh.Text = "Tải lại (F5)";
        this.btnRefresh.UseVisualStyleBackColor = false;

        //
        // lblHint
        //
        this.lblHint.AutoSize = true;
        this.lblHint.BackColor = System.Drawing.Color.Transparent;
        this.lblHint.Font = new System.Drawing.Font("Segoe UI", 8.5F);
        this.lblHint.ForeColor = System.Drawing.ColorTranslator.FromHtml("#767E88");
        this.lblHint.Location = new System.Drawing.Point(500, 18);
        this.lblHint.Name = "lblHint";
        this.lblHint.Size = new System.Drawing.Size(300, 15);
        this.lblHint.TabIndex = 4;
        this.lblHint.Text = "Mật khẩu để trống = 6 số cuối CCCD";

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
            this.colTemporary});
        this.dgvTenants.Dock = System.Windows.Forms.DockStyle.Fill;
        this.dgvTenants.Location = new System.Drawing.Point(14, 64);
        this.dgvTenants.Name = "dgvTenants";
        this.dgvTenants.Size = new System.Drawing.Size(972, 450);
        this.dgvTenants.TabIndex = 1;

        //
        // colFullName
        //
        this.colFullName.HeaderText = "HỌ TÊN";
        this.colFullName.Name = "colFullName";
        this.colFullName.ReadOnly = true;
        this.colFullName.Width = 160;

        //
        // colIdCard
        //
        this.colIdCard.DefaultCellStyle = new System.Windows.Forms.DataGridViewCellStyle
        {
            Font = new System.Drawing.Font("Consolas", 9.5F),
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
            Font = new System.Drawing.Font("Consolas", 9.5F),
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
            Font = new System.Drawing.Font("Consolas", 9.5F),
        };
        this.colPhone.HeaderText = "SĐT";
        this.colPhone.Name = "colPhone";
        this.colPhone.ReadOnly = true;
        this.colPhone.Width = 125;

        //
        // colHometown
        //
        this.colHometown.HeaderText = "QUÊ QUÁN";
        this.colHometown.Name = "colHometown";
        this.colHometown.ReadOnly = true;
        this.colHometown.Width = 150;

        //
        // colTemporary
        //
        this.colTemporary.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
        this.colTemporary.HeaderText = "TẠM TRÚ";
        this.colTemporary.Name = "colTemporary";
        this.colTemporary.ReadOnly = true;

        //
        // pnlActions
        //
        this.pnlActions.BackColor = System.Drawing.ColorTranslator.FromHtml("#161B1F");
        this.pnlActions.Controls.Add(this.btnDelete);
        this.pnlActions.Controls.Add(this.btnCheckout);
        this.pnlActions.Controls.Add(this.btnEdit);
        this.pnlActions.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.pnlActions.Location = new System.Drawing.Point(14, 514);
        this.pnlActions.Name = "pnlActions";
        this.pnlActions.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
        this.pnlActions.Size = new System.Drawing.Size(972, 52);
        this.pnlActions.TabIndex = 2;

        //
        // btnEdit
        //
        this.btnEdit.AccessibleName = "Sửa người thuê đã chọn";
        this.btnEdit.BackColor = System.Drawing.ColorTranslator.FromHtml("#1C2126");
        this.btnEdit.FlatAppearance.BorderColor = System.Drawing.ColorTranslator.FromHtml("#2E373F");
        this.btnEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnEdit.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.btnEdit.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.btnEdit.Location = new System.Drawing.Point(12, 10);
        this.btnEdit.Name = "btnEdit";
        this.btnEdit.Size = new System.Drawing.Size(96, 32);
        this.btnEdit.TabIndex = 0;
        this.btnEdit.Text = "Sửa";
        this.btnEdit.UseVisualStyleBackColor = false;

        //
        // btnCheckout
        //
        this.btnCheckout.AccessibleName = "Cho người thuê trả phòng";
        this.btnCheckout.BackColor = System.Drawing.ColorTranslator.FromHtml("#1C2126");
        this.btnCheckout.FlatAppearance.BorderColor = System.Drawing.ColorTranslator.FromHtml("#2E373F");
        this.btnCheckout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnCheckout.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.btnCheckout.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.btnCheckout.Location = new System.Drawing.Point(118, 10);
        this.btnCheckout.Name = "btnCheckout";
        this.btnCheckout.Size = new System.Drawing.Size(112, 32);
        this.btnCheckout.TabIndex = 1;
        this.btnCheckout.Text = "Trả phòng";
        this.btnCheckout.UseVisualStyleBackColor = false;

        //
        // btnDelete
        //
        this.btnDelete.AccessibleName = "Xóa hồ sơ người thuê đã chọn";
        this.btnDelete.BackColor = System.Drawing.ColorTranslator.FromHtml("#1F252A");
        this.btnDelete.FlatAppearance.BorderColor = System.Drawing.ColorTranslator.FromHtml("#3E2925");
        this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnDelete.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.btnDelete.ForeColor = System.Drawing.ColorTranslator.FromHtml("#FFB4AB");
        this.btnDelete.Location = new System.Drawing.Point(240, 10);
        this.btnDelete.Name = "btnDelete";
        this.btnDelete.Size = new System.Drawing.Size(114, 32);
        this.btnDelete.TabIndex = 2;
        this.btnDelete.Text = "Xóa hồ sơ";
        this.btnDelete.UseVisualStyleBackColor = false;

        //
        // pnlForm
        //
        this.pnlForm.BackColor = System.Drawing.ColorTranslator.FromHtml("#181C1F");
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
        this.pnlForm.Location = new System.Drawing.Point(586, 64);
        this.pnlForm.Name = "pnlForm";
        this.pnlForm.Padding = new System.Windows.Forms.Padding(14);
        this.pnlForm.Size = new System.Drawing.Size(400, 450);
        this.pnlForm.TabIndex = 3;
        this.pnlForm.Visible = false;

        //
        // lblFormTitle
        //
        this.lblFormTitle.AutoSize = true;
        this.lblFormTitle.BackColor = System.Drawing.Color.Transparent;
        this.lblFormTitle.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
        this.lblFormTitle.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
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
        this.lblFullName.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblFullName.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblFullName.Location = new System.Drawing.Point(14, 48);
        this.lblFullName.Name = "lblFullName";
        this.lblFullName.Size = new System.Drawing.Size(45, 15);
        this.lblFullName.TabIndex = 1;
        this.lblFullName.Text = "Họ tên";

        //
        // txtFullName
        //
        this.txtFullName.AccessibleName = "Họ tên người thuê";
        this.txtFullName.BackColor = System.Drawing.ColorTranslator.FromHtml("#15191D");
        this.txtFullName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.txtFullName.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.txtFullName.Location = new System.Drawing.Point(14, 68);
        this.txtFullName.MaxLength = 100;
        this.txtFullName.Name = "txtFullName";
        this.txtFullName.Size = new System.Drawing.Size(170, 23);
        this.txtFullName.TabIndex = 2;

        //
        // lblIdCard
        //
        this.lblIdCard.AutoSize = true;
        this.lblIdCard.BackColor = System.Drawing.Color.Transparent;
        this.lblIdCard.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblIdCard.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblIdCard.Location = new System.Drawing.Point(204, 48);
        this.lblIdCard.Name = "lblIdCard";
        this.lblIdCard.Size = new System.Drawing.Size(35, 15);
        this.lblIdCard.TabIndex = 3;
        this.lblIdCard.Text = "CCCD";

        //
        // txtIdCard
        //
        this.txtIdCard.AccessibleName = "Số CCCD người thuê, 12 chữ số";
        this.txtIdCard.BackColor = System.Drawing.ColorTranslator.FromHtml("#15191D");
        this.txtIdCard.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.txtIdCard.Font = new System.Drawing.Font("Consolas", 9.5F);
        this.txtIdCard.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.txtIdCard.Location = new System.Drawing.Point(204, 68);
        this.txtIdCard.MaxLength = 12;
        this.txtIdCard.Name = "txtIdCard";
        this.txtIdCard.Size = new System.Drawing.Size(170, 22);
        this.txtIdCard.TabIndex = 4;

        //
        // lblDateOfBirth
        //
        this.lblDateOfBirth.AutoSize = true;
        this.lblDateOfBirth.BackColor = System.Drawing.Color.Transparent;
        this.lblDateOfBirth.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblDateOfBirth.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblDateOfBirth.Location = new System.Drawing.Point(14, 104);
        this.lblDateOfBirth.Name = "lblDateOfBirth";
        this.lblDateOfBirth.Size = new System.Drawing.Size(65, 15);
        this.lblDateOfBirth.TabIndex = 5;
        this.lblDateOfBirth.Text = "Ngày sinh";

        //
        // dtpDateOfBirth
        //
        this.dtpDateOfBirth.AccessibleName = "Ngày sinh người thuê";
        this.dtpDateOfBirth.CalendarForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.dtpDateOfBirth.CalendarMonthBackground = System.Drawing.ColorTranslator.FromHtml("#181C1F");
        this.dtpDateOfBirth.CustomFormat = "dd/MM/yyyy";
        this.dtpDateOfBirth.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
        this.dtpDateOfBirth.Location = new System.Drawing.Point(14, 124);
        this.dtpDateOfBirth.Name = "dtpDateOfBirth";
        this.dtpDateOfBirth.Size = new System.Drawing.Size(170, 23);
        this.dtpDateOfBirth.TabIndex = 6;
        this.dtpDateOfBirth.Value = new System.DateTime(2000, 1, 1, 0, 0, 0, 0);

        //
        // lblPhone
        //
        this.lblPhone.AutoSize = true;
        this.lblPhone.BackColor = System.Drawing.Color.Transparent;
        this.lblPhone.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblPhone.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblPhone.Location = new System.Drawing.Point(204, 104);
        this.lblPhone.Name = "lblPhone";
        this.lblPhone.Size = new System.Drawing.Size(77, 15);
        this.lblPhone.TabIndex = 7;
        this.lblPhone.Text = "Số điện thoại";

        //
        // txtPhone
        //
        this.txtPhone.AccessibleName = "Số điện thoại người thuê";
        this.txtPhone.BackColor = System.Drawing.ColorTranslator.FromHtml("#15191D");
        this.txtPhone.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.txtPhone.Font = new System.Drawing.Font("Consolas", 9.5F);
        this.txtPhone.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.txtPhone.Location = new System.Drawing.Point(204, 124);
        this.txtPhone.MaxLength = 20;
        this.txtPhone.Name = "txtPhone";
        this.txtPhone.Size = new System.Drawing.Size(170, 22);
        this.txtPhone.TabIndex = 8;

        //
        // lblHometown
        //
        this.lblHometown.AutoSize = true;
        this.lblHometown.BackColor = System.Drawing.Color.Transparent;
        this.lblHometown.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblHometown.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblHometown.Location = new System.Drawing.Point(14, 160);
        this.lblHometown.Name = "lblHometown";
        this.lblHometown.Size = new System.Drawing.Size(55, 15);
        this.lblHometown.TabIndex = 9;
        this.lblHometown.Text = "Quê quán";

        //
        // txtHometown
        //
        this.txtHometown.AccessibleName = "Quê quán người thuê";
        this.txtHometown.BackColor = System.Drawing.ColorTranslator.FromHtml("#15191D");
        this.txtHometown.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.txtHometown.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.txtHometown.Location = new System.Drawing.Point(14, 180);
        this.txtHometown.MaxLength = 150;
        this.txtHometown.Name = "txtHometown";
        this.txtHometown.Size = new System.Drawing.Size(170, 23);
        this.txtHometown.TabIndex = 10;

        //
        // lblWorkplace
        //
        this.lblWorkplace.AutoSize = true;
        this.lblWorkplace.BackColor = System.Drawing.Color.Transparent;
        this.lblWorkplace.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblWorkplace.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblWorkplace.Location = new System.Drawing.Point(204, 160);
        this.lblWorkplace.Name = "lblWorkplace";
        this.lblWorkplace.Size = new System.Drawing.Size(78, 15);
        this.lblWorkplace.TabIndex = 11;
        this.lblWorkplace.Text = "Nơi làm việc";

        //
        // txtWorkplace
        //
        this.txtWorkplace.AccessibleName = "Nơi làm việc người thuê";
        this.txtWorkplace.BackColor = System.Drawing.ColorTranslator.FromHtml("#15191D");
        this.txtWorkplace.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.txtWorkplace.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.txtWorkplace.Location = new System.Drawing.Point(204, 180);
        this.txtWorkplace.MaxLength = 150;
        this.txtWorkplace.Name = "txtWorkplace";
        this.txtWorkplace.Size = new System.Drawing.Size(170, 23);
        this.txtWorkplace.TabIndex = 12;

        //
        // chkTemporary
        //
        this.chkTemporary.AccessibleName = "Đã đăng ký tạm trú";
        this.chkTemporary.AutoSize = true;
        this.chkTemporary.ForeColor = System.Drawing.ColorTranslator.FromHtml("#CAC6C1");
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
        this.lblPassword.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblPassword.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblPassword.Location = new System.Drawing.Point(14, 254);
        this.lblPassword.Name = "lblPassword";
        this.lblPassword.Size = new System.Drawing.Size(61, 15);
        this.lblPassword.TabIndex = 14;
        this.lblPassword.Text = "Mật khẩu";

        //
        // txtPassword
        //
        this.txtPassword.AccessibleName = "Mật khẩu người thuê, không bắt buộc";
        this.txtPassword.BackColor = System.Drawing.ColorTranslator.FromHtml("#15191D");
        this.txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.txtPassword.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.txtPassword.Location = new System.Drawing.Point(14, 274);
        this.txtPassword.MaxLength = 100;
        this.txtPassword.Name = "txtPassword";
        this.txtPassword.PasswordChar = '●';
        this.txtPassword.Size = new System.Drawing.Size(360, 23);
        this.txtPassword.TabIndex = 15;

        //
        // lblPasswordHint
        //
        this.lblPasswordHint.AutoSize = true;
        this.lblPasswordHint.BackColor = System.Drawing.Color.Transparent;
        this.lblPasswordHint.Font = new System.Drawing.Font("Segoe UI", 8.25F);
        this.lblPasswordHint.ForeColor = System.Drawing.ColorTranslator.FromHtml("#767E88");
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
        this.lblError.ForeColor = System.Drawing.ColorTranslator.FromHtml("#FFB4AB");
        this.lblError.Location = new System.Drawing.Point(14, 326);
        this.lblError.Name = "lblError";
        this.lblError.Size = new System.Drawing.Size(360, 38);
        this.lblError.TabIndex = 17;

        //
        // btnSave
        //
        this.btnSave.AccessibleName = "Lưu người thuê";
        this.btnSave.BackColor = System.Drawing.ColorTranslator.FromHtml("#D95D39");
        this.btnSave.FlatAppearance.BorderColor = System.Drawing.ColorTranslator.FromHtml("#D95D39");
        this.btnSave.FlatAppearance.MouseOverBackColor = System.Drawing.ColorTranslator.FromHtml("#EA6944");
        this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnSave.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.btnSave.ForeColor = System.Drawing.Color.White;
        this.btnSave.Location = new System.Drawing.Point(14, 374);
        this.btnSave.Name = "btnSave";
        this.btnSave.Size = new System.Drawing.Size(174, 34);
        this.btnSave.TabIndex = 18;
        this.btnSave.Text = "Lưu";
        this.btnSave.UseVisualStyleBackColor = false;

        //
        // btnCancel
        //
        this.btnCancel.AccessibleName = "Hủy nhập người thuê";
        this.btnCancel.BackColor = System.Drawing.ColorTranslator.FromHtml("#1C2126");
        this.btnCancel.FlatAppearance.BorderColor = System.Drawing.ColorTranslator.FromHtml("#2E373F");
        this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.btnCancel.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.btnCancel.Location = new System.Drawing.Point(200, 374);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(174, 34);
        this.btnCancel.TabIndex = 19;
        this.btnCancel.Text = "Hủy";
        this.btnCancel.UseVisualStyleBackColor = false;

        //
        // TenantsForm
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.AccessibleName = "Trang quản lý người thuê";
        this.BackColor = System.Drawing.ColorTranslator.FromHtml("#101417");
        this.Controls.Add(this.dgvTenants);
        this.Controls.Add(this.pnlForm);
        this.Controls.Add(this.pnlActions);
        this.Controls.Add(this.pnlToolbar);
        this.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.Name = "TenantsForm";
        this.Padding = new System.Windows.Forms.Padding(14);
        this.Size = new System.Drawing.Size(1000, 600);

        this.pnlToolbar.ResumeLayout(false);
        this.pnlToolbar.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvTenants)).EndInit();
        this.pnlActions.ResumeLayout(false);
        this.pnlForm.ResumeLayout(false);
        this.pnlForm.PerformLayout();
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.Panel pnlToolbar;
    private System.Windows.Forms.Label lblRoom;
    private System.Windows.Forms.ComboBox cboRoom;
    private System.Windows.Forms.Button btnAdd;
    private System.Windows.Forms.Button btnRefresh;
    private System.Windows.Forms.Label lblHint;
    private System.Windows.Forms.DataGridView dgvTenants;
    private System.Windows.Forms.DataGridViewTextBoxColumn colFullName;
    private System.Windows.Forms.DataGridViewTextBoxColumn colIdCard;
    private System.Windows.Forms.DataGridViewTextBoxColumn colDateOfBirth;
    private System.Windows.Forms.DataGridViewTextBoxColumn colPhone;
    private System.Windows.Forms.DataGridViewTextBoxColumn colHometown;
    private System.Windows.Forms.DataGridViewTextBoxColumn colTemporary;
    private System.Windows.Forms.Panel pnlActions;
    private System.Windows.Forms.Button btnDelete;
    private System.Windows.Forms.Button btnCheckout;
    private System.Windows.Forms.Button btnEdit;
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
