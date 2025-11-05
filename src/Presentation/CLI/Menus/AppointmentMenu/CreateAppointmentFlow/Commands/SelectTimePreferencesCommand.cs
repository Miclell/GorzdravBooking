using System.Globalization;
using Application.DTOs.AppointmentSearchRequest;
using Application.DTOs.TimePreferences;
using CLI.Menus.AppointmentMenu.CreateAppointmentFlow.Providers;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu.CreateAppointmentFlow.Commands;

public class SelectTimePreferencesCommand(
    TimePreferencesPresetDto timePreferencesPresetDto,
    IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title { get; } = $"{timePreferencesPresetDto.Name}";

    public async Task<MenuResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var dataService = serviceProvider.GetRequiredService<IDataService>();
        dataService.Set(nameof(TimePreferencesPresetDto), timePreferencesPresetDto);

        var inputService = serviceProvider.GetRequiredService<IConsoleInputService>();
        var createAppointmentSearchRequestDto =
            await inputService.ReadModelAsync<CreateAppointmentSearchRequestDto>(cancellationToken);
        
        createAppointmentSearchRequestDto!.TimePreferencesPresetName = timePreferencesPresetDto.Name;
        createAppointmentSearchRequestDto.SpecificStartPoints = ReadSpecificStartPoints();
        dataService.Set(nameof(CreateAppointmentSearchRequestDto), createAppointmentSearchRequestDto);

        var createAppointmentProvider = serviceProvider.GetRequiredService<CreateAppointmentProvider>();
        return MenuResult.Push(await createAppointmentProvider.CreateMenuAsync(cancellationToken));
    }

    private static List<DateTime> ReadSpecificStartPoints()
    {
        Console.WriteLine("Добавьте стартовые точки через ; (необязательно)");
        Console.Write("Введите даты (HH:mm): ");
    
        var input = Console.ReadLine();
    
        if (string.IsNullOrWhiteSpace(input))
            return [];
    
        var startPoints = new List<DateTime>();
        var timeStrings = input.Split(';', StringSplitOptions.RemoveEmptyEntries);
    
        foreach (var timeString in timeStrings)
        {
            var trimmedTime = timeString.Trim();
        
            if (DateTime.TryParseExact(trimmedTime, "HH:mm", 
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime time))
            {
                var dateTime = DateTime.Today.Add(time.TimeOfDay);
                startPoints.Add(dateTime);
            }
            else
            {
                Console.WriteLine($"Неверный формат даты: '{trimmedTime}'");
            }
        }
        return startPoints;
    }
}