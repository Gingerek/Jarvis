using System.IO.Pipes;
using System.Text.Json;

namespace Jarvis.Browser;

public sealed class BrowserCommandClient : IAsyncDisposable
{
    public const string PipeName = "Jarvis.BrowserBridge";
    private readonly SemaphoreSlim _gate = new(1, 1);
    private NamedPipeClientStream? _pipe;
    private StreamReader? _reader;
    private StreamWriter? _writer;

    public async Task<BrowserCommandResult> SendAsync(
        string action,
        string? argument = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        await _gate.WaitAsync(cancellationToken);
        try
        {
            await EnsureConnectedAsync(cancellationToken);
            var id = Guid.NewGuid().ToString("N");
            var request = new BrowserBridgeRequest(id, action, argument);
            var json = JsonSerializer.Serialize(request, BrowserJson.Options);
            await _writer!.WriteLineAsync(json.AsMemory(), cancellationToken);
            await _writer.FlushAsync(cancellationToken);
            using var responseCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            responseCts.CancelAfter(TimeSpan.FromSeconds(3));
            var line = await _reader!.ReadLineAsync(responseCts.Token);
            if (string.IsNullOrWhiteSpace(line))
                return new(false, "Browser bridge disconnected.");

            var response = JsonSerializer.Deserialize<BrowserBridgeResponse>(line, BrowserJson.Options);
            if (response is null || response.Id != id)
                return new(false, "Browser bridge response mismatch.");
            return new(response.Ok, response.Error, response.Data);
        }
        catch (Exception ex) when (ex is IOException or TimeoutException or OperationCanceledException)
        {
            await ResetConnectionAsync();
            return new(false, ex is OperationCanceledException && cancellationToken.IsCancellationRequested
                ? "Browser command cancelled."
                : "Browser bridge is not connected.");
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task EnsureConnectedAsync(CancellationToken cancellationToken)
    {
        if (_pipe?.IsConnected == true) return;
        await ResetConnectionAsync();
        _pipe = new NamedPipeClientStream(
            ".", PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        using var connectCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        connectCts.CancelAfter(TimeSpan.FromMilliseconds(800));
        await _pipe.ConnectAsync(connectCts.Token);
        _reader = new StreamReader(_pipe, leaveOpen: true);
        _writer = new StreamWriter(_pipe, leaveOpen: true) { AutoFlush = true };
    }

    private async Task ResetConnectionAsync()
    {
        _reader?.Dispose();
        if (_writer is not null) await _writer.DisposeAsync();
        if (_pipe is not null) await _pipe.DisposeAsync();
        _reader = null;
        _writer = null;
        _pipe = null;
    }

    public async ValueTask DisposeAsync()
    {
        await _gate.WaitAsync();
        try
        {
            await ResetConnectionAsync();
        }
        finally
        {
            _gate.Release();
            _gate.Dispose();
        }
    }
}

