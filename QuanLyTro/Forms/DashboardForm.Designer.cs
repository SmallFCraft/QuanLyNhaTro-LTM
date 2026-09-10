#nullable enable

namespace QuanLyTro.Forms;

partial class DashboardForm
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
        this.tblKpis = new System.Windows.Forms.TableLayoutPanel();
        this.cardTotalRooms = new KpiPaintPanel();
        this.pnlTotalStrip = new System.Windows.Forms.Panel();
        this.lblTotalLabel = new System.Windows.Forms.Label();
        this.lblTotalValue = new System.Windows.Forms.Label();
        this.lblTotalSub = new System.Windows.Forms.Label();
        this.cardAvailable = new KpiPaintPanel();
        this.pnlAvailableStrip = new System.Windows.Forms.Panel();
        this.lblAvailableLabel = new System.Windows.Forms.Label();
        this.lblAvailableValue = new System.Windows.Forms.Label();
        this.lblAvailableSub = new System.Windows.Forms.Label();
        this.cardOccupants = new KpiPaintPanel();
        this.pnlOccupantsStrip = new System.Windows.Forms.Panel();
        this.lblOccupantsLabel = new System.Windows.Forms.Label();
        this.lblOccupantsValue = new System.Windows.Forms.Label();
        this.lblOccupantsSub = new System.Windows.Forms.Label();
        this.cardDebt = new KpiPaintPanel();
        this.pnlDebtStrip = new System.Windows.Forms.Panel();
        this.lblDebtLabel = new System.Windows.Forms.Label();
        this.lblDebtValue = new System.Windows.Forms.Label();
        this.lblDebtSub = new System.Windows.Forms.Label();
        this.tblTables = new System.Windows.Forms.TableLayoutPanel();
        this.cardDebtList = new KpiPaintPanel();
        this.dgvDebt = new System.Windows.Forms.DataGridView();
        this.colDebtRoom = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colDebtPeriod = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colDebtAmount = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colDebtRepresentative = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.lblDebtListTitle = new System.Windows.Forms.Label();
        this.cardExpiring = new KpiPaintPanel();
        this.dgvExpiring = new System.Windows.Forms.DataGridView();
        this.colExpRoom = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colExpRepresentative = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colExpEndDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colExpRemaining = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.lblExpiringTitle = new System.Windows.Forms.Label();
        this.tblKpis.SuspendLayout();
        this.cardTotalRooms.SuspendLayout();
        this.cardAvailable.SuspendLayout();
        this.cardOccupants.SuspendLayout();
        this.cardDebt.SuspendLayout();
        this.tblTables.SuspendLayout();
        this.cardDebtList.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvDebt)).BeginInit();
        this.cardExpiring.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvExpiring)).BeginInit();
        this.SuspendLayout();

        //
        // tblKpis — 4 cột đều nhau, dải KPI cao 104px (DESIGN.md §3.4)
        //
        this.tblKpis.ColumnCount = 4;
        this.tblKpis.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
        this.tblKpis.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
        this.tblKpis.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
        this.tblKpis.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
        this.tblKpis.Controls.Add(this.cardTotalRooms, 0, 0);
        this.tblKpis.Controls.Add(this.cardAvailable, 1, 0);
        this.tblKpis.Controls.Add(this.cardOccupants, 2, 0);
        this.tblKpis.Controls.Add(this.cardDebt, 3, 0);
        this.tblKpis.Dock = System.Windows.Forms.DockStyle.Top;
        this.tblKpis.Location = new System.Drawing.Point(14, 14);
        this.tblKpis.Name = "tblKpis";
        this.tblKpis.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
        this.tblKpis.RowCount = 1;
        this.tblKpis.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tblKpis.Size = new System.Drawing.Size(972, 104);
        this.tblKpis.TabIndex = 0;

        //
        // cardTotalRooms — KPI tổng số phòng, dải đỉnh muted 2px
        //
        this.cardTotalRooms.BorderHex = "#262A2E";
        this.cardTotalRooms.Controls.Add(this.lblTotalSub);
        this.cardTotalRooms.Controls.Add(this.lblTotalValue);
        this.cardTotalRooms.Controls.Add(this.lblTotalLabel);
        this.cardTotalRooms.Controls.Add(this.pnlTotalStrip);
        this.cardTotalRooms.Dock = System.Windows.Forms.DockStyle.Fill;
        this.cardTotalRooms.FillHex = "#181C1F";
        this.cardTotalRooms.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
        this.cardTotalRooms.Name = "cardTotalRooms";
        this.cardTotalRooms.Radius = 6;
        this.cardTotalRooms.TabIndex = 0;

        //
        // pnlTotalStrip
        //
        this.pnlTotalStrip.BackColor = System.Drawing.ColorTranslator.FromHtml("#3A444E");
        this.pnlTotalStrip.Dock = System.Windows.Forms.DockStyle.Top;
        this.pnlTotalStrip.Location = new System.Drawing.Point(0, 0);
        this.pnlTotalStrip.Name = "pnlTotalStrip";
        this.pnlTotalStrip.Size = new System.Drawing.Size(240, 2);
        this.pnlTotalStrip.TabIndex = 0;

        //
        // lblTotalLabel
        //
        this.lblTotalLabel.AutoSize = true;
        this.lblTotalLabel.BackColor = System.Drawing.Color.Transparent;
        this.lblTotalLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
        this.lblTotalLabel.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblTotalLabel.Location = new System.Drawing.Point(14, 14);
        this.lblTotalLabel.Name = "lblTotalLabel";
        this.lblTotalLabel.Size = new System.Drawing.Size(120, 13);
        this.lblTotalLabel.TabIndex = 1;
        this.lblTotalLabel.Text = "TỔNG SỐ PHÒNG";

        //
        // lblTotalValue
        //
        this.lblTotalValue.AutoSize = true;
        this.lblTotalValue.BackColor = System.Drawing.Color.Transparent;
        this.lblTotalValue.Font = new System.Drawing.Font("Consolas", 16F, System.Drawing.FontStyle.Bold);
        this.lblTotalValue.ForeColor = System.Drawing.ColorTranslator.FromHtml("#FAF8F5");
        this.lblTotalValue.Location = new System.Drawing.Point(13, 32);
        this.lblTotalValue.Name = "lblTotalValue";
        this.lblTotalValue.Size = new System.Drawing.Size(24, 24);
        this.lblTotalValue.TabIndex = 2;
        this.lblTotalValue.Text = "0";

        //
        // lblTotalSub
        //
        this.lblTotalSub.AutoSize = true;
        this.lblTotalSub.BackColor = System.Drawing.Color.Transparent;
        this.lblTotalSub.Font = new System.Drawing.Font("Consolas", 8.25F);
        this.lblTotalSub.ForeColor = System.Drawing.ColorTranslator.FromHtml("#767E88");
        this.lblTotalSub.Location = new System.Drawing.Point(14, 64);
        this.lblTotalSub.Name = "lblTotalSub";
        this.lblTotalSub.Size = new System.Drawing.Size(95, 13);
        this.lblTotalSub.TabIndex = 3;
        this.lblTotalSub.Text = "Thuê 0 · Trống 0";

        //
        // cardAvailable — KPI phòng trống, dải đỉnh sage 2px
        //
        this.cardAvailable.BorderHex = "#262A2E";
        this.cardAvailable.Controls.Add(this.lblAvailableSub);
        this.cardAvailable.Controls.Add(this.lblAvailableValue);
        this.cardAvailable.Controls.Add(this.lblAvailableLabel);
        this.cardAvailable.Controls.Add(this.pnlAvailableStrip);
        this.cardAvailable.Dock = System.Windows.Forms.DockStyle.Fill;
        this.cardAvailable.FillHex = "#181C1F";
        this.cardAvailable.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
        this.cardAvailable.Name = "cardAvailable";
        this.cardAvailable.Radius = 6;
        this.cardAvailable.TabIndex = 1;

        //
        // pnlAvailableStrip
        //
        this.pnlAvailableStrip.BackColor = System.Drawing.ColorTranslator.FromHtml("#8BD7A3");
        this.pnlAvailableStrip.Dock = System.Windows.Forms.DockStyle.Top;
        this.pnlAvailableStrip.Location = new System.Drawing.Point(0, 0);
        this.pnlAvailableStrip.Name = "pnlAvailableStrip";
        this.pnlAvailableStrip.Size = new System.Drawing.Size(240, 2);
        this.pnlAvailableStrip.TabIndex = 0;

        //
        // lblAvailableLabel
        //
        this.lblAvailableLabel.AutoSize = true;
        this.lblAvailableLabel.BackColor = System.Drawing.Color.Transparent;
        this.lblAvailableLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
        this.lblAvailableLabel.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblAvailableLabel.Location = new System.Drawing.Point(14, 14);
        this.lblAvailableLabel.Name = "lblAvailableLabel";
        this.lblAvailableLabel.Size = new System.Drawing.Size(96, 13);
        this.lblAvailableLabel.TabIndex = 1;
        this.lblAvailableLabel.Text = "PHÒNG TRỐNG";

        //
        // lblAvailableValue
        //
        this.lblAvailableValue.AutoSize = true;
        this.lblAvailableValue.BackColor = System.Drawing.Color.Transparent;
        this.lblAvailableValue.Font = new System.Drawing.Font("Consolas", 16F, System.Drawing.FontStyle.Bold);
        this.lblAvailableValue.ForeColor = System.Drawing.ColorTranslator.FromHtml("#8BD7A3");
        this.lblAvailableValue.Location = new System.Drawing.Point(13, 32);
        this.lblAvailableValue.Name = "lblAvailableValue";
        this.lblAvailableValue.Size = new System.Drawing.Size(24, 24);
        this.lblAvailableValue.TabIndex = 2;
        this.lblAvailableValue.Text = "0";

        //
        // lblAvailableSub
        //
        this.lblAvailableSub.AutoSize = true;
        this.lblAvailableSub.BackColor = System.Drawing.Color.Transparent;
        this.lblAvailableSub.Font = new System.Drawing.Font("Consolas", 8.25F);
        this.lblAvailableSub.ForeColor = System.Drawing.ColorTranslator.FromHtml("#767E88");
        this.lblAvailableSub.Location = new System.Drawing.Point(14, 64);
        this.lblAvailableSub.Name = "lblAvailableSub";
        this.lblAvailableSub.Size = new System.Drawing.Size(125, 13);
        this.lblAvailableSub.TabIndex = 3;
        this.lblAvailableSub.Text = "Tỷ lệ lấp đầy 0%";

        //
        // cardOccupants — KPI người đang ở
        //
        this.cardOccupants.BorderHex = "#262A2E";
        this.cardOccupants.Controls.Add(this.lblOccupantsSub);
        this.cardOccupants.Controls.Add(this.lblOccupantsValue);
        this.cardOccupants.Controls.Add(this.lblOccupantsLabel);
        this.cardOccupants.Controls.Add(this.pnlOccupantsStrip);
        this.cardOccupants.Dock = System.Windows.Forms.DockStyle.Fill;
        this.cardOccupants.FillHex = "#181C1F";
        this.cardOccupants.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
        this.cardOccupants.Name = "cardOccupants";
        this.cardOccupants.Radius = 6;
        this.cardOccupants.TabIndex = 2;

        //
        // pnlOccupantsStrip
        //
        this.pnlOccupantsStrip.BackColor = System.Drawing.ColorTranslator.FromHtml("#3A444E");
        this.pnlOccupantsStrip.Dock = System.Windows.Forms.DockStyle.Top;
        this.pnlOccupantsStrip.Location = new System.Drawing.Point(0, 0);
        this.pnlOccupantsStrip.Name = "pnlOccupantsStrip";
        this.pnlOccupantsStrip.Size = new System.Drawing.Size(240, 2);
        this.pnlOccupantsStrip.TabIndex = 0;

        //
        // lblOccupantsLabel
        //
        this.lblOccupantsLabel.AutoSize = true;
        this.lblOccupantsLabel.BackColor = System.Drawing.Color.Transparent;
        this.lblOccupantsLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
        this.lblOccupantsLabel.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblOccupantsLabel.Location = new System.Drawing.Point(14, 14);
        this.lblOccupantsLabel.Name = "lblOccupantsLabel";
        this.lblOccupantsLabel.Size = new System.Drawing.Size(101, 13);
        this.lblOccupantsLabel.TabIndex = 1;
        this.lblOccupantsLabel.Text = "NGƯỜI ĐANG Ở";

        //
        // lblOccupantsValue
        //
        this.lblOccupantsValue.AutoSize = true;
        this.lblOccupantsValue.BackColor = System.Drawing.Color.Transparent;
        this.lblOccupantsValue.Font = new System.Drawing.Font("Consolas", 16F, System.Drawing.FontStyle.Bold);
        this.lblOccupantsValue.ForeColor = System.Drawing.ColorTranslator.FromHtml("#FAF8F5");
        this.lblOccupantsValue.Location = new System.Drawing.Point(13, 32);
        this.lblOccupantsValue.Name = "lblOccupantsValue";
        this.lblOccupantsValue.Size = new System.Drawing.Size(24, 24);
        this.lblOccupantsValue.TabIndex = 2;
        this.lblOccupantsValue.Text = "0";

        //
        // lblOccupantsSub
        //
        this.lblOccupantsSub.AutoSize = true;
        this.lblOccupantsSub.BackColor = System.Drawing.Color.Transparent;
        this.lblOccupantsSub.Font = new System.Drawing.Font("Consolas", 8.25F);
        this.lblOccupantsSub.ForeColor = System.Drawing.ColorTranslator.FromHtml("#767E88");
        this.lblOccupantsSub.Location = new System.Drawing.Point(14, 64);
        this.lblOccupantsSub.Name = "lblOccupantsSub";
        this.lblOccupantsSub.Size = new System.Drawing.Size(125, 13);
        this.lblOccupantsSub.TabIndex = 3;
        this.lblOccupantsSub.Text = "Trên 0 phòng";

        //
        // cardDebt — KPI còn nợ tháng, dải đỉnh terracotta 2px
        //
        this.cardDebt.BorderHex = "#262A2E";
        this.cardDebt.Controls.Add(this.lblDebtSub);
        this.cardDebt.Controls.Add(this.lblDebtValue);
        this.cardDebt.Controls.Add(this.lblDebtLabel);
        this.cardDebt.Controls.Add(this.pnlDebtStrip);
        this.cardDebt.Dock = System.Windows.Forms.DockStyle.Fill;
        this.cardDebt.FillHex = "#181C1F";
        this.cardDebt.Margin = new System.Windows.Forms.Padding(0);
        this.cardDebt.Name = "cardDebt";
        this.cardDebt.Radius = 6;
        this.cardDebt.TabIndex = 3;

        //
        // pnlDebtStrip
        //
        this.pnlDebtStrip.BackColor = System.Drawing.ColorTranslator.FromHtml("#D95D39");
        this.pnlDebtStrip.Dock = System.Windows.Forms.DockStyle.Top;
        this.pnlDebtStrip.Location = new System.Drawing.Point(0, 0);
        this.pnlDebtStrip.Name = "pnlDebtStrip";
        this.pnlDebtStrip.Size = new System.Drawing.Size(240, 2);
        this.pnlDebtStrip.TabIndex = 0;

        //
        // lblDebtLabel
        //
        this.lblDebtLabel.AutoSize = true;
        this.lblDebtLabel.BackColor = System.Drawing.Color.Transparent;
        this.lblDebtLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
        this.lblDebtLabel.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblDebtLabel.Location = new System.Drawing.Point(14, 14);
        this.lblDebtLabel.Name = "lblDebtLabel";
        this.lblDebtLabel.Size = new System.Drawing.Size(90, 13);
        this.lblDebtLabel.TabIndex = 1;
        this.lblDebtLabel.Text = "CÒN NỢ THÁNG";

        //
        // lblDebtValue
        //
        this.lblDebtValue.AutoSize = true;
        this.lblDebtValue.BackColor = System.Drawing.Color.Transparent;
        this.lblDebtValue.Font = new System.Drawing.Font("Consolas", 16F, System.Drawing.FontStyle.Bold);
        this.lblDebtValue.ForeColor = System.Drawing.ColorTranslator.FromHtml("#D95D39");
        this.lblDebtValue.Location = new System.Drawing.Point(13, 32);
        this.lblDebtValue.Name = "lblDebtValue";
        this.lblDebtValue.Size = new System.Drawing.Size(42, 24);
        this.lblDebtValue.TabIndex = 2;
        this.lblDebtValue.Text = "0 đ";

        //
        // lblDebtSub
        //
        this.lblDebtSub.AutoSize = true;
        this.lblDebtSub.BackColor = System.Drawing.Color.Transparent;
        this.lblDebtSub.Font = new System.Drawing.Font("Consolas", 8.25F);
        this.lblDebtSub.ForeColor = System.Drawing.ColorTranslator.FromHtml("#767E88");
        this.lblDebtSub.Location = new System.Drawing.Point(14, 64);
        this.lblDebtSub.Name = "lblDebtSub";
        this.lblDebtSub.Size = new System.Drawing.Size(125, 13);
        this.lblDebtSub.TabIndex = 3;
        this.lblDebtSub.Text = "0 phòng chưa thu";

        //
        // tblTables — hai bảng điều hành cạnh nhau
        //
        this.tblTables.ColumnCount = 2;
        this.tblTables.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        this.tblTables.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        this.tblTables.Controls.Add(this.cardDebtList, 0, 0);
        this.tblTables.Controls.Add(this.cardExpiring, 1, 0);
        this.tblTables.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tblTables.Location = new System.Drawing.Point(14, 118);
        this.tblTables.Name = "tblTables";
        this.tblTables.RowCount = 1;
        this.tblTables.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tblTables.Size = new System.Drawing.Size(972, 468);
        this.tblTables.TabIndex = 1;

        //
        // cardDebtList — Còn nợ: đôn đốc (US-17)
        //
        this.cardDebtList.BorderHex = "#262A2E";
        this.cardDebtList.Controls.Add(this.dgvDebt);
        this.cardDebtList.Controls.Add(this.lblDebtListTitle);
        this.cardDebtList.Dock = System.Windows.Forms.DockStyle.Fill;
        this.cardDebtList.FillHex = "#181C1F";
        this.cardDebtList.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
        this.cardDebtList.Name = "cardDebtList";
        this.cardDebtList.Padding = new System.Windows.Forms.Padding(10);
        this.cardDebtList.Radius = 8;
        this.cardDebtList.TabIndex = 0;

        //
        // dgvDebt
        //
        this.dgvDebt.AccessibleName = "Bảng còn nợ cần đôn đốc";
        this.dgvDebt.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        this.dgvDebt.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colDebtRoom,
            this.colDebtPeriod,
            this.colDebtAmount,
            this.colDebtRepresentative});
        this.dgvDebt.Dock = System.Windows.Forms.DockStyle.Fill;
        this.dgvDebt.Location = new System.Drawing.Point(10, 40);
        this.dgvDebt.Name = "dgvDebt";
        this.dgvDebt.Size = new System.Drawing.Size(456, 404);
        this.dgvDebt.TabIndex = 1;

        //
        // colDebtRoom
        //
        this.colDebtRoom.HeaderText = "PHÒNG";
        this.colDebtRoom.Name = "colDebtRoom";
        this.colDebtRoom.ReadOnly = true;
        this.colDebtRoom.Width = 90;

        //
        // colDebtPeriod
        //
        this.colDebtPeriod.HeaderText = "KỲ";
        this.colDebtPeriod.Name = "colDebtPeriod";
        this.colDebtPeriod.ReadOnly = true;
        this.colDebtPeriod.Width = 80;

        //
        // colDebtAmount
        //
        this.colDebtAmount.DefaultCellStyle = new System.Windows.Forms.DataGridViewCellStyle
        {
            Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight,
            Font = new System.Drawing.Font("Consolas", 9.5F, System.Drawing.FontStyle.Bold),
            ForeColor = System.Drawing.ColorTranslator.FromHtml("#D95D39"),
        };
        this.colDebtAmount.HeaderText = "CÒN NỢ";
        this.colDebtAmount.Name = "colDebtAmount";
        this.colDebtAmount.ReadOnly = true;
        this.colDebtAmount.Width = 110;

        //
        // colDebtRepresentative
        //
        this.colDebtRepresentative.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
        this.colDebtRepresentative.HeaderText = "ĐẠI DIỆN";
        this.colDebtRepresentative.Name = "colDebtRepresentative";
        this.colDebtRepresentative.ReadOnly = true;

        //
        // lblDebtListTitle
        //
        this.lblDebtListTitle.Dock = System.Windows.Forms.DockStyle.Top;
        this.lblDebtListTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.lblDebtListTitle.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblDebtListTitle.Location = new System.Drawing.Point(10, 10);
        this.lblDebtListTitle.Name = "lblDebtListTitle";
        this.lblDebtListTitle.Padding = new System.Windows.Forms.Padding(2, 0, 0, 6);
        this.lblDebtListTitle.Size = new System.Drawing.Size(456, 30);
        this.lblDebtListTitle.TabIndex = 0;
        this.lblDebtListTitle.Text = "CÒN NỢ — ĐÔN ĐỐC (US-17)";
        this.lblDebtListTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

        //
        // cardExpiring — HĐ sắp hết hạn (US-11)
        //
        this.cardExpiring.BorderHex = "#262A2E";
        this.cardExpiring.Controls.Add(this.dgvExpiring);
        this.cardExpiring.Controls.Add(this.lblExpiringTitle);
        this.cardExpiring.Dock = System.Windows.Forms.DockStyle.Fill;
        this.cardExpiring.FillHex = "#181C1F";
        this.cardExpiring.Margin = new System.Windows.Forms.Padding(0);
        this.cardExpiring.Name = "cardExpiring";
        this.cardExpiring.Padding = new System.Windows.Forms.Padding(10);
        this.cardExpiring.Radius = 8;
        this.cardExpiring.TabIndex = 1;

        //
        // dgvExpiring
        //
        this.dgvExpiring.AccessibleName = "Bảng hợp đồng sắp hết hạn";
        this.dgvExpiring.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        this.dgvExpiring.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colExpRoom,
            this.colExpRepresentative,
            this.colExpEndDate,
            this.colExpRemaining});
        this.dgvExpiring.Dock = System.Windows.Forms.DockStyle.Fill;
        this.dgvExpiring.Location = new System.Drawing.Point(10, 40);
        this.dgvExpiring.Name = "dgvExpiring";
        this.dgvExpiring.Size = new System.Drawing.Size(456, 404);
        this.dgvExpiring.TabIndex = 1;

        //
        // colExpRoom
        //
        this.colExpRoom.HeaderText = "PHÒNG";
        this.colExpRoom.Name = "colExpRoom";
        this.colExpRoom.ReadOnly = true;
        this.colExpRoom.Width = 90;

        //
        // colExpRepresentative
        //
        this.colExpRepresentative.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
        this.colExpRepresentative.HeaderText = "ĐẠI DIỆN";
        this.colExpRepresentative.Name = "colExpRepresentative";
        this.colExpRepresentative.ReadOnly = true;

        //
        // colExpEndDate
        //
        this.colExpEndDate.DefaultCellStyle = new System.Windows.Forms.DataGridViewCellStyle
        {
            Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight,
            Font = new System.Drawing.Font("Consolas", 9.5F),
        };
        this.colExpEndDate.HeaderText = "HẾT HẠN";
        this.colExpEndDate.Name = "colExpEndDate";
        this.colExpEndDate.ReadOnly = true;
        this.colExpEndDate.Width = 100;

        //
        // colExpRemaining
        //
        this.colExpRemaining.DefaultCellStyle = new System.Windows.Forms.DataGridViewCellStyle
        {
            Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight,
            Font = new System.Drawing.Font("Consolas", 9.5F, System.Drawing.FontStyle.Bold),
        };
        this.colExpRemaining.HeaderText = "CÒN";
        this.colExpRemaining.Name = "colExpRemaining";
        this.colExpRemaining.ReadOnly = true;
        this.colExpRemaining.Width = 90;

        //
        // lblExpiringTitle
        //
        this.lblExpiringTitle.Dock = System.Windows.Forms.DockStyle.Top;
        this.lblExpiringTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.lblExpiringTitle.ForeColor = System.Drawing.ColorTranslator.FromHtml("#A89988");
        this.lblExpiringTitle.Location = new System.Drawing.Point(10, 10);
        this.lblExpiringTitle.Name = "lblExpiringTitle";
        this.lblExpiringTitle.Padding = new System.Windows.Forms.Padding(2, 0, 0, 6);
        this.lblExpiringTitle.Size = new System.Drawing.Size(456, 30);
        this.lblExpiringTitle.TabIndex = 0;
        this.lblExpiringTitle.Text = "HỢP ĐỒNG SẮP HẾT HẠN (US-11)";
        this.lblExpiringTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

        //
        // DashboardForm
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.AccessibleName = "Trang tổng quan vận hành";
        this.BackColor = System.Drawing.ColorTranslator.FromHtml("#101417");
        this.Controls.Add(this.tblTables);
        this.Controls.Add(this.tblKpis);
        this.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.Name = "DashboardForm";
        this.Padding = new System.Windows.Forms.Padding(14);
        this.Size = new System.Drawing.Size(1000, 600);

        this.tblKpis.ResumeLayout(false);
        this.cardTotalRooms.ResumeLayout(false);
        this.cardTotalRooms.PerformLayout();
        this.cardAvailable.ResumeLayout(false);
        this.cardAvailable.PerformLayout();
        this.cardOccupants.ResumeLayout(false);
        this.cardOccupants.PerformLayout();
        this.cardDebt.ResumeLayout(false);
        this.cardDebt.PerformLayout();
        this.tblTables.ResumeLayout(false);
        this.cardDebtList.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.dgvDebt)).EndInit();
        this.cardExpiring.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.dgvExpiring)).EndInit();
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.TableLayoutPanel tblKpis;
    private KpiPaintPanel cardTotalRooms;
    private System.Windows.Forms.Panel pnlTotalStrip;
    private System.Windows.Forms.Label lblTotalLabel;
    private System.Windows.Forms.Label lblTotalValue;
    private System.Windows.Forms.Label lblTotalSub;
    private KpiPaintPanel cardAvailable;
    private System.Windows.Forms.Panel pnlAvailableStrip;
    private System.Windows.Forms.Label lblAvailableLabel;
    private System.Windows.Forms.Label lblAvailableValue;
    private System.Windows.Forms.Label lblAvailableSub;
    private KpiPaintPanel cardOccupants;
    private System.Windows.Forms.Panel pnlOccupantsStrip;
    private System.Windows.Forms.Label lblOccupantsLabel;
    private System.Windows.Forms.Label lblOccupantsValue;
    private System.Windows.Forms.Label lblOccupantsSub;
    private KpiPaintPanel cardDebt;
    private System.Windows.Forms.Panel pnlDebtStrip;
    private System.Windows.Forms.Label lblDebtLabel;
    private System.Windows.Forms.Label lblDebtValue;
    private System.Windows.Forms.Label lblDebtSub;
    private System.Windows.Forms.TableLayoutPanel tblTables;
    private KpiPaintPanel cardDebtList;
    private System.Windows.Forms.DataGridView dgvDebt;
    private System.Windows.Forms.DataGridViewTextBoxColumn colDebtRoom;
    private System.Windows.Forms.DataGridViewTextBoxColumn colDebtPeriod;
    private System.Windows.Forms.DataGridViewTextBoxColumn colDebtAmount;
    private System.Windows.Forms.DataGridViewTextBoxColumn colDebtRepresentative;
    private System.Windows.Forms.Label lblDebtListTitle;
    private KpiPaintPanel cardExpiring;
    private System.Windows.Forms.DataGridView dgvExpiring;
    private System.Windows.Forms.DataGridViewTextBoxColumn colExpRoom;
    private System.Windows.Forms.DataGridViewTextBoxColumn colExpRepresentative;
    private System.Windows.Forms.DataGridViewTextBoxColumn colExpEndDate;
    private System.Windows.Forms.DataGridViewTextBoxColumn colExpRemaining;
    private System.Windows.Forms.Label lblExpiringTitle;
}
