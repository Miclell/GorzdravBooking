using Application.UseCases.TimePreferences;
using Application.UseCases.TimePreferences.DTOs;
using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using Core.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.AppointmentMenu.TimePreferencesMenu.Commands;

public class DeleteTimePreferencesCommand(TimePreferencesPresetDto timePreferences, IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title { get; } = $"{timePreferences.Name}";
    public async Task<MenuState?> ExecuteAsync()
    {
        var deleteTimePreferencesUseCase = serviceProvider.GetRequiredService<DeleteTimePreferencesUseCase>();
        await deleteTimePreferencesUseCase.ExecuteAsync(timePreferences);
        Console.WriteLine($"Пресет {Title} успешно удален!");
        Console.ReadKey();
        
        return null;
    }
}