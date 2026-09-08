using System.Reflection;
using Jarvis.Core;

namespace Jarvis.Plugins;

public sealed class PluginLoader
{
    public IReadOnlyList<IJarvisPlugin> Discover(string directory)
    {
        if (!Directory.Exists(directory)) return [];

        var plugins = new List<IJarvisPlugin>();
        foreach (var dll in Directory.EnumerateFiles(directory, "*.dll", SearchOption.TopDirectoryOnly))
        {
            try
            {
                var assembly = Assembly.LoadFrom(dll);
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsAbstract || type.IsInterface || !typeof(IJarvisPlugin).IsAssignableFrom(type)) continue;
                    if (Activator.CreateInstance(type) is IJarvisPlugin plugin) plugins.Add(plugin);
                }
            }
            catch (BadImageFormatException) { }
            catch (FileLoadException) { }
            catch (ReflectionTypeLoadException) { }
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
