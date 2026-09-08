using System.IO.Pipes;
using System.Text.Json;

namespace Jarvis.VoiceHost;

public sealed class HostEventSink : IAsyncDisposable
{
    private readonly NamedPipeServerStream? _pipe;
    private readonly StreamWriter? _writer;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private HostEventSink(NamedPipeServerStream? pipe, StreamWriter? writer)
    {
        _pipe = pipe;
        _writer = writer;
    }

    public static async Task<HostEventSink> CreateAsync(
        string? pipeName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pipeName))
            return new HostEventSink(null, null);

        var pipe = new NamedPipeServerStream(
            pipeName, PipeDirection.Out, 1,
            PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        await pipe.WaitForConnectionAsync(cancellationToken);
        var writer = new StreamWriter(pipe) { AutoFlush = true };
        return new HostEventSink(pipe, writer);
    }
    public async Task EmitAsync(
        string type,
        string? state = null,
        string? text = null,
        object? data = null)
    {
        var payload = JsonSerializer.Serialize(new
        {
            type,
            state,
            text,
            data,
            timestamp = DateTimeOffset.UtcNow
        });

        Console.WriteLine(payload);
        if (_writer is null) return;

        await _gate.WaitAsync();
        try
        {
            await _writer.WriteLineAsync(payload);
        }
        catch (IOException)
        {
            // UI disconnected; host continues safely.
        }
        finally
        {
            _gate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        _writer?.Dispose();
        if (_pipe is not null) await _pipe.DisposeAsync();
        _gate.Dispose();
    }
}
