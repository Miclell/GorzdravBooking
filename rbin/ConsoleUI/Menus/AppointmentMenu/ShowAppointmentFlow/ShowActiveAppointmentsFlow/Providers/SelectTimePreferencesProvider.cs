using Application.Services;
using Application.UseCases.TimePreferences;
using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.AppointmentMenu.ShowAppointmentFlow.ShowActiveAppointmentsFlow.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.AppointmentMenu.ShowAppointmentFlow.ShowActiveAppointmentsFlow.Providers;

public class SelectTimePreferencesProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync()
    {
        var appSettingsService = serviceProvider.GetService<AppSettingsService>();
        
        var getTimePreferencesUseCase = serviceProvider.GetRequiredService<GetTimePreferencesUseCase>();
        var timePreferences = await getTimePreferencesUseCase.ExecuteAsync(await appSettingsService.GetDefaultUserIdAsync());

        var commands = timePreferences.Select(tp => new UpdateTimePreferencesCommand(tp, serviceProvider))
            .Cast<IMenuCommand>()
            .Append(new BackCommand())
            .ToList();

        return new MenuState()
        {
            Title = "Выберите временной пресет",
            Commands = commands
        };
    }
}