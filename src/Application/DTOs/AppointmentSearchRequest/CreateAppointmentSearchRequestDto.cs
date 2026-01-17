using Core.Enums;
using StatefulMenu.Core.Attributes;

namespace Application.DTOs.AppointmentSearchRequest;

[InputModel("создания записи")]
public class CreateAppointmentSearchRequestDto
{
    public int? ReferralNumber { get; set; }

    public Guid PatientProfileId { get; set; }

    public string LpuName { get; set; } = null!;
    public string Speciality { get; set; } = null!;
    public DoctorSelectionMode DoctorMode { get; set; }
    public List<string>? DoctorIds { get; set; }
    public List<string>? DoctorNames { get; set; }

    public string TimePreferencesPresetName { get; set; } = null!;
    [InputField("Интервал поиска (мин)")] public TimeSpan SearchInterval { get; set; }
    public List<DateTime>? SpecificStartPoints { get; set; } = [];

    [InputField("Макс. дней для поиска")] public int MaxDaysToSearch { get; set; } = 30;
    [InputField("Только просмотр")] public bool ViewOnly { get; set; } = false;
}