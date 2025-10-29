using CLI.Menus.PatientMenu.CreatePatientFlow.Providers;
using Core.Models;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.PatientMenu.CreatePatientFlow.Commands;

public class LpuSelectionCommand(Lpu lpu, IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title { get; } = lpu.LpuShortName;
    public async Task<MenuResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var dataService = serviceProvider.GetRequiredService<IDataService>();
        dataService.Set(nameof(Lpu), lpu);
        
        var createPatientProvider = serviceProvider.GetRequiredService<CreatePatientProvider>();
        
        return MenuResult.Push(await createPatientProvider.CreateMenuAsync(cancellationToken));

    }
}