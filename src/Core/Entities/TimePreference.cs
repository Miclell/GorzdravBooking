using Core.Entities.Common;
using Core.Enums;

namespace Core.Entities;

public abstract class TimePreference : IOwnedEntity
{
    public required string Name { get; set; }
    public User User { get; set; } = null!;

    public TimeSelectionMode TimeMode { get; set; }

    public TimeOnly? PreferredTimeFrom { get; set; }
    public TimeOnly? PreferredTimeTo { get; set; }
    public List<DateOnly>? ExcludedDates { get; set; }

    public int MaxDaysAhead { get; set; } = 14;
    public int MinHoursFromNow { get; set; } = 2;
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
}