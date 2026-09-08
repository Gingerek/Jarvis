using System.Text.Json;
using Jarvis.Core;
using Microsoft.Extensions.Logging;

namespace Jarvis.Diagnostics;

public sealed class JsonFileLoggerProvider : ILoggerProvider
{
    private readonly string _path;
    private readonly object _gate = new();

    public JsonFileLoggerProvider(string? path = null)
    {
        JarvisPaths.EnsureCreated();
        _path = path ?? Path.Combine(JarvisPaths.LogsDirectory, $"jarvis-{DateTime.UtcNow:yyyyMMdd}.jsonl");
    }

    public ILogger CreateLogger(string categoryName) => new JsonFileLogger(categoryName, _path, _gate);
    public void Dispose() { }

    private sealed class JsonFileLogger(string category, string path, object gate) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;
            var entry = JsonSerializer.Serialize(new
            {
                ts = DateTimeOffset.UtcNow,
                level = logLevel.ToString(),
                category,
                eventId = eventId.Id,
                message = formatter(state, exception),
                exception = exception?.ToString()
            });
            lock (gate) File.AppendAllText(path, entry + Environment.NewLine);
        }
    }
}
