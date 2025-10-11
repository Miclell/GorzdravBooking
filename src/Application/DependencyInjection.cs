using System.Reflection;
using Application.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace App;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblyOf<IAppService>()
            .AddClasses(classes => classes.AssignableTo<IAppService>())
            .AsImplementedInterfaces()
            .WithScopedLifetime()
            
            .FromAssemblyOf<IAppUseCase>()
            .AddClasses(classes => classes.AssignableTo<IAppUseCase>())
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}