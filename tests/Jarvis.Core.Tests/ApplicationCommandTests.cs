using Jarvis.Commands;
using Jarvis.Execution;

namespace Jarvis.Core.Tests;

public sealed class ApplicationCommandTests
{
    [Theory]
    [InlineData("otwórz Lightroom", "lightroom")]
    [InlineData("włącz Lightroom Classic", "lightroom classic")]
    [InlineData("uruchom Adobe Lightroom", "adobe lightroom")]
    [InlineData("open Lightroom", "lightroom")]
    [InlineData("Różom Lightroom", "lightroom")]
    public void Parser_Recognizes_Open_Verbs_And_Observed_Asr_Confusion(string text, string expected)
    {
        var command = new OpenApplicationCommandParser().Parse(text);
        Assert.NotNull(command);
        Assert.Equal(expected, command.RequestedName);
    }

    [Fact]
    public void Launcher_Does_Not_Start_Missing_Executable()
    {
        var target = new ApplicationTarget(
            "missing", "Missing", @"C:\definitely-missing\app.exe", "missing", []);
        var result = new ApplicationLauncher().Launch(target);
        Assert.Equal(ApplicationLaunchStatus.ExecutableNotFound, result.Status);
    }
}
