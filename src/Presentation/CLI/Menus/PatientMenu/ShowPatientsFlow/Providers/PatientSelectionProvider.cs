using Application.DTOs.Patient;
using CLI.Menus.PatientMenu.ShowPatientsFlow.Commands;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.BuiltIn;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.PatientMenu.ShowPatientsFlow.Providers;

public class PatientSelectionProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public Task<MenuState> CreateMenuAsync(CancellationToken cancellationToken = default)
    {
        var commands = new IMenuCommand[]
        {
            serviceProvider.GetRequiredService<DeletePatientCommand>(),
            serviceProvider.GetRequiredService<BackCommand>()
        };
        
        var items = commands
            .Select(c => 
                new MenuItem(c.Title, _ => c.ExecuteAsync(cancellationToken)))
            .ToList();

        var dataService = serviceProvider.GetRequiredService<IDataService>();
        dataService.TryGet<BasePatientProfileDto>(nameof(BasePatientProfileDto), out var patient);
        return Task.FromResult(new MenuState($"Выберите действие для пациента " +
                                             $"{patient!.LpuShortName} | {patient.PatientFirstName} " +
                                             $"{patient.PatientLastName}", items));
    }
}