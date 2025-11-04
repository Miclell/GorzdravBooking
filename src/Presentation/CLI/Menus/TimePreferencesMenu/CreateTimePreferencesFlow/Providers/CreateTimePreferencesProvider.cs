using Application.DTOs.TimePreferences;
using Application.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Attributes;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.TimePreferencesMenu.CreateTimePreferencesFlow.Providers;

// TODO сделать нормальный ввод
public class CreateTimePreferencesProvider(
    ITimePreferencesService timePreferencesService,
    IAppSettingsService appSettingsService,
    IServiceProvider serviceProvider) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync(CancellationToken cancellationToken = default)
    {
        var createTimePreferenceDto = await ReadTimePreferences(await appSettingsService.GetDefaultUserIdAsync());
        var dto = createTimePreferenceDto.ToList();
        Console.WriteLine(dto.First().UserId);
        await timePreferencesService.CreateRangeAsync(dto, cancellationToken);

        Console.WriteLine("Пресет успешно создан! Нажмите клавишу для продолжения..");
        Console.ReadKey();

        var mainMenuProvider = serviceProvider.GetRequiredService<MainMenuProvider>();
        return mainMenuProvider.CreateMenuAsync(cancellationToken).Result;
    }

    private async Task<IEnumerable<CreateTimePreferenceDto>> ReadTimePreferences(Guid userId)
    {
        var inputService = serviceProvider.GetRequiredService<IConsoleInputService>();
        var baseFields = await inputService.ReadModelAsync<TimePreferencesInputHelperDto>();

        var result = new List<CreateTimePreferenceDto>();

        if (baseFields!.AnyTime)
        {
            var preference = new CreateTimePreferenceDto
            (
                UserId: userId,
                Name: baseFields.Name,
                AnyTime: baseFields.AnyTime,
                Day: null,
                PreferredTimeFrom: null,
                PreferredTimeTo: null
            );
            result.Add(preference);
        }
        else
        {
            foreach (var day in Enum.GetValues<DayOfWeek>())
            {
                Console.WriteLine($"Добавить предпочтение для {day}? (да/нет)");
                var input = Console.ReadLine()?.Trim().ToLower();

                while (input == "да")
                {
                    var preference = await inputService.ReadModelAsync<CreateTimePreferenceDto>();
                    result.Add(preference!);
                    Console.WriteLine($"Добавить еще одно предпочтение для {day}? (да/нет)");
                    input = Console.ReadLine()?.Trim().ToLower();
                }
            }

            for (var i = 0; i < result.Count; i++)
                result[i] = result[i] with
                {
                    UserId = userId,
                    Name = baseFields.Name,
                    AnyTime = baseFields.AnyTime
                };
        }

        return result;
    }

    private class TimePreferencesInputHelperDto
    {
        [InputField("Имя")] public string Name { get; } = null!;

        [InputField("Любое время? (да/нет)")] public bool AnyTime { get; } = false;
    }
}