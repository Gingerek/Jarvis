namespace Jarvis.Core;

public sealed class JarvisSettings
{
    public string Language { get; set; } = "pl-PL";
    public string WakeWord { get; set; } = "Jarvis";
    public bool StartMinimized { get; set; }
    public bool EnableTelemetry { get; set; }
    public string LogLevel { get; set; } = "Information";
    public string PluginDirectory { get; set; } = JarvisPaths.PluginsDirectory;
    public string? AudioInputDeviceId { get; set; }
    public string? AudioInputDeviceName { get; set; }
}
