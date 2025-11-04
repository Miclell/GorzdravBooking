using CLI.Menus.PatientMenu.ShowPatientsFlow.Providers;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.PatientMenu.ShowPatientsFlow;

public class RunShowPatientsFlowCommand(ShowPatientsProvider showPatientsProvider) : IMenuCommand
{
    public string Title { get; } = "Просмотр и редактирование";

    public async Task<MenuResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return MenuResult.Push(await showPatientsProvider.CreateMenuAsync(cancellationToken));
    }
}