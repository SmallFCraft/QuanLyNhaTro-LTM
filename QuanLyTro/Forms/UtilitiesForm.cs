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
                cboRoom.Items.Add(new RoomComboItem(r.Id, $"{r.RoomNumber} — {r.CurrentOccupants}/{r.MaxOccupants} người"));
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

    /// <summary>US-13: điền chỉ số cũ kỳ trước vào ô "Chỉ số cũ".</summary>
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

            lblStatusMsg.ForeColor = ColorTranslator.FromHtml("#CAC6C1");
            lblStatusMsg.Text = prev is null
                ? "Phòng chưa có chỉ số kỳ trước — bắt đầu từ 0."
                : $"Đã nạp chỉ số kỳ trước: điện {prev.OldElectricity:N0} kWh · nước {prev.OldWater:N0} m³.";
        }
        catch (Exception ex)
        {
            lblStatusMsg.ForeColor = ColorTranslator.FromHtml("#FFB4AB");
            lblStatusMsg.Text = ex.Message;
        }

        UpdatePreview();
    }

    /// <summary>US-13: ước tính chỉ để UX — số chính thức do Server tính khi UTILITY_RECORD.</summary>
    private void UpdatePreview()
    {
        var elecUsage = numElecNew.Value - numElecOld.Value;
        var waterUsage = numWaterNew.Value - numWaterOld.Value;
        var elecAmount = elecUsage * numElecRate.Value;
        var waterAmount = waterUsage * numWaterRate.Value;

        lblElecUsageVal.Text = $"{elecUsage:N0} kWh";
        lblElecEstVal.Text = $"{elecAmount:N0} VNĐ";
        lblWaterUsageVal.Text = $"{waterUsage:N0} m³";
        lblWaterEstVal.Text = $"{waterAmount:N0} VNĐ";
        lblTotalEstVal.Text = $"{elecAmount + waterAmount:N0} VNĐ";

        // Negative usage is a client-side format hint only; Server enforces BR-07.
        var invalid = elecUsage < 0 || waterUsage < 0;
        lblElecEstVal.ForeColor = ColorTranslator.FromHtml(invalid ? "#D95D39" : "#E0AF68");
        lblWaterEstVal.ForeColor = ColorTranslator.FromHtml(invalid ? "#D95D39" : "#8BD7A3");
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

            lblStatusMsg.ForeColor = ColorTranslator.FromHtml("#8BD7A3");
            lblStatusMsg.Text = $"Đã chốt điện nước tháng {saved.BillingMonth} cho phòng {selectedRoom.DisplayText}.";
        }
        catch (Exception ex)
        {
            // Server message verbatim (BR-07 / BR-08), inputs preserved.
            MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);

            lblStatusMsg.ForeColor = ColorTranslator.FromHtml("#FFB4AB");
            lblStatusMsg.Text = ex.Message;
        }
    }
}
