using Jarvis.Core;
using Jarvis.Plugins;

namespace Jarvis.Core.Tests;

public sealed class PluginRegistryTests
{
    [Fact]
    public void Register_AndResolve_Plugin()
    {
        var registry = new PluginRegistry();
        var plugin = new FakePlugin("system.test");

        registry.Register(plugin);

        Assert.True(registry.TryGet("SYSTEM.TEST", out var resolved));
        Assert.Same(plugin, resolved);
    }

    [Fact]
    public void Duplicate_Plugin_Id_Is_Rejected()
    {
        var registry = new PluginRegistry();
        registry.Register(new FakePlugin("duplicate"));

        Assert.Throws<InvalidOperationException>(() => registry.Register(new FakePlugin("duplicate")));
    }

    private sealed class FakePlugin(string id) : IJarvisPlugin
    {
        public PluginDescriptor Descriptor { get; } = new(id, new Version(1, 0), [], []);
        public ValueTask<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(new HealthCheckResult(HealthState.Healthy, "ok"));
    }
}
