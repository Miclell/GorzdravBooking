using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Services;
using Core.Interfaces;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.Providers;

public class DoctorMenuProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync()
    {
        var navigation = serviceProvider.GetRequiredService<NavigationService>();
        var specialityId = navigation.GetSharedData<MedicalSpeciality>(nameof(MedicalSpeciality)).Id;
        var lpuId = navigation.GetSharedData<Lpu>(nameof(Lpu)).Id;
        
        var doctorService = serviceProvider.GetRequiredService<IExternalDoctorService>();
        var doctors = await doctorService.GetBySpecialtyAsync(lpuId, specialityId);
        
        var commands = doctors
            .Select(d => new Commands.DoctorSelectionCommand(d, serviceProvider))
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