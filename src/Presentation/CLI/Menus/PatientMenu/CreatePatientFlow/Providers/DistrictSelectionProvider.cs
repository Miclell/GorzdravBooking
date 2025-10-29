using CLI.Menus.PatientMenu.CreatePatientFlow.Commands;
using Core.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.BuiltIn;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.PatientMenu.CreatePatientFlow.Providers;

public class DistrictSelectionProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync(CancellationToken cancellationToken = default)
    {
        var districtService = serviceProvider.GetRequiredService<IExternalDistrictService>();
        var districts = await districtService.GetDistrictsAsync();
        
        var commands = districts
            .Select(d => new DistrictSelectionCommand(d, serviceProvider))
            .Cast<IMenuCommand>()
            .Append(new BackCommand())
            .ToList();
        
        var items = commands
            .Select(c => 
                new MenuItem(c.Title, _ => c.ExecuteAsync(cancellationToken)))
            .ToList();

        return new MenuState("Выбор района", items);
    }
}