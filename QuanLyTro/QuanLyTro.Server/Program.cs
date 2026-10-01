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
            await new SchemaInitializer(database).InitializeAsync();
            Console.WriteLine("Database initialized.");
            return 0;
        }

        if (args.Contains("--seed-demo"))
        {
            await DemoSeeder.SeedAsync(database);
            Console.WriteLine("Demo data seeded.");
            return 0;
        }

        var sessions = new SessionStore();
        var router = new RequestRouter(
            new AuthService(new UserRepository(database), new TenantAuthRepository(database), sessions),
            new RoomService(new RoomRepository(database)),
            new TenantService(new TenantRepository(database)),
            new ContractService(new ContractRepository(database)),
            new UtilityService(new UtilityRepository(database)),
            new InvoiceService(new InvoiceRepository(database)),
            new ReportService(new ReportRepository(database)),
            new ResidenceService(new ResidenceRepository(database)),
            sessions);

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
