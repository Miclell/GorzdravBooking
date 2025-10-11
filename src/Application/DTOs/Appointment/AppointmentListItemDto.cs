namespace Application.DTOs.Appointment;

public record AppointmentListItemDto(
    Guid Id,
    DateTime VisitStart,
    DateTime VisitEnd,
    string? Address,
    string? Number,
    string PatientFullName,      // Для списка - сразу видно к кому запись
    string LpuShortName          // Для списка - где запись
);