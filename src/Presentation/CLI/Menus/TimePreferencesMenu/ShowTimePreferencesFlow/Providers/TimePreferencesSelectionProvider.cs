using Application.DTOs.TimePreferences;
using CLI.Helpers;
using CLI.Menus.TimePreferencesMenu.ShowTimePreferencesFlow.Commands;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.BuiltIn;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.TimePreferencesMenu.ShowTimePreferencesFlow.Providers;

public class TimePreferencesSelectionProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public Task<MenuState> CreateMenuAsync(CancellationToken cancellationToken = default)
    {
        var commands = new IMenuCommand[]
        {
            serviceProvider.GetRequiredService<DeleteTimePreferencesCommand>(),
            serviceProvider.GetRequiredService<BackCommand>()
        };

        var items = commands
            .Select(c =>
                new MenuItem(c.Title, _ => c.ExecuteAsync(cancellationToken)))
            .ToList();

        var dataService = serviceProvider.GetRequiredService<IDataService>();
        dataService.TryGet<TimePreferencesPresetDto>(nameof(TimePreferencesPresetDto),
            out var timePreferencesPresetDto);
        return Task.FromResult(new MenuState($"Выберите действие для пресета {timePreferencesPresetDto!.Name}", items,
            header: HeaderFactorySetup.SetupHeader()));
    }
}