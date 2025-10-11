using Core.Entities;
using Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Persistence.Configurations;

public class AppointmentSearchRequestConfiguration : IEntityTypeConfiguration<AppointmentSearchRequest>
{
    public void Configure(EntityTypeBuilder<AppointmentSearchRequest> builder)
    {
        builder.ToTable("AppointmentSearchRequests");
        
        // Primary Key
        builder.HasKey(asr => asr.Id);
        
        // Relationships
        builder.HasOne(asr => asr.PatientProfile)
            .WithMany(pp => pp.AppointmentSearchRequests)
            .HasForeignKey(asr => asr.PatientProfileId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Properties
        builder.Property(asr => asr.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(asr => asr.LpuName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(asr => asr.DoctorId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(asr => asr.DoctorName)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(asr => asr.SearchInterval)
            .IsRequired()
            .HasConversion(
                timeSpan => timeSpan.Ticks,
                ticks => new TimeSpan(ticks)
            )
            .HasDefaultValue(TimeSpan.FromHours(1));

        builder.Property(asr => asr.SpecificStartPoints);

        builder.Property(asr => asr.TimePreferencesPresetName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(asr => asr.ViewOnly)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(asr => asr.CreatedAt)
            .IsRequired();

        builder.Property(asr => asr.LastSearchAttempt)
            .IsRequired(false);

        builder.Property(asr => asr.AttemptCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(asr => asr.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(SearchRequestStatus.Pending);

        builder.Property(asr => asr.MaxDaysToSearch)
            .IsRequired()
            .HasDefaultValue(30);

        // Indexes
        builder.HasIndex(asr => asr.PatientProfileId)
            .HasDatabaseName("IX_AppointmentSearchRequests_PatientProfileId");

        builder.HasIndex(asr => asr.CreatedAt)
            .HasDatabaseName("IX_AppointmentSearchRequests_CreatedAt");

        builder.HasIndex(asr => asr.Status)
            .HasDatabaseName("IX_AppointmentSearchRequests_Status");

        builder.HasIndex(asr => new { asr.DoctorId, asr.LpuName })
            .HasDatabaseName("IX_AppointmentSearchRequests_Doctor_Lpu");

        // Check constraints
        builder.HasCheckConstraint("CK_AppointmentSearchRequests_AttemptCount", 
            @$"""AttemptCount"" >= 0");
    }
}