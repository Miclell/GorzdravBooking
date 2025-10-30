using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu.ShowAppointmentFlow;

public class RunShowAppointmentFlowCommand : IMenuCommand
{
    public string Title { get; }
    public Task<MenuResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}