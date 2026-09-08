using System.Diagnostics;

namespace Jarvis.Execution;

public sealed record ApplicationTarget(
    string Id,
    string DisplayName,
    string ExecutablePath,
    string ProcessName,
    string[] Aliases);

public enum ApplicationLaunchStatus
{
    Started,
    AlreadyRunning,
    ExecutableNotFound,
    Failed
}

public sealed record ApplicationLaunchResult(
    ApplicationLaunchStatus Status,
    ApplicationTarget Target,
    int? ProcessId = null,
    string? Error = null);

public sealed class ApplicationLauncher
{
    public ApplicationLaunchResult Launch(ApplicationTarget target)
    {
        if (!File.Exists(target.ExecutablePath))
            return new(ApplicationLaunchStatus.ExecutableNotFound, target);
        var existing = Process.GetProcessesByName(target.ProcessName).FirstOrDefault();
        if (existing is not null)
            return new(ApplicationLaunchStatus.AlreadyRunning, target, existing.Id);

        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = target.ExecutablePath,
                UseShellExecute = true,
                WorkingDirectory = Path.GetDirectoryName(target.ExecutablePath)
            });
            return process is null
                ? new(ApplicationLaunchStatus.Failed, target, Error: "Process.Start returned null.")
                : new(ApplicationLaunchStatus.Started, target, process.Id);
        }
        catch (Exception ex)
        {
            return new(ApplicationLaunchStatus.Failed, target, Error: ex.Message);
        }
    }
}

public static class KnownApplications
{
    public static readonly ApplicationTarget Lightroom = new(
        "adobe.lightroom.classic",
        "Adobe Lightroom Classic",
        @"C:\Program Files\Adobe\Adobe Lightroom Classic\Lightroom.exe",
        "Lightroom",
        ["Lightroom", "Lightroom Classic", "Adobe Lightroom", "ligh atrom", "light room"]);

    public static IReadOnlyList<ApplicationTarget> All { get; } = [Lightroom];
}
