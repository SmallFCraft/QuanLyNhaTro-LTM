namespace QuanLyTro;

partial class Form1
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.pnlTitleBar = new System.Windows.Forms.Panel();
        this.lblTitleIcon = new System.Windows.Forms.Label();
        this.lblTitleText = new System.Windows.Forms.Label();
        this.menuStrip = new System.Windows.Forms.MenuStrip();
        this.menuItemHeThong = new System.Windows.Forms.ToolStripMenuItem();
        this.menuItemDangXuat = new System.Windows.Forms.ToolStripMenuItem();
        this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
        this.menuItemThoat = new System.Windows.Forms.ToolStripMenuItem();
        this.menuItemQuanLy = new System.Windows.Forms.ToolStripMenuItem();
        this.menuItemBaoCao = new System.Windows.Forms.ToolStripMenuItem();
        this.menuItemTroGiup = new System.Windows.Forms.ToolStripMenuItem();
        this.menuItemThongTin = new System.Windows.Forms.ToolStripMenuItem();
        this.statusStrip = new System.Windows.Forms.StatusStrip();
        this.lblStatusRole = new System.Windows.Forms.ToolStripStatusLabel();
        this.lblStatusSpring = new System.Windows.Forms.ToolStripStatusLabel();
        this.lblStatusRuntime = new System.Windows.Forms.ToolStripStatusLabel();
        this.pnlContent = new System.Windows.Forms.Panel();
        this.pnlLoginCard = new System.Windows.Forms.Panel();
        this.lblLoginTitle = new System.Windows.Forms.Label();
        this.lblLoginSubtitle = new System.Windows.Forms.Label();
        this.pnlRoleSegment = new System.Windows.Forms.Panel();
        this.btnRoleLandlord = new System.Windows.Forms.Button();
        this.btnRoleTenant = new System.Windows.Forms.Button();
        this.lblAccount = new System.Windows.Forms.Label();
        this.txtAccount = new System.Windows.Forms.TextBox();
        this.lblPassword = new System.Windows.Forms.Label();
        this.txtPassword = new System.Windows.Forms.TextBox();
        this.btnLogin = new System.Windows.Forms.Button();
        this.lblLoginError = new System.Windows.Forms.Label();
        this.tabControlMain = new System.Windows.Forms.TabControl();
        this.pnlTitleBar.SuspendLayout();
        this.menuStrip.SuspendLayout();
        this.statusStrip.SuspendLayout();
        this.pnlContent.SuspendLayout();
        this.pnlLoginCard.SuspendLayout();
        this.pnlRoleSegment.SuspendLayout();
        this.SuspendLayout();
        //
        // pnlTitleBar
        //
        this.pnlTitleBar.Controls.Add(this.lblTitleIcon);
        this.pnlTitleBar.Controls.Add(this.lblTitleText);
        this.pnlTitleBar.Dock = System.Windows.Forms.DockStyle.Top;
        this.pnlTitleBar.Location = new System.Drawing.Point(0, 0);
        this.pnlTitleBar.Name = "pnlTitleBar";
        this.pnlTitleBar.Size = new System.Drawing.Size(1024, 32);
        this.pnlTitleBar.TabIndex = 0;
        //
        // lblTitleIcon
        //
        this.lblTitleIcon.Dock = System.Windows.Forms.DockStyle.Left;
        this.lblTitleIcon.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.lblTitleIcon.Location = new System.Drawing.Point(0, 0);
        this.lblTitleIcon.Name = "lblTitleIcon";
        this.lblTitleIcon.Size = new System.Drawing.Size(32, 32);
        this.lblTitleIcon.TabIndex = 0;
        this.lblTitleIcon.Text = "■";
        this.lblTitleIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // lblTitleText
        //
        this.lblTitleText.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lblTitleText.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.lblTitleText.Location = new System.Drawing.Point(32, 0);
        this.lblTitleText.Name = "lblTitleText";
        this.lblTitleText.Size = new System.Drawing.Size(992, 32);
        this.lblTitleText.TabIndex = 1;
        this.lblTitleText.Text = "Quản Lý Phòng Trọ Phường Ngũ Hành Sơn";
        this.lblTitleText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // menuStrip
        //
        this.menuStrip.AutoSize = false;
        this.menuStrip.Dock = System.Windows.Forms.DockStyle.Top;
        this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemHeThong,
            this.menuItemQuanLy,
            this.menuItemBaoCao,
            this.menuItemTroGiup});
        this.menuStrip.Location = new System.Drawing.Point(0, 32);
        this.menuStrip.Name = "menuStrip";
        this.menuStrip.Size = new System.Drawing.Size(1024, 36);
        this.menuStrip.TabIndex = 1;
        this.menuStrip.Text = "menuStrip";
        //
        // menuItemHeThong
        //
        this.menuItemHeThong.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemDangXuat,
            this.toolStripSeparator1,
            this.menuItemThoat});
        this.menuItemHeThong.Name = "menuItemHeThong";
        this.menuItemHeThong.Size = new System.Drawing.Size(69, 32);
        this.menuItemHeThong.Text = "Hệ thống";
        //
        // menuItemDangXuat
        //
        this.menuItemDangXuat.Name = "menuItemDangXuat";
        this.menuItemDangXuat.Size = new System.Drawing.Size(128, 22);
        this.menuItemDangXuat.Text = "Đăng xuất";
        //
        // toolStripSeparator1
        //
        this.toolStripSeparator1.Name = "toolStripSeparator1";
        this.toolStripSeparator1.Size = new System.Drawing.Size(125, 6);
        //
        // menuItemThoat
        //
        this.menuItemThoat.Name = "menuItemThoat";
        this.menuItemThoat.Size = new System.Drawing.Size(128, 22);
        this.menuItemThoat.Text = "Thoát";
        //
        // menuItemQuanLy
        //
        this.menuItemQuanLy.Name = "menuItemQuanLy";
        this.menuItemQuanLy.Size = new System.Drawing.Size(60, 32);
        this.menuItemQuanLy.Text = "Quản lý";
        //
        // menuItemBaoCao
        //
        this.menuItemBaoCao.Name = "menuItemBaoCao";
        this.menuItemBaoCao.Size = new System.Drawing.Size(61, 32);
        this.menuItemBaoCao.Text = "Báo cáo";
        //
        // menuItemTroGiup
        //
        this.menuItemTroGiup.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemThongTin});
        this.menuItemTroGiup.Name = "menuItemTroGiup";
        this.menuItemTroGiup.Size = new System.Drawing.Size(62, 32);
        this.menuItemTroGiup.Text = "Trợ giúp";
        //
        // menuItemThongTin
        //
        this.menuItemThongTin.Name = "menuItemThongTin";
        this.menuItemThongTin.Size = new System.Drawing.Size(175, 22);
        this.menuItemThongTin.Text = "Thông tin phần mềm";
        //
        // statusStrip
        //
        this.statusStrip.AutoSize = false;
        this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatusRole,
            this.lblStatusSpring,
            this.lblStatusRuntime});
        this.statusStrip.Location = new System.Drawing.Point(0, 618);
        this.statusStrip.Name = "statusStrip";
        this.statusStrip.Size = new System.Drawing.Size(1024, 22);
        this.statusStrip.SizingGrip = false;
        this.statusStrip.TabIndex = 2;
        this.statusStrip.Text = "statusStrip";
        //
        // lblStatusRole
        //
        this.lblStatusRole.Name = "lblStatusRole";
        this.lblStatusRole.Size = new System.Drawing.Size(105, 17);
        this.lblStatusRole.Text = "Chưa đăng nhập";
        //
        // lblStatusSpring
        //
        this.lblStatusSpring.Name = "lblStatusSpring";
        this.lblStatusSpring.Size = new System.Drawing.Size(816, 17);
        this.lblStatusSpring.Spring = true;
        //
        // lblStatusRuntime
        //
        this.lblStatusRuntime.Name = "lblStatusRuntime";
        this.lblStatusRuntime.Size = new System.Drawing.Size(88, 17);
        this.lblStatusRuntime.Text = ".NET 8.0 CLR";
        //
        // pnlContent
        //
        this.pnlContent.Controls.Add(this.pnlLoginCard);
        this.pnlContent.Controls.Add(this.tabControlMain);
        this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
        this.pnlContent.Location = new System.Drawing.Point(0, 68);
        this.pnlContent.Name = "pnlContent";
        this.pnlContent.Size = new System.Drawing.Size(1024, 550);
        this.pnlContent.TabIndex = 3;
        //
        // pnlLoginCard
        //
        this.pnlLoginCard.Anchor = System.Windows.Forms.AnchorStyles.None;
        this.pnlLoginCard.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.pnlLoginCard.Controls.Add(this.lblLoginTitle);
        this.pnlLoginCard.Controls.Add(this.lblLoginSubtitle);
        this.pnlLoginCard.Controls.Add(this.pnlRoleSegment);
        this.pnlLoginCard.Controls.Add(this.lblAccount);
        this.pnlLoginCard.Controls.Add(this.txtAccount);
        this.pnlLoginCard.Controls.Add(this.lblPassword);
        this.pnlLoginCard.Controls.Add(this.txtPassword);
        this.pnlLoginCard.Controls.Add(this.btnLogin);
        this.pnlLoginCard.Controls.Add(this.lblLoginError);
        this.pnlLoginCard.Location = new System.Drawing.Point(332, 75);
        this.pnlLoginCard.Name = "pnlLoginCard";
        this.pnlLoginCard.Size = new System.Drawing.Size(360, 390);
        this.pnlLoginCard.TabIndex = 0;
        //
        // lblLoginTitle
        //
        this.lblLoginTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.lblLoginTitle.Location = new System.Drawing.Point(24, 20);
        this.lblLoginTitle.Name = "lblLoginTitle";
        this.lblLoginTitle.Size = new System.Drawing.Size(312, 28);
        this.lblLoginTitle.TabIndex = 0;
        this.lblLoginTitle.Text = "ĐĂNG NHẬP HỆ THỐNG";
        this.lblLoginTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // lblLoginSubtitle
        //
        this.lblLoginSubtitle.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.lblLoginSubtitle.Location = new System.Drawing.Point(24, 48);
        this.lblLoginSubtitle.Name = "lblLoginSubtitle";
        this.lblLoginSubtitle.Size = new System.Drawing.Size(312, 20);
        this.lblLoginSubtitle.TabIndex = 1;
        this.lblLoginSubtitle.Text = "Chọn vai trò để tiếp tục làm việc";
        this.lblLoginSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // pnlRoleSegment
        //
        this.pnlRoleSegment.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.pnlRoleSegment.Controls.Add(this.btnRoleLandlord);
        this.pnlRoleSegment.Controls.Add(this.btnRoleTenant);
        this.pnlRoleSegment.Location = new System.Drawing.Point(24, 80);
        this.pnlRoleSegment.Name = "pnlRoleSegment";
        this.pnlRoleSegment.Size = new System.Drawing.Size(312, 34);
        this.pnlRoleSegment.TabIndex = 2;
        //
        // btnRoleLandlord
        //
        this.btnRoleLandlord.AccessibleName = "Chọn vai trò Chủ trọ";
        this.btnRoleLandlord.Dock = System.Windows.Forms.DockStyle.Left;
        this.btnRoleLandlord.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnRoleLandlord.Location = new System.Drawing.Point(0, 0);
        this.btnRoleLandlord.Name = "btnRoleLandlord";
        this.btnRoleLandlord.Size = new System.Drawing.Size(155, 32);
        this.btnRoleLandlord.TabIndex = 0;
        this.btnRoleLandlord.Text = "Chủ trọ";
        this.btnRoleLandlord.UseVisualStyleBackColor = true;
        //
        // btnRoleTenant
        //
        this.btnRoleTenant.AccessibleName = "Chọn vai trò Khách thuê";
        this.btnRoleTenant.Dock = System.Windows.Forms.DockStyle.Right;
        this.btnRoleTenant.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnRoleTenant.Location = new System.Drawing.Point(155, 0);
        this.btnRoleTenant.Name = "btnRoleTenant";
        this.btnRoleTenant.Size = new System.Drawing.Size(155, 32);
        this.btnRoleTenant.TabIndex = 1;
        this.btnRoleTenant.Text = "Khách thuê";
        this.btnRoleTenant.UseVisualStyleBackColor = true;
        //
        // lblAccount
        //
        this.lblAccount.AutoSize = true;
        this.lblAccount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.lblAccount.Location = new System.Drawing.Point(24, 130);
        this.lblAccount.Name = "lblAccount";
        this.lblAccount.Size = new System.Drawing.Size(57, 15);
        this.lblAccount.TabIndex = 3;
        this.lblAccount.Text = "Tài khoản";
        //
        // txtAccount
        //
        this.txtAccount.AccessibleName = "Tài khoản";
        this.txtAccount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.txtAccount.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.txtAccount.Location = new System.Drawing.Point(24, 150);
        this.txtAccount.Name = "txtAccount";
        this.txtAccount.Size = new System.Drawing.Size(312, 24);
        this.txtAccount.TabIndex = 4;
        //
        // lblPassword
        //
        this.lblPassword.AutoSize = true;
        this.lblPassword.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.lblPassword.Location = new System.Drawing.Point(24, 190);
        this.lblPassword.Name = "lblPassword";
        this.lblPassword.Size = new System.Drawing.Size(57, 15);
        this.lblPassword.TabIndex = 5;
        this.lblPassword.Text = "Mật khẩu";
        //
        // txtPassword
        //
        this.txtPassword.AccessibleName = "Mật khẩu";
        this.txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.txtPassword.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.txtPassword.Location = new System.Drawing.Point(24, 210);
        this.txtPassword.Name = "txtPassword";
        this.txtPassword.PasswordChar = '●';
        this.txtPassword.Size = new System.Drawing.Size(312, 24);
        this.txtPassword.TabIndex = 6;
        //
        // btnLogin
        //
        this.btnLogin.AccessibleName = "Đăng nhập";
        this.btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnLogin.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
        this.btnLogin.Location = new System.Drawing.Point(24, 260);
        this.btnLogin.Name = "btnLogin";
        this.btnLogin.Size = new System.Drawing.Size(312, 36);
        this.btnLogin.TabIndex = 7;
        this.btnLogin.Text = "Đăng nhập";
        this.btnLogin.UseVisualStyleBackColor = true;
        //
        // lblLoginError
        //
        this.lblLoginError.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.lblLoginError.Location = new System.Drawing.Point(24, 305);
        this.lblLoginError.Name = "lblLoginError";
        this.lblLoginError.Size = new System.Drawing.Size(312, 60);
        this.lblLoginError.TabIndex = 8;
        this.lblLoginError.TextAlign = System.Drawing.ContentAlignment.TopCenter;
        //
        // tabControlMain
        //
        this.tabControlMain.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tabControlMain.Location = new System.Drawing.Point(0, 0);
        this.tabControlMain.Name = "tabControlMain";
        this.tabControlMain.SelectedIndex = 0;
        this.tabControlMain.Size = new System.Drawing.Size(1024, 550);
        this.tabControlMain.TabIndex = 1;
        this.tabControlMain.Visible = false;
        //
        // Form1
        //
        this.AcceptButton = this.btnLogin;
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(1024, 640);
        this.Controls.Add(this.pnlContent);
        this.Controls.Add(this.statusStrip);
        this.Controls.Add(this.menuStrip);
        this.Controls.Add(this.pnlTitleBar);
        this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
        this.MainMenuStrip = this.menuStrip;
        this.MinimumSize = new System.Drawing.Size(960, 600);
        this.Name = "Form1";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Quản Lý Phòng Trọ";
        this.pnlTitleBar.ResumeLayout(false);
        this.menuStrip.ResumeLayout(false);
        this.menuStrip.PerformLayout();
        this.statusStrip.ResumeLayout(false);
        this.statusStrip.PerformLayout();
        this.pnlContent.ResumeLayout(false);
        this.pnlLoginCard.ResumeLayout(false);
        this.pnlLoginCard.PerformLayout();
        this.pnlRoleSegment.ResumeLayout(false);
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel pnlTitleBar;
    private System.Windows.Forms.Label lblTitleIcon;
    private System.Windows.Forms.Label lblTitleText;
    private System.Windows.Forms.MenuStrip menuStrip;
    private System.Windows.Forms.ToolStripMenuItem menuItemHeThong;
    private System.Windows.Forms.ToolStripMenuItem menuItemDangXuat;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    private System.Windows.Forms.ToolStripMenuItem menuItemThoat;
    private System.Windows.Forms.ToolStripMenuItem menuItemQuanLy;
    private System.Windows.Forms.ToolStripMenuItem menuItemBaoCao;
    private System.Windows.Forms.ToolStripMenuItem menuItemTroGiup;
    private System.Windows.Forms.ToolStripMenuItem menuItemThongTin;
    private System.Windows.Forms.StatusStrip statusStrip;
    private System.Windows.Forms.ToolStripStatusLabel lblStatusRole;
    private System.Windows.Forms.ToolStripStatusLabel lblStatusSpring;
    private System.Windows.Forms.ToolStripStatusLabel lblStatusRuntime;
    private System.Windows.Forms.Panel pnlContent;
    private System.Windows.Forms.Panel pnlLoginCard;
    private System.Windows.Forms.Label lblLoginTitle;
    private System.Windows.Forms.Label lblLoginSubtitle;
    private System.Windows.Forms.Panel pnlRoleSegment;
    private System.Windows.Forms.Button btnRoleLandlord;
    private System.Windows.Forms.Button btnRoleTenant;
    private System.Windows.Forms.Label lblAccount;
    private System.Windows.Forms.TextBox txtAccount;
    private System.Windows.Forms.Label lblPassword;
    private System.Windows.Forms.TextBox txtPassword;
    private System.Windows.Forms.Button btnLogin;
    private System.Windows.Forms.Label lblLoginError;
    private System.Windows.Forms.TabControl tabControlMain;
}
