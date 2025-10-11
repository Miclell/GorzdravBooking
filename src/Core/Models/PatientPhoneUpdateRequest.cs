namespace Core.Models;

public class PatientPhoneUpdateRequest
{
    public string LpuId { get; set; }  = null!;
    public string MobilePhoneNumber { get; set; } = null!;
    public string PatientId { get; set; } = null!;
}