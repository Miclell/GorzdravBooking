using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        
        // Primary Key
        builder.HasKey(u => u.Id);
        
        // Properties
        builder.Property(u => u.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        // Relationships
        builder.HasMany(u => u.Profiles)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.TimePreferences)
            .WithOne(tp => tp.User)
            .HasForeignKey(tp => tp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Timestamps
        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .IsRequired(false)
            // .ValueGeneratedOnAddOrUpdate()
            .ValueGeneratedNever();

        // Indexes
        builder.HasIndex(u => u.Username)
            .IsUnique()
            .HasDatabaseName("IX_Users_Username");
            
        builder.HasIndex(u => u.CreatedAt)
            .HasDatabaseName("IX_Users_CreatedAt");

        // Optional: Check constraints
        builder.HasCheckConstraint("CK_Users_Username_Length", 
            @"LENGTH(""username"") >= 3");
    }
}