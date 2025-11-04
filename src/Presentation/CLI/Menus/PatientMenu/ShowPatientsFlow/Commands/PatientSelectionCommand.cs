using Application.DTOs.Patient;
using CLI.Menus.PatientMenu.ShowPatientsFlow.Providers;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.PatientMenu.ShowPatientsFlow.Commands;

public class PatientSelectionCommand(BasePatientProfileDto patient, IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title { get; } = $"{patient.LpuShortName} | {patient.PatientFirstName} {patient.PatientLastName}";

    public async Task<MenuResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var dataService = serviceProvider.GetRequiredService<IDataService>();
        dataService.Set(nameof(BasePatientProfileDto), patient);

        var patientSelectionProvider = serviceProvider.GetRequiredService<PatientSelectionProvider>();
        return MenuResult.Push(await patientSelectionProvider.CreateMenuAsync(cancellationToken));
    }
}