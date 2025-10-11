using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.Patient.CreatePatientFlow.Commands;
using Core.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.Patient.CreatePatientFlow.Providers;

public class DistrictMenuProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync()
    {
        var districtService = serviceProvider.GetRequiredService<IDistrictService>();
        var districts = await districtService.GetDistrictsAsync();
        
        var commands = districts
            .Select(d => new DistrictSelectionCommand(d, serviceProvider))
            .Cast<IMenuCommand>()
            .Append(new BackCommand())
            .ToList();

        return new MenuState
        {
            Title = "Выбор района",
            Commands = commands
        };
    }
}