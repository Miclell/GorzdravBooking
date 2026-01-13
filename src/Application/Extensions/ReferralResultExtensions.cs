using Application.DTOs.Patient;
using Core.Models.Referral;

namespace Application.Extensions;

public static class ReferralResultExtensions
{
    public static CreatePatientDto ToCreatePatientDto(this ReferralResult referralResult, Guid userId)
    {
        return new CreatePatientDto(
            userId,
            referralResult.LpuId,
            referralResult.LpuShortName,
            referralResult.LpuAddress,
            referralResult.PatId,
            ToTitleCase(referralResult.LastName),
            ToTitleCase(referralResult.FirstName),
            ToTitleCase(referralResult.MiddleName),
            referralResult.BirthDate,
            null,
            referralResult.MobilePhoneNumber
        );
    }

    private static string ToTitleCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
    
        if (input.Length == 1)
            return input.ToUpper();
    
        return char.ToUpper(input[0]) + input[1..].ToLower();
    }
}