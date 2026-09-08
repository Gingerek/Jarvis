using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using NAudio.CoreAudioApi;

namespace Jarvis.Execution;

public sealed record SystemActionResult(bool Success, string Message);

[SupportedOSPlatform("windows")]
public sealed class WindowsSystemController
{
    public int GetVolumePercent()
    {
        using var enumerator = new MMDeviceEnumerator();
        using var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        return (int)Math.Round(device.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
    }

    public SystemActionResult SetVolumePercent(int percent)
    {
        percent = Math.Clamp(percent, 0, 100);
        using var enumerator = new MMDeviceEnumerator();
        using var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        device.AudioEndpointVolume.MasterVolumeLevelScalar = percent / 100f;
        return new(true, $"Głośność {percent} procent.");
    }
    public SystemActionResult ChangeVolume(int delta)
    {
        var current = GetVolumePercent();
        return SetVolumePercent(current + delta);
    }

    public SystemActionResult SetMute(bool muted)
    {
        using var enumerator = new MMDeviceEnumerator();
        using var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        device.AudioEndpointVolume.Mute = muted;
        return new(true, muted ? "Wyciszam dźwięk." : "Włączam dźwięk.");
    }

    public SystemActionResult MinimizeForegroundWindow()
    {
        var hwnd = GetForegroundWindow();
        if (hwnd == IntPtr.Zero) return new(false, "Nie widzę aktywnego okna.");
        ShowWindow(hwnd, 6);
        return new(true, "Minimalizuję okno.");
    }

    public SystemActionResult MaximizeForegroundWindow()
    {
        var hwnd = GetForegroundWindow();
        if (hwnd == IntPtr.Zero) return new(false, "Nie widzę aktywnego okna.");
        ShowWindow(hwnd, 3);
        return new(true, "Maksymalizuję okno.");
    }
    public SystemActionResult RestoreForegroundWindow()
    {
        var hwnd = GetForegroundWindow();
        if (hwnd == IntPtr.Zero) return new(false, "Nie widzę aktywnego okna.");
        ShowWindow(hwnd, 9);
        return new(true, "Przywracam okno.");
    }

    public SystemActionResult CloseForegroundWindow()
    {
        var hwnd = GetForegroundWindow();
        if (hwnd == IntPtr.Zero) return new(false, "Nie widzę aktywnego okna.");
        return PostMessage(hwnd, 0x0010, IntPtr.Zero, IntPtr.Zero)
            ? new(true, "Zamykam aktywne okno.")
            : new(false, "Nie udało się zamknąć aktywnego okna.");
    }

    public SystemActionResult ShowDesktop()
    {
        try
        {
            var type = Type.GetTypeFromProgID("Shell.Application");
            if (type is null) return new(false, "Nie mogę połączyć się z powłoką Windows.");
            dynamic shell = Activator.CreateInstance(type)!;
            shell.ToggleDesktop();
            Marshal.FinalReleaseComObject(shell);
            return new(true, "Pokazuję pulpit.");
        }
        catch
        {
            return new(false, "Nie udało się pokazać pulpitu.");
        }
    }
    public SystemActionResult LockComputer()
    {
        return LockWorkStation()
            ? new(true, "Blokuję komputer.")
            : new(false, "Nie udało się zablokować komputera.");
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool LockWorkStation();
}
