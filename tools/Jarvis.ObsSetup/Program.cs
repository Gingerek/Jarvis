using System.Text.Json;
using Jarvis.Security;

var configPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "obs-studio", "plugin_config", "obs-websocket", "config.json");
if (!File.Exists(configPath))
{
    Console.WriteLine("OBS_CONFIG_NOT_FOUND");
    return 2;
}
using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(configPath));
var root = doc.RootElement;
var authRequired = root.TryGetProperty("auth_required", out var auth) && auth.GetBoolean();
var password = root.TryGetProperty("server_password", out var pass) ? pass.GetString() : null;
if (authRequired && string.IsNullOrEmpty(password))
{
    Console.WriteLine("OBS_PASSWORD_MISSING");
    return 3;
}
if (!string.IsNullOrEmpty(password))
    new WindowsCredentialSecretStore().Write("ObsWebSocketPassword", password);
Console.WriteLine($"OBS_SECRET_IMPORTED={!string.IsNullOrEmpty(password)} AUTH_REQUIRED={authRequired}");
return 0;
