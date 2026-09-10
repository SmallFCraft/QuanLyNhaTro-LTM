using QuanLyTro.Server.Config;
using QuanLyTro.Server.Data;

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

        if (args.Contains("--initialize-only"))
        {
            var database = new Database(options.ConnectionString);
            await new SchemaInitializer(database).InitializeAsync();
            Console.WriteLine("Database initialized.");
            return 0;
        }

        Console.WriteLine($"QuanLyTro Server (.NET 8) - TCP Port {options.Port}");
        return 0;
    }
}
