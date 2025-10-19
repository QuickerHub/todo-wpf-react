using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TodoApp.Api.Services;

namespace TodoApp.Desktop.Configuration;

/// <summary>
/// Service configuration for dependency injection
/// </summary>
public static class ServiceConfiguration
{
    /// <summary>
    /// Configures services for the desktop application
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Configured service collection</returns>
    public static IServiceCollection AddDesktopServices(this IServiceCollection services)
    {
        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

            // Add API service
            services.AddSingleton<ApiService>();

        // Add other services here as needed
        // services.AddSingleton<ISomeOtherService, SomeOtherService>();

        return services;
    }
}
