using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.Commands;

public class ShowDistrictMenuCommand(IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title => "Районы";

    public Task<MenuResult> ExecuteAsync(CancellationToken ct = default)
    {
        var provider = serviceProvider.GetRequiredService<DistrictMenuProvider>();
        return Task.FromResult(MenuResult.Push(provider.CreateMenuAsync(ct).Result));
    }
}

public class DistrictMenuProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public Task<MenuState> CreateMenuAsync(CancellationToken ct = default)
    {
        var items = new List<MenuItem>
        {
            new("Назад", _ => Task.FromResult(MenuResult.Pop()))
        };
        return Task.FromResult(new MenuState("Районы", items));
    }
}


