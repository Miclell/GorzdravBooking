using Application.DTOs.TimePreferences;
using Core.Enums;
using FluentValidation;

namespace Server.Validators.TimePreferences;

// Валидатор для одного элемента
public class CreateTimePreferenceDtoValidator : AbstractValidator<CreateTimePreferenceDto>
{
    public CreateTimePreferenceDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название обязательно")
            .MaximumLength(100).WithMessage("Название не длиннее 100 символов");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId обязателен");

        RuleFor(x => x.TimeMode)
            .IsInEnum().WithMessage("Неверный режим выбора времени")
            .Must(mode => Enum.IsDefined(typeof(TimeSelectionMode), mode))
            .WithMessage("Неверное значение режима");

        RuleFor(x => x.MaxDaysAhead)
            .GreaterThan(0).WithMessage("Макс. дней вперед > 0")
            .LessThanOrEqualTo(365).WithMessage("Макс. дней вперед ≤ 365");

        RuleFor(x => x.MinHoursFromNow)
            .GreaterThanOrEqualTo(0).WithMessage("Мин. часов от сейчас ≥ 0")
            .LessThanOrEqualTo(24 * 30).WithMessage("Мин. часов от сейчас ≤ 720");

        RuleFor(x => x.Date)
            .Must(BeNullWhenAnyTime)
            .WithMessage("Дата должна быть null при AnyTime")
            .When(x => x.TimeMode == TimeSelectionMode.AnyTime);

        RuleFor(x => x.Day)
            .Must(BeNullWhenAnyTime)
            .WithMessage("День недели должен быть null при AnyTime")
            .When(x => x.TimeMode == TimeSelectionMode.AnyTime);

        RuleFor(x => x.PreferredTimeFrom)
            .Must(BeNullWhenAnyTime)
            .WithMessage("Время с должно быть null при AnyTime")
            .When(x => x.TimeMode == TimeSelectionMode.AnyTime);

        RuleFor(x => x.PreferredTimeTo)
            .Must(BeNullWhenAnyTime)
            .WithMessage("Время до должно быть null при AnyTime")
            .When(x => x.TimeMode == TimeSelectionMode.AnyTime);

        RuleFor(x => x.Date)
            .Must(BeNullWhenWeekdayPattern)
            .WithMessage("Дата должна быть null при WeekdayPattern")
            .When(x => x.TimeMode == TimeSelectionMode.WeekdayPattern);

        RuleFor(x => x.Day)
            .Must(BeNullWhenSpecificDates)
            .WithMessage("День недели должен быть null при SpecificDates")
            .When(x => x.TimeMode == TimeSelectionMode.SpecificDates);

        RuleFor(x => x)
            .Must(HaveValidTimeRange)
            .WithMessage("Время с должно быть раньше Время до");

        RuleFor(x => x.ExcludedDates)
            .Must(dates => dates is not { Count: 0 })
            .WithMessage("Исключаемые даты не должны быть пустым списком");
    }

    private static bool BeNullWhenAnyTime(CreateTimePreferenceDto dto, DateOnly? value)
    {
        return value == null;
    }

    private static bool BeNullWhenAnyTime(CreateTimePreferenceDto dto, DayOfWeek? value)
    {
        return value == null;
    }

    private static bool BeNullWhenAnyTime(CreateTimePreferenceDto dto, TimeOnly? value)
    {
        return value == null;
    }

    private static bool BeNullWhenWeekdayPattern(CreateTimePreferenceDto dto, DateOnly? value)
    {
        return value == null;
    }

    private static bool BeNullWhenSpecificDates(CreateTimePreferenceDto dto, DayOfWeek? value)
    {
        return value == null;
    }

    private static bool HaveValidTimeRange(CreateTimePreferenceDto dto)
    {
        return dto.PreferredTimeFrom == null || dto.PreferredTimeTo == null ||
               dto.PreferredTimeFrom < dto.PreferredTimeTo;
    }
}

public class CreateTimePreferenceListValidator : AbstractValidator<List<CreateTimePreferenceDto>>
{
    public CreateTimePreferenceListValidator()
    {
        RuleFor(x => x)
            .NotEmpty().WithMessage("Временные предпочтения не должны быть пустыми");

        RuleForEach(x => x)
            .SetValidator(new CreateTimePreferenceDtoValidator());
    }
}