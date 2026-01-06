namespace Core.Models.Referral;

public class ReferralResult
{
    public string LpuId { get; set; }
    public string LpuShortName { get; set; }
    public string LpuFullName { get; set; }
    public string LpuAddress { get; set; }
    public string LpuPhone { get; set; }
    
    public string PatId { get; set; }
    public string LastName { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    
    public string? PolisN { get; set; }
    public string? PolisS { get; set; }
    public DateTime BirthDate { get; set; }
    
    public string? HomePhoneNumber { get; set; }
    public string? MobilePhoneNumber { get; set; }
    public string? Email { get; set; }
    
    public List<ReferralSpeciality> Specialities { get; set; } = [];
}