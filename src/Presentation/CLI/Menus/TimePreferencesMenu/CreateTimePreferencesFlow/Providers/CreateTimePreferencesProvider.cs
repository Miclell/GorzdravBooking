using Application.DTOs.TimePreferences;
using Application.Services.Interfaces;
using CLI.Extensions.Converters;
using Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Attributes;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.TimePreferencesMenu.CreateTimePreferencesFlow.Providers;

public class CreateTimePreferencesProvider(
    ITimePreferencesService timePreferencesService,
    IAppSettingsService appSettingsService,
    IServiceProvider serviceProvider) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync(CancellationToken cancellationToken = default)
    {
        var createTimePreferenceDto = await ReadTimePreferences(await appSettingsService.GetDefaultUserIdAsync());
        var dto = createTimePreferenceDto.ToList();

        var result = await timePreferencesService.CreateRangeAsync(dto, cancellationToken);
        if (result.IsFailure)
        {
            Console.WriteLine($"Ошибка создания пресета: {result.Error}");
            Console.WriteLine("Нажмите клавишу для продолжения..");
        }
        else
        {
            Console.WriteLine("Пресет успешно создан! Нажмите клавишу для продолжения..");
        }

        Console.ReadKey();

        var mainMenuProvider = serviceProvider.GetRequiredService<MainMenuProvider>();
        return await mainMenuProvider.CreateMenuAsync(cancellationToken);
    }

    private async Task<IEnumerable<CreateTimePreferenceDto>> ReadTimePreferences(Guid userId)
    {
        var inputService = serviceProvider.GetRequiredService<IConsoleInputService>();
        var baseFields = await inputService.ReadModelAsync<TimePreferencesInputModel>();

        var result = new List<CreateTimePreferenceDto>();
        if (baseFields!.AnyTime)
        {
            result.Add(new CreateTimePreferenceDto(
                baseFields.Name,
                userId,
                TimeSelectionMode.AnyTime,
                null,
                null,
                null,
                null,
                baseFields.ExcludedDates,
                baseFields.MaxDaysAhead,
                baseFields.MinHoursFromNow
            ));

            return result;
        }

        var timeSelectionMode = (TimeSelectionMode)(await inputService
            .ReadModelAsync<TimeSelectionModeInputModel>())!.TimeSelectionMode;

        if (timeSelectionMode == TimeSelectionMode.WeekdayPattern)
        {
            foreach (var day in Enum.GetValues<DayOfWeek>())
            {
                InputWeekday:
                Console.WriteLine($"Добавить предпочтение для {day}? (да/нет)");
                var input = Console.ReadLine()?.Trim().ToLower();

                if (input is not ("да" or "нет"))
                {
                    Console.WriteLine("Неверный ввод. Введите 'да' или 'нет'.");
                    goto InputWeekday;
                }

                while (input == "да")
                {
                    var preference = await inputService.ReadModelAsync<CreateTimePreferenceDto>();
                    result.Add(preference! with
                    {
                        Name = baseFields.Name,
                        UserId = userId,
                        TimeMode = timeSelectionMode,
                        Day = day,
                        ExcludedDates = baseFields.ExcludedDates,
                        MaxDaysAhead = baseFields.MaxDaysAhead,
                        MinHoursFromNow = baseFields.MinHoursFromNow
                    });
                    Console.WriteLine($"Добавить еще одно предпочтение для {day}? (да/нет)");
                    input = Console.ReadLine()?.Trim().ToLower();
                }
            }
        }
        else
        {
            var specificDates = (await inputService
                .ReadModelAsync<SpecificDatesInputModel>())!.SpecificDates;

            foreach (var date in specificDates)
            {
                InputDates:
                Console.WriteLine($"Добавить временное окно для {date}? (да/нет)");
                var input = Console.ReadLine()?.Trim().ToLower();

                if (input is not ("да" or "нет"))
                {
                    Console.WriteLine("Неверный ввод. Введите 'да' или 'нет'.");
                    goto InputDates;
                }

                while (input == "да")
                {
                    var preference = await inputService.ReadModelAsync<CreateTimePreferenceDto>();
                    result.Add(preference! with
                    {
                        Name = baseFields.Name,
                        UserId = userId,
                        TimeMode = timeSelectionMode,
                        Date = date,
                        ExcludedDates = baseFields.ExcludedDates,
                        MaxDaysAhead = baseFields.MaxDaysAhead,
                        MinHoursFromNow = baseFields.MinHoursFromNow
                    });
                    Console.WriteLine($"Добавить еще одно временное окно для {date}? (да/нет)");
                    input = Console.ReadLine()?.Trim().ToLower();
                }
            }
        }

        return result;
    }

    [InputModel("временных предпочтений")]
    private record TimePreferencesInputModel(
        [property: InputField("Имя")] string Name,
        [property: InputField("Любое время? (да/нет)")]
        bool AnyTime,
        [property: InputField("Введите даты, которые хотели бы " +
                              "исключить в формате dd.MM.yy через ;",
            Converters = [typeof(ListDateOnlyConverter)], IsRequired = false)]
        List<DateOnly> ExcludedDates,
        [property: InputField("Максимум дней от записи до номерка")]
        int MaxDaysAhead,
        [property: InputField("Минимум часов от записи до номерка")]
        int MinHoursFromNow
    );

    [InputModel("выбора предпочтения")]
    private record TimeSelectionModeInputModel(
        [property: InputField("Введите 1 если хотите выбрать " +
                              "предпочтения по дням недели или 2 для предпочтений по датам",
            Pattern = "[1-2]", ErrorMessage = "Вводите только 1 или 2!")]
        int TimeSelectionMode
    );

    [InputModel("дат")]
    private record SpecificDatesInputModel(
        [property: InputField("Введите даты, на которые хотели бы " +
                              "получить запись в формате dd.MM.yy через ;",
            Converters = [typeof(ListDateOnlyConverter)])]
        List<DateOnly> SpecificDates);
}