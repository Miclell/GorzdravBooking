using Application.DTOs.TimePreferences;
using Core.Entities;

namespace Application.Extensions;

public static class TimePreferenceExtensions
{
    public static TimePreference ToEntity(this CreateTimePreferenceDto dto)
    {
        TimePreference preference = dto switch
        {
            { Day: not null } => new WeekDayPreference
            {
                Name = dto.Name,
                UserId = dto.UserId,
                PreferredTimeFrom = dto.PreferredTimeFrom,
                PreferredTimeTo = dto.PreferredTimeTo,
                MaxDaysAhead = dto.MaxDaysAhead,
                MinHoursFromNow = dto.MinHoursFromNow,
                Day = dto.Day.Value
            },
            { Date: not null } => new MonthDayPreference
            {
                Name = dto.Name,
                UserId = dto.UserId,
                PreferredTimeFrom = dto.PreferredTimeFrom,
                PreferredTimeTo = dto.PreferredTimeTo,
                MaxDaysAhead = dto.MaxDaysAhead,
                MinHoursFromNow = dto.MinHoursFromNow,
                Date = dto.Date.Value
            },
            _ => throw new InvalidOperationException("Day and Date dont must be null")
        };
        
        return preference;
    }
    
    public static TimePreferenceDto ToDto(this TimePreference timePreference)
    {
        return timePreference switch
        {
            WeekDayPreference week => new TimePreferenceDto(
                Date: null,
                Day: week.Day,
                From: timePreference.PreferredTimeFrom,
                To: timePreference.PreferredTimeTo
            ),
            MonthDayPreference month => new TimePreferenceDto(
                Date: month.Date,
                Day: null,
                From: timePreference.PreferredTimeFrom,
                To: timePreference.PreferredTimeTo
            ),
            _ => throw new InvalidOperationException($"Unknown preference type: {timePreference.GetType().Name}")
        };
    }
}