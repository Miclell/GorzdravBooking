using System.Threading;
using System.Threading.Tasks;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace StatefulMenu.Commands.BuiltIn;

public class HomeCommand : IMenuCommand
{
    public string Title => "На главную";

    public Task<MenuResult> ExecuteAsync(CancellationToken ct = default)
    {
        return Task.FromResult<MenuResult>(MenuResult.Pop(int.MaxValue));
    }
}