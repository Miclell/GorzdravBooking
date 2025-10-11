using Application.Converters;
using Application.UseCases.TimePreferences.Commands;

namespace ConsoleUI.Services;

public class TimePreferencesInputService
{
    public List<CreateTimePreferencesCommand> ReadModel(Guid patientProfileId)
    {
        string name;
        do
        {
            Console.Write("Введите название пресета: ");
            name = Console.ReadLine()?.Trim() ?? string.Empty;
        } while (string.IsNullOrEmpty(name));

        var yesNoConverter = new YesNoConverter("да", "нет", "Вводите только {1}/{2}!");
        bool? anyTime;
        do
        {
            Console.Write("Любое время? (да/нет): ");
            var input = Console.ReadLine();
            anyTime = yesNoConverter.Convert(input, out var message);

            if (!string.IsNullOrEmpty(message))
                Console.WriteLine($"{message}");
        } while (string.IsNullOrEmpty(anyTime?.ToString()));

        if (anyTime == true)
        {
            return [new CreateTimePreferencesCommand(
                name, patientProfileId, 
                Day: null, PreferredTimeFrom: null, PreferredTimeTo: null, 
                anyTime.Value)];
        }

        var result = new List<CreateTimePreferencesCommand>(); // TODO внедрить функциональность в сервис ввода
        foreach (var day in Enum.GetValues<DayOfWeek>())
        {
            Console.WriteLine($"Добавить предпочтение для {day}? (да/нет)");
            var input = Console.ReadLine()?.Trim().ToLower();

            while (input == "да")
            {
                Console.Write("Введите начало времени (чч:мм): ");
                var from = TimeOnly.Parse(Console.ReadLine()!);

                Console.Write("Введите конец времени (чч:мм): ");
                var to = TimeOnly.Parse(Console.ReadLine()!);

                result.Add(new CreateTimePreferencesCommand(
                    name, patientProfileId, day, from, to, false
                ));

                Console.WriteLine($"Добавить ещё один промежуток для {day}? (да/нет)");
                input = Console.ReadLine()?.Trim().ToLower();
            }
        }

        return result;
    }
}
