namespace Application.DTOs.TimePreferences;

using StatefulMenu.Core.Attributes;

public class DeleteTimePreferencesDto
{
    [InputField("Профиль пациента Id")]
    public Guid PatientProfileId { get; set; }

    [InputField("Название пресета")]
    public string Name { get; set; } = null!;
}