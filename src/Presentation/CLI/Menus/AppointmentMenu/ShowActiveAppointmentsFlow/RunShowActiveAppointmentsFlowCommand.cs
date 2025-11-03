using CLI.Menus.AppointmentMenu.ShowActiveAppointmentsFlow.Providers;
using CLI.Menus.AppointmentMenu.ShowAppointmentsFlow.Providers;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu.ShowActiveAppointmentsFlow;

public class RunShowActiveAppointmentsFlowCommand(ShowAppointmentSearchRequestsProvider showAppointmentSearchRequestsProvider) : IMenuCommand
{
    public string Title { get; } = "Активные запросы";
    public async Task<MenuResult> ExecuteAsync(CancellationToken cancellationToken = default) =>
        MenuResult.Push(await showAppointmentSearchRequestsProvider.CreateMenuAsync(cancellationToken));
}