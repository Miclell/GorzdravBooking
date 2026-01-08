using Core.Entities;
using Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class TimePreferenceConfiguration : IEntityTypeConfiguration<TimePreference>
{
    public void Configure(EntityTypeBuilder<TimePreference> builder)
    {
        builder.ToTable("TimePreferences");

        // Primary Key
        builder.HasKey(tp => tp.Id);

        // TPH Discriminator
        builder
            .HasDiscriminator(tp => tp.TimeMode)
            .HasValue<WeekDayPreference>(TimeSelectionMode.WeekdayPattern)
            .HasValue<MonthDayPreference>(TimeSelectionMode.SpecificDates)
            .HasValue<AnyTimePreference>(TimeSelectionMode.AnyTime);

        // Relationships
        builder.HasOne(tp => tp.User)
            .WithMany(u => u.TimePreferences)
            .HasForeignKey(tp => tp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Properties
        builder.Property(tp => tp.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(tp => tp.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(tp => tp.TimeMode)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(tp => tp.PreferredTimeFrom)
            .IsRequired(false)
            .HasConversion(
                time => time.HasValue ? time.Value.ToTimeSpan() : (TimeSpan?)null,
                ts => ts.HasValue ? TimeOnly.FromTimeSpan(ts.Value) : null
            );

        builder.Property(tp => tp.PreferredTimeTo)
            .IsRequired(false)
            .HasConversion(
                time => time.HasValue ? time.Value.ToTimeSpan() : (TimeSpan?)null,
                ts => ts.HasValue ? TimeOnly.FromTimeSpan(ts.Value) : null
            );

        builder.Property(tp => tp.ExcludedDates)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(tp => tp.UserId);

        builder.HasIndex(tp => new { tp.UserId, tp.Name })
            .HasDatabaseName("IX_TimePreferences_UserId_Name");

        // Check constraints
        builder.HasCheckConstraint("CK_TimePreferences_TimeRange",
            @"(""PreferredTimeFrom"" IS NULL AND ""PreferredTimeTo"" IS NULL) OR 
              (""PreferredTimeFrom"" IS NOT NULL AND ""PreferredTimeTo"" IS NOT NULL AND 
               ""PreferredTimeFrom"" < ""PreferredTimeTo"")");
    }
}