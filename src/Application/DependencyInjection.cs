using Application.Abstract;
using Application.Coordinators.Implementation;
using Application.Coordinators.Interfaces;
using Application.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

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
            .AsSelf()
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.AddScoped<IAppointmentCoordinator, AppointmentCoordinator>();

        // services.AddScoped<CheckAppointmentSearchRequestsUseCase>();
        // services.AddScoped<CreateDefaultUserUseCase>();

        return services;
    }
}