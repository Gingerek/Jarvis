namespace Jarvis.VoiceHost;

public sealed record CommandExecutionOutcome(
    string Reply,
    string Status,
    string? Target = null);
