using Application.DTOs.Patient;
using Core.Models.Referral;

namespace Application.DTOs.UseCases;

public record ReferralValidationResult(
    BasePatientProfileDto PatientProfile,
    List<ReferralSpeciality> Specialities
    );