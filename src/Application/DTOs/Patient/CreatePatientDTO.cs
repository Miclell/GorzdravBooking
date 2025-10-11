namespace Application.DTOs.Patient;

public record CreatePatientDto(
    Guid UserId,
    string LpuId,
    string LpuShortName,
    string LpuAddress,
    string PatientId,
    string PatientLastName,
    string PatientFirstName,
    string PatientMiddleName,
    DateTime PatientBirthdate,
    string? RecipientEmail = null,
    string? MobilePhoneNumber = null
    );