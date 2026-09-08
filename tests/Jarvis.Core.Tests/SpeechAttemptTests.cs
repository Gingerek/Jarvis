using Jarvis.Speech;

namespace Jarvis.Core.Tests;

public sealed class SpeechAttemptTests
{
    [Fact]
    public async Task Provider_failure_does_not_escape_or_retry()
    {
        var calls = 0;
        Assert.False(await SpeechAttempt.TryAsync(_ =>
        {
            calls++;
            throw new HttpRequestException("private provider details");
        }));
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task Success_is_reported()
    {
        Assert.True(await SpeechAttempt.TryAsync(_ => Task.CompletedTask));
    }

    [Fact]
    public async Task Shutdown_is_not_swallowed()
    {
        using var stop = new CancellationTokenSource();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => SpeechAttempt.TryAsync(token =>
        {
            stop.Cancel();
            token.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }, stop.Token));
    }

    [Fact]
    public async Task Provider_cancellation_allows_following_reply()
    {
        Assert.False(await SpeechAttempt.TryAsync(_ => Task.FromCanceled(new CancellationToken(true))));
        Assert.True(await SpeechAttempt.TryAsync(_ => Task.CompletedTask));
    }
}
