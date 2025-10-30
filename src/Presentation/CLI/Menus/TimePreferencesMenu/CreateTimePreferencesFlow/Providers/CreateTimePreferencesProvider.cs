using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.TimePreferencesMenu.CreateTimePreferencesFlow.Providers;

public class CreateTimePreferencesProvider : IMenuProvider
{
    public Task<MenuState> CreateMenuAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}