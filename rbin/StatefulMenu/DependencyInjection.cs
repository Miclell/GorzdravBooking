using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Infrastructure.Components;
using StatefulMenu.Infrastructure.Localization;
using StatefulMenu.Infrastructure.Services;

namespace StatefulMenu;

public static class DependencyInjection
{
    public static IServiceCollection AddStatefulMenu(this IServiceCollection services)
    {
        services.AddSingleton<IConsoleLocalizer>(sp =>
        {
            var lang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            return lang == "ru" ? new RuConsoleLocalizer() : new EnConsoleLocalizer();
        });
        services.AddSingleton<NavigationStack>();
        services.AddSingleton<MenuRenderer>();
        services.AddSingleton<IDataService, DataService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IConsoleInputService, ConsoleInputService>();

        var callingAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
    
        var assemblies = new[] 
        { 
            typeof(IMenuCommand).Assembly, // StatefulMenu
            typeof(IMenuProvider).Assembly, // StatefulMenu  
            callingAssembly // CLI
        };

        services.Scan(scan => scan
            .FromAssemblies(assemblies)
            .AddClasses(classes => classes.AssignableTo<IMenuCommand>())
            .AsSelf()
            .AsImplementedInterfaces() 
            .WithTransientLifetime()
            .AddClasses(classes => classes.AssignableTo<IMenuProvider>())
            .AsSelf()
            .AsImplementedInterfaces()
            .WithTransientLifetime());

        return services;
    }
}