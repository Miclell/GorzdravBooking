using System.Diagnostics;
using System.Reflection;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<PatientProfile> PatientProfiles { get; set; }
    public DbSet<TimePreferences> TimePreferences { get; set; }
    public DbSet<AppointmentSearchRequest> AppointmentSearchRequests { get; set; }
    public DbSet<ManualSearchRequest> ManualSearchRequests { get; set; }
    public DbSet<ReferralSearchRequest> ReferralSearchRequests { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<AppSetting> AppSettings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Включаем логирование SQL запросов в консоль
        optionsBuilder
            .EnableSensitiveDataLogging() // Показывает значения параметров
            .LogTo(Console.WriteLine, LogLevel.Trace) // Логи в консоль
            .LogTo(message => Debug.WriteLine(message), LogLevel.Trace); // Логи в Debug output


        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}