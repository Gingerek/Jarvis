namespace Jarvis.Core;

public enum HealthState
{
    Healthy,
    Degraded,
    Unavailable
}

public sealed record HealthCheckResult(HealthState State, string Message);

public sealed record PluginDescriptor(
    string Id,
    Version Version,
    IReadOnlyCollection<string> Capabilities,
    IReadOnlyCollection<string> Permissions);

public interface IJarvisPlugin
{
    PluginDescriptor Descriptor { get; }
    ValueTask<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default);
}
