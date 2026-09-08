using System.Diagnostics;
using System.IO.Pipes;
using System.Text.Json;

namespace Jarvis_UI;

public sealed record VoiceHostEvent(
    string Type,
    string? State,
    string? Text,
    JsonElement? Data);

public sealed class VoiceHostClient : IAsyncDisposable
{
    private const string PipeName = "Jarvis.VoiceHost";
    private NamedPipeClientStream? _pipe;
    private Process? _process;
    private CancellationTokenSource? _cts;

    public event EventHandler<VoiceHostEvent>? EventReceived;

    public async Task StartAsync()
    {
        _cts = new CancellationTokenSource();
        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var hostPath = Path.Combine(local, "Jarvis", "runtime", "Jarvis.VoiceHost.exe");
        var repoRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Documents", "Jarvis");

        if (!File.Exists(hostPath))
            throw new FileNotFoundException("Jarvis VoiceHost is not installed.", hostPath);
        foreach (var stale in Process.GetProcessesByName("Jarvis.VoiceHost"))
        {
            try { stale.Kill(entireProcessTree: true); }
            catch { }
            finally { stale.Dispose(); }
        }

        _process = Process.Start(new ProcessStartInfo
        {
            FileName = hostPath,
            Arguments = $"--service --root \"{repoRoot}\" --pipe {PipeName}",
            WorkingDirectory = repoRoot,
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        }) ?? throw new InvalidOperationException("Could not start Jarvis VoiceHost.");

        _pipe = new NamedPipeClientStream(
            ".", PipeName, PipeDirection.In, PipeOptions.Asynchronous);
        using var connectCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token);
        connectCts.CancelAfter(TimeSpan.FromSeconds(15));
        await _pipe.ConnectAsync(connectCts.Token);

        _ = Task.Run(() => ReadLoopAsync(_cts.Token));
    }

    private async Task ReadLoopAsync(CancellationToken cancellationToken)
    {
        if (_pipe is null) return;
        using var reader = new StreamReader(_pipe);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(cancellationToken);
                if (line is null) break;
                using var json = JsonDocument.Parse(line);
                var root = json.RootElement;
                var evt = new VoiceHostEvent(
                    root.GetProperty("type").GetString() ?? "unknown",
                    root.TryGetProperty("state", out var state) ? state.GetString() : null,
                    root.TryGetProperty("text", out var text) ? text.GetString() : null,
                    root.TryGetProperty("data", out var data) ? data.Clone() : null);
                EventReceived?.Invoke(this, evt);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            EventReceived?.Invoke(this,
                new VoiceHostEvent("error", "ERROR", ex.Message, null));
        }
    }

    public async ValueTask DisposeAsync()
    {
        _cts?.Cancel();
        if (_pipe is not null) await _pipe.DisposeAsync();

        if (_process is { HasExited: false })
        {
            try { _process.Kill(entireProcessTree: true); }
            catch { }
        }

        _process?.Dispose();
        _cts?.Dispose();
    }
}
