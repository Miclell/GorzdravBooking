namespace Application.DTOs.TimePreferences;

using StatefulMenu.Core.Attributes;

public class DeleteTimePreferencesDto
{
    public DeleteTimePreferencesDto(Guid id, string name) // TODO сделать record
    {
        UserId = id;
        Name = name;
    }

    [InputField("Профиль пациента Id")]
    public Guid UserId { get; set; }

    [InputField("Название пресета")]
    public string Name { get; set; } = null!;
}