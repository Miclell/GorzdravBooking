using Application.UseCases.AppointmentSearchRequest;
using Application.UseCases.AppointmentSearchRequest.Commands;
using Application.UseCases.TimePreferences.DTOs;
using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.AppointmentMenu.CreateAppointmentFlow.Commands;

public class SelectTimePreferencesCommand(TimePreferencesPresetDto timePreferences, IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title { get; } = $"{timePreferences.Name}";
    public async Task<MenuState?> ExecuteAsync()
    {
        var navigation = serviceProvider.GetRequiredService<NavigationService>();

        var createAppointmentSearchRequestCommand =
            navigation.GetSharedData<CreateAppointmentSearchRequestCommand>(
                nameof(CreateAppointmentSearchRequestCommand));

        createAppointmentSearchRequestCommand = createAppointmentSearchRequestCommand with
        {
            TimePreferencesPresetName = timePreferences.Name
        };
        
        var createAppointmentSearchRequestUseCase = serviceProvider.GetRequiredService<CreateAppointmentSearchRequestUseCase>();
        await createAppointmentSearchRequestUseCase.ExecuteAsync(createAppointmentSearchRequestCommand);

        var mainMenuProvider = serviceProvider.GetRequiredService<IMenuProvider>();
        return await mainMenuProvider.CreateMenuAsync();
    }
}