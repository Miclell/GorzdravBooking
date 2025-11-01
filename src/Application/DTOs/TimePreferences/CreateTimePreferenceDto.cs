namespace Application.DTOs.TimePreferences;

using StatefulMenu.Core.Attributes;

// TODO to record
public class CreateTimePreferenceDto
{
    public CreateTimePreferenceDto(string name, Guid id, DayOfWeek day,
        TimeOnly? preferredTimeFrom, TimeOnly? preferredTimeTo,
        bool anyTime)
    {
        Name = name;
        UserId = id;
        Day = day;
        PreferredTimeFrom = preferredTimeFrom;
        PreferredTimeTo = preferredTimeTo;
        AnyTime = anyTime;
    }
    
    public CreateTimePreferenceDto() { }
    
    public string Name { get; set; }
    public Guid UserId { get; set; }
    
    public DayOfWeek? Day { get; set; }
    
    [InputField("Время с")]
    public TimeOnly? PreferredTimeFrom { get; set; }
    
    [InputField("Время до")]
    public TimeOnly? PreferredTimeTo { get; set; }
    
    public bool AnyTime { get; set; } = false;
}