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
        this.pnlToolbar = new System.Windows.Forms.Panel();
        this.lblToolbarTitle = new System.Windows.Forms.Label();
        this.lblCount = new System.Windows.Forms.Label();
        this.btnRefresh = new System.Windows.Forms.Button();
        this.btnDelete = new System.Windows.Forms.Button();
        this.btnEdit = new System.Windows.Forms.Button();
        this.btnAdd = new System.Windows.Forms.Button();
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
        this.dgvRooms = new System.Windows.Forms.DataGridView();
        this.colRoomNumber = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colPrice = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colMaxOccupants = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colCurrentOccupants = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.pnlForm.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.numPrice)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numMaxOccupants)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.dgvRooms)).BeginInit();
        this.SuspendLayout();

        //
        // pnlToolbar
        //
        this.pnlToolbar.BackColor = System.Drawing.ColorTranslator.FromHtml("#161B1F");
        this.pnlToolbar.Controls.Add(this.lblToolbarTitle);
        this.pnlToolbar.Controls.Add(this.lblCount);
        this.pnlToolbar.Controls.Add(this.btnRefresh);
        this.pnlToolbar.Controls.Add(this.btnDelete);
        this.pnlToolbar.Controls.Add(this.btnEdit);
        this.pnlToolbar.Controls.Add(this.btnAdd);
        this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
        this.pnlToolbar.Location = new System.Drawing.Point(14, 14);
        this.pnlToolbar.Name = "pnlToolbar";
        this.pnlToolbar.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
        this.pnlToolbar.Size = new System.Drawing.Size(972, 50);
        this.pnlToolbar.TabIndex = 0;

        //
        // lblToolbarTitle
        //
        this.lblToolbarTitle.AutoSize = true;
        this.lblToolbarTitle.BackColor = System.Drawing.Color.Transparent;
        this.lblToolbarTitle.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
        this.lblToolbarTitle.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.lblToolbarTitle.Location = new System.Drawing.Point(12, 16);
        this.lblToolbarTitle.Name = "lblToolbarTitle";
        this.lblToolbarTitle.Size = new System.Drawing.Size(128, 17);
        this.lblToolbarTitle.TabIndex = 0;
        this.lblToolbarTitle.Text = "DANH SÁCH PHÒNG";

        //
        // btnAdd
        //
        this.btnAdd.AccessibleName = "Thêm phòng mới";
        this.btnAdd.BackColor = System.Drawing.ColorTranslator.FromHtml("#D95D39");
        this.btnAdd.FlatAppearance.BorderColor = System.Drawing.ColorTranslator.FromHtml("#D95D39");
        this.btnAdd.FlatAppearance.MouseOverBackColor = System.Drawing.ColorTranslator.FromHtml("#EA6944");
        this.btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnAdd.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.btnAdd.ForeColor = System.Drawing.Color.White;
        this.btnAdd.Location = new System.Drawing.Point(548, 9);
        this.btnAdd.Name = "btnAdd";
        this.btnAdd.Size = new System.Drawing.Size(96, 32);
        this.btnAdd.TabIndex = 1;
        this.btnAdd.Text = "+ Thêm";
        this.btnAdd.UseVisualStyleBackColor = false;

        //
        // btnEdit
        //
        this.btnEdit.AccessibleName = "Sửa phòng đã chọn";
        this.btnEdit.BackColor = System.Drawing.ColorTranslator.FromHtml("#1C2126");
        this.btnEdit.FlatAppearance.BorderColor = System.Drawing.ColorTranslator.FromHtml("#2E373F");
        this.btnEdit.FlatAppearance.MouseOverBackColor = System.Drawing.ColorTranslator.FromHtml("#272F36");
        this.btnEdit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnEdit.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.btnEdit.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.btnEdit.Location = new System.Drawing.Point(650, 9);
        this.btnEdit.Name = "btnEdit";
        this.btnEdit.Size = new System.Drawing.Size(84, 32);
        this.btnEdit.TabIndex = 2;
        this.btnEdit.Text = "Sửa";
        this.btnEdit.UseVisualStyleBackColor = false;

        //
        // btnDelete
        //
        this.btnDelete.AccessibleName = "Xóa phòng đã chọn";
        this.btnDelete.BackColor = System.Drawing.ColorTranslator.FromHtml("#1F252A");
        this.btnDelete.FlatAppearance.BorderColor = System.Drawing.ColorTranslator.FromHtml("#3E2925");
        this.btnDelete.FlatAppearance.MouseOverBackColor = System.Drawing.ColorTranslator.FromHtml("#32191B");
        this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnDelete.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.btnDelete.ForeColor = System.Drawing.ColorTranslator.FromHtml("#FFB4AB");
        this.btnDelete.Location = new System.Drawing.Point(740, 9);
        this.btnDelete.Name = "btnDelete";
        this.btnDelete.Size = new System.Drawing.Size(84, 32);
        this.btnDelete.TabIndex = 3;
        this.btnDelete.Text = "Xóa";
        this.btnDelete.UseVisualStyleBackColor = false;

        //
        // btnRefresh
        //
        this.btnRefresh.AccessibleName = "Tải lại danh sách phòng (F5)";
        this.btnRefresh.BackColor = System.Drawing.ColorTranslator.FromHtml("#1C2126");
        this.btnRefresh.FlatAppearance.BorderColor = System.Drawing.ColorTranslator.FromHtml("#2E373F");
        this.btnRefresh.FlatAppearance.MouseOverBackColor = System.Drawing.ColorTranslator.FromHtml("#272F36");
        this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnRefresh.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.btnRefresh.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.btnRefresh.Location = new System.Drawing.Point(830, 9);
        this.btnRefresh.Name = "btnRefresh";
        this.btnRefresh.Size = new System.Drawing.Size(132, 32);
        this.btnRefresh.TabIndex = 4;
        this.btnRefresh.Text = "Tải lại (F5)";
        this.btnRefresh.UseVisualStyleBackColor = false;

        //
        // lblCount
        //
        this.lblCount.AutoSize = true;
        this.lblCount.BackColor = System.Drawing.Color.Transparent;
        this.lblCount.Font = new System.Drawing.Font("Consolas", 9F);
        this.lblCount.ForeColor = System.Drawing.ColorTranslator.FromHtml("#767E88");
        this.lblCount.Location = new System.Drawing.Point(12, 33);
        this.lblCount.Name = "lblCount";
        this.lblCount.Size = new System.Drawing.Size(35, 14);
        this.lblCount.TabIndex = 5;
        this.lblCount.Text = "0 bản ghi";

        //
        // pnlForm
        //
        this.pnlForm.BackColor = System.Drawing.ColorTranslator.FromHtml("#181C1F");
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
        this.pnlForm.Location = new System.Drawing.Point(646, 64);
        this.pnlForm.Name = "pnlForm";
        this.pnlForm.Padding = new System.Windows.Forms.Padding(14);
        this.pnlForm.Size = new System.Drawing.Size(340, 522);
        this.pnlForm.TabIndex = 2;
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
        this.lblFormTitle.Size = new System.Drawing.Size(107, 17);
        this.lblFormTitle.TabIndex = 0;
        this.lblFormTitle.Text = "THÔNG TIN PHÒNG";

        //
        // lblRoomNumber
        //
        this.lblRoomNumber.AutoSize = true;
        this.lblRoomNumber.BackColor = System.Drawing.Color.Transparent;
        this.lblRoomNumber.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblRoomNumber.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblRoomNumber.Location = new System.Drawing.Point(14, 52);
        this.lblRoomNumber.Name = "lblRoomNumber";
        this.lblRoomNumber.Size = new System.Drawing.Size(59, 15);
        this.lblRoomNumber.TabIndex = 1;
        this.lblRoomNumber.Text = "Số phòng";

        //
        // txtRoomNumber
        //
        this.txtRoomNumber.AccessibleName = "Số phòng";
        this.txtRoomNumber.BackColor = System.Drawing.ColorTranslator.FromHtml("#15191D");
        this.txtRoomNumber.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.txtRoomNumber.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.txtRoomNumber.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
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
        this.lblPrice.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblPrice.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblPrice.Location = new System.Drawing.Point(14, 112);
        this.lblPrice.Name = "lblPrice";
        this.lblPrice.Size = new System.Drawing.Size(96, 15);
        this.lblPrice.TabIndex = 3;
        this.lblPrice.Text = "Giá thuê (VNĐ)";

        //
        // numPrice
        //
        this.numPrice.AccessibleName = "Giá thuê phòng";
        this.numPrice.BackColor = System.Drawing.ColorTranslator.FromHtml("#15191D");
        this.numPrice.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.numPrice.Font = new System.Drawing.Font("Consolas", 9.5F);
        this.numPrice.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
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
        this.lblMaxOccupants.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblMaxOccupants.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblMaxOccupants.Location = new System.Drawing.Point(14, 172);
        this.lblMaxOccupants.Name = "lblMaxOccupants";
        this.lblMaxOccupants.Size = new System.Drawing.Size(62, 15);
        this.lblMaxOccupants.TabIndex = 5;
        this.lblMaxOccupants.Text = "Sức chứa";

        //
        // numMaxOccupants
        //
        this.numMaxOccupants.AccessibleName = "Sức chứa tối đa";
        this.numMaxOccupants.BackColor = System.Drawing.ColorTranslator.FromHtml("#15191D");
        this.numMaxOccupants.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.numMaxOccupants.Font = new System.Drawing.Font("Consolas", 9.5F);
        this.numMaxOccupants.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
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
        this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblStatus.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblStatus.Location = new System.Drawing.Point(14, 232);
        this.lblStatus.Name = "lblStatus";
        this.lblStatus.Size = new System.Drawing.Size(60, 15);
        this.lblStatus.TabIndex = 7;
        this.lblStatus.Text = "Trạng thái";

        //
        // cboStatus
        //
        this.cboStatus.AccessibleName = "Trạng thái phòng";
        this.cboStatus.BackColor = System.Drawing.ColorTranslator.FromHtml("#15191D");
        this.cboStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboStatus.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.cboStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.cboStatus.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.cboStatus.Location = new System.Drawing.Point(14, 252);
        this.cboStatus.Name = "cboStatus";
        this.cboStatus.Size = new System.Drawing.Size(312, 23);
        this.cboStatus.TabIndex = 8;

        //
        // lblDescription
        //
        this.lblDescription.AutoSize = true;
        this.lblDescription.BackColor = System.Drawing.Color.Transparent;
        this.lblDescription.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblDescription.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblDescription.Location = new System.Drawing.Point(14, 292);
        this.lblDescription.Name = "lblDescription";
        this.lblDescription.Size = new System.Drawing.Size(48, 15);
        this.lblDescription.TabIndex = 9;
        this.lblDescription.Text = "Mô tả";

        //
        // txtDescription
        //
        this.txtDescription.AccessibleName = "Mô tả phòng";
        this.txtDescription.BackColor = System.Drawing.ColorTranslator.FromHtml("#15191D");
        this.txtDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.txtDescription.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.txtDescription.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
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
        this.lblError.ForeColor = System.Drawing.ColorTranslator.FromHtml("#FFB4AB");
        this.lblError.Location = new System.Drawing.Point(14, 384);
        this.lblError.Name = "lblError";
        this.lblError.Size = new System.Drawing.Size(312, 40);
        this.lblError.TabIndex = 11;

        //
        // btnSave
        //
        this.btnSave.AccessibleName = "Lưu phòng";
        this.btnSave.BackColor = System.Drawing.ColorTranslator.FromHtml("#D95D39");
        this.btnSave.FlatAppearance.BorderColor = System.Drawing.ColorTranslator.FromHtml("#D95D39");
        this.btnSave.FlatAppearance.MouseOverBackColor = System.Drawing.ColorTranslator.FromHtml("#EA6944");
        this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnSave.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.btnSave.ForeColor = System.Drawing.Color.White;
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
        this.btnCancel.BackColor = System.Drawing.ColorTranslator.FromHtml("#1C2126");
        this.btnCancel.FlatAppearance.BorderColor = System.Drawing.ColorTranslator.FromHtml("#2E373F");
        this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.btnCancel.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.btnCancel.Location = new System.Drawing.Point(176, 432);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(150, 34);
        this.btnCancel.TabIndex = 13;
        this.btnCancel.Text = "Hủy";
        this.btnCancel.UseVisualStyleBackColor = false;

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
            this.colDescription});
        this.dgvRooms.Dock = System.Windows.Forms.DockStyle.Fill;
        this.dgvRooms.Location = new System.Drawing.Point(354, 64);
        this.dgvRooms.Name = "dgvRooms";
        this.dgvRooms.Size = new System.Drawing.Size(632, 522);
        this.dgvRooms.TabIndex = 1;

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
            Font = new System.Drawing.Font("Consolas", 9.5F),
        };
        this.colPrice.HeaderText = "GIÁ THUÊ";
        this.colPrice.Name = "colPrice";
        this.colPrice.ReadOnly = true;
        this.colPrice.Width = 120;

        //
        // colMaxOccupants
        //
        this.colMaxOccupants.DefaultCellStyle = new System.Windows.Forms.DataGridViewCellStyle
        {
            Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight,
            Font = new System.Drawing.Font("Consolas", 9.5F),
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
            Font = new System.Drawing.Font("Consolas", 9.5F),
        };
        this.colCurrentOccupants.HeaderText = "SỐ NGƯỜI";
        this.colCurrentOccupants.Name = "colCurrentOccupants";
        this.colCurrentOccupants.ReadOnly = true;
        this.colCurrentOccupants.Width = 90;

        //
        // colStatus
        //
        this.colStatus.HeaderText = "TRẠNG THÁI";
        this.colStatus.Name = "colStatus";
        this.colStatus.ReadOnly = true;
        this.colStatus.Width = 120;

        //
        // colDescription
        //
        this.colDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
        this.colDescription.HeaderText = "MÔ TẢ";
        this.colDescription.Name = "colDescription";
        this.colDescription.ReadOnly = true;

        //
        // RoomsForm
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.AccessibleName = "Trang quản lý phòng";
        this.BackColor = System.Drawing.ColorTranslator.FromHtml("#101417");
        this.Controls.Add(this.dgvRooms);
        this.Controls.Add(this.pnlForm);
        this.Controls.Add(this.pnlToolbar);
        this.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.Name = "RoomsForm";
        this.Padding = new System.Windows.Forms.Padding(14);
        this.Size = new System.Drawing.Size(1000, 600);

        this.pnlToolbar.ResumeLayout(false);
        this.pnlToolbar.PerformLayout();
        this.pnlForm.ResumeLayout(false);
        this.pnlForm.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.numPrice)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numMaxOccupants)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.dgvRooms)).EndInit();
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.Panel pnlToolbar;
    private System.Windows.Forms.Label lblToolbarTitle;
    private System.Windows.Forms.Label lblCount;
    private System.Windows.Forms.Button btnRefresh;
    private System.Windows.Forms.Button btnDelete;
    private System.Windows.Forms.Button btnEdit;
    private System.Windows.Forms.Button btnAdd;
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
    private System.Windows.Forms.DataGridView dgvRooms;
    private System.Windows.Forms.DataGridViewTextBoxColumn colRoomNumber;
    private System.Windows.Forms.DataGridViewTextBoxColumn colPrice;
    private System.Windows.Forms.DataGridViewTextBoxColumn colMaxOccupants;
    private System.Windows.Forms.DataGridViewTextBoxColumn colCurrentOccupants;
    private System.Windows.Forms.DataGridViewTextBoxColumn colStatus;
    private System.Windows.Forms.DataGridViewTextBoxColumn colDescription;
}
