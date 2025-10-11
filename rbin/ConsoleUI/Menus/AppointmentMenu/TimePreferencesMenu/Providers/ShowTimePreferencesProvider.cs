using Application.Services;
using Application.UseCases.TimePreferences;
using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.AppointmentMenu.TimePreferencesMenu.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.AppointmentMenu.TimePreferencesMenu.Providers;

public class ShowTimePreferencesProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync()
    {
        var appSettingsService = serviceProvider.GetService<AppSettingsService>();
        
        var getTimePreferencesUseCase = serviceProvider.GetRequiredService<GetTimePreferencesUseCase>();
        var timePreferences = await getTimePreferencesUseCase.ExecuteAsync(await appSettingsService.GetDefaultUserIdAsync());
        
        var commands = timePreferences.Select(tp => new DeleteTimePreferencesCommand(tp, serviceProvider))
            .Cast<IMenuCommand>()
            .Append(new BackCommand())
            .ToList();

        return new MenuState()
        {
            Title = "Выберите пресет для удаления",
            Commands = commands
        };
    }
}