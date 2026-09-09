using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Jarvis.OBS;

public sealed record ObsRequestResult(
    bool Success,
    JsonElement? Data = null,
    int Code = 0,
    string? Error = null);

public sealed class ObsWebSocketClient : IAsyncDisposable
{
    private readonly Uri _endpoint;
    private readonly string? _password;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private ClientWebSocket? _socket;

    public ObsWebSocketClient(string? password, string endpoint = "ws://127.0.0.1:4455")
    {
        _password = password;
        _endpoint = new Uri(endpoint);
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (_socket?.State == WebSocketState.Open) return;
        if (_socket is not null) _socket.Dispose();
        _socket = new ClientWebSocket();
        _socket.Options.AddSubProtocol("obswebsocket.json");
        await _socket.ConnectAsync(_endpoint, cancellationToken);

        using var hello = JsonDocument.Parse(await ReceiveTextAsync(cancellationToken));
        if (hello.RootElement.GetProperty("op").GetInt32() != 0)
            throw new InvalidDataException("OBS did not send Hello.");
        var data = hello.RootElement.GetProperty("d");
        var rpc = data.GetProperty("rpcVersion").GetInt32();
        string? authentication = null;
        if (data.TryGetProperty("authentication", out var auth))
        {
            if (string.IsNullOrEmpty(_password))
                throw new InvalidOperationException("OBS WebSocket password is not configured.");
            authentication = CreateAuthentication(
                _password, auth.GetProperty("salt").GetString()!,
                auth.GetProperty("challenge").GetString()!);
        }

        var identifyData = new Dictionary<string, object?>
        {
            ["rpcVersion"] = Math.Min(rpc, 1),
            ["eventSubscriptions"] = 0
        };
        if (authentication is not null) identifyData["authentication"] = authentication;
        await SendAsync(new { op = 1, d = identifyData }, cancellationToken);

        using var identified = JsonDocument.Parse(await ReceiveTextAsync(cancellationToken));
        if (identified.RootElement.GetProperty("op").GetInt32() != 2)
            throw new InvalidDataException("OBS identification failed.");
    }

    public async Task<ObsRequestResult> RequestAsync(
        string requestType,
        object? requestData = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestType);
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(2));
        var requestToken = timeoutCts.Token;
        await _gate.WaitAsync(requestToken);
        try
        {
            await ConnectAsync(requestToken);
            var requestId = Guid.NewGuid().ToString("N");
            var payload = new Dictionary<string, object?>
            {
                ["requestType"] = requestType,
                ["requestId"] = requestId
            };
            if (requestData is not null) payload["requestData"] = requestData;
            await SendAsync(new { op = 6, d = payload }, requestToken);

            while (true)
            {
                using var message = JsonDocument.Parse(await ReceiveTextAsync(requestToken));
                if (message.RootElement.GetProperty("op").GetInt32() != 7) continue;
                var d = message.RootElement.GetProperty("d");
                if (d.GetProperty("requestId").GetString() != requestId) continue;
                var status = d.GetProperty("requestStatus");
                var success = status.GetProperty("result").GetBoolean();
                var code = status.GetProperty("code").GetInt32();
                var error = status.TryGetProperty("comment", out var comment)
                    ? comment.GetString() : null;
                var response = d.TryGetProperty("responseData", out var responseData)
                    ? responseData.Clone() : (JsonElement?)null;
                return new(success, response, code, error);
            }
        }
        catch (Exception ex) when (ex is WebSocketException or IOException ||
            ex is OperationCanceledException && !cancellationToken.IsCancellationRequested)
        {
            await ResetAsync();
            return new(false, null, 0, ex is OperationCanceledException
                ? "OBS WebSocket timed out." : "OBS WebSocket is unavailable.");
        }
        finally { _gate.Release(); }
    }

    private async Task SendAsync(object value, CancellationToken cancellationToken)
    {
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value));
        await _socket!.SendAsync(bytes, WebSocketMessageType.Text, true, cancellationToken);
    }

    private async Task<string> ReceiveTextAsync(CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream();
        var buffer = new byte[8192];
        while (true)
        {
            var result = await _socket!.ReceiveAsync(buffer, cancellationToken);
            if (result.MessageType == WebSocketMessageType.Close)
                throw new WebSocketException($"OBS closed WebSocket: {_socket.CloseStatus}.");
            stream.Write(buffer, 0, result.Count);
            if (result.EndOfMessage) break;
        }
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static string CreateAuthentication(string password, string salt, string challenge)
    {
        var secret = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password + salt)));
        return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(secret + challenge)));
    }

    private async Task ResetAsync()
    {
        if (_socket is not null) _socket.Dispose();
        _socket = null;
    }

    public async ValueTask DisposeAsync()
    {
        await _gate.WaitAsync();
        try { await ResetAsync(); }
        finally
        {
            _gate.Release();
            _gate.Dispose();
        }
    }
}
