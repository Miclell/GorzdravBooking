using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.PatientMenu.CreatePatientFlow.Commands;

public class RunCreatePatientFlowCommand : IMenuCommand
{
    public string Title { get; } = "Создать пациента";
    
    public Task<MenuResult> ExecuteAsync(CancellationToken ct = new CancellationToken())
    {
        throw new NotImplementedException();
    }
}