using StatefulMenu.Core.Attributes;

namespace Application.DTOs.Patient;

[InputModel("профиля пациента")]
public record CreatePatientDto(
    Guid UserId,
    string LpuId,
    string LpuShortName,
    string LpuAddress,
    string PatientId,
    [property: InputField("Фамилия")] string PatientLastName,
    [property: InputField("Имя")] string PatientFirstName,
    [property: InputField("Отчество")] string PatientMiddleName,
    [property: InputField("Дата рождения")]
    DateTime PatientBirthdate,
    [property:
        InputField("Email", IsRequired = false, Pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            ErrorMessage = "Введите корректный email")]
    string? RecipientEmail = null,
    [property:
        InputField("Телефон", IsRequired = false, Pattern = @"\d{10}$",
            ErrorMessage = "Телефон в формате 89991234567 или 9991234567")]
    string? MobilePhoneNumber = null
);