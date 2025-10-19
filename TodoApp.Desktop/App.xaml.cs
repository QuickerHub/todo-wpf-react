using System.Windows;

namespace TodoApp.Desktop;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Start the application using the launcher
        Launcher.Start();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Stop the launcher and dispose of resources
        Launcher.Dispose();
        base.OnExit(e);
    }
}

