using Jarvis.Commands;

namespace Jarvis.Core.Tests;

public sealed class BrowserCommandParserTests
{
    [Theory]
    [InlineData("nowa karta", CommandIntent.BrowserNewTab)]
    [InlineData("zamknij kartę", CommandIntent.BrowserCloseTab)]
    [InlineData("następna karta", CommandIntent.BrowserNextTab)]
    [InlineData("poprzednia karta", CommandIntent.BrowserPreviousTab)]
    [InlineData("odśwież stronę", CommandIntent.BrowserReload)]
    [InlineData("przewiń w dół", CommandIntent.BrowserScrollDown)]
    [InlineData("na górę strony", CommandIntent.BrowserScrollTop)]
    [InlineData("wycisz kartę", CommandIntent.BrowserMuteTab)]
    [InlineData("jaka jest aktywna karta", CommandIntent.BrowserContext)]
    public void Registry_Parses_Browser_Commands(string text, CommandIntent expected)
    {
        var request = new CommandRegistry().Parse(text);
        Assert.NotNull(request);
        Assert.Equal(expected, request.Intent);
    }
}