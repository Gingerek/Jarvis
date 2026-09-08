using Jarvis.Core;

namespace Jarvis.Core.Tests;

public sealed class SettingsLoaderTests
{
    [Fact]
    public async Task LoadOrCreate_Creates_Default_Settings_File()
    {
        var path = Path.Combine(Path.GetTempPath(), $"jarvis-settings-{Guid.NewGuid():N}.json");
        try
        {
            var settings = await SettingsLoader.LoadOrCreateAsync(path);
            Assert.Equal("pl-PL", settings.Language);
            Assert.Equal("Jarvis", settings.WakeWord);
            Assert.True(File.Exists(path));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
