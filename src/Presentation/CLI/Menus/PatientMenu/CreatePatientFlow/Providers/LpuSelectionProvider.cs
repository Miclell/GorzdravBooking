using CLI.Menus.PatientMenu.CreatePatientFlow.Commands;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.BuiltIn;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.PatientMenu.CreatePatientFlow.Providers;

public class LpuSelectionProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync(CancellationToken cancellationToken = default)
    {
        var dataService = serviceProvider.GetRequiredService<IDataService>();
        dataService.TryGet<District>(nameof(District), out var district);
        
        var lpuService = serviceProvider.GetRequiredService<IExternalLpuService>();
        var lpus = await lpuService.GetByDistrictAsync(district!.Id);
        
        var commands = lpus
            .Select(l => new LpuSelectionCommand(l, serviceProvider))
            .Cast<IMenuCommand>()
            .Append(new BackCommand())
            .ToList();
        
        var items = commands
            .Select(c => 
                new MenuItem(c.Title, _ => c.ExecuteAsync(cancellationToken)))
            .ToList();

        return new MenuState("Выбор поликлиники", items);
    }
}