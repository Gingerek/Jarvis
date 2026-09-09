using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace Jarvis.ASR;

public sealed class PythonFasterWhisperAsrEngine : IAsrEngine
{
    private readonly Process _process;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly string _initialPrompt;
    private bool _disposed;

    private PythonFasterWhisperAsrEngine(Process process, string modelName, string initialPrompt)
    {
        _process = process;
        ModelName = modelName;
        _initialPrompt = initialPrompt;
    }

    public string ModelName { get; }

    public static async Task<PythonFasterWhisperAsrEngine> CreateAsync(
        string pythonPath, string workerPath, string modelsDirectory,
        string modelName = "base", CancellationToken cancellationToken = default)
    {
        var psi = new ProcessStartInfo
        {
            FileName = pythonPath,
            Arguments = $"\"{workerPath}\" {modelName} \"{modelsDirectory}\"",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardInputEncoding = new UTF8Encoding(false),
            StandardOutputEncoding = new UTF8Encoding(false),
            StandardErrorEncoding = new UTF8Encoding(false),
        };
        psi.Environment["PYTHONIOENCODING"] = "utf-8";

        var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start faster-whisper worker.");

        var readyLine = await process.StandardOutput.ReadLineAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(readyLine))
            throw new InvalidOperationException("ASR worker did not report readiness.");

        using var ready = JsonDocument.Parse(readyLine);
        if (ready.RootElement.GetProperty("type").GetString() != "ready")
            throw new InvalidOperationException($"Unexpected ASR worker response: {readyLine}");

        var prompt = "Jarvis. Otwórz. Włącz. Uruchom. Odpal. Zamknij. Wyłącz. Wyszukaj. Znajdź. Głośniej. Ciszej. Wycisz. Odcisz. Głośność. Minimalizuj. Maksymalizuj. Przywróć okno. Pokaż pulpit. Zablokuj komputer. Open. Close. Lightroom. Lightroom Classic. YouTube. Google. Marktplaats. DaVinci Resolve. OBS Studio. Ekspozycja. Kontrast. Światła. Cienie. Biele. Czernie. Tekstura. Przejrzystość. Odmglenie. Wibracja. Nasycenie. Temperatura. Odcień. Auto Tone. Auto balans bieli. OBS. Nagrywanie. Stream. Transmisja. Scena. Przełącz scenę. Status OBS. Zacznij nagrywanie. Zatrzymaj nagrywanie. Pauza nagrywania. Wznów nagrywanie. Lista wejść OBS. Wycisz mikrofon w OBS. Odcisz mikrofon w OBS. Pokaż źródło. Ukryj źródło. Kamera jeden. Kamera dwa. Google Chrome. Microsoft Edge. Notatnik. Menedżer zadań.  Karta. Zakładka. Nowa karta. Zamknij kartę. Następna karta. Poprzednia karta. Odśwież stronę. Przewiń w dół. Przewiń w górę. Wycisz kartę. Pauza. Przykład: Jarvis, otwórz YouTube.";
        return new PythonFasterWhisperAsrEngine(process, modelName, prompt);
    }
    public async Task<AsrTranscript> TranscribeAsync(
        ReadOnlyMemory<short> pcm16,
        string language = "pl",
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (pcm16.IsEmpty) throw new ArgumentException("Audio cannot be empty.", nameof(pcm16));

        await _gate.WaitAsync(cancellationToken);
        try
        {
            var bytes = MemoryMarshal.AsBytes(pcm16.Span).ToArray();
            var id = Guid.NewGuid().ToString("N");
            var request = JsonSerializer.Serialize(new
            {
                id,
                pcm16_b64 = Convert.ToBase64String(bytes),
                language,
                initial_prompt = _initialPrompt,
            });

            await _process.StandardInput.WriteLineAsync(request.AsMemory(), cancellationToken);
            await _process.StandardInput.FlushAsync(cancellationToken);
            var line = await _process.StandardOutput.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line))
                throw new InvalidOperationException("ASR worker returned no response.");
            using var response = JsonDocument.Parse(line);
            var root = response.RootElement;
            if (root.TryGetProperty("error", out var error))
                throw new InvalidOperationException($"ASR worker error: {error.GetString()}");
            if (root.GetProperty("id").GetString() != id)
                throw new InvalidOperationException("ASR worker response ID mismatch.");

            return new AsrTranscript(
                root.GetProperty("text").GetString() ?? string.Empty,
                root.GetProperty("language").GetString() ?? language,
                root.GetProperty("language_probability").GetDouble(),
                root.GetProperty("inference_ms").GetDouble());
        }
        finally
        {
            _gate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        try { _process.StandardInput.Close(); } catch { }
        if (!_process.HasExited) _process.Kill(entireProcessTree: true);
        await _process.WaitForExitAsync();
        _process.Dispose();
        _gate.Dispose();
    }
}

