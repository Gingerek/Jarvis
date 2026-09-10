using System.Text.Json;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Jarvis_UI;

public sealed partial class MainPage : Page
{
    private VoiceHostClient? _voiceHost;

    public MainPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_voiceHost is not null) return;

        _voiceHost = new VoiceHostClient();
        _voiceHost.EventReceived += OnVoiceHostEvent;
        try
        {
            DetailText.Text = "Uruchamiam VoiceHost";
            await _voiceHost.StartAsync();
        }
        catch (Exception ex)
        {
            ApplyState("ERROR", ex.Message);
        }
    }
    private void OnVoiceHostEvent(object? sender, VoiceHostEvent evt)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            if (evt.Type == "state")
            {
                ApplyState(evt.State ?? "UNKNOWN", evt.Text);
                if (evt.Text?.StartsWith("Nasłuch:", StringComparison.OrdinalIgnoreCase) == true)
                    MicText.Text = evt.Text.Replace("Nasłuch:", "MICROPHONE:", StringComparison.OrdinalIgnoreCase).ToUpperInvariant();
                return;
            }

            if (evt.Type == "transcript" && !string.IsNullOrWhiteSpace(evt.Text))
            {
                TranscriptText.Text = evt.Text;
                var ignored = evt.Data is JsonElement data &&
                    data.ValueKind == JsonValueKind.Object &&
                    data.TryGetProperty("disposition", out var disposition) &&
                    disposition.GetString() == "IgnoredWhileSleeping";
                ResponseText.Text = ignored ? "Czekam na słowo „Jarvis”." : "—";
                return;
            }

            if (evt.Type == "command" && !string.IsNullOrWhiteSpace(evt.Text))
            {
                ResponseText.Text = $"Polecenie: {evt.Text}";
                return;
            }

            if (evt.Type == "execution" && !string.IsNullOrWhiteSpace(evt.Text))
            {
                ResponseText.Text = evt.Text;
                return;
            }

            if (evt.Type == "error")
                ApplyState("ERROR", evt.Text);
        });
    }
    private void ApplyState(string state, string? detail)
    {
        var normalized = state.ToUpperInvariant();
        StatusText.Text = normalized switch
        {
            "STARTING" => "URUCHAMIANIE",
            "SLEEPING" => "CZEKAM NA „JARVIS”",
            "LISTENING" when detail?.Contains("Tryb ciągły", StringComparison.OrdinalIgnoreCase) == true => "TRYB CIĄGŁY",
            "LISTENING" => "SŁUCHAM",
            "PROCESSING" => "PRZETWARZAM",
            "SPEAKING" => "MÓWIĘ",
            "STOPPED" => "ZATRZYMANO",
            "ERROR" => "BŁĄD",
            _ => normalized
        };

        DetailText.Text = string.IsNullOrWhiteSpace(detail) ? "—" : detail;
        CoreGlow.Opacity = normalized switch
        {
            "SLEEPING" => 0.35,
            "STARTING" => 0.45,
            "PROCESSING" => 0.70,
            "LISTENING" => 0.95,
            "SPEAKING" => 1.0,
            "ERROR" => 0.20,
            _ => 0.55
        };
        CoreRing.Opacity = normalized is "LISTENING" or "SPEAKING" ? 1.0 : 0.60;
        ConnectionDot.Opacity = normalized == "ERROR" ? 0.25 : 1.0;
    }

    public async Task ShutdownAsync()
    {
        if (_voiceHost is null) return;
        _voiceHost.EventReceived -= OnVoiceHostEvent;
        await _voiceHost.DisposeAsync();
        _voiceHost = null;
    }
}
