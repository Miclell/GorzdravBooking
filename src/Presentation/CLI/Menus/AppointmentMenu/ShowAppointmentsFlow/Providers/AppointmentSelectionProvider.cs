using Application.DTOs.Appointment;
using CLI.Helpers;
using CLI.Menus.AppointmentMenu.ShowActiveAppointmentsFlow.Commands;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.BuiltIn;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu.ShowAppointmentsFlow.Providers;

public class AppointmentSelectionProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public Task<MenuState> CreateMenuAsync(CancellationToken cancellationToken = default)
    {
        var commands = new IMenuCommand[]
        {
            serviceProvider.GetRequiredService<CancelAppointmentCommand>(),
            serviceProvider.GetRequiredService<BackCommand>()
        };

        var items = commands
            .Select(c =>
                new MenuItem(c.Title, _ => c.ExecuteAsync(cancellationToken)))
            .ToList();

        var dataService = serviceProvider.GetRequiredService<IDataService>();
        dataService.TryGet<AppointmentListItemDto>(nameof(AppointmentListItemDto), out var appointment);
        return Task.FromResult(new MenuState($"Выберите действие для записи " +
                                             $"{appointment!.LpuShortName} " +
                                             $"{appointment.PatientFullName} | " +
                                             $"{appointment.VisitStart}", items,
            header: HeaderFactorySetup.SetupHeader()));
    }
}