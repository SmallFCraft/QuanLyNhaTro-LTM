using QuanLyTro.Server.Services;

namespace QuanLyTro.Server.Network;

/// <summary>
/// Chạy đối soát SePay định kỳ ở nền. Chỉ MỘT vòng lặp cho cả server (không phải mỗi client),
/// và không bao giờ làm sập server: lỗi mạng/token sai chỉ ghi log rồi thử lại vòng sau.
/// </summary>
public sealed class DoiSoatPoller(DoiSoatService doi_soat, TimeSpan? chu_ky = null)
{
    private static readonly TimeSpan ChuKyMacDinh = TimeSpan.FromSeconds(60);

    private readonly TimeSpan _chu_ky = chu_ky ?? ChuKyMacDinh;

    public async Task RunAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(_chu_ky);
        while (await timer.WaitForNextTickAsync(ct).ConfigureAwait(false))
        {
            try
            {
                var ket_qua = await doi_soat.ChayAsync(ct);
                if (ket_qua.SoGiaoDichMoi > 0)
                {
                    ServerLog.Info(
                        $"[SePay] {ket_qua.SoGiaoDichMoi} giao dịch mới: {ket_qua.SoKhop} khớp, {ket_qua.SoBoQua} bỏ qua.");
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                // Token sai / SePay sập / mất mạng: thử lại vòng sau, không giết server.
                ServerLog.Info($"[SePay] đối soát lỗi, bỏ qua vòng này: {ex.Message}");
            }
        }
    }
}
