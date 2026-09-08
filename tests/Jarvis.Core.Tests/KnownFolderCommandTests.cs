using Jarvis.Commands;

namespace Jarvis.Core.Tests;

public sealed class KnownFolderCommandTests
{
    [Theory]
    [InlineData("otwórz Pobrane", "downloads")]
    [InlineData("pokaż mi Dokumenty", "documents")]
    [InlineData("otwórz Zdjęcia", "pictures")]
    [InlineData("przejdź do Muzyka", "music")]
    [InlineData("otwórz Wideo", "videos")]
    [InlineData("otwórz folder Jarvis", "jarvis")]
    public void Registry_Parses_Known_Folders(string text, string expected)
    {
        var result = new CommandRegistry().Parse(text);
        Assert.NotNull(result);
        Assert.Equal(CommandIntent.OpenFolder, result.Intent);
        Assert.Equal(expected, result.Argument);
    }
}
