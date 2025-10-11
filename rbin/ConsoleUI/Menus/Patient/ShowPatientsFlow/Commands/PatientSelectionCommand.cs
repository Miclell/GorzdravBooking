using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.Patient.ShowPatientsFlow.Providers;
using ConsoleUI.Services;
using Core.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.Patient.ShowPatientsFlow.Commands;

public class PatientSelectionCommand(PatientProfile patient, IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title { get; } = $"{patient.LpuShortName} | {patient.PatientFirstName} {patient.PatientLastName}";
    public async Task<MenuState?> ExecuteAsync()
    {
        var navigation = serviceProvider.GetRequiredService<NavigationService>();
        navigation.SetSharedData(nameof(PatientProfile), patient);
        
        var patientSelectionProvider = serviceProvider.GetRequiredService<PatientSelectionProvider>();
        return await patientSelectionProvider.CreateMenuAsync();
    }
}