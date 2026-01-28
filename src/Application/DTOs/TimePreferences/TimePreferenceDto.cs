namespace Application.DTOs.TimePreferences;

public record TimePreferenceDto(
    DateOnly? Date,
    DayOfWeek? Day,
    TimeOnly? From,
    TimeOnly? To
);