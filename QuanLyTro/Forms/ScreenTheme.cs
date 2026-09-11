using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace QuanLyTro.Forms;


/// <summary>
/// Dùng chung màu / font / bo góc / vẽ card-tag của màn Hóa đơn · Thống kê · Hóa đơn của tôi.
/// Mọi giá trị lấy từ DESIGN.md — không hardcode màu ngoài đó.
/// </summary>
internal static class ScreenTheme
{
    // Surfaces (DESIGN.md §2.1)
    public const string Base = "#101417";        // surface-base
    public const string Card = "#181C1F";        // surface-card (KPI)
    public const string Panel = "#1A1F24";       // surface-panel (DataGrid, header khối)
    public const string Active = "#21262B";      // surface-active (dòng chọn)
    public const string Field = "#15191D";       // surface-field (input)
    public const string GridBg = "#14181C";      // DESIGN.md §5 DataGridView.BackgroundColor
    public const string HeaderBg = "#1A2025";    // DESIGN.md §3.2 header DataGrid
    public const string HoverBg = "#1F252A";     // surface-hover (nền nút Danger)
    public const string HeaderStrip = "#14191D"; // surface-header (khối tổng tiền)

    // Borders (DESIGN.md §2.2)
    public const string Hairline = "#262A2E";    // border-hairline
    public const string Subtle = "#21272C";      // border-subtle (đường kẻ hàng)
    public const string Input = "#2C3237";       // border-input
    public const string OutlineBtn = "#2E373F";  // border-outline-btn
    public const string DangerBtn = "#3E2925";   // viền nút Danger (§3.3)
    public const string EmphasisTop = "#2B333E"; // border-emphasis-top (khối tổng tiền)
    public const string RowAlt = "#14181C";      // DESIGN.md §3.2 nền xen kẽ
    public const string RowAlt2 = "#161B1F";
    public const string FootBg = "#12161A";       // template .tblfoot / .note / .calc input[disabled]
    public const string FootBorder = "#262D33";   // template .tblwrap / .tblfoot viền trên
    public const string ToolbarBg = "#161B1F";    // template .toolbar nền
    public const string ToolbarBorder = "#262D33";// template .toolbar viền
    public const string TabStripBg = "#181C20";   // template .tabs nền container
    public const string TabStripBorder = "#262D33";// template .tabs viền container
    public const string TabOnBg = "#252C33";      // template .tab.on nền
    public const string TabHoverBg = "#1E2429";   // template .tab:hover nền
    public const string MenuBtnBg = "#1C2126";    // template .menustrip .mi nền
    public const string MeterTrack = "#12161A";   // template .meter track
    public const string MeterBorder = "#2A3239";  // template .meter viền
    public const string ChartBarBg = "#262A2E";   // template .chartbar .b nền
    public const string ChartBarBorder = "#2E3338";// template .chartbar .b viền
    public const string ChartBarOnBg = "#4A2E24"; // rgba(217,93,57,.35) trên #181C1F
    public const string ChartBarOnBorder = "#7A4130"; // rgba(217,93,57,.60) trên #181C1F
    public const string ReceiptHeadBg = "#1A2027";// template khối header bảng kê
    public const string ReceiptFootBg = "#111519";// template khối tổng tiền bảng kê
    public const string AvatarBg = "#202730";     // template avatar ô chữ cái
    public const string HeadlineBadgeBg = "#2E241F";     // rgba(217,93,57,.15) trên #101417
    public const string HeadlineBadgeBorder = "#432A1E"; // rgba(217,93,57,.30) trên #101417
    public const string AvatarBorder = "#303945";// border-avatar
    public const string TagEmptyBorder = "#313A42"; // border-tag-empty
    public const string OutlineHover = "#3A444E";// border-strong (Outline hover)

    // Text (DESIGN.md §2.3)
    public const string Cream = "#F4EFEA";       // text-cream
    public const string CreamLight = "#FAF8F5";  // text-cream-light (tổng tiền, dòng chọn)
    public const string Neutral = "#CAC6C1";     // text-neutral (tag trống/chờ)
    public const string OnFill = "#FFFFFF";      // text-on-fill (nút Primary)
    public const string Muted = "#A89988";       // text-muted (nhãn phụ, header grid)
    public const string Dim = "#767E88";         // text-dim (tiêu đề cột)
    public const string Placeholder = "#555D66"; // text-placeholder

    // Primary (DESIGN.md §2.4)
    public const string Terracotta = "#D95D39";      // primary-terracotta
    public const string TerracottaHover = "#EA6944"; // primary-terracotta-hover
    public const string TerracottaActive = "#B84524";// primary-terracotta-active
    public const string Tint = "#FFB5A0";            // primary-tint

    // Semantic (DESIGN.md §2.4)
    public const string Sage = "#8BD7A3";        // status-sage (đã thu / OK)
    public const string SageBorder = "#2E4A35";  // status-sage-border
    public const string SageBg = "#29322F";      // rgba(139,215,163,.12) trên #181C1F
    public const string Amber = "#E0AF68";       // status-amber
    public const string AmberBorder = "#483A24"; // status-amber-border
    public const string AmberBg = "#2A2319";     // status-amber-bg
    public const string Error = "#FFB4AB";       // status-error (nợ / banner quá hạn)
    public const string ErrorBg = "#1F1715";     // status-error-surface (banner)
    public const string ErrorBorder = "#693323"; // rgba(217,93,57,.40) trên #1F1715
    public const string ErrorBgSoft = "#44251C"; // rgba(217,93,57,.20) trên #1F1715
    public const string ErrorBorderSoft = "#572C20"; // rgba(217,93,57,.30) trên #1F1715
    public const string DangerTagBg = "#2C2120";     // rgba(217,93,57,.12) trên nền hàng
    public const string DangerTagBorder = "#5A3127"; // rgba(217,93,57,.35) trên nền hàng
    public const string EmptyBg = "#1C2227";     // nền tag trống/chờ (§2.4)

    public static readonly Font Body = new("Segoe UI", 9f);
    public static readonly Font FootFont = new("Consolas", 8.25f);
    public static readonly Font Mono = new("Consolas", 9.5f);
    public static readonly Font HeaderFont = new("Segoe UI", 8.25f, FontStyle.Bold);
    public static readonly Font KpiValue = new("Consolas", 14f, FontStyle.Bold);
    public static readonly Font KpiLabel = new("Segoe UI", 8.25f, FontStyle.Bold);
    public static readonly Font SectionTitle = new("Segoe UI", 9.75f, FontStyle.Bold);
    public static readonly Font TotalValue = new("Consolas", 15f, FontStyle.Bold);

    public static Color From(string hex) => ColorTranslator.FromHtml(hex);

    public static string Money(decimal value) =>
        value.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " đ";

    public static string Day(DateTime? value) =>
        value?.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture) ?? "—";

    /// <summary>Bo góc theo DESIGN.md: 4px component, 6px KPI, 8px card. Không pill.</summary>
    public static GraphicsPath RoundedRect(Rectangle r, int radius)
    {
        var path = new GraphicsPath();
        var d = radius * 2;
        path.AddArc(r.X, r.Y, d, d, 180, 90);
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    /// <summary>Card bo góc có viền hairline — dùng cho panel nền tối đè lên surface-base.</summary>
    public static void PaintCard(Graphics g, Rectangle bounds, string fill, string border, int radius)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var r = new Rectangle(bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
        using var path = RoundedRect(r, radius);
        using var brush = new SolidBrush(From(fill));
        g.FillPath(brush, path);
        using var pen = new Pen(From(border));
        g.DrawPath(pen, path);
    }

    /// <summary>Tag trạng thái: bo 4px, chữ + nền + viền theo bảng ngữ nghĩa DESIGN.md §2.4.</summary>
    public static void PaintTag(Graphics g, Rectangle cell, string text, string fore, string back, string border)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var size = g.MeasureString(text, Body);
        var w = (int)size.Width + 18;
        var h = 22;
        var r = new Rectangle(
            cell.X + 6,
            cell.Y + (cell.Height - h) / 2,
            Math.Min(w, Math.Max(24, cell.Width - 12)),
            h);

        using (var path = RoundedRect(r, 4))
        {
            using var brush = new SolidBrush(From(back));
            g.FillPath(brush, path);
            using var pen = new Pen(From(border));
            g.DrawPath(pen, path);
        }

        TextRenderer.DrawText(g, text, Body, r, From(fore),
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    /// <summary>Nút vẽ trong DataGridView (cột Thu) — bo 4px, có trạng thái disabled.</summary>
    public static void PaintButton(Graphics g, Rectangle cell, string text, string back, string fore, string border)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var r = new Rectangle(cell.X + 6, cell.Y + 6, Math.Max(24, cell.Width - 12), cell.Height - 12);
        using (var path = RoundedRect(r, 4))
        {
            using var brush = new SolidBrush(From(back));
            g.FillPath(brush, path);
            using var pen = new Pen(From(border));
            g.DrawPath(pen, path);
        }

        TextRenderer.DrawText(g, text, Body, r, From(fore),
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    /// <summary>
    /// Card bo góc (mặc định 8px, KPI 6px) có viền hairline — DESIGN.md §1, §3.4.
    /// Con bên trong đặt BackColor = Color.Transparent sẽ thấy đúng nền card bo góc.
    /// </summary>
    public class CardPanel : Panel
    {
        public CardPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer, true);
            FillHex = Card;
            BorderHex = Hairline;
            Radius = 8;
            BackColor = From(Base);
        }

        public string FillHex { get; set; }

        public string BorderHex { get; set; }

        public int Radius { get; set; }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            ScreenTheme.PaintCard(e.Graphics, ClientRectangle, FillHex, BorderHex, Radius);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Nền đã vẽ ở OnPaintBackground; không gọi base để tránh tô đè hình chữ nhật.
        }
    }

    /// <summary>
    /// Tag trạng thái bo 4px vẽ trọn control — màu theo bảng ngữ nghĩa DESIGN.md §2.4.
    /// Đặt trên nền card đặc nên không cần trong suốt.
    /// </summary>
    public class TagLabel : Label
    {
        public TagLabel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer, true);
            AutoSize = false;
            TextAlign = ContentAlignment.MiddleCenter;
            Font = Body;
            FillHex = EmptyBg;
            BorderHex = TagEmptyBorder;
            ForeHex = Neutral;
            BackColor = From(Card);
        }

        public string FillHex { get; set; }

        public string BorderHex { get; set; }

        public string ForeHex { get; set; }

        public void SetRole(string fore, string back, string border)
        {
            ForeHex = fore;
            FillHex = back;
            BorderHex = border;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var r = new Rectangle(0, 0, Width - 1, Height - 1);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = RoundedRect(r, 4))
            {
                using var brush = new SolidBrush(From(FillHex));
                e.Graphics.FillPath(brush, path);
                using var pen = new Pen(From(BorderHex));
                e.Graphics.DrawPath(pen, path);
            }

            TextRenderer.DrawText(e.Graphics, Text, Body, r, From(ForeHex),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
    }
}

/// <summary>
/// Dải chân bảng theo template .tblfoot: nền #12161A, viền trên #262D33,
/// mono 10.5px — trái "Tổng: N bản ghi | Chọn: X" (phần chọn tô Terracotta), phải "Đã đồng bộ với Server".
/// </summary>
public class TblFootPanel : Panel
{
    private readonly Label _left = new();
    private readonly Label _right = new();

    public TblFootPanel()
    {
        BackColor = ScreenTheme.From(ScreenTheme.FootBg);
        Height = 28;
        Padding = new Padding(10, 0, 10, 0);

        _left.AutoSize = true;
        _left.Font = ScreenTheme.FootFont;
        _left.ForeColor = ScreenTheme.From(ScreenTheme.Muted);
        _left.Dock = DockStyle.Left;
        _left.TextAlign = ContentAlignment.MiddleLeft;
        _left.AccessibleName = "Tổng số bản ghi và dòng đang chọn";

        _right.AutoSize = true;
        _right.Font = ScreenTheme.FootFont;
        _right.ForeColor = ScreenTheme.From(ScreenTheme.Muted);
        _right.Dock = DockStyle.Right;
        _right.TextAlign = ContentAlignment.MiddleRight;
        _right.Text = "Đã đồng bộ với Server";

        Controls.Add(_right);
        Controls.Add(_left);
        SetTotal(0);
    }

    public void SetTotal(int count)
    {
        _total = count;
        Refresh_();
    }

    public void SetSelected(string? label)
    {
        _selected = label;
        Refresh_();
    }

    private int _total;
    private string? _selected;

    private void Refresh_()
    {
        var tail = _selected is null ? "Chọn: —" : $"Chọn: {_selected}";
        _left.Text = string.Concat("Tổng: ", _total.ToString(CultureInfo.InvariantCulture),
            " bản ghi  |  ", tail);
        _left.Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var pen = new Pen(ScreenTheme.From(ScreenTheme.FootBorder));
        e.Graphics.DrawLine(pen, 0, 0, Width, 0);
    }

    /// <summary>Phần "Chọn:" tô Terracotta như template .sel-txt.</summary>
    protected override void OnPaintBackground(PaintEventArgs e)
    {
        base.OnPaintBackground(e);
    }
}

/// <summary>Dải ghi chú theo template .note: nền #12161A, viền #21272C, chữ 11px #767E88.</summary>
public static class NoteBar
{
    public static Panel Create(string text, string? glyph = "\u2139")
    {
        var panel = new Panel
        {
            BackColor = ScreenTheme.From(ScreenTheme.FootBg),
            Height = 34,
            Padding = new Padding(11, 0, 11, 0),
            Margin = new Padding(0),
            AccessibleName = "Ghi chú nghiệp vụ",
        };

        var label = new Label
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 8.5f),
            ForeColor = ScreenTheme.From(ScreenTheme.Dim),
            TextAlign = ContentAlignment.MiddleLeft,
            Text = glyph is null ? text : $"{glyph}  {text}",
            AccessibleName = text,
        };

        panel.Controls.Add(label);
        panel.Paint += (_, e) =>
        {
            using var pen = new Pen(ScreenTheme.From(ScreenTheme.Subtle));
            e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
        };
        return panel;
    }
}

/// <summary>Thanh công cụ theo template .toolbar: nền #161B1F, viền #262D33, bo 8px, đệm 6px.</summary>
public class ToolbarPanel : Panel
{
    public ToolbarPanel()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
            | ControlStyles.OptimizedDoubleBuffer, true);
        FillHex = ScreenTheme.ToolbarBg;
        BorderHex = ScreenTheme.ToolbarBorder;
        Radius = 8;
        BackColor = ScreenTheme.From(ScreenTheme.Base);
        Padding = new Padding(6);
        Height = 44;
    }

    public string FillHex { get; set; }

    public string BorderHex { get; set; }

    public int Radius { get; set; }

    protected override void OnPaintBackground(PaintEventArgs e) =>
        ScreenTheme.PaintCard(e.Graphics, ClientRectangle, FillHex, BorderHex, Radius);

    protected override void OnPaint(PaintEventArgs e)
    {
    }
}

/// <summary>Tách chuỗi thành chữ in hoa có giãn ký tự — không có trong WinForms, dùng cho badge/nhãn.</summary>
public static class ScreenText
{
    public static string Upper(string value) => value.ToUpper(CultureInfo.GetCultureInfo("vi-VN"));
}
