using Jarvis.Intents;

namespace Jarvis.Core.Tests;

public sealed class KnownEntityResolverTests
{
    [Theory]
    [InlineData("ligh atrom", "Lightroom")]
    [InlineData("light room", "Lightroom")]
    [InlineData("youtub", "YouTube")]
    [InlineData("davinchi resolve", "DaVinci Resolve")]
    public void Resolves_Common_Asr_Distortions(string spoken, string expected)
    {
        var resolver = new KnownEntityResolver();
        resolver.Add("Lightroom", "light room");
        resolver.Add("YouTube", "you tube");
        resolver.Add("DaVinci Resolve", "davinchi resolve");

        var match = resolver.Resolve(spoken);

        Assert.NotNull(match);
        Assert.Equal(expected, match.CanonicalName);
    }
}
