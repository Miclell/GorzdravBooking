using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace StatefulMenu.Commands.BuiltIn;

public class ReloadCommand : IMenuCommand
{
    public string Title => "Обновить";

    public Task<MenuResult> ExecuteAsync(CancellationToken ct = default)
    {
        return Task.FromResult<MenuResult>(MenuResult.None());
    }
}