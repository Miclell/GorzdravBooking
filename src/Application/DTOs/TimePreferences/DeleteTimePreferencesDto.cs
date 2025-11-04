using StatefulMenu.Core.Attributes;

namespace Application.DTOs.TimePreferences;

public record DeleteTimePreferencesDto(
    [property: InputField("Профиль пациента Id")]
    Guid UserId,
    [property: InputField("Название пресета")]
    string Name
);