using CLI.Menus.PatientMenu.CreatePatientFlow.Providers;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.PatientMenu.CreatePatientFlow;

public class RunCreatePatientFlowCommand(DistrictSelectionProvider districtSelectionProvider) : IMenuCommand
{
    public string Title { get; } = "Создать пациента";

    public async Task<MenuResult> ExecuteAsync(CancellationToken cancellationToken = default) =>
        MenuResult.Push(await districtSelectionProvider.CreateMenuAsync(cancellationToken));
}