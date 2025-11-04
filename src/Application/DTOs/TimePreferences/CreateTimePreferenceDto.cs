using Core.Attributes;

namespace Application.DTOs.TimePreferences;

public record CreateTimePreferenceDto(
    string Name,
    Guid UserId,
    DayOfWeek? Day,
    [property: InputField("Время с")] TimeOnly? PreferredTimeFrom,
    [property: InputField("Время до")] TimeOnly? PreferredTimeTo,
    bool AnyTime = false
);