using Jarvis.Lightroom;

var command = args.Length > 0 ? args[0] : "ping";
var argument1 = args.Length > 1 ? args[1] : null;
var argument2 = args.Length > 2 ? args[2] : null;

await using var client = new LightroomBridgeClient();
var result = await client.SendAsync(command, argument1, argument2);
Console.WriteLine($"SUCCESS={result.Success}");
if (result.Success)
    Console.WriteLine($"VALUE={result.Value}");
else
    Console.WriteLine($"ERROR={result.Error}");
