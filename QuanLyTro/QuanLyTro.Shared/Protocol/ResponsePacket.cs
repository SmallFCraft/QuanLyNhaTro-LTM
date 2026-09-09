using System.Text.Json;

namespace QuanLyTro.Shared.Protocol;

/// <summary>
/// Gói tin phản hồi từ Server trả về Client qua TCP.
/// Kết thúc bằng ký tự xuống dòng '\n'.
/// </summary>
public sealed record ResponsePacket(bool Success, string Message, JsonElement? Data)
{
    public static ResponsePacket Ok<T>(T data, string message = "Thành công.") =>
        new(true, message, JsonSerializer.SerializeToElement(data, JsonDefaults.Options));

    public static ResponsePacket Ok(string message = "Thành công.") =>
        new(true, message, null);

    public static ResponsePacket Fail(string message) =>
        new(false, message, null);

    public T? GetData<T>() =>
        Data.HasValue ? Data.Value.Deserialize<T>(JsonDefaults.Options) : default;
}
