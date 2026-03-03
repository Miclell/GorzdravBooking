using Application.DTOs.AppointmentSearchRequest;
using FluentValidation;

namespace Server.Validators.AppointmentSearchRequest;

public class UpdatePreferencesDtoValidator : AbstractValidator<UpdatePreferencesDto>
{
    public UpdatePreferencesDtoValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEmpty().WithMessage("ID запроса обязателен");

        RuleFor(x => x.TimePreferencesName)
            .NotEmpty().WithMessage("Название временных предпочтений обязательно")
            .MaximumLength(100).WithMessage("Название не длиннее 100 символов");

        RuleFor(x => x.SpecificStartPoints)
            .Must(points => points is not { Count: 0 })
            .WithMessage("Конкретные точки старта не должны быть пустым списком");

        RuleFor(x => x.SearchInterval)
            .Must(interval => interval > TimeSpan.Zero)
            .WithMessage("Интервал поиска должен быть больше 0")
            .Must(interval => interval <= TimeSpan.FromHours(24))
            .WithMessage("Интервал поиска не должен превышать 24 часа");

        RuleFor(x => x.MaxDaysToSearch)
            .GreaterThan(0).WithMessage("Макс. дней для поиска > 0")
            .LessThanOrEqualTo(365).WithMessage("Макс. дней для поиска ≤ 365");
    }
}