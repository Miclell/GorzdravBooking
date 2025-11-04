using Core.Enums;

namespace Core.Entities;

public class AppointmentSearchRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PatientProfileId { get; set; }
    public PatientProfile PatientProfile { get; set; } = null!;

    public string LpuName { get; set; } = null!;
    public string DoctorId { get; set; } = null!;
    public string DoctorName { get; set; } = null!;
    public string Speciality { get; set; } = null!;

    public TimeSpan SearchInterval { get; set; }
    public List<DateTime> SpecificStartPoints { get; set; } = [];

    public string TimePreferencesPresetName { get; set; } = null!;

    public bool ViewOnly { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastSearchAttempt { get; set; }
    public int AttemptCount { get; set; } = 0;

    public SearchRequestStatus Status { get; set; } = SearchRequestStatus.InProgress;
    public int MaxDaysToSearch { get; set; } = 30;
}