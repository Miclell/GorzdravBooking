using Application.DTOs.User;
using FluentValidation;

namespace Server.Validators.User;

public class BaseUserDtoValidator : AbstractValidator<BaseUserDto>
{
    public BaseUserDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Имя пользователя обязательно")
            .MinimumLength(3).WithMessage("Имя пользователя должно быть не менее 3 символов")
            .MaximumLength(50).WithMessage("Имя пользователя должно быть не более 50 символов")
            .Matches("^[a-zA-Z0-9_]+$")
            .WithMessage("Имя пользователя может содержать только буквы, цифры и подчеркивания");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен")
            .MinimumLength(6).WithMessage("Пароль должен быть не менее 6 символов")
            .MaximumLength(100).WithMessage("Пароль должен быть не более 100 символов")
            .Must(ContainDigit).WithMessage("Пароль должен содержать хотя бы одну цифру")
            .Must(ContainLetter).WithMessage("Пароль должен содержать хотя бы одну букву");
    }

    private bool ContainDigit(string password)
    {
        return password?.Any(char.IsDigit) == true;
    }

    private bool ContainLetter(string password)
    {
        return password?.Any(char.IsLetter) == true;
    }
}