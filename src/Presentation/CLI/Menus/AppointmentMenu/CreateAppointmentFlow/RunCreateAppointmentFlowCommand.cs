using CLI.Menus.AppointmentMenu.CreateAppointmentFlow.Providers;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu.CreateAppointmentFlow;

public class RunCreateAppointmentFlowCommand(PatientSelectionProvider patientSelectionProvider) : IMenuCommand
{
    public string Title { get; } = "Создать запрос на запись";

    public async Task<MenuResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return MenuResult.Push(await patientSelectionProvider.CreateMenuAsync(cancellationToken));
    }
}