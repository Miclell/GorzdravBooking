using System.Threading;
using System.Threading.Tasks;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace StatefulMenu.Commands.BuiltIn;

public class BackCommand : IMenuCommand
{
    public string Title => "Назад";

    public Task<MenuResult> ExecuteAsync(CancellationToken ct = default)
    {
        return Task.FromResult<MenuResult>(MenuResult.Pop());
    }
}