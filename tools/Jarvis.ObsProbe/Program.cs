using Jarvis.OBS;
using Jarvis.Security;

var password = new WindowsCredentialSecretStore().Read("ObsWebSocketPassword");
await using var client = new ObsWebSocketClient(password);

async Task<ObsRequestResult> Probe(string request, object? data = null)
{
    var result = await client.RequestAsync(request, data);
    Console.WriteLine($"{request}: OK={result.Success} CODE={result.Code}");
    if (!result.Success) Console.WriteLine($"ERROR={result.Error}");
    return result;
}

var version = await Probe("GetVersion");
if (version.Data is { } v)
    Console.WriteLine($"OBS={v.GetProperty("obsVersion").GetString()} WS={v.GetProperty("obsWebSocketVersion").GetString()}");

var record = await Probe("GetRecordStatus");
if (record.Data is { } r) Console.WriteLine($"RECORDING={r.GetProperty("outputActive").GetBoolean()}");
var stream = await Probe("GetStreamStatus");
if (stream.Data is { } st) Console.WriteLine($"STREAMING={st.GetProperty("outputActive").GetBoolean()}");

var scene = await Probe("GetCurrentProgramScene");
var sceneName = scene.Data?.GetProperty("sceneName").GetString();
Console.WriteLine($"SCENE={sceneName}");
var inputs = await Probe("GetInputList");
if (inputs.Data is { } i)
{
    var names = i.GetProperty("inputs").EnumerateArray()
        .Select(x => x.GetProperty("inputName").GetString())
        .Where(x => !string.IsNullOrWhiteSpace(x)).Cast<string>().ToArray();
    Console.WriteLine("INPUTS=" + string.Join(" | ", names));
    foreach (var name in names.Where(x => x.Contains("Mikrofon", StringComparison.OrdinalIgnoreCase) || x.Contains("audio", StringComparison.OrdinalIgnoreCase)))
    {
        var mute = await Probe("GetInputMute", new { inputName = name });
        if (mute.Data is { } m) Console.WriteLine($"INPUT_MUTE={name}|{m.GetProperty("inputMuted").GetBoolean()}");
    }
}

if (!string.IsNullOrWhiteSpace(sceneName))
{
    var items = await Probe("GetSceneItemList", new { sceneName });
    if (items.Data is { } si)
    {
        var itemsData = si.GetProperty("sceneItems").EnumerateArray().ToArray();
        foreach (var item in itemsData)
            Console.WriteLine($"SCENE_ITEM={item.GetProperty("sourceName").GetString()}|ID={item.GetProperty("sceneItemId").GetInt32()}|ENABLED={item.GetProperty("sceneItemEnabled").GetBoolean()}");
    }
}
