namespace Core.Entities;

public class ReferralSearchRequest : AppointmentSearchRequest
{
    public int ReferralNumber { get; set; }
}