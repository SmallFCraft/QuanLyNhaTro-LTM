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

        var router = new DieuPhoiYeuCau(
            new XacThucService(new TaiKhoanRepository(database), new KhachThueAuthRepository(database), sessions),
            new PhongService(new PhongRepository(database)),
            new KhachThueService(new KhachThueRepository(database)),
            new HopDongService(new HopDongRepository(database)),
            new DienNuocService(new DienNuocRepository(database)),
            new HoaDonService(new HoaDonRepository(database)),
            new BaoCaoService(new BaoCaoRepository(database)),
            new CuTruService(new CuTruRepository(database)),
            sessions,
            permissions);

        Console.WriteLine($"QuanLyTro Server (.NET 8) - TCP Port {options.Port}");

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true; // Nhường Server tự đóng listener thay vì thoát ngay.
            cts.Cancel();
        };

        try
        {
            await new TcpListenerServer(router).StartAsync(options.Port, cts.Token);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Server dừng do lỗi: {ex.Message}");
            return 1;
        }

        Console.WriteLine("Server đã dừng.");
        return 0;
    }
}
