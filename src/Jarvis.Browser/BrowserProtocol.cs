using System.Text.Json;

namespace Jarvis.Browser;

public sealed record BrowserBridgeRequest(
    string Id,
    string Action,
    string? Argument = null);

public sealed record BrowserBridgeResponse(
    string Id,
    bool Ok,
    string? Error = null,
    JsonElement? Data = null);

public sealed record BrowserCommandResult(
    bool Success,
    string? Error = null,
    JsonElement? Data = null);

public static class BrowserJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };
}

