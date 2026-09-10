using System.Text.Json;

namespace QuanLyTro.Server.Config;

/// <summary>Cấu hình Server: chuỗi kết nối MySQL và cổng TCP lắng nghe.</summary>
public sealed record ServerOptions(string ConnectionString, int Port)
{
    public static ServerOptions Load(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Không tìm thấy file cấu hình: {path}", path);
        }

        var json = File.ReadAllText(path);
        var options = JsonSerializer.Deserialize<ServerOptions>(json, JsonOptions)
            ?? throw new InvalidDataException($"File cấu hình rỗng hoặc sai định dạng: {path}");

        return Validate(options);
    }

    /// <summary>Chặn cấu hình sai ngay lúc khởi động, thay vì lỗi mơ hồ lúc chạy.</summary>
    public static ServerOptions Validate(ServerOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            throw new InvalidDataException("ConnectionString không được để trống.");
        }

        if (options.Port is < 1024 or > 65535)
        {
            throw new InvalidDataException($"Port phải nằm trong khoảng 1024-65535, nhận được {options.Port}.");
        }

        return options;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };
}
