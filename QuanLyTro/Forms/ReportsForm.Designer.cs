namespace QuanLyTro.Forms
{
    partial class ReportsForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel pnlToolbar;
        private System.Windows.Forms.Panel pnlKpis;
        private System.Windows.Forms.Panel pnlKpiPaid;
        private System.Windows.Forms.Panel pnlKpiPaidBar;
        private System.Windows.Forms.Panel pnlKpiUnpaid;
        private System.Windows.Forms.Panel pnlKpiUnpaidBar;
        private System.Windows.Forms.Panel pnlKpiOccupancy;
        private System.Windows.Forms.Panel pnlKpiOccupancyBar;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Panel pnlGridHost;
        private System.Windows.Forms.Panel pnlNote;

        private System.Windows.Forms.Label lblMonth;
        private System.Windows.Forms.ComboBox cboMonth;
        private System.Windows.Forms.Button btnView;
        private System.Windows.Forms.Button btnExportResidence;

        private System.Windows.Forms.Label lblKpiPaidTitle;
        private System.Windows.Forms.Label lblKpiPaidValue;
        private System.Windows.Forms.Label lblKpiPaidSub;

        private System.Windows.Forms.Label lblKpiUnpaidTitle;
        private System.Windows.Forms.Label lblKpiUnpaidValue;
        private System.Windows.Forms.Label lblKpiUnpaidSub;

        private System.Windows.Forms.Label lblKpiOccupancyTitle;
        private System.Windows.Forms.Label lblKpiOccupancyValue;
        private System.Windows.Forms.Label lblKpiOccupancySub;

        private System.Windows.Forms.Label lblSectionTitle;
        private System.Windows.Forms.Label lblRangeNote;
        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.Label lblNoteText;

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
            this.pnlKpis = new System.Windows.Forms.Panel();
            this.pnlKpiPaid = new System.Windows.Forms.Panel();
            this.pnlKpiPaidBar = new System.Windows.Forms.Panel();
            this.pnlKpiUnpaid = new System.Windows.Forms.Panel();
            this.pnlKpiUnpaidBar = new System.Windows.Forms.Panel();
            this.pnlKpiOccupancy = new System.Windows.Forms.Panel();
            this.pnlKpiOccupancyBar = new System.Windows.Forms.Panel();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.pnlGridHost = new System.Windows.Forms.Panel();
            this.pnlNote = new System.Windows.Forms.Panel();
            this.lblMonth = new System.Windows.Forms.Label();
            this.cboMonth = new System.Windows.Forms.ComboBox();
            this.btnView = new System.Windows.Forms.Button();
            this.btnExportResidence = new System.Windows.Forms.Button();
            this.lblKpiPaidTitle = new System.Windows.Forms.Label();
            this.lblKpiPaidValue = new System.Windows.Forms.Label();
            this.lblKpiPaidSub = new System.Windows.Forms.Label();
            this.lblKpiUnpaidTitle = new System.Windows.Forms.Label();
            this.lblKpiUnpaidValue = new System.Windows.Forms.Label();
            this.lblKpiUnpaidSub = new System.Windows.Forms.Label();
            this.lblKpiOccupancyTitle = new System.Windows.Forms.Label();
            this.lblKpiOccupancyValue = new System.Windows.Forms.Label();
            this.lblKpiOccupancySub = new System.Windows.Forms.Label();
            this.lblSectionTitle = new System.Windows.Forms.Label();
            this.lblRangeNote = new System.Windows.Forms.Label();
            this.grid = new System.Windows.Forms.DataGridView();
            this.lblNoteText = new System.Windows.Forms.Label();

            this.pnlToolbar.SuspendLayout();
            this.pnlKpis.SuspendLayout();
            this.pnlKpiPaid.SuspendLayout();
            this.pnlKpiUnpaid.SuspendLayout();
            this.pnlKpiOccupancy.SuspendLayout();
            this.pnlHeader.SuspendLayout();
            this.pnlGridHost.SuspendLayout();
            this.pnlNote.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.SuspendLayout();

            // ---- toolbar ----
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Height = 44;
            this.pnlToolbar.BackColor = ScreenTheme.From(ScreenTheme.Base);
            this.pnlToolbar.Padding = new System.Windows.Forms.Padding(12, 6, 12, 6);
            this.pnlToolbar.Controls.Add(this.lblMonth);
            this.pnlToolbar.Controls.Add(this.cboMonth);
            this.pnlToolbar.Controls.Add(this.btnView);
            this.pnlToolbar.Controls.Add(this.btnExportResidence);

            this.lblMonth.AutoSize = true;
            this.lblMonth.Location = new System.Drawing.Point(12, 14);
            this.lblMonth.Text = "Kỳ báo cáo";
            this.lblMonth.ForeColor = ScreenTheme.From(ScreenTheme.Muted);
            this.lblMonth.Font = ScreenTheme.Body;
            this.lblMonth.BackColor = System.Drawing.Color.Transparent;

            this.cboMonth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.cboMonth.Location = new System.Drawing.Point(86, 10);
            this.cboMonth.Size = new System.Drawing.Size(104, 23);
            this.cboMonth.TabIndex = 1;
            this.cboMonth.AccessibleName = "Kỳ báo cáo";

            this.btnView.Location = new System.Drawing.Point(198, 9);
            this.btnView.Size = new System.Drawing.Size(86, 25);
            this.btnView.TabIndex = 2;
            this.btnView.Text = "Xem (F5)";
            this.btnView.AccessibleName = "Xem thống kê";

            this.btnExportResidence.Location = new System.Drawing.Point(830, 9);
            this.btnExportResidence.Size = new System.Drawing.Size(180, 25);
            this.btnExportResidence.TabIndex = 3;
            this.btnExportResidence.Text = "Xuất CSV tạm trú (US-08)";
            this.btnExportResidence.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnExportResidence.AccessibleName = "Xuất file CSV danh sách tạm trú";

            // ---- kpi panel ----
            this.pnlKpis.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlKpis.Height = 96;
            this.pnlKpis.BackColor = ScreenTheme.From(ScreenTheme.Base);
            this.pnlKpis.Padding = new System.Windows.Forms.Padding(12, 0, 12, 8);
            this.pnlKpis.Controls.Add(this.pnlKpiOccupancy);
            this.pnlKpis.Controls.Add(this.pnlKpiUnpaid);
            this.pnlKpis.Controls.Add(this.pnlKpiPaid);

            // ---- KPI 1: Đã thu (Sage top bar) ----
            this.pnlKpiPaid.Location = new System.Drawing.Point(12, 0);
            this.pnlKpiPaid.Size = new System.Drawing.Size(320, 88);
            this.pnlKpiPaid.BackColor = ScreenTheme.From(ScreenTheme.Card);
            this.pnlKpiPaid.Controls.Add(this.pnlKpiPaidBar);
            this.pnlKpiPaid.Controls.Add(this.lblKpiPaidTitle);
            this.pnlKpiPaid.Controls.Add(this.lblKpiPaidValue);
            this.pnlKpiPaid.Controls.Add(this.lblKpiPaidSub);

            this.pnlKpiPaidBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlKpiPaidBar.Height = 2;
            this.pnlKpiPaidBar.BackColor = ScreenTheme.From(ScreenTheme.Sage);

            this.lblKpiPaidTitle.AutoSize = true;
            this.lblKpiPaidTitle.Location = new System.Drawing.Point(12, 10);
            this.lblKpiPaidTitle.Text = "ĐÃ THU TRONG KỲ";
            this.lblKpiPaidTitle.Font = ScreenTheme.HeaderFont;
            this.lblKpiPaidTitle.ForeColor = ScreenTheme.From(ScreenTheme.Muted);
            this.lblKpiPaidTitle.BackColor = System.Drawing.Color.Transparent;

            this.lblKpiPaidValue.AutoSize = true;
            this.lblKpiPaidValue.Location = new System.Drawing.Point(10, 30);
            this.lblKpiPaidValue.Text = "0 đ";
            this.lblKpiPaidValue.Font = ScreenTheme.KpiValue;
            this.lblKpiPaidValue.ForeColor = ScreenTheme.From(ScreenTheme.Sage);
            this.lblKpiPaidValue.BackColor = System.Drawing.Color.Transparent;

            this.lblKpiPaidSub.AutoSize = true;
            this.lblKpiPaidSub.Location = new System.Drawing.Point(12, 60);
            this.lblKpiPaidSub.Text = "Hóa đơn đã thanh toán";
            this.lblKpiPaidSub.Font = ScreenTheme.Body;
            this.lblKpiPaidSub.ForeColor = ScreenTheme.From(ScreenTheme.Dim);
            this.lblKpiPaidSub.BackColor = System.Drawing.Color.Transparent;

            // ---- KPI 2: Còn nợ (Terracotta top bar) ----
            this.pnlKpiUnpaid.Location = new System.Drawing.Point(344, 0);
            this.pnlKpiUnpaid.Size = new System.Drawing.Size(320, 88);
            this.pnlKpiUnpaid.BackColor = ScreenTheme.From(ScreenTheme.Card);
            this.pnlKpiUnpaid.Controls.Add(this.pnlKpiUnpaidBar);
            this.pnlKpiUnpaid.Controls.Add(this.lblKpiUnpaidTitle);
            this.pnlKpiUnpaid.Controls.Add(this.lblKpiUnpaidValue);
            this.pnlKpiUnpaid.Controls.Add(this.lblKpiUnpaidSub);

            this.pnlKpiUnpaidBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlKpiUnpaidBar.Height = 2;
            this.pnlKpiUnpaidBar.BackColor = ScreenTheme.From(ScreenTheme.Terracotta);

            this.lblKpiUnpaidTitle.AutoSize = true;
            this.lblKpiUnpaidTitle.Location = new System.Drawing.Point(12, 10);
            this.lblKpiUnpaidTitle.Text = "CÒN NỢ (CHƯA THU)";
            this.lblKpiUnpaidTitle.Font = ScreenTheme.HeaderFont;
            this.lblKpiUnpaidTitle.ForeColor = ScreenTheme.From(ScreenTheme.Muted);
            this.lblKpiUnpaidTitle.BackColor = System.Drawing.Color.Transparent;

            this.lblKpiUnpaidValue.AutoSize = true;
            this.lblKpiUnpaidValue.Location = new System.Drawing.Point(10, 30);
            this.lblKpiUnpaidValue.Text = "0 đ";
            this.lblKpiUnpaidValue.Font = ScreenTheme.KpiValue;
            this.lblKpiUnpaidValue.ForeColor = ScreenTheme.From(ScreenTheme.Terracotta);
            this.lblKpiUnpaidValue.BackColor = System.Drawing.Color.Transparent;

            this.lblKpiUnpaidSub.AutoSize = true;
            this.lblKpiUnpaidSub.Location = new System.Drawing.Point(12, 60);
            this.lblKpiUnpaidSub.Text = "Chờ đôn đốc thu tiền";
            this.lblKpiUnpaidSub.Font = ScreenTheme.Body;
            this.lblKpiUnpaidSub.ForeColor = ScreenTheme.From(ScreenTheme.Dim);
            this.lblKpiUnpaidSub.BackColor = System.Drawing.Color.Transparent;

            // ---- KPI 3: Lấp đầy (Sage/Muted) ----
            this.pnlKpiOccupancy.Location = new System.Drawing.Point(676, 0);
            this.pnlKpiOccupancy.Size = new System.Drawing.Size(336, 88);
            this.pnlKpiOccupancy.BackColor = ScreenTheme.From(ScreenTheme.Card);
            this.pnlKpiOccupancy.Controls.Add(this.pnlKpiOccupancyBar);
            this.pnlKpiOccupancy.Controls.Add(this.lblKpiOccupancyTitle);
            this.pnlKpiOccupancy.Controls.Add(this.lblKpiOccupancyValue);
            this.pnlKpiOccupancy.Controls.Add(this.lblKpiOccupancySub);

            this.pnlKpiOccupancyBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlKpiOccupancyBar.Height = 2;
            this.pnlKpiOccupancyBar.BackColor = ScreenTheme.From(ScreenTheme.Sage);

            this.lblKpiOccupancyTitle.AutoSize = true;
            this.lblKpiOccupancyTitle.Location = new System.Drawing.Point(12, 10);
            this.lblKpiOccupancyTitle.Text = "CÔNG SUẤT PHÒNG";
            this.lblKpiOccupancyTitle.Font = ScreenTheme.HeaderFont;
            this.lblKpiOccupancyTitle.ForeColor = ScreenTheme.From(ScreenTheme.Muted);
            this.lblKpiOccupancyTitle.BackColor = System.Drawing.Color.Transparent;

            this.lblKpiOccupancyValue.AutoSize = true;
            this.lblKpiOccupancyValue.Location = new System.Drawing.Point(10, 30);
            this.lblKpiOccupancyValue.Text = "0%";
            this.lblKpiOccupancyValue.Font = ScreenTheme.KpiValue;
            this.lblKpiOccupancyValue.ForeColor = ScreenTheme.From(ScreenTheme.CreamLight);
            this.lblKpiOccupancyValue.BackColor = System.Drawing.Color.Transparent;

            this.lblKpiOccupancySub.AutoSize = true;
            this.lblKpiOccupancySub.Location = new System.Drawing.Point(12, 60);
            this.lblKpiOccupancySub.Text = "Đang thuê 0 / 0 phòng · 0 người ở";
            this.lblKpiOccupancySub.Font = ScreenTheme.Body;
            this.lblKpiOccupancySub.ForeColor = ScreenTheme.From(ScreenTheme.Dim);
            this.lblKpiOccupancySub.BackColor = System.Drawing.Color.Transparent;

            // ---- header khối bảng ----
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height = 34;
            this.pnlHeader.BackColor = ScreenTheme.From(ScreenTheme.Panel);
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(12, 0, 12, 0);
            this.pnlHeader.Controls.Add(this.lblSectionTitle);
            this.pnlHeader.Controls.Add(this.lblRangeNote);

            this.lblSectionTitle.AutoSize = true;
            this.lblSectionTitle.Location = new System.Drawing.Point(12, 9);
            this.lblSectionTitle.Text = "DOANH THU & CÔNG NỢ THEO THÁNG (US-19)";
            this.lblSectionTitle.Font = ScreenTheme.HeaderFont;
            this.lblSectionTitle.ForeColor = ScreenTheme.From(ScreenTheme.Muted);
            this.lblSectionTitle.BackColor = System.Drawing.Color.Transparent;

            this.lblRangeNote.AutoSize = true;
            this.lblRangeNote.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.lblRangeNote.Location = new System.Drawing.Point(740, 9);
            this.lblRangeNote.Text = "6 kỳ gần nhất · Dòng tổng cộng lũy kế";
            this.lblRangeNote.Font = ScreenTheme.Body;
            this.lblRangeNote.ForeColor = ScreenTheme.From(ScreenTheme.Dim);
            this.lblRangeNote.BackColor = System.Drawing.Color.Transparent;

            // ---- grid host ----
            this.pnlGridHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlGridHost.BackColor = ScreenTheme.From(ScreenTheme.Base);
            this.pnlGridHost.Padding = new System.Windows.Forms.Padding(12, 0, 12, 0);
            this.pnlGridHost.Controls.Add(this.grid);

            // ---- grid ----
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.TabIndex = 0;
            this.grid.AccessibleName = "Bảng doanh thu nhiều tháng";
            this.grid.Name = "grid";

            // ---- note panel ----
            this.pnlNote.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlNote.Height = 30;
            this.pnlNote.BackColor = ScreenTheme.From(ScreenTheme.Base);
            this.pnlNote.Padding = new System.Windows.Forms.Padding(12, 6, 12, 6);
            this.pnlNote.Controls.Add(this.lblNoteText);

            this.lblNoteText.AutoSize = true;
            this.lblNoteText.Location = new System.Drawing.Point(12, 6);
            this.lblNoteText.Text = "US-19: Bảng tổng hợp nhiều tháng được Client truy vấn từng kỳ rồi tự cộng dòng tổng. CSV tạm trú (US-08) xuất mã hóa UTF-8 BOM.";
            this.lblNoteText.Font = ScreenTheme.Body;
            this.lblNoteText.ForeColor = ScreenTheme.From(ScreenTheme.Dim);
            this.lblNoteText.BackColor = System.Drawing.Color.Transparent;

            // ---- ReportsForm ----
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = ScreenTheme.From(ScreenTheme.Base);
            this.Controls.Add(this.pnlGridHost);
            this.Controls.Add(this.pnlNote);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.pnlKpis);
            this.Controls.Add(this.pnlToolbar);
            this.Font = ScreenTheme.Body;
            this.MinimumSize = new System.Drawing.Size(760, 420);
            this.Name = "ReportsForm";
            this.Padding = new System.Windows.Forms.Padding(0);
            this.Size = new System.Drawing.Size(1040, 620);
            this.AccessibleName = "Màn hình báo cáo thống kê";

            this.pnlToolbar.ResumeLayout(false);
            this.pnlToolbar.PerformLayout();
            this.pnlKpis.ResumeLayout(false);
            this.pnlKpiPaid.ResumeLayout(false);
            this.pnlKpiPaid.PerformLayout();
            this.pnlKpiUnpaid.ResumeLayout(false);
            this.pnlKpiUnpaid.PerformLayout();
            this.pnlKpiOccupancy.ResumeLayout(false);
            this.pnlKpiOccupancy.PerformLayout();
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlGridHost.ResumeLayout(false);
            this.pnlNote.ResumeLayout(false);
            this.pnlNote.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion
    }
}
