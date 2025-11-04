using Core.Events.Common;
using Core.Interfaces.Repositories;
using Core.Interfaces.Security;
using Core.Interfaces.Services;
using Infrastructure.Events;
using Infrastructure.Repositories;
using Infrastructure.Security;
using Infrastructure.Services;
using Infrastructure.Stubs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite("Data Source=GorzdravBooking.db"));

        // ApiClient
        //services.AddGorzdravClient();
        services.AddFakeGorzdravClient();

        // Services
        services.AddScoped<IExternalAppointmentService, ExternalAppointmentService>();
        services.AddScoped<IExternalDistrictService, ExternalDistrictService>();
        services.AddScoped<IExternalDoctorService, ExternalDoctorService>();
        services.AddScoped<IExternalLpuService, ExternalLpuService>();
        services.AddScoped<IExternalSpecialtyService, ExternalSpecialtyService>();
        services.AddScoped<IExternalPatientService, ExternalPatientService>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IAppointmentSearchRequestRepository, AppointmentSearchRequestRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<ITimePreferencesRepository, TimePreferencesRepository>();
        services.AddScoped<IAppSettingRepository, AppSettingRepository>();

        // Security
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        // Events
        services.AddSingleton<IEventBus, InMemoryEventBus>();

        return services;
    }
}