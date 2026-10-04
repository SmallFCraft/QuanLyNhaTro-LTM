namespace QuanLyTro.Server;

/// <summary>
/// Log console cho Server: mỗi yêu cầu TCP một dòng gồm endpoint, hanh_dong, kết quả và
/// thời gian xử lý — đủ để soi bug mà không cần dựng lại kịch bản.
///
/// KHÔNG BAO GIỜ ghi payload: gói tin chứa `MatKhau`, `ma_qr_token`, `ma_pin`; chỉ ghi tên
/// hanh_dong cùng thông báo lỗi tiếng Việt mà Server trả về cho Client.
///
/// Mặc định TẮT để `dotnet test` không ngập output; Program.cs bật khi chạy Server thật.
/// ponytail: chỉ ghi ra console — thêm sink ghi file khi cần đọc log từ xa.
/// </summary>
public static class ServerLog
{
    public static bool Enabled { get; set; }

    /// <summary>Một dòng cho một yêu cầu: `giờ [mức] endpoint | hanh_dong | OK/FAIL | N ms | thông báo`.</summary>
    public static string FormatRequest(
        string endpoint, string hanh_dong, bool success, string? message, long elapsedMs)
    {
        var level = success ? "INFO" : "WARN";
        var ketQua = success ? "OK" : "FAIL";
        var tail = string.IsNullOrWhiteSpace(message) ? string.Empty : $" | {message}";
        return $"{Now()} [{level}] {endpoint} | {hanh_dong} | {ketQua} | {elapsedMs} ms{tail}";
    }

    public static void Request(
        string endpoint, string hanh_dong, bool success, string? message, long elapsedMs)
    {
        if (!Enabled)
        {
            return;
        }

        var line = FormatRequest(endpoint, hanh_dong, success, message, elapsedMs);
        if (success)
        {
            Console.WriteLine(line);
        }
        else
        {
            // Yêu cầu hỏng ra stderr để nổi bật khi console bị cuộn.
            Console.Error.WriteLine(line);
        }
    }

    public static void Info(string message)
    {
        if (Enabled)
        {
            Console.WriteLine($"{Now()} [INFO] {message}");
        }
    }

    public static void Warn(string message)
    {
        if (Enabled)
        {
            Console.Error.WriteLine($"{Now()} [WARN] {message}");
        }
    }

    public static void Error(string message, Exception? exception = null)
    {
        if (!Enabled)
        {
            return;
        }

        var detail = exception is null ? string.Empty : $" | {exception.GetType().Name}: {exception.Message}";
        Console.Error.WriteLine($"{Now()} [ERROR] {message}{detail}");
    }

    private static string Now() => DateTime.Now.ToString("HH:mm:ss.fff");
}
