using System.Text.Json;

namespace QuanLyTro.Shared.Protocol;

/// <summary>
/// Gói tin yêu cầu từ Client gửi lên Server qua TCP.
/// Kết thúc bằng ký tự xuống dòng '\n'.
/// </summary>
public sealed record RequestPacket(string Action, string? Token, JsonElement Data)
{
    public static RequestPacket Create<T>(string hanh_dong, string? token, T data) =>
        new(hanh_dong, token, JsonSerializer.SerializeToElement(data, JsonDefaults.Options));

    public T GetData<T>() => Data.Deserialize<T>(JsonDefaults.Options)
        ?? throw new JsonException("Request Data deserialized to null.");
}
