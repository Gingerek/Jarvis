using System.Text.Json;
using Jarvis.Browser;

var action = args.Length > 0 ? args[0] : "ping";
var argument = args.Length > 1 ? string.Join(' ', args.Skip(1)) : null;

await using var client = new BrowserCommandClient();
var result = await client.SendAsync(action, argument);
Console.WriteLine($"SUCCESS={result.Success}");
if (!string.IsNullOrWhiteSpace(result.Error))
    Console.WriteLine($"ERROR={result.Error}");
if (result.Data is JsonElement data)
    Console.WriteLine($"DATA={data.GetRawText()}");
return result.Success ? 0 : 1;
