namespace Core.Entities;

public abstract class TimePreference
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Name { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public TimeOnly? PreferredTimeFrom { get; set; }
    public TimeOnly? PreferredTimeTo { get; set; }
    public List<DateOnly>? ExcludedDates { get; set; }

    public int MaxDaysAhead { get; set; } = 14;
    public int MinHoursFromNow { get; set; } = 2;
}