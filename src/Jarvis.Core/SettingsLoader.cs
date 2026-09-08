using System.Text.Json;

namespace Jarvis.Core;

public static class SettingsLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static async Task<JarvisSettings> LoadOrCreateAsync(string? path = null, CancellationToken cancellationToken = default)
    {
        JarvisPaths.EnsureCreated();
        path ??= Path.Combine(JarvisPaths.ConfigDirectory, "settings.json");

        if (!File.Exists(path))
        {
            var defaults = new JarvisSettings();
            await SaveAsync(defaults, path, cancellationToken);
            return defaults;
        }

        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<JarvisSettings>(stream, JsonOptions, cancellationToken)
            ?? new JarvisSettings();
    }

    public static async Task SaveAsync(JarvisSettings settings, string path, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, settings, JsonOptions, cancellationToken);
    }
}
