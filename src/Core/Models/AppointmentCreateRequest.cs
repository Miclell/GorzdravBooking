namespace Core.Models;

public class AppointmentCreateRequest
{
    public string? EsiaId { get; set; }
    public string LpuId { get; set; } = null!;
    public string PatientId { get; set; } = null!;
    public string AppointmentId { get; set; } = null!;
    public string? ReferralId { get; set; }
    public string? IpmpiCardId { get; set; }
    public string? RecipientEmail { get; set; }
    public string PatientLastName { get; set; } = null!;
    public string PatientFirstName { get; set; } = null!;
    public string? PatientMiddleName { get; set; }
    public DateTime PatientBirthdate { get; set; }
    public string? Room { get; set; }
    public string Address { get; set; } = null!;
    public DateTime VisitDate { get; set; }
}
