using Core.Entities;
using Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AppointmentSearchRequestConfiguration : IEntityTypeConfiguration<AppointmentSearchRequest>
{
    public void Configure(EntityTypeBuilder<AppointmentSearchRequest> builder)
    {
        builder.ToTable("AppointmentSearchRequests");

        // Primary Key
        builder.HasKey(asr => asr.Id);
        
        // TPH Discriminator
        builder
            .HasDiscriminator<string>("RequestType")
            .HasValue<ManualSearchRequest>("Manual")
            .HasValue<ReferralSearchRequest>("Referral");

        // Relationships
        builder.HasOne(asr => asr.PatientProfile)
            .WithMany(pp => pp.AppointmentSearchRequests)
            .HasForeignKey(asr => asr.PatientProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        // Properties
        builder.Property(asr => asr.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(asr => asr.LpuName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(asr => asr.Speciality)
            .IsRequired()
            .HasMaxLength(300);
        
        builder.Property(asr => asr.DoctorMode)
            .IsRequired()
            .HasConversion<string>();
        
        builder.Property(asr => asr.DoctorIds)
            .HasMaxLength(100);

        builder.Property(asr => asr.DoctorNames)
            .HasMaxLength(200);
        
        builder.Property(asr => asr.TimeMode)
            .IsRequired()
            .HasConversion<string>();
        
        builder.Property(asr => asr.TimePreferencesPresetName)
            .HasMaxLength(100);
        
        builder.Property(asr => asr.SearchInterval)
            .IsRequired()
            .HasConversion(
                timeSpan => timeSpan.Ticks,
                ticks => new TimeSpan(ticks)
            );
        
        builder.Property(asr => asr.SpecificStartPoints);
        
        builder.Property(asr => asr.MaxDaysToSearch)
            .IsRequired();

        builder.Property(asr => asr.ViewOnly)
            .IsRequired();
        
        builder.Property(asr => asr.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(asr => asr.CreatedAt)
            .IsRequired();

        builder.Property(asr => asr.LastSearchAttempt)
            .IsRequired(false);

        builder.Property(asr => asr.AttemptCount)
            .IsRequired()
            .HasDefaultValue(0);

        // Indexes
        builder.HasIndex(asr => asr.PatientProfileId)
            .HasDatabaseName("IX_AppointmentSearchRequests_PatientProfileId");

        builder.HasIndex(asr => asr.CreatedAt)
            .HasDatabaseName("IX_AppointmentSearchRequests_CreatedAt");

        builder.HasIndex(asr => asr.Status)
            .HasDatabaseName("IX_AppointmentSearchRequests_Status");

        builder.HasIndex(asr => new { DoctorId = asr.DoctorIds, asr.LpuName })
            .HasDatabaseName("IX_AppointmentSearchRequests_Doctor_Lpu");
        
        builder.HasIndex(asr => new { asr.Status, asr.LastSearchAttempt })
            .HasDatabaseName("IX_AppointmentSearchRequests_Status_LastSearchAttempt");
    }
}