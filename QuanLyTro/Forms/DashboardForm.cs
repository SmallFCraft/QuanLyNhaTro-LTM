using System.Drawing.Drawing2D;
using System.Globalization;
using QuanLyTro.Shared.Models;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Forms;

/// <summary>
/// Tab Tổng quan (Sub-Plan F — Step 0).
/// Gọi REPORT_SUMMARY + CONTRACT_GET_ALL + INVOICE_GET_ALL đúng MỘT lần khi mở tab:
/// không timer, không vòng lặp lặp lại. Hai bảng dưới dùng lại chính dữ liệu đó.
/// </summary>
public partial class DashboardForm : UserControl
{
    /// <summary>Khớp JSON của CONTRACT_GET_ALL (ContractsForm.ContractGetAllItem).</summary>
    public sealed record ContractRow(
        ContractDto? Contract,
        string? RoomNumber,
        string? RepresentativeName)
    {
        public string EndDateText => Contract is null
            ? "—"
            : Contract.EndDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

        public int RemainingDays => Contract is null
            ? 0
            : Contract.EndDate.DayNumber - DateOnly.FromDateTime(DateTime.Today).DayNumber;

        public string RemainingText => RemainingDays >= 0 ? $"{RemainingDays} ngày" : "Quá hạn";
    }

    /// <summary>DESIGN.md §3.5: ngưỡng cảnh báo hợp đồng sắp hết hạn (US-11).</summary>
    public const int ExpiryWarningDays = 30;

    // Mọi màu lấy từ DESIGN.md — không hardcode màu mới.
    private static readonly Color SurfacePanel = ColorTranslator.FromHtml("#1A1F24");
    private static readonly Color SurfaceActive = ColorTranslator.FromHtml("#21262B");
    private static readonly Color HeaderBg = ColorTranslator.FromHtml("#1A2025");
    private static readonly Color GridBg = ColorTranslator.FromHtml("#14181C");
    private static readonly Color RowAlt = ColorTranslator.FromHtml("#161B1F");
    private static readonly Color BorderSubtle = ColorTranslator.FromHtml("#21272C");
    private static readonly Color TextDim = ColorTranslator.FromHtml("#767E88");
    private static readonly Color TextCream = ColorTranslator.FromHtml("#F4EFEA");
    private static readonly Color TextCreamLight = ColorTranslator.FromHtml("#FAF8F5");
    private static readonly Color TextMuted = ColorTranslator.FromHtml("#A89988");
    private static readonly Color Terracotta = ColorTranslator.FromHtml("#D95D39");

    private static readonly Font HeaderFont = new("Segoe UI", 8.25f, FontStyle.Bold);
    private static readonly Font BodyFont = new("Segoe UI", 9f);

    // Tra cứu tên phòng / đại diện lấy từ CONTRACT_GET_ALL — không gọi thêm action nào.
    private readonly Dictionary<int, string> _roomNames = [];
    private readonly Dictionary<int, string> _roomRepresentatives = [];

    /// <summary>Form1 gắn khi dựng tab: 3 = Hợp đồng, 5 = Hóa đơn (thứ tự tab chủ trọ).</summary>
    public Action<int>? NavigationRequested { get; set; }

    private bool _loaded;

    public DashboardForm()
    {
        InitializeComponent();
        ConfigureGrid(dgvDebt);
        ConfigureGrid(dgvExpiring);
        dgvDebt.CellDoubleClick += (_, e) => NavigateFromDebt(e.RowIndex);
        dgvExpiring.CellDoubleClick += (_, e) => NavigateFromExpiring(e.RowIndex);
        dgvDebt.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) NavigateFromDebt(dgvDebt.CurrentRow?.Index ?? -1); };
        dgvExpiring.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) NavigateFromExpiring(dgvExpiring.CurrentRow?.Index ?? -1); };
    }

    private static Color FromHex(string hex) => ColorTranslator.FromHtml(hex);

    private void ConfigureGrid(DataGridView grid)
    {
        grid.AutoGenerateColumns = false;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToResizeRows = false;
        grid.ReadOnly = true;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.RowHeadersVisible = false;
        grid.BorderStyle = BorderStyle.None;
        grid.BackgroundColor = GridBg;
        grid.GridColor = BorderSubtle;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.RowTemplate.Height = 40;
        grid.ColumnHeadersHeight = 34;
        grid.EnableHeadersVisualStyles = false;
        grid.ScrollBars = ScrollBars.Vertical;

        grid.ColumnHeadersDefaultCellStyle.BackColor = HeaderBg;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = TextDim;
        grid.ColumnHeadersDefaultCellStyle.Font = HeaderFont;
        grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

        grid.DefaultCellStyle.BackColor = SurfacePanel;
        grid.DefaultCellStyle.ForeColor = TextCream;
        grid.DefaultCellStyle.Font = BodyFont;
        grid.DefaultCellStyle.SelectionBackColor = SurfaceActive;
        grid.DefaultCellStyle.SelectionForeColor = TextCreamLight;
        grid.AlternatingRowsDefaultCellStyle.BackColor = RowAlt;
        grid.AlternatingRowsDefaultCellStyle.ForeColor = TextCream;
        grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = SurfaceActive;
        grid.AlternatingRowsDefaultCellStyle.SelectionForeColor = TextCreamLight;

        grid.RowPostPaint += Grid_RowPostPaint;
    }

    /// <summary>Form1 gọi khi tab Tổng quan được chọn — chỉ tải một lần cho tới khi F5.</summary>
    public async Task EnsureLoadedAsync(bool force = false)
    {
        if (_loaded && !force)
        {
            return;
        }

        _loaded = true;
        await LoadSummaryAsync();
        await LoadContractsAsync();
        await LoadInvoicesAsync();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.F5)
        {
            _ = EnsureLoadedAsync(force: true);
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private async Task LoadSummaryAsync()
    {
        try
        {
            var month = CurrentMonth();
            var summary = await Form1.Client.SendAsync<object, SummaryReportDto>(
                ActionNames.ReportSummary, new { BillingMonth = month }, CancellationToken.None);

            lblTotalValue.Text = summary.TotalRooms.ToString(CultureInfo.InvariantCulture);
            lblTotalSub.Text = $"Thuê {summary.RentedRooms} · Trống {summary.AvailableRooms}";

            lblAvailableValue.Text = summary.AvailableRooms.ToString(CultureInfo.InvariantCulture);
            var occupancy = summary.TotalRooms > 0
                ? (int)Math.Round(summary.RentedRooms * 100d / summary.TotalRooms)
                : 0;
            lblAvailableSub.Text = $"Tỷ lệ lấp đầy {occupancy}%";

            lblOccupantsValue.Text = summary.CurrentTenants.ToString(CultureInfo.InvariantCulture);
            lblOccupantsSub.Text = $"Trên {summary.TotalRooms} phòng";

            lblDebtLabel.Text = $"CÒN NỢ {MonthLabel(month)}";
            lblDebtValue.Text = Money(summary.UnpaidAmount);
        }
        catch (Exception ex)
        {
            lblDebtSub.Text = ex.Message;
        }
    }

    /// <summary>CONTRACT_GET_ALL: nạp bảng HĐ sắp hết hạn + bảng tra tên phòng/đại diện.</summary>
    private async Task LoadContractsAsync()
    {
        dgvExpiring.Rows.Clear();
        _roomNames.Clear();
        _roomRepresentatives.Clear();

        try
        {
            var items = await Form1.Client.SendAsync<object, List<ContractRow>>(
                ActionNames.ContractGetAll, new { }, CancellationToken.None);

            foreach (var item in items)
            {
                if (item.Contract is null)
                {
                    continue;
                }

                var roomId = item.Contract.RoomId;
                if (!string.IsNullOrWhiteSpace(item.RoomNumber))
                {
                    _roomNames[roomId] = item.RoomNumber!;
                }

                if (!string.IsNullOrWhiteSpace(item.RepresentativeName))
                {
                    _roomRepresentatives[roomId] = item.RepresentativeName!;
                }

                if (item.Contract.Status != ContractStatus.Active || item.RemainingDays >= ExpiryWarningDays)
                {
                    continue;
                }

                var index = dgvExpiring.Rows.Add(
                    RoomLabel(roomId),
                    Representative(roomId),
                    item.EndDateText,
                    item.RemainingText);
                dgvExpiring.Rows[index].Tag = roomId;

                // US-11: cột "Còn" < 30 ngày tô Terracotta (DESIGN.md §2.4).
                dgvExpiring.Rows[index].Cells[colExpRemaining.Index].Style.ForeColor = Terracotta;
            }
        }
        catch (Exception ex)
        {
            var index = dgvExpiring.Rows.Add("—", ex.Message, "—", "—");
            dgvExpiring.Rows[index].DefaultCellStyle.ForeColor = TextMuted;
        }
    }

    private async Task LoadInvoicesAsync()
    {
        dgvDebt.Rows.Clear();

        try
        {
            var month = CurrentMonth();
            var invoices = await Form1.Client.SendAsync<object, List<InvoiceDto>>(
                ActionNames.InvoiceGetAll, new { BillingMonth = month }, CancellationToken.None);

            var debtTotal = 0m;
            var debtRooms = 0;
            foreach (var invoice in invoices)
            {
                if (invoice.Status != InvoiceStatus.Unpaid)
                {
                    continue;
                }

                debtTotal += invoice.TotalAmount;
                debtRooms++;
                var index = dgvDebt.Rows.Add(
                    RoomLabel(invoice.RoomId),
                    MonthLabel(invoice.BillingMonth),
                    invoice.TotalAmount.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")),
                    Representative(invoice.RoomId));
                dgvDebt.Rows[index].Tag = invoice.RoomId;
            }

            lblDebtSub.Text = $"{debtRooms} phòng chưa thu · {Money(debtTotal)}";
        }
        catch (Exception ex)
        {
            lblDebtSub.Text = ex.Message;
        }
    }

    private static string CurrentMonth() =>
        DateTime.Today.ToString("yyyy-MM", CultureInfo.InvariantCulture);

    private string RoomLabel(int roomId) =>
        _roomNames.TryGetValue(roomId, out var name) ? name : $"P.{roomId}";

    private string Representative(int roomId) =>
        _roomRepresentatives.TryGetValue(roomId, out var name) ? name : "—";

    private void NavigateFromDebt(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= dgvDebt.Rows.Count)
        {
            return;
        }

        NavigationRequested?.Invoke(5); // Hóa đơn
    }

    private void NavigateFromExpiring(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= dgvExpiring.Rows.Count)
        {
            return;
        }

        NavigationRequested?.Invoke(3); // Hợp đồng
    }

    private static string MonthLabel(string billingMonth) =>
        DateTime.TryParseExact(billingMonth, "yyyy-MM", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var parsed)
            ? parsed.ToString("MM/yyyy", CultureInfo.InvariantCulture)
            : billingMonth;

    private static string Money(decimal value) =>
        value.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " đ";

    /// <summary>Dòng đang chọn: nền #21262B + dải trái 3px #D95D39 (DESIGN.md §3.2).</summary>
    private void Grid_RowPostPaint(object? sender, DataGridViewRowPostPaintEventArgs e)
    {
        if ((e.State & DataGridViewElementStates.Selected) != 0)
        {
            using var brush = new SolidBrush(Terracotta);
            e.Graphics.FillRectangle(brush, e.RowBounds.Left, e.RowBounds.Top, 3, e.RowBounds.Height);
        }
    }

    /// <summary>Thẻ KPI vẽ nền + viền + dải ngữ nghĩa trên đỉnh (DESIGN.md §3.4).</summary>
    private sealed class KpiPaintPanel : Panel
    {
        public KpiPaintPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer, true);
            BackColor = ColorTranslator.FromHtml("#101417");
        }

        public string FillHex { get; set; } = "#181C1F";

        public string BorderHex { get; set; } = "#262A2E";

        public int Radius { get; set; } = 6;

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            using var path = Rounded(bounds, Radius);
            using var fill = new SolidBrush(ColorTranslator.FromHtml(FillHex));
            e.Graphics.FillPath(fill, path);
            using var border = new Pen(ColorTranslator.FromHtml(BorderHex));
            e.Graphics.DrawPath(border, path);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Nền đã vẽ ở OnPaintBackground — không tô đè hình chữ nhật.
        }

        private static GraphicsPath Rounded(Rectangle r, int radius)
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
    }
}
