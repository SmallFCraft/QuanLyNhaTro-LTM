#nullable enable

namespace QuanLyTro.Forms;

partial class UtilitiesForm
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
        this.pnlMain = new System.Windows.Forms.Panel();
        this.toolbarUtils = new QuanLyTro.Forms.ToolbarPanel();
        this.lblRoom = new System.Windows.Forms.Label();
        this.cboRoom = new System.Windows.Forms.ComboBox();
        this.lblMonth = new System.Windows.Forms.Label();
        this.dtpMonth = new System.Windows.Forms.DateTimePicker();
        this.tagReady = new QuanLyTro.Forms.ScreenTheme.TagLabel();
        this.cardCalc = new QuanLyTro.Forms.ScreenTheme.CardPanel();
        this.tblElecRow = new System.Windows.Forms.TableLayoutPanel();
        this.lblElecCaption = new System.Windows.Forms.Label();
        this.lblElecOldCaption = new System.Windows.Forms.Label();
        this.numElecOld = new System.Windows.Forms.NumericUpDown();
        this.lblElecNewCaption = new System.Windows.Forms.Label();
        this.numElecNew = new System.Windows.Forms.NumericUpDown();
        this.lblElecRateCaption = new System.Windows.Forms.Label();
        this.numElecRate = new System.Windows.Forms.NumericUpDown();
        this.lblElecTotal = new System.Windows.Forms.Label();
        this.tblWaterRow = new System.Windows.Forms.TableLayoutPanel();
        this.lblWaterCaption = new System.Windows.Forms.Label();
        this.lblWaterOldCaption = new System.Windows.Forms.Label();
        this.numWaterOld = new System.Windows.Forms.NumericUpDown();
        this.lblWaterNewCaption = new System.Windows.Forms.Label();
        this.numWaterNew = new System.Windows.Forms.NumericUpDown();
        this.lblWaterRateCaption = new System.Windows.Forms.Label();
        this.numWaterRate = new System.Windows.Forms.NumericUpDown();
        this.lblWaterTotal = new System.Windows.Forms.Label();
        this.pnlBottom = new System.Windows.Forms.Panel();
        this.noteUtils = new System.Windows.Forms.Panel();
        this.lblStatusMsg = new System.Windows.Forms.Label();
        this.pnlRowButtons = new System.Windows.Forms.Panel();
        this.btnRecord = new System.Windows.Forms.Button();
        this.pnlMain.SuspendLayout();
        this.toolbarUtils.SuspendLayout();
        this.cardCalc.SuspendLayout();
        this.tblElecRow.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.numElecOld)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numElecNew)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numElecRate)).BeginInit();
        this.tblWaterRow.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.numWaterOld)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numWaterNew)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numWaterRate)).BeginInit();
        this.pnlBottom.SuspendLayout();
        this.pnlRowButtons.SuspendLayout();
        this.SuspendLayout();

        //
        // pnlMain — thứ tự Add quyết định layout dock: Fill trước, rồi từ ngoài vào trong.
        //
        this.pnlMain.BackColor = System.Drawing.ColorTranslator.FromHtml("#101417");
        this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
        this.pnlMain.Controls.Add(this.pnlBottom);
        this.pnlMain.Controls.Add(this.cardCalc);
        this.pnlMain.Controls.Add(this.toolbarUtils);
        this.pnlMain.Location = new System.Drawing.Point(0, 0);
        this.pnlMain.Name = "pnlMain";
        this.pnlMain.Padding = new System.Windows.Forms.Padding(12);
        this.pnlMain.TabIndex = 0;

        //
        // toolbarUtils — .toolbar: Phòng + Tháng + tag trạng thái (template #tab-utils)
        //
        this.toolbarUtils.Controls.Add(this.lblRoom);
        this.toolbarUtils.Controls.Add(this.cboRoom);
        this.toolbarUtils.Controls.Add(this.lblMonth);
        this.toolbarUtils.Controls.Add(this.dtpMonth);
        this.toolbarUtils.Controls.Add(this.tagReady);
        this.toolbarUtils.Dock = System.Windows.Forms.DockStyle.Top;
        this.toolbarUtils.Location = new System.Drawing.Point(12, 12);
        this.toolbarUtils.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
        this.toolbarUtils.Name = "toolbarUtils";
        this.toolbarUtils.Size = new System.Drawing.Size(976, 56);
        this.toolbarUtils.TabIndex = 0;

        //
        // lblRoom
        //
        this.lblRoom.AutoSize = true;
        this.lblRoom.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblRoom.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblRoom.Location = new System.Drawing.Point(12, 19);
        this.lblRoom.Name = "lblRoom";
        this.lblRoom.Size = new System.Drawing.Size(41, 15);
        this.lblRoom.TabIndex = 0;
        this.lblRoom.Text = "Phòng";

        //
        // cboRoom
        //
        this.cboRoom.AccessibleName = "Chọn phòng chốt điện nước";
        this.cboRoom.BackColor = System.Drawing.ColorTranslator.FromHtml("#15191D");
        this.cboRoom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboRoom.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.cboRoom.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.cboRoom.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.cboRoom.FormattingEnabled = true;
        this.cboRoom.ItemHeight = 20;
        this.cboRoom.Location = new System.Drawing.Point(58, 13);
        this.cboRoom.Name = "cboRoom";
        this.cboRoom.Size = new System.Drawing.Size(180, 28);
        this.cboRoom.TabIndex = 1;

        //
        // lblMonth
        //
        this.lblMonth.AutoSize = true;
        this.lblMonth.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblMonth.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblMonth.Location = new System.Drawing.Point(254, 19);
        this.lblMonth.Name = "lblMonth";
        this.lblMonth.Size = new System.Drawing.Size(41, 15);
        this.lblMonth.TabIndex = 2;
        this.lblMonth.Text = "Tháng";

        //
        // dtpMonth
        //
        this.dtpMonth.AccessibleName = "Tháng chốt điện nước";
        this.dtpMonth.CalendarForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.dtpMonth.CalendarMonthBackground = System.Drawing.ColorTranslator.FromHtml("#181C1F");
        this.dtpMonth.CustomFormat = "MM/yyyy";
        this.dtpMonth.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.dtpMonth.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
        this.dtpMonth.Location = new System.Drawing.Point(301, 15);
        this.dtpMonth.Name = "dtpMonth";
        this.dtpMonth.Size = new System.Drawing.Size(112, 23);
        this.dtpMonth.TabIndex = 3;

        //
        // tagReady — .tag.rent: Sage trên nền SageBg, viền SageBorder
        //
        this.tagReady.AccessibleName = "Trạng thái sẵn sàng chốt chỉ số";
        this.tagReady.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.tagReady.BackColor = QuanLyTro.Forms.ScreenTheme.From(QuanLyTro.Forms.ScreenTheme.ToolbarBg);
        this.tagReady.Location = new System.Drawing.Point(864, 15);
        this.tagReady.Name = "tagReady";
        this.tagReady.SetRole(QuanLyTro.Forms.ScreenTheme.Sage, QuanLyTro.Forms.ScreenTheme.SageBg, QuanLyTro.Forms.ScreenTheme.SageBorder);
        this.tagReady.Size = new System.Drawing.Size(100, 22);
        this.tagReady.TabIndex = 4;
        this.tagReady.Text = "Sẵn sàng chốt";

        //
        // cardCalc — .calc: card bo 8px nền #181C1F, 2 hàng ngang phân cách #21272C
        //
        this.cardCalc.BorderHex = QuanLyTro.Forms.ScreenTheme.Hairline;
        this.cardCalc.Controls.Add(this.tblWaterRow);
        this.cardCalc.Controls.Add(this.tblElecRow);
        this.cardCalc.Dock = System.Windows.Forms.DockStyle.Top;
        this.cardCalc.FillHex = QuanLyTro.Forms.ScreenTheme.Card;
        this.cardCalc.Location = new System.Drawing.Point(12, 68);
        this.cardCalc.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
        this.cardCalc.Name = "cardCalc";
        this.cardCalc.Padding = new System.Windows.Forms.Padding(14, 11, 14, 11);
        this.cardCalc.Radius = 8;
        this.cardCalc.Size = new System.Drawing.Size(976, 90);
        this.cardCalc.TabIndex = 1;

        //
        // tblElecRow — hàng Điện: nhãn + cũ + ô CŨ (ReadOnly) + mới + ô MỚI + × giá + ĐƠN GIÁ + tổng phải
        //
        this.tblElecRow.ColumnCount = 8;
        this.tblElecRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 64F));
        this.tblElecRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 28F));
        this.tblElecRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 86F));
        this.tblElecRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 32F));
        this.tblElecRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 86F));
        this.tblElecRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
        this.tblElecRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 96F));
        this.tblElecRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tblElecRow.Controls.Add(this.lblElecCaption, 0, 0);
        this.tblElecRow.Controls.Add(this.lblElecOldCaption, 1, 0);
        this.tblElecRow.Controls.Add(this.numElecOld, 2, 0);
        this.tblElecRow.Controls.Add(this.lblElecNewCaption, 3, 0);
        this.tblElecRow.Controls.Add(this.numElecNew, 4, 0);
        this.tblElecRow.Controls.Add(this.lblElecRateCaption, 5, 0);
        this.tblElecRow.Controls.Add(this.numElecRate, 6, 0);
        this.tblElecRow.Controls.Add(this.lblElecTotal, 7, 0);
        this.tblElecRow.Dock = System.Windows.Forms.DockStyle.Top;
        this.tblElecRow.Location = new System.Drawing.Point(14, 11);
        this.tblElecRow.Margin = new System.Windows.Forms.Padding(0);
        this.tblElecRow.Name = "tblElecRow";
        this.tblElecRow.Paint += new System.Windows.Forms.PaintEventHandler(this.RowRule_Paint);
        this.tblElecRow.RowCount = 1;
        this.tblElecRow.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
        this.tblElecRow.Size = new System.Drawing.Size(948, 45);
        this.tblElecRow.TabIndex = 0;

        this.lblElecCaption.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.lblElecCaption.AutoSize = true;
        this.lblElecCaption.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblElecCaption.ForeColor = System.Drawing.ColorTranslator.FromHtml("#E0AF68");
        this.lblElecCaption.Name = "lblElecCaption";
        this.lblElecCaption.Text = "Điện";

        this.lblElecOldCaption.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.lblElecOldCaption.AutoSize = true;
        this.lblElecOldCaption.Font = new System.Drawing.Font("Segoe UI", 8.25F);
        this.lblElecOldCaption.ForeColor = System.Drawing.ColorTranslator.FromHtml("#767E88");
        this.lblElecOldCaption.Name = "lblElecOldCaption";
        this.lblElecOldCaption.Text = "cũ";

        this.numElecOld.AccessibleName = "Chỉ số điện cũ (chỉ đọc)";
        this.numElecOld.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.numElecOld.BackColor = System.Drawing.ColorTranslator.FromHtml("#12161A");
        this.numElecOld.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.numElecOld.Font = new System.Drawing.Font("Consolas", 9.5F);
        this.numElecOld.ForeColor = System.Drawing.ColorTranslator.FromHtml("#767E88");
        this.numElecOld.Location = new System.Drawing.Point(95, 11);
        this.numElecOld.Maximum = new decimal(new int[] { 9999999, 0, 0, 0 });
        this.numElecOld.Name = "numElecOld";
        this.numElecOld.ReadOnly = true;
        this.numElecOld.Size = new System.Drawing.Size(80, 22);
        this.numElecOld.TabIndex = 0;
        this.numElecOld.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;

        this.lblElecNewCaption.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.lblElecNewCaption.AutoSize = true;
        this.lblElecNewCaption.Font = new System.Drawing.Font("Segoe UI", 8.25F);
        this.lblElecNewCaption.ForeColor = System.Drawing.ColorTranslator.FromHtml("#767E88");
        this.lblElecNewCaption.Name = "lblElecNewCaption";
        this.lblElecNewCaption.Text = "mới";

        this.numElecNew.AccessibleName = "Chỉ số điện mới";
        this.numElecNew.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.numElecNew.BackColor = System.Drawing.ColorTranslator.FromHtml("#15191D");
        this.numElecNew.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.numElecNew.Font = new System.Drawing.Font("Consolas", 9.5F);
        this.numElecNew.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.numElecNew.Location = new System.Drawing.Point(213, 11);
        this.numElecNew.Maximum = new decimal(new int[] { 9999999, 0, 0, 0 });
        this.numElecNew.Name = "numElecNew";
        this.numElecNew.Size = new System.Drawing.Size(80, 22);
        this.numElecNew.TabIndex = 1;
        this.numElecNew.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;

        this.lblElecRateCaption.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.lblElecRateCaption.AutoSize = true;
        this.lblElecRateCaption.Font = new System.Drawing.Font("Segoe UI", 8.25F);
        this.lblElecRateCaption.ForeColor = System.Drawing.ColorTranslator.FromHtml("#767E88");
        this.lblElecRateCaption.Name = "lblElecRateCaption";
        this.lblElecRateCaption.Text = "× giá";

        this.numElecRate.AccessibleName = "Đơn giá điện";
        this.numElecRate.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.numElecRate.BackColor = System.Drawing.ColorTranslator.FromHtml("#15191D");
        this.numElecRate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.numElecRate.Font = new System.Drawing.Font("Consolas", 9.5F);
        this.numElecRate.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.numElecRate.Increment = new decimal(new int[] { 100, 0, 0, 0 });
        this.numElecRate.Location = new System.Drawing.Point(349, 11);
        this.numElecRate.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
        this.numElecRate.Name = "numElecRate";
        this.numElecRate.Size = new System.Drawing.Size(90, 22);
        this.numElecRate.TabIndex = 2;
        this.numElecRate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.numElecRate.ThousandsSeparator = true;
        this.numElecRate.Value = new decimal(new int[] { 3500, 0, 0, 0 });

        this.lblElecTotal.AccessibleName = "Thành tiền điện tạm tính";
        this.lblElecTotal.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblElecTotal.Font = new System.Drawing.Font("Consolas", 10F, System.Drawing.FontStyle.Bold);
        this.lblElecTotal.ForeColor = System.Drawing.ColorTranslator.FromHtml("#FFB5A0");
        this.lblElecTotal.Name = "lblElecTotal";
        this.lblElecTotal.Size = new System.Drawing.Size(420, 44);
        this.lblElecTotal.TabIndex = 3;
        this.lblElecTotal.Text = "0 đ";
        this.lblElecTotal.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

        //
        // tblWaterRow — hàng Nước, cùng khuôn hàng Điện
        //
        this.tblWaterRow.ColumnCount = 8;
        this.tblWaterRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 64F));
        this.tblWaterRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 28F));
        this.tblWaterRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 86F));
        this.tblWaterRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 32F));
        this.tblWaterRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 86F));
        this.tblWaterRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
        this.tblWaterRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 96F));
        this.tblWaterRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tblWaterRow.Controls.Add(this.lblWaterCaption, 0, 0);
        this.tblWaterRow.Controls.Add(this.lblWaterOldCaption, 1, 0);
        this.tblWaterRow.Controls.Add(this.numWaterOld, 2, 0);
        this.tblWaterRow.Controls.Add(this.lblWaterNewCaption, 3, 0);
        this.tblWaterRow.Controls.Add(this.numWaterNew, 4, 0);
        this.tblWaterRow.Controls.Add(this.lblWaterRateCaption, 5, 0);
        this.tblWaterRow.Controls.Add(this.numWaterRate, 6, 0);
        this.tblWaterRow.Controls.Add(this.lblWaterTotal, 7, 0);
        this.tblWaterRow.Dock = System.Windows.Forms.DockStyle.Top;
        this.tblWaterRow.Location = new System.Drawing.Point(14, 56);
        this.tblWaterRow.Margin = new System.Windows.Forms.Padding(0);
        this.tblWaterRow.Name = "tblWaterRow";
        this.tblWaterRow.RowCount = 1;
        this.tblWaterRow.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
        this.tblWaterRow.Size = new System.Drawing.Size(948, 45);
        this.tblWaterRow.TabIndex = 1;

        this.lblWaterCaption.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.lblWaterCaption.AutoSize = true;
        this.lblWaterCaption.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblWaterCaption.ForeColor = System.Drawing.ColorTranslator.FromHtml("#767E88");
        this.lblWaterCaption.Name = "lblWaterCaption";
        this.lblWaterCaption.Text = "Nước";

        this.lblWaterOldCaption.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.lblWaterOldCaption.AutoSize = true;
        this.lblWaterOldCaption.Font = new System.Drawing.Font("Segoe UI", 8.25F);
        this.lblWaterOldCaption.ForeColor = System.Drawing.ColorTranslator.FromHtml("#767E88");
        this.lblWaterOldCaption.Name = "lblWaterOldCaption";
        this.lblWaterOldCaption.Text = "cũ";

        this.numWaterOld.AccessibleName = "Chỉ số nước cũ (chỉ đọc)";
        this.numWaterOld.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.numWaterOld.BackColor = System.Drawing.ColorTranslator.FromHtml("#12161A");
        this.numWaterOld.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.numWaterOld.Font = new System.Drawing.Font("Consolas", 9.5F);
        this.numWaterOld.ForeColor = System.Drawing.ColorTranslator.FromHtml("#767E88");
        this.numWaterOld.Location = new System.Drawing.Point(95, 11);
        this.numWaterOld.Maximum = new decimal(new int[] { 9999999, 0, 0, 0 });
        this.numWaterOld.Name = "numWaterOld";
        this.numWaterOld.ReadOnly = true;
        this.numWaterOld.Size = new System.Drawing.Size(80, 22);
        this.numWaterOld.TabIndex = 0;
        this.numWaterOld.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;

        this.lblWaterNewCaption.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.lblWaterNewCaption.AutoSize = true;
        this.lblWaterNewCaption.Font = new System.Drawing.Font("Segoe UI", 8.25F);
        this.lblWaterNewCaption.ForeColor = System.Drawing.ColorTranslator.FromHtml("#767E88");
        this.lblWaterNewCaption.Name = "lblWaterNewCaption";
        this.lblWaterNewCaption.Text = "mới";

        this.numWaterNew.AccessibleName = "Chỉ số nước mới";
        this.numWaterNew.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.numWaterNew.BackColor = System.Drawing.ColorTranslator.FromHtml("#15191D");
        this.numWaterNew.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.numWaterNew.Font = new System.Drawing.Font("Consolas", 9.5F);
        this.numWaterNew.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.numWaterNew.Location = new System.Drawing.Point(213, 11);
        this.numWaterNew.Maximum = new decimal(new int[] { 9999999, 0, 0, 0 });
        this.numWaterNew.Name = "numWaterNew";
        this.numWaterNew.Size = new System.Drawing.Size(80, 22);
        this.numWaterNew.TabIndex = 1;
        this.numWaterNew.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;

        this.lblWaterRateCaption.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.lblWaterRateCaption.AutoSize = true;
        this.lblWaterRateCaption.Font = new System.Drawing.Font("Segoe UI", 8.25F);
        this.lblWaterRateCaption.ForeColor = System.Drawing.ColorTranslator.FromHtml("#767E88");
        this.lblWaterRateCaption.Name = "lblWaterRateCaption";
        this.lblWaterRateCaption.Text = "× giá";

        this.numWaterRate.AccessibleName = "Đơn giá nước";
        this.numWaterRate.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.numWaterRate.BackColor = System.Drawing.ColorTranslator.FromHtml("#15191D");
        this.numWaterRate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.numWaterRate.Font = new System.Drawing.Font("Consolas", 9.5F);
        this.numWaterRate.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.numWaterRate.Increment = new decimal(new int[] { 500, 0, 0, 0 });
        this.numWaterRate.Location = new System.Drawing.Point(349, 11);
        this.numWaterRate.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
        this.numWaterRate.Name = "numWaterRate";
        this.numWaterRate.Size = new System.Drawing.Size(90, 22);
        this.numWaterRate.TabIndex = 2;
        this.numWaterRate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.numWaterRate.ThousandsSeparator = true;
        this.numWaterRate.Value = new decimal(new int[] { 15000, 0, 0, 0 });

        this.lblWaterTotal.AccessibleName = "Thành tiền nước tạm tính";
        this.lblWaterTotal.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblWaterTotal.Font = new System.Drawing.Font("Consolas", 10F, System.Drawing.FontStyle.Bold);
        this.lblWaterTotal.ForeColor = System.Drawing.ColorTranslator.FromHtml("#FFB5A0");
        this.lblWaterTotal.Name = "lblWaterTotal";
        this.lblWaterTotal.Size = new System.Drawing.Size(420, 44);
        this.lblWaterTotal.TabIndex = 3;
        this.lblWaterTotal.Text = "0 đ";
        this.lblWaterTotal.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

        //
        // pnlBottom — .rowbn (Lưu chỉ số) rồi .note
        //
        this.pnlBottom.BackColor = System.Drawing.ColorTranslator.FromHtml("#101417");
        this.pnlBottom.Controls.Add(this.noteUtils);
        this.pnlBottom.Controls.Add(this.lblStatusMsg);
        this.pnlBottom.Controls.Add(this.pnlRowButtons);
        this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.pnlBottom.Location = new System.Drawing.Point(12, 446);
        this.pnlBottom.Name = "pnlBottom";
        this.pnlBottom.Size = new System.Drawing.Size(976, 142);
        this.pnlBottom.TabIndex = 2;

        //
        // pnlRowButtons
        //
        this.pnlRowButtons.BackColor = System.Drawing.ColorTranslator.FromHtml("#101417");
        this.pnlRowButtons.Controls.Add(this.btnRecord);
        this.pnlRowButtons.Dock = System.Windows.Forms.DockStyle.Top;
        this.pnlRowButtons.Location = new System.Drawing.Point(0, 0);
        this.pnlRowButtons.Name = "pnlRowButtons";
        this.pnlRowButtons.Size = new System.Drawing.Size(976, 44);
        this.pnlRowButtons.TabIndex = 0;

        //
        // btnRecord — giữ nguyên nhãn nghiệp vụ hiện có
        //
        this.btnRecord.AccessibleName = "Lưu chỉ số điện nước";
        this.btnRecord.BackColor = System.Drawing.ColorTranslator.FromHtml("#D95D39");
        this.btnRecord.FlatAppearance.BorderSize = 0;
        this.btnRecord.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnRecord.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.btnRecord.ForeColor = System.Drawing.Color.White;
        this.btnRecord.Location = new System.Drawing.Point(0, 0);
        this.btnRecord.Name = "btnRecord";
        this.btnRecord.Size = new System.Drawing.Size(140, 32);
        this.btnRecord.TabIndex = 0;
        this.btnRecord.Text = "Lưu chỉ số";
        this.btnRecord.UseVisualStyleBackColor = false;

        //
        // lblStatusMsg — thông báo nạp chỉ số cũ / lỗi Server (nguyên văn)
        //
        this.lblStatusMsg.AccessibleName = "Thông báo trạng thái chốt chỉ số";
        this.lblStatusMsg.Dock = System.Windows.Forms.DockStyle.Top;
        this.lblStatusMsg.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblStatusMsg.ForeColor = System.Drawing.ColorTranslator.FromHtml("#CAC6C1");
        this.lblStatusMsg.Location = new System.Drawing.Point(0, 44);
        this.lblStatusMsg.Name = "lblStatusMsg";
        this.lblStatusMsg.Padding = new System.Windows.Forms.Padding(1, 6, 1, 0);
        this.lblStatusMsg.Size = new System.Drawing.Size(976, 30);
        this.lblStatusMsg.TabIndex = 1;

        //
        // noteUtils — .note
        //
        this.noteUtils.Dock = System.Windows.Forms.DockStyle.Fill;
        this.noteUtils.Location = new System.Drawing.Point(0, 74);
        this.noteUtils.Name = "noteUtils";
        this.noteUtils.Size = new System.Drawing.Size(976, 68);
        this.noteUtils.TabIndex = 2;

        //
        // UtilitiesForm
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.AccessibleName = "Màn hình chốt chỉ số điện nước";
        this.BackColor = System.Drawing.ColorTranslator.FromHtml("#101417");
        this.Controls.Add(this.pnlMain);
        this.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.Name = "UtilitiesForm";
        this.Size = new System.Drawing.Size(1000, 600);

        this.pnlMain.ResumeLayout(false);
        this.toolbarUtils.ResumeLayout(false);
        this.toolbarUtils.PerformLayout();
        this.cardCalc.ResumeLayout(false);
        this.tblElecRow.ResumeLayout(false);
        this.tblElecRow.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.numElecOld)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numElecNew)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numElecRate)).EndInit();
        this.tblWaterRow.ResumeLayout(false);
        this.tblWaterRow.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.numWaterOld)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numWaterNew)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numWaterRate)).EndInit();
        this.pnlBottom.ResumeLayout(false);
        this.pnlRowButtons.ResumeLayout(false);
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.Panel pnlMain;
    private QuanLyTro.Forms.ToolbarPanel toolbarUtils;
    private System.Windows.Forms.Label lblRoom;
    private System.Windows.Forms.ComboBox cboRoom;
    private System.Windows.Forms.Label lblMonth;
    private System.Windows.Forms.DateTimePicker dtpMonth;
    private QuanLyTro.Forms.ScreenTheme.TagLabel tagReady;
    private QuanLyTro.Forms.ScreenTheme.CardPanel cardCalc;
    private System.Windows.Forms.TableLayoutPanel tblElecRow;
    private System.Windows.Forms.Label lblElecCaption;
    private System.Windows.Forms.Label lblElecOldCaption;
    private System.Windows.Forms.NumericUpDown numElecOld;
    private System.Windows.Forms.Label lblElecNewCaption;
    private System.Windows.Forms.NumericUpDown numElecNew;
    private System.Windows.Forms.Label lblElecRateCaption;
    private System.Windows.Forms.NumericUpDown numElecRate;
    private System.Windows.Forms.Label lblElecTotal;
    private System.Windows.Forms.TableLayoutPanel tblWaterRow;
    private System.Windows.Forms.Label lblWaterCaption;
    private System.Windows.Forms.Label lblWaterOldCaption;
    private System.Windows.Forms.NumericUpDown numWaterOld;
    private System.Windows.Forms.Label lblWaterNewCaption;
    private System.Windows.Forms.NumericUpDown numWaterNew;
    private System.Windows.Forms.Label lblWaterRateCaption;
    private System.Windows.Forms.NumericUpDown numWaterRate;
    private System.Windows.Forms.Label lblWaterTotal;
    private System.Windows.Forms.Panel pnlBottom;
    private System.Windows.Forms.Panel noteUtils;
    private System.Windows.Forms.Label lblStatusMsg;
    private System.Windows.Forms.Panel pnlRowButtons;
    private System.Windows.Forms.Button btnRecord;
}
