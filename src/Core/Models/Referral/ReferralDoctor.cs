namespace Core.Models.Referral;

public class ReferralDoctor
{
    public string Id { get; set; }
    public string? Description { get; set; }
    public string Name { get; set; }
    public List<Appointment> Appointments { get; set; } = [];
}