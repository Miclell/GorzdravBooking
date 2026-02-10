using Application.DTOs.TimePreferences;
using FluentValidation;

namespace Server.Validators.TimePreferences;

public class DeleteTimePreferencesDtoValidator : AbstractValidator<DeleteTimePreferencesDto>
{
    public DeleteTimePreferencesDtoValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId обязателен");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название обязательно")
            .MaximumLength(100).WithMessage("Название не длиннее 100 символов");
    }
}