using QuanLyTro.Server.Config;
using QuanLyTro.Server.Data;
using QuanLyTro.Server.Network;
using QuanLyTro.Server.Repositories;
using QuanLyTro.Server.Security;
using QuanLyTro.Server.Services;

namespace QuanLyTro.Server;

internal class Program
{
    static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var appsettings = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        ServerOptions options;
        try
        {
            options = ServerOptions.Load(appsettings);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Cấu hình lỗi: {ex.Message}");
            return 1;
        }

        var database = new Database(options.ConnectionString);

        if (args.Contains("--initialize-only"))
        {
            await new KhoiTaoSchema(database).InitializeAsync();
            Console.WriteLine("Database initialized.");
            return 0;
        }

        if (args.Contains("--seed-demo"))
        {
            await DuLieuMau.SeedAsync(database);
            Console.WriteLine("Demo data seeded.");
            return 0;
        }

        // Tự động đảm bảo schema + quyền phân vai trò đồng bộ mỗi lần Server bật (idempotent, an toàn).
        try
        {
            await new KhoiTaoSchema(database).InitializeAsync();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Cảnh báo: không thể chạy khởi tạo schema/quyền tự động: {ex.Message}");
        }

        var sessions = new SessionStore();
        var permissions = new PhanQuyenRepository(database);

        // Nạp ma trận quyền động từ CSDL vào cache. DB lỗi/chưa seed không được làm sập Server —
        // MaTranPhanQuyen đã có sẵn quyền mặc định từ code nên cứ chạy tiếp.
        try
        {
            MaTranPhanQuyen.ApplyMatrix(await permissions.GetAllAsync());
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Không nạp được phân quyền từ CSDL, dùng quyền mặc định: {ex.Message}");
        }

        var cau_hinh_repo = new CauHinhRepository(database);
        var hoa_don_repo = new HoaDonRepository(database);
        var hop_dong_repo = new HopDongRepository(database);
        var doi_soat = new DoiSoatService(
            cau_hinh_repo,
            new GiaoDichSepayRepository(database),
            hoa_don_repo,
            new SePayClient(new HttpClient { Timeout = TimeSpan.FromSeconds(30) }));

        var router = new DieuPhoiYeuCau(
            new XacThucService(new TaiKhoanRepository(database), new KhachThueAuthRepository(database), sessions),
            new PhongService(new PhongRepository(database)),
            new KhachThueService(new KhachThueRepository(database)),
            new HopDongService(hop_dong_repo),
            new DienNuocService(new DienNuocRepository(database), hop_dong_repo),
            new HoaDonService(hoa_don_repo),
            new BaoCaoService(new BaoCaoRepository(database)),
            new CuTruService(new CuTruRepository(database)),
            sessions,
            permissions,
            new CauHinhService(cau_hinh_repo),
            doi_soat);

        // Bật log console cho Server thật (test để mặc định TẮT cho output gọn).
        ServerLog.Enabled = true;
        ServerLog.Info($"QuanLyTro Server (.NET 8) - TCP Port {options.Port}");

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true; // Nhường Server tự đóng listener thay vì thoát ngay.
            cts.Cancel();
        };

        // Đối soát SePay chạy nền, tách khỏi luồng TCP: chậm mạng không được làm nghẽn client.
        var poller = Task.Run(() => new DoiSoatPoller(doi_soat).RunAsync(cts.Token));

        try
        {
            await new TcpListenerServer(router).StartAsync(options.Port, cts.Token);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Server dừng do lỗi: {ex.Message}");
            return 1;
        }

        // Poller dừng theo cts; chờ ngắn để nó thoát gọn trước khi tiến trình kết thúc.
        try { await poller; } catch (OperationCanceledException) { }

        Console.WriteLine("Server đã dừng.");
        return 0;
    }
}
