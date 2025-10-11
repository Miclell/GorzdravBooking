

using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.AppointmentMenu.CreateAppointmentFlow.Providers;
using ConsoleUI.Services;
using Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.AppointmentMenu.CreateAppointmentFlow.Commands;

public class SpecialitySelectionCommand(MedicalSpeciality speciality, IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title => speciality.Name;
    
    public async Task<MenuState?> ExecuteAsync()
    {
        var navigation = serviceProvider.GetRequiredService<NavigationService>();
        navigation.SetSharedData(nameof(MedicalSpeciality), speciality);
        
        var doctorMenuProvider = serviceProvider.GetRequiredService<DoctorMenuProvider>();
        
        return await doctorMenuProvider.CreateMenuAsync();
    }
}