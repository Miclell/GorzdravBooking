using Application.DTOs.User;
using FluentValidation;
using Server.Validators.Common;

namespace Server.Validators.User;

public class UpdatePasswordRequestValidator : AbstractValidator<UpdatePasswordRequest>
{
    public UpdatePasswordRequestValidator()
    {
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Пароль обязателен")
            .MinimumLength(6).WithMessage("Пароль должен быть не менее 6 символов")
            .MaximumLength(100).WithMessage("Пароль должен быть не более 100 символов")
            .Must(Helper.ContainDigit).WithMessage("Пароль должен содержать хотя бы одну цифру")
            .Must(Helper.ContainLetter).WithMessage("Пароль должен содержать хотя бы одну букву");
    }
}