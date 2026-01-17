namespace Core.Exceptions;

public class ReferralNotFoundException(int referralNumber)
    : Exception($"Referral {referralNumber} not found")
{
    public int ReferralNumber { get; } = referralNumber;
}