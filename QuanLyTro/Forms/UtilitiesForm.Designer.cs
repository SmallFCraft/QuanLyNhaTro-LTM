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
        this.splitMain = new System.Windows.Forms.SplitContainer();
        this.cardInput = new System.Windows.Forms.Panel();
        this.lblTitle = new System.Windows.Forms.Label();
        this.lblRoom = new System.Windows.Forms.Label();
        this.cboRoom = new System.Windows.Forms.ComboBox();
        this.lblMonth = new System.Windows.Forms.Label();
        this.dtpMonth = new System.Windows.Forms.DateTimePicker();
        this.grpElectricity = new System.Windows.Forms.GroupBox();
        this.lblElecOld = new System.Windows.Forms.Label();
        this.numElecOld = new System.Windows.Forms.NumericUpDown();
        this.lblElecNew = new System.Windows.Forms.Label();
        this.numElecNew = new System.Windows.Forms.NumericUpDown();
        this.lblElecRate = new System.Windows.Forms.Label();
        this.numElecRate = new System.Windows.Forms.NumericUpDown();
        this.grpWater = new System.Windows.Forms.GroupBox();
        this.lblWaterOld = new System.Windows.Forms.Label();
        this.numWaterOld = new System.Windows.Forms.NumericUpDown();
        this.lblWaterNew = new System.Windows.Forms.Label();
        this.numWaterNew = new System.Windows.Forms.NumericUpDown();
        this.lblWaterRate = new System.Windows.Forms.Label();
        this.numWaterRate = new System.Windows.Forms.NumericUpDown();
        this.btnRecord = new System.Windows.Forms.Button();
        this.cardPreview = new System.Windows.Forms.Panel();
        this.lblPreviewHeader = new System.Windows.Forms.Label();
        this.lblPreviewDisclaimer = new System.Windows.Forms.Label();
        this.lblElecUsageLabel = new System.Windows.Forms.Label();
        this.lblElecUsageVal = new System.Windows.Forms.Label();
        this.lblElecEstLabel = new System.Windows.Forms.Label();
        this.lblElecEstVal = new System.Windows.Forms.Label();
        this.lblWaterUsageLabel = new System.Windows.Forms.Label();
        this.lblWaterUsageVal = new System.Windows.Forms.Label();
        this.lblWaterEstLabel = new System.Windows.Forms.Label();
        this.lblWaterEstVal = new System.Windows.Forms.Label();
        this.pnlDivider = new System.Windows.Forms.Panel();
        this.lblTotalEstLabel = new System.Windows.Forms.Label();
        this.lblTotalEstVal = new System.Windows.Forms.Label();
        this.lblStatusMsg = new System.Windows.Forms.Label();
        ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
        this.splitMain.Panel1.SuspendLayout();
        this.splitMain.Panel2.SuspendLayout();
        this.splitMain.SuspendLayout();
        this.cardInput.SuspendLayout();
        this.grpElectricity.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.numElecOld)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numElecNew)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numElecRate)).BeginInit();
        this.grpWater.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.numWaterOld)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numWaterNew)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.numWaterRate)).BeginInit();
        this.cardPreview.SuspendLayout();
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
        this.splitMain.SplitterDistance = 460;
        this.splitMain.TabIndex = 0;

        //
        // splitMain.Panel1
        //
        this.splitMain.Panel1.BackColor = System.Drawing.ColorTranslator.FromHtml("#101417");
        this.splitMain.Panel1.Controls.Add(this.cardInput);
        this.splitMain.Panel1.Padding = new System.Windows.Forms.Padding(12);

        //
        // splitMain.Panel2
        //
        this.splitMain.Panel2.BackColor = System.Drawing.ColorTranslator.FromHtml("#101417");
        this.splitMain.Panel2.Controls.Add(this.cardPreview);
        this.splitMain.Panel2.Padding = new System.Windows.Forms.Padding(0, 12, 12, 12);

        //
        // cardInput
        //
        this.cardInput.AutoScroll = true;
        this.cardInput.BackColor = System.Drawing.ColorTranslator.FromHtml("#181C1F");
        this.cardInput.Controls.Add(this.lblTitle);
        this.cardInput.Controls.Add(this.lblRoom);
        this.cardInput.Controls.Add(this.cboRoom);
        this.cardInput.Controls.Add(this.lblMonth);
        this.cardInput.Controls.Add(this.dtpMonth);
        this.cardInput.Controls.Add(this.grpElectricity);
        this.cardInput.Controls.Add(this.grpWater);
        this.cardInput.Controls.Add(this.btnRecord);
        this.cardInput.Dock = System.Windows.Forms.DockStyle.Fill;
        this.cardInput.Location = new System.Drawing.Point(12, 12);
        this.cardInput.Name = "cardInput";
        this.cardInput.Padding = new System.Windows.Forms.Padding(16);
        this.cardInput.TabIndex = 0;

        //
        // lblTitle
        //
        this.lblTitle.AutoSize = true;
        this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
        this.lblTitle.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.lblTitle.Location = new System.Drawing.Point(16, 14);
        this.lblTitle.Name = "lblTitle";
        this.lblTitle.Size = new System.Drawing.Size(200, 19);
        this.lblTitle.TabIndex = 0;
        this.lblTitle.Text = "CHỐT CHỈ SỐ ĐIỆN / NƯỚC";

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
        this.cboRoom.AccessibleName = "Chọn phòng chốt điện nước";
        this.cboRoom.BackColor = System.Drawing.ColorTranslator.FromHtml("#15191D");
        this.cboRoom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboRoom.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.cboRoom.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.cboRoom.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.cboRoom.FormattingEnabled = true;
        this.cboRoom.Location = new System.Drawing.Point(16, 65);
        this.cboRoom.Name = "cboRoom";
        this.cboRoom.Size = new System.Drawing.Size(240, 28);
        this.cboRoom.TabIndex = 2;

        //
        // lblMonth
        //
        this.lblMonth.AutoSize = true;
        this.lblMonth.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblMonth.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblMonth.Location = new System.Drawing.Point(268, 45);
        this.lblMonth.Name = "lblMonth";
        this.lblMonth.Size = new System.Drawing.Size(92, 15);
        this.lblMonth.TabIndex = 3;
        this.lblMonth.Text = "Tháng chốt (Kỳ)";

        //
        // dtpMonth
        //
        this.dtpMonth.AccessibleName = "Tháng chốt điện nước";
        this.dtpMonth.CalendarForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.dtpMonth.CalendarMonthBackground = System.Drawing.ColorTranslator.FromHtml("#181C1F");
        this.dtpMonth.CustomFormat = "yyyy-MM";
        this.dtpMonth.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.dtpMonth.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
        this.dtpMonth.Location = new System.Drawing.Point(268, 65);
        this.dtpMonth.Name = "dtpMonth";
        this.dtpMonth.Size = new System.Drawing.Size(148, 23);
        this.dtpMonth.TabIndex = 4;

        //
        // grpElectricity
        //
        this.grpElectricity.BackColor = System.Drawing.ColorTranslator.FromHtml("#181C1F");
        this.grpElectricity.Controls.Add(this.lblElecOld);
        this.grpElectricity.Controls.Add(this.numElecOld);
        this.grpElectricity.Controls.Add(this.lblElecNew);
        this.grpElectricity.Controls.Add(this.numElecNew);
        this.grpElectricity.Controls.Add(this.lblElecRate);
        this.grpElectricity.Controls.Add(this.numElecRate);
        this.grpElectricity.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.grpElectricity.ForeColor = System.Drawing.ColorTranslator.FromHtml("#E0AF68");
        this.grpElectricity.Location = new System.Drawing.Point(16, 110);
        this.grpElectricity.Name = "grpElectricity";
        this.grpElectricity.Size = new System.Drawing.Size(400, 150);
        this.grpElectricity.TabIndex = 5;
        this.grpElectricity.TabStop = false;
        this.grpElectricity.Text = "⚡  CHỈ SỐ ĐIỆN (kWh)";

        //
        // lblElecOld
        //
        this.lblElecOld.AutoSize = true;
        this.lblElecOld.Font = new System.Drawing.Font("Segoe UI", 8.5F);
        this.lblElecOld.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblElecOld.Location = new System.Drawing.Point(16, 26);
        this.lblElecOld.Name = "lblElecOld";
        this.lblElecOld.Size = new System.Drawing.Size(130, 15);
        this.lblElecOld.TabIndex = 0;
        this.lblElecOld.Text = "Chỉ số cũ (Kỳ trước)";

        //
        // numElecOld
        //
        this.numElecOld.AccessibleName = "Chỉ số điện cũ";
        this.numElecOld.BackColor = System.Drawing.ColorTranslator.FromHtml("#15191D");
        this.numElecOld.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.numElecOld.Font = new System.Drawing.Font("Consolas", 9.5F);
        this.numElecOld.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.numElecOld.Location = new System.Drawing.Point(16, 46);
        this.numElecOld.Maximum = new decimal(new int[] { 9999999, 0, 0, 0 });
        this.numElecOld.Name = "numElecOld";
        this.numElecOld.Size = new System.Drawing.Size(160, 22);
        this.numElecOld.TabIndex = 1;
        this.numElecOld.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;

        //
        // lblElecNew
        //
        this.lblElecNew.AutoSize = true;
        this.lblElecNew.Font = new System.Drawing.Font("Segoe UI", 8.5F);
        this.lblElecNew.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblElecNew.Location = new System.Drawing.Point(210, 26);
        this.lblElecNew.Name = "lblElecNew";
        this.lblElecNew.Size = new System.Drawing.Size(117, 15);
        this.lblElecNew.TabIndex = 2;
        this.lblElecNew.Text = "Chỉ số mới (Kỳ này)";

        //
        // numElecNew
        //
        this.numElecNew.AccessibleName = "Chỉ số điện mới";
        this.numElecNew.BackColor = System.Drawing.ColorTranslator.FromHtml("#15191D");
        this.numElecNew.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.numElecNew.Font = new System.Drawing.Font("Consolas", 9.5F);
        this.numElecNew.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.numElecNew.Location = new System.Drawing.Point(210, 46);
        this.numElecNew.Maximum = new decimal(new int[] { 9999999, 0, 0, 0 });
        this.numElecNew.Name = "numElecNew";
        this.numElecNew.Size = new System.Drawing.Size(160, 22);
        this.numElecNew.TabIndex = 3;
        this.numElecNew.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;

        //
        // lblElecRate
        //
        this.lblElecRate.AutoSize = true;
        this.lblElecRate.Font = new System.Drawing.Font("Segoe UI", 8.5F);
        this.lblElecRate.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblElecRate.Location = new System.Drawing.Point(16, 84);
        this.lblElecRate.Name = "lblElecRate";
        this.lblElecRate.Size = new System.Drawing.Size(148, 15);
        this.lblElecRate.TabIndex = 4;
        this.lblElecRate.Text = "Đơn giá điện (VNĐ/kWh)";

        //
        // numElecRate
        //
        this.numElecRate.AccessibleName = "Đơn giá điện";
        this.numElecRate.BackColor = System.Drawing.ColorTranslator.FromHtml("#15191D");
        this.numElecRate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.numElecRate.Font = new System.Drawing.Font("Consolas", 9.5F);
        this.numElecRate.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.numElecRate.Increment = new decimal(new int[] { 100, 0, 0, 0 });
        this.numElecRate.Location = new System.Drawing.Point(16, 104);
        this.numElecRate.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
        this.numElecRate.Name = "numElecRate";
        this.numElecRate.Size = new System.Drawing.Size(160, 22);
        this.numElecRate.TabIndex = 5;
        this.numElecRate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.numElecRate.ThousandsSeparator = true;
        this.numElecRate.Value = new decimal(new int[] { 3500, 0, 0, 0 });

        //
        // grpWater
        //
        this.grpWater.BackColor = System.Drawing.ColorTranslator.FromHtml("#181C1F");
        this.grpWater.Controls.Add(this.lblWaterOld);
        this.grpWater.Controls.Add(this.numWaterOld);
        this.grpWater.Controls.Add(this.lblWaterNew);
        this.grpWater.Controls.Add(this.numWaterNew);
        this.grpWater.Controls.Add(this.lblWaterRate);
        this.grpWater.Controls.Add(this.numWaterRate);
        this.grpWater.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.grpWater.ForeColor = System.Drawing.ColorTranslator.FromHtml("#8BD7A3");
        this.grpWater.Location = new System.Drawing.Point(16, 276);
        this.grpWater.Name = "grpWater";
        this.grpWater.Size = new System.Drawing.Size(400, 150);
        this.grpWater.TabIndex = 6;
        this.grpWater.TabStop = false;
        this.grpWater.Text = "💧  CHỈ SỐ NƯỚC (m³)";

        //
        // lblWaterOld
        //
        this.lblWaterOld.AutoSize = true;
        this.lblWaterOld.Font = new System.Drawing.Font("Segoe UI", 8.5F);
        this.lblWaterOld.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblWaterOld.Location = new System.Drawing.Point(16, 26);
        this.lblWaterOld.Name = "lblWaterOld";
        this.lblWaterOld.Size = new System.Drawing.Size(130, 15);
        this.lblWaterOld.TabIndex = 0;
        this.lblWaterOld.Text = "Chỉ số cũ (Kỳ trước)";

        //
        // numWaterOld
        //
        this.numWaterOld.AccessibleName = "Chỉ số nước cũ";
        this.numWaterOld.BackColor = System.Drawing.ColorTranslator.FromHtml("#15191D");
        this.numWaterOld.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.numWaterOld.Font = new System.Drawing.Font("Consolas", 9.5F);
        this.numWaterOld.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.numWaterOld.Location = new System.Drawing.Point(16, 46);
        this.numWaterOld.Maximum = new decimal(new int[] { 9999999, 0, 0, 0 });
        this.numWaterOld.Name = "numWaterOld";
        this.numWaterOld.Size = new System.Drawing.Size(160, 22);
        this.numWaterOld.TabIndex = 1;
        this.numWaterOld.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;

        //
        // lblWaterNew
        //
        this.lblWaterNew.AutoSize = true;
        this.lblWaterNew.Font = new System.Drawing.Font("Segoe UI", 8.5F);
        this.lblWaterNew.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblWaterNew.Location = new System.Drawing.Point(210, 26);
        this.lblWaterNew.Name = "lblWaterNew";
        this.lblWaterNew.Size = new System.Drawing.Size(117, 15);
        this.lblWaterNew.TabIndex = 2;
        this.lblWaterNew.Text = "Chỉ số mới (Kỳ này)";

        //
        // numWaterNew
        //
        this.numWaterNew.AccessibleName = "Chỉ số nước mới";
        this.numWaterNew.BackColor = System.Drawing.ColorTranslator.FromHtml("#15191D");
        this.numWaterNew.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.numWaterNew.Font = new System.Drawing.Font("Consolas", 9.5F);
        this.numWaterNew.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.numWaterNew.Location = new System.Drawing.Point(210, 46);
        this.numWaterNew.Maximum = new decimal(new int[] { 9999999, 0, 0, 0 });
        this.numWaterNew.Name = "numWaterNew";
        this.numWaterNew.Size = new System.Drawing.Size(160, 22);
        this.numWaterNew.TabIndex = 3;
        this.numWaterNew.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;

        //
        // lblWaterRate
        //
        this.lblWaterRate.AutoSize = true;
        this.lblWaterRate.Font = new System.Drawing.Font("Segoe UI", 8.5F);
        this.lblWaterRate.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblWaterRate.Location = new System.Drawing.Point(16, 84);
        this.lblWaterRate.Name = "lblWaterRate";
        this.lblWaterRate.Size = new System.Drawing.Size(142, 15);
        this.lblWaterRate.TabIndex = 4;
        this.lblWaterRate.Text = "Đơn giá nước (VNĐ/m³)";

        //
        // numWaterRate
        //
        this.numWaterRate.AccessibleName = "Đơn giá nước";
        this.numWaterRate.BackColor = System.Drawing.ColorTranslator.FromHtml("#15191D");
        this.numWaterRate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.numWaterRate.Font = new System.Drawing.Font("Consolas", 9.5F);
        this.numWaterRate.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.numWaterRate.Increment = new decimal(new int[] { 500, 0, 0, 0 });
        this.numWaterRate.Location = new System.Drawing.Point(16, 104);
        this.numWaterRate.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
        this.numWaterRate.Name = "numWaterRate";
        this.numWaterRate.Size = new System.Drawing.Size(160, 22);
        this.numWaterRate.TabIndex = 5;
        this.numWaterRate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        this.numWaterRate.ThousandsSeparator = true;
        this.numWaterRate.Value = new decimal(new int[] { 15000, 0, 0, 0 });

        //
        // btnRecord
        //
        this.btnRecord.AccessibleName = "Ghi chỉ số điện nước";
        this.btnRecord.BackColor = System.Drawing.ColorTranslator.FromHtml("#D95D39");
        this.btnRecord.FlatAppearance.BorderSize = 0;
        this.btnRecord.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnRecord.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.btnRecord.ForeColor = System.Drawing.Color.White;
        this.btnRecord.Location = new System.Drawing.Point(16, 442);
        this.btnRecord.Name = "btnRecord";
        this.btnRecord.Size = new System.Drawing.Size(400, 36);
        this.btnRecord.TabIndex = 7;
        this.btnRecord.Text = "Ghi Chỉ Số & Chốt Tháng";
        this.btnRecord.UseVisualStyleBackColor = false;

        //
        // cardPreview
        //
        this.cardPreview.BackColor = System.Drawing.ColorTranslator.FromHtml("#181C1F");
        this.cardPreview.Controls.Add(this.lblPreviewHeader);
        this.cardPreview.Controls.Add(this.lblPreviewDisclaimer);
        this.cardPreview.Controls.Add(this.lblElecUsageLabel);
        this.cardPreview.Controls.Add(this.lblElecUsageVal);
        this.cardPreview.Controls.Add(this.lblElecEstLabel);
        this.cardPreview.Controls.Add(this.lblElecEstVal);
        this.cardPreview.Controls.Add(this.lblWaterUsageLabel);
        this.cardPreview.Controls.Add(this.lblWaterUsageVal);
        this.cardPreview.Controls.Add(this.lblWaterEstLabel);
        this.cardPreview.Controls.Add(this.lblWaterEstVal);
        this.cardPreview.Controls.Add(this.pnlDivider);
        this.cardPreview.Controls.Add(this.lblTotalEstLabel);
        this.cardPreview.Controls.Add(this.lblTotalEstVal);
        this.cardPreview.Controls.Add(this.lblStatusMsg);
        this.cardPreview.Dock = System.Windows.Forms.DockStyle.Fill;
        this.cardPreview.Location = new System.Drawing.Point(0, 12);
        this.cardPreview.Name = "cardPreview";
        this.cardPreview.Padding = new System.Windows.Forms.Padding(20);
        this.cardPreview.TabIndex = 0;

        //
        // lblPreviewHeader
        //
        this.lblPreviewHeader.AutoSize = true;
        this.lblPreviewHeader.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
        this.lblPreviewHeader.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.lblPreviewHeader.Location = new System.Drawing.Point(20, 18);
        this.lblPreviewHeader.Name = "lblPreviewHeader";
        this.lblPreviewHeader.Size = new System.Drawing.Size(200, 19);
        this.lblPreviewHeader.TabIndex = 0;
        this.lblPreviewHeader.Text = "XEM TRƯỚC TIỀN TÍNH TOÁN";

        //
        // lblPreviewDisclaimer
        //
        this.lblPreviewDisclaimer.AutoSize = true;
        this.lblPreviewDisclaimer.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Italic);
        this.lblPreviewDisclaimer.ForeColor = System.Drawing.ColorTranslator.FromHtml("#767E88");
        this.lblPreviewDisclaimer.Location = new System.Drawing.Point(20, 42);
        this.lblPreviewDisclaimer.Name = "lblPreviewDisclaimer";
        this.lblPreviewDisclaimer.Size = new System.Drawing.Size(292, 13);
        this.lblPreviewDisclaimer.TabIndex = 1;
        this.lblPreviewDisclaimer.Text = "(Ước tính trải nghiệm UX; số tiền chính thức do máy chủ quyết định)";

        //
        // lblElecUsageLabel
        //
        this.lblElecUsageLabel.AutoSize = true;
        this.lblElecUsageLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblElecUsageLabel.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblElecUsageLabel.Location = new System.Drawing.Point(20, 80);
        this.lblElecUsageLabel.Name = "lblElecUsageLabel";
        this.lblElecUsageLabel.Size = new System.Drawing.Size(126, 15);
        this.lblElecUsageLabel.TabIndex = 2;
        this.lblElecUsageLabel.Text = "Điện năng tiêu thụ:";

        //
        // lblElecUsageVal
        //
        this.lblElecUsageVal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.lblElecUsageVal.Font = new System.Drawing.Font("Consolas", 10F, System.Drawing.FontStyle.Bold);
        this.lblElecUsageVal.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.lblElecUsageVal.Location = new System.Drawing.Point(200, 78);
        this.lblElecUsageVal.Name = "lblElecUsageVal";
        this.lblElecUsageVal.Size = new System.Drawing.Size(280, 20);
        this.lblElecUsageVal.TabIndex = 3;
        this.lblElecUsageVal.Text = "0 kWh";
        this.lblElecUsageVal.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

        //
        // lblElecEstLabel
        //
        this.lblElecEstLabel.AutoSize = true;
        this.lblElecEstLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblElecEstLabel.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblElecEstLabel.Location = new System.Drawing.Point(20, 110);
        this.lblElecEstLabel.Name = "lblElecEstLabel";
        this.lblElecEstLabel.Size = new System.Drawing.Size(107, 15);
        this.lblElecEstLabel.TabIndex = 4;
        this.lblElecEstLabel.Text = "Tiền điện tạm tính:";

        //
        // lblElecEstVal
        //
        this.lblElecEstVal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.lblElecEstVal.Font = new System.Drawing.Font("Consolas", 10F, System.Drawing.FontStyle.Bold);
        this.lblElecEstVal.ForeColor = System.Drawing.ColorTranslator.FromHtml("#E0AF68");
        this.lblElecEstVal.Location = new System.Drawing.Point(200, 108);
        this.lblElecEstVal.Name = "lblElecEstVal";
        this.lblElecEstVal.Size = new System.Drawing.Size(280, 20);
        this.lblElecEstVal.TabIndex = 5;
        this.lblElecEstVal.Text = "0 VNĐ";
        this.lblElecEstVal.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

        //
        // lblWaterUsageLabel
        //
        this.lblWaterUsageLabel.AutoSize = true;
        this.lblWaterUsageLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblWaterUsageLabel.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblWaterUsageLabel.Location = new System.Drawing.Point(20, 150);
        this.lblWaterUsageLabel.Name = "lblWaterUsageLabel";
        this.lblWaterUsageLabel.Size = new System.Drawing.Size(126, 15);
        this.lblWaterUsageLabel.TabIndex = 6;
        this.lblWaterUsageLabel.Text = "Lượng nước tiêu thụ:";

        //
        // lblWaterUsageVal
        //
        this.lblWaterUsageVal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.lblWaterUsageVal.Font = new System.Drawing.Font("Consolas", 10F, System.Drawing.FontStyle.Bold);
        this.lblWaterUsageVal.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.lblWaterUsageVal.Location = new System.Drawing.Point(200, 148);
        this.lblWaterUsageVal.Name = "lblWaterUsageVal";
        this.lblWaterUsageVal.Size = new System.Drawing.Size(280, 20);
        this.lblWaterUsageVal.TabIndex = 7;
        this.lblWaterUsageVal.Text = "0 m³";
        this.lblWaterUsageVal.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

        //
        // lblWaterEstLabel
        //
        this.lblWaterEstLabel.AutoSize = true;
        this.lblWaterEstLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblWaterEstLabel.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblWaterEstLabel.Location = new System.Drawing.Point(20, 180);
        this.lblWaterEstLabel.Name = "lblWaterEstLabel";
        this.lblWaterEstLabel.Size = new System.Drawing.Size(113, 15);
        this.lblWaterEstLabel.TabIndex = 8;
        this.lblWaterEstLabel.Text = "Tiền nước tạm tính:";

        //
        // lblWaterEstVal
        //
        this.lblWaterEstVal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.lblWaterEstVal.Font = new System.Drawing.Font("Consolas", 10F, System.Drawing.FontStyle.Bold);
        this.lblWaterEstVal.ForeColor = System.Drawing.ColorTranslator.FromHtml("#8BD7A3");
        this.lblWaterEstVal.Location = new System.Drawing.Point(200, 178);
        this.lblWaterEstVal.Name = "lblWaterEstVal";
        this.lblWaterEstVal.Size = new System.Drawing.Size(280, 20);
        this.lblWaterEstVal.TabIndex = 9;
        this.lblWaterEstVal.Text = "0 VNĐ";
        this.lblWaterEstVal.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

        //
        // pnlDivider
        //
        this.pnlDivider.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
        this.pnlDivider.BackColor = System.Drawing.ColorTranslator.FromHtml("#2B333E");
        this.pnlDivider.Location = new System.Drawing.Point(20, 220);
        this.pnlDivider.Name = "pnlDivider";
        this.pnlDivider.Size = new System.Drawing.Size(460, 2);
        this.pnlDivider.TabIndex = 10;

        //
        // lblTotalEstLabel
        //
        this.lblTotalEstLabel.AutoSize = true;
        this.lblTotalEstLabel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
        this.lblTotalEstLabel.ForeColor = System.Drawing.ColorTranslator.FromHtml("#F4EFEA");
        this.lblTotalEstLabel.Location = new System.Drawing.Point(20, 238);
        this.lblTotalEstLabel.Name = "lblTotalEstLabel";
        this.lblTotalEstLabel.Size = new System.Drawing.Size(149, 19);
        this.lblTotalEstLabel.TabIndex = 11;
        this.lblTotalEstLabel.Text = "TỔNG TIỀN DỊCH VỤ:";

        //
        // lblTotalEstVal
        //
        this.lblTotalEstVal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.lblTotalEstVal.Font = new System.Drawing.Font("Consolas", 14F, System.Drawing.FontStyle.Bold);
        this.lblTotalEstVal.ForeColor = System.Drawing.ColorTranslator.FromHtml("#D95D39");
        this.lblTotalEstVal.Location = new System.Drawing.Point(200, 234);
        this.lblTotalEstVal.Name = "lblTotalEstVal";
        this.lblTotalEstVal.Size = new System.Drawing.Size(280, 28);
        this.lblTotalEstVal.TabIndex = 12;
        this.lblTotalEstVal.Text = "0 VNĐ";
        this.lblTotalEstVal.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

        //
        // lblStatusMsg
        //
        this.lblStatusMsg.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
        this.lblStatusMsg.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblStatusMsg.ForeColor = System.Drawing.ColorTranslator.FromHtml("#CAC6C1");
        this.lblStatusMsg.Location = new System.Drawing.Point(20, 290);
        this.lblStatusMsg.Name = "lblStatusMsg";
        this.lblStatusMsg.Size = new System.Drawing.Size(460, 60);
        this.lblStatusMsg.TabIndex = 13;

        //
        // UtilitiesForm
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.ColorTranslator.FromHtml("#101417");
        this.Controls.Add(this.splitMain);
        this.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.Name = "UtilitiesForm";
        this.Size = new System.Drawing.Size(980, 600);

        this.splitMain.Panel1.ResumeLayout(false);
        this.splitMain.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
        this.splitMain.ResumeLayout(false);
        this.cardInput.ResumeLayout(false);
        this.cardInput.PerformLayout();
        this.grpElectricity.ResumeLayout(false);
        this.grpElectricity.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.numElecOld)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numElecNew)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numElecRate)).EndInit();
        this.grpWater.ResumeLayout(false);
        this.grpWater.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.numWaterOld)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numWaterNew)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.numWaterRate)).EndInit();
        this.cardPreview.ResumeLayout(false);
        this.cardPreview.PerformLayout();
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.SplitContainer splitMain;
    private System.Windows.Forms.Panel cardInput;
    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Label lblRoom;
    private System.Windows.Forms.ComboBox cboRoom;
    private System.Windows.Forms.Label lblMonth;
    private System.Windows.Forms.DateTimePicker dtpMonth;
    private System.Windows.Forms.GroupBox grpElectricity;
    private System.Windows.Forms.Label lblElecOld;
    private System.Windows.Forms.NumericUpDown numElecOld;
    private System.Windows.Forms.Label lblElecNew;
    private System.Windows.Forms.NumericUpDown numElecNew;
    private System.Windows.Forms.Label lblElecRate;
    private System.Windows.Forms.NumericUpDown numElecRate;
    private System.Windows.Forms.GroupBox grpWater;
    private System.Windows.Forms.Label lblWaterOld;
    private System.Windows.Forms.NumericUpDown numWaterOld;
    private System.Windows.Forms.Label lblWaterNew;
    private System.Windows.Forms.NumericUpDown numWaterNew;
    private System.Windows.Forms.Label lblWaterRate;
    private System.Windows.Forms.NumericUpDown numWaterRate;
    private System.Windows.Forms.Button btnRecord;
    private System.Windows.Forms.Panel cardPreview;
    private System.Windows.Forms.Label lblPreviewHeader;
    private System.Windows.Forms.Label lblPreviewDisclaimer;
    private System.Windows.Forms.Label lblElecUsageLabel;
    private System.Windows.Forms.Label lblElecUsageVal;
    private System.Windows.Forms.Label lblElecEstLabel;
    private System.Windows.Forms.Label lblElecEstVal;
    private System.Windows.Forms.Label lblWaterUsageLabel;
    private System.Windows.Forms.Label lblWaterUsageVal;
    private System.Windows.Forms.Label lblWaterEstLabel;
    private System.Windows.Forms.Label lblWaterEstVal;
    private System.Windows.Forms.Panel pnlDivider;
    private System.Windows.Forms.Label lblTotalEstLabel;
    private System.Windows.Forms.Label lblTotalEstVal;
    private System.Windows.Forms.Label lblStatusMsg;
}
