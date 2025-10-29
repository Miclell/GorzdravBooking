using StatefulMenu.Core.Models;

namespace StatefulMenu.Commands.Interfaces;

public interface IMenuCommand
{
    string Title { get; }
    Task<MenuResult> ExecuteAsync(CancellationToken cancellationToken = default);
}