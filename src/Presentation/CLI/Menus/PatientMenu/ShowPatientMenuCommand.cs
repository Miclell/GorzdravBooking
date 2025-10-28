using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.PatientMenu;

public class ShowPatientMenuCommand(ShowPatientMenuProvider showPatientMenuProvider) : IMenuCommand
{
    public string Title { get; } = "Меню пациента";
    
    public async Task<MenuResult> ExecuteAsync(CancellationToken cancellationToken = default) =>
        MenuResult.Push(await showPatientMenuProvider.CreateMenuAsync(cancellationToken));
}