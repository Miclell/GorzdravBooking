namespace Application.DTOs.TimePreferences;

public record TimePreferenceDto(
    DayOfWeek? Day,
    TimeOnly? From,
    TimeOnly? To
);