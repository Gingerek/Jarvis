using Jarvis.Commands;

namespace Jarvis.Core.Tests;

public sealed class ObsCommandParserTests
{
    [Theory]
    [InlineData("status OBS", CommandIntent.ObsStatus)]
    [InlineData("czy OBS nagrywa", CommandIntent.ObsRecordStatus)]
    [InlineData("status streamu", CommandIntent.ObsStreamStatus)]
    [InlineData("jaka scena jest aktywna", CommandIntent.ObsCurrentScene)]
    [InlineData("lista scen OBS", CommandIntent.ObsListScenes)]
    [InlineData("zacznij nagrywanie", CommandIntent.ObsStartRecord)]
    [InlineData("zatrzymaj nagrywanie", CommandIntent.ObsStopRecord)]
    [InlineData("zacznij transmisję", CommandIntent.ObsStartStream)]
    [InlineData("zatrzymaj transmisję", CommandIntent.ObsStopStream)]
    [InlineData("Jarvis proszę status OBS", CommandIntent.ObsStatus)]
    [InlineData("pauza nagrywania", CommandIntent.ObsPauseRecord)]
    [InlineData("wznów nagrywanie", CommandIntent.ObsResumeRecord)]
    [InlineData("lista wejść OBS", CommandIntent.ObsListInputs)]
    public void Registry_Parses_Obs_Commands(string text, CommandIntent expected)
    {
        var request = new CommandRegistry().Parse(text);
        Assert.NotNull(request);
        Assert.Equal(expected, request.Intent);
    }

    [Fact]
    public void Registry_Parses_Obs_Scene_Argument()
    {
        var request = new CommandRegistry().Parse("przełącz na scenę Kamera");
        Assert.NotNull(request);
        Assert.Equal(CommandIntent.ObsSetScene, request.Intent);
        Assert.Equal("kamera", request.Argument);
    }
    [Theory]
    [InlineData("wycisz mikrofon w OBS", CommandIntent.ObsMuteInput, "mikrofon")]
    [InlineData("odcisz mikrofon w OBS", CommandIntent.ObsUnmuteInput, "mikrofon")]
    [InlineData("pokaż źródło kamera 2", CommandIntent.ObsShowSource, "kamera 2")]
    [InlineData("ukryj źródło kamera 2", CommandIntent.ObsHideSource, "kamera 2")]
    public void Registry_Parses_Obs_Target_Commands(string text, CommandIntent expected, string argument)
    {
        var request = new CommandRegistry().Parse(text);
        Assert.NotNull(request);
        Assert.Equal(expected, request.Intent);
        Assert.Equal(argument, request.Argument);
    }

}
