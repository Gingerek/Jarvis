namespace Jarvis.Speech;

/// <summary>Keep optional speech failures separate from command execution.</summary>
public static class SpeechAttempt
{
    public static async Task<bool> TryAsync(
        Func<CancellationToken, Task> speak,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(speak);
        cancellationToken.ThrowIfCancellationRequested();
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(15));
        try
        {
            await speak(deadline.Token);
            cancellationToken.ThrowIfCancellationRequested();
            return !deadline.IsCancellationRequested;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return false;
        }
    }
}
