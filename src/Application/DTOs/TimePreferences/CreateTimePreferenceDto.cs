namespace Application.DTOs.TimePreferences;

using StatefulMenu.Core.Attributes;

public class CreateTimePreferenceDto
{
    [InputField("Название пресета")]
    public string Name { get; set; } = null!;
    
    [InputField("Пациент Id")]
    public Guid PatientProfileId { get; set; }
    
    [InputField("День недели", IsRequired = false)]
    public DayOfWeek? Day { get; set; }
    
    [InputField("Время с", IsRequired = false)]
    public TimeOnly? PreferredTimeFrom { get; set; }
    
    [InputField("Время до", IsRequired = false)]
    public TimeOnly? PreferredTimeTo { get; set; }
    
    [InputField("Любое время")]
    public bool AnyTime { get; set; } = false;
}