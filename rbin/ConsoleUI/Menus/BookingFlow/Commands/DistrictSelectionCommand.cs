using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.Providers;
using ConsoleUI.Services;
using Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.Commands;

public class DistrictSelectionCommand(District district, IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title => district.Name;

    public async Task<MenuState?> ExecuteAsync()
    {
        var navigation = serviceProvider.GetRequiredService<NavigationService>();
        navigation.SetSharedData(nameof(District), district);
        
        var lpuMenuProvider = serviceProvider.GetRequiredService<LpuMenuProvider>();
        return await lpuMenuProvider.CreateMenuAsync();
    }
}