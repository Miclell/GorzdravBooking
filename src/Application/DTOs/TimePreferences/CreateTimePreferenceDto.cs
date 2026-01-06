using StatefulMenu.Core.Attributes;

namespace Application.DTOs.TimePreferences;

[InputModel("временных предпочтений")]
public record CreateTimePreferenceDto(
    string Name,
    Guid UserId,
    DateOnly? Date,
    DayOfWeek? Day,
    [property: InputField("Время с")] 
    TimeOnly? PreferredTimeFrom,
    [property: InputField("Время до")] 
    TimeOnly? PreferredTimeTo,
    List<DateOnly>? ExcludedDates,
    int MaxDaysAhead,
    int MinHoursFromNow
);