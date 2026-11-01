using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Repositories;
using Persistence.Services;

namespace Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        // Database
        services.AddDbContext<AppDbContext>((provider, options) =>
            options.UseSqlite(DatabaseConfiguration.GetConnectionString(
                provider.GetRequiredService<IConfiguration>())));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IAppointmentSearchRequestRepository, AppointmentSearchRequestRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<ITimePreferencesRepository, TimePreferencesRepository>();
        services.AddScoped<IAppSettingRepository, AppSettingRepository>();

        // Auth
        services.AddScoped<IOwnershipChecker, OwnershipChecker>();

        return services;
    }
}
