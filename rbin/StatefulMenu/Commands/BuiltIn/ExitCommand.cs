using System.Threading;
using System.Threading.Tasks;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace StatefulMenu.Commands.BuiltIn;

public class ExitCommand : IMenuCommand
{
    public string Title => "Выход";

    public Task<MenuResult> ExecuteAsync(CancellationToken ct = default)
    {
        return Task.FromResult<MenuResult>(MenuResult.Exit());
    }
}