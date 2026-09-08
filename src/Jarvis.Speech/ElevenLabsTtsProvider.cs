using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace Jarvis.Speech;

public sealed class ElevenLabsTtsProvider : ITtsProvider
{
    private readonly string _apiKey;
    private readonly string _modelId;
    private bool _disposed;

    public ElevenLabsTtsProvider(
        string apiKey,
        string voiceId,
        string modelId = "eleven_flash_v2_5")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(voiceId);
        _apiKey = apiKey;
        VoiceId = voiceId;
        _modelId = modelId;
    }

    public string ProviderName => "ElevenLabs";
    public string VoiceId { get; }
    public async IAsyncEnumerable<ReadOnlyMemory<byte>> StreamAsync(
        string text,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        using var socket = new ClientWebSocket();
        socket.Options.SetRequestHeader("xi-api-key", _apiKey);
        var uri = new Uri(
            $"wss://api.elevenlabs.io/v1/text-to-speech/{VoiceId}/stream-input" +
            $"?model_id={_modelId}&output_format=pcm_24000&language_code=pl");
        await socket.ConnectAsync(uri, cancellationToken);

        await SendJsonAsync(socket, new
        {
            text = " ",
            voice_settings = new { stability = 0.5, similarity_boost = 0.8, speed = 1.0 }
        }, cancellationToken);
        await SendJsonAsync(socket, new { text = text + " " }, cancellationToken);
        await SendJsonAsync(socket, new { text = "" }, cancellationToken);
        while (socket.State == WebSocketState.Open)
        {
            var message = await ReceiveTextAsync(socket, cancellationToken);
            if (message is null) yield break;
            using var json = JsonDocument.Parse(message);
            var root = json.RootElement;

            if (root.TryGetProperty("audio", out var audioElement))
            {
                var encoded = audioElement.GetString();
                if (!string.IsNullOrWhiteSpace(encoded))
                    yield return Convert.FromBase64String(encoded);
            }

            if (root.TryGetProperty("is_final", out var finalElement) &&
                finalElement.ValueKind == JsonValueKind.True)
                yield break;
        }
    }

    private static async Task SendJsonAsync(
        ClientWebSocket socket,
        object payload,
        CancellationToken cancellationToken)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(payload);
        await socket.SendAsync(bytes, WebSocketMessageType.Text, true, cancellationToken);
    }
    private static async Task<string?> ReceiveTextAsync(
        ClientWebSocket socket,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[8192];
        using var stream = new MemoryStream();
        while (true)
        {
            var result = await socket.ReceiveAsync(buffer, cancellationToken);
            if (result.MessageType == WebSocketMessageType.Close) return null;
            stream.Write(buffer, 0, result.Count);
            if (result.EndOfMessage) break;
        }
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    public ValueTask DisposeAsync()
    {
        _disposed = true;
        return ValueTask.CompletedTask;
    }
}
