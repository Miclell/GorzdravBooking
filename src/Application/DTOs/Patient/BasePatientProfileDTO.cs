namespace Application.DTOs.Patient;

public record BasePatientProfileDto(
    Guid Id,
    string LpuShortName,
    string PatientLastName,
    string PatientFirstName,
    string PatientMiddleName,
    DateTime PatientBirthdate,
    string? RecipientEmail,
    string? MobilePhoneNumber
    );