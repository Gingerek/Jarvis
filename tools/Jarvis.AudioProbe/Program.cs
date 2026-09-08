using Jarvis.Audio;

var service = new WasapiDeviceService();
var devices = service.GetCaptureDevices();

Console.WriteLine($"Capture devices: {devices.Count}");
foreach (var device in devices)
{
    Console.WriteLine($"[{(device.IsDefault ? "DEFAULT" : "       ")}] {device.Name}");
    Console.WriteLine($"  ID: {device.Id}");
    Console.WriteLine($"  Mix: {device.SampleRate} Hz / {device.Channels} ch / {device.BitsPerSample} bit");
}

if (args.Contains("--scan", StringComparer.OrdinalIgnoreCase))
{
    foreach (var device in devices)
    {
        Console.WriteLine($"SCAN: {device.Name}");
        using var capture = new WasapiCaptureSession(device.Id, 524288);
        var metrics = await capture.CaptureForAsync(TimeSpan.FromSeconds(2));
        Console.WriteLine($"  Peak={metrics.Peak:F6} RMS={metrics.Rms:F6} Frames={metrics.Frames}");
        Console.WriteLine($"  First={metrics.FirstFrameLatencyMs:F1}ms MeanInterval={metrics.MeanCallbackIntervalMs:F1}ms MaxJitter={metrics.MaxCallbackJitterMs:F1}ms");
    }
    return;
}

if (args.Contains("--capture", StringComparer.OrdinalIgnoreCase))
{
    var device = service.GetDefaultCaptureDevice();
    Console.WriteLine($"Capturing DEFAULT: {device.Name}");
    using var capture = new WasapiCaptureSession(device.Id);
    Console.WriteLine($"Actual format: {capture.Format}");
    var metrics = await capture.CaptureForAsync(TimeSpan.FromSeconds(4));
    Console.WriteLine($"Duration: {metrics.Duration.TotalMilliseconds:F0} ms");
    Console.WriteLine($"Frames: {metrics.Frames}, Bytes: {metrics.Bytes}");
    Console.WriteLine($"Peak: {metrics.Peak:F6}, RMS: {metrics.Rms:F6}");
    Console.WriteLine($"First={metrics.FirstFrameLatencyMs:F1}ms MeanInterval={metrics.MeanCallbackIntervalMs:F1}ms MaxJitter={metrics.MaxCallbackJitterMs:F1}ms");
    Console.WriteLine($"RingBuffer: {capture.Buffer.Count}/{capture.Buffer.Capacity} bytes");
}
