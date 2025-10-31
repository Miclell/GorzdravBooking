using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class TimePreferencesConfiguration : IEntityTypeConfiguration<TimePreferences>
{
    public void Configure(EntityTypeBuilder<TimePreferences> builder)
    {
        builder.ToTable("TimePreferences");
        
        // Primary Key
        builder.HasKey(tp => tp.Id);
        
        // Properties
        builder.Property(tp => tp.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(tp => tp.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(tp => tp.Day)
            .IsRequired(false)
            .HasConversion<int?>();

        builder.Property(tp => tp.PreferredTimeFrom)
            .IsRequired(false)
            .HasConversion(
                time => time.HasValue ? time.Value.ToTimeSpan() : (TimeSpan?)null,
                ts => ts.HasValue ? TimeOnly.FromTimeSpan(ts.Value) : (TimeOnly?)null
            );

        builder.Property(tp => tp.PreferredTimeTo)
            .IsRequired(false)
            .HasConversion(
                time => time.HasValue ? time.Value.ToTimeSpan() : (TimeSpan?)null,
                ts => ts.HasValue ? TimeOnly.FromTimeSpan(ts.Value) : (TimeOnly?)null
            );

        builder.Property(tp => tp.AnyTime)
            .IsRequired()
            .HasDefaultValue(false);

        // Relationships
        builder.HasOne(tp => tp.User)
            .WithMany(u => u.TimePreferences)
            .HasForeignKey(tp => tp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(tp => tp.UserId);
        
        builder.HasIndex(tp => new { tp.UserId, tp.Name })
            .HasDatabaseName("IX_TimePreferences_UserId_Name");

        builder.HasIndex(tp => new { tp.UserId, tp.Day })
            .HasDatabaseName("IX_TimePreferences_UserId_Day");

        // Check constraints
        builder.HasCheckConstraint("CK_TimePreferences_TimeRange", 
            @$"(""AnyTime"" = true OR (""PreferredTimeFrom"" IS NOT NULL AND 
            ""PreferredTimeTo"" IS NOT NULL AND 
            ""PreferredTimeFrom"" < ""PreferredTimeTo""))");

        builder.HasCheckConstraint("CK_TimePreferences_Day_Required",
            @$"(""AnyTime"" = true OR ""Day"" IS NOT NULL)");
    }
}