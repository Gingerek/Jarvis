using Jarvis.Plugins;

namespace Jarvis.Core.Tests;

public sealed class PluginLoaderTests
{
    [Fact]
    public void Discover_Returns_Empty_For_Missing_Directory()
    {
        var loader = new PluginLoader();
        var path = Path.Combine(Path.GetTempPath(), $"jarvis-plugins-{Guid.NewGuid():N}");

        var plugins = loader.Discover(path);

        Assert.Empty(plugins);
    }
}
