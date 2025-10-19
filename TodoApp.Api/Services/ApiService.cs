using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.NetworkInformation;

namespace TodoApp.Api.Services;

/// <summary>
/// API service for managing the web server
/// </summary>
public class ApiService : IDisposable
{
    private readonly ILogger<ApiService> _logger;
    private readonly object _lock = new object();

    public ApiService(ILogger<ApiService> logger)
    {
        _logger = logger;
    }

    public WebApplication? App { get; private set; }
    public bool IsRunning { get; private set; }
    public int Port { get; private set; }
    public string? ServerUrl { get; private set; }

    /// <summary>
    /// Starts the API server
    /// </summary>
    /// <returns>Port number where the server is running</returns>
    public int Start()
    {
        lock (_lock)
        {
            if (IsRunning)
            {
                _logger.LogInformation("API server is already running on {ServerUrl}", ServerUrl);
                return Port;
            }

            try
            {
                _logger.LogInformation("Starting API server...");

                var builder = WebApplication.CreateBuilder();

                // Configure CORS
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowAll", policy =>
                    {
                        policy
#if DEBUG
                            .WithOrigins("http://localhost:5173")
#endif
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials();
                    });
                });

                builder.Services.AddControllers();
                builder.Services.AddSignalR();

#if DEBUG
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();
#endif

                App = builder.Build();

#if DEBUG
                if (App.Environment.IsDevelopment())
                {
                    App.UseSwagger();
                    App.UseSwaggerUI();
                }
#endif

#if !DEBUG
                // Dynamic port assignment for production
                Port = GetAvailablePort(5000);
                builder.WebHost.UseUrls($"http://localhost:{Port}");
#else
                Port = 5000; // Use fixed port for development
#endif

                App.UseCors("AllowAll");
                App.UseHttpsRedirection();
                App.UseAuthorization();

                App.MapControllers();
                App.UseDefaultFiles(); // Serve index.html by default
                App.UseStaticFiles();  // Serve files from wwwroot

                // Demo API endpoint
                App.MapGet("/api/demo", () => new { message = "It works! 🎉" });

                // Start the server asynchronously
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await App.RunAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error running API server");
                    }
                });

                ServerUrl = $"http://localhost:{Port}";
                IsRunning = true;
                _logger.LogInformation("API server started on port {Port}", Port);
                return Port;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start API server");
                throw;
            }
        }
    }

    /// <summary>
    /// Stops the API server
    /// </summary>
    public void Stop()
    {
        lock (_lock)
        {
            if (!IsRunning)
            {
                _logger.LogInformation("API server is already stopped");
                return;
            }

            try
            {
                _logger.LogInformation("Stopping API server...");
                
                if (App != null)
                {
                    App.StopAsync();
                    App = null;
                }
                
                IsRunning = false;
                ServerUrl = null;
                Port = 0;
                _logger.LogInformation("API server stopped");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping API server");
            }
        }
    }

    public void Dispose()
    {
        Stop();
    }

    private static int GetAvailablePort(int startingPort)
    {
        IPEndPoint[] endPoints;
        List<int> portArray = new List<int>();

        IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();

        // Get active connections
        TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();
        portArray.AddRange(
            from n in connections
            where n.LocalEndPoint.Port >= startingPort
            select n.LocalEndPoint.Port
        );

        // Get active TCP listeners
        endPoints = properties.GetActiveTcpListeners();
        portArray.AddRange(from n in endPoints where n.Port >= startingPort select n.Port);

        // Get active UDP listeners
        endPoints = properties.GetActiveUdpListeners();
        portArray.AddRange(from n in endPoints where n.Port >= startingPort select n.Port);

        portArray.Sort();

        for (int i = startingPort; i < UInt16.MaxValue; i++)
            if (!portArray.Contains(i))
                return i;

        return 0;
    }
}
