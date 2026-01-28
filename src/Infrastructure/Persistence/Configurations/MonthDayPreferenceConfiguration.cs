using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class MonthDayPreferenceConfiguration : IEntityTypeConfiguration<MonthDayPreference>
{
    public void Configure(EntityTypeBuilder<MonthDayPreference> builder)
    {
        // Properties
        builder.Property(p => p.Date)
            .IsRequired()
            .HasConversion<string>();
    }
}