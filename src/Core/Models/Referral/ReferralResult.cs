namespace Core.Models.Referral;

public class ReferralResult
{
    public string LpuId { get; set; } = null!;
    public string LpuShortName { get; set; } = null!;
    public string LpuFullName { get; set; } = null!;
    public string LpuAddress { get; set; } = null!;
    public string LpuPhone { get; set; } = null!;

    public string PatId { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string MiddleName { get; set; } = null!;

    public string? PolisN { get; set; }
    public string? PolisS { get; set; }
    public DateTime BirthDate { get; set; }

    public string? HomePhoneNumber { get; set; }
    public string? MobilePhoneNumber { get; set; }
    public string? Email { get; set; }

    public List<ReferralSpeciality> Specialities { get; set; } = [];
}