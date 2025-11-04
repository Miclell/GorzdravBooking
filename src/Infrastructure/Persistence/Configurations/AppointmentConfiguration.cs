using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");

        // Primary Key
        builder.HasKey(a => a.Id);

        // Relationships
        builder.HasOne(a => a.PatientProfile)
            .WithMany(pp => pp.Appointments)
            .HasForeignKey(a => a.PatientProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // Properties
        builder.Property(a => a.AppointmentId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.VisitStart)
            .IsRequired();

        builder.Property(a => a.VisitEnd)
            .IsRequired();

        builder.Property(a => a.Address)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(a => a.Number)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(a => a.Room)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(a => a.Doctor)
            .IsRequired();

        builder.Property(a => a.Speciality)
            .IsRequired();

        // Indexes
        builder.HasIndex(a => a.PatientProfileId)
            .HasDatabaseName("IX_Appointments_PatientProfileId");

        builder.HasIndex(a => a.AppointmentId)
            .IsUnique()
            .HasDatabaseName("IX_Appointments_AppointmentId_Unique");

        builder.HasIndex(a => a.VisitStart)
            .HasDatabaseName("IX_Appointments_VisitStart");

        builder.HasIndex(a => a.VisitEnd)
            .HasDatabaseName("IX_Appointments_VisitEnd");

        builder.HasIndex(a => new { a.PatientProfileId, a.VisitStart })
            .HasDatabaseName("IX_Appointments_PatientId_VisitStart");

        // Comment
        builder.HasComment("Записи на прием к врачу");
    }
}