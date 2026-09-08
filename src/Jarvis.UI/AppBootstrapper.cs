using Jarvis.Core;
using Jarvis.Diagnostics;
using Jarvis.Learning;
using Jarvis.Plugins;
using Microsoft.Extensions.Logging;

namespace Jarvis_UI;

public sealed record StartupSnapshot(
    string Status,
    string ConfigPath,
    string DatabasePath,
    int PluginCount,
    string LogDirectory);

public sealed class AppBootstrapper : IDisposable
{
    private ILoggerFactory? _loggerFactory;

    public async Task<StartupSnapshot> InitializeAsync(CancellationToken cancellationToken = default)
    {
        JarvisPaths.EnsureCreated();
        var configPath = Path.Combine(JarvisPaths.ConfigDirectory, "settings.json");
        var settings = await SettingsLoader.LoadOrCreateAsync(configPath, cancellationToken);

        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(ParseLogLevel(settings.LogLevel));
            builder.AddProvider(new JsonFileLoggerProvider());
        });

        var logger = _loggerFactory.CreateLogger<AppBootstrapper>();
        var databasePath = Path.Combine(JarvisPaths.DataDirectory, "learning.db");
        var database = new LearningDatabase(databasePath);
        await database.InitializeAsync(cancellationToken);

        var registry = new PluginRegistry();
        var loader = new PluginLoader();
        var pluginCount = loader.RegisterDiscovered(settings.PluginDirectory, registry);

        logger.LogInformation("Jarvis foundation initialized with {PluginCount} plugins", pluginCount);
        return new StartupSnapshot("Ready", configPath, databasePath, pluginCount, JarvisPaths.LogsDirectory);
    }

    public void Dispose() => _loggerFactory?.Dispose();

    private static LogLevel ParseLogLevel(string value) =>
        Enum.TryParse<LogLevel>(value, true, out var parsed) ? parsed : LogLevel.Information;
}
