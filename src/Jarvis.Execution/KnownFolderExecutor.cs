using System.Diagnostics;
using System.Runtime.Versioning;

namespace Jarvis.Execution;

public sealed record FolderOpenResult(
    bool Success,
    string DisplayName,
    string Path,
    string Message);

[SupportedOSPlatform("windows")]
public sealed class KnownFolderExecutor
{
    private readonly string _jarvisRoot;

    public KnownFolderExecutor(string jarvisRoot)
    {
        _jarvisRoot = Path.GetFullPath(jarvisRoot);
    }

    public FolderOpenResult Open(string key)
    {
        var (displayName, path) = Resolve(key);
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            return new(false, displayName, path ?? string.Empty,
                $"Nie znalazłem folderu {displayName}.");

        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"\"{path}\"",
            UseShellExecute = true
        });
        return new(true, displayName, path,
            $"Otwieram folder {displayName}.");
    }

    private (string DisplayName, string Path) Resolve(string key)
    {
        return key.ToLowerInvariant() switch
        {
            "downloads" => ("Pobrane", Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads")),
            "documents" => ("Dokumenty",
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)),
            "pictures" => ("Zdjęcia",
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)),
            "music" => ("Muzyka",
                Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)),
            "videos" => ("Wideo",
                Environment.GetFolderPath(Environment.SpecialFolder.MyVideos)),
            "desktop" => ("Pulpit",
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)),
            "jarvis" => ("Jarvis", _jarvisRoot),
            _ => (key, string.Empty)
        };
    }
}
