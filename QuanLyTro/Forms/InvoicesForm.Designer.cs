namespace QuanLyTro.Forms
{
    partial class InvoicesForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel pnlToolbar;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.Panel pnlDetail;
        private System.Windows.Forms.Panel pnlDetailHeader;
        private System.Windows.Forms.Panel pnlDetailBody;
        private System.Windows.Forms.Panel pnlDetailTotal;
        private System.Windows.Forms.Panel pnlTotalRule;
        private System.Windows.Forms.Panel pnlForm;

        private System.Windows.Forms.Label lblMonth;
        private System.Windows.Forms.ComboBox cboMonth;
        private System.Windows.Forms.Button btnReload;
        private System.Windows.Forms.CheckBox chkUnpaidOnly;
        private System.Windows.Forms.Button btnToggleCreate;

        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.Label lblListTitle;
        private System.Windows.Forms.Label lblRowCount;

        private System.Windows.Forms.Label lblDetailTitle;
        private System.Windows.Forms.Label lblDetailSubtitle;
        private System.Windows.Forms.Label lblDetailStatus;
        private System.Windows.Forms.Label lblEmptyDetail;
        private System.Windows.Forms.Label lblRoom;
        private System.Windows.Forms.Label lblElectricity;
        private System.Windows.Forms.Label lblWater;
        private System.Windows.Forms.Label lblOther;
        private System.Windows.Forms.Label lblRoomValue;
        private System.Windows.Forms.Label lblElectricityValue;
        private System.Windows.Forms.Label lblWaterValue;
        private System.Windows.Forms.Label lblOtherValue;
        private System.Windows.Forms.Label lblTotalCaption;
        private System.Windows.Forms.Label lblTotalValue;

        private System.Windows.Forms.Label lblFormTitle;
        private System.Windows.Forms.Label lblFormRoom;
        private System.Windows.Forms.ComboBox cboRoom;
        private System.Windows.Forms.Label lblFormFees;
        private System.Windows.Forms.NumericUpDown numOtherFees;
        private System.Windows.Forms.Label lblFormNote;
        private System.Windows.Forms.Label lblError;
        private System.Windows.Forms.Button btnCreateSubmit;

        /// <summary>Clean up any resources being used.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                if (components == null)
                {
                    grid?.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pnlToolbar = new System.Windows.Forms.Panel();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.pnlMain = new System.Windows.Forms.Panel();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.grid = new System.Windows.Forms.DataGridView();
            this.pnlDetail = new System.Windows.Forms.Panel();
            this.pnlDetailHeader = new System.Windows.Forms.Panel();
            this.pnlDetailBody = new System.Windows.Forms.Panel();
            this.pnlDetailTotal = new System.Windows.Forms.Panel();
            this.pnlTotalRule = new System.Windows.Forms.Panel();
            this.pnlForm = new System.Windows.Forms.Panel();
            this.lblMonth = new System.Windows.Forms.Label();
            this.cboMonth = new System.Windows.Forms.ComboBox();
            this.btnReload = new System.Windows.Forms.Button();
            this.chkUnpaidOnly = new System.Windows.Forms.CheckBox();
            this.btnToggleCreate = new System.Windows.Forms.Button();
            this.lblListTitle = new System.Windows.Forms.Label();
            this.lblRowCount = new System.Windows.Forms.Label();
            this.lblDetailTitle = new System.Windows.Forms.Label();
            this.lblDetailSubtitle = new System.Windows.Forms.Label();
            this.lblDetailStatus = new System.Windows.Forms.Label();
            this.lblEmptyDetail = new System.Windows.Forms.Label();
            this.lblRoom = new System.Windows.Forms.Label();
            this.lblElectricity = new System.Windows.Forms.Label();
            this.lblWater = new System.Windows.Forms.Label();
            this.lblOther = new System.Windows.Forms.Label();
            this.lblRoomValue = new System.Windows.Forms.Label();
            this.lblElectricityValue = new System.Windows.Forms.Label();
            this.lblWaterValue = new System.Windows.Forms.Label();
            this.lblOtherValue = new System.Windows.Forms.Label();
            this.lblTotalCaption = new System.Windows.Forms.Label();
            this.lblTotalValue = new System.Windows.Forms.Label();
            this.lblFormTitle = new System.Windows.Forms.Label();
            this.lblFormRoom = new System.Windows.Forms.Label();
            this.cboRoom = new System.Windows.Forms.ComboBox();
            this.lblFormFees = new System.Windows.Forms.Label();
            this.numOtherFees = new System.Windows.Forms.NumericUpDown();
            this.lblFormNote = new System.Windows.Forms.Label();
            this.lblError = new System.Windows.Forms.Label();
            this.btnCreateSubmit = new System.Windows.Forms.Button();

            this.pnlToolbar.SuspendLayout();
            this.pnlHeader.SuspendLayout();
            this.pnlMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.pnlDetail.SuspendLayout();
            this.pnlDetailHeader.SuspendLayout();
            this.pnlDetailBody.SuspendLayout();
            this.pnlDetailTotal.SuspendLayout();
            this.pnlForm.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numOtherFees)).BeginInit();
            this.SuspendLayout();

            // ---- toolbar ----
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Height = 44;
            this.pnlToolbar.BackColor = ScreenTheme.From(ScreenTheme.Base);
            this.pnlToolbar.Padding = new System.Windows.Forms.Padding(12, 6, 12, 6);
            this.pnlToolbar.Controls.Add(this.lblMonth);
            this.pnlToolbar.Controls.Add(this.cboMonth);
            this.pnlToolbar.Controls.Add(this.btnReload);
            this.pnlToolbar.Controls.Add(this.chkUnpaidOnly);
            this.pnlToolbar.Controls.Add(this.btnToggleCreate);

            this.lblMonth.AutoSize = true;
            this.lblMonth.Location = new System.Drawing.Point(12, 14);
            this.lblMonth.Text = "&Tháng";
            this.lblMonth.TabIndex = 0;
            this.lblMonth.ForeColor = ScreenTheme.From(ScreenTheme.Muted);
            this.lblMonth.Font = ScreenTheme.Body;
            this.lblMonth.BackColor = System.Drawing.Color.Transparent;

            this.cboMonth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.cboMonth.Location = new System.Drawing.Point(58, 10);
            this.cboMonth.Size = new System.Drawing.Size(104, 23);
            this.cboMonth.TabIndex = 1;
            this.cboMonth.AccessibleName = "Chọn tháng hóa đơn";

            this.btnReload.Location = new System.Drawing.Point(170, 9);
            this.btnReload.Size = new System.Drawing.Size(92, 25);
            this.btnReload.TabIndex = 2;
            this.btnReload.Text = "Tải lại (F5)";
            this.btnReload.AccessibleName = "Tải lại danh sách";

            this.chkUnpaidOnly.AutoSize = true;
            this.chkUnpaidOnly.Location = new System.Drawing.Point(272, 13);
            this.chkUnpaidOnly.TabIndex = 3;
            this.chkUnpaidOnly.Text = "Chỉ hiện chưa thu";
            this.chkUnpaidOnly.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
            this.chkUnpaidOnly.Font = ScreenTheme.Body;
            this.chkUnpaidOnly.BackColor = System.Drawing.Color.Transparent;
            this.chkUnpaidOnly.AccessibleName = "Lọc chỉ hiện chưa thu";

            this.btnToggleCreate.Location = new System.Drawing.Point(880, 9);
            this.btnToggleCreate.Size = new System.Drawing.Size(130, 25);
            this.btnToggleCreate.TabIndex = 4;
            this.btnToggleCreate.Text = "+ Lập hóa đơn";
            this.btnToggleCreate.AccessibleName = "Mở form lập hóa đơn";

            // ---- header ----
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height = 34;
            this.pnlHeader.BackColor = ScreenTheme.From(ScreenTheme.Panel);
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(12, 0, 12, 0);
            this.pnlHeader.Controls.Add(this.lblListTitle);
            this.pnlHeader.Controls.Add(this.lblRowCount);

            this.lblListTitle.AutoSize = true;
            this.lblListTitle.Location = new System.Drawing.Point(12, 9);
            this.lblListTitle.Text = "DANH SÁCH HÓA ĐƠN";
            this.lblListTitle.Font = ScreenTheme.HeaderFont;
            this.lblListTitle.ForeColor = ScreenTheme.From(ScreenTheme.Muted);
            this.lblListTitle.BackColor = System.Drawing.Color.Transparent;

            this.lblRowCount.AutoSize = false;
            this.lblRowCount.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.lblRowCount.Location = new System.Drawing.Point(920, 9);
            this.lblRowCount.Size = new System.Drawing.Size(108, 16);
            this.lblRowCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblRowCount.Text = "0 bản ghi";
            this.lblRowCount.Font = ScreenTheme.Body;
            this.lblRowCount.ForeColor = ScreenTheme.From(ScreenTheme.Dim);
            this.lblRowCount.BackColor = System.Drawing.Color.Transparent;

            // ---- main / split ----
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.BackColor = ScreenTheme.From(ScreenTheme.Base);
            this.pnlMain.Padding = new System.Windows.Forms.Padding(12, 0, 12, 8);
            this.pnlMain.Controls.Add(this.splitMain);

            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitMain.SplitterDistance = 640;
            this.splitMain.SplitterWidth = 8;
            this.splitMain.Panel1.Controls.Add(this.grid);
            this.splitMain.Panel2.Controls.Add(this.pnlForm);
            this.splitMain.Panel2.Controls.Add(this.pnlDetail);

            // ---- grid ----
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.TabIndex = 0;
            this.grid.AccessibleName = "Danh sách hóa đơn";
            this.grid.Name = "grid";

            // ---- detail panel ----
            this.pnlDetail.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDetail.Height = 270;
            this.pnlDetail.BackColor = ScreenTheme.From(ScreenTheme.Card);
            this.pnlDetail.Controls.Add(this.pnlDetailBody);
            this.pnlDetail.Controls.Add(this.pnlDetailTotal);
            this.pnlDetail.Controls.Add(this.pnlDetailHeader);

            this.pnlDetailHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDetailHeader.Height = 40;
            this.pnlDetailHeader.BackColor = ScreenTheme.From(ScreenTheme.Panel);
            this.pnlDetailHeader.Controls.Add(this.lblDetailTitle);
            this.pnlDetailHeader.Controls.Add(this.lblDetailSubtitle);
            this.pnlDetailHeader.Controls.Add(this.lblDetailStatus);

            this.lblDetailTitle.AutoSize = true;
            this.lblDetailTitle.Location = new System.Drawing.Point(10, 5);
            this.lblDetailTitle.Text = "CHI TIẾT HÓA ĐƠN";
            this.lblDetailTitle.Font = ScreenTheme.HeaderFont;
            this.lblDetailTitle.ForeColor = ScreenTheme.From(ScreenTheme.Muted);
            this.lblDetailTitle.BackColor = System.Drawing.Color.Transparent;

            this.lblDetailSubtitle.AutoSize = true;
            this.lblDetailSubtitle.Location = new System.Drawing.Point(10, 21);
            this.lblDetailSubtitle.Text = "Chọn một dòng trong danh sách";
            this.lblDetailSubtitle.Font = ScreenTheme.Body;
            this.lblDetailSubtitle.ForeColor = ScreenTheme.From(ScreenTheme.Dim);
            this.lblDetailSubtitle.BackColor = System.Drawing.Color.Transparent;

            this.lblDetailStatus.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.lblDetailStatus.AutoSize = false;
            this.lblDetailStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblDetailStatus.Location = new System.Drawing.Point(180, 10);
            this.lblDetailStatus.Size = new System.Drawing.Size(160, 20);
            this.lblDetailStatus.Font = ScreenTheme.Body;
            this.lblDetailStatus.BackColor = System.Drawing.Color.Transparent;

            this.pnlDetailBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDetailBody.BackColor = ScreenTheme.From(ScreenTheme.Card);
            this.pnlDetailBody.Controls.Add(this.lblRoom);
            this.pnlDetailBody.Controls.Add(this.lblRoomValue);
            this.pnlDetailBody.Controls.Add(this.lblElectricity);
            this.pnlDetailBody.Controls.Add(this.lblElectricityValue);
            this.pnlDetailBody.Controls.Add(this.lblWater);
            this.pnlDetailBody.Controls.Add(this.lblWaterValue);
            this.pnlDetailBody.Controls.Add(this.lblOther);
            this.pnlDetailBody.Controls.Add(this.lblOtherValue);
            this.pnlDetailBody.Controls.Add(this.lblEmptyDetail);

            this.lblRoom.AutoSize = true;
            this.lblRoom.Location = new System.Drawing.Point(12, 12);
            this.lblRoom.Text = "Tiền thuê phòng";
            this.lblRoom.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
            this.lblRoom.Font = ScreenTheme.Body;
            this.lblRoom.BackColor = System.Drawing.Color.Transparent;

            this.lblRoomValue.AutoSize = false;
            this.lblRoomValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblRoomValue.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.lblRoomValue.Location = new System.Drawing.Point(210, 12);
            this.lblRoomValue.Size = new System.Drawing.Size(130, 18);
            this.lblRoomValue.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
            this.lblRoomValue.Font = ScreenTheme.Mono;
            this.lblRoomValue.BackColor = System.Drawing.Color.Transparent;

            this.lblElectricity.AutoSize = true;
            this.lblElectricity.Location = new System.Drawing.Point(12, 40);
            this.lblElectricity.Text = "Điện năng";
            this.lblElectricity.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
            this.lblElectricity.Font = ScreenTheme.Body;
            this.lblElectricity.BackColor = System.Drawing.Color.Transparent;

            this.lblElectricityValue.AutoSize = false;
            this.lblElectricityValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblElectricityValue.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.lblElectricityValue.Location = new System.Drawing.Point(210, 40);
            this.lblElectricityValue.Size = new System.Drawing.Size(130, 18);
            this.lblElectricityValue.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
            this.lblElectricityValue.Font = ScreenTheme.Mono;
            this.lblElectricityValue.BackColor = System.Drawing.Color.Transparent;

            this.lblWater.AutoSize = true;
            this.lblWater.Location = new System.Drawing.Point(12, 68);
            this.lblWater.Text = "Nước sinh hoạt";
            this.lblWater.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
            this.lblWater.Font = ScreenTheme.Body;
            this.lblWater.BackColor = System.Drawing.Color.Transparent;

            this.lblWaterValue.AutoSize = false;
            this.lblWaterValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblWaterValue.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.lblWaterValue.Location = new System.Drawing.Point(210, 68);
            this.lblWaterValue.Size = new System.Drawing.Size(130, 18);
            this.lblWaterValue.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
            this.lblWaterValue.Font = ScreenTheme.Mono;
            this.lblWaterValue.BackColor = System.Drawing.Color.Transparent;

            this.lblOther.AutoSize = true;
            this.lblOther.Location = new System.Drawing.Point(12, 96);
            this.lblOther.Text = "Dịch vụ & khác";
            this.lblOther.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
            this.lblOther.Font = ScreenTheme.Body;
            this.lblOther.BackColor = System.Drawing.Color.Transparent;

            this.lblOtherValue.AutoSize = false;
            this.lblOtherValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblOtherValue.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.lblOtherValue.Location = new System.Drawing.Point(210, 96);
            this.lblOtherValue.Size = new System.Drawing.Size(130, 18);
            this.lblOtherValue.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
            this.lblOtherValue.Font = ScreenTheme.Mono;
            this.lblOtherValue.BackColor = System.Drawing.Color.Transparent;

            this.lblEmptyDetail.AutoSize = true;
            this.lblEmptyDetail.Location = new System.Drawing.Point(12, 12);
            this.lblEmptyDetail.Text = "Chưa chọn hóa đơn nào.";
            this.lblEmptyDetail.ForeColor = ScreenTheme.From(ScreenTheme.Dim);
            this.lblEmptyDetail.Font = ScreenTheme.Body;
            this.lblEmptyDetail.BackColor = System.Drawing.Color.Transparent;

            this.pnlDetailTotal.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlDetailTotal.Height = 56;
            this.pnlDetailTotal.BackColor = ScreenTheme.From(ScreenTheme.HeaderStrip);
            this.pnlDetailTotal.Controls.Add(this.pnlTotalRule);
            this.pnlDetailTotal.Controls.Add(this.lblTotalCaption);
            this.pnlDetailTotal.Controls.Add(this.lblTotalValue);

            // border-emphasis-top: dải trên dày 2px của khối tổng tiền (DESIGN.md §2.2)
            this.pnlTotalRule.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTotalRule.Height = 2;
            this.pnlTotalRule.BackColor = ScreenTheme.From(ScreenTheme.EmphasisTop);

            this.lblTotalCaption.AutoSize = true;
            this.lblTotalCaption.Location = new System.Drawing.Point(12, 9);
            this.lblTotalCaption.Text = "TỔNG CẦN THANH TOÁN";
            this.lblTotalCaption.Font = ScreenTheme.HeaderFont;
            this.lblTotalCaption.ForeColor = ScreenTheme.From(ScreenTheme.Muted);
            this.lblTotalCaption.BackColor = System.Drawing.Color.Transparent;

            this.lblTotalValue.AutoSize = true;
            this.lblTotalValue.Location = new System.Drawing.Point(10, 24);
            this.lblTotalValue.Text = "0 đ";
            this.lblTotalValue.Font = ScreenTheme.TotalValue;
            this.lblTotalValue.ForeColor = ScreenTheme.From(ScreenTheme.CreamLight);
            this.lblTotalValue.BackColor = System.Drawing.Color.Transparent;

            // ---- create form ----
            this.pnlForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlForm.BackColor = ScreenTheme.From(ScreenTheme.Card);
            this.pnlForm.Controls.Add(this.lblFormTitle);
            this.pnlForm.Controls.Add(this.lblFormRoom);
            this.pnlForm.Controls.Add(this.cboRoom);
            this.pnlForm.Controls.Add(this.lblFormFees);
            this.pnlForm.Controls.Add(this.numOtherFees);
            this.pnlForm.Controls.Add(this.lblFormNote);
            this.pnlForm.Controls.Add(this.lblError);
            this.pnlForm.Controls.Add(this.btnCreateSubmit);

            this.lblFormTitle.AutoSize = true;
            this.lblFormTitle.Location = new System.Drawing.Point(12, 10);
            this.lblFormTitle.Text = "LẬP HÓA ĐƠN THÁNG";
            this.lblFormTitle.Font = ScreenTheme.HeaderFont;
            this.lblFormTitle.ForeColor = ScreenTheme.From(ScreenTheme.Muted);
            this.lblFormTitle.BackColor = System.Drawing.Color.Transparent;

            this.lblFormRoom.AutoSize = true;
            this.lblFormRoom.Location = new System.Drawing.Point(12, 34);
            this.lblFormRoom.Text = "&Phòng";
            this.lblFormRoom.TabIndex = 4;
            this.lblFormRoom.ForeColor = ScreenTheme.From(ScreenTheme.Muted);
            this.lblFormRoom.Font = ScreenTheme.Body;
            this.lblFormRoom.BackColor = System.Drawing.Color.Transparent;

            this.cboRoom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboRoom.Location = new System.Drawing.Point(12, 52);
            this.cboRoom.Size = new System.Drawing.Size(160, 23);
            this.cboRoom.TabIndex = 5;
            this.cboRoom.AccessibleName = "Chọn phòng lập hóa đơn";

            this.lblFormFees.AutoSize = true;
            this.lblFormFees.Location = new System.Drawing.Point(12, 84);
            this.lblFormFees.Text = "Phí &khác (VNĐ)";
            this.lblFormFees.TabIndex = 6;
            this.lblFormFees.ForeColor = ScreenTheme.From(ScreenTheme.Muted);
            this.lblFormFees.Font = ScreenTheme.Body;
            this.lblFormFees.BackColor = System.Drawing.Color.Transparent;

            this.numOtherFees.Location = new System.Drawing.Point(12, 102);
            this.numOtherFees.Size = new System.Drawing.Size(160, 23);
            this.numOtherFees.TabIndex = 7;
            this.numOtherFees.Increment = 10000;
            this.numOtherFees.Minimum = 0;
            this.numOtherFees.Maximum = 1000000000;
            this.numOtherFees.Font = ScreenTheme.Mono;
            this.numOtherFees.AccessibleName = "Nhập phí khác";

            this.lblFormNote.Location = new System.Drawing.Point(12, 134);
            this.lblFormNote.Size = new System.Drawing.Size(220, 60);
            this.lblFormNote.Text = "Server tự tính tiền phòng, điện, nước (BR-10). Cần có HĐ Active và đã chốt điện nước (BR-09).";
            this.lblFormNote.ForeColor = ScreenTheme.From(ScreenTheme.Dim);
            this.lblFormNote.Font = ScreenTheme.Body;
            this.lblFormNote.BackColor = System.Drawing.Color.Transparent;

            this.lblError.Location = new System.Drawing.Point(12, 196);
            this.lblError.Size = new System.Drawing.Size(220, 36);
            this.lblError.Text = string.Empty;
            this.lblError.ForeColor = ScreenTheme.From(ScreenTheme.Error);
            this.lblError.Font = ScreenTheme.Body;
            this.lblError.BackColor = System.Drawing.Color.Transparent;
            this.lblError.Visible = false;

            this.btnCreateSubmit.Location = new System.Drawing.Point(12, 236);
            this.btnCreateSubmit.Size = new System.Drawing.Size(160, 28);
            this.btnCreateSubmit.TabIndex = 8;
            this.btnCreateSubmit.Text = "Xác nhận tạo";
            this.btnCreateSubmit.AccessibleName = "Xác nhận lập hóa đơn";

            // ---- InvoicesForm ----
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = ScreenTheme.From(ScreenTheme.Base);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.pnlToolbar);
            this.Font = ScreenTheme.Body;
            this.MinimumSize = new System.Drawing.Size(760, 420);
            this.Name = "InvoicesForm";
            this.Padding = new System.Windows.Forms.Padding(0);
            this.Size = new System.Drawing.Size(1040, 620);
            this.AccessibleName = "Màn hình quản lý hóa đơn";

            this.pnlToolbar.ResumeLayout(false);
            this.pnlToolbar.PerformLayout();
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlMain.ResumeLayout(false);
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.pnlDetail.ResumeLayout(false);
            this.pnlDetailHeader.ResumeLayout(false);
            this.pnlDetailHeader.PerformLayout();
            this.pnlDetailBody.ResumeLayout(false);
            this.pnlDetailBody.PerformLayout();
            this.pnlDetailTotal.ResumeLayout(false);
            this.pnlDetailTotal.PerformLayout();
            this.pnlForm.ResumeLayout(false);
            this.pnlForm.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numOtherFees)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion
    }
}
