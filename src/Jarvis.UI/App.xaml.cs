using Microsoft.UI.Xaml;

namespace Jarvis_UI;

public partial class App : Application
{
    private Window? _window;
    private readonly AppBootstrapper _bootstrapper = new();

    public static StartupSnapshot? Startup { get; private set; }

    public App()
    {
        InitializeComponent();
        UnhandledException += OnUnhandledException;
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            Startup = await _bootstrapper.InitializeAsync();
        }
        catch (Exception ex)
        {
            Startup = new StartupSnapshot($"Startup error: {ex.Message}", string.Empty, string.Empty, 0, string.Empty);
        }

        _window = new MainWindow();
        _window.Activate();
    }

    private static void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        e.Handled = false;
    }
}
