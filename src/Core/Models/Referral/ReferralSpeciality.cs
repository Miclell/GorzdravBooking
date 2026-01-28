namespace Core.Models.Referral;

public class ReferralSpeciality
{
    public string Id { get; set; } = null!;
    public string? FerId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public List<ReferralDoctor> Doctors { get; set; } = [];
}