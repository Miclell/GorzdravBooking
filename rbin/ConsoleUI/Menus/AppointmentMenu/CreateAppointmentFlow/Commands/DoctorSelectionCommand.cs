using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.AppointmentMenu.CreateAppointmentFlow.Providers;
using ConsoleUI.Menus.Providers;
using ConsoleUI.Services;
using Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.AppointmentMenu.CreateAppointmentFlow.Commands;

public class DoctorSelectionCommand(Doctor doctor, IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title => doctor.Name;
    public async Task<MenuState?> ExecuteAsync()
    {
        var navigation = serviceProvider.GetRequiredService<NavigationService>();
        navigation.SetSharedData(nameof(Doctor), doctor);
        
        var createAppointmentProvider = serviceProvider.GetRequiredService<CreateAppointmentProvider>();
        
        return await createAppointmentProvider.CreateMenuAsync();
    }
}