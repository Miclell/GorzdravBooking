using Application.DTOs.TimePreferences;
using Core.Entities;
using Core.Enums;

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
                TimeMode = TimeSelectionMode.WeekdayPattern,
                PreferredTimeFrom = dto.PreferredTimeFrom,
                PreferredTimeTo = dto.PreferredTimeTo,
                ExcludedDates = dto.ExcludedDates,
                MaxDaysAhead = dto.MaxDaysAhead,
                MinHoursFromNow = dto.MinHoursFromNow,
                Day = dto.Day.Value
            },
            { Date: not null } => new MonthDayPreference
            {
                Name = dto.Name,
                UserId = dto.UserId,
                TimeMode = TimeSelectionMode.SpecificDates,
                PreferredTimeFrom = dto.PreferredTimeFrom,
                PreferredTimeTo = dto.PreferredTimeTo,
                ExcludedDates = dto.ExcludedDates,
                MaxDaysAhead = dto.MaxDaysAhead,
                MinHoursFromNow = dto.MinHoursFromNow,
                Date = dto.Date.Value
            },
            _ => new AnyTimePreference
            {
                Name = dto.Name,
                UserId = dto.UserId,
                TimeMode = TimeSelectionMode.AnyTime,
                ExcludedDates = dto.ExcludedDates,
                MaxDaysAhead = dto.MaxDaysAhead,
                MinHoursFromNow = dto.MinHoursFromNow
            }
        };

        return preference;
    }

    public static TimePreferenceDto ToDto(this TimePreference timePreference)
    {
        return timePreference switch
        {
            WeekDayPreference week => new TimePreferenceDto(
                null,
                week.Day,
                timePreference.PreferredTimeFrom,
                timePreference.PreferredTimeTo
            ),
            MonthDayPreference month => new TimePreferenceDto(
                month.Date,
                null,
                timePreference.PreferredTimeFrom,
                timePreference.PreferredTimeTo
            ),
            _ => new TimePreferenceDto(
                null,
                null,
                null,
                null
            )
        };
    }
}