using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using Jarvis.Browser;

const string ExpectedOrigin = "chrome-extension://cdlnajihofjpmochnpnkcomdifdjimgd/";
var callerOrigin = args.FirstOrDefault(x => x.StartsWith("chrome-extension://", StringComparison.OrdinalIgnoreCase));
if (callerOrigin is not null && !callerOrigin.Equals(ExpectedOrigin, StringComparison.OrdinalIgnoreCase))
{
    Console.Error.WriteLine($"Rejected native messaging origin: {callerOrigin}");
    return 2;
}

using var lifetime = new CancellationTokenSource();
var input = Console.OpenStandardInput();
var output = Console.OpenStandardOutput();
var nativeWriteGate = new SemaphoreSlim(1, 1);
var pending = new ConcurrentDictionary<string, TaskCompletionSource<BrowserBridgeResponse>>();

var nativeReader = Task.Run(() => ReadNativeLoopAsync(
    input, pending, lifetime.Token));
var pipeServer = Task.Run(() => RunPipeServerAsync(
    output, nativeWriteGate, pending, lifetime.Token));

await Task.WhenAny(nativeReader, pipeServer);
lifetime.Cancel();
try { await Task.WhenAll(nativeReader, pipeServer); } catch (OperationCanceledException) { }
return 0;
static async Task ReadNativeLoopAsync(
    Stream input,
    ConcurrentDictionary<string, TaskCompletionSource<BrowserBridgeResponse>> pending,
    CancellationToken cancellationToken)
{
    while (!cancellationToken.IsCancellationRequested)
    {
        var json = await ReadNativeMessageAsync(input, cancellationToken);
        if (json is null) return;

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var kind = root.TryGetProperty("kind", out var kindEl) ? kindEl.GetString() : null;
        if (kind == "hello")
        {
            Console.Error.WriteLine("Jarvis browser extension connected.");
            continue;
        }

        if (kind != "response" || !root.TryGetProperty("id", out var idEl)) continue;
        var id = idEl.GetString();
        if (string.IsNullOrWhiteSpace(id) || !pending.TryRemove(id, out var waiter)) continue;
        var response = JsonSerializer.Deserialize<BrowserBridgeResponse>(json, BrowserJson.Options);
        if (response is not null) waiter.TrySetResult(response);
    }
}
static async Task RunPipeServerAsync(
    Stream nativeOutput,
    SemaphoreSlim nativeWriteGate,
    ConcurrentDictionary<string, TaskCompletionSource<BrowserBridgeResponse>> pending,
    CancellationToken cancellationToken)
{
    while (!cancellationToken.IsCancellationRequested)
    {
        await using var pipe = new NamedPipeServerStream(
            BrowserCommandClient.PipeName,
            PipeDirection.InOut,
            1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous);
        await pipe.WaitForConnectionAsync(cancellationToken);

        using var reader = new StreamReader(pipe, Encoding.UTF8, false, 4096, leaveOpen: true);
        using var writer = new StreamWriter(pipe, new UTF8Encoding(false), 4096, leaveOpen: true)
        {
            AutoFlush = true
        };

        while (pipe.IsConnected && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null) break;
            var request = JsonSerializer.Deserialize<BrowserBridgeRequest>(line, BrowserJson.Options);
            if (request is null) continue;
            var response = await ForwardToExtensionAsync(
                request, nativeOutput, nativeWriteGate, pending, cancellationToken);
            var json = JsonSerializer.Serialize(response, BrowserJson.Options);
            await writer.WriteLineAsync(json.AsMemory(), cancellationToken);
        }
    }
}
static async Task<BrowserBridgeResponse> ForwardToExtensionAsync(
    BrowserBridgeRequest request,
    Stream nativeOutput,
    SemaphoreSlim nativeWriteGate,
    ConcurrentDictionary<string, TaskCompletionSource<BrowserBridgeResponse>> pending,
    CancellationToken cancellationToken)
{
    var waiter = new TaskCompletionSource<BrowserBridgeResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
    if (!pending.TryAdd(request.Id, waiter))
        return new(request.Id, false, "Duplicate browser request id.");

    try
    {
        var command = JsonSerializer.Serialize(new
        {
            kind = "command",
            id = request.Id,
            action = request.Action,
            argument = request.Argument
        }, BrowserJson.Options);
        await WriteNativeMessageAsync(nativeOutput, command, nativeWriteGate, cancellationToken);

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(3));
        return await waiter.Task.WaitAsync(timeout.Token);
    }
    catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
    {
        return new(request.Id, false, "Browser extension response timeout.");
    }
    finally
    {
        pending.TryRemove(request.Id, out _);
    }
}
static async Task<string?> ReadNativeMessageAsync(Stream input, CancellationToken cancellationToken)
{
    var lengthBuffer = new byte[4];
    if (!await ReadExactAsync(input, lengthBuffer, cancellationToken)) return null;
    var length = BinaryPrimitives.ReadInt32LittleEndian(lengthBuffer);
    if (length <= 0 || length > 64 * 1024 * 1024)
        throw new InvalidDataException($"Invalid native message length: {length}");

    var payload = new byte[length];
    if (!await ReadExactAsync(input, payload, cancellationToken)) return null;
    return Encoding.UTF8.GetString(payload);
}

static async Task<bool> ReadExactAsync(
    Stream stream,
    Memory<byte> buffer,
    CancellationToken cancellationToken)
{
    var offset = 0;
    while (offset < buffer.Length)
    {
        var read = await stream.ReadAsync(buffer[offset..], cancellationToken);
        if (read == 0) return false;
        offset += read;
    }
    return true;
}
static async Task WriteNativeMessageAsync(
    Stream output,
    string json,
    SemaphoreSlim gate,
    CancellationToken cancellationToken)
{
    var payload = Encoding.UTF8.GetBytes(json);
    if (payload.Length > 1024 * 1024)
        throw new InvalidDataException("Native response exceeds Chrome 1 MB limit.");

    var lengthBuffer = new byte[4];
    BinaryPrimitives.WriteInt32LittleEndian(lengthBuffer, payload.Length);
    await gate.WaitAsync(cancellationToken);
    try
    {
        await output.WriteAsync(lengthBuffer, cancellationToken);
        await output.WriteAsync(payload, cancellationToken);
        await output.FlushAsync(cancellationToken);
    }
    finally
    {
        gate.Release();
    }
}
