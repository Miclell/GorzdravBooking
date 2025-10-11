namespace Application.DTOs.Appointment;

public record CreateAppointmentDto(
    Guid PatientProfileId,
    string AppointmentId,
    DateTime VisitStart,
    DateTime VisitEnd,
    string? Address = null,
    string? Number = null,
    string? Room = null
);