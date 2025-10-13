using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.Commands;

public class ExitCommand : IMenuCommand
{
    public string Title => "Выход";

    public Task<MenuResult> ExecuteAsync(CancellationToken ct = default)
    {
        return Task.FromResult(MenuResult.Exit());
    }
}


