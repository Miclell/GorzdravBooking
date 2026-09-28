using System.Text;
using Application;
using Application.Services.Interfaces;
using Application.Workers;
using CLI.Helpers;
using CLI.Menus;
using Core.Events.Common;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using StatefulMenu;
using StatefulMenu.Core.Interfaces;

namespace CLI;

public static class Program
{
    private static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        var migrateOnly = args.Contains("--migrate-only", StringComparer.Ordinal);
        using var host = Host.CreateDefaultBuilder([.. args.Where(arg => arg != "--migrate-only")])
            .ConfigureLogging(logging => logging.ClearProviders())
            .UseSerilog((_, _, logging) => logging
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .WriteTo.File(
                    Path.Combine(AppContext.BaseDirectory, "logs", "cli-.log"),
                    rollingInterval: RollingInterval.Day,
                    fileSizeLimitBytes: 10 * 1024 * 1024,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 7,
                    shared: true))
            .ConfigureServices((_, services) =>
            {
                services.AddInfrastructure();
                services.AddApplication();
                services.AddStatefulMenu();
                services.AddHostedService<AppointmentSchedulerWorker>();
            })
            .Build();

        await host.Services.MigrateDatabaseAsync();
        if (migrateOnly)
            return;

        using (var scope = host.Services.CreateScope())
        {
            var appSettingsService = scope.ServiceProvider.GetRequiredService<IAppSettingsService>();
            await appSettingsService.AppInitializeAsync();
        }

        var eventBus = host.Services.GetRequiredService<IEventBus>();
        HeaderFactorySetup.Initialize(eventBus);

        var nav = host.Services.GetRequiredService<INavigationService>();
        var root = host.Services.GetRequiredService<MainMenuProvider>();
        await host.StartAsync();

        try
        {
            await nav.RunAsync(root);
        }
        finally
        {
            await host.StopAsync();
        }
    }
}
