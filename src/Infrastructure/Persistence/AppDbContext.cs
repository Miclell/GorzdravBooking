using System.Reflection;
using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<PatientProfile> PatientProfiles { get; set; }
    public DbSet<TimePreference> TimePreferences { get; set; }
    public DbSet<AppointmentSearchRequest> AppointmentSearchRequests { get; set; }
    public DbSet<ManualSearchRequest> ManualSearchRequests { get; set; }
    public DbSet<ReferralSearchRequest> ReferralSearchRequests { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<AppSetting> AppSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
