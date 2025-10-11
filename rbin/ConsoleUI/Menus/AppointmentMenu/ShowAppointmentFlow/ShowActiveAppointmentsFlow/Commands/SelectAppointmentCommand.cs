using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.AppointmentMenu.ShowAppointmentFlow.ShowActiveAppointmentsFlow.Providers;
using ConsoleUI.Services;
using Core.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.AppointmentMenu.ShowAppointmentFlow.ShowActiveAppointmentsFlow.Commands;

public class SelectAppointmentCommand(AppointmentSearchRequest appointmentSearchRequest, IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title { get; } = $"{appointmentSearchRequest.PatientProfile.LpuShortName}"
        + $" {appointmentSearchRequest.PatientProfile.PatientFirstName} {appointmentSearchRequest.PatientProfile.PatientLastName} |"
        + $" {appointmentSearchRequest.DoctorName} | {appointmentSearchRequest.TimePreferencesPresetName}";
    
    public async Task<MenuState?> ExecuteAsync()
    {
        var navigation = serviceProvider.GetRequiredService<NavigationService>();
        navigation.SetSharedData(nameof(AppointmentSearchRequest), appointmentSearchRequest);
        
        var selectAppointmentProvider = serviceProvider.GetRequiredService<SelectAppointmentProvider>();
        return await selectAppointmentProvider.CreateMenuAsync();
    }
}