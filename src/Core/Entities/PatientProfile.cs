namespace Core.Entities;

public class PatientProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<TimePreferences> TimePreferences { get; set; } = [];
    public ICollection<AppointmentSearchRequest> AppointmentSearchRequests { get; set; } = [];
    public ICollection<Appointment> Appointments { get; set; } = [];

    public string LpuId { get; set; } = null!;
    public string PatientId { get; set; } = null!;
    public string LpuShortName { get; set; } = null!;
    public string LpuAddress { get; set; } = null!;
    public string? RecipientEmail { get; set; }
    public string? MobilePhoneNumber { get; set; }
    public string PatientLastName { get; set; } = null!;
    public string PatientFirstName { get; set; } = null!;
    public string PatientMiddleName { get; set; } = null!;
    public DateTime PatientBirthdate { get; set; }
}