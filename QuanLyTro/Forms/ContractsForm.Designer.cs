#nullable enable

namespace QuanLyTro.Forms;

partial class ContractsForm
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
        this.splitMain = new System.Windows.Forms.SplitContainer();
        this.cardInput = new System.Windows.Forms.Panel();
        this.lblTitleInput = new System.Windows.Forms.Label();
        this.lblRoom = new System.Windows.Forms.Label();
        this.cboRoom = new System.Windows.Forms.ComboBox();
        this.lblRepresentative = new System.Windows.Forms.Label();
        this.cboRepresentative = new System.Windows.Forms.ComboBox();
        this.lblStartDate = new System.Windows.Forms.Label();
        this.dtpStartDate = new System.Windows.Forms.DateTimePicker();
        this.lblEndDate = new System.Windows.Forms.Label();
        this.dtpEndDate = new System.Windows.Forms.DateTimePicker();
        this.lblRentalPrice = new System.Windows.Forms.Label();
        this.numRentalPrice = new System.Windows.Forms.NumericUpDown();
        this.lblDepositAmount = new System.Windows.Forms.Label();
        this.numDepositAmount = new System.Windows.Forms.NumericUpDown();
        this.lblNotes = new System.Windows.Forms.Label();
        this.txtNotes = new System.Windows.Forms.TextBox();
        this.btnCreate = new System.Windows.Forms.Button();
        this.tblRight = new System.Windows.Forms.TableLayoutPanel();
        this.toolbarContracts = new QuanLyTro.Forms.ToolbarPanel();
        this.txtSearch = new System.Windows.Forms.TextBox();
        this.cboFilter = new System.Windows.Forms.ComboBox();
        this.btnRefresh = new System.Windows.Forms.Button();
        this.tblKpis = new System.Windows.Forms.TableLayoutPanel();
        this.cardKpiActive = new QuanLyTro.Forms.ContractsForm.KpiCard();
        this.lblKpiActiveCaption = new System.Windows.Forms.Label();
        this.lblKpiActive = new System.Windows.Forms.Label();
        this.cardKpiExpiring = new QuanLyTro.Forms.ContractsForm.KpiCard();
        this.lblKpiExpiringCaption = new System.Windows.Forms.Label();
        this.lblKpiExpiring = new System.Windows.Forms.Label();
        this.cardKpiDeposit = new QuanLyTro.Forms.ContractsForm.KpiCard();
        this.lblKpiDepositCaption = new System.Windows.Forms.Label();
        this.lblKpiDeposit = new System.Windows.Forms.Label();
        this.pnlGridWrap = new System.Windows.Forms.Panel();
        this.dgvContracts = new System.Windows.Forms.DataGridView();
        this.colId = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colRoom = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colRepresentative = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colStartDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colEndDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colRemainingDays = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colRentalPrice = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colDepositAmount = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colNotes = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.tblFoot = new QuanLyTro.Forms.TblFootPanel();
        this.pnlRowButtons = new System.Windows.Forms.Panel();
        this.btnRenew = new System.Windows.Forms.Button();
        this.btnTerminate = new System.Windows.Forms.Button();
        this.noteContracts = new System.Windows.Forms.Panel();
        ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
        this.splitMain.Panel1.SuspendLayout();
        this.splitMain.Panel2.SuspendLayout();
        this.splitMain.SuspendLayout();
        this.cardInput.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.numRentalPrice)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numDepositAmount)).BeginInit();
        this.tblRight.SuspendLayout();
        this.toolbarContracts.SuspendLayout();
        this.tblKpis.SuspendLayout();
        this.cardKpiActive.SuspendLayout();
        this.cardKpiExpiring.SuspendLayout();
        this.cardKpiDeposit.SuspendLayout();
        this.pnlGridWrap.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvContracts)).BeginInit();
        this.pnlRowButtons.SuspendLayout();
        this.SuspendLayout();

        //
        // splitMain
        //
        this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
        this.splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
        this.splitMain.IsSplitterFixed = true;
        this.splitMain.Location = new System.Drawing.Point(0, 0);
        this.splitMain.Name = "splitMain";
        this.splitMain.Orientation = System.Windows.Forms.Orientation.Vertical;
        this.splitMain.SplitterDistance = 320;
        this.splitMain.TabIndex = 0;

        //
        // splitMain.Panel1 (Inputs Card)
        //
        this.splitMain.Panel1.BackColor = System.Drawing.ColorTranslator.FromHtml("#101417");
        this.splitMain.Panel1.Controls.Add(this.cardInput);
        this.splitMain.Panel1.Padding = new System.Windows.Forms.Padding(12);

        //
        // splitMain.Panel2 (Grid Section)
        //
        this.splitMain.Panel2.BackColor = System.Drawing.ColorTranslator.FromHtml("#101417");
        this.splitMain.Panel2.Controls.Add(this.tblRight);
        this.splitMain.Panel2.Padding = new System.Windows.Forms.Padding(0, 12, 12, 12);

        //
        // cardInput
        //
        this.cardInput.BackColor = System.Drawing.ColorTranslator.FromHtml("#181C1F");
        this.cardInput.Dock = System.Windows.Forms.DockStyle.Fill;
        this.cardInput.Controls.Add(this.lblTitleInput);
        this.cardInput.Controls.Add(this.lblRoom);
        this.cardInput.Controls.Add(this.cboRoom);
        this.cardInput.Controls.Add(this.lblRepresentative);
        this.cardInput.Controls.Add(this.cboRepresentative);
        this.cardInput.Controls.Add(this.lblStartDate);
        this.cardInput.Controls.Add(this.dtpStartDate);
        this.cardInput.Controls.Add(this.lblEndDate);
        this.cardInput.Controls.Add(this.dtpEndDate);
        this.cardInput.Controls.Add(this.lblRentalPrice);
        this.cardInput.Controls.Add(this.numRentalPrice);
        this.cardInput.Controls.Add(this.lblDepositAmount);
        this.cardInput.Controls.Add(this.numDepositAmount);
        this.cardInput.Controls.Add(this.lblNotes);
        this.cardInput.Controls.Add(this.txtNotes);
        this.cardInput.Controls.Add(this.btnCreate);
        this.cardInput.Location = new System.Drawing.Point(12, 12);
        this.cardInput.Name = "cardInput";
        this.cardInput.Padding = new System.Windows.Forms.Padding(16);
        this.cardInput.TabIndex = 0;

        //
        // lblTitleInput
        //
        this.lblTitleInput.AutoSize = true;
        this.lblTitleInput.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
        this.lblTitleInput.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.lblTitleInput.Location = new System.Drawing.Point(16, 14);
        this.lblTitleInput.Name = "lblTitleInput";
        this.lblTitleInput.Size = new System.Drawing.Size(160, 19);
        this.lblTitleInput.TabIndex = 0;
        this.lblTitleInput.Text = "LẬP HỢP ĐỒNG MỚI";

        //
        // lblRoom
        //
        this.lblRoom.AutoSize = true;
        this.lblRoom.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblRoom.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblRoom.Location = new System.Drawing.Point(16, 45);
        this.lblRoom.Name = "lblRoom";
        this.lblRoom.Size = new System.Drawing.Size(73, 15);
        this.lblRoom.TabIndex = 1;
        this.lblRoom.Text = "Chọn phòng";

        //
        // cboRoom
        //
        this.cboRoom.AccessibleName = "Chọn phòng lập hợp đồng";
        this.cboRoom.BackColor = System.Drawing.ColorTranslator.FromHtml("#15191D");
        this.cboRoom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboRoom.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.cboRoom.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.cboRoom.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.cboRoom.FormattingEnabled = true;
        this.cboRoom.ItemHeight = 20;
        this.cboRoom.Location = new System.Drawing.Point(16, 65);
        this.cboRoom.Name = "cboRoom";
        this.cboRoom.Size = new System.Drawing.Size(264, 28);
        this.cboRoom.TabIndex = 2;

        //
        // lblRepresentative
        //
        this.lblRepresentative.AutoSize = true;
        this.lblRepresentative.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblRepresentative.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblRepresentative.Location = new System.Drawing.Point(16, 103);
        this.lblRepresentative.Name = "lblRepresentative";
        this.lblRepresentative.Size = new System.Drawing.Size(117, 15);
        this.lblRepresentative.TabIndex = 3;
        this.lblRepresentative.Text = "Người đại diện (ở trọ)";

        //
        // cboRepresentative
        //
        this.cboRepresentative.AccessibleName = "Chọn người đại diện ký hợp đồng";
        this.cboRepresentative.BackColor = System.Drawing.ColorTranslator.FromHtml("#15191D");
        this.cboRepresentative.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboRepresentative.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.cboRepresentative.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.cboRepresentative.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.cboRepresentative.FormattingEnabled = true;
        this.cboRepresentative.ItemHeight = 20;
        this.cboRepresentative.Location = new System.Drawing.Point(16, 123);
        this.cboRepresentative.Name = "cboRepresentative";
        this.cboRepresentative.Size = new System.Drawing.Size(264, 28);
        this.cboRepresentative.TabIndex = 4;

        //
        // lblStartDate
        //
        this.lblStartDate.AutoSize = true;
        this.lblStartDate.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblStartDate.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblStartDate.Location = new System.Drawing.Point(16, 161);
        this.lblStartDate.Name = "lblStartDate";
        this.lblStartDate.Size = new System.Drawing.Size(78, 15);
        this.lblStartDate.TabIndex = 5;
        this.lblStartDate.Text = "Ngày bắt đầu";

        //
        // dtpStartDate
        //
        this.dtpStartDate.AccessibleName = "Ngày bắt đầu hợp đồng";
        this.dtpStartDate.CalendarForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.dtpStartDate.CalendarMonthBackground = System.Drawing.ColorTranslator.FromHtml("#181C1F");
        this.dtpStartDate.CustomFormat = "dd/MM/yyyy";
        this.dtpStartDate.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.dtpStartDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
        this.dtpStartDate.Location = new System.Drawing.Point(16, 181);
        this.dtpStartDate.Name = "dtpStartDate";
        this.dtpStartDate.Size = new System.Drawing.Size(264, 23);
        this.dtpStartDate.TabIndex = 6;

        //
        // lblEndDate
        //
        this.lblEndDate.AutoSize = true;
        this.lblEndDate.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblEndDate.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblEndDate.Location = new System.Drawing.Point(16, 217);
        this.lblEndDate.Name = "lblEndDate";
        this.lblEndDate.Size = new System.Drawing.Size(80, 15);
        this.lblEndDate.TabIndex = 7;
        this.lblEndDate.Text = "Ngày kết thúc";

        //
        // dtpEndDate
        //
        this.dtpEndDate.AccessibleName = "Ngày kết thúc hợp đồng";
        this.dtpEndDate.CalendarForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.dtpEndDate.CalendarMonthBackground = System.Drawing.ColorTranslator.FromHtml("#181C1F");
        this.dtpEndDate.CustomFormat = "dd/MM/yyyy";
        this.dtpEndDate.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.dtpEndDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
        this.dtpEndDate.Location = new System.Drawing.Point(16, 237);
        this.dtpEndDate.Name = "dtpEndDate";
        this.dtpEndDate.Size = new System.Drawing.Size(264, 23);
        this.dtpEndDate.TabIndex = 8;

        //
        // lblRentalPrice
        //
        this.lblRentalPrice.AutoSize = true;
        this.lblRentalPrice.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblRentalPrice.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblRentalPrice.Location = new System.Drawing.Point(16, 273);
        this.lblRentalPrice.Name = "lblRentalPrice";
        this.lblRentalPrice.Size = new System.Drawing.Size(95, 15);
        this.lblRentalPrice.TabIndex = 9;
        this.lblRentalPrice.Text = "Giá thuê (VNĐ)";

        //
        // numRentalPrice
        //
        this.numRentalPrice.AccessibleName = "Giá thuê hợp đồng";
        this.numRentalPrice.BackColor = System.Drawing.ColorTranslator.FromHtml("#15191D");
        this.numRentalPrice.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.numRentalPrice.Font = new System.Drawing.Font("Consolas", 9.5F);
        this.numRentalPrice.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.numRentalPrice.Increment = new decimal(new int[] { 100000, 0, 0, 0 });
        this.numRentalPrice.Location = new System.Drawing.Point(16, 293);
        this.numRentalPrice.Maximum = new decimal(new int[] { 1000000000, 0, 0, 0 });
        this.numRentalPrice.Name = "numRentalPrice";
        this.numRentalPrice.Size = new System.Drawing.Size(264, 22);
        this.numRentalPrice.TabIndex = 10;
        this.numRentalPrice.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.numRentalPrice.ThousandsSeparator = true;

        //
        // lblDepositAmount
        //
        this.lblDepositAmount.AutoSize = true;
        this.lblDepositAmount.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblDepositAmount.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblDepositAmount.Location = new System.Drawing.Point(16, 329);
        this.lblDepositAmount.Name = "lblDepositAmount";
        this.lblDepositAmount.Size = new System.Drawing.Size(96, 15);
        this.lblDepositAmount.TabIndex = 11;
        this.lblDepositAmount.Text = "Tiền cọc (VNĐ)";

        //
        // numDepositAmount
        //
        this.numDepositAmount.AccessibleName = "Tiền đặt cọc";
        this.numDepositAmount.BackColor = System.Drawing.ColorTranslator.FromHtml("#15191D");
        this.numDepositAmount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.numDepositAmount.Font = new System.Drawing.Font("Consolas", 9.5F);
        this.numDepositAmount.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.numDepositAmount.Increment = new decimal(new int[] { 100000, 0, 0, 0 });
        this.numDepositAmount.Location = new System.Drawing.Point(16, 349);
        this.numDepositAmount.Maximum = new decimal(new int[] { 1000000000, 0, 0, 0 });
        this.numDepositAmount.Name = "numDepositAmount";
        this.numDepositAmount.Size = new System.Drawing.Size(264, 22);
        this.numDepositAmount.TabIndex = 12;
        this.numDepositAmount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.numDepositAmount.ThousandsSeparator = true;

        //
        // lblNotes
        //
        this.lblNotes.AutoSize = true;
        this.lblNotes.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblNotes.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblNotes.Location = new System.Drawing.Point(16, 385);
        this.lblNotes.Name = "lblNotes";
        this.lblNotes.Size = new System.Drawing.Size(48, 15);
        this.lblNotes.TabIndex = 13;
        this.lblNotes.Text = "Ghi chú";

        //
        // txtNotes
        //
        this.txtNotes.AccessibleName = "Ghi chú hợp đồng";
        this.txtNotes.BackColor = System.Drawing.ColorTranslator.FromHtml("#15191D");
        this.txtNotes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.txtNotes.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.txtNotes.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.txtNotes.Location = new System.Drawing.Point(16, 405);
        this.txtNotes.MaxLength = 255;
        this.txtNotes.Multiline = true;
        this.txtNotes.Name = "txtNotes";
        this.txtNotes.Size = new System.Drawing.Size(264, 52);
        this.txtNotes.TabIndex = 14;

        //
        // btnCreate
        //
        this.btnCreate.AccessibleName = "Tạo hợp đồng mới";
        this.btnCreate.BackColor = System.Drawing.ColorTranslator.FromHtml("#D95D39");
        this.btnCreate.FlatAppearance.BorderSize = 0;
        this.btnCreate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnCreate.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.btnCreate.ForeColor = System.Drawing.Color.White;
        this.btnCreate.Location = new System.Drawing.Point(16, 475);
        this.btnCreate.Name = "btnCreate";
        this.btnCreate.Size = new System.Drawing.Size(264, 34);
        this.btnCreate.TabIndex = 15;
        this.btnCreate.Text = "Tạo Hợp Đồng";
        this.btnCreate.UseVisualStyleBackColor = false;

        //
        // tblRight — cột phải theo template #tab-contracts: toolbar → KPI → bảng → nút → note
        //
        this.tblRight.ColumnCount = 1;
        this.tblRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tblRight.Controls.Add(this.toolbarContracts, 0, 0);
        this.tblRight.Controls.Add(this.tblKpis, 0, 1);
        this.tblRight.Controls.Add(this.pnlGridWrap, 0, 2);
        this.tblRight.Controls.Add(this.pnlRowButtons, 0, 3);
        this.tblRight.Controls.Add(this.noteContracts, 0, 4);
        this.tblRight.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tblRight.Location = new System.Drawing.Point(0, 12);
        this.tblRight.Name = "tblRight";
        this.tblRight.RowCount = 5;
        this.tblRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56F));
        this.tblRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 86F));
        this.tblRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tblRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
        this.tblRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
        this.tblRight.TabIndex = 0;

        //
        // toolbarContracts — .toolbar: tìm kiếm + lọc + nút Tải lại
        //
        this.toolbarContracts.Controls.Add(this.txtSearch);
        this.toolbarContracts.Controls.Add(this.cboFilter);
        this.toolbarContracts.Controls.Add(this.btnRefresh);
        this.toolbarContracts.Dock = System.Windows.Forms.DockStyle.Fill;
        this.toolbarContracts.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
        this.toolbarContracts.Name = "toolbarContracts";
        this.toolbarContracts.TabIndex = 0;

        //
        // txtSearch
        //
        this.txtSearch.AccessibleName = "Tìm kiếm hợp đồng theo phòng hoặc tên khách";
        this.txtSearch.BackColor = System.Drawing.ColorTranslator.FromHtml("#15191D");
        this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.txtSearch.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.txtSearch.Location = new System.Drawing.Point(6, 7);
        this.txtSearch.Name = "txtSearch";
        this.txtSearch.PlaceholderText = "Tìm CCCD, SĐT, tên khách hoặc số phòng...";
        this.txtSearch.Size = new System.Drawing.Size(280, 23);
        this.txtSearch.TabIndex = 0;

        //
        // cboFilter
        //
        this.cboFilter.AccessibleName = "Lọc hợp đồng theo trạng thái";
        this.cboFilter.BackColor = System.Drawing.ColorTranslator.FromHtml("#15191D");
        this.cboFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboFilter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.cboFilter.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.cboFilter.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.cboFilter.FormattingEnabled = true;
        this.cboFilter.ItemHeight = 20;
        this.cboFilter.Location = new System.Drawing.Point(294, 6);
        this.cboFilter.Name = "cboFilter";
        this.cboFilter.Size = new System.Drawing.Size(190, 28);
        this.cboFilter.TabIndex = 1;

        //
        // btnRefresh
        //
        this.btnRefresh.AccessibleName = "Tải lại danh sách hợp đồng";
        this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.btnRefresh.BackColor = System.Drawing.ColorTranslator.FromHtml("#1C2126");
        this.btnRefresh.FlatAppearance.BorderColor = System.Drawing.ColorTranslator.FromHtml("#2E373F");
        this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnRefresh.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.btnRefresh.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.btnRefresh.Location = new System.Drawing.Point(566, 6);
        this.btnRefresh.Name = "btnRefresh";
        this.btnRefresh.Size = new System.Drawing.Size(88, 32);
        this.btnRefresh.TabIndex = 2;
        this.btnRefresh.Text = "Tải lại";
        this.btnRefresh.UseVisualStyleBackColor = false;

        //
        // tblKpis — 3 thẻ KPI: Hiệu lực / Sắp hết / Tổng cọc
        //
        this.tblKpis.ColumnCount = 3;
        this.tblKpis.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
        this.tblKpis.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
        this.tblKpis.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.34F));
        this.tblKpis.Controls.Add(this.cardKpiActive, 0, 0);
        this.tblKpis.Controls.Add(this.cardKpiExpiring, 1, 0);
        this.tblKpis.Controls.Add(this.cardKpiDeposit, 2, 0);
        this.tblKpis.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tblKpis.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
        this.tblKpis.Name = "tblKpis";
        this.tblKpis.RowCount = 1;
        this.tblKpis.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tblKpis.TabIndex = 1;

        //
        // cardKpiActive — kpi.green: dải đỉnh Sage
        //
        this.cardKpiActive.BorderHex = QuanLyTro.Forms.ScreenTheme.Hairline;
        this.cardKpiActive.Controls.Add(this.lblKpiActiveCaption);
        this.cardKpiActive.Controls.Add(this.lblKpiActive);
        this.cardKpiActive.Dock = System.Windows.Forms.DockStyle.Fill;
        this.cardKpiActive.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
        this.cardKpiActive.Name = "cardKpiActive";
        this.cardKpiActive.TopHex = QuanLyTro.Forms.ScreenTheme.Sage;
        this.cardKpiActive.TabIndex = 0;

        this.lblKpiActiveCaption.AutoSize = true;
        this.lblKpiActiveCaption.Font = QuanLyTro.Forms.ScreenTheme.KpiLabel;
        this.lblKpiActiveCaption.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblKpiActiveCaption.Location = new System.Drawing.Point(12, 11);
        this.lblKpiActiveCaption.Name = "lblKpiActiveCaption";
        this.lblKpiActiveCaption.Text = "HIỆU LỰC";

        this.lblKpiActive.AccessibleName = "Số hợp đồng đang hiệu lực";
        this.lblKpiActive.AutoSize = true;
        this.lblKpiActive.Font = QuanLyTro.Forms.ScreenTheme.KpiValue;
        this.lblKpiActive.ForeColor = System.Drawing.ColorTranslator.FromHtml("#8BD7A3");
        this.lblKpiActive.Location = new System.Drawing.Point(12, 32);
        this.lblKpiActive.Name = "lblKpiActive";
        this.lblKpiActive.Text = "—";

        //
        // cardKpiExpiring — kpi.warn: nền #1F1715, viền #693323, dải đỉnh Terracotta
        //
        this.cardKpiExpiring.BorderHex = QuanLyTro.Forms.ScreenTheme.ErrorBorder;
        this.cardKpiExpiring.Controls.Add(this.lblKpiExpiringCaption);
        this.cardKpiExpiring.Controls.Add(this.lblKpiExpiring);
        this.cardKpiExpiring.Dock = System.Windows.Forms.DockStyle.Fill;
        this.cardKpiExpiring.FillHex = QuanLyTro.Forms.ScreenTheme.ErrorBg;
        this.cardKpiExpiring.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
        this.cardKpiExpiring.Name = "cardKpiExpiring";
        this.cardKpiExpiring.TopHex = QuanLyTro.Forms.ScreenTheme.Terracotta;
        this.cardKpiExpiring.TabIndex = 1;

        this.lblKpiExpiringCaption.AutoSize = true;
        this.lblKpiExpiringCaption.BackColor = System.Drawing.Color.Transparent;
        this.lblKpiExpiringCaption.Font = QuanLyTro.Forms.ScreenTheme.KpiLabel;
        this.lblKpiExpiringCaption.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblKpiExpiringCaption.Location = new System.Drawing.Point(12, 11);
        this.lblKpiExpiringCaption.Name = "lblKpiExpiringCaption";
        this.lblKpiExpiringCaption.Text = "SẮP HẾT";

        this.lblKpiExpiring.AccessibleName = "Số hợp đồng sắp hết hạn trong 30 ngày";
        this.lblKpiExpiring.AutoSize = true;
        this.lblKpiExpiring.BackColor = System.Drawing.Color.Transparent;
        this.lblKpiExpiring.Font = QuanLyTro.Forms.ScreenTheme.KpiValue;
        this.lblKpiExpiring.ForeColor = System.Drawing.ColorTranslator.FromHtml("#D95D39");
        this.lblKpiExpiring.Location = new System.Drawing.Point(12, 32);
        this.lblKpiExpiring.Name = "lblKpiExpiring";
        this.lblKpiExpiring.Text = "—";

        //
        // cardKpiDeposit — kpi mặc định: dải đỉnh border-emphasis-top
        //
        this.cardKpiDeposit.BorderHex = QuanLyTro.Forms.ScreenTheme.Hairline;
        this.cardKpiDeposit.Controls.Add(this.lblKpiDepositCaption);
        this.cardKpiDeposit.Controls.Add(this.lblKpiDeposit);
        this.cardKpiDeposit.Dock = System.Windows.Forms.DockStyle.Fill;
        this.cardKpiDeposit.Name = "cardKpiDeposit";
        this.cardKpiDeposit.TopHex = QuanLyTro.Forms.ScreenTheme.EmphasisTop;
        this.cardKpiDeposit.TabIndex = 2;

        this.lblKpiDepositCaption.AutoSize = true;
        this.lblKpiDepositCaption.Font = QuanLyTro.Forms.ScreenTheme.KpiLabel;
        this.lblKpiDepositCaption.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblKpiDepositCaption.Location = new System.Drawing.Point(12, 11);
        this.lblKpiDepositCaption.Name = "lblKpiDepositCaption";
        this.lblKpiDepositCaption.Text = "TỔNG CỌC";

        this.lblKpiDeposit.AccessibleName = "Tổng tiền đặt cọc";
        this.lblKpiDeposit.AutoSize = true;
        this.lblKpiDeposit.Font = QuanLyTro.Forms.ScreenTheme.KpiValue;
        this.lblKpiDeposit.ForeColor = System.Drawing.ColorTranslator.FromHtml("#FAF8F5");
        this.lblKpiDeposit.Location = new System.Drawing.Point(12, 32);
        this.lblKpiDeposit.Name = "lblKpiDeposit";
        this.lblKpiDeposit.Text = "—";

        //
        // pnlGridWrap — .tblwrap: bảng + dải chân
        //
        this.pnlGridWrap.BackColor = System.Drawing.ColorTranslator.FromHtml("#14181C");
        this.pnlGridWrap.Controls.Add(this.dgvContracts);
        this.pnlGridWrap.Controls.Add(this.tblFoot);
        this.pnlGridWrap.Dock = System.Windows.Forms.DockStyle.Fill;
        this.pnlGridWrap.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
        this.pnlGridWrap.Name = "pnlGridWrap";
        this.pnlGridWrap.TabIndex = 2;

        //
        // dgvContracts
        //
        this.dgvContracts.AccessibleName = "Bảng danh sách hợp đồng";
        this.dgvContracts.AllowUserToAddRows = false;
        this.dgvContracts.AllowUserToDeleteRows = false;
        this.dgvContracts.AllowUserToResizeRows = false;
        this.dgvContracts.AutoGenerateColumns = false;
        this.dgvContracts.BackgroundColor = System.Drawing.ColorTranslator.FromHtml("#14181C");
        this.dgvContracts.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.dgvContracts.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
        this.dgvContracts.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
        this.dgvContracts.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#1A2025");
        this.dgvContracts.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
        this.dgvContracts.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.ColorTranslator.FromHtml("#767E88");
        this.dgvContracts.ColumnHeadersHeight = 34;
        this.dgvContracts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        this.dgvContracts.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colId,
            this.colRoom,
            this.colRepresentative,
            this.colStartDate,
            this.colEndDate,
            this.colRemainingDays,
            this.colRentalPrice,
            this.colDepositAmount,
            this.colStatus,
            this.colNotes});
        this.dgvContracts.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#161B1F");
        this.dgvContracts.DefaultCellStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#14181C");
        this.dgvContracts.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.dgvContracts.DefaultCellStyle.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.dgvContracts.DefaultCellStyle.SelectionBackColor = System.Drawing.ColorTranslator.FromHtml("#21262B");
        this.dgvContracts.DefaultCellStyle.SelectionForeColor = System.Drawing.ColorTranslator.FromHtml("#FAF8F5");
        this.dgvContracts.Dock = System.Windows.Forms.DockStyle.Fill;
        this.dgvContracts.EnableHeadersVisualStyles = false;
        this.dgvContracts.GridColor = System.Drawing.ColorTranslator.FromHtml("#1F252B");
        this.dgvContracts.Location = new System.Drawing.Point(0, 0);
        this.dgvContracts.MultiSelect = false;
        this.dgvContracts.Name = "dgvContracts";
        this.dgvContracts.ReadOnly = true;
        this.dgvContracts.RowHeadersVisible = false;
        this.dgvContracts.RowTemplate.DefaultCellStyle.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
        this.dgvContracts.RowTemplate.Height = 40;
        this.dgvContracts.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this.dgvContracts.Size = new System.Drawing.Size(650, 480);
        this.dgvContracts.TabIndex = 0;

        //
        // tblFoot — .tblfoot
        //
        this.tblFoot.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.tblFoot.Name = "tblFoot";
        this.tblFoot.TabIndex = 1;

        //
        // colId
        //
        this.colId.DataPropertyName = "Id";
        this.colId.HeaderText = "ID";
        this.colId.Name = "colId";
        this.colId.ReadOnly = true;
        this.colId.Width = 45;

        //
        // colRoom
        //
        this.colRoom.DataPropertyName = "RoomNumber";
        this.colRoom.HeaderText = "PHÒNG";
        this.colRoom.Name = "colRoom";
        this.colRoom.ReadOnly = true;
        this.colRoom.Width = 75;

        //
        // colRepresentative
        //
        this.colRepresentative.DataPropertyName = "RepresentativeName";
        this.colRepresentative.HeaderText = "ĐẠI DIỆN";
        this.colRepresentative.Name = "colRepresentative";
        this.colRepresentative.ReadOnly = true;
        this.colRepresentative.Width = 130;

        //
        // colStartDate
        //
        this.colStartDate.DataPropertyName = "StartDateText";
        this.colStartDate.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
        this.colStartDate.HeaderText = "BẮT ĐẦU";
        this.colStartDate.Name = "colStartDate";
        this.colStartDate.ReadOnly = true;
        this.colStartDate.Width = 85;

        //
        // colEndDate
        //
        this.colEndDate.DataPropertyName = "EndDateText";
        this.colEndDate.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
        this.colEndDate.HeaderText = "KẾT THÚC";
        this.colEndDate.Name = "colEndDate";
        this.colEndDate.ReadOnly = true;
        this.colEndDate.Width = 85;

        //
        // colRemainingDays
        //
        this.colRemainingDays.DataPropertyName = "RemainingDaysText";
        this.colRemainingDays.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
        this.colRemainingDays.DefaultCellStyle.Font = new System.Drawing.Font("Consolas", 9.5F, System.Drawing.FontStyle.Bold);
        this.colRemainingDays.HeaderText = "CÒN";
        this.colRemainingDays.Name = "colRemainingDays";
        this.colRemainingDays.ReadOnly = true;
        this.colRemainingDays.Width = 70;

        //
        // colRentalPrice
        //
        this.colRentalPrice.DataPropertyName = "RentalPriceText";
        this.colRentalPrice.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
        this.colRentalPrice.DefaultCellStyle.Font = new System.Drawing.Font("Consolas", 9.5F);
        this.colRentalPrice.HeaderText = "GIÁ THUÊ";
        this.colRentalPrice.Name = "colRentalPrice";
        this.colRentalPrice.ReadOnly = true;
        this.colRentalPrice.Width = 95;

        //
        // colDepositAmount
        //
        this.colDepositAmount.DataPropertyName = "DepositAmountText";
        this.colDepositAmount.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
        this.colDepositAmount.DefaultCellStyle.Font = new System.Drawing.Font("Consolas", 9.5F);
        this.colDepositAmount.HeaderText = "TIỀN CỌC";
        this.colDepositAmount.Name = "colDepositAmount";
        this.colDepositAmount.ReadOnly = true;
        this.colDepositAmount.Width = 95;

        //
        // colStatus
        //
        this.colStatus.DataPropertyName = "StatusText";
        this.colStatus.HeaderText = "TRẠNG THÁI";
        this.colStatus.Name = "colStatus";
        this.colStatus.ReadOnly = true;
        this.colStatus.Width = 95;

        //
        // colNotes
        //
        this.colNotes.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
        this.colNotes.DataPropertyName = "Notes";
        this.colNotes.HeaderText = "GHI CHÚ";
        this.colNotes.Name = "colNotes";
        this.colNotes.ReadOnly = true;

        //
        // pnlRowButtons — .rowbn dưới bảng: Gia hạn / Chấm dứt
        //
        this.pnlRowButtons.BackColor = System.Drawing.ColorTranslator.FromHtml("#101417");
        this.pnlRowButtons.Controls.Add(this.btnRenew);
        this.pnlRowButtons.Controls.Add(this.btnTerminate);
        this.pnlRowButtons.Dock = System.Windows.Forms.DockStyle.Fill;
        this.pnlRowButtons.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
        this.pnlRowButtons.Name = "pnlRowButtons";
        this.pnlRowButtons.TabIndex = 3;

        //
        // btnRenew
        //
        this.btnRenew.AccessibleName = "Gia hạn hợp đồng đã chọn";
        this.btnRenew.BackColor = System.Drawing.ColorTranslator.FromHtml("#1C2126");
        this.btnRenew.FlatAppearance.BorderColor = System.Drawing.ColorTranslator.FromHtml("#2E373F");
        this.btnRenew.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnRenew.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.btnRenew.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.btnRenew.Location = new System.Drawing.Point(0, 0);
        this.btnRenew.Name = "btnRenew";
        this.btnRenew.Size = new System.Drawing.Size(100, 32);
        this.btnRenew.TabIndex = 0;
        this.btnRenew.Text = "Gia hạn...";
        this.btnRenew.UseVisualStyleBackColor = false;

        //
        // btnTerminate
        //
        this.btnTerminate.AccessibleName = "Chấm dứt hợp đồng đã chọn";
        this.btnTerminate.BackColor = System.Drawing.ColorTranslator.FromHtml("#1F252A");
        this.btnTerminate.FlatAppearance.BorderColor = System.Drawing.ColorTranslator.FromHtml("#3E2925");
        this.btnTerminate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnTerminate.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.btnTerminate.ForeColor = System.Drawing.ColorTranslator.FromHtml("#FFB4AB");
        this.btnTerminate.Location = new System.Drawing.Point(108, 0);
        this.btnTerminate.Name = "btnTerminate";
        this.btnTerminate.Size = new System.Drawing.Size(110, 32);
        this.btnTerminate.TabIndex = 1;
        this.btnTerminate.Text = "Chấm dứt...";
        this.btnTerminate.UseVisualStyleBackColor = false;

        //
        // noteContracts — .note
        //
        this.noteContracts.Dock = System.Windows.Forms.DockStyle.Fill;
        this.noteContracts.Margin = new System.Windows.Forms.Padding(0);
        this.noteContracts.Name = "noteContracts";
        this.noteContracts.TabIndex = 4;

        //
        // ContractsForm
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.ColorTranslator.FromHtml("#101417");
        this.Controls.Add(this.splitMain);
        this.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.Name = "ContractsForm";
        this.Size = new System.Drawing.Size(1000, 600);

        this.splitMain.Panel1.ResumeLayout(false);
        this.splitMain.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
        this.splitMain.ResumeLayout(false);
        this.cardInput.ResumeLayout(false);
        this.cardInput.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.numRentalPrice)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numDepositAmount)).EndInit();
        this.tblRight.ResumeLayout(false);
        this.toolbarContracts.ResumeLayout(false);
        this.toolbarContracts.PerformLayout();
        this.tblKpis.ResumeLayout(false);
        this.cardKpiActive.ResumeLayout(false);
        this.cardKpiExpiring.ResumeLayout(false);
        this.cardKpiDeposit.ResumeLayout(false);
        this.pnlGridWrap.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.dgvContracts)).EndInit();
        this.pnlRowButtons.ResumeLayout(false);
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.SplitContainer splitMain;
    private System.Windows.Forms.Panel cardInput;
    private System.Windows.Forms.Label lblTitleInput;
    private System.Windows.Forms.Label lblRoom;
    private System.Windows.Forms.ComboBox cboRoom;
    private System.Windows.Forms.Label lblRepresentative;
    private System.Windows.Forms.ComboBox cboRepresentative;
    private System.Windows.Forms.Label lblStartDate;
    private System.Windows.Forms.DateTimePicker dtpStartDate;
    private System.Windows.Forms.Label lblEndDate;
    private System.Windows.Forms.DateTimePicker dtpEndDate;
    private System.Windows.Forms.Label lblRentalPrice;
    private System.Windows.Forms.NumericUpDown numRentalPrice;
    private System.Windows.Forms.Label lblDepositAmount;
    private System.Windows.Forms.NumericUpDown numDepositAmount;
    private System.Windows.Forms.Label lblNotes;
    private System.Windows.Forms.TextBox txtNotes;
    private System.Windows.Forms.Button btnCreate;
    private System.Windows.Forms.TableLayoutPanel tblRight;
    private QuanLyTro.Forms.ToolbarPanel toolbarContracts;
    private System.Windows.Forms.TextBox txtSearch;
    private System.Windows.Forms.ComboBox cboFilter;
    private System.Windows.Forms.Button btnRefresh;
    private System.Windows.Forms.TableLayoutPanel tblKpis;
    private QuanLyTro.Forms.ContractsForm.KpiCard cardKpiActive;
    private System.Windows.Forms.Label lblKpiActiveCaption;
    private System.Windows.Forms.Label lblKpiActive;
    private QuanLyTro.Forms.ContractsForm.KpiCard cardKpiExpiring;
    private System.Windows.Forms.Label lblKpiExpiringCaption;
    private System.Windows.Forms.Label lblKpiExpiring;
    private QuanLyTro.Forms.ContractsForm.KpiCard cardKpiDeposit;
    private System.Windows.Forms.Label lblKpiDepositCaption;
    private System.Windows.Forms.Label lblKpiDeposit;
    private System.Windows.Forms.Panel pnlGridWrap;
    private System.Windows.Forms.DataGridView dgvContracts;
    private System.Windows.Forms.DataGridViewTextBoxColumn colId;
    private System.Windows.Forms.DataGridViewTextBoxColumn colRoom;
    private System.Windows.Forms.DataGridViewTextBoxColumn colRepresentative;
    private System.Windows.Forms.DataGridViewTextBoxColumn colStartDate;
    private System.Windows.Forms.DataGridViewTextBoxColumn colEndDate;
    private System.Windows.Forms.DataGridViewTextBoxColumn colRemainingDays;
    private System.Windows.Forms.DataGridViewTextBoxColumn colRentalPrice;
    private System.Windows.Forms.DataGridViewTextBoxColumn colDepositAmount;
    private System.Windows.Forms.DataGridViewTextBoxColumn colStatus;
    private System.Windows.Forms.DataGridViewTextBoxColumn colNotes;
    private QuanLyTro.Forms.TblFootPanel tblFoot;
    private System.Windows.Forms.Panel pnlRowButtons;
    private System.Windows.Forms.Button btnRenew;
    private System.Windows.Forms.Button btnTerminate;
    private System.Windows.Forms.Panel noteContracts;
}
