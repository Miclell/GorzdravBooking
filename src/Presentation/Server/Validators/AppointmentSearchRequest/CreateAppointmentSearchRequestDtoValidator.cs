using Application.DTOs.AppointmentSearchRequest;
using Core.Enums;
using FluentValidation;

namespace Server.Validators.AppointmentSearchRequest;

public class CreateAppointmentSearchRequestDtoValidator : AbstractValidator<CreateAppointmentSearchRequestDto>
{
    public CreateAppointmentSearchRequestDtoValidator()
    {
        RuleFor(x => x.PatientProfileId)
            .NotEmpty().WithMessage("ID профиля пациента обязателен");

        RuleFor(x => x.LpuName)
            .NotEmpty().WithMessage("Название ЛПУ обязательно");

        RuleFor(x => x.Speciality)
            .NotEmpty().WithMessage("Специальность обязательна");

        RuleFor(x => x.DoctorMode)
            .IsInEnum().WithMessage("Неверный режим выбора врача");

        RuleFor(x => x)
            .Must(HaveAtLeastOneDoctorIdentifier)
            .WithMessage("Должен быть указан хотя бы один врач")
            .When(x => x.DoctorMode != DoctorSelectionMode.AnyOfSpeciality);

        RuleFor(x => x.TimePreferencesPresetName)
            .NotEmpty().WithMessage("Название пресета временных предпочтений обязательно")
            .MaximumLength(100).WithMessage("Название пресета не длиннее 100 символов");

        RuleFor(x => x.SearchInterval)
            .Must(interval => interval > TimeSpan.Zero)
            .WithMessage("Интервал поиска должен быть больше 0")
            .Must(interval => interval <= TimeSpan.FromHours(24))
            .WithMessage("Интервал поиска не должен превышать 24 часа");

        RuleFor(x => x.MaxDaysToSearch)
            .GreaterThan(0).WithMessage("Макс. дней для поиска > 0")
            .LessThanOrEqualTo(365).WithMessage("Макс. дней для поиска ≤ 365");

        RuleFor(x => x.SpecificStartPoints)
            .Must(points => points is not { Count: 0 })
            .WithMessage("Конкретные точки старта не должны быть пустым списком");

        RuleFor(x => x.ReferralNumber)
            .MaximumLength(50).WithMessage("Номер направления не длиннее 50 символов")
            .When(x => !string.IsNullOrWhiteSpace(x.ReferralNumber));
    }

    private static bool HaveAtLeastOneDoctorIdentifier(CreateAppointmentSearchRequestDto dto)
    {
        return dto is { DoctorIds: { Count: > 0 }, DoctorNames.Count: > 0 };
    }
}