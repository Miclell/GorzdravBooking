namespace Core.Entities;

public class ReferralSearchRequest : AppointmentSearchRequest
{
    public required string ReferralNumber { get; set; }
}