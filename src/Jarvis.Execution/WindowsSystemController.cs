using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using NAudio.CoreAudioApi;

namespace Jarvis.Execution;

public sealed record SystemActionResult(bool Success, string Message);

[SupportedOSPlatform("windows")]
public sealed class WindowsSystemController
{
    private const uint KeyUp = 0x0002;
    public int GetVolumePercent()
    {
        using var enumerator = new MMDeviceEnumerator();
        using var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        return (int)Math.Round(device.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
    }
    public SystemActionResult GetVolumeStatus() => new(true, $"Głośność jest ustawiona na {GetVolumePercent()} procent.");
    public SystemActionResult SetVolumePercent(int percent)
    {
        percent = Math.Clamp(percent, 0, 100);
        using var enumerator = new MMDeviceEnumerator();
        using var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        device.AudioEndpointVolume.MasterVolumeLevelScalar = percent / 100f;
        return new(true, $"Głośność {percent} procent.");
    }
    public SystemActionResult ChangeVolume(int delta) => SetVolumePercent(GetVolumePercent() + delta);
    public SystemActionResult SetMute(bool muted)
    {
        using var enumerator = new MMDeviceEnumerator();
        using var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        device.AudioEndpointVolume.Mute = muted;
        return new(true, muted ? "Wyciszam dźwięk." : "Włączam dźwięk.");
    }
    public SystemActionResult GetBatteryStatus()
    {
        if (!GetSystemPowerStatus(out var s)) return new(false, "Nie mogę odczytać stanu zasilania.");
        if ((s.BatteryFlag & 128) != 0 || s.BatteryLifePercent == 255) return new(true, "Ten komputer nie ma aktywnej baterii.");
        var power = s.ACLineStatus == 1 ? "Zasilacz jest podłączony." : "Komputer działa z baterii.";
        return new(true, $"Bateria ma {s.BatteryLifePercent} procent. {power}");
    }
    public SystemActionResult GetDiskSpace()
    {
        try
        {
            var root = Path.GetPathRoot(Environment.SystemDirectory)!;
            var drive = new DriveInfo(root);
            var free = drive.AvailableFreeSpace / 1073741824d;
            var total = drive.TotalSize / 1073741824d;
            return new(true, $"Na dysku {root.TrimEnd('\\')} jest {free:F0} gigabajtów wolnego z {total:F0} gigabajtów.");
        }
        catch { return new(false, "Nie mogę odczytać miejsca na dysku."); }
    }
    public SystemActionResult GetUptime()
    {
        var up = TimeSpan.FromMilliseconds(Environment.TickCount64);
        var days = up.Days > 0 ? $"{up.Days} dni, " : string.Empty;
        return new(true, $"Komputer działa od {days}{up.Hours} godzin i {up.Minutes} minut.");
    }
    public SystemActionResult GetActiveWindowTitle()
    {
        var hwnd = GetForegroundWindow();
        if (hwnd == IntPtr.Zero) return new(false, "Nie widzę aktywnego okna.");
        var length = GetWindowTextLength(hwnd);
        var title = new StringBuilder(Math.Max(length + 1, 256));
        GetWindowText(hwnd, title, title.Capacity);
        return new(true, title.Length == 0 ? "Aktywne okno nie ma tytułu." : $"Aktywne okno to {title}.");
    }
    public SystemActionResult MinimizeForegroundWindow() => WindowAction(6, "Minimalizuję okno.");
    public SystemActionResult MaximizeForegroundWindow() => WindowAction(3, "Maksymalizuję okno.");
    public SystemActionResult RestoreForegroundWindow() => WindowAction(9, "Przywracam okno.");
    private SystemActionResult WindowAction(int command, string message)
    {
        var hwnd = GetForegroundWindow();
        if (hwnd == IntPtr.Zero) return new(false, "Nie widzę aktywnego okna.");
        ShowWindow(hwnd, command); return new(true, message);
    }
    public SystemActionResult CloseForegroundWindow()
    {
        var hwnd = GetForegroundWindow();
        if (hwnd == IntPtr.Zero) return new(false, "Nie widzę aktywnego okna.");
        return PostMessage(hwnd, 0x0010, IntPtr.Zero, IntPtr.Zero) ? new(true, "Zamykam aktywne okno.") : new(false, "Nie udało się zamknąć aktywnego okna.");
    }
    public SystemActionResult ShowDesktop()
    {
        try
        {
            var type = Type.GetTypeFromProgID("Shell.Application");
            if (type is null) return new(false, "Nie mogę połączyć się z powłoką Windows.");
            dynamic shell = Activator.CreateInstance(type)!; shell.ToggleDesktop(); Marshal.FinalReleaseComObject(shell);
            return new(true, "Pokazuję pulpit.");
        }
        catch { return new(false, "Nie udało się pokazać pulpitu."); }
    }
    public SystemActionResult LockComputer() => LockWorkStation() ? new(true, "Blokuję komputer.") : new(false, "Nie udało się zablokować komputera.");
    public SystemActionResult SwitchWindow()
    {
        KeyCombo(0x12, 0x09); return new(true, "Przełączam na następne okno.");
    }
    public SystemActionResult MediaPlayPause() { TapKey(0xB3); return new(true, "Przełączam odtwarzanie."); }
    public SystemActionResult MediaNextTrack() { TapKey(0xB0); return new(true, "Następny utwór."); }
    public SystemActionResult MediaPreviousTrack() { TapKey(0xB1); return new(true, "Poprzedni utwór."); }
    public SystemActionResult SendShortcut(string shortcut)
    {
        var map = new Dictionary<string, (byte mod, byte key, string msg)>(StringComparer.Ordinal)
        {
            ["copy"]=(0x11,0x43,"Kopiuję."), ["paste"]=(0x11,0x56,"Wklejam."), ["cut"]=(0x11,0x58,"Wycinam."),
            ["undo"]=(0x11,0x5A,"Cofam."), ["redo"]=(0x11,0x59,"Ponawiam."), ["save"]=(0x11,0x53,"Zapisuję."),
            ["select_all"]=(0x11,0x41,"Zaznaczam wszystko."), ["find"]=(0x11,0x46,"Otwieram wyszukiwanie."),
            ["new"]=(0x11,0x4E,"Nowy dokument."), ["print"]=(0x11,0x50,"Otwieram drukowanie.")
        };
        if (map.TryGetValue(shortcut, out var combo)) { KeyCombo(combo.mod, combo.key); return new(true, combo.msg); }
        var single = shortcut switch { "escape" => (byte)0x1B, "enter" => (byte)0x0D, "delete" => (byte)0x2E, _ => (byte)0 };
        if (single != 0) { TapKey(single); return new(true, $"Naciskam {shortcut}."); }
        return new(false, "Nie znam tego skrótu klawiaturowego.");
    }
    public SystemActionResult TypeText(string text)
    {
        if (string.IsNullOrEmpty(text)) return new(false, "Nie podałeś tekstu do wpisania.");
        foreach (var ch in text)
        {
            var inputs = new[] { UnicodeInput(ch, false), UnicodeInput(ch, true) };
            if (SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>()) == 0) return new(false, "Nie udało się wpisać tekstu.");
        }
        return new(true, "Wpisałem tekst.");
    }
    private static void KeyCombo(byte modifier, byte key)
    {
        keybd_event(modifier, 0, 0, UIntPtr.Zero); keybd_event(key, 0, 0, UIntPtr.Zero);
        keybd_event(key, 0, KeyUp, UIntPtr.Zero); keybd_event(modifier, 0, KeyUp, UIntPtr.Zero);
    }
    private static void TapKey(byte key) { keybd_event(key, 0, 0, UIntPtr.Zero); keybd_event(key, 0, KeyUp, UIntPtr.Zero); }
    private static INPUT UnicodeInput(char ch, bool keyUp) => new() { type = 1, U = new InputUnion { ki = new KEYBDINPUT { wScan = ch, dwFlags = 0x0004u | (keyUp ? 0x0002u : 0u) } } };

    [StructLayout(LayoutKind.Sequential)] private struct SYSTEM_POWER_STATUS { public byte ACLineStatus, BatteryFlag, BatteryLifePercent, SystemStatusFlag; public uint BatteryLifeTime, BatteryFullLifeTime; }
    [StructLayout(LayoutKind.Sequential)] private struct INPUT { public uint type; public InputUnion U; }
    [StructLayout(LayoutKind.Explicit)] private struct InputUnion { [FieldOffset(0)] public KEYBDINPUT ki; }
    [StructLayout(LayoutKind.Sequential)] private struct KEYBDINPUT { public ushort wVk, wScan; public uint dwFlags, time; public UIntPtr dwExtraInfo; }
    [DllImport("kernel32.dll")] private static extern bool GetSystemPowerStatus(out SYSTEM_POWER_STATUS status);
    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] private static extern int GetWindowTextLength(IntPtr hWnd);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int maxCount);
    [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")] private static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll")] private static extern bool LockWorkStation();
    [DllImport("user32.dll")] private static extern void keybd_event(byte bVk, byte bScan, uint flags, UIntPtr extraInfo);
    [DllImport("user32.dll", SetLastError = true)] private static extern uint SendInput(uint count, INPUT[] inputs, int size);
}
