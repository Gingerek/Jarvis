using System.Text.Json;

namespace Jarvis.Core;

public sealed record PluginManifest(
    string Id,
    string Assembly,
    string Version,
    string[] Capabilities,
    string[] Permissions);

public static class PluginManifestReader
{
    public static PluginManifest Read(string path)
    {
        var json = File.ReadAllText(path);
        var manifest = JsonSerializer.Deserialize<PluginManifest>(json)
            ?? throw new InvalidDataException("Plugin manifest is empty or invalid.");

        if (string.IsNullOrWhiteSpace(manifest.Id)) throw new InvalidDataException("Plugin id is required.");
        if (string.IsNullOrWhiteSpace(manifest.Assembly)) throw new InvalidDataException("Plugin assembly is required.");
        if (!Version.TryParse(manifest.Version, out _)) throw new InvalidDataException("Plugin version is invalid.");
        if (Path.GetFileName(manifest.Assembly) != manifest.Assembly) throw new InvalidDataException("Plugin assembly must be a file name, not a path.");

        return manifest;
    }
}
