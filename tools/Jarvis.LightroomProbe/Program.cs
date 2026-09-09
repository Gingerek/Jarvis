using Jarvis.Lightroom;

await using var client = new LightroomBridgeClient();
if (args.Length > 0 && args[0] is "batch-range" or "batch-check")
{
    foreach (var parameter in args.Skip(1).Distinct(StringComparer.Ordinal))
    {
        if (args[0] == "batch-check")
        {
            var value = await client.SendAsync("develop_get", parameter);
            var range = await client.SendAsync("develop_range", parameter);
            var ok = value.Success && range.Success;
            Console.WriteLine($"{parameter}|{(ok ? "OK" : "FAIL")}|value={(value.Success ? value.Value : value.Error)}|range={(range.Success ? range.Value : range.Error)}");
        }
        else
        {
            var result = await client.SendAsync("develop_range", parameter);
            Console.WriteLine($"{parameter}|{(result.Success ? "OK" : "FAIL")}|{(result.Success ? result.Value : result.Error)}");
        }
    }
    return;
}

var command = args.Length > 0 ? args[0] : "ping";
var argument1 = args.Length > 1 ? args[1] : null;
var argument2 = args.Length > 2 ? args[2] : null;
var single = await client.SendAsync(command, argument1, argument2);
Console.WriteLine($"SUCCESS={single.Success}");
Console.WriteLine(single.Success ? $"VALUE={single.Value}" : $"ERROR={single.Error}");