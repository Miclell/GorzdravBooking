namespace Core.Models;

public class AppointmentCreateRequest
{
    public string? EsiaId { get; set; }
    public required string LpuId { get; set; }
    public required string PatientId { get; set; }
    public required string AppointmentId { get; set; }
    public string? ReferralId { get; set; }
    public string? IpmpiCardId { get; set; }
    public string? RecipientEmail { get; set; }
    public required string PatientLastName { get; set; }
    public required string PatientFirstName { get; set; }
    public string? PatientMiddleName { get; set; }
    public required DateTime PatientBirthdate { get; set; }
    public string? Room { get; set; }
    public required string Address { get; set; }
    public required DateTime VisitDate { get; set; }
}