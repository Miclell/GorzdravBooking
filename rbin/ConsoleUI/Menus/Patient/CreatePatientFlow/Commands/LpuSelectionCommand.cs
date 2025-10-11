using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.Patient.CreatePatientFlow.Providers;
using ConsoleUI.Menus.Providers;
using ConsoleUI.Services;
using Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.Patient.CreatePatientFlow.Commands;

public class LpuSelectionCommand(Lpu lpu, IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title => lpu.LpuShortName;

    public async Task<MenuState?> ExecuteAsync()
    {
        var navigation = serviceProvider.GetRequiredService<NavigationService>();
        navigation.SetSharedData(nameof(Lpu), lpu);

        var createPatientProvider = serviceProvider.GetRequiredService<CreatePatientProvider>();
        
        return await createPatientProvider.CreateMenuAsync();
    }
}