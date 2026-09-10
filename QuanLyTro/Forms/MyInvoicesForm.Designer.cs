namespace QuanLyTro.Forms
{
    partial class MyInvoicesForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel pnlMain;
        private ScreenTheme.CardPanel pnlProfile;
        private ScreenTheme.CardPanel pnlAvatar;
        private System.Windows.Forms.Label lblAvatarInitials;
        private System.Windows.Forms.Label lblTenantName;
        private ScreenTheme.TagLabel lblRoomTag;
        private System.Windows.Forms.Label lblTenantMeta;
        private System.Windows.Forms.Label lblContractExpiry;
        private System.Windows.Forms.Label lblReadOnly;

        private ScreenTheme.CardPanel pnlBanner;
        private ScreenTheme.CardPanel pnlBannerIcon;
        private System.Windows.Forms.Label lblBannerIconText;
        private System.Windows.Forms.Label lblBannerText;
        private ScreenTheme.TagLabel lblBannerTag;

        private ScreenTheme.CardPanel pnlReceipt;
        private System.Windows.Forms.Panel pnlReceiptHeader;
        private System.Windows.Forms.Label lblReceiptTitle;
        private System.Windows.Forms.Label lblReceiptCode;
        private ScreenTheme.TagLabel lblReceiptStatus;

        private System.Windows.Forms.Panel pnlReceiptBody;
        private System.Windows.Forms.Panel pnlLineRoom;
        private System.Windows.Forms.Label lblLineRoom;
        private System.Windows.Forms.Label lblLineRoomValue;
        private System.Windows.Forms.Panel pnlLineElec;
        private System.Windows.Forms.Label lblLineElec;
        private System.Windows.Forms.Label lblLineElecValue;
        private System.Windows.Forms.Label lblElecReading;
        private System.Windows.Forms.Panel pnlLineWater;
        private System.Windows.Forms.Label lblLineWater;
        private System.Windows.Forms.Label lblLineWaterValue;
        private System.Windows.Forms.Label lblWaterReading;
        private System.Windows.Forms.Panel pnlLineOther;
        private System.Windows.Forms.Label lblLineOther;
        private System.Windows.Forms.Label lblLineOtherValue;

        private System.Windows.Forms.Panel pnlReceiptTotal;
        private System.Windows.Forms.Panel pnlTotalRule;
        private System.Windows.Forms.Label lblTotalCaption;
        private System.Windows.Forms.Label lblTotalValue;
        private System.Windows.Forms.Label lblDueDate;

        private System.Windows.Forms.Panel pnlHistoryHeader;
        private System.Windows.Forms.Label lblHistoryTitle;
        private System.Windows.Forms.Label lblRowCount;
        private System.Windows.Forms.Panel pnlHistoryHost;
        private System.Windows.Forms.DataGridView grid;
        private ScreenTheme.CardPanel pnlEmptyState;
        private System.Windows.Forms.Label lblEmpty;

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
            this.pnlMain = new System.Windows.Forms.Panel();
            this.pnlProfile = new ScreenTheme.CardPanel();
            this.pnlAvatar = new ScreenTheme.CardPanel();
            this.lblAvatarInitials = new System.Windows.Forms.Label();
            this.lblTenantName = new System.Windows.Forms.Label();
            this.lblRoomTag = new ScreenTheme.TagLabel();
            this.lblTenantMeta = new System.Windows.Forms.Label();
            this.lblContractExpiry = new System.Windows.Forms.Label();
            this.lblReadOnly = new System.Windows.Forms.Label();
            this.pnlBanner = new ScreenTheme.CardPanel();
            this.pnlBannerIcon = new ScreenTheme.CardPanel();
            this.lblBannerIconText = new System.Windows.Forms.Label();
            this.lblBannerText = new System.Windows.Forms.Label();
            this.lblBannerTag = new ScreenTheme.TagLabel();
            this.pnlReceipt = new ScreenTheme.CardPanel();
            this.pnlReceiptHeader = new System.Windows.Forms.Panel();
            this.lblReceiptTitle = new System.Windows.Forms.Label();
            this.lblReceiptCode = new System.Windows.Forms.Label();
            this.lblReceiptStatus = new ScreenTheme.TagLabel();
            this.pnlReceiptBody = new System.Windows.Forms.Panel();
            this.pnlLineRoom = new System.Windows.Forms.Panel();
            this.lblLineRoom = new System.Windows.Forms.Label();
            this.lblLineRoomValue = new System.Windows.Forms.Label();
            this.pnlLineElec = new System.Windows.Forms.Panel();
            this.lblLineElec = new System.Windows.Forms.Label();
            this.lblLineElecValue = new System.Windows.Forms.Label();
            this.lblElecReading = new System.Windows.Forms.Label();
            this.pnlLineWater = new System.Windows.Forms.Panel();
            this.lblLineWater = new System.Windows.Forms.Label();
            this.lblLineWaterValue = new System.Windows.Forms.Label();
            this.lblWaterReading = new System.Windows.Forms.Label();
            this.pnlLineOther = new System.Windows.Forms.Panel();
            this.lblLineOther = new System.Windows.Forms.Label();
            this.lblLineOtherValue = new System.Windows.Forms.Label();
            this.pnlReceiptTotal = new System.Windows.Forms.Panel();
            this.pnlTotalRule = new System.Windows.Forms.Panel();
            this.lblTotalCaption = new System.Windows.Forms.Label();
            this.lblTotalValue = new System.Windows.Forms.Label();
            this.lblDueDate = new System.Windows.Forms.Label();
            this.pnlHistoryHeader = new System.Windows.Forms.Panel();
            this.lblHistoryTitle = new System.Windows.Forms.Label();
            this.lblRowCount = new System.Windows.Forms.Label();
            this.pnlHistoryHost = new System.Windows.Forms.Panel();
            this.grid = new System.Windows.Forms.DataGridView();
            this.pnlEmptyState = new ScreenTheme.CardPanel();
            this.lblEmpty = new System.Windows.Forms.Label();

            this.pnlMain.SuspendLayout();
            this.pnlProfile.SuspendLayout();
            this.pnlAvatar.SuspendLayout();
            this.pnlBanner.SuspendLayout();
            this.pnlBannerIcon.SuspendLayout();
            this.pnlReceipt.SuspendLayout();
            this.pnlReceiptHeader.SuspendLayout();
            this.pnlReceiptBody.SuspendLayout();
            this.pnlLineRoom.SuspendLayout();
            this.pnlLineElec.SuspendLayout();
            this.pnlLineWater.SuspendLayout();
            this.pnlLineOther.SuspendLayout();
            this.pnlReceiptTotal.SuspendLayout();
            this.pnlHistoryHeader.SuspendLayout();
            this.pnlHistoryHost.SuspendLayout();
            this.pnlEmptyState.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.SuspendLayout();

            // ---- pnlMain ----
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.BackColor = ScreenTheme.From(ScreenTheme.Base);
            this.pnlMain.Padding = new System.Windows.Forms.Padding(12);
            // Thứ tự Add quyết định layout dock: Fill trước, rồi từ dưới lên trên
            // để thứ tự thị giác là Hồ sơ → Banner → Empty → Bảng kê → Lịch sử.
            this.pnlMain.Controls.Add(this.pnlHistoryHost);
            this.pnlMain.Controls.Add(this.pnlHistoryHeader);
            this.pnlMain.Controls.Add(this.pnlReceipt);
            this.pnlMain.Controls.Add(this.pnlEmptyState);
            this.pnlMain.Controls.Add(this.pnlBanner);
            this.pnlMain.Controls.Add(this.pnlProfile);

            // ---- profile card ----
            this.pnlProfile.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlProfile.Height = 62;
            this.pnlProfile.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.pnlProfile.Controls.Add(this.pnlAvatar);
            this.pnlProfile.Controls.Add(this.lblTenantName);
            this.pnlProfile.Controls.Add(this.lblRoomTag);
            this.pnlProfile.Controls.Add(this.lblTenantMeta);
            this.pnlProfile.Controls.Add(this.lblContractExpiry);
            this.pnlProfile.Controls.Add(this.lblReadOnly);

            this.pnlAvatar.FillHex = ScreenTheme.Panel;
            this.pnlAvatar.BorderHex = ScreenTheme.AvatarBorder;
            this.pnlAvatar.Radius = 4;
            this.pnlAvatar.Location = new System.Drawing.Point(13, 13);
            this.pnlAvatar.Size = new System.Drawing.Size(36, 36);
            this.pnlAvatar.BackColor = System.Drawing.Color.Transparent;
            this.pnlAvatar.Controls.Add(this.lblAvatarInitials);

            this.lblAvatarInitials.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAvatarInitials.Text = "—";
            this.lblAvatarInitials.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblAvatarInitials.Font = ScreenTheme.Mono;
            this.lblAvatarInitials.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
            this.lblAvatarInitials.BackColor = System.Drawing.Color.Transparent;

            this.lblTenantName.AutoSize = true;
            this.lblTenantName.Location = new System.Drawing.Point(60, 12);
            this.lblTenantName.Text = "Người thuê";
            this.lblTenantName.Font = ScreenTheme.SectionTitle;
            this.lblTenantName.ForeColor = ScreenTheme.From(ScreenTheme.CreamLight);
            this.lblTenantName.BackColor = System.Drawing.Color.Transparent;

            this.lblRoomTag.Location = new System.Drawing.Point(60, 34);
            this.lblRoomTag.Size = new System.Drawing.Size(66, 20);
            this.lblRoomTag.Text = "P.—";
            this.lblRoomTag.AccessibleName = "Phòng đang thuê";

            this.lblTenantMeta.AutoSize = true;
            this.lblTenantMeta.Location = new System.Drawing.Point(134, 36);
            this.lblTenantMeta.Text = "Khu trọ Ngũ Hành Sơn";
            this.lblTenantMeta.Font = ScreenTheme.Body;
            this.lblTenantMeta.ForeColor = ScreenTheme.From(ScreenTheme.Dim);
            this.lblTenantMeta.BackColor = System.Drawing.Color.Transparent;

            this.lblContractExpiry.AutoSize = false;
            this.lblContractExpiry.Location = new System.Drawing.Point(400, 13);
            this.lblContractExpiry.Size = new System.Drawing.Size(588, 18);
            this.lblContractExpiry.Text = "Hạn HĐ: —";
            this.lblContractExpiry.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblContractExpiry.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.lblContractExpiry.Font = ScreenTheme.Body;
            this.lblContractExpiry.ForeColor = ScreenTheme.From(ScreenTheme.Muted);
            this.lblContractExpiry.BackColor = System.Drawing.Color.Transparent;

            this.lblReadOnly.AutoSize = false;
            this.lblReadOnly.Location = new System.Drawing.Point(400, 32);
            this.lblReadOnly.Size = new System.Drawing.Size(588, 18);
            this.lblReadOnly.Text = "Chỉ xem (ReadOnly)";
            this.lblReadOnly.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblReadOnly.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.lblReadOnly.Font = ScreenTheme.Body;
            this.lblReadOnly.ForeColor = ScreenTheme.From(ScreenTheme.Dim);
            this.lblReadOnly.BackColor = System.Drawing.Color.Transparent;

            // ---- overdue banner: #1F1715 + viền rgba(217,93,57,.40) + chữ #FFB4AB (DESIGN.md §2.4)
            this.pnlBanner.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlBanner.Height = 48;
            this.pnlBanner.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.pnlBanner.FillHex = ScreenTheme.ErrorBg;
            this.pnlBanner.BorderHex = ScreenTheme.ErrorBorder;
            this.pnlBanner.Radius = 8;
            this.pnlBanner.Visible = false;
            this.pnlBanner.Controls.Add(this.pnlBannerIcon);
            this.pnlBanner.Controls.Add(this.lblBannerText);
            this.pnlBanner.Controls.Add(this.lblBannerTag);

            this.pnlBannerIcon.FillHex = ScreenTheme.ErrorBgSoft;
            this.pnlBannerIcon.BorderHex = ScreenTheme.ErrorBorderSoft;
            this.pnlBannerIcon.Radius = 4;
            this.pnlBannerIcon.Location = new System.Drawing.Point(11, 11);
            this.pnlBannerIcon.Size = new System.Drawing.Size(26, 26);
            this.pnlBannerIcon.BackColor = System.Drawing.Color.Transparent;
            this.pnlBannerIcon.Controls.Add(this.lblBannerIconText);

            this.lblBannerIconText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblBannerIconText.Text = "!";
            this.lblBannerIconText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblBannerIconText.Font = ScreenTheme.SectionTitle;
            this.lblBannerIconText.ForeColor = ScreenTheme.From(ScreenTheme.Terracotta);
            this.lblBannerIconText.BackColor = System.Drawing.Color.Transparent;

            this.lblBannerText.AutoSize = false;
            this.lblBannerText.Location = new System.Drawing.Point(48, 8);
            this.lblBannerText.Size = new System.Drawing.Size(820, 32);
            this.lblBannerText.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.lblBannerText.Text = "Kỳ cước chưa hoàn tất thanh toán.";
            this.lblBannerText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblBannerText.Font = ScreenTheme.Body;
            this.lblBannerText.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
            this.lblBannerText.BackColor = System.Drawing.Color.Transparent;

            this.lblBannerTag.Location = new System.Drawing.Point(890, 13);
            this.lblBannerTag.Size = new System.Drawing.Size(88, 22);
            this.lblBannerTag.Text = "Quá hạn";
            this.lblBannerTag.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.lblBannerTag.SetRole(ScreenTheme.Error, ScreenTheme.ErrorBgSoft, ScreenTheme.ErrorBorderSoft);
            this.lblBannerTag.AccessibleName = "Trạng thái quá hạn";

            // ---- receipt card ----
            this.pnlReceipt.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlReceipt.Height = 336;
            this.pnlReceipt.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.pnlReceipt.Controls.Add(this.pnlReceiptBody);
            this.pnlReceipt.Controls.Add(this.pnlReceiptTotal);
            this.pnlReceipt.Controls.Add(this.pnlReceiptHeader);

            this.pnlReceiptHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlReceiptHeader.Height = 44;
            this.pnlReceiptHeader.BackColor = ScreenTheme.From(ScreenTheme.Panel);
            this.pnlReceiptHeader.Controls.Add(this.lblReceiptTitle);
            this.pnlReceiptHeader.Controls.Add(this.lblReceiptCode);
            this.pnlReceiptHeader.Controls.Add(this.lblReceiptStatus);

            this.lblReceiptTitle.AutoSize = true;
            this.lblReceiptTitle.Location = new System.Drawing.Point(12, 6);
            this.lblReceiptTitle.Text = "CHI TIẾT QUYẾT TOÁN CƯỚC";
            this.lblReceiptTitle.Font = ScreenTheme.HeaderFont;
            this.lblReceiptTitle.ForeColor = ScreenTheme.From(ScreenTheme.CreamLight);
            this.lblReceiptTitle.BackColor = System.Drawing.Color.Transparent;

            this.lblReceiptCode.AutoSize = true;
            this.lblReceiptCode.Location = new System.Drawing.Point(12, 24);
            this.lblReceiptCode.Text = "MÃ SỐ BẢNG KÊ: —";
            this.lblReceiptCode.Font = ScreenTheme.Mono;
            this.lblReceiptCode.ForeColor = ScreenTheme.From(ScreenTheme.Dim);
            this.lblReceiptCode.BackColor = System.Drawing.Color.Transparent;

            this.lblReceiptStatus.Location = new System.Drawing.Point(870, 11);
            this.lblReceiptStatus.Size = new System.Drawing.Size(118, 22);
            this.lblReceiptStatus.Text = "Chưa thanh toán";
            this.lblReceiptStatus.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.lblReceiptStatus.SetRole(ScreenTheme.Terracotta, ScreenTheme.ErrorBgSoft, ScreenTheme.ErrorBorderSoft);
            this.lblReceiptStatus.AccessibleName = "Trạng thái hóa đơn";

            this.pnlReceiptBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlReceiptBody.BackColor = ScreenTheme.From(ScreenTheme.Card);
            this.pnlReceiptBody.Controls.Add(this.pnlLineOther);
            this.pnlReceiptBody.Controls.Add(this.pnlLineWater);
            this.pnlReceiptBody.Controls.Add(this.pnlLineElec);
            this.pnlReceiptBody.Controls.Add(this.pnlLineRoom);

            // line: room
            this.pnlLineRoom.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLineRoom.Height = 34;
            this.pnlLineRoom.BackColor = ScreenTheme.From(ScreenTheme.Card);
            this.pnlLineRoom.Paint += new System.Windows.Forms.PaintEventHandler(this.LineRule_Paint);
            this.pnlLineRoom.Controls.Add(this.lblLineRoom);
            this.pnlLineRoom.Controls.Add(this.lblLineRoomValue);

            this.lblLineRoom.AutoSize = true;
            this.lblLineRoom.Location = new System.Drawing.Point(12, 9);
            this.lblLineRoom.Text = "Tiền thuê phòng cố định";
            this.lblLineRoom.Font = ScreenTheme.Body;
            this.lblLineRoom.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
            this.lblLineRoom.BackColor = System.Drawing.Color.Transparent;

            this.lblLineRoomValue.AutoSize = false;
            this.lblLineRoomValue.Location = new System.Drawing.Point(760, 9);
            this.lblLineRoomValue.Size = new System.Drawing.Size(228, 18);
            this.lblLineRoomValue.Text = "0 đ";
            this.lblLineRoomValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblLineRoomValue.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.lblLineRoomValue.Font = ScreenTheme.Mono;
            this.lblLineRoomValue.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
            this.lblLineRoomValue.BackColor = System.Drawing.Color.Transparent;

            // line: electricity (có dòng phụ chỉ số)
            this.pnlLineElec.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLineElec.Height = 52;
            this.pnlLineElec.BackColor = ScreenTheme.From(ScreenTheme.Card);
            this.pnlLineElec.Paint += new System.Windows.Forms.PaintEventHandler(this.LineRule_Paint);
            this.pnlLineElec.Controls.Add(this.lblLineElec);
            this.pnlLineElec.Controls.Add(this.lblLineElecValue);
            this.pnlLineElec.Controls.Add(this.lblElecReading);

            this.lblLineElec.AutoSize = true;
            this.lblLineElec.Location = new System.Drawing.Point(12, 8);
            this.lblLineElec.Text = "Điện năng tiêu thụ";
            this.lblLineElec.Font = ScreenTheme.Body;
            this.lblLineElec.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
            this.lblLineElec.BackColor = System.Drawing.Color.Transparent;

            this.lblLineElecValue.AutoSize = false;
            this.lblLineElecValue.Location = new System.Drawing.Point(760, 8);
            this.lblLineElecValue.Size = new System.Drawing.Size(228, 18);
            this.lblLineElecValue.Text = "0 đ";
            this.lblLineElecValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblLineElecValue.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.lblLineElecValue.Font = ScreenTheme.Mono;
            this.lblLineElecValue.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
            this.lblLineElecValue.BackColor = System.Drawing.Color.Transparent;

            this.lblElecReading.AutoSize = true;
            this.lblElecReading.Location = new System.Drawing.Point(24, 28);
            this.lblElecReading.Text = "Chỉ số: —";
            this.lblElecReading.Font = ScreenTheme.Body;
            this.lblElecReading.ForeColor = ScreenTheme.From(ScreenTheme.Dim);
            this.lblElecReading.BackColor = System.Drawing.Color.Transparent;

            // line: water
            this.pnlLineWater.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLineWater.Height = 52;
            this.pnlLineWater.BackColor = ScreenTheme.From(ScreenTheme.Card);
            this.pnlLineWater.Paint += new System.Windows.Forms.PaintEventHandler(this.LineRule_Paint);
            this.pnlLineWater.Controls.Add(this.lblLineWater);
            this.pnlLineWater.Controls.Add(this.lblLineWaterValue);
            this.pnlLineWater.Controls.Add(this.lblWaterReading);

            this.lblLineWater.AutoSize = true;
            this.lblLineWater.Location = new System.Drawing.Point(12, 8);
            this.lblLineWater.Text = "Nước sinh hoạt";
            this.lblLineWater.Font = ScreenTheme.Body;
            this.lblLineWater.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
            this.lblLineWater.BackColor = System.Drawing.Color.Transparent;

            this.lblLineWaterValue.AutoSize = false;
            this.lblLineWaterValue.Location = new System.Drawing.Point(760, 8);
            this.lblLineWaterValue.Size = new System.Drawing.Size(228, 18);
            this.lblLineWaterValue.Text = "0 đ";
            this.lblLineWaterValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblLineWaterValue.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.lblLineWaterValue.Font = ScreenTheme.Mono;
            this.lblLineWaterValue.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
            this.lblLineWaterValue.BackColor = System.Drawing.Color.Transparent;

            this.lblWaterReading.AutoSize = true;
            this.lblWaterReading.Location = new System.Drawing.Point(24, 28);
            this.lblWaterReading.Text = "Khối lượng: —";
            this.lblWaterReading.Font = ScreenTheme.Body;
            this.lblWaterReading.ForeColor = ScreenTheme.From(ScreenTheme.Dim);
            this.lblWaterReading.BackColor = System.Drawing.Color.Transparent;

            // line: other
            this.pnlLineOther.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLineOther.Height = 34;
            this.pnlLineOther.BackColor = ScreenTheme.From(ScreenTheme.Card);
            this.pnlLineOther.Controls.Add(this.lblLineOther);
            this.pnlLineOther.Controls.Add(this.lblLineOtherValue);

            this.lblLineOther.AutoSize = true;
            this.lblLineOther.Location = new System.Drawing.Point(12, 9);
            this.lblLineOther.Text = "Dịch vụ quản lý & tiện ích";
            this.lblLineOther.Font = ScreenTheme.Body;
            this.lblLineOther.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
            this.lblLineOther.BackColor = System.Drawing.Color.Transparent;

            this.lblLineOtherValue.AutoSize = false;
            this.lblLineOtherValue.Location = new System.Drawing.Point(760, 9);
            this.lblLineOtherValue.Size = new System.Drawing.Size(228, 18);
            this.lblLineOtherValue.Text = "0 đ";
            this.lblLineOtherValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblLineOtherValue.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.lblLineOtherValue.Font = ScreenTheme.Mono;
            this.lblLineOtherValue.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
            this.lblLineOtherValue.BackColor = System.Drawing.Color.Transparent;

            // ---- total block (border-emphasis-top 2px - DESIGN.md §2.2) ----
            this.pnlReceiptTotal.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlReceiptTotal.Height = 64;
            this.pnlReceiptTotal.BackColor = ScreenTheme.From(ScreenTheme.HeaderStrip);
            this.pnlReceiptTotal.Controls.Add(this.pnlTotalRule);
            this.pnlReceiptTotal.Controls.Add(this.lblTotalCaption);
            this.pnlReceiptTotal.Controls.Add(this.lblTotalValue);
            this.pnlReceiptTotal.Controls.Add(this.lblDueDate);

            this.pnlTotalRule.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTotalRule.Height = 2;
            this.pnlTotalRule.BackColor = ScreenTheme.From(ScreenTheme.EmphasisTop);

            this.lblTotalCaption.AutoSize = true;
            this.lblTotalCaption.Location = new System.Drawing.Point(13, 12);
            this.lblTotalCaption.Text = "TỔNG CẦN THANH TOÁN";
            this.lblTotalCaption.Font = ScreenTheme.HeaderFont;
            this.lblTotalCaption.ForeColor = ScreenTheme.From(ScreenTheme.Muted);
            this.lblTotalCaption.BackColor = System.Drawing.Color.Transparent;

            this.lblTotalValue.AutoSize = true;
            this.lblTotalValue.Location = new System.Drawing.Point(11, 30);
            this.lblTotalValue.Text = "0 đ";
            this.lblTotalValue.Font = ScreenTheme.TotalValue;
            this.lblTotalValue.ForeColor = ScreenTheme.From(ScreenTheme.CreamLight);
            this.lblTotalValue.BackColor = System.Drawing.Color.Transparent;
            this.lblTotalValue.AccessibleName = "Tổng cần thanh toán";

            this.lblDueDate.AutoSize = false;
            this.lblDueDate.Location = new System.Drawing.Point(700, 26);
            this.lblDueDate.Size = new System.Drawing.Size(288, 22);
            this.lblDueDate.Text = "Hạn chót: —";
            this.lblDueDate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblDueDate.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.lblDueDate.Font = ScreenTheme.Body;
            this.lblDueDate.ForeColor = ScreenTheme.From(ScreenTheme.Error);
            this.lblDueDate.BackColor = System.Drawing.Color.Transparent;

            // ---- history ----
            this.pnlHistoryHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHistoryHeader.Height = 34;
            this.pnlHistoryHeader.BackColor = ScreenTheme.From(ScreenTheme.Panel);
            this.pnlHistoryHeader.Controls.Add(this.lblHistoryTitle);
            this.pnlHistoryHeader.Controls.Add(this.lblRowCount);

            this.lblHistoryTitle.AutoSize = true;
            this.lblHistoryTitle.Location = new System.Drawing.Point(12, 9);
            this.lblHistoryTitle.Text = "LỊCH SỬ THANH TOÁN";
            this.lblHistoryTitle.Font = ScreenTheme.HeaderFont;
            this.lblHistoryTitle.ForeColor = ScreenTheme.From(ScreenTheme.Muted);
            this.lblHistoryTitle.BackColor = System.Drawing.Color.Transparent;

            this.lblRowCount.AutoSize = false;
            this.lblRowCount.Location = new System.Drawing.Point(880, 9);
            this.lblRowCount.Size = new System.Drawing.Size(108, 16);
            this.lblRowCount.Text = "0 bản ghi";
            this.lblRowCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblRowCount.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.lblRowCount.Font = ScreenTheme.Body;
            this.lblRowCount.ForeColor = ScreenTheme.From(ScreenTheme.Dim);
            this.lblRowCount.BackColor = System.Drawing.Color.Transparent;

            this.pnlHistoryHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlHistoryHost.BackColor = ScreenTheme.From(ScreenTheme.Base);
            this.pnlHistoryHost.Controls.Add(this.grid);

            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.TabIndex = 0;
            this.grid.AccessibleName = "Bảng lịch sử thanh toán";
            this.grid.Name = "grid";

            this.pnlEmptyState.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlEmptyState.Height = 120;
            this.pnlEmptyState.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.pnlEmptyState.Visible = false;
            this.pnlEmptyState.Controls.Add(this.lblEmpty);

            this.lblEmpty.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblEmpty.Text = "Chưa có hóa đơn nào cho phòng của bạn.";
            this.lblEmpty.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblEmpty.Font = ScreenTheme.Body;
            this.lblEmpty.ForeColor = ScreenTheme.From(ScreenTheme.Dim);
            this.lblEmpty.BackColor = System.Drawing.Color.Transparent;

            // ---- MyInvoicesForm ----
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = ScreenTheme.From(ScreenTheme.Base);
            this.Controls.Add(this.pnlMain);
            this.Font = ScreenTheme.Body;
            this.MinimumSize = new System.Drawing.Size(720, 420);
            this.Name = "MyInvoicesForm";
            this.Padding = new System.Windows.Forms.Padding(0);
            this.Size = new System.Drawing.Size(1040, 680);
            this.AccessibleName = "Màn hình hóa đơn của tôi (người thuê)";

            this.pnlMain.ResumeLayout(false);
            this.pnlProfile.ResumeLayout(false);
            this.pnlProfile.PerformLayout();
            this.pnlAvatar.ResumeLayout(false);
            this.pnlBanner.ResumeLayout(false);
            this.pnlBannerIcon.ResumeLayout(false);
            this.pnlReceipt.ResumeLayout(false);
            this.pnlReceiptHeader.ResumeLayout(false);
            this.pnlReceiptHeader.PerformLayout();
            this.pnlReceiptBody.ResumeLayout(false);
            this.pnlLineRoom.ResumeLayout(false);
            this.pnlLineRoom.PerformLayout();
            this.pnlLineElec.ResumeLayout(false);
            this.pnlLineElec.PerformLayout();
            this.pnlLineWater.ResumeLayout(false);
            this.pnlLineWater.PerformLayout();
            this.pnlLineOther.ResumeLayout(false);
            this.pnlLineOther.PerformLayout();
            this.pnlReceiptTotal.ResumeLayout(false);
            this.pnlReceiptTotal.PerformLayout();
            this.pnlHistoryHeader.ResumeLayout(false);
            this.pnlHistoryHeader.PerformLayout();
            this.pnlHistoryHost.ResumeLayout(false);
            this.pnlEmptyState.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion
    }
}
