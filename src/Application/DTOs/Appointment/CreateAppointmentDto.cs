namespace Application.DTOs.Appointment;

public record CreateAppointmentDto(
    Guid PatientProfileId,
    string AppointmentId,
    DateTime VisitStart,
    DateTime VisitEnd,
    string? Address,
    string? Number,
    string? Room,
    string Speciality,
    string Doctor
);