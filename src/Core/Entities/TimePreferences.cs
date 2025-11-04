namespace Core.Entities;

public class TimePreferences
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; }

    public DayOfWeek? Day { get; set; }
    public TimeOnly? PreferredTimeFrom { get; set; }
    public TimeOnly? PreferredTimeTo { get; set; }

    public bool AnyTime { get; set; } = false;
}