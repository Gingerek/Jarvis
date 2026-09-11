using System.Net.Http.Json;
using System.Text.Json;

namespace Jarvis.Vision;

public sealed record VisionAnalysisResult(bool Success, string Message);

public sealed class OllamaVisionClient
{
    private readonly HttpClient _client;
    private readonly string _model;

    public OllamaVisionClient(string model = "qwen2.5vl:3b", HttpClient? client = null)
    {
        _model = model;
        _client = client ?? new HttpClient
        {
            BaseAddress = new Uri("http://127.0.0.1:11434"),
            Timeout = TimeSpan.FromSeconds(75)
        };
    }

    public async Task<VisionAnalysisResult> AnalyzeAsync(byte[] jpeg, string prompt, CancellationToken cancellationToken = default)
    {
        if (jpeg.Length == 0) return new(false, "Nie mam obrazu do analizy.");
        var request = new
        {
            model = _model,
            stream = false,
            keep_alive = "30m",
            options = new { num_predict = 120, temperature = 0 },
            messages = new[]
            {
                new { role = "user", content = prompt, images = new[] { Convert.ToBase64String(jpeg) } }
            }
        };
        try
        {
            using var response = await _client.PostAsJsonAsync("/api/chat", request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return new(false, $"Lokalny model vision zwrócił błąd {response.StatusCode}.");

            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var content = json.RootElement.GetProperty("message").GetProperty("content").GetString();
            return string.IsNullOrWhiteSpace(content)
                ? new(false, "Model vision nie zwrócił opisu.")
                : new(true, content.Trim());
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new(false, "Analiza obrazu trwała zbyt długo.");
        }
        catch (HttpRequestException)
        {
            return new(false, "Nie mogę połączyć się z lokalnym Ollama.");
        }
        catch (Exception ex)
        {
            return new(false, $"Analiza obrazu nie powiodła się: {ex.Message}");
        }
    }
    public async Task WarmUpAsync(CancellationToken cancellationToken = default)
    {
        var request = new
        {
            model = _model,
            stream = false,
            keep_alive = "30m",
            options = new { num_predict = 1, temperature = 0 },
            messages = new[] { new { role = "user", content = "Odpowiedz tylko: OK" } }
        };
        try
        {
            using var response = await _client.PostAsJsonAsync("/api/chat", request, cancellationToken);
        }
        catch { }
    }

}
