using QuanLyTro.Server.Services;

namespace QuanLyTro.Server.Network;

/// <summary>
/// Chạy đối soát SePay định kỳ ở nền. Chỉ MỘT vòng lặp cho cả server (không phải mỗi client),
/// và không bao giờ làm sập server: lỗi mạng/token sai chỉ ghi log rồi thử lại vòng sau.
/// </summary>
public sealed class DoiSoatPoller(DoiSoatService doi_soat, TimeSpan? chu_ky = null)
{
    private static readonly TimeSpan ChuKyMacDinh = TimeSpan.FromSeconds(60);

    /// <summary>Chưa cấu hình API token: cứ 10 tick mới kiểm tra lại 1 lần (10 phút).</summary>
    private const int ChuKyBoQuaKhongToken = 10;

    private readonly TimeSpan _chu_ky = chu_ky ?? ChuKyMacDinh;

    public async Task RunAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(_chu_ky);
        var so_lan_khong_token = 0;
        var da_bao_khong_token = false;

        while (await timer.WaitForNextTickAsync(ct).ConfigureAwait(false))
        {
            // Backoff: chưa có token thì không query DB mỗi 60 giây. Cứ 10 phút mới thử lại.
            if (so_lan_khong_token > 0 && so_lan_khong_token % ChuKyBoQuaKhongToken != 0)
            {
                so_lan_khong_token++;
                continue;
            }

            try
            {
                var ket_qua = await doi_soat.ChayAsync(ct);
                so_lan_khong_token = 0;
                da_bao_khong_token = false;
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
            catch (LoiNghiepVu ex) when (ex.Message.Contains("API token"))
            {
                // Chưa có token: poller lùi lại 10 phút/lần, không spam log.
                so_lan_khong_token = 1;
                if (!da_bao_khong_token)
                {
                    ServerLog.Info("[SePay] chưa có API token — đối soát tự động tạm nghỉ, kiểm tra lại sau mỗi 10 phút.");
                    da_bao_khong_token = true;
                }
            }
            catch (Exception ex)
            {
                // Token sai / SePay sập / mất mạng: thử lại vòng sau, không giết server.
                ServerLog.Info($"[SePay] đối soát lỗi, bỏ qua vòng này: {ex.Message}");
            }
        }
    }
}
