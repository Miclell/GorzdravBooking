namespace Application.DTOs.AppointmentSearchRequest;

public record UpdateTimePreferencesDto(
    Guid RequestId,
    string TimePreferencesName
);