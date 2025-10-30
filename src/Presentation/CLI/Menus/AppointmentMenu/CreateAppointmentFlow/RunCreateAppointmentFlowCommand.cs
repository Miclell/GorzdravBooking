using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu.CreateAppointmentFlow;

public class RunCreateAppointmentFlowCommand : IMenuCommand
{
    public string Title { get; }
    public Task<MenuResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}