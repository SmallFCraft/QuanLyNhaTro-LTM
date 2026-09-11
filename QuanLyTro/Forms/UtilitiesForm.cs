using System.Globalization;
using QuanLyTro.Shared.Models;
using QuanLyTro.Shared.Protocol;

namespace QuanLyTro.Forms;

public partial class UtilitiesForm : UserControl
{
    /// <summary>Chỉ số kỳ trước do Server trả về (US-13).</summary>
    private sealed record PreviousReading(int OldElectricity, int OldWater);

    private sealed record RoomComboItem(int Id, string DisplayText);

    public UtilitiesForm()
    {
        InitializeComponent();
        WireEvents();

        noteUtils.Controls.Add(
            NoteBar.Create("Chỉ số cũ tự điền từ kỳ trước. Server tính tiền, preview chỉ để xem (US-12/13)."));
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (DesignMode) return;
        dtpMonth.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        UpdatePreview();
        _ = LoadRoomsAsync();
    }

    private void WireEvents()
    {
        cboRoom.SelectedIndexChanged += async (_, _) => await LoadPreviousReadingAsync();
        dtpMonth.ValueChanged += async (_, _) => await LoadPreviousReadingAsync();
        numElecOld.ValueChanged += (_, _) => UpdatePreview();
        numElecNew.ValueChanged += (_, _) => UpdatePreview();
        numElecRate.ValueChanged += (_, _) => UpdatePreview();
        numWaterOld.ValueChanged += (_, _) => UpdatePreview();
        numWaterNew.ValueChanged += (_, _) => UpdatePreview();
        numWaterRate.ValueChanged += (_, _) => UpdatePreview();
        btnRecord.Click += async (_, _) => await OnRecordAsync();
    }

    private static string BillingMonthOf(DateTimePicker picker) =>
        picker.Value.ToString("yyyy-MM", CultureInfo.InvariantCulture);

    public async Task LoadRoomsAsync()
    {
        try
        {
            var rooms = await Form1.Client.SendAsync<object, List<RoomDto>>(
                ActionNames.RoomGetAll, new { }, CancellationToken.None);

            cboRoom.Items.Clear();
            foreach (var r in rooms)
            {
                cboRoom.Items.Add(new RoomComboItem(r.Id, r.RoomNumber));
            }

            cboRoom.DisplayMember = nameof(RoomComboItem.DisplayText);
            cboRoom.ValueMember = nameof(RoomComboItem.Id);

            if (cboRoom.Items.Count > 0)
            {
                cboRoom.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi tải phòng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    /// <summary>US-13: điền chỉ số cũ kỳ trước vào ô "Chỉ số cũ" (ReadOnly).</summary>
    private async Task LoadPreviousReadingAsync()
    {
        if (cboRoom.SelectedItem is not RoomComboItem selectedRoom) return;

        try
        {
            var prev = await Form1.Client.SendAsync<object, PreviousReading>(
                ActionNames.UtilityGetPrevious,
                new { RoomId = selectedRoom.Id, BillingMonth = BillingMonthOf(dtpMonth) },
                CancellationToken.None);

            numElecOld.Value = prev?.OldElectricity ?? 0;
            numWaterOld.Value = prev?.OldWater ?? 0;

            if (numElecNew.Value < numElecOld.Value) numElecNew.Value = numElecOld.Value;
            if (numWaterNew.Value < numWaterOld.Value) numWaterNew.Value = numWaterOld.Value;

            lblStatusMsg.ForeColor = ScreenTheme.From(ScreenTheme.Neutral);
            lblStatusMsg.Text = prev is null
                ? "Phòng chưa có chỉ số kỳ trước — bắt đầu từ 0."
                : $"Đã nạp chỉ số kỳ trước: điện {prev.OldElectricity:N0} kWh · nước {prev.OldWater:N0} m³.";
        }
        catch (Exception ex)
        {
            lblStatusMsg.ForeColor = ScreenTheme.From(ScreenTheme.Error);
            lblStatusMsg.Text = ex.Message;
        }

        UpdatePreview();
    }

    /// <summary>US-13: ước tính chỉ để UX — số chính thức do Server tính khi UTILITY_RECORD.</summary>
    private void UpdatePreview()
    {
        var elecAmount = (numElecNew.Value - numElecOld.Value) * numElecRate.Value;
        var waterAmount = (numWaterNew.Value - numWaterOld.Value) * numWaterRate.Value;

        lblElecTotal.Text = ScreenTheme.Money(elecAmount);
        lblWaterTotal.Text = ScreenTheme.Money(waterAmount);

        // Negative usage is a client-side format hint only; Server enforces BR-07.
        var invalid = numElecNew.Value < numElecOld.Value || numWaterNew.Value < numWaterOld.Value;
        lblElecTotal.ForeColor = ScreenTheme.From(invalid ? ScreenTheme.Terracotta : ScreenTheme.Tint);
        lblWaterTotal.ForeColor = ScreenTheme.From(invalid ? ScreenTheme.Terracotta : ScreenTheme.Tint);
    }

    private async Task OnRecordAsync()
    {
        if (cboRoom.SelectedItem is not RoomComboItem selectedRoom)
        {
            MessageBox.Show("Vui lòng chọn phòng.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var reading = new UtilityReadingDto(
            Id: 0,
            RoomId: selectedRoom.Id,
            BillingMonth: BillingMonthOf(dtpMonth),
            OldElectricity: (int)numElecOld.Value,
            NewElectricity: (int)numElecNew.Value,
            ElectricityRate: numElecRate.Value,
            OldWater: (int)numWaterOld.Value,
            NewWater: (int)numWaterNew.Value,
            WaterRate: numWaterRate.Value);

        try
        {
            var saved = await Form1.Client.SendAsync<UtilityReadingDto, UtilityReadingDto>(
                ActionNames.UtilityRecord, reading, CancellationToken.None);

            numElecOld.Value = saved.NewElectricity;
            numWaterOld.Value = saved.NewWater;
            UpdatePreview();

            lblStatusMsg.ForeColor = ScreenTheme.From(ScreenTheme.Sage);
            lblStatusMsg.Text = $"Đã chốt điện nước tháng {saved.BillingMonth} cho phòng {selectedRoom.DisplayText}.";
        }
        catch (Exception ex)
        {
            // Server message verbatim (BR-07 / BR-08), inputs preserved.
            MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);

            lblStatusMsg.ForeColor = ScreenTheme.From(ScreenTheme.Error);
            lblStatusMsg.Text = ex.Message;
        }
    }

    /// <summary>Đường kẻ #21272C dưới mỗi hàng .calc (template .calc .row border-bottom).</summary>
    private void RowRule_Paint(object? sender, PaintEventArgs e)
    {
        using var pen = new Pen(ScreenTheme.From(ScreenTheme.Subtle));
        e.Graphics.DrawLine(pen, 0, e.ClipRectangle.Height - 1, e.ClipRectangle.Width, e.ClipRectangle.Height - 1);
    }
}
