using System.Windows;
using Microsoft.Extensions.Logging;
using TodoApp.Api.Services;

namespace TodoApp.Desktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
        Closed += MainWindow_Closed;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var apiService = Launcher.GetService<ApiService>();
            var logger = Launcher.GetService<ILogger<MainWindow>>();

            // 检查 API 服务器是否已经在运行
            if (!apiService.IsRunning)
            {
                logger.LogInformation("Starting API server...");
                var port = apiService.Start();
                logger.LogInformation("API server started on port {Port}", port);
            }
            else
            {
                logger.LogInformation("API server is already running on {ServerUrl}", apiService.ServerUrl);
            }

            await webView.EnsureCoreWebView2Async();

#if DEBUG
            webView.CoreWebView2.Navigate("http://localhost:5173");
            logger.LogInformation("Navigating to development server (localhost:5173)");
#else
            var serverUrl = apiService.ServerUrl;
            if (!string.IsNullOrEmpty(serverUrl))
            {
                webView.CoreWebView2.Navigate(serverUrl);
                logger.LogInformation("Navigating to production server: {ServerUrl}", serverUrl);
            }
            else
            {
                logger.LogError("Server URL is null or empty");
            }
#endif
        }
        catch (Exception ex)
        {
            var logger = Launcher.GetService<ILogger<MainWindow>>();
            logger.LogError(ex, "Error loading main window");
        }
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        try
        {
            var logger = Launcher.GetService<ILogger<MainWindow>>();
            logger.LogInformation("Main window closed, shutting down application...");

            // 退出整个应用程序
            Launcher.Exit();
        }
        catch (Exception ex)
        {
            var logger = Launcher.GetService<ILogger<MainWindow>>();
            logger.LogError(ex, "Error during application shutdown");
        }
    }
}