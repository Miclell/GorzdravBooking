using Application;
using Application.Services;
using Application.UseCases.User;
using Infrastructure;
using ConsoleUI.Components;
using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.Commands;
using ConsoleUI.Menus.Providers;
using ConsoleUI.Services;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ConsoleUI;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        var services = new ServiceCollection();
        
        services.AddInfrastructure();
        services.AddApplication();
        services.AddMenu();
        
        var serviceProvider = services.BuildServiceProvider();
        
        using (var scope = serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.MigrateAsync();

            var appSettingsService = scope.ServiceProvider.GetRequiredService<AppSettingsService>();
            await appSettingsService.AppInitializeAsync();
        }
        
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, serviceCollection) =>
            {
                serviceCollection.AddInfrastructure();
                serviceCollection.AddApplication();
                serviceCollection.AddHostedService<AppointmentSchedulerService>();
            })
            .Build();
        
        //await host.StartAsync();

        
        var navigationService = serviceProvider.GetRequiredService<NavigationService>();
        
        await navigationService.StartAsync();
    }
}