using System.Globalization;
using System.Text;
using QuanLyTro.Shared.Models;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Forms;

/// <summary>
/// Màn hình thống kê doanh thu (Sub-Plan F - Step 6).
/// - US-19: Client gọi REPORT_SUMMARY từng tháng rồi tự cộng dòng tổng.
/// - US-08: EXPORT_RESIDENCE trả List&lt;ResidenceExportDto&gt;, ghi CSV bằng StreamWriter
///   UTF-8 BOM, escape bằng cách bọc "" và nhân đôi "" bên trong. Không dùng package Excel/CSV.
/// </summary>
public partial class ReportsForm : UserControl
{
    private const int MonthRange = 6;
    private readonly List<SummaryReportDto> _summaries = [];

    public ReportsForm()
    {
        InitializeComponent();
        ApplyDesignTokens();
        ConfigureGrid();
        PopulateMonths();
    }

    private void ApplyDesignTokens()
    {
        btnView.BackColor = ScreenTheme.From(ScreenTheme.Panel);
        btnView.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
        btnView.FlatStyle = FlatStyle.Flat;
        btnView.FlatAppearance.BorderColor = ScreenTheme.From(ScreenTheme.OutlineBtn);
        btnView.FlatAppearance.MouseOverBackColor = ScreenTheme.From(ScreenTheme.HoverBg);

        // Nút xuất CSV là hành động chính của màn -> Primary theo DESIGN.md §3.3
        btnExportResidence.BackColor = ScreenTheme.From(ScreenTheme.Terracotta);
        btnExportResidence.ForeColor = ScreenTheme.From(ScreenTheme.OnFill);
        btnExportResidence.FlatStyle = FlatStyle.Flat;
        btnExportResidence.FlatAppearance.BorderColor = ScreenTheme.From(ScreenTheme.Terracotta);
        btnExportResidence.FlatAppearance.MouseOverBackColor = ScreenTheme.From(ScreenTheme.TerracottaHover);

        cboMonth.BackColor = ScreenTheme.From(ScreenTheme.Field);
        cboMonth.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
    }

    private void ConfigureGrid()
    {
        grid.AutoGenerateColumns = false;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToResizeRows = false;
        grid.ReadOnly = true;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.RowHeadersVisible = false;
        grid.BackgroundColor = ScreenTheme.From(ScreenTheme.GridBg);
        grid.BorderStyle = BorderStyle.None;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.GridColor = ScreenTheme.From(ScreenTheme.Subtle);
        grid.RowTemplate.Height = 40;
        grid.EnableHeadersVisualStyles = false;
        grid.ScrollBars = ScrollBars.Vertical;

        grid.ColumnHeadersHeight = 34;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.ColumnHeadersDefaultCellStyle.BackColor = ScreenTheme.From(ScreenTheme.HeaderBg);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = ScreenTheme.From(ScreenTheme.Dim);
        grid.ColumnHeadersDefaultCellStyle.Font = ScreenTheme.HeaderFont;
        grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

        grid.DefaultCellStyle.BackColor = ScreenTheme.From(ScreenTheme.RowAlt);
        grid.DefaultCellStyle.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
        grid.DefaultCellStyle.Font = ScreenTheme.Body;
        grid.DefaultCellStyle.SelectionBackColor = ScreenTheme.From(ScreenTheme.Active);
        grid.DefaultCellStyle.SelectionForeColor = ScreenTheme.From(ScreenTheme.CreamLight);

        grid.AlternatingRowsDefaultCellStyle.BackColor = ScreenTheme.From(ScreenTheme.RowAlt2);
        grid.AlternatingRowsDefaultCellStyle.ForeColor = ScreenTheme.From(ScreenTheme.Cream);
        grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = ScreenTheme.From(ScreenTheme.Active);
        grid.AlternatingRowsDefaultCellStyle.SelectionForeColor = ScreenTheme.From(ScreenTheme.CreamLight);

        grid.Columns.Clear();
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "ColMonth",
            HeaderText = "KỲ CƯỚC",
            Width = 160,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft, Font = ScreenTheme.Mono },
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "ColPaid",
            HeaderText = "ĐÃ THU",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = ScreenTheme.Mono },
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "ColUnpaid",
            HeaderText = "CÒN NỢ",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = ScreenTheme.Mono },
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "ColTotal",
            HeaderText = "TỔNG PHẢI THU",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Font = ScreenTheme.Mono },
        });

        grid.CellPainting += Grid_CellPainting;
    }

    private void PopulateMonths()
    {
        var now = DateTime.Now;
        for (var i = 0; i < 12; i++)
        {
            cboMonth.Items.Add(now.AddMonths(-i).ToString("yyyy-MM", CultureInfo.InvariantCulture));
        }
        if (cboMonth.Items.Count > 0)
        {
            cboMonth.SelectedIndex = 0;
        }

        btnView.Click += async (s, e) => await RefreshAsync();
        cboMonth.SelectedIndexChanged += async (s, e) => await RefreshAsync();
        btnExportResidence.Click += async (s, e) => await ExportResidenceCsvAsync();
    }

    /// <summary>F5 tải lại — điều hướng bàn phím đầy đủ.</summary>
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.F5)
        {
            _ = RefreshAsync();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (!DesignMode)
        {
            await RefreshAsync();
        }
    }

    /// <summary>US-19: gọi REPORT_SUMMARY cho từng tháng trong khoảng, cộng dòng tổng tại client.</summary>
    public async Task RefreshAsync()
    {
        var anchor = ParseAnchorMonth();
        _summaries.Clear();
        grid.Rows.Clear();

        try
        {
            var client = Form1.Client;
            if (client == null) return;

            for (var i = MonthRange - 1; i >= 0; i--)
            {
                var month = anchor.AddMonths(-i).ToString("yyyy-MM", CultureInfo.InvariantCulture);
                var summary = await client.SendAsync<object, SummaryReportDto>(
                    ActionNames.ReportSummary,
                    new { BillingMonth = month },
                    CancellationToken.None);

                _summaries.Add(summary);
                grid.Rows.Add(
                    month,
                    ScreenTheme.Money(summary.PaidAmount),
                    ScreenTheme.Money(summary.UnpaidAmount),
                    ScreenTheme.Money(summary.PaidAmount + summary.UnpaidAmount));
            }

            AddTotalRow();
            ApplyKpis();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi tải thống kê", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private DateTime ParseAnchorMonth()
    {
        return DateTime.TryParseExact(cboMonth.Text.Trim(), "yyyy-MM", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var parsed)
            ? parsed
            : DateTime.Now;
    }

    private void AddTotalRow()
    {
        var paid = _summaries.Sum(s => s.PaidAmount);
        var unpaid = _summaries.Sum(s => s.UnpaidAmount);

        var index = grid.Rows.Add("TỔNG CỘNG", ScreenTheme.Money(paid), ScreenTheme.Money(unpaid), ScreenTheme.Money(paid + unpaid));
        var row = grid.Rows[index];
        row.DefaultCellStyle.Font = ScreenTheme.Mono;
        row.DefaultCellStyle.ForeColor = ScreenTheme.From(ScreenTheme.CreamLight);
        row.DefaultCellStyle.BackColor = ScreenTheme.From(ScreenTheme.Panel);
        row.DefaultCellStyle.SelectionBackColor = ScreenTheme.From(ScreenTheme.Active);
        row.DefaultCellStyle.SelectionForeColor = ScreenTheme.From(ScreenTheme.CreamLight);
    }

    private void ApplyKpis()
    {
        // KPI lấy kỳ đang chọn (phần tử cuối trong dãy)
        var current = _summaries.Count > 0 ? _summaries[^1] : new SummaryReportDto(0, 0, 0, 0, 0m, 0m);

        lblKpiPaidValue.Text = ScreenTheme.Money(current.PaidAmount);
        lblKpiPaidSub.Text = $"Kỳ {cboMonth.Text.Trim()} · đã thanh toán";

        lblKpiUnpaidValue.Text = ScreenTheme.Money(current.UnpaidAmount);
        lblKpiUnpaidSub.Text = current.UnpaidAmount > 0
            ? "Còn nợ — cần đôn đốc thu"
            : "Không còn nợ trong kỳ";

        var occupancy = current.TotalRooms > 0
            ? (int)Math.Round(current.RentedRooms * 100d / current.TotalRooms)
            : 0;
        lblKpiOccupancyValue.Text = $"{occupancy}%";
        lblKpiOccupancySub.Text = $"Đang thuê {current.RentedRooms} / {current.TotalRooms} phòng · {current.CurrentTenants} người ở";
    }

    private void Grid_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.Graphics == null)
        {
            return;
        }

        var isSelected = (e.State & DataGridViewElementStates.Selected) != 0;

        // Signature dòng chọn: dải trái 3px Terracotta (DESIGN.md §3.2)
        if (isSelected && e.ColumnIndex == 0)
        {
            e.Paint(e.CellBounds, DataGridViewPaintParts.All);
            using var brush = new SolidBrush(ScreenTheme.From(ScreenTheme.Terracotta));
            e.Graphics.FillRectangle(brush, e.CellBounds.X, e.CellBounds.Y, 3, e.CellBounds.Height);
            e.Handled = true;
            return;
        }

        // Cột Còn nợ: tô Terracotta khi có nợ (DESIGN.md §3.2 "due-red")
        var colUnpaid = grid.Columns["ColUnpaid"];
        if (colUnpaid != null && e.ColumnIndex == colUnpaid.Index && !isSelected)
        {
            var text = grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString() ?? string.Empty;
            var hasDebt = text != "0 đ";
            if (hasDebt)
            {
                e.PaintBackground(e.CellBounds, false);
                TextRenderer.DrawText(e.Graphics, text, ScreenTheme.Mono, e.CellBounds,
                    ScreenTheme.From(ScreenTheme.Terracotta),
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
                e.Handled = true;
            }
        }
    }

    /// <summary>
    /// US-08: xuất danh sách tạm trú ra CSV. Stdlib only — StreamWriter + UTF-8 BOM,
    /// escape bằng bọc "" và nhân đôi "" bên trong (RFC 4180).
    /// </summary>
    public async Task ExportResidenceCsvAsync()
    {
        List<ResidenceExportDto> rows;
        try
        {
            var client = Form1.Client;
            if (client == null) return;

            rows = await client.SendAsync<object, List<ResidenceExportDto>>(
                ActionNames.ExportResidence, new { }, CancellationToken.None);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Lỗi lấy dữ liệu tạm trú", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Title = "Lưu danh sách tạm trú",
            Filter = "Tệp CSV (*.csv)|*.csv",
            FileName = $"danh-sach-tam-tru-{DateTime.Now:yyyyMMdd-HHmm}.csv",
            OverwritePrompt = true,
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            await using (var writer = new StreamWriter(dialog.FileName, false, new UTF8Encoding(true)))
            {
                await writer.WriteLineAsync("Họ tên,Ngày sinh,CCCD,Quê quán,Số phòng");
                foreach (var row in rows)
                {
                    await writer.WriteLineAsync(string.Join(',',
                        Csv(row.FullName),
                        Csv(row.DateOfBirth.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)),
                        Csv(row.IdCard),
                        Csv(row.Hometown),
                        Csv(row.RoomNumber)));
                }
            }

            MessageBox.Show(this, $"Đã xuất {rows.Count} dòng tạm trú ra:\n{dialog.FileName}",
                "Xuất CSV", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Không ghi được tệp CSV: {ex.Message}", "Lỗi xuất CSV",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>Bọc field trong "" và nhân đôi mọi " bên trong (RFC 4180).</summary>
    public static string Csv(string? value)
    {
        var text = value ?? string.Empty;
        return "\"" + text.Replace("\"", "\"\"") + "\"";
    }
}
