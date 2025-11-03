namespace Application.DTOs.AppointmentSearchRequest;

using StatefulMenu.Core.Attributes;

public class CreateAppointmentSearchRequestDto
{
    public Guid PatientProfileId { get; set; }
    public string LpuName { get; set; } = null!;
    public string DoctorId { get; set; } = null!;
    public string DoctorName { get; set; } = null!;

    [InputField("Интервал поиска (мин)")]
    public TimeSpan SearchInterval { get; set; }

    [InputField("Конкретные моменты запуска", IsRequired = false)]
    public List<DateTime> SpecificStartPoints { get; set; } = [];
    public string TimePreferencesPresetName { get; set; } = null!;

    [InputField("Только просмотр")]
    public bool ViewOnly { get; set; } = false;

    [InputField("Макс. дней для поиска")]
    public int MaxDaysToSearch { get; set; } = 30;
}