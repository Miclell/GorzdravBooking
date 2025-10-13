namespace Application.DTOs.AppointmentSearchRequest;

using StatefulMenu.Core.Attributes;

public class CreateAppointmentSearchRequestDto
{
    [InputField("Профиль пациента Id")]
    public Guid PatientProfileId { get; set; }

    [InputField("ЛПУ название")]
    public string LpuName { get; set; } = null!;

    [InputField("Идентификатор врача")]
    public string DoctorId { get; set; } = null!;

    [InputField("Имя врача")]
    public string DoctorName { get; set; } = null!;

    [InputField("Интервал поиска (мин)")]
    public TimeSpan SearchInterval { get; set; }

    [InputField("Конкретные моменты запуска", IsRequired = false)]
    public List<DateTime> SpecificStartPoints { get; set; } = [];

    [InputField("Профиль временных предпочтений")]
    public string TimePreferencesPresetName { get; set; } = null!;

    [InputField("Только просмотр")]
    public bool ViewOnly { get; set; } = false;

    [InputField("Макс. дней для поиска")]
    public int MaxDaysToSearch { get; set; } = 30;
}