using CLI.Menus.AppointmentMenu.CreateReferralAppointmentFlow.Providers;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu.CreateReferralAppointmentFlow;

public class RunCreateReferralAppointmentFlowCommand(ReferralInputProvider referralInputProvider) : IMenuCommand
{
    public string Title { get; } = "Создаст запрос на запись по направлению";

    public async Task<MenuResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return MenuResult.Push(await referralInputProvider.CreateMenuAsync(cancellationToken));
    }
}