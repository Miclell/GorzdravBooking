namespace Application.DTOs.Patient;

using StatefulMenu.Core.Attributes;

public record CreatePatientDto(
    Guid UserId,
    string LpuId,
    string LpuShortName,
    string LpuAddress,
    string PatientId,
    
    [property: InputField("Фамилия")]
    string PatientLastName,
    
    [property: InputField("Имя")]
    string PatientFirstName,
    
    [property: InputField("Отчество")]
    string PatientMiddleName,
    
    [property: InputField("Дата рождения")]
    DateTime PatientBirthdate,
    
    [property: InputField("Email", IsRequired = false)]
    string? RecipientEmail = null,
    
    [property: InputField("Телефон", IsRequired = false, Pattern = @"^\+?\d{10,15}$", ErrorMessage = "Телефон в формате +79991234567")]
    string? MobilePhoneNumber = null
);