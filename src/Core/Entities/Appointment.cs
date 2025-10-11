namespace Core.Entities;

public class Appointment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PatientProfileId { get; set; }
    public PatientProfile PatientProfile { get; set; } = null!;

    public string AppointmentId { get; set; } = null!;
    public DateTime VisitStart { get; set; }
    public DateTime VisitEnd { get; set; }
    public string? Address { get; set; }
    public string? Number { get; set; }
    public string? Room { get; set; }
    public string Speciality { get; set; } = null!;
    public string Doctor { get; set; } = null!;
}