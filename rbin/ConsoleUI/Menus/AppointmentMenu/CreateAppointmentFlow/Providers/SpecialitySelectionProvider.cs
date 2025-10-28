using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.AppointmentMenu.CreateAppointmentFlow.Commands;
using ConsoleUI.Services;
using Core.Entities;
using Core.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.AppointmentMenu.CreateAppointmentFlow.Providers;

public class SpecialitySelectionProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync()
    {
        var navigation = serviceProvider.GetRequiredService<NavigationService>();
        var patient = navigation.GetSharedData<PatientProfile>(nameof(PatientProfile));

        var specialityService = serviceProvider.GetRequiredService<IExternalSpecialtyService>();
        var specialities = await specialityService.GetByLpuAsync(int.Parse(patient.LpuId));

        var commands = specialities.Select(s => new SpecialitySelectionCommand(s, serviceProvider))
            .Cast<IMenuCommand>()
            .Append(new BackCommand())
            .ToList();

        return new MenuState()
        {
            Title = "Выберите специальность",
            Commands = commands
        };
    }
}