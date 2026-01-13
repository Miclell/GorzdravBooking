using Application.DTOs.AppointmentSearchRequest;
using Application.Extensions;
using CLI.Menus.AppointmentMenu.ShowActiveAppointmentsFlow.Providers;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu.ShowActiveAppointmentsFlow.Commands;

public class AppointmentSearchRequestSelectionCommand(
    AppointmentSearchRequestDto appointment,
    IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title { get; } = appointment.GetDisplayTitle();

    public async Task<MenuResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var dataService = serviceProvider.GetRequiredService<IDataService>();
        dataService.Set(nameof(AppointmentSearchRequestDto), appointment);

        var appointmentSearchRequestsSelectionProvider =
            serviceProvider.GetRequiredService<AppointmentSearchRequestsSelectionProvider>();
        return MenuResult.Push(await appointmentSearchRequestsSelectionProvider.CreateMenuAsync(cancellationToken));
    }
}