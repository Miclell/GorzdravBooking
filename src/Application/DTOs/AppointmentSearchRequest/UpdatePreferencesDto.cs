namespace Application.DTOs.AppointmentSearchRequest;

public record UpdatePreferencesDto(
    Guid RequestId,
    string TimePreferencesName,
    List<DateTime>? SpecificStartPoints,
    TimeSpan SearchInterval,
    int MaxDaysToSearch,
    bool ViewOnly
);