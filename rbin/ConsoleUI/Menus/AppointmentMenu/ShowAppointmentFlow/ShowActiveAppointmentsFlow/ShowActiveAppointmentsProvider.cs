using Application.Services;
using Application.UseCases.Appointment;
using Application.UseCases.AppointmentSearchRequest;
using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.AppointmentMenu.ShowAppointmentFlow.ShowActiveAppointmentsFlow.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.AppointmentMenu.ShowAppointmentFlow.ShowActiveAppointmentsFlow;

public class ShowActiveAppointmentsProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync()
    {
        var appSettings = serviceProvider.GetRequiredService<AppSettingsService>();
        
        
        var getActiveAppointmentsByUserUseCase = serviceProvider.GetRequiredService<GetActiveAppointmentsUseCase>();

        var appointments =
            await getActiveAppointmentsByUserUseCase.ExecuteAsync(
                await appSettings.GetDefaultUserIdAsync());
        
        var commands = appointments.Select(r => new SelectAppointmentCommand(r, serviceProvider))
            .Cast<IMenuCommand>()
            .Append(new BackCommand())
            .ToList();
        
        return new MenuState()
        {
            Title = "Выберите запрос на запись",
            Commands = commands
        };
    }
}