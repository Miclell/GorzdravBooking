using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.TimePreferencesMenu.ShowTimePreferencesFlow;

public class RunShowTimePreferencesFlowCommand : IMenuCommand
{
    public string Title { get; } = "Показать пресеты";
    public Task<MenuResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}