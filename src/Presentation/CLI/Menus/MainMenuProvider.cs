using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;
using StatefulMenu.Infrastructure.Services;

namespace CLI.Menus;

public class MainMenuProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public Task<MenuState> CreateMenuAsync(CancellationToken ct = new CancellationToken())
    {
        var navigation = serviceProvider.GetRequiredService<NavigationService>();
        
        
        throw new NotImplementedException();
    }
}