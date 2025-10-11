using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.Commands;
using ConsoleUI.Services;
using Core.Interfaces;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.Providers;

public class SpecialityMenuProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync()
    {
        var specialityService = serviceProvider.GetRequiredService<ISpecialtyService>();
        var navigation = serviceProvider.GetRequiredService<NavigationService>();
        
        var districtId = navigation.GetSharedData<Lpu>(nameof(Lpu)).Id;
        
        var specialties = await specialityService.GetByLpuAsync(districtId);
        
        var commands = specialties
            .Select(s => new SpecialitySelectionCommand(s, serviceProvider))
            .Cast<IMenuCommand>()
            .Append(new BackCommand())
            .ToList();

        return new MenuState
        {
            Title = "Выбор специальности",
            Commands = commands
        };
    }
}