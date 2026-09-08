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

public enum ApplicationCloseStatus
{
    CloseRequested,
    NotRunning,
    Failed
}
public sealed record ApplicationLaunchResult(
    ApplicationLaunchStatus Status,
    ApplicationTarget Target,
    int? ProcessId = null,
    string? Error = null);

public sealed record ApplicationCloseResult(
    ApplicationCloseStatus Status,
    ApplicationTarget Target,
    int ClosedWindows = 0,
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

    public ApplicationCloseResult Close(ApplicationTarget target)
    {
        var processes = Process.GetProcessesByName(target.ProcessName);
        if (processes.Length == 0)
            return new(ApplicationCloseStatus.NotRunning, target);
        try
        {
            var requested = 0;
            foreach (var process in processes)
            {
                using (process)
                {
                    if (process.CloseMainWindow()) requested++;
                }
            }
            return new(ApplicationCloseStatus.CloseRequested, target, requested);
        }
        catch (Exception ex)
        {
            return new(ApplicationCloseStatus.Failed, target, Error: ex.Message);
        }
    }
}

public static class KnownApplications
{
    public static readonly ApplicationTarget Lightroom = new(
        "adobe.lightroom.classic", "Adobe Lightroom Classic",
        @"C:\Program Files\Adobe\Adobe Lightroom Classic\Lightroom.exe", "Lightroom",
        ["Lightroom", "Lightroom Classic", "Adobe Lightroom", "light room"]);
    public static readonly ApplicationTarget Resolve = new(
        "blackmagic.resolve", "DaVinci Resolve",
        @"C:\Program Files\Blackmagic Design\DaVinci Resolve\Resolve.exe", "Resolve",
        ["DaVinci", "DaVinci Resolve", "Resolve"]);

    public static readonly ApplicationTarget Obs = new(
        "obs.studio", "OBS Studio",
        @"C:\Program Files\obs-studio\bin\64bit\obs64.exe", "obs64",
        ["OBS", "OBS Studio"]);

    public static readonly ApplicationTarget Chrome = new(
        "google.chrome", "Google Chrome",
        @"C:\Program Files\Google\Chrome\Application\chrome.exe", "chrome",
        ["Chrome", "Google Chrome"]);

    public static readonly ApplicationTarget Edge = new(
        "microsoft.edge", "Microsoft Edge",
        @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe", "msedge",
        ["Edge", "Microsoft Edge"]);
    public static readonly ApplicationTarget Notepad = new(
        "windows.notepad", "Notatnik",
        @"C:\Windows\System32\notepad.exe", "notepad",
        ["Notatnik", "Notepad"]);

    public static readonly ApplicationTarget TaskManager = new(
        "windows.taskmanager", "Menedżer zadań",
        @"C:\Windows\System32\Taskmgr.exe", "Taskmgr",
        ["Menedżer zadań", "Task Manager", "Taskmanager"]);

    public static IReadOnlyList<ApplicationTarget> All { get; } =
        [Lightroom, Resolve, Obs, Chrome, Edge, Notepad, TaskManager];
}
