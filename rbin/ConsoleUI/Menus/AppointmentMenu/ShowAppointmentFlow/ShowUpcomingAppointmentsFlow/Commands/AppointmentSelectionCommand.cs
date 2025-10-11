using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.AppointmentMenu.ShowAppointmentFlow.ShowUpcomingAppointmentsFlow.Providers;
using ConsoleUI.Services;
using Core.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.AppointmentMenu.ShowAppointmentFlow.ShowUpcomingAppointmentsFlow.Commands;

public class AppointmentSelectionCommand(Appointment appointment, IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title { get; } = 
        $"{appointment.PatientProfile.LpuShortName} | {appointment.PatientProfile.PatientFirstName} {appointment.PatientProfile.PatientLastName}\n" + 
        $"{appointment.Speciality} {appointment.Doctor} {appointment.Room} {appointment.VisitStart}";
    
    public async Task<MenuState?> ExecuteAsync()
    {
        var navigation = serviceProvider.GetRequiredService<NavigationService>();
        navigation.SetSharedData(nameof(Appointment), appointment);
        
        var appointmentCancelProvider = serviceProvider.GetRequiredService<AppointmentCancelProvider>();

        return await appointmentCancelProvider.CreateMenuAsync();
    }
}