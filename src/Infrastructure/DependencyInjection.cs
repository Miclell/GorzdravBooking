using Core.Events.Common;
using Core.Interfaces.Auth;
using Core.Interfaces.Security;
using Core.Interfaces.Services;
using Infrastructure.Auth;
using Infrastructure.Events;
using Infrastructure.Security;
using Infrastructure.Services;
using Infrastructure.Stubs;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // ApiClient
        // services.AddGorzdravClient(); // Local, unpublished real client.
        services.AddFakeGorzdravClient();

        // Services
        services.AddScoped<IExternalAppointmentService, ExternalAppointmentService>();
        services.AddScoped<IExternalDistrictService, ExternalDistrictService>();
        services.AddScoped<IExternalDoctorService, ExternalDoctorService>();
        services.AddScoped<IExternalLpuService, ExternalLpuService>();
        services.AddScoped<IExternalSpecialtyService, ExternalSpecialtyService>();
        services.AddScoped<IExternalPatientService, ExternalPatientService>();

        // Security
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        // Events
        services.AddSingleton<IEventBus, InMemoryEventBus>();

        // Auth
        services.AddScoped<IAuthorizationProvider, AuthorizationProvider>();

        return services;
    }
}
