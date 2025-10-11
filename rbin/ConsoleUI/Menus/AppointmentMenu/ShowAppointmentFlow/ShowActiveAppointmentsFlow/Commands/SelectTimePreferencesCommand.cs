using Application.Services;
using Application.UseCases.TimePreferences;
using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.AppointmentMenu.ShowAppointmentFlow.ShowActiveAppointmentsFlow.Providers;
using ConsoleUI.Menus.AppointmentMenu.TimePreferencesMenu.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.AppointmentMenu.ShowAppointmentFlow.ShowActiveAppointmentsFlow.Commands;

public class SelectTimePreferencesCommand(IServiceProvider serviceProvider) : IMenuCommand
{
    // TODO FlowCommand -> Provider (получили залупу) -> Обновили пресет
    
    public string Title { get; } = "Изменить пресет времени";
    public async Task<MenuState?> ExecuteAsync()
    {
        var appSettingsService = serviceProvider.GetService<AppSettingsService>();
        
        var getTimePreferencesUseCase = serviceProvider.GetRequiredService<GetTimePreferencesUseCase>();
        var timePreferences = await getTimePreferencesUseCase.ExecuteAsync(await appSettingsService.GetDefaultUserIdAsync());

        // var commands = timePreferences.Select(tp => new UpdateTimePreferencesCommand(tp, serviceProvider))
        //     .Cast<IMenuCommand>()
        //     .Append(new BackCommand())
        //     .ToList();

        var selectTimePreferencesProvider = serviceProvider.GetRequiredService<SelectTimePreferencesProvider>();
        return await selectTimePreferencesProvider.CreateMenuAsync();
    }
}