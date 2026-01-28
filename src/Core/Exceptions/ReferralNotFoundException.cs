namespace Core.Exceptions;

public class ReferralNotFoundException(string referralNumber)
    : Exception($"Referral {referralNumber} not found")
{
    public string ReferralNumber { get; } = referralNumber;
}