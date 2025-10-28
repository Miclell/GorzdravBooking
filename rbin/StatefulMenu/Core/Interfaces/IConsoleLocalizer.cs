namespace StatefulMenu.Core.Interfaces;

public interface IConsoleLocalizer
{
    string OptionalHint { get; }
    string RequiredFieldMessage { get; }
    string DefaultRegexError { get; }
    string ErrorPrefix { get; }
    string ConvertFailed { get; }
    IReadOnlyCollection<string> YesWords { get; }
    IReadOnlyCollection<string> NoWords { get; }
    string NoInputFields(string typeName);
    string InputHeader(string typeName);
    string EnumInvalidValue(string[] allowedValues);
    string ValidatorRejected(string validatorTypeName);
    string BoolExpectedYesNo(string yes, string no);
}