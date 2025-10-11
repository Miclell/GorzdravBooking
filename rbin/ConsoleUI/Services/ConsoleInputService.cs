using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Application.Validators.Abstract;
using ConsoleUI.Services.Interfaces;
using Core.Attributes;

namespace ConsoleUI.Services;

public class ConsoleInputService : IConsoleInputService
{
    private readonly Dictionary<Type, Func<string, object>> _typeConverters;

    public ConsoleInputService()
    {
        _typeConverters = new Dictionary<Type, Func<string, object>>
        {
            [typeof(string)] = input => input,
            [typeof(int)] = input => int.Parse(input),
            [typeof(DateTime)] = input => DateTime.ParseExact(input, "dd.MM.yyyy", CultureInfo.InvariantCulture),
            [typeof(TimeSpan)] = input => TimeSpan.FromMinutes(int.Parse(input)),
            [typeof(bool)] = input => ParseBool(input),
            [typeof(decimal)] = input => decimal.Parse(input),
            [typeof(double)] = input => double.Parse(input),
            [typeof(Guid)] = input => Guid.Parse(input)
        };
    }

    public Task<T?> ReadModelAsync<T>()
    {
        var result = ReadModel(typeof(T));
        return Task.FromResult(result is not null ? (T)result : default);
    }

    public object? ReadModel(Type modelType)
    {
        var properties = modelType.GetProperties()
            .Where(p => p.GetCustomAttribute<InputFieldAttribute>() != null)
            .OrderBy(p => p.GetCustomAttribute<InputFieldAttribute>()?.Order ?? 0)
            .ToList();

        if (properties.Count == 0)
        {
            Console.WriteLine($"⚠ Нет свойств с атрибутом [InputField] в типе {modelType.Name}");
            return null;
        }

        Console.WriteLine();
        Console.WriteLine($"── Ввод данных для {modelType.Name} ──");
        Console.WriteLine();

        var model = CreateModelInstance(modelType);
        if (model is null) return null;

        foreach (var property in properties)
        {
            var attribute = property.GetCustomAttribute<InputFieldAttribute>();
            if (attribute is null) continue;

            var value = ReadProperty(property.PropertyType, attribute);
            if (value is not null)
            {
                SetPropertyValue(model, property, value);
            }
        }

        return model;
    }

    private static object? CreateModelInstance(Type modelType)
    {
        try
        {
            var constructors = modelType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            
            if (constructors.Length == 0)
            {
                Console.WriteLine($"⚠ Нет публичных конструкторов в {modelType.Name}");
                return null;
            }

            var constructor = constructors.OrderBy(c => c.GetParameters().Length).First();
            var parameters = constructor.GetParameters();
            
            if (parameters.Length == 0)
            {
                return constructor.Invoke(null);
            }

            var paramValues = new object?[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                paramValues[i] = GetDefaultValue(parameters[i].ParameterType);
            }

            return constructor.Invoke(paramValues);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ Не удалось создать экземпляр {modelType.Name}: {ex.Message}");
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
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ Не удалось установить значение для {property.Name}: {ex.Message}");
        }
    }

    private object? ReadProperty(Type propertyType, InputFieldAttribute attribute)
    {
        while (true)
        {
            try
            {
                Console.Write($"{attribute.DisplayName}: ");
                if (!attribute.IsRequired)
                {
                    Console.Write("(необязательно) ");
                }

                var input = Console.ReadLine()?.Trim();

                var underlyingType = Nullable.GetUnderlyingType(propertyType);
                var targetType = underlyingType ?? propertyType;

                if (string.IsNullOrEmpty(input))
                {
                    if (attribute.IsRequired)
                    {
                        Console.WriteLine("⚠ Поле обязательно для заполнения!");
                        continue;
                    }
                    return underlyingType is not null ? null : GetDefaultValue(targetType);
                }

                if (attribute.Validators.Length > 0)
                {
                    var validationResult = ValidateInput(input, attribute.Validators);
                    if (!validationResult.isValid)
                    {
                        Console.WriteLine($"{validationResult.errorMessage}");
                        continue;
                    }
                }

                if (!string.IsNullOrEmpty(attribute.Pattern) && 
                    !Regex.IsMatch(input, attribute.Pattern))
                {
                    var errorMessage = attribute.ErrorMessage ?? "⚠ Некорректный ввод!";
                    Console.WriteLine(errorMessage);
                    continue;
                }

                if (_typeConverters.TryGetValue(targetType, out var converter))
                {
                    return converter(input);
                }

                return Convert.ChangeType(input, targetType);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠ Ошибка: {ex.Message}");
            }
        }
    }

    private static (bool isValid, string errorMessage) ValidateInput(string input, Type[] validatorTypes)
    {
        foreach (var validatorType in validatorTypes)
        {
            if (!typeof(IInputValidator).IsAssignableFrom(validatorType)) continue;
            
            try
            {
                if (Activator.CreateInstance(validatorType) is IInputValidator validator && 
                    !validator.Validate(input, out var errorMessage) && 
                    !string.IsNullOrEmpty(errorMessage))
                {
                    return (false, errorMessage);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка валидатора {validatorType.Name}: {ex.Message}");
            }
        }
        return (true, string.Empty);
    }

    private static bool ParseBool(string input)
    {
        return input.ToLower() switch
        {
            "да" or "yes" or "true" or "1" => true,
            "нет" or "no" or "false" or "0" => false,
            _ => throw new FormatException("Введите 'да' или 'нет'")
        };
    }

    private static object? GetDefaultValue(Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }

    public void RegisterConverter<T>(Func<string, T> converter)
    {
        _typeConverters[typeof(T)] = input => converter(input)!;
    }
}