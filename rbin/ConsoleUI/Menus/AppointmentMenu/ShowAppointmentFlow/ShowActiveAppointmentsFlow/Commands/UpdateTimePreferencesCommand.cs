using Application.UseCases.AppointmentSearchRequest;
using Application.UseCases.TimePreferences.DTOs;
using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.Providers;
using ConsoleUI.Services;
using Core.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.AppointmentMenu.ShowAppointmentFlow.ShowActiveAppointmentsFlow.Commands;

public class UpdateTimePreferencesCommand(TimePreferencesPresetDto timePreferences, IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title { get; } = $"{timePreferences.Name}";
    public async Task<MenuState?> ExecuteAsync()
    {
        var navigation = serviceProvider.GetRequiredService<NavigationService>();
        var appointmentSearchRequest = navigation.GetSharedData<AppointmentSearchRequest>(nameof(AppointmentSearchRequest));
        
        var updateTimePreferencesUseCase = serviceProvider.GetRequiredService<UpdateTimePreferencesUseCase>();
        await updateTimePreferencesUseCase.ExecuteAsync(appointmentSearchRequest, timePreferences.Name);
        Console.WriteLine($"Пресет {Title} успешно применен!");
        Console.ReadKey();

        var mainMenuProvider = serviceProvider.GetRequiredService<MainMenuProvider>();
        return await mainMenuProvider.CreateMenuAsync();
    }
}