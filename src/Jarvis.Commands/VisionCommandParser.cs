namespace Jarvis.Commands;

public sealed class VisionCommandParser
{
    private static readonly HashSet<string> Describe = new(StringComparer.Ordinal)
    {
        "co widzisz", "co teraz widzisz", "co ci pokazuje", "co ci pokazuje kamera",
        "opisz co widzisz", "opisz obraz", "spojrz na kamere", "popatrz na to"
    };

    private static readonly HashSet<string> ReadText = new(StringComparer.Ordinal)
    {
        "przeczytaj to", "przeczytaj co tu jest", "przeczytaj tekst", "co tu jest napisane",
        "przeczytaj ten blad", "jaki jest ten blad", "co to za blad", "odczytaj blad"
    };

    private static readonly HashSet<string> Diagnose = new(StringComparer.Ordinal)
    {
        "co jest nie tak", "co tu jest nie tak", "znajdz problem", "zdiagnozuj to",
        "sprawdz ten blad", "co oznacza ten blad", "pomoz z tym bledem"
    };

    private static readonly HashSet<string> Cameras = new(StringComparer.Ordinal)
    {
        "jakie kamery widzisz", "jaka kamera jest podlaczona", "lista kamer", "pokaz kamery"
    };
    public CommandRequest? Parse(string normalized)
    {
        if (Describe.Contains(normalized)) return new(CommandIntent.VisionDescribe);
        if (ReadText.Contains(normalized)) return new(CommandIntent.VisionReadText);
        if (Diagnose.Contains(normalized)) return new(CommandIntent.VisionDiagnose);
        if (Cameras.Contains(normalized)) return new(CommandIntent.VisionListCameras);
        return null;
    }
}
