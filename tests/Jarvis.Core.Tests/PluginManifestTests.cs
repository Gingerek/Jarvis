using Jarvis.Core;

namespace Jarvis.Core.Tests;

public sealed class PluginManifestTests
{
    [Fact]
    public void Read_Rejects_Assembly_Path_Traversal()
    {
        var path = Path.Combine(Path.GetTempPath(), $"jarvis-plugin-{Guid.NewGuid():N}.plugin.json");
        try
        {
            File.WriteAllText(path, """
                {"Id":"test.plugin","Assembly":"..\\evil.dll","Version":"1.0.0","Capabilities":[],"Permissions":[]}
                """);

            Assert.Throws<InvalidDataException>(() => PluginManifestReader.Read(path));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
