namespace Core.Models.Referral;

public class ReferralDoctor
{
    public string Id { get; set; } = null!;
    public string? Description { get; set; }
    public string Name { get; set; } = null!;
    public List<Appointment> Appointments { get; set; } = [];
}