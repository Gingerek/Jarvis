using Jarvis.Core;

namespace Jarvis.Diagnostics;

public sealed record FoundationHealthItem(string Name, HealthState State, string Message);

public static class FoundationHealth
{
    public static async Task<IReadOnlyList<FoundationHealthItem>> CheckAsync(CancellationToken cancellationToken = default)
    {
        JarvisPaths.EnsureCreated();
        var items = new List<FoundationHealthItem>();

        var configPath = Path.Combine(JarvisPaths.ConfigDirectory, "settings.json");
        try
        {
            _ = await SettingsLoader.LoadOrCreateAsync(configPath, cancellationToken);
            items.Add(new("config", HealthState.Healthy, configPath));
        }
        catch (Exception ex)
        {
            items.Add(new("config", HealthState.Unavailable, ex.Message));
        }

        items.Add(CheckDirectory("plugins", JarvisPaths.PluginsDirectory));
        items.Add(CheckDirectory("logs", JarvisPaths.LogsDirectory));

        var dbPath = Path.Combine(JarvisPaths.DataDirectory, "learning.db");
        items.Add(new("database", File.Exists(dbPath) ? HealthState.Healthy : HealthState.Degraded,
            File.Exists(dbPath) ? dbPath : "learning.db has not been created yet"));
        return items;
    }

    private static FoundationHealthItem CheckDirectory(string name, string path)
    {
        try
        {
            Directory.CreateDirectory(path);
            var probe = Path.Combine(path, $".write-probe-{Guid.NewGuid():N}");
            File.WriteAllText(probe, "ok");
            File.Delete(probe);
            return new(name, HealthState.Healthy, path);
        }
        catch (Exception ex)
        {
            return new(name, HealthState.Unavailable, ex.Message);
        }
    }
}
