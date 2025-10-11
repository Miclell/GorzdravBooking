using Core.Interfaces.Repositories;
using Core.Interfaces.Security;
using Core.Interfaces.Services;
using Infrastructure.Http;
using Infrastructure.Repositories;
using Infrastructure.Security;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite("Data Source=GorzdravBooking.db"));
        
        // ApiClient
        services.AddGorzdravClient(); 

        // Services
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IDistrictService, DistrictService>();
        services.AddScoped<IDoctorService, DoctorService>();
        services.AddScoped<ILpuService, LpuService>();
        services.AddScoped<ISpecialtyService, SpecialtyService>();
        services.AddScoped<IPatientService, PatientService>();
        
        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IAppointmentSearchRequestRepository, AppointmentSearchRequestRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<ITimePreferencesRepository, TimePreferencesRepository>();
        services.AddScoped<IAppSettingRepository, AppSettingRepository>();

        // Security
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        return services;
    }
}