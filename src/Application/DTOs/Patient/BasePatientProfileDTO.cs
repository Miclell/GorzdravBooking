namespace Application.DTOs.Patient;

public record BasePatientProfileDto(
    Guid Id,
    string LpuShortName,
    string LpuId,
    string PatientLastName,
    string PatientFirstName,
    string PatientMiddleName,
    DateTime PatientBirthdate,
    string? RecipientEmail,
    string? MobilePhoneNumber
);