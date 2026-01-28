using CLI.Menus.AppointmentMenu.CreateManualAppointmentFlow.Providers;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu.CreateManualAppointmentFlow;

public class RunCreateAppointmentFlowCommand(PatientSelectionProvider patientSelectionProvider) : IMenuCommand
{
    public string Title { get; } = "Создать запрос на запись";

    public async Task<MenuResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return MenuResult.Push(await patientSelectionProvider.CreateMenuAsync(cancellationToken));
    }
}