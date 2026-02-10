using Application.DTOs.Patient;
using FluentValidation;

namespace Server.Validators.Patient;

public class CreatePatientDtoValidator : AbstractValidator<CreatePatientDto>
{
    public CreatePatientDtoValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId обязателен");

        RuleFor(x => x.LpuId)
            .NotEmpty().WithMessage("ID ЛПУ обязателен");

        RuleFor(x => x.LpuShortName)
            .NotEmpty().WithMessage("Название ЛПУ обязательно");

        RuleFor(x => x.LpuAddress)
            .NotEmpty().WithMessage("Адрес ЛПУ обязателен");

        RuleFor(x => x.PatientId)
            .NotEmpty().WithMessage("ID пациента обязателен");

        RuleFor(x => x.PatientLastName)
            .NotEmpty().WithMessage("Фамилия обязательна")
            .MaximumLength(100).WithMessage("Фамилия не длиннее 100 символов");

        RuleFor(x => x.PatientFirstName)
            .NotEmpty().WithMessage("Имя обязательно")
            .MaximumLength(100).WithMessage("Имя не длиннее 100 символов");

        RuleFor(x => x.PatientMiddleName)
            .NotEmpty().WithMessage("Отчество обязательно")
            .MaximumLength(100).WithMessage("Отчество не длиннее 100 символов");

        RuleFor(x => x.PatientBirthdate)
            .NotEmpty().WithMessage("Дата рождения обязательна")
            .LessThan(DateTime.Now).WithMessage("Дата рождения должна быть в прошлом")
            .GreaterThan(DateTime.Now.AddYears(-150)).WithMessage("Дата рождения не может быть более 150 лет назад");

        RuleFor(x => x.RecipientEmail)
            .EmailAddress().WithMessage("Введите корректный email")
            .Matches(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")
            .WithMessage("Введите корректный email")
            .When(x => !string.IsNullOrWhiteSpace(x.RecipientEmail));

        RuleFor(x => x.MobilePhoneNumber)
            .Matches(@"^\d{10}$")
            .WithMessage("Телефон в формате 89991234567 или 9991234567")
            .When(x => !string.IsNullOrWhiteSpace(x.MobilePhoneNumber));
    }
}