using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net.Sockets;
using System.Windows.Forms;
using QuanLyTro.Network;
using QuanLyTro.Forms;
using QuanLyTro.Protocol;
using QuanLyTro.Shared.Models;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro
{
    /// <summary>
    /// Shell ứng dụng: title bar, menu, vùng đăng nhập, TabControl theo vai, status strip.
    /// Nội dung từng tab thuộc Task 11.
    /// </summary>
    public partial class Form1 : Form
    {
        // Token từ DESIGN.md §2 — nguồn duy nhất cho màu sắc.
        private static readonly Color SurfaceCanvas = ColorTranslator.FromHtml("#0B0F12");
        private static readonly Color SurfaceBase = ColorTranslator.FromHtml("#101417");
        private static readonly Color SurfaceCard = ColorTranslator.FromHtml("#181C1F");
        private static readonly Color SurfaceField = ColorTranslator.FromHtml("#15191D");
        private static readonly Color SurfaceActive = ColorTranslator.FromHtml("#21262B");
        private static readonly Color SurfaceHeader = ColorTranslator.FromHtml("#14191D");
        private static readonly Color SurfaceStatus = ColorTranslator.FromHtml("#0B0E11");
        private static readonly Color BorderHairline = ColorTranslator.FromHtml("#262A2E");
        private static readonly Color BorderInput = ColorTranslator.FromHtml("#2C3237");
        private static readonly Color BorderSegmented = ColorTranslator.FromHtml("#3D4349");
        private static readonly Color TextCream = ColorTranslator.FromHtml("#F4EFEA");
        private static readonly Color TextNeutral = ColorTranslator.FromHtml("#CAC6C1");
        private static readonly Color TextMuted = ColorTranslator.FromHtml("#A89988");
        private static readonly Color TextDim = ColorTranslator.FromHtml("#767E88");
        private static readonly Color Primary = ColorTranslator.FromHtml("#D95D39");
        private static readonly Color PrimaryHover = ColorTranslator.FromHtml("#EA6944");
        private static readonly Color StatusError = ColorTranslator.FromHtml("#FFB4AB");

        private static readonly string[] LandlordTabs =
        {
            "Tổng quan", "Phòng", "Người thuê", "Hợp đồng", "Điện nước", "Hóa đơn", "Thống kê",
        };

        private static readonly string[] TenantTabs = { "Hóa Đơn Của Tôi" };

        /// <summary>Singleton client cho các màn UserControl của Task 11 gọi tới.</summary>
        public static TcpClientService Client { get; } = new();

        private readonly TcpClientService _client = Client;
        private UserRole _selectedRole = UserRole.Landlord;
        private UserRole? _currentRole;
        private int? _roomCount;

        public Form1()
        {
            InitializeComponent();
            ApplyDesignTokens();
            WireEvents();
            _client.Disconnected += OnClientDisconnected;
        }

        /// <summary>Áp token thiết kế — không hardcode màu mới ngoài DESIGN.md.</summary>
        private void ApplyDesignTokens()
        {
            BackColor = SurfaceBase;
            ForeColor = TextCream;
            Font = new Font("Segoe UI", 9F);

            pnlTitleBar.BackColor = SurfaceCanvas;
            lblTitleIcon.ForeColor = Primary;
            lblTitleText.ForeColor = TextCream;

            menuStrip.BackColor = SurfaceHeader;
            menuStrip.ForeColor = TextNeutral;
            menuStrip.Renderer = new FlatMenuRenderer();
            menuStrip.Padding = new Padding(8, 0, 0, 0);

            statusStrip.BackColor = SurfaceStatus;
            statusStrip.ForeColor = TextDim;
            lblStatusRole.ForeColor = TextNeutral;
            lblStatusRuntime.ForeColor = TextDim;

            pnlContent.BackColor = SurfaceBase;

            pnlLoginCard.BackColor = SurfaceCard;
            lblLoginTitle.ForeColor = TextCream;
            lblLoginTitle.Text = "Ký Túc Xá & Nhà Trọ Số";
            lblLoginSubtitle.ForeColor = TextDim;
            lblLoginSubtitle.Text = "PHƯỜNG NGŨ HÀNH SƠN · TP. ĐÀ NẴNG";

            pnlRoleSegment.BackColor = SurfaceField;
            StyleRoleButton(btnRoleLandlord, selected: true);
            StyleRoleButton(btnRoleTenant, selected: false);

            lblAccount.ForeColor = TextMuted;
            lblPassword.ForeColor = TextMuted;
            StyleField(txtAccount);
            StyleField(txtPassword);

            btnLogin.BackColor = Primary;
            btnLogin.ForeColor = ColorTranslator.FromHtml("#FFFFFF");
            btnLogin.FlatAppearance.BorderColor = Primary;
            btnLogin.FlatAppearance.MouseOverBackColor = PrimaryHover;
            btnLogin.Cursor = Cursors.Hand;

            lblLoginError.ForeColor = StatusError;
            lblLoginError.Text = string.Empty;

            tabControlMain.BackColor = SurfaceBase;
            tabControlMain.ForeColor = TextNeutral;
            tabControlMain.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControlMain.ItemSize = new Size(150, 32);
            tabControlMain.SizeMode = TabSizeMode.Fixed;
            tabControlMain.Padding = new Point(10, 4);
            tabControlMain.AccessibleName = "Khu vực chức năng theo vai trò";
            StyleUserbar();
            StyleHeadline();
            SetStatus("Chưa đăng nhập");
        }

        /// <summary>Userbar theo template .userbar — chỉ hiện sau khi đăng nhập.</summary>
        private void StyleUserbar()
        {
            pnlUserbar.BackColor = SurfaceHeader;
            lblUserIcon.ForeColor = TextMuted;
            lblUserIcon.Font = new Font("Segoe UI", 11F);
            lblUserName.ForeColor = TextCream;
            lblUserName.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            lblRoleBadge.FillHex = ScreenTheme.DangerTagBg;
            lblRoleBadge.BorderHex = ScreenTheme.DangerTagBorder;
            lblRoleBadge.ForeHex = ScreenTheme.Tint;
            lblRoleBadge.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);

            btnLogoutUser.BackColor = SurfaceHeader;
            btnLogoutUser.ForeColor = TextDim;
            btnLogoutUser.FlatAppearance.BorderSize = 0;
            btnLogoutUser.FlatAppearance.MouseOverBackColor = ColorTranslator.FromHtml(ScreenTheme.HoverBg);
            btnLogoutUser.Cursor = Cursors.Hand;
        }

        /// <summary>Headline theo template .headline — badge MODULE + tên màn + chip phiên bản.</summary>
        private void StyleHeadline()
        {
            pnlHeadline.BackColor = SurfaceBase;
            lblModuleBadge.FillHex = ScreenTheme.HeadlineBadgeBg;
            lblModuleBadge.BorderHex = ScreenTheme.HeadlineBadgeBorder;
            lblModuleBadge.ForeHex = ScreenTheme.Terracotta;
            lblModuleBadge.Font = new Font("Segoe UI", 7F, FontStyle.Bold);
            lblModuleTitle.ForeColor = ColorTranslator.FromHtml(ScreenTheme.CreamLight);
            lblModuleTitle.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            lblVerChip.ForeColor = TextDim;
            lblVerChip.Font = new Font("Consolas", 8F);
        }

        private static void StyleField(TextBox box)
        {
            box.BackColor = SurfaceField;
            box.ForeColor = TextCream;
        }

        private void StyleRoleButton(Button button, bool selected)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = selected ? SurfaceActive : SurfaceField;
            button.ForeColor = selected ? TextCream : TextMuted;
            button.Font = new Font("Segoe UI", 9F, selected ? FontStyle.Bold : FontStyle.Regular);
            button.Cursor = Cursors.Hand;
        }

        private void WireEvents()
        {
            btnRoleLandlord.Click += (_, _) => SelectRole(UserRole.Landlord);
            btnRoleTenant.Click += (_, _) => SelectRole(UserRole.Tenant);
            btnLogin.Click += BtnLogin_Click;
            tabControlMain.DrawItem += TabControlMain_DrawItem;
            tabControlMain.SelectedIndexChanged += async (_, _) => await OnTabChangedAsync();
            menuItemDangXuat.Click += (_, _) => Logout();
            btnLogoutUser.Click += (_, _) => Logout();
            menuItemThoat.Click += (_, _) => Close();
            menuItemThongTin.Click += (_, _) => MessageBox.Show(
                this,
                "Hệ Thống Quản Lý Phòng Trọ Phường Ngũ Hành Sơn\n.NET 8.0 — WinForms Client",
                "Thông tin phần mềm",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        /// <summary>Segmented control chỉ đổi nhãn gợi ý; vai thật do server trả về trong LoginResult.</summary>
        private void SelectRole(UserRole role)
        {
            _selectedRole = role;
            StyleRoleButton(btnRoleLandlord, role == UserRole.Landlord);
            StyleRoleButton(btnRoleTenant, role == UserRole.Tenant);
            txtAccount.PlaceholderText = role == UserRole.Landlord
                ? "Tài khoản quản trị"
                : "Số CCCD";
        }

        private async void BtnLogin_Click(object? sender, EventArgs e)
        {
            var account = txtAccount.Text.Trim();
            var password = txtPassword.Text;
            if (account.Length == 0 || password.Length == 0)
            {
                ShowLoginError("Vui lòng nhập tài khoản và mật khẩu.");
                return;
            }

            btnLogin.Enabled = false;
            lblLoginError.Text = string.Empty;
            try
            {
                var (host, port) = ReadServerEndpoint();
                if (!_client.IsConnected)
                {
                    await _client.ConnectAsync(host, port);
                }

                var login = await _client.SendAsync<LoginRequest, LoginResult>(
                    ActionNames.AuthLogin, new LoginRequest(account, password));

                _client.Token = login.Token;
                EnterWorkspace(login);
            }
            catch (ClientRequestException ex)
            {
                // Message tiếng Việt từ BusinessRuleException — hiển thị nguyên văn, giữ nguyên input.
                ShowLoginError(ex.Message);
            }
            catch (Exception ex) when (ex is SocketException or System.IO.IOException or InvalidOperationException)
            {
                ShowLoginError("Không kết nối được tới máy chủ. Vui lòng kiểm tra máy chủ đã chạy chưa.");
            }
            finally
            {
                btnLogin.Enabled = true;
            }
        }

        /// <summary>Địa chỉ hạ tầng đọc từ App.config — người dùng cuối không cấu hình.</summary>
        private static (string Host, int Port) ReadServerEndpoint()
        {
            var host = ConfigurationManager.AppSettings["ServerHost"] ?? "127.0.0.1";
            var port = int.TryParse(ConfigurationManager.AppSettings["ServerPort"], out var parsed)
                ? parsed
                : 8888;
            return (host, port);
        }

        private void ShowLoginError(string message)
        {
            lblLoginError.Text = message;
        }

        /// <summary>Dựng tab theo vai do server trả về. Nội dung tab là Task 11.</summary>
        private void EnterWorkspace(LoginResult login)
        {
            _currentRole = login.Role;
            BuildTabs(login.Role);

            pnlLoginCard.Visible = false;
            pnlUserbar.Visible = true;
            pnlHeadline.Visible = true;
            lblUserName.Text = login.FullName;
            lblRoleBadge.Text = login.Role == UserRole.Landlord ? "CHỦ TRỌ" : "KHÁCH THUÊ";
            tabControlMain.Visible = true;
            tabControlMain.SelectedIndex = 0;
            UpdateHeadline();

            var roleText = login.Role == UserRole.Landlord
                ? "Chủ trọ (Landlord)"
                : "Khách thuê (Tenant)";
            SetStatus($"{login.FullName} • {roleText}");
            Text = $"Quản Lý Phòng Trọ — {roleText}";

            menuItemQuanLy.Visible = login.Role == UserRole.Landlord;
            menuItemBaoCao.Visible = login.Role == UserRole.Landlord;

            if (login.Role == UserRole.Landlord)
            {
                _ = LoadRoomCountAsync();
            }
        }

        /// <summary>Tiêu đề module theo tab — ánh xạ y hệt template JS (dòng 782-783).</summary>
        private void UpdateHeadline()
        {
            if (_currentRole == UserRole.Tenant)
            {
                lblModuleBadge.Text = "CHỨNG TỪ THU CƯỚC";
                lblModuleTitle.Text = "Hóa Đơn Của Tôi";
                return;
            }

            lblModuleBadge.Text = "MODULE";
            lblModuleTitle.Text = tabControlMain.SelectedIndex switch
            {
                0 => "Tổng quan vận hành cơ sở",
                1 => "Danh sách phòng trọ",
                2 => "Hồ sơ khách thuê",
                3 => "Hợp đồng & khách thuê",
                4 => "Chốt chỉ số điện nước",
                5 => "Hóa đơn & thu tiền",
                6 => "Thống kê doanh thu",
                _ => string.Empty,
            };
        }

        private void BuildTabs(UserRole role)
        {
            tabControlMain.TabPages.Clear();

            if (role == UserRole.Landlord)
            {
                AddTab(LandlordTabs[0], new DashboardForm { NavigationRequested = SelectTab });
                AddTab(LandlordTabs[1], new RoomsForm());
                AddTab(LandlordTabs[2], new TenantsForm());
                AddTab(LandlordTabs[3], new ContractsForm());
                AddTab(LandlordTabs[4], new UtilitiesForm());
                AddTab(LandlordTabs[5], new InvoicesForm());
                AddTab(LandlordTabs[6], new ReportsForm());
            }
            else
            {
                AddTab(TenantTabs[0], new MyInvoicesForm());
            }

            tabControlMain.SelectedIndex = 0;
            _ = OnTabChangedAsync();
        }

        private void AddTab(string title, UserControl content)
        {
            var page = new TabPage(title)
            {
                BackColor = SurfaceBase,
                ForeColor = TextNeutral,
                AccessibleName = $"Trang {title}",
                Padding = new Padding(0),
            };

            content.Dock = DockStyle.Fill;
            page.Controls.Add(content);
            tabControlMain.TabPages.Add(page);
        }

        private void SelectTab(int index)
        {
            if (index >= 0 && index < tabControlMain.TabPages.Count)
            {
                tabControlMain.SelectedIndex = index;
            }
        }

        /// <summary>Tổng quan gọi 3 action đúng một lần khi tab được mở (Sub-plan F Step 0).</summary>
        private async Task OnTabChangedAsync()
        {
            UpdateHeadline();
            if (tabControlMain.SelectedTab?.Controls.OfType<DashboardForm>().FirstOrDefault() is { } dashboard)
            {
                await dashboard.EnsureLoadedAsync();
            }
        }

        private void Logout()
        {
            _client.Disconnect();
            _currentRole = null;
            tabControlMain.TabPages.Clear();
            tabControlMain.Visible = false;
            pnlUserbar.Visible = false;
            pnlHeadline.Visible = false;
            lblUserName.Text = string.Empty;
            pnlLoginCard.Visible = true;
            txtPassword.Clear();
            lblLoginError.Text = string.Empty;
            SetStatus("Chưa đăng nhập");
            Text = "Quản Lý Phòng Trọ";
            menuItemQuanLy.Visible = true;
            menuItemBaoCao.Visible = true;
        }

        private void OnClientDisconnected(object? sender, EventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            BeginInvoke(() =>
            {
                if (_currentRole is not null)
                {
                    ShowLoginError("Mất kết nối tới máy chủ. Vui lòng đăng nhập lại.");
                }
                else
                {
                    ShowLoginError("Không kết nối được tới máy chủ. Vui lòng kiểm tra máy chủ đã chạy chưa.");
                }
            });
        }

        private void SetStatus(string roleText)
        {
            lblStatusRole.Text = roleText;
            lblStatusRuntime.Text = ".NET 8.0 CLR";
        }

        /// <summary>Tab dạng segmented theo template .tabs: bo 4px, tab chọn nền #252C33 + viền Terracotta.</summary>
        private void TabControlMain_DrawItem(object? sender, DrawItemEventArgs e)
        {
            var page = tabControlMain.TabPages[e.Index];
            var selected = e.Index == tabControlMain.SelectedIndex;
            var tab = new Rectangle(e.Bounds.X + 2, e.Bounds.Y + 2, e.Bounds.Width - 4, e.Bounds.Height - 4);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = ScreenTheme.RoundedRect(tab, 4))
            {
                using var brush = new SolidBrush(selected ? ScreenTheme.From(ScreenTheme.TabOnBg)
                    : SurfaceBase);
                e.Graphics.FillPath(brush, path);
                if (selected)
                {
                    using var pen = new Pen(ScreenTheme.From(ScreenTheme.DangerTagBorder));
                    e.Graphics.DrawPath(pen, path);
                }
            }

            var label = page.Text;
            if (_roomCount.HasValue && page.Text == LandlordTabs[1])
            {
                label = $"{page.Text}  ({_roomCount.Value})";
            }

            TextRenderer.DrawText(
                e.Graphics,
                label,
                Font,
                tab,
                selected ? ScreenTheme.From(ScreenTheme.CreamLight) : TextMuted,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        /// <summary>Số phòng cho badge trên tab — nạp một lần sau khi đăng nhập chủ trọ, lỗi thì bỏ badge.</summary>
        private async Task LoadRoomCountAsync()
        {
            try
            {
                var rooms = await _client.SendAsync<object, List<RoomDto>>(ActionNames.RoomGetAll, new { });
                _roomCount = rooms?.Count ?? 0;
                tabControlMain.Invalidate();
            }
            catch (Exception)
            {
                _roomCount = null;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _client.Disconnected -= OnClientDisconnected;
            _client.Dispose();
            base.OnFormClosed(e);
        }

        /// <summary>Renderer phẳng tối cho MenuStrip — không gradient, không viền sáng.</summary>
        private sealed class FlatMenuRenderer : ToolStripProfessionalRenderer
        {
            public FlatMenuRenderer() : base(new FlatColorTable()) { }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = e.Item.Selected ? TextCream : TextNeutral;
                base.OnRenderItemText(e);
            }
        }

        private sealed class FlatColorTable : ProfessionalColorTable
        {
            public override Color MenuStripGradientBegin => SurfaceHeader;
            public override Color MenuStripGradientEnd => SurfaceHeader;
            public override Color MenuItemSelected => SurfaceActive;
            public override Color MenuItemSelectedGradientBegin => SurfaceActive;
            public override Color MenuItemSelectedGradientEnd => SurfaceActive;
            public override Color MenuItemBorder => Primary;
            public override Color MenuItemPressedGradientBegin => SurfaceActive;
            public override Color MenuItemPressedGradientEnd => SurfaceActive;
            public override Color ToolStripDropDownBackground => SurfaceCard;
            public override Color ImageMarginGradientBegin => SurfaceCard;
            public override Color ImageMarginGradientMiddle => SurfaceCard;
            public override Color ImageMarginGradientEnd => SurfaceCard;
            public override Color SeparatorDark => BorderHairline;
            public override Color SeparatorLight => BorderHairline;
        }
    }
}
