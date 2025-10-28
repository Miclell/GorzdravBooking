using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.AppointmentMenu.CreateAppointmentFlow.Commands;
using ConsoleUI.Services;
using Core.Entities;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.AppointmentMenu.CreateAppointmentFlow.Providers;

public class DoctorMenuProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync()
    {
        var navigation = serviceProvider.GetRequiredService<NavigationService>();
        var specialityId = navigation.GetSharedData<MedicalSpeciality>(nameof(MedicalSpeciality)).Id;
        var lpuId = navigation.GetSharedData<PatientProfile>(nameof(PatientProfile)).LpuId;
        
        var doctorService = serviceProvider.GetRequiredService<IExternalDoctorService>();
        var doctors = await doctorService.GetBySpecialtyAsync(int.Parse(lpuId), specialityId);
        
        var commands = doctors
            .Select(d => new DoctorSelectionCommand(d, serviceProvider))
            .Cast<IMenuCommand>()
            .Append(new BackCommand())
            .ToList();

        return new MenuState
        {
            Title = "Выберите специалиста",
            Commands = commands
        };
    }
}