using System.Net.Http.Headers;
using System.Text.Json;
using Jarvis.Security;

var secrets = new WindowsCredentialSecretStore();
var apiKey = secrets.Read("ElevenLabsApiKey");
if (string.IsNullOrWhiteSpace(apiKey))
{
    Console.Error.WriteLine("ElevenLabsApiKey is missing.");
    return 2;
}

using var http = new HttpClient();
http.DefaultRequestHeaders.Add("xi-api-key", apiKey);
http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

using var response = await http.GetAsync("https://api.elevenlabs.io/v2/voices?page_size=100");
Console.WriteLine($"HTTP {(int)response.StatusCode}");
if (!response.IsSuccessStatusCode)
{
    Console.Error.WriteLine(await response.Content.ReadAsStringAsync());
    return 3;
}

using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
if (!json.RootElement.TryGetProperty("voices", out var voices))
    return 4;
var candidates = new List<(int Score, string Name, string Id, string Category, string Labels, string Description, string Preview)>();
foreach (var voice in voices.EnumerateArray())
{
    var name = GetString(voice, "name");
    var id = GetString(voice, "voice_id");
    var category = GetString(voice, "category");
    var description = GetString(voice, "description");
    var preview = GetString(voice, "preview_url");
    var labels = voice.TryGetProperty("labels", out var labelElement)
        ? string.Join(", ", labelElement.EnumerateObject().Select(x => $"{x.Name}={x.Value.ToString()}"))
        : string.Empty;

    var haystack = $"{name} {category} {description} {labels}".ToLowerInvariant();
    var score = 0;
    if (haystack.Contains("male")) score += 30;
    if (haystack.Contains("deep") || haystack.Contains("baritone") || haystack.Contains("low")) score += 20;
    if (haystack.Contains("calm") || haystack.Contains("warm") || haystack.Contains("smooth")) score += 12;
    if (haystack.Contains("narration") || haystack.Contains("narrator")) score += 8;
    if (haystack.Contains("polish") || haystack.Contains("pl-pl")) score += 15;
    candidates.Add((score, name, id, category, labels, description, preview));
}

Console.WriteLine($"VOICES={candidates.Count}");
foreach (var c in candidates.OrderByDescending(x => x.Score).ThenBy(x => x.Name).Take(20))
    Console.WriteLine($"{c.Score}|{c.Name}|{c.Id}|{c.Category}|{c.Labels}|{c.Description}|{c.Preview}");
return 0;

static string GetString(JsonElement element, string property)
{
    if (!element.TryGetProperty(property, out var value)) return string.Empty;
    return value.ValueKind == JsonValueKind.String ? value.GetString() ?? string.Empty : value.ToString();
}
