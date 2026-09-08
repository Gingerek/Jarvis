using Jarvis.Core;

namespace Jarvis.Plugins;

public sealed class PluginRegistry
{
    private readonly Dictionary<string, IJarvisPlugin> _plugins = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<IJarvisPlugin> Plugins => _plugins.Values;

    public void Register(IJarvisPlugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);

        if (!_plugins.TryAdd(plugin.Descriptor.Id, plugin))
        {
            throw new InvalidOperationException($"Plugin '{plugin.Descriptor.Id}' is already registered.");
        }
    }

    public bool TryGet(string pluginId, out IJarvisPlugin? plugin) =>
        _plugins.TryGetValue(pluginId, out plugin);
}
