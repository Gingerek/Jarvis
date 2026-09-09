using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Jarvis.Lightroom;

public sealed record LightroomCommandResult(
    bool Success,
    string? Value = null,
    string? Error = null);

public sealed class LightroomBridgeClient : IAsyncDisposable
{
    private readonly string _bridgeDirectory;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private TcpClient? _commandClient;
    private TcpClient? _responseClient;
    private StreamWriter? _writer;
    private StreamReader? _reader;

    public LightroomBridgeClient(string? bridgeDirectory = null)
    {
        _bridgeDirectory = bridgeDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Jarvis", "lightroom");
    }

    public async Task<LightroomCommandResult> SendAsync(
        string command,
        string? argument1 = null,
        string? argument2 = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command);
        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_commandClient?.Connected != true || _responseClient?.Connected != true)
            {
                using var connectTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                connectTimeout.CancelAfter(TimeSpan.FromSeconds(12));
                await EnsureConnectedAsync(connectTimeout.Token);
            }

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeoutFor(command));
            var id = Guid.NewGuid().ToString("N");
            var line = string.Join('|', id, Clean(command), Clean(argument1), Clean(argument2));
            await _writer!.WriteLineAsync(line.AsMemory(), timeout.Token);
            await _writer.FlushAsync(timeout.Token);

            while (true)
            {
                var response = await _reader!.ReadLineAsync(timeout.Token);
                if (response is null) throw new IOException("Lightroom bridge closed the response channel.");
                var parts = response.Split('|', 3);
                if (parts.Length < 2 || !parts[0].Equals(id, StringComparison.Ordinal)) continue;
                var value = parts.Length == 3 ? parts[2] : string.Empty;
                return parts[1].Equals("ok", StringComparison.OrdinalIgnoreCase)
                    ? new(true, value)
                    : new(false, null, value);
            }
        }
        catch (Exception ex) when (ex is IOException or SocketException or OperationCanceledException)
        {
            Reset();
            return new(false, null, ex is OperationCanceledException
                ? "Lightroom bridge timed out."
                : "Lightroom bridge is unavailable.");
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task EnsureConnectedAsync(CancellationToken cancellationToken)
    {
        if (_commandClient?.Connected == true && _responseClient?.Connected == true)
            return;

        Reset();
        Directory.CreateDirectory(_bridgeDirectory);
        var commandPortPath = Path.Combine(_bridgeDirectory, "bridge.port1");
        var responsePortPath = Path.Combine(_bridgeDirectory, "bridge.port2");

        // Response ports are ephemeral per command-channel session. Never trust
        // a leftover port2 file from a previous Lightroom/plugin connection.
        try { if (File.Exists(responsePortPath)) File.Delete(responsePortPath); } catch { }

        _commandClient = await ConnectFromPortFileAsync(commandPortPath, cancellationToken);
        _responseClient = await ConnectFromPortFileAsync(responsePortPath, cancellationToken);
        _writer = new StreamWriter(
            _commandClient.GetStream(), new UTF8Encoding(false), leaveOpen: true)
        {
            NewLine = "\n",
            AutoFlush = true
        };
        _reader = new StreamReader(
            _responseClient.GetStream(), new UTF8Encoding(false),
            detectEncodingFromByteOrderMarks: false, leaveOpen: true);
    }

    private static async Task<TcpClient> ConnectFromPortFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        Exception? last = null;
        while (!cancellationToken.IsCancellationRequested)
        {
            if (File.Exists(path) && int.TryParse(
                (await File.ReadAllTextAsync(path, cancellationToken)).Trim(), out var port))
            {
                var client = new TcpClient(AddressFamily.InterNetwork);
                try
                {
                    await client.ConnectAsync(IPAddress.Loopback, port, cancellationToken);
                    return client;
                }
                catch (Exception ex) when (ex is SocketException or OperationCanceledException)
                {
                    last = ex;
                    client.Dispose();
                    if (ex is OperationCanceledException) throw;
                }
            }
            await Task.Delay(50, cancellationToken);
        }

        throw last ?? new TimeoutException($"Lightroom port file unavailable: {path}");
    }

    private static TimeSpan TimeoutFor(string command) => command switch
    {
        "mask_create_ai" => TimeSpan.FromSeconds(30),
        "mask_reset" or "mask_overlay" => TimeSpan.FromSeconds(10),
        "undo" or "redo" => TimeSpan.FromSeconds(10),
        "paste_develop" => TimeSpan.FromSeconds(10),
        _ => TimeSpan.FromSeconds(3)
    };

    private static string Clean(string? value) =>
        (value ?? string.Empty).Replace('|', '/').Replace("\r", " ").Replace("\n", " ");

    private void Reset()
    {
        try { _writer?.Dispose(); } catch { }
        try { _reader?.Dispose(); } catch { }
        try { _commandClient?.Dispose(); } catch { }
        try { _responseClient?.Dispose(); } catch { }
        _writer = null;
        _reader = null;
        _commandClient = null;
        _responseClient = null;
    }

    public async ValueTask DisposeAsync()
    {
        await _gate.WaitAsync();
        try { Reset(); }
        finally
        {
            _gate.Release();
            _gate.Dispose();
        }
    }
}
