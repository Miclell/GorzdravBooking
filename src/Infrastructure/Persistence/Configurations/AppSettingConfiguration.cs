using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AppSettingConfiguration : IEntityTypeConfiguration<AppSetting>
{
    public void Configure(EntityTypeBuilder<AppSetting> builder)
    {
        builder.ToTable("AppConfiguration");
        
        // Primary Key
        builder.HasKey(a => a.Key);
        
        // Properties
        builder.Property(a => a.Value)
            .IsRequired()
            .HasMaxLength(255);
    }
}