using StatefulMenu.Core.Interfaces;

namespace StatefulMenu.Infrastructure.Localization;

public class RuConsoleLocalizer : IConsoleLocalizer
{
    public string NoInputFields(string typeName)
    {
        return $"Нет свойств с атрибутом [InputField] в типе {typeName}";
    }

    public string InputHeader(string typeName)
    {
        return $"Ввод данных для {typeName}";
    }

    public string OptionalHint => "необязательно. ";
    public string RequiredFieldMessage => "Поле обязательно для заполнения!";
    public string DefaultRegexError => "Некорректный ввод!";

    public string EnumInvalidValue(string[] allowedValues)
    {
        return $"Некорректное значение. Допустимые: {string.Join(", ", allowedValues)}";
    }

    public string ErrorPrefix => "Ошибка";

    public string ValidatorRejected(string validatorTypeName)
    {
        return $"Валидатор {validatorTypeName} отклонил ввод";
    }

    public string ConvertFailed => "Не удалось сконвертировать ввод";

    public string BoolExpectedYesNo(string yes, string no)
    {
        return $"Введите '{yes}' или '{no}'";
    }

    public IReadOnlyCollection<string> YesWords => new[] {"да", "yes", "true", "1"};
    public IReadOnlyCollection<string> NoWords => new[] {"нет", "no", "false", "0"};
}