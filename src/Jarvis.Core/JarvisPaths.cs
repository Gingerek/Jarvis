namespace Jarvis.Core;

public static class JarvisPaths
{
    public static string AppDataRoot =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Jarvis");

    public static string ConfigDirectory => Path.Combine(AppDataRoot, "config");
    public static string DataDirectory => Path.Combine(AppDataRoot, "data");
    public static string LogsDirectory => Path.Combine(AppDataRoot, "logs");
    public static string PluginsDirectory => Path.Combine(AppDataRoot, "plugins");

    public static void EnsureCreated()
    {
        Directory.CreateDirectory(AppDataRoot);
        Directory.CreateDirectory(ConfigDirectory);
        Directory.CreateDirectory(DataDirectory);
        Directory.CreateDirectory(LogsDirectory);
        Directory.CreateDirectory(PluginsDirectory);
    }
}
