using Application;
using Application.Services.Interfaces;
using Application.Workers;
using CLI.Menus;
using Infrastructure;
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
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .ConfigureServices((hostContext, services) =>
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