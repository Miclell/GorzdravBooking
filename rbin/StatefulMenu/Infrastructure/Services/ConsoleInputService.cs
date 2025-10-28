using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using StatefulMenu.Core.Attributes;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Infrastructure.Localization;

namespace StatefulMenu.Infrastructure.Services;

public class ConsoleInputService : IConsoleInputService
{
    private readonly CultureInfo _culture;
    private readonly IConsoleLocalizer _loc;
    private readonly Dictionary<Type, Func<string, object>> _typeConverters;

    public ConsoleInputService(IConsoleLocalizer? localizer = null, CultureInfo? culture = null)
    {
        _culture = culture ?? CultureInfo.CurrentCulture;
        _loc = localizer ?? (_culture.TwoLetterISOLanguageName == "ru"
            ? new RuConsoleLocalizer()
            : new EnConsoleLocalizer());
        _typeConverters = new Dictionary<Type, Func<string, object>>
        {
            [typeof(string)] = input => input,
            [typeof(int)] = input => int.Parse(input, _culture),
            [typeof(long)] = input => long.Parse(input, _culture),
            [typeof(short)] = input => short.Parse(input, _culture),
            [typeof(byte)] = input => byte.Parse(input, _culture),
            [typeof(decimal)] = input => decimal.Parse(input, _culture),
            [typeof(double)] = input => double.Parse(input, _culture),
            [typeof(float)] = input => float.Parse(input, _culture),
            [typeof(Guid)] = input => Guid.Parse(input),
            [typeof(bool)] = input => ParseBool(input),
            [typeof(DateTime)] = input => ParseDateTime(input, _culture),
            [typeof(DateOnly)] = input => DateOnly.Parse(input, _culture),
            [typeof(TimeOnly)] = input => TimeOnly.Parse(input, _culture),
            [typeof(TimeSpan)] = input => TimeSpan.Parse(input, _culture)
        };
    }

    public Task<T?> ReadModelAsync<T>(CancellationToken ct = default)
    {
        var result = ReadModel(typeof(T), ct);
        return Task.FromResult(result is not null ? (T) result : default);
    }

    private object? ReadModel(Type modelType, CancellationToken ct)
    {
        var properties = modelType.GetProperties()
            .Where(p => p.GetCustomAttribute<InputFieldAttribute>() != null)
            .OrderBy(p => p.GetCustomAttribute<InputFieldAttribute>()?.Order ?? 0)
            .ToList();

        if (properties.Count == 0)
        {
            Console.WriteLine(_loc.NoInputFields(modelType.Name));
            return null;
        }

        Console.WriteLine();
        Console.WriteLine(_loc.InputHeader(modelType.Name));
        Console.WriteLine();

        // First collect values for all properties
        var collectedValues = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in properties)
        {
            if (ct.IsCancellationRequested) return null;
            var attribute = property.GetCustomAttribute<InputFieldAttribute>();
            if (attribute is null) continue;

            var value = ReadProperty(property.PropertyType, attribute, ct);
            collectedValues[property.Name] = value;
        }

        // Try to construct via a constructor using collected values (record-friendly)
        var instance = TryConstructWithParameters(modelType, collectedValues);
        if (instance is null) instance = CreateModelInstance(modelType);
        if (instance is null) return null;

        // Set any settable properties (covers classes and init-setters where accessible)
        foreach (var property in properties)
        {
            if (!collectedValues.TryGetValue(property.Name, out var value) || value is null) continue;
            SetPropertyValue(instance, property, value);
        }

        return instance;
    }

    private static object? CreateModelInstance(Type modelType)
    {
        try
        {
            // Prefer the constructor with the fewest parameters; for records with required parameters,
            // we initialize with defaults and then set mutable properties via setters/reflection.
            var constructors = modelType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            if (constructors.Length == 0) return Activator.CreateInstance(modelType);

            var constructor = constructors.OrderBy(c => c.GetParameters().Length).First();
            var parameters = constructor.GetParameters();

            if (parameters.Length == 0) return constructor.Invoke(null);

            var paramValues = new object?[parameters.Length];
            for (var i = 0; i < parameters.Length; i++) paramValues[i] = GetDefaultValue(parameters[i].ParameterType);

            return constructor.Invoke(paramValues);
        }
        catch
        {
            return null;
        }
    }

    private static void SetPropertyValue(object model, PropertyInfo property, object value)
    {
        try
        {
            if (property.CanWrite)
            {
                property.SetValue(model, value);
            }
            else
            {
                var setMethod = property.GetSetMethod(true);
                setMethod?.Invoke(model, [value]);
            }
        }
        catch
        {
            // swallow and continue to next property
        }
    }

    private object? ReadProperty(Type propertyType, InputFieldAttribute attribute, CancellationToken ct)
    {
        while (true)
        {
            if (ct.IsCancellationRequested) return null;

            try
            {
                Console.Write($"{attribute.DisplayName}: ");
                if (!attribute.IsRequired) Console.Write(_loc.OptionalHint);

                var raw = Console.ReadLine();
                if (raw is null)
                    // End of input stream (e.g., tests). Abort this field to avoid infinite loop.
                    return null;
                var input = raw.Trim();

                var underlyingType = Nullable.GetUnderlyingType(propertyType);
                var targetType = underlyingType ?? propertyType;

                if (string.IsNullOrEmpty(input))
                {
                    if (attribute.IsRequired)
                    {
                        Console.WriteLine(_loc.RequiredFieldMessage);
                        continue;
                    }

                    return underlyingType is not null ? null : GetDefaultValue(targetType);
                }

                if (attribute.Validators.Length > 0)
                {
                    var (isValid, errorMessage) = ValidateInput(input, attribute.Validators);
                    if (!isValid)
                    {
                        Console.WriteLine(errorMessage);
                        continue;
                    }
                }

                if (!string.IsNullOrEmpty(attribute.Pattern) &&
                    !Regex.IsMatch(input, attribute.Pattern))
                {
                    var errorMessage = attribute.ErrorMessage ?? _loc.DefaultRegexError;
                    Console.WriteLine(errorMessage);
                    continue;
                }

                if (attribute.Converters.Length <= 0)
                {
                    if (targetType.IsEnum)
                    {
                        if (Enum.TryParse(targetType, input, true, out var enumValue))
                            return enumValue!;
                        Console.WriteLine(_loc.EnumInvalidValue(Enum.GetNames(targetType)));
                        continue;
                    }

                    return _typeConverters.TryGetValue(targetType, out var converter)
                        ? converter(input)
                        : Convert.ChangeType(input, targetType, _culture);
                }

                {
                    var (converted, ok, errorMessage) = TryConverters(attribute.Converters, targetType, input);
                    if (ok) return converted;
                    Console.WriteLine(errorMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{_loc.ErrorPrefix}: {ex.Message}");
            }
        }
    }

    private (bool isValid, string errorMessage) ValidateInput(string input, Type[] validatorTypes)
    {
        foreach (var validatorType in validatorTypes)
        {
            var validateMethod = validatorType.GetMethod("Validate", BindingFlags.Instance | BindingFlags.Public);
            if (validateMethod == null || validateMethod.ReturnType != typeof(bool))
                continue;

            object? instance;
            try
            {
                instance = Activator.CreateInstance(validatorType);
            }
            catch (Exception ex)
            {
                return (false, $"{_loc.ErrorPrefix} валидатора {validatorType.Name}: {ex.Message}");
            }

            var parameters = new object?[] {input, null};
            var ok = (bool) (validateMethod.Invoke(instance, parameters) ?? false);
            if (ok) continue;
            var message = parameters[1] as string ?? string.Empty;
            return !string.IsNullOrEmpty(message)
                ? (false, message)
                : (false, _loc.ValidatorRejected(validatorType.Name));
        }

        return (true, string.Empty);
    }

    private (object? value, bool ok, string error) TryConverters(Type[] converterTypes, Type targetType, string input)
    {
        foreach (var converterType in converterTypes)
        {
            var convertMethod = converterType.GetMethod("Convert", BindingFlags.Instance | BindingFlags.Public);
            if (convertMethod == null) continue;

            object? instance;
            try
            {
                instance = Activator.CreateInstance(converterType);
            }
            catch (Exception ex)
            {
                return (null, false, $"{_loc.ErrorPrefix} конвертера {converterType.Name}: {ex.Message}");
            }

            var args = new object?[] {input, null};
            var result = convertMethod.Invoke(instance, args);

            if (result is not null)
            {
                if (targetType.IsInstanceOfType(result)) return (result, true, string.Empty);

                try
                {
                    var changed = Convert.ChangeType(result, targetType, CultureInfo.CurrentCulture);
                    return (changed, true, string.Empty);
                }
                catch
                {
                    // ignored
                }
            }

            var errorMessage = args[1] as string ?? string.Empty;
            if (!string.IsNullOrEmpty(errorMessage)) return (null, false, errorMessage);
        }

        return (null, false, _loc.ConvertFailed);
    }

    private static object? GetDefaultValue(Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }

    private bool ParseBool(string input)
    {
        var lower = input.ToLowerInvariant();
        if (_loc.YesWords.Contains(lower)) return true;
        if (_loc.NoWords.Contains(lower)) return false;
        throw new FormatException(_loc.BoolExpectedYesNo(_loc.YesWords.First(), _loc.NoWords.First()));
    }

    private static DateTime ParseDateTime(string input, CultureInfo culture)
    {
        // Try common formats first then culture default
        string[] formats =
        [
            "dd.MM.yyyy",
            "dd/MM/yyyy",
            "yyyy-MM-dd",
            "dd.MM.yyyy HH:mm",
            "yyyy-MM-ddTHH:mm"
        ];

        return DateTime.TryParseExact(input, formats, culture, DateTimeStyles.None, out var dt)
            ? dt
            : DateTime.Parse(input, culture);
    }

    private static object?[] MapArguments(ParameterInfo[] parameters, IDictionary<string, object?> values)
    {
        var args = new object?[parameters.Length];
        for (var i = 0; i < parameters.Length; i++)
        {
            var p = parameters[i];
            if (values.TryGetValue(p.Name ?? string.Empty, out var val))
                args[i] = val;
            else
                args[i] = p.HasDefaultValue ? p.DefaultValue : GetDefaultValue(p.ParameterType);
        }

        return args;
    }

    private static object? TryConstructWithParameters(Type modelType, IDictionary<string, object?> values)
    {
        try
        {
            var ctors = modelType.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .OrderByDescending(c => c.GetParameters().Length)
                .ToList();
            foreach (var ctor in ctors)
            {
                var ps = ctor.GetParameters();
                if (ps.All(p => values.ContainsKey(p.Name ?? string.Empty)))
                {
                    var args = MapArguments(ps, values);
                    return ctor.Invoke(args);
                }
            }
        }
        catch
        {
            // ignored
        }

        return null;
    }
}