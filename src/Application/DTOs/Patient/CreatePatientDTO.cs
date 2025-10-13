namespace Application.DTOs.Patient;

using StatefulMenu.Core.Attributes;

public class CreatePatientDto
{
    [InputField("Пользователь Id")]
    public Guid UserId { get; set; }
    
    [InputField("ЛПУ Id")]
    public string LpuId { get; set; } = null!;
    
    [InputField("ЛПУ краткое название")]
    public string LpuShortName { get; set; } = null!;
    
    [InputField("Адрес ЛПУ")]
    public string LpuAddress { get; set; } = null!;
    
    [InputField("Идентификатор пациента в ЛПУ")]
    public string PatientId { get; set; } = null!;
    
    [InputField("Фамилия")]
    public string PatientLastName { get; set; } = null!;
    
    [InputField("Имя")]
    public string PatientFirstName { get; set; } = null!;
    
    [InputField("Отчество")]
    public string PatientMiddleName { get; set; } = null!;
    
    [InputField("Дата рождения")]
    public DateTime PatientBirthdate { get; set; }
    
    [InputField("Email", IsRequired = false)]
    public string? RecipientEmail { get; set; }
    
    [InputField("Телефон", IsRequired = false, Pattern = @"^\+?\d{10,15}$", ErrorMessage = "Телефон в формате +79991234567")]
    public string? MobilePhoneNumber { get; set; }
}