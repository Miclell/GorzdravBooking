using Application.DTOs.TimePreferences;
using Application.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.TimePreferencesMenu.ShowTimePreferencesFlow.Commands;

public class DeleteTimePreferencesCommand(
    IDataService dataService, 
    ITimePreferencesService timePreferencesService,
    MainMenuProvider mainMenuProvider) : IMenuCommand
{
    public string Title { get; } = "Удалить пресет";
    public async Task<MenuResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        dataService.TryGet<TimePreferencesPresetDto>(nameof(TimePreferencesPresetDto), out var timePreferencesPresetDto);

        var dto = new DeleteTimePreferencesDto(
            timePreferencesPresetDto!.UserId,
            timePreferencesPresetDto.Name);
        
        await timePreferencesService.DeleteByPresetAsync(dto, cancellationToken);
        
        dataService.Remove(nameof(TimePreferencesPresetDto));

        Console.WriteLine($"Пресет {dto.Name} успешно удален!\nНажмите любую клавишу для продолжения..");
        Console.ReadKey();
        
        return MenuResult.Push(mainMenuProvider.CreateMenuAsync(cancellationToken).Result);
    }
}