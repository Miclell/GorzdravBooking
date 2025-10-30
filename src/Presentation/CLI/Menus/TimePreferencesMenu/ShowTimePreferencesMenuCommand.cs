using CLI.Menus.PatientMenu.ShowPatientsFlow.Providers;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.TimePreferencesMenu;

public class ShowTimePreferencesMenuCommand(ShowTimePreferencesMenuProvider showTimePreferencesMenuProvider) : IMenuCommand
{
    public string Title { get; } = "Временные предпочтения";
    public async Task<MenuResult> ExecuteAsync(CancellationToken cancellationToken = default) =>
        MenuResult.Push(await showTimePreferencesMenuProvider.CreateMenuAsync(cancellationToken));
}