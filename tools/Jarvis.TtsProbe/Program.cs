using System.Text.Json;
using Jarvis.Security;
using Jarvis.Speech;

var settingsPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "Jarvis", "config", "settings.json");

using var settings = JsonDocument.Parse(await File.ReadAllTextAsync(settingsPath));
var root = settings.RootElement;
var voiceId = root.TryGetProperty("TtsVoiceId", out var voiceElement)
    ? voiceElement.GetString()
    : null;
var modelId = root.TryGetProperty("TtsModelId", out var modelElement)
    ? modelElement.GetString() ?? "eleven_flash_v2_5"
    : "eleven_flash_v2_5";

var secrets = new WindowsCredentialSecretStore();
var apiKey = secrets.Read("ElevenLabsApiKey");
if (string.IsNullOrWhiteSpace(apiKey))
{
    Console.Error.WriteLine("ElevenLabs API key is not configured in Windows Credential Manager.");
    return 2;
}
if (string.IsNullOrWhiteSpace(voiceId))
{
    Console.Error.WriteLine("TtsVoiceId is not configured in settings.json.");
    return 3;
}

var text = args.Length == 0
    ? "Jestem gotowy."
    : string.Join(' ', args);

await using var provider = new ElevenLabsTtsProvider(apiKey, voiceId, modelId);
await using var output = new SpeechOutputManager(provider);
Console.WriteLine($"TTS: ElevenLabs / {modelId} / voice={voiceId}");
await output.SpeakAsync(text);
return 0;
