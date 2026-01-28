using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class WeekDayPreferenceConfiguration : IEntityTypeConfiguration<WeekDayPreference>
{
    public void Configure(EntityTypeBuilder<WeekDayPreference> builder)
    {
        // Properties
        builder.Property(p => p.Day)
            .IsRequired()
            .HasConversion<int>();
    }
}