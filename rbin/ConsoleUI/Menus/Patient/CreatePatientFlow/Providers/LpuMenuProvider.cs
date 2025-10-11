using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.Patient.CreatePatientFlow.Commands;
using ConsoleUI.Services;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.Patient.CreatePatientFlow.Providers;

public class LpuMenuProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync()
    {
        var lpuService = serviceProvider.GetRequiredService<ILpuService>();
        var navigation = serviceProvider.GetRequiredService<NavigationService>();
        
        var districtId = navigation.GetSharedData<District>(nameof(District)).Id;
        
        var lpus = await lpuService.GetByDistrictAsync(districtId);
        
        var commands = lpus
            .Select(l => new LpuSelectionCommand(l, serviceProvider))
            .Cast<IMenuCommand>()
            .Append(new BackCommand())
            .ToList();

        return new MenuState
        {
            Title = "Выбор медицинского учреждения",
            Commands = commands
        };
    }
}