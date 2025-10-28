using StatefulMenu.Core.Models;

namespace StatefulMenu.Commands.Interfaces;

public interface IMenuProvider
{
    Task<MenuState> CreateMenuAsync(CancellationToken ct = default);
}