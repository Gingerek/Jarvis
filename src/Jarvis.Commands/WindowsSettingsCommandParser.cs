namespace Jarvis.Commands;

public sealed class WindowsSettingsCommandParser
{
    private static readonly Dictionary<string, string> Pages = new(StringComparer.Ordinal)
    {
        ["ustawienia"] = "ms-settings:",
        ["ekran"] = "ms-settings:display",
        ["ekranu"] = "ms-settings:display",
        ["wyswietlanie"] = "ms-settings:display",
        ["dzwiek"] = "ms-settings:sound",
        ["dzwieku"] = "ms-settings:sound",
        ["audio"] = "ms-settings:sound",
        ["mikser glosnosci"] = "ms-settings:apps-volume",
        ["bluetooth"] = "ms-settings:bluetooth",
        ["wifi"] = "ms-settings:network-wifi",
        ["wi fi"] = "ms-settings:network-wifi",
        ["siec"] = "ms-settings:network-status",
        ["sieci"] = "ms-settings:network-status",
        ["internet"] = "ms-settings:network-status",
        ["internetu"] = "ms-settings:network-status",
        ["aktualizacje"] = "ms-settings:windowsupdate",
        ["aktualizacji"] = "ms-settings:windowsupdate",
        ["windows update"] = "ms-settings:windowsupdate",
        ["aplikacje"] = "ms-settings:appsfeatures",
        ["aplikacji"] = "ms-settings:appsfeatures",
        ["pamiec"] = "ms-settings:storagesense",
        ["pamieci"] = "ms-settings:storagesense",
        ["miejsce na dysku"] = "ms-settings:storagesense",
        ["zasilanie"] = "ms-settings:powersleep",
        ["zasilania"] = "ms-settings:powersleep",
        ["personalizacja"] = "ms-settings:personalization",
        ["personalizacji"] = "ms-settings:personalization",
        ["prywatnosc"] = "ms-settings:privacy",
        ["prywatnosci"] = "ms-settings:privacy",
        ["mikrofon"] = "ms-settings:privacy-microphone",
        ["mikrofonu"] = "ms-settings:privacy-microphone",
        ["kamera"] = "ms-settings:privacy-webcam",
        ["kamery"] = "ms-settings:privacy-webcam",
        ["powiadomienia"] = "ms-settings:notifications",
        ["powiadomien"] = "ms-settings:notifications",
        ["schowek"] = "ms-settings:clipboard",
        ["schowka"] = "ms-settings:clipboard",
        ["informacje o systemie"] = "ms-settings:about"
    };
    public CommandRequest? Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var normalized = CommandRegistry.Normalize(text);

        if (normalized == "ustawienia")
            return new(CommandIntent.OpenSettings, Pages["ustawienia"]);

        string[] prefixes =
            ["otworz ustawienia ", "pokaz ustawienia ", "ustawienia ", "przejdz do ustawien "];

        foreach (var prefix in prefixes)
        {
            if (!normalized.StartsWith(prefix, StringComparison.Ordinal)) continue;
            var page = normalized[prefix.Length..].Trim();
            if (Pages.TryGetValue(page, out var uri))
                return new(CommandIntent.OpenSettings, uri);
        }

        return null;
    }
}
