using Application.DTOs.TimePreferences;
using CLI.Menus.TimePreferencesMenu.ShowTimePreferencesFlow.Providers;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.TimePreferencesMenu.ShowTimePreferencesFlow.Commands;

public class TimePreferencesSelectionCommand(
    TimePreferencesPresetDto timePreferencesPresetDto,
    IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title { get; } = timePreferencesPresetDto.Name;

    public async Task<MenuResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var dataService = serviceProvider.GetRequiredService<IDataService>();
        dataService.Set(nameof(TimePreferencesPresetDto), timePreferencesPresetDto);

        var timePreferencesSelectionProvider = serviceProvider.GetRequiredService<TimePreferencesSelectionProvider>();
        return MenuResult.Push(await timePreferencesSelectionProvider.CreateMenuAsync(cancellationToken));
    }
}