using CLI.Menus.AppointmentMenu.ShowActiveAppointmentsFlow.Providers;
using CLI.Menus.AppointmentMenu.ShowAppointmentsFlow.Providers;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu.ShowAppointmentsFlow;

public class RunShowAppointmentsFlowCommand(ShowAppointmentsProvider showAppointmentsProvider) : IMenuCommand
{
    public string Title { get; } = "Готовые записи";
    public async Task<MenuResult> ExecuteAsync(CancellationToken cancellationToken = default) =>
        MenuResult.Push(await showAppointmentsProvider.CreateMenuAsync(cancellationToken));
}