namespace Application.DTOs.TimePreferences;

public record DeleteTimePreferencesDto(
    Guid UserId,
    string Name
);