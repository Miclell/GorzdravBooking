using CLI.Menus.TimePreferencesMenu.ShowTimePreferencesFlow.Providers;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.TimePreferencesMenu.ShowTimePreferencesFlow;

public class RunShowTimePreferencesFlowCommand(ShowTimePreferencesProvider showTimePreferencesProvider) : IMenuCommand
{
    public string Title { get; } = "Показать пресеты";

    public async Task<MenuResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return MenuResult.Push(await showTimePreferencesProvider.CreateMenuAsync(cancellationToken));
    }
}