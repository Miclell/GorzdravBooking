using System.Reflection;
using Application;
using CLI.Menus;
using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu;
using StatefulMenu.Core.Interfaces;

namespace CLI;

public static class Program
{
    private static async Task Main(string[] args)
    {
        var services = new ServiceCollection();

        services.AddInfrastructure();
        services.AddApplication();
        services.AddStatefulMenu();

        var provider = services.BuildServiceProvider();

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();
        }

        var nav = provider.GetRequiredService<INavigationService>();
        var root = provider.GetRequiredService<MainMenuProvider>();
        await nav.RunAsync(root);
    }
}