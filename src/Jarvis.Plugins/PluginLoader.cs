using System.Reflection;
using Jarvis.Core;

namespace Jarvis.Plugins;

public sealed class PluginLoader
{
    public IReadOnlyList<IJarvisPlugin> Discover(string directory)
    {
        if (!Directory.Exists(directory)) return [];

        var plugins = new List<IJarvisPlugin>();
        foreach (var manifestPath in Directory.EnumerateFiles(directory, "*.plugin.json", SearchOption.TopDirectoryOnly))
        {
            var manifest = PluginManifestReader.Read(manifestPath);
            var assemblyPath = Path.Combine(directory, manifest.Assembly);
            if (!File.Exists(assemblyPath)) throw new FileNotFoundException("Plugin assembly was not found.", assemblyPath);

            var assembly = Assembly.LoadFrom(assemblyPath);
            var instances = assembly.GetTypes()
                .Where(type => !type.IsAbstract && !type.IsInterface && typeof(IJarvisPlugin).IsAssignableFrom(type))
                .Select(type => Activator.CreateInstance(type))
                .OfType<IJarvisPlugin>()
                .Where(plugin => string.Equals(plugin.Descriptor.Id, manifest.Id, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (instances.Length != 1)
                throw new InvalidDataException($"Plugin manifest '{manifest.Id}' must resolve to exactly one matching plugin type.");

            if (instances[0].Descriptor.Version != Version.Parse(manifest.Version))
                throw new InvalidDataException($"Plugin '{manifest.Id}' version does not match its manifest.");

            plugins.Add(instances[0]);
        }
        return plugins;
    }

    public int RegisterDiscovered(string directory, PluginRegistry registry)
    {
        var count = 0;
        foreach (var plugin in Discover(directory))
        {
            registry.Register(plugin);
            count++;
        }
        return count;
    }
}
