using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuanLyTro.Shared.Protocol;

/// <summary>
/// Cấu hình JSON chung cho Client và Server.
/// </summary>
public static class JsonDefaults
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter(),
        },
    };
}
