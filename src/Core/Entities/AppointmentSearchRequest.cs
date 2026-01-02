using Core.Enums;

namespace Core.Entities;

public abstract class AppointmentSearchRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid PatientProfileId { get; set; }
    public PatientProfile PatientProfile { get; set; } = null!;
    
    public string LpuName { get; set; }
    public string Speciality { get; set; }
    public DoctorSelectionMode DoctorMode { get; set; } = DoctorSelectionMode.SpecificDoctorOrRange;
    public string? DoctorId { get; set; }
    public string? DoctorName { get; set; }

    public TimeSelectionMode TimeMode { get; set; } = TimeSelectionMode.AnyTime;
    public string? TimePreferencesPresetName { get; set; }  // TODO зарефакторить tp
    public TimeSpan SearchInterval { get; set; } = TimeSpan.FromMinutes(15);
    public List<DateTime>? SpecificStartPoints { get; set; }
    
    public int MaxDaysToSearch { get; set; } = 30;
    public bool ViewOnly { get; set; } = false;
    
    public SearchRequestStatus Status { get; set; } = SearchRequestStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // TODO проверить настройку чтобы GMT +3 был
    public DateTime? LastSearchAttempt { get; set; }
    public int AttemptCount { get; set; } = 0;
}