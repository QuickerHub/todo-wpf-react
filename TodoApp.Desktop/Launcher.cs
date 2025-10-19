using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Windows;
using TodoApp.Api.Services;
using TodoApp.Desktop.Configuration;

namespace TodoApp.Desktop;

public enum LauncherStatus
{
    NotStarted,
    Started,
    Stopped
}

/// <summary>
/// Application launcher for managing services and application lifecycle
/// </summary>
public static class Launcher
{
    private static readonly object _lockObject = new();
    private static LauncherStatus _status = LauncherStatus.NotStarted;
    
    private static readonly IHost _host = Host.CreateDefaultBuilder()
        .ConfigureServices((context, services) =>
        {
            // Configure desktop services
            services.AddDesktopServices();
        })
        .Build();

    /// <summary>
    /// Gets a service from the dependency injection container
    /// </summary>
    /// <typeparam name="T">Service type</typeparam>
    /// <returns>Service instance</returns>
    public static T GetService<T>() where T : class
    {
        return _host.Services.GetService<T>()
            ?? throw new InvalidOperationException($"Service of type {typeof(T).Name} not found.");
    }

    /// <summary>
    /// Starts the application launcher and all services
    /// </summary>
    public static void Start()
    {
        lock (_lockObject)
        {
            if (_status == LauncherStatus.Started)
            {
                return;
            }

            if (_status == LauncherStatus.Stopped)
            {
                return;
            }

            try
            {
                // Initialize API service
                var apiService = GetService<ApiService>();
                apiService.Start();

                // Create and show main window
                var mainWindow = new MainWindow();
                mainWindow.Show();

                _status = LauncherStatus.Started;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Application startup failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }
    }

    /// <summary>
    /// Stops the application launcher and all services
    /// </summary>
    public static void Stop()
    {
        lock (_lockObject)
        {
            if (_status != LauncherStatus.Started) return;

            _status = LauncherStatus.Stopped;
        }
    }

    /// <summary>
    /// Exits the application completely
    /// </summary>
    public static void Exit()
    {
        lock (_lockObject)
        {
            if (_status == LauncherStatus.Stopped) return;

            Stop();
            Application.Current.Shutdown();
        }
    }

    /// <summary>
    /// Gets the current launcher status
    /// </summary>
    public static LauncherStatus Status => _status;

    /// <summary>
    /// Checks if the launcher is running
    /// </summary>
    public static bool IsRunning => _status == LauncherStatus.Started;

    /// <summary>
    /// Disposes of the launcher and all resources
    /// </summary>
    public static void Dispose()
    {
        lock (_lockObject)
        {
            _status = LauncherStatus.Stopped;
        }
    }
}
