using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.AppointmentMenu.CreateAppointmentFlow.Providers;
using ConsoleUI.Services;
using Core.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.AppointmentMenu.CreateAppointmentFlow.Commands;

public class SelectPatientCommand(PatientProfile patient, IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title { get; } = $"{patient.LpuShortName} | {patient.PatientFirstName} {patient.PatientLastName}";
    public async Task<MenuState?> ExecuteAsync()
    {
        var navigation = serviceProvider.GetRequiredService<NavigationService>();
        navigation.SetSharedData(nameof(PatientProfile), patient);
        
        var createAppointmentProvider = serviceProvider.GetRequiredService<SpecialitySelectionProvider>();
        return await createAppointmentProvider.CreateMenuAsync();
    }
}