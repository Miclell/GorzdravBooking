using CLI.Menus.TimePreferencesMenu.CreateTimePreferencesFlow;
using CLI.Menus.TimePreferencesMenu.ShowTimePreferencesFlow;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.BuiltIn;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.TimePreferencesMenu;

public class ShowTimePreferencesMenuProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public Task<MenuState> CreateMenuAsync(CancellationToken cancellationToken = default)
    {
        var commands = new IMenuCommand[]
        {
            serviceProvider.GetRequiredService<RunCreateTimePreferencesFlowCommand>(),
            serviceProvider.GetRequiredService<RunShowTimePreferencesFlowCommand>(),
            serviceProvider.GetRequiredService<BackCommand>()
        };
        
        var items = commands
            .Select(c => 
                new MenuItem(c.Title, _ => c.ExecuteAsync(cancellationToken)))
            .ToList();

        return Task.FromResult(new MenuState("Меню временных предпочтений", items));
    }
}