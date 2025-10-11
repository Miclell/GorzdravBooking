using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Services;
using Core.Interfaces;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.Providers;

public class AppointmentMenuProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync()
    {
        var navigation = serviceProvider.GetRequiredService<NavigationService>();
        var doctorId = navigation.GetSharedData<Doctor>(nameof(Doctor)).Id;
        var lpuId = navigation.GetSharedData<Lpu>(nameof(Lpu)).Id;
        
        var appointmentService = serviceProvider.GetRequiredService<IAppointmentService>();
        var appointments = await appointmentService.GetByDoctorAsync(lpuId, doctorId);
        
        var commands = appointments
            .Select(a => new Commands.AppointmentSelectionCommand(a, serviceProvider))
            .Cast<IMenuCommand>()
            .Append(new BackCommand())
            .ToList();

        return new MenuState
        {
            Title = "Выбор района",
            Commands = commands
        };
    }
}