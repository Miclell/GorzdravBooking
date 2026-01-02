using System.Text;
using Application;
using Application.Services.Interfaces;
using Application.Workers;
using CLI.Helpers;
using CLI.Menus;
using Core.Events.Common;
using Infrastructure;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StatefulMenu;
using StatefulMenu.Core.Interfaces;

namespace CLI;

public static class Program
{
    private static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                // AddDebugLogging(logging);
                AddProductionLogging(logging);
            })
            .ConfigureServices((_, services) =>
            {
                services.AddInfrastructure();
                services.AddApplication();
                services.AddStatefulMenu();
                services.AddHostedService<AppointmentSchedulerWorker>();
            })
            .Build();

        using (var scope = host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();

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

    private static void AddDebugLogging(ILoggingBuilder logging)
    {
        logging.AddConsole(_ => { })
            .AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning)
            .AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning)
            .AddFilter("System.Net.Http", LogLevel.Warning)
            .AddFilter("Default", LogLevel.Information)
            .AddFilter("Infrastructure", LogLevel.Debug)
            .AddFilter("Core", LogLevel.Debug)
            .AddFilter("Application", LogLevel.Debug)
            .AddFilter("CLI", LogLevel.Debug);
    }

    private static void AddProductionLogging(ILoggingBuilder logging)
    {
        logging.AddConsole(_ => { });

        logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.None);
        logging.AddFilter("Microsoft.EntityFrameworkCore.Database", LogLevel.None);
        logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
        logging.AddFilter("Microsoft.EntityFrameworkCore.Infrastructure", LogLevel.None);
        logging.AddFilter("Microsoft.EntityFrameworkCore.Query", LogLevel.None);
        logging.AddFilter("Microsoft.EntityFrameworkCore.Update", LogLevel.None);

        logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
        logging.AddFilter("Microsoft.AspNetCore.Hosting", LogLevel.None);
        logging.AddFilter("Microsoft.AspNetCore.Routing", LogLevel.None);
        
        logging.AddFilter("System.Net.Http.HttpClient", LogLevel.None);
        logging.AddFilter("System.Net.Http.HttpClient.*", LogLevel.None);

        logging.AddFilter("Default", LogLevel.Information);
        logging.AddFilter("Infrastructure", LogLevel.Information);
        logging.AddFilter("Core", LogLevel.Information);
        logging.AddFilter("Application", LogLevel.Information);
        logging.AddFilter("CLI", LogLevel.Information);
    }
}