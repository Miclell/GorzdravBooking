using CLI.Menus.PatientMenu.CreatePatientFlow.Providers;
using Core.Models;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.PatientMenu.CreatePatientFlow.Commands;

public class DistrictSelectionCommand(District district, IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title { get; } = district.Name;

    public async Task<MenuResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var dataService = serviceProvider.GetRequiredService<IDataService>();
        dataService.Set(nameof(District), district);

        var lpuSelectionProvider = serviceProvider.GetRequiredService<LpuSelectionProvider>();

        return MenuResult.Push(await lpuSelectionProvider.CreateMenuAsync(cancellationToken));
    }
}