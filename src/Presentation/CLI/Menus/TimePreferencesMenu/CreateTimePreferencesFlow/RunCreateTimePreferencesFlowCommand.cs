using CLI.Menus.TimePreferencesMenu.CreateTimePreferencesFlow.Providers;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.TimePreferencesMenu.CreateTimePreferencesFlow;

public class RunCreateTimePreferencesFlowCommand(CreateTimePreferencesProvider createTimePreferencesProvider)
    : IMenuCommand
{
    public string Title { get; } = "Создать пресет";

    public async Task<MenuResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return MenuResult.Push(await createTimePreferencesProvider.CreateMenuAsync(cancellationToken));
    }
}