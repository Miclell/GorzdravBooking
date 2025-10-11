namespace Application.DTOs.Appointment;

public record AppointmentDto(
    Guid Id,
    Guid PatientProfileId,
    string AppointmentId,
    DateTime VisitStart,
    DateTime VisitEnd,
    string? Address,
    string? Number,
    string? Room,
    string PatientFullName,      // "Иванов Иван Иванович"
    string LpuShortName          // "Городская больница №1"
);