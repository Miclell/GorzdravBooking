using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.Commands;

public class ShowDistrictMenuCommand(IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title => "Выбрать район";

    public async Task<MenuState?> ExecuteAsync()
    {
        var districtMenuProvider = serviceProvider.GetRequiredService<DistrictMenuProvider>();
        return await districtMenuProvider.CreateMenuAsync();
    }
}