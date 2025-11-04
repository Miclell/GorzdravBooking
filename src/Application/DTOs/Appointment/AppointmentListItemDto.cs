namespace Application.DTOs.Appointment;

public record AppointmentListItemDto(
    Guid Id,
    DateTime VisitStart,
    DateTime VisitEnd,
    string? Address,
    string? Number,
    string PatientFullName,
    string LpuShortName,
    string Doctor
);