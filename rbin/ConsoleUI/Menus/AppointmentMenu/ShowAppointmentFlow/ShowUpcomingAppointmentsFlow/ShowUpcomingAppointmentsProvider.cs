using System.Text;
using Application.Services;
using Application.UseCases.Appointment;
using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.AppointmentMenu.ShowAppointmentFlow.ShowUpcomingAppointmentsFlow.Commands;
using Core.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.AppointmentMenu.ShowAppointmentFlow.ShowUpcomingAppointmentsFlow;

public class ShowUpcomingAppointmentsProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync()
    {
        var appSettingService = serviceProvider.GetRequiredService<AppSettingsService>();

        var getAppointmentsByUserUseCase = serviceProvider.GetRequiredService<GetByUserUseCase>();
        var appointments = await getAppointmentsByUserUseCase.ExecuteAsync(await appSettingService.GetDefaultUserIdAsync());

        var commands = appointments.Select(a => new AppointmentSelectionCommand(a, serviceProvider))
            .Cast<IMenuCommand>()
            .Append(new BackCommand())
            .ToList();

        return new MenuState()
        {
            Title = "Выберите запись для отмены",
            Commands = commands
        };
    }
}