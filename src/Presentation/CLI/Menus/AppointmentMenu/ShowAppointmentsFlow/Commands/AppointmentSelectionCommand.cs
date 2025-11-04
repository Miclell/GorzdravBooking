using Application.DTOs.Appointment;
using Application.DTOs.AppointmentSearchRequest;
using CLI.Menus.AppointmentMenu.ShowActiveAppointmentsFlow.Providers;
using CLI.Menus.AppointmentMenu.ShowAppointmentsFlow.Providers;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu.ShowAppointmentsFlow.Commands;

public class AppointmentSelectionCommand(AppointmentListItemDto appointment, IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title { get; } = $"{appointment.LpuShortName} " +
                                   $"{appointment.PatientFullName} | " +
                                   $"{appointment.Doctor} - " +
                                   $"{appointment.VisitStart}";
    public async Task<MenuResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var dataService = serviceProvider.GetRequiredService<IDataService>();
        dataService.Set(nameof(AppointmentListItemDto), appointment);

        var appointmentSelectionProvider = serviceProvider.GetRequiredService<AppointmentSelectionProvider>();
        return MenuResult.Push(await appointmentSelectionProvider.CreateMenuAsync(cancellationToken));
    }
}