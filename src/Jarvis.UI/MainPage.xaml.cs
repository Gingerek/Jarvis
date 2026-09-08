using Microsoft.UI.Xaml.Controls;

namespace Jarvis_UI;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        InitializeComponent();
        var startup = App.Startup;

        StatusText.Text = startup?.Status ?? "Starting";
        PluginText.Text = $"Plugins: {startup?.PluginCount ?? 0}";
        ConfigText.Text = $"Config: {startup?.ConfigPath ?? "not initialized"}";
        DataText.Text = $"Data: {startup?.DatabasePath ?? "not initialized"}";
    }
}
