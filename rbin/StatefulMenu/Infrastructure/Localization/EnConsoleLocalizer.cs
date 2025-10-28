using StatefulMenu.Core.Interfaces;

namespace StatefulMenu.Infrastructure.Localization;

public class EnConsoleLocalizer : IConsoleLocalizer
{
    public string NoInputFields(string typeName)
    {
        return $"No properties with [InputField] in type {typeName}";
    }

    public string InputHeader(string typeName)
    {
        return $"Input for {typeName}";
    }

    public string OptionalHint => "optional. ";
    public string RequiredFieldMessage => "Field is required!";
    public string DefaultRegexError => "Invalid input!";

    public string EnumInvalidValue(string[] allowedValues)
    {
        return $"Invalid value. Allowed: {string.Join(", ", allowedValues)}";
    }

    public string ErrorPrefix => "Error";

    public string ValidatorRejected(string validatorTypeName)
    {
        return $"Validator {validatorTypeName} rejected input";
    }

    public string ConvertFailed => "Failed to convert input";

    public string BoolExpectedYesNo(string yes, string no)
    {
        return $"Enter '{yes}' or '{no}'";
    }

    public IReadOnlyCollection<string> YesWords => new[] {"yes", "true", "1"};
    public IReadOnlyCollection<string> NoWords => new[] {"no", "false", "0"};
}