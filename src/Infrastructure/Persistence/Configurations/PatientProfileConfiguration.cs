using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PatientProfileConfiguration : IEntityTypeConfiguration<PatientProfile>
{
    public void Configure(EntityTypeBuilder<PatientProfile> builder)
    {
        builder.ToTable("PatientProfiles");
        
        // Primary Key
        builder.HasKey(p => p.Id);
        
        // Properties
        builder.Property(p => p.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(p => p.UserId)
            .IsRequired();
        
        builder.Property(p => p.LpuId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.LpuAddress)
            .IsRequired();

        builder.Property(p => p.PatientId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.RecipientEmail)
            .HasMaxLength(255);

        builder.Property(p => p.MobilePhoneNumber)
            .HasMaxLength(20);

        builder.Property(p => p.PatientLastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.PatientFirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.PatientMiddleName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.PatientBirthdate)
            .IsRequired();

        // Relationships
        builder.HasOne(p => p.User)
            .WithMany(u => u.Profiles)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.TimePreferences)
            .WithOne(tp => tp.PatientProfile)
            .HasForeignKey(tp => tp.PatientProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.AppointmentSearchRequests)
            .WithOne(asr => asr.PatientProfile)
            .HasForeignKey(asr => asr.PatientProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Appointments)
            .WithOne(a => a.PatientProfile)
            .HasForeignKey(a => a.PatientProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(p => p.UserId)
            .HasDatabaseName("IX_PatientProfiles_UserId");

        builder.HasIndex(p => p.PatientId)
            .HasDatabaseName("IX_PatientProfiles_PatientId");

        builder.HasIndex(p => new { p.UserId, p.PatientId })
            .IsUnique()
            .HasDatabaseName("IX_PatientProfiles_UserId_PatientId_Unique");

        // Check constraints
        builder.HasCheckConstraint("CK_PatientProfiles_Email_Format", 
            @$"""RecipientEmail"" IS NULL OR ""RecipientEmail"" LIKE '%@%.%'");

        builder.HasCheckConstraint("CK_PatientProfiles_Birthdate", 
            @$"""PatientBirthdate"" <= CURRENT_DATE");
    }
}